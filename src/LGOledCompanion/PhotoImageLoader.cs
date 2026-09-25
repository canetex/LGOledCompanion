// PhotoImageLoader.cs
// Top 5: Load O(bytes), LoadWebp O(bytes), ApplyExif O(1)

using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using LGOledCompanion.Core;

namespace LGOledCompanion;

internal static class PhotoImageLoader
{
    public static ImageSource? Load(string path)
    {
        try
        {
            if (string.Equals(Path.GetExtension(path), ".webp", StringComparison.OrdinalIgnoreCase))
            {
                return LoadWebp(path);
            }

            return LoadWpf(path);
        }
        catch
        {
            return null;
        }
    }

    private static ImageSource LoadWpf(string path)
    {
        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.UriSource = new Uri(path);
        bitmap.EndInit();
        bitmap.Freeze();
        return ApplyExif(bitmap);
    }

    private static ImageSource LoadWebp(string path)
    {
        using var stream = new MemoryStream(WebpDecoder.ToPngBytes(path));

        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.StreamSource = stream;
        bitmap.EndInit();
        bitmap.Freeze();
        return bitmap;
    }

    private static ImageSource ApplyExif(BitmapImage bitmap)
    {
        if (bitmap.Metadata is not BitmapMetadata metadata)
        {
            return bitmap;
        }

        var query = metadata.GetQuery("/app1/ifd/{ushort=274}");
        if (query is not ushort orientation)
        {
            return bitmap;
        }

        Transform? transform = orientation switch
        {
            3 => new RotateTransform(180),
            6 => new RotateTransform(90),
            8 => new RotateTransform(270),
            _ => null
        };

        if (transform is null)
        {
            return bitmap;
        }

        var rotated = new TransformedBitmap(bitmap, transform);
        rotated.Freeze();
        return rotated;
    }
}
