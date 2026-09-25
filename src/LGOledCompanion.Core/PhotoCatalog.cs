// PhotoCatalog.cs
// Top 5: Walk O(n), Shuffle O(n), ListPaths O(n), CanOpen O(1), IsPhoto O(1)

using SixLabors.ImageSharp;

namespace LGOledCompanion.Core;

public static class PhotoCatalog
{
    private static readonly HashSet<string> Extensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".bmp"
    };

    public static IReadOnlyList<string> ListPaths(string folder)
    {
        if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
        {
            return [];
        }

        var found = new List<string>();
        Walk(folder, found);
        found.Sort(StringComparer.OrdinalIgnoreCase);
        return found;
    }

    public static IReadOnlyList<string> Shuffle(IReadOnlyList<string> paths, Random rng)
    {
        var copy = paths.ToArray();
        // O(n) Fisher-Yates
        for (var i = copy.Length - 1; i > 0; i--)
        {
            var j = rng.Next(i + 1);
            (copy[i], copy[j]) = (copy[j], copy[i]);
        }

        return copy;
    }

    public static bool CanOpen(string path)
    {
        try
        {
            _ = Image.Identify(path);
            return true;
        }
        catch
        {
            return false;
        }
    }

    // O(n) n = arquivos e pastas visitados
    private static void Walk(string directory, List<string> acc)
    {
        try
        {
            foreach (var file in Directory.EnumerateFiles(directory))
            {
                if (IsPhoto(file))
                {
                    acc.Add(file);
                }
            }

            foreach (var child in Directory.EnumerateDirectories(directory))
            {
                Walk(child, acc);
            }
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or DirectoryNotFoundException or IOException)
        {
        }
    }

    private static bool IsPhoto(string path)
    {
        return Extensions.Contains(Path.GetExtension(path));
    }
}
