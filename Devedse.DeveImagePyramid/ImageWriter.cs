using BitMiracle.LibTiff.Classic;
using Devedse.DeveImagePyramid.Logging;
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
    public class ImageWriter
    {
        private readonly RetryHandler _retryHandler;
        private readonly ILogger _logger;

        public ImageWriter(RetryHandler retryHandler, ILogger logger)
        {
            _retryHandler = retryHandler;
            _logger = logger;
        }

        public void WriteImage(string path, PretzelImage pretzelImage)
        {
            var extension = Path.GetExtension(path);

            Action saveAction;

            if (extension == ".tiff" || extension == ".tif")
            {
                saveAction = () => WriteImageTiff(path, pretzelImage);
            }
            else if (extension == ".png")
            {
                saveAction = () => WriteImagePng(path, pretzelImage);
            }
            else
            {
                var exceptionString = $"We don't support images with the extension: {extension}";
                _logger.WriteError(exceptionString, LogLevel.Exception);
                throw new NotSupportedException(exceptionString);
            }

            _retryHandler.Retry(saveAction, 10);
        }

        private void WriteImageTiff(string path, PretzelImage pretzelImage)
        {
            using (var tif = Tiff.Open(path, "w"))
            {
                if (tif == null)
                {
                    var exceptionString = $"Tif file could not be opened. It is probably in use: {path}";
                    _logger.WriteError(exceptionString, LogLevel.Exception);
                    throw new InvalidOperationException(exceptionString);
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

        private void WriteImagePng(string path, PretzelImage pretzelImage)
        {
            _logger.Write($"Writing image Png: {path}, PretzelImage.Width: {pretzelImage.Width} PretzelImage.Height: {pretzelImage.Height} PretzelImage.Data.Length: {pretzelImage.Data.Length}", LogLevel.Verbose);

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
