// SlideshowWindow.xaml.cs

using System.Windows;
using System.Windows.Media;

namespace LGOledCompanion;

public partial class SlideshowWindow : Window
{
    public SlideshowWindow()
    {
        InitializeComponent();
    }

    public void ShowPhoto(ImageSource? source, double overlay_opacity)
    {
        Photo.Source = source;
        Overlay.Opacity = source is null ? 0 : overlay_opacity;
    }
}
