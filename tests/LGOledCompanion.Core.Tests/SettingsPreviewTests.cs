using LGOledCompanion.Core;

namespace LGOledCompanion.Core.Tests;

public sealed class SettingsPreviewTests
{
    private static readonly byte[] OnePixelPng = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==");

    [Fact]
    public void Missing_folder_has_no_preview_photo()
    {
        Assert.Null(SettingsPreview.FirstPhotoPath(string.Empty));
        Assert.Null(SettingsPreview.FirstPhotoPath(@"C:\lgoc-no-such-folder"));
    }

    [Fact]
    public void First_openable_photo_is_used_for_preview()
    {
        var root = Path.Combine(Path.GetTempPath(), $"lgoc-preview-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            File.WriteAllText(Path.Combine(root, "a.jpg"), "not-an-image");
            File.WriteAllBytes(Path.Combine(root, "b.png"), OnePixelPng);

            var path = SettingsPreview.FirstPhotoPath(root);
            Assert.NotNull(path);
            Assert.EndsWith("b.png", path, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public void Next_index_wraps_to_the_other_openable_photo()
    {
        var paths = new[] { "a.jpg", "b.jpg", "c.jpg" };
        var next = SettingsPreview.NextIndex(paths, 1, path => path != "b.jpg");
        Assert.Equal(2, next);
    }

    [Fact]
    public void Previous_index_walks_backward()
    {
        var paths = new[] { "a.jpg", "b.jpg", "c.jpg" };
        var previous = SettingsPreview.PreviousIndex(paths, 0, path => path != "a.jpg");
        Assert.Equal(2, previous);
    }
}
