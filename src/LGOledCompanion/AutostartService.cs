// AutostartService.cs

using System.IO;

namespace LGOledCompanion;

internal static class AutostartService
{
    private static string ShortcutPath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "LGOledCompanion.lnk");

    public static bool IsEnabled() => File.Exists(ShortcutPath);

    public static void SetEnabled(bool enabled)
    {
        if (enabled)
        {
            Enable();
            return;
        }

        if (File.Exists(ShortcutPath))
        {
            File.Delete(ShortcutPath);
        }
    }

    private static void Enable()
    {
        var exe = Environment.ProcessPath ?? Path.Combine(AppContext.BaseDirectory, "LGOledCompanion.exe");
        var shell_type = Type.GetTypeFromProgID("WScript.Shell");
        if (shell_type is null)
        {
            return;
        }

        dynamic shell = Activator.CreateInstance(shell_type)!;
        dynamic shortcut = shell.CreateShortcut(ShortcutPath);
        shortcut.TargetPath = exe;
        shortcut.WorkingDirectory = Path.GetDirectoryName(exe);
        shortcut.Description = "LGOledCompanion";
        shortcut.Save();
    }
}
