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
    public static class ImageWriter
    {
        public static void WriteImage(string path, PretzelImage pretzelImage)
        {
            var extension = Path.GetExtension(path);

            if (extension == ".tiff" || extension == ".tif")
            {
                WriteImageTiff(path, pretzelImage);
            }
            else if (extension == ".png")
            {
                WriteImagePng(path, pretzelImage);
            }
            else
            {
                throw new NotSupportedException($"We don't support images with the extension: {extension}");
            }
        }

        private static void WriteImageTiff(string path, PretzelImage pretzelImage)
        {
            using (var tif = Tiff.Open(path, "w"))
            {
                if (tif == null)
                {
                    throw new InvalidOperationException("Tif file could not be opened. It is probably in use: " + path);
                }

                tif.SetField(TiffTag.IMAGEWIDTH, pretzelImage.Width);
                tif.SetField(TiffTag.IMAGELENGTH, pretzelImage.Height);
                tif.SetField(TiffTag.BITSPERSAMPLE, 8);
                tif.SetField(TiffTag.SAMPLESPERPIXEL, 3);
                tif.SetField(TiffTag.PHOTOMETRIC, Photometric.RGB);
                tif.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);

                //LZW is about 4 times as fast as DEFLATE (on an SSD) but takes around 1.5 times the disk space
                //Deflate is also better or equal to PngOut, probably due to using tiles here
                //On a HDD DEFLATE was faster for some reason
                //It's still better in compression then PNG
                tif.SetField(TiffTag.COMPRESSION, Compression.DEFLATE);

                //TileSizes lower then 16 are not supported so we write the image using scanlines here
                if (pretzelImage.Width < 16 || pretzelImage.Height < 16)
                {
                    tif.SetField(TiffTag.ROWSPERSTRIP, 1);

                    byte[] scanLineColor_ptr = new byte[pretzelImage.Width * 3];

                    for (int y = 0; y < pretzelImage.Height; y++)
                    {
                        for (int x = 0; x < pretzelImage.Width; x++)
                        {
                            int startPos = x * 3;
                            int sourcePos = y * pretzelImage.Width * 3 + x * 3;

                            scanLineColor_ptr[startPos + 0] = pretzelImage.Data[sourcePos + 0];
                            scanLineColor_ptr[startPos + 1] = pretzelImage.Data[sourcePos + 1];
                            scanLineColor_ptr[startPos + 2] = pretzelImage.Data[sourcePos + 2];
                        }
                        tif.WriteScanline(scanLineColor_ptr, y);
                    }
                }
                else
                {
                    tif.SetField(TiffTag.TILEWIDTH, pretzelImage.Width);
                    tif.SetField(TiffTag.TILELENGTH, pretzelImage.Height);

                    tif.WriteEncodedTile(0, pretzelImage.Data, pretzelImage.Width * pretzelImage.Height * 3);
                }
                tif.FlushData();
                tif.Close();
            }
        }

        private static void WriteImagePng(string path, PretzelImage pretzelImage)
        {
            using (var image = new Bitmap(pretzelImage.Width, pretzelImage.Height, PixelFormat.Format24bppRgb))
            {
                var lockedBits = image.LockBits(new Rectangle(0, 0, pretzelImage.Width, pretzelImage.Height), ImageLockMode.WriteOnly, image.PixelFormat);

                var outputBytes = new byte[lockedBits.Height * lockedBits.Stride];

                const int pixelSize = 3;
                var padding = lockedBits.Stride - (lockedBits.Width * pixelSize);

                var index = 0;
                for (int y = 0; y < pretzelImage.Height; y++)
                {
                    for (int x = 0; x < pretzelImage.Width; x++)
                    {
                        int startPos = y * image.Width * 3 + x * 3;

                        outputBytes[index + 2] = pretzelImage.Data[startPos + 0];
                        outputBytes[index + 1] = pretzelImage.Data[startPos + 1];
                        outputBytes[index + 0] = pretzelImage.Data[startPos + 2];

                        index += pixelSize;
                    }

                    index += padding;
                }

                Marshal.Copy(outputBytes, 0, lockedBits.Scan0, outputBytes.Length);

                image.Save(path, ImageFormat.Png);
            }
        }
    }
}
