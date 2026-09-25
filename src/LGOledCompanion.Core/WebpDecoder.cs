// WebpDecoder.cs

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace LGOledCompanion.Core;

public static class WebpDecoder
{
    public static byte[] ToPngBytes(string path)
    {
        using var image = Image.Load<Rgba32>(path);
        image.Mutate(context => context.AutoOrient());
        using var stream = new MemoryStream();
        image.SaveAsPng(stream);
        return stream.ToArray();
    }
}
