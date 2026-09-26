// SettingsWindow.xaml.cs
// Top 5: ReloadPreviewPhotos O(n), TickPreviewCycle O(1), ApplyAccent O(1)

using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using LGOledCompanion.Core;
using WinForms = System.Windows.Forms;

namespace LGOledCompanion;

public partial class SettingsWindow : Window
{
    private readonly ConfigStore _store;
    private readonly Action<AppConfig> _on_saved;
    private readonly Action? _on_test_tv;
    private readonly DispatcherTimer _preview_timer;
    private OverlayTint _overlay_tint;
    private IReadOnlyList<string> _preview_paths = [];
    private int _preview_index = -1;
    private DateTime _cycle_started = DateTime.Now;

    public SettingsWindow(AppConfig config, ConfigStore store, Action<AppConfig> on_saved, Action? on_test_tv = null)
    {
        InitializeComponent();
        _store = store;
        _on_saved = on_saved;
        _on_test_tv = on_test_tv;
        _overlay_tint = OverlayTint.Parse(config.Cor_Overlay);
        IdleMinutes.Text = config.Tempo_Inatividade.ToString();
        FadeOutSlider.Value = config.Tempo_FadeOut_Overlay;
        FadeInSlider.Value = config.Tempo_FadeIn_Overlay;
        HoldSlider.Value = config.Tempo_Transicao;
        OverlaySlider.Value = config.Opacidade_Overlay;
        ApplyOverlaySwatch();
        PhotoFolder.Text = config.Pasta_Fotos;
        NightStart.Text = config.Horario_Noite_Inicio;
        NightEnd.Text = config.Horario_Noite_Fim;
        WebOsDevice.Text = string.IsNullOrWhiteSpace(config.Device_WebOS) ? "Device1" : config.Device_WebOS;
        CliPath.Text = config.Caminho_LGTVcli;
        DebugMode.IsChecked = config.Modo_Debug;
        Autostart.IsChecked = AutostartService.IsEnabled();
        AccentCombo.SelectedIndex = AccentTheme.IndexOf(config.Cor_Destaque);
        ApplyAccent(AccentTheme.Normalize(config.Cor_Destaque));
        UpdateCycleLabels();
        ReloadPreviewPhotos();
        _preview_timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
        _preview_timer.Tick += (_, _) => TickPreviewCycle();
        _preview_timer.Start();
    }

    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            DragMove();
        }
    }

    private void Nav_Checked(object sender, RoutedEventArgs e)
    {
        if (GeralPanel is null || VisualPanel is null || TvPanel is null)
        {
            return;
        }

        GeralPanel.Visibility = NavGeral.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        VisualPanel.Visibility = NavVisual.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        TvPanel.Visibility = NavTv.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        ContentScroll.VerticalScrollBarVisibility = NavVisual.IsChecked == true
            ? ScrollBarVisibility.Disabled
            : ScrollBarVisibility.Auto;
    }

    private void AccentCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded)
        {
            return;
        }

        ApplyAccent(AccentTheme.HexAt(AccentCombo.SelectedIndex));
    }

    private void ApplyAccent(string hex)
    {
        var tint = OverlayTint.Parse(hex);
        var brush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(tint.R, tint.G, tint.B));
        Resources["AccentBrush"] = brush;
        BrandTitle.Foreground = brush;
        SaveButton.Background = brush;
    }

    private void OverlaySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!IsLoaded)
        {
            return;
        }

        TickPreviewCycle();
    }

    private void CycleSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!IsLoaded)
        {
            return;
        }

        UpdateCycleLabels();
        RestartPreviewCycle();
    }

    private void PhotoFolder_LostFocus(object sender, RoutedEventArgs e)
    {
        ReloadPreviewPhotos();
    }

    private void PickOverlayColor_Click(object sender, RoutedEventArgs e)
    {
        using var dialog = new WinForms.ColorDialog
        {
            FullOpen = true,
            Color = System.Drawing.Color.FromArgb(_overlay_tint.R, _overlay_tint.G, _overlay_tint.B)
        };
        if (dialog.ShowDialog() != WinForms.DialogResult.OK)
        {
            return;
        }

        _overlay_tint = new OverlayTint(dialog.Color.R, dialog.Color.G, dialog.Color.B);
        ApplyOverlaySwatch();
        TickPreviewCycle();
    }

    private void ApplyOverlaySwatch()
    {
        OverlayColorButton.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(_overlay_tint.R, _overlay_tint.G, _overlay_tint.B));
        OverlayColorButton.Content = _overlay_tint.ToHex();
        OverlayColorButton.Foreground = _overlay_tint.R + _overlay_tint.G + _overlay_tint.B < 360
            ? System.Windows.Media.Brushes.White
            : System.Windows.Media.Brushes.Black;
    }

    private void BrowseFolder_Click(object sender, RoutedEventArgs e)
    {
        using var dialog = new WinForms.FolderBrowserDialog();
        if (dialog.ShowDialog() == WinForms.DialogResult.OK)
        {
            PhotoFolder.Text = dialog.SelectedPath;
            ReloadPreviewPhotos();
        }
    }

    private void BrowseCli_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "LGTVcli|LGTVcli.exe|Executáveis|*.exe",
            FileName = "LGTVcli.exe"
        };
        if (dialog.ShowDialog() == true)
        {
            CliPath.Text = dialog.FileName;
        }
    }

    private void PreviewPrevious_Click(object sender, RoutedEventArgs e)
    {
        _preview_index = SettingsPreview.PreviousIndex(_preview_paths, _preview_index, PhotoCatalog.CanOpen);
        ShowPreviewPhoto();
        RestartPreviewCycle();
    }

    private void PreviewNext_Click(object sender, RoutedEventArgs e)
    {
        _preview_index = SettingsPreview.NextIndex(_preview_paths, _preview_index, PhotoCatalog.CanOpen);
        ShowPreviewPhoto();
        RestartPreviewCycle();
    }

    private void ReloadPreviewPhotos()
    {
        _preview_paths = PhotoCatalog.ListPaths(PhotoFolder.Text.Trim());
        _preview_index = PhotoPlayback.FindLoadable(_preview_paths, 0, PhotoCatalog.CanOpen);
        ShowPreviewPhoto();
        RestartPreviewCycle();
    }

    private void ShowPreviewPhoto()
    {
        if (_preview_index < 0 || _preview_index >= _preview_paths.Count)
        {
            PreviewPhoto.Source = null;
            PreviewPlaceholder.Visibility = Visibility.Visible;
            PreviewCaption.Text = "Pré-visualização ao vivo";
            return;
        }

        PreviewPhoto.Source = PhotoImageLoader.Load(_preview_paths[_preview_index]);
        PreviewPlaceholder.Visibility = Visibility.Collapsed;
        PreviewCaption.Text = Path.GetFileName(_preview_paths[_preview_index]);
    }

    private void TickPreviewCycle()
    {
        if (PreviewOverlay is null)
        {
            return;
        }

        var fade_out = TimeSpan.FromSeconds(FadeOutSlider.Value);
        var hold = TimeSpan.FromSeconds(HoldSlider.Value);
        var fade_in = TimeSpan.FromSeconds(FadeInSlider.Value);
        var configured = Math.Clamp(OverlaySlider.Value, 0, 90) / 100.0;
        var progress = OverlayCycle.At(DateTime.Now - _cycle_started, fade_out, hold, fade_in, configured);
        PreviewOverlay.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(_overlay_tint.R, _overlay_tint.G, _overlay_tint.B));
        PreviewOverlay.Opacity = progress.Opacity;

        if (!progress.Completed)
        {
            return;
        }

        _preview_index = SettingsPreview.NextIndex(_preview_paths, _preview_index, PhotoCatalog.CanOpen);
        ShowPreviewPhoto();
        RestartPreviewCycle();
    }

    private void RestartPreviewCycle()
    {
        _cycle_started = DateTime.Now;
        TickPreviewCycle();
    }

    private void UpdateCycleLabels()
    {
        if (FadeOutLabel is null)
        {
            return;
        }

        var fade_out = (int)FadeOutSlider.Value;
        var fade_in = (int)FadeInSlider.Value;
        var hold = (int)HoldSlider.Value;
        var total = OverlayCycle.Total(
            TimeSpan.FromSeconds(fade_out),
            TimeSpan.FromSeconds(hold),
            TimeSpan.FromSeconds(fade_in));
        FadeOutLabel.Text = $"Fade-out overlay ({fade_out}s)";
        FadeInLabel.Text = $"Fade-in overlay ({fade_in}s)";
        HoldLabel.Text = $"Transição imagem ({hold}s)";
        CycleTotalLabel.Text = $"Ciclo total: {total.TotalSeconds:0}s (100% → overlay → 0% → overlay → próxima)";
    }

    private void TestTv_Click(object sender, RoutedEventArgs e)
    {
        _on_test_tv?.Invoke();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        _preview_timer.Stop();
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(IdleMinutes.Text, out var idle) || idle < 1)
        {
            idle = 5;
        }

        var overlay = (int)OverlaySlider.Value;
        if (overlay < 0) overlay = 0;
        if (overlay > 90) overlay = 90;

        try
        {
            _ = NightWindow.Parse(NightStart.Text.Trim(), NightEnd.Text.Trim());
        }
        catch
        {
            System.Windows.MessageBox.Show("Horário noturno inválido. Use HH:mm.", "LGOledCompanion");
            return;
        }

        var device = WebOsDevice.Text?.Trim();
        var config = new AppConfig
        {
            Tempo_Inatividade = idle,
            Tempo_FadeOut_Overlay = (int)FadeOutSlider.Value,
            Tempo_FadeIn_Overlay = (int)FadeInSlider.Value,
            Tempo_Transicao = (int)HoldSlider.Value,
            Opacidade_Overlay = overlay,
            Cor_Overlay = _overlay_tint.ToHex(),
            Cor_Destaque = AccentTheme.HexAt(AccentCombo.SelectedIndex),
            Pasta_Fotos = PhotoFolder.Text.Trim(),
            Horario_Noite_Inicio = NightStart.Text.Trim(),
            Horario_Noite_Fim = NightEnd.Text.Trim(),
            Device_WebOS = string.IsNullOrWhiteSpace(device) ? "Device1" : device,
            Caminho_LGTVcli = CliPath.Text.Trim(),
            Modo_Debug = DebugMode.IsChecked == true
        };

        _store.Save(config);
        AutostartService.SetEnabled(Autostart.IsChecked == true);
        _on_saved(config);
        Close();
    }
}
