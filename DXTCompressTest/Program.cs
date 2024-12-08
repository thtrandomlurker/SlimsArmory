// See https://aka.ms/new-console-template for more information

// am i alone in absolutely hating this new format for new dotnet projects


using RaCLib.DXTCompressor;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

Image<Rgba32> im = Image.Load<Rgba32>(args[0]);

byte[] pixelData = new byte[im.Width * im.Height * 4];

im.ProcessPixelRows(accessor =>
{
    for (int y = 0; y < im.Height; y++)
    {
        Span<Rgba32> pixelRow = accessor.GetRowSpan(y);
        for (int x = 0; x < im.Width; x++)
        {
            // Get a reference to the pixel at position x
            ref Rgba32 pixel = ref pixelRow[x];
            pixelData[(y * im.Height) + (x * 4)] = pixel.R;
            pixelData[(y * im.Height) + (x * 4) + 1] = pixel.G;
            pixelData[(y * im.Height) + (x * 4) + 2] = pixel.B;
            pixelData[(y * im.Height) + (x * 4) + 3] = pixel.A;
        }
    }
});

byte[] dxtCompressed = DXTCompressor.CompressDXT1(pixelData, im.Width, im.Height);

using (BinaryWriter writer = new BinaryWriter(File.Create("test.dxt")))
{
    writer.Write(dxtCompressed);
}