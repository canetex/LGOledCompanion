// SettingsWindow.xaml.cs

using System.Windows;
using LGOledCompanion.Core;
using WinForms = System.Windows.Forms;

namespace LGOledCompanion;

public partial class SettingsWindow : Window
{
    private readonly ConfigStore _store;
    private readonly Action<AppConfig> _on_saved;

    public SettingsWindow(AppConfig config, ConfigStore store, Action<AppConfig> on_saved)
    {
        InitializeComponent();
        _store = store;
        _on_saved = on_saved;
        IdleMinutes.Text = config.Tempo_Inatividade.ToString();
        TransitionSeconds.Text = config.Tempo_Transicao.ToString();
        OverlaySlider.Value = config.Opacidade_Overlay;
        PhotoFolder.Text = config.Pasta_Fotos;
        NightStart.Text = config.Horario_Noite_Inicio;
        NightEnd.Text = config.Horario_Noite_Fim;
        WebOsDevice.Text = config.Device_WebOS;
        CliPath.Text = config.Caminho_LGTVcli;
        DebugMode.IsChecked = config.Modo_Debug;
        Autostart.IsChecked = AutostartService.IsEnabled();
    }

    private void BrowseFolder_Click(object sender, RoutedEventArgs e)
    {
        using var dialog = new WinForms.FolderBrowserDialog();
        if (dialog.ShowDialog() == WinForms.DialogResult.OK)
        {
            PhotoFolder.Text = dialog.SelectedPath;
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

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(IdleMinutes.Text, out var idle) || idle < 1)
        {
            idle = 5;
        }

        if (!int.TryParse(TransitionSeconds.Text, out var transition) || transition < 1)
        {
            transition = 10;
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

        var config = new AppConfig
        {
            Tempo_Inatividade = idle,
            Tempo_Transicao = transition,
            Opacidade_Overlay = overlay,
            Pasta_Fotos = PhotoFolder.Text.Trim(),
            Horario_Noite_Inicio = NightStart.Text.Trim(),
            Horario_Noite_Fim = NightEnd.Text.Trim(),
            Device_WebOS = string.IsNullOrWhiteSpace(WebOsDevice.Text) ? "Device1" : WebOsDevice.Text.Trim(),
            Caminho_LGTVcli = CliPath.Text.Trim(),
            Modo_Debug = DebugMode.IsChecked == true
        };

        _store.Save(config);
        AutostartService.SetEnabled(Autostart.IsChecked == true);
        _on_saved(config);
        Close();
    }
}
