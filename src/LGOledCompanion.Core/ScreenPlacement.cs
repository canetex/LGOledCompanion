// ScreenPlacement.cs
// Top 5: ToDip O(1)

namespace LGOledCompanion.Core;

public readonly record struct DipRect(double Left, double Top, double Width, double Height);

public static class ScreenPlacement
{
    public static DipRect ToDip(
        int pixel_left,
        int pixel_top,
        int pixel_width,
        int pixel_height,
        double scale_x,
        double scale_y)
    {
        var safe_x = scale_x <= 0 ? 1 : scale_x;
        var safe_y = scale_y <= 0 ? 1 : scale_y;
        return new DipRect(
            pixel_left / safe_x,
            pixel_top / safe_y,
            pixel_width / safe_x,
            pixel_height / safe_y);
    }
}
