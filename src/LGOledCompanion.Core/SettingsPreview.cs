// SettingsPreview.cs
// Top 5: FirstPhotoPath O(n), NextIndex O(n), PreviousIndex O(n)

namespace LGOledCompanion.Core;

public static class SettingsPreview
{
    public static string? FirstPhotoPath(string folder)
    {
        var paths = PhotoCatalog.ListPaths(folder);
        var index = PhotoPlayback.FindLoadable(paths, 0, PhotoCatalog.CanOpen);
        return index < 0 ? null : paths[index];
    }

    public static int NextIndex(IReadOnlyList<string> paths, int current, Func<string, bool> can_load)
    {
        if (paths.Count == 0)
        {
            return -1;
        }

        return PhotoPlayback.FindLoadable(paths, current + 1, can_load);
    }

    // O(n) n = fotos
    public static int PreviousIndex(IReadOnlyList<string> paths, int current, Func<string, bool> can_load)
    {
        if (paths.Count == 0)
        {
            return -1;
        }

        for (var offset = 1; offset <= paths.Count; offset++)
        {
            var index = ((current - offset) % paths.Count + paths.Count) % paths.Count;
            if (can_load(paths[index]))
            {
                return index;
            }
        }

        return -1;
    }
}
