using LGOledCompanion.Core;

namespace LGOledCompanion.Core.Tests;

public sealed class PhotoPlaybackTests
{
    [Fact]
    public void Empty_list_has_no_current_photo()
    {
        Assert.Equal(-1, PhotoPlayback.FindLoadable([], 0, _ => true));
    }

    [Fact]
    public void Skips_unloadable_and_returns_next()
    {
        var paths = new[] { "bad.jpg", "good.jpg", "also.jpg" };
        var index = PhotoPlayback.FindLoadable(paths, 0, path => path == "good.jpg");
        Assert.Equal(1, index);
    }

    [Fact]
    public void Wraps_around_the_list()
    {
        var paths = new[] { "a.jpg", "b.jpg", "c.jpg" };
        var index = PhotoPlayback.FindLoadable(paths, 2, path => path == "a.jpg");
        Assert.Equal(0, index);
    }

    [Fact]
    public void All_unloadable_returns_none()
    {
        Assert.Equal(-1, PhotoPlayback.FindLoadable(["a.jpg", "b.jpg"], 0, _ => false));
    }
}
