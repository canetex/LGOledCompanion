// SessionHost.cs
// Top 5: Tick O(1), StartSlideshow O(n+s), AdvanceIfNeeded O(1), Apply O(1), ShowCurrent O(s)
// n = fotos, s = telas

using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using LGOledCompanion.Core;
using Microsoft.Win32;
using DrawingBrushes = System.Drawing.Brushes;

namespace LGOledCompanion;

internal sealed class SessionHost : IDisposable
{
    private readonly DispatcherTimer _timer;
    private readonly ConfigStore _store;
    private readonly List<SlideshowWindow> _windows = [];
    private readonly NotifyIcon _tray;
    private readonly ToolStripMenuItem _pause_item;
    private readonly ToolStripMenuItem _test_item;

    private AppConfig _config;
    private FileLogger _logger;
    private SessionState _state = SessionState.Desktop;
    private IReadOnlyList<string> _photos = [];
    private int _photo_index;
    private DateTime _last_advance = DateTime.MinValue;
    private bool _paused;
    private bool _hold_cli_until_activity;
    private bool _testing_tv;
    private SettingsWindow? _settings;

    public SessionHost()
    {
        var config_path = Path.Combine(AppContext.BaseDirectory, "config.json");
        _store = new ConfigStore(config_path);
        _config = _store.Load();
        _logger = CreateLogger();

        _pause_item = new ToolStripMenuItem("Pausar", null, (_, _) => TogglePause());
        _test_item = new ToolStripMenuItem("Testar TV", null, async (_, _) => await TestTvAsync());
        var menu = new ContextMenuStrip();
        menu.Items.Add(new ToolStripMenuItem("Settings", null, (_, _) => OpenSettings()));
        menu.Items.Add(_pause_item);
        menu.Items.Add(_test_item);
        menu.Items.Add(new ToolStripMenuItem("Sair", null, (_, _) => System.Windows.Application.Current.Shutdown()));

        _tray = new NotifyIcon
        {
            Text = "LGOledCompanion",
            Visible = true,
            Icon = CreateTrayIcon(),
            ContextMenuStrip = menu
        };
        _tray.DoubleClick += (_, _) => OpenSettings();

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
        _timer.Tick += (_, _) => Tick();
        _timer.Start();

        SystemEvents.PowerModeChanged += OnPowerModeChanged;
        _logger.Info("started");
    }

    public void OpenSettings()
    {
        if (_settings is not null)
        {
            _settings.Activate();
            return;
        }

        _settings = new SettingsWindow(_config, _store, OnConfigSaved);
        _settings.Closed += (_, _) => _settings = null;
        _settings.Show();
        _settings.Activate();
    }

    public void Dispose()
    {
        SystemEvents.PowerModeChanged -= OnPowerModeChanged;
        _timer.Stop();
        CloseSlideshow();
        _tray.Visible = false;
        _tray.Dispose();
    }

    private void OnConfigSaved(AppConfig config)
    {
        _config = config;
        _logger = CreateLogger();
        _logger.Info("config saved");
    }

    private FileLogger CreateLogger()
    {
        return new FileLogger(Path.Combine(AppContext.BaseDirectory, "log.txt"), _config.Modo_Debug);
    }

    private void TogglePause()
    {
        _paused = !_paused;
        _pause_item.Text = _paused ? "Retomar" : "Pausar";
        _test_item.Enabled = !_paused;
        _logger.Info(_paused ? "paused" : "resumed");
    }

    private async Task TestTvAsync()
    {
        if (_paused || _testing_tv)
        {
            return;
        }

        _testing_tv = true;
        _test_item.Enabled = false;
        try
        {
            var client = CreateClient();
            if (!client.ScreenOff())
            {
                Balloon("Falha ao enviar -screenoff.");
                return;
            }

            await Task.Delay(TimeSpan.FromSeconds(3));
            if (!client.ScreenOn())
            {
                Balloon("Falha ao enviar -screenon.");
            }
        }
        finally
        {
            _testing_tv = false;
            _test_item.Enabled = !_paused;
        }
    }

    private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
    {
        if (e.Mode != PowerModes.Resume)
        {
            return;
        }

        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            CloseSlideshow();
            _state = SessionState.Desktop;
            if (!CreateClient().ScreenOn())
            {
                Balloon("Falha ao ligar a TV no resume.");
            }
        });
    }

    private void Tick()
    {
        NightWindow night_window;
        try
        {
            night_window = NightWindow.Parse(_config.Horario_Noite_Inicio, _config.Horario_Noite_Fim);
        }
        catch
        {
            night_window = NightWindow.Parse("23:00", "07:00");
        }

        var idle = NativeIdle.GetIdleTime();
        var threshold = TimeSpan.FromMinutes(Math.Max(1, _config.Tempo_Inatividade));
        if (_hold_cli_until_activity && idle < threshold)
        {
            _hold_cli_until_activity = false;
        }

        var action = SessionPolicy.Decide(new SessionSnapshot
        {
            State = _state,
            IsPaused = _paused,
            SettingsOpen = _settings is not null,
            DisplayRequired = NativeIdle.IsDisplayRequired(),
            IdleFor = idle,
            IdleThreshold = threshold,
            IsNight = night_window.IsActive(TimeOnly.FromDateTime(DateTime.Now))
        });

        if (_hold_cli_until_activity)
        {
            action = action switch
            {
                SessionAction.ScreenOff => SessionAction.None,
                SessionAction.SwitchToNight => SessionAction.StopSlideshow,
                _ => action
            };
        }

        Apply(action);

        if (_state == SessionState.Slideshow)
        {
            AdvanceIfNeeded();
        }
    }

    private void Apply(SessionAction action)
    {
        switch (action)
        {
            case SessionAction.StartSlideshow:
                StartSlideshow();
                break;
            case SessionAction.StopSlideshow:
                CloseSlideshow();
                _state = SessionState.Desktop;
                break;
            case SessionAction.ScreenOff:
                TryScreenOff();
                break;
            case SessionAction.ScreenOn:
                CloseSlideshow();
                TryScreenOn();
                _state = SessionState.Desktop;
                break;
            case SessionAction.SwitchToNight:
                CloseSlideshow();
                TryScreenOff();
                break;
            case SessionAction.SwitchToDay:
                TryScreenOn();
                StartSlideshow();
                break;
        }
    }

    private void StartSlideshow()
    {
        CloseSlideshow();
        var readable = new List<string>();
        // O(n) n = arquivos listados
        foreach (var path in PhotoCatalog.ListPaths(_config.Pasta_Fotos))
        {
            if (PhotoCatalog.CanOpen(path))
            {
                readable.Add(path);
            }
        }

        _photos = PhotoCatalog.Shuffle(readable, Random.Shared);
        _photo_index = 0;
        _last_advance = DateTime.Now;

        // O(s) s = telas
        foreach (var screen in Screen.AllScreens)
        {
            var window = new SlideshowWindow();
            window.Left = screen.Bounds.Left + 8;
            window.Top = screen.Bounds.Top + 8;
            window.Width = 200;
            window.Height = 200;
            window.Show();
            window.WindowState = WindowState.Maximized;
            _windows.Add(window);
        }

        ShowCurrent();
        _state = SessionState.Slideshow;
        _logger.Info($"slideshow start photos={_photos.Count} screens={_windows.Count}");
    }

    private void AdvanceIfNeeded()
    {
        if (_photos.Count == 0)
        {
            return;
        }

        var dwell = TimeSpan.FromSeconds(Math.Max(1, _config.Tempo_Transicao));
        if (DateTime.Now - _last_advance < dwell)
        {
            return;
        }

        _photo_index++;
        if (_photo_index >= _photos.Count)
        {
            _photos = PhotoCatalog.Shuffle(_photos, Random.Shared);
            _photo_index = 0;
        }

        _last_advance = DateTime.Now;
        ShowCurrent();
    }

    private void ShowCurrent()
    {
        var source = _photos.Count == 0 ? null : PhotoImageLoader.Load(_photos[_photo_index]);
        if (source is null && _photos.Count > 0)
        {
            return;
        }

        var overlay = Math.Clamp(_config.Opacidade_Overlay, 0, 90) / 100.0;
        // O(s) s = telas
        foreach (var window in _windows)
        {
            window.ShowPhoto(source, overlay);
        }
    }

    private void CloseSlideshow()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();
        _photos = [];
    }

    private void TryScreenOff()
    {
        if (CreateClient().ScreenOff())
        {
            _state = SessionState.NightScreenOff;
            _logger.Info("screenoff ok");
            return;
        }

        _hold_cli_until_activity = true;
        _state = SessionState.Desktop;
        Balloon("Falha ao enviar -screenoff. Nova tentativa no próximo idle.");
        _logger.Info("screenoff failed");
    }

    private void TryScreenOn()
    {
        if (CreateClient().ScreenOn())
        {
            _logger.Info("screenon ok");
            return;
        }

        Balloon("Falha ao enviar -screenon.");
        _logger.Info("screenon failed");
    }

    private LgtvClient CreateClient()
    {
        return new LgtvClient(_config.Caminho_LGTVcli, _config.Device_WebOS, new ProcessRunner(), new ThreadDelay());
    }

    private void Balloon(string message)
    {
        _tray.BalloonTipTitle = "LGOledCompanion";
        _tray.BalloonTipText = message;
        _tray.ShowBalloonTip(4000);
    }

    private static Icon CreateTrayIcon()
    {
        var bitmap = new Bitmap(16, 16);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.FromArgb(20, 20, 20));
        graphics.FillEllipse(DrawingBrushes.LimeGreen, 2, 2, 12, 12);
        return Icon.FromHandle(bitmap.GetHicon());
    }
}
