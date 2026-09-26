// SlideshowWindow.xaml.cs

using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using LGOledCompanion.Core;

namespace LGOledCompanion;

public partial class SlideshowWindow : Window
{
    private static readonly IntPtr HwndTopmost = new(-1);
    private const uint SwpShowWindow = 0x0040;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(
        IntPtr hwnd,
        IntPtr hwnd_insert_after,
        int x,
        int y,
        int cx,
        int cy,
        uint flags);

    public SlideshowWindow()
    {
        InitializeComponent();
    }

    public void CoverPixelBounds(int pixel_left, int pixel_top, int pixel_width, int pixel_height)
    {
        WindowState = WindowState.Normal;
        Show();
        var dpi = VisualTreeHelper.GetDpi(this);
        var dip = ScreenPlacement.ToDip(
            pixel_left,
            pixel_top,
            pixel_width,
            pixel_height,
            dpi.DpiScaleX,
            dpi.DpiScaleY);
        Left = dip.Left;
        Top = dip.Top;
        Width = dip.Width;
        Height = dip.Height;
        var hwnd = new WindowInteropHelper(this).Handle;
        SetWindowPos(hwnd, HwndTopmost, pixel_left, pixel_top, pixel_width, pixel_height, SwpShowWindow);
    }

    public void ShowPhoto(ImageSource? source, double overlay_opacity, OverlayTint tint)
    {
        Photo.Source = source;
        SetOverlay(overlay_opacity, tint);
    }

    public void SetOverlay(double overlay_opacity, OverlayTint tint)
    {
        Overlay.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(tint.R, tint.G, tint.B));
        Overlay.Opacity = overlay_opacity;
    }
}
