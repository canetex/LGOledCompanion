// OverlayTint.cs
// Top 5: Parse O(1), ToHex O(1)

using System.Globalization;

namespace LGOledCompanion.Core;

public readonly record struct OverlayTint(byte R, byte G, byte B)
{
    public static OverlayTint Black { get; } = new(0, 0, 0);

    public static OverlayTint Parse(string? hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
        {
            return Black;
        }

        var value = hex.Trim();
        if (value.StartsWith('#'))
        {
            value = value[1..];
        }

        if (value.Length == 3)
        {
            value = string.Concat(value[0], value[0], value[1], value[1], value[2], value[2]);
        }

        if (value.Length == 6
            && byte.TryParse(value[..2], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var r)
            && byte.TryParse(value[2..4], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var g)
            && byte.TryParse(value[4..6], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var b))
        {
            return new OverlayTint(r, g, b);
        }

        return Black;
    }

    public string ToHex() => $"#{R:X2}{G:X2}{B:X2}";
}
