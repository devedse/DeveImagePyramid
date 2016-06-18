using BitMiracle.LibTiff.Classic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Devedse.DeveImagePyramid
{
    public static class ImageReader
    {
        public static PretzelImage ReadImage(string path)
        {
            var extension = Path.GetExtension(path);

            if (extension == ".tiff" || extension == ".tif")
            {
                return ReadImageTiff(path);
            }
            else if (extension == ".png")
            {
                return ReadImagePng(path);
            }
            throw new NotSupportedException($"We don't support images with the extension: {extension}");
        }

        private static PretzelImage ReadImageTiff(string path)
        {
            using (var tiff = Tiff.Open(path, "r"))
            {
                if (tiff == null)
                {
                    throw new ArgumentException($"Could not load tiff file from path '{path}'. Does the file exist?");
                }

                var imageWidth = (int)((FieldValue)tiff.GetField(TiffTag.IMAGEWIDTH).GetValue(0)).Value;
                var imageHeight = (int)((FieldValue)tiff.GetField(TiffTag.IMAGELENGTH).GetValue(0)).Value;
                var bitsPerSample = (short)((FieldValue)tiff.GetField(TiffTag.BITSPERSAMPLE).GetValue(0)).Value;
                var samplesPerPixel = (short)((FieldValue)tiff.GetField(TiffTag.SAMPLESPERPIXEL).GetValue(0)).Value;

                if (bitsPerSample != 8)
                {
                    throw new ArgumentException("We currently only support TIFF images with 8 bits per sample");
                }
                if (samplesPerPixel != 3)
                {
                    throw new ArgumentException("We currently only support TIFF iamges with 3 samples per pixel");
                }

                byte[] data = new byte[3 * imageWidth * imageHeight];

                if (tiff.IsTiled())
                {
                    tiff.ReadEncodedTile(0, data, 0, data.Length);
                }
                else
                {
                    for (int y = 0; y < imageHeight; y++)
                    {
                        tiff.ReadScanline(data, 3 * y * imageWidth, y, 0);
                    }
                }

                return new PretzelImage(data, imageWidth, imageHeight);
            }
        }

        private static PretzelImage ReadImagePng(string path)
        {
            using (var image = new Bitmap(path))
            {
                var imageWidth = image.Width;
                var imageHeight = image.Height;

                byte[] data = new byte[3 * imageWidth * imageHeight];

                var lockedBits = image.LockBits(new Rectangle(Point.Empty, image.Size), ImageLockMode.ReadOnly, image.PixelFormat);

                var pixelSize = lockedBits.PixelFormat == PixelFormat.Format32bppArgb ? 4 : 3; // only works with 32 or 24 pixel-size bitmap!
                var padding = lockedBits.Stride - (lockedBits.Width * pixelSize);
                var bytes = new byte[lockedBits.Height * lockedBits.Stride];

                // copy the bytes from bitmap to array
                Marshal.Copy(lockedBits.Scan0, bytes, 0, bytes.Length);

                var index = 0;

                for (var y = 0; y < lockedBits.Height; y++)
                {
                    for (var x = 0; x < lockedBits.Width; x++)
                    {
                        int startPos = y * image.Width * 3 + x * 3;

                        data[startPos + 0] = bytes[index + 2];
                        data[startPos + 1] = bytes[index + 1];
                        data[startPos + 2] = bytes[index + 0];

                        index += pixelSize;
                    }

                    index += padding;
                }

                return new PretzelImage(data, imageWidth, imageHeight);
            }
        }
    }
}
