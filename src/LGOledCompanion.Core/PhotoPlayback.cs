// PhotoPlayback.cs
// Top 5: FindLoadable O(n)

namespace LGOledCompanion.Core;

public static class PhotoPlayback
{
    // O(n) n = fotos
    public static int FindLoadable(IReadOnlyList<string> paths, int start, Func<string, bool> can_load)
    {
        if (paths.Count == 0)
        {
            return -1;
        }

        var origin = ((start % paths.Count) + paths.Count) % paths.Count;
        for (var offset = 0; offset < paths.Count; offset++)
        {
            var index = (origin + offset) % paths.Count;
            if (can_load(paths[index]))
            {
                return index;
            }
        }

        return -1;
    }
}
