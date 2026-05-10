using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using ImageSharpImage = SixLabors.ImageSharp.Image;

namespace Viewer.App.Pak;

public static class ImageResourceDecoder
{
    public static Image LoadBitmap(byte[] data)
    {
        try
        {
            return LoadWithSystemDrawing(data);
        }
        catch
        {
            return LoadWithImageSharp(data);
        }
    }

    public static bool IsImageExtension(string extension)
    {
        return SupportedExtensions.Contains(extension);
    }

    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png",
        ".bmp",
        ".jpg",
        ".jpeg",
        ".gif",
        ".tga",
        ".targa",
        ".tif",
        ".tiff",
        ".webp"
    };

    private static Image LoadWithSystemDrawing(byte[] data)
    {
        using var stream = new MemoryStream(data);
        using var image = Image.FromStream(stream);
        return new Bitmap(image);
    }

    private static Image LoadWithImageSharp(byte[] data)
    {
        using var input = new MemoryStream(data);
        using var image = ImageSharpImage.Load<Rgba32>(input);
        using var output = new MemoryStream();
        image.Save(output, new PngEncoder());
        output.Position = 0;
        return new Bitmap(output);
    }
}
