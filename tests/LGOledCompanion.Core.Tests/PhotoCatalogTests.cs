using LGOledCompanion.Core;

namespace LGOledCompanion.Core.Tests;

public sealed class PhotoCatalogTests
{
    [Fact]
    public void Missing_folder_returns_empty()
    {
        Assert.Empty(PhotoCatalog.ListPaths(@"C:\this-folder-does-not-exist-lgoc"));
        Assert.Empty(PhotoCatalog.ListPaths(string.Empty));
    }

    [Fact]
    public void Lists_recursive_supported_extensions_only()
    {
        var root = Path.Combine(Path.GetTempPath(), $"lgoc-photos-{Guid.NewGuid():N}");
        var nested = Path.Combine(root, "album");
        Directory.CreateDirectory(nested);
        try
        {
            File.WriteAllText(Path.Combine(root, "keep.jpg"), "x");
            File.WriteAllText(Path.Combine(nested, "keep.png"), "x");
            File.WriteAllText(Path.Combine(root, "skip.txt"), "x");
            File.WriteAllText(Path.Combine(nested, "skip.gif"), "x");

            var listed = PhotoCatalog.ListPaths(root);
            Assert.Equal(2, listed.Count);
            Assert.Contains(listed, path => path.EndsWith("keep.jpg", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(listed, path => path.EndsWith("keep.png", StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public void Shuffle_with_same_seed_is_stable_and_changes_order()
    {
        var paths = new[] { "a.jpg", "b.jpg", "c.jpg", "d.jpg", "e.jpg" };
        var first = PhotoCatalog.Shuffle(paths, new Random(7));
        var second = PhotoCatalog.Shuffle(paths, new Random(7));
        Assert.Equal(first, second);
        Assert.NotEqual(paths, first);
    }

    [Fact]
    public void CanOpen_rejects_unreadable_file()
    {
        var path = Path.Combine(Path.GetTempPath(), $"lgoc-{Guid.NewGuid():N}.jpg");
        File.WriteAllText(path, "not-an-image");
        try
        {
            Assert.False(PhotoCatalog.CanOpen(path));
        }
        finally
        {
            File.Delete(path);
        }
    }
}
