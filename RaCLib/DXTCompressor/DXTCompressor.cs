using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaCLib.DXTCompressor
{
    public struct RGBAColor
    {
        public byte R;
        public byte G;
        public byte B;
        public byte A;

        public RGBAColor(byte R, byte G, byte B, byte A)
        {
            this.R = R;
            this.G = G;
            this.B = B;
            this.A = A;
        }

        public int Value => BitConverter.ToInt32(new byte[] {this.R, this.G, this.B, this.A});
    }
    public static class DXTCompressor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pixelData"> RGBA Formatted Pixel Data</param>
        /// <param name="width">Image Data Width</param>
        /// <param name="height"> Image Data Height</param>
        /// <returns></returns>
        public static byte[] CompressDXT1(byte[] pixelData, int width, int height)
        {
            // DXT1 size would be width * height / 2
            int outWidth = width % 0x4 != 0 ? width + (0x4 - (width % 0x4)) : width;
            int outHeight = height % 0x4 != 0 ? height + (0x4 - (height % 0x4)) : height;
            int outSize = (width * height) / 2;
            byte[] result = new byte[outSize];

            int currentResultPos = 0;

            for (int y = 0; y < outHeight; y+= 4)
            {
                for (int x = 0; x < outWidth; x += 4)
                {
                    //int index = (outWidth * y) + x * 4;

                    RGBAColor[] palette = new RGBAColor[4];
                    RGBAColor[] blockPixels = new RGBAColor[16];

                    for (int blockY = 0; blockY < 4; blockY++)
                    {
                        for (int blockX = 0; blockX < 4; blockX++)
                        {
                            int index = (outWidth * (y + blockY)) + (x + blockX * 4);
                            blockPixels[(4 * blockY) + blockX] = new RGBAColor(pixelData[index], pixelData[index+1], pixelData[index + 2], pixelData[index + 3]);
                        }
                    }

                    palette[0] = blockPixels.MinBy(x => x.Value);
                    palette[1] = blockPixels.MaxBy(x => x.Value);
                    palette[2].R = (byte)(palette[0].R * (2.0f / 3.0f) + palette[1].R * (1.0f / 3.0f));
                    palette[2].G = (byte)(palette[0].G * (2.0f / 3.0f) + palette[1].G * (1.0f / 3.0f));
                    palette[2].B = (byte)(palette[0].B * (2.0f / 3.0f) + palette[1].B * (1.0f / 3.0f));
                    palette[2].A = (byte)(palette[0].A * (2.0f / 3.0f) + palette[1].A * (1.0f / 3.0f));
                    palette[3].R = (byte)(palette[0].R * (1.0f / 3.0f) + palette[1].R * (2.0f / 3.0f));
                    palette[3].G = (byte)(palette[0].G * (1.0f / 3.0f) + palette[1].G * (2.0f / 3.0f));
                    palette[3].B = (byte)(palette[0].B * (1.0f / 3.0f) + palette[1].B * (2.0f / 3.0f));
                    palette[3].A = (byte)(palette[0].A * (1.0f / 3.0f) + palette[1].A * (2.0f / 3.0f));

                    int pixelBits = 0b00_00_00_00_00_00_00_00_00_00_00_00_00_00_00_00;

                    for (int blockY = 0; blockY < 4; blockY++)
                    {
                        for (int blockX = 0; blockX < 4; blockX++)
                        {
                            int index = (outWidth * (y + blockY)) + (x + blockX * 4);
                            RGBAColor matchColor = new RGBAColor(pixelData[index], pixelData[index + 1], pixelData[index + 2], pixelData[index + 3]);

                            int[] differences = new int[4];
                            for (int i = 0; i < 4; i++)
                            {
                                differences[i] = palette[i].Value - matchColor.Value;
                            }
                            Array.IndexOf(differences, differences.Min());
                            pixelBits |= (byte)(Array.IndexOf(differences, differences.Min()) << ((4 * blockY) + blockX * 2));
                        }
                    }

                    ushort minColor = 0;
                    minColor |= (ushort)(((palette[0].R * 31 + 127) / 255) << 11);
                    minColor |= (ushort)(((palette[0].G * 63 + 127) / 255) << 5);
                    minColor |= (ushort)((palette[0].B * 31 + 127) / 255);
                                 
                    ushort maxColor = 0;
                    maxColor |= (ushort)(((palette[1].R * 31 + 127) / 255) << 11);
                    maxColor |= (ushort)(((palette[1].G * 63 + 127) / 255) << 5);
                    maxColor |= (ushort)((palette[1].B * 31 + 127) / 255);

                    if (minColor == maxColor)
                        maxColor += 1;

                    Buffer.BlockCopy(BitConverter.GetBytes(minColor), 0, result, currentResultPos, 2);
                    Buffer.BlockCopy(BitConverter.GetBytes(maxColor), 0, result, currentResultPos + 2, 2);
                    Buffer.BlockCopy(BitConverter.GetBytes(pixelBits), 0, result, currentResultPos + 4, 4);
                    currentResultPos += 8;
                }
            }

            return result;
        }
    }
}
