// AccentTheme.cs
// Top 5: Normalize O(n), IndexOf O(n), HexAt O(1)

namespace LGOledCompanion.Core;

public static class AccentTheme
{
    public const string Magenta = "#FF3B7C";
    public const string Cyan = "#22D3EE";
    public const string Lime = "#84CC16";
    public const string Orange = "#F97316";

    public static readonly IReadOnlyList<string> Presets =
    [
        Magenta,
        Cyan,
        Lime,
        Orange
    ];

    public static string Normalize(string? hex)
    {
        var value = OverlayTint.Parse(hex).ToHex();
        return Presets.Contains(value, StringComparer.OrdinalIgnoreCase) ? value : Magenta;
    }

    public static int IndexOf(string? hex)
    {
        var value = OverlayTint.Parse(hex).ToHex();
        for (var i = 0; i < Presets.Count; i++)
        {
            if (string.Equals(Presets[i], value, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return 0;
    }

    public static string HexAt(int index)
    {
        if (index < 0 || index >= Presets.Count)
        {
            return Magenta;
        }

        return Presets[index];
    }
}
