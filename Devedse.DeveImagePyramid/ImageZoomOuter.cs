using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devedse.DeveImagePyramid
{
    public static class ImageZoomOuter
    {
        public static PretzelImage Scale(PretzelImageCombined combinedImage, bool useRealitvePixelScale)
        {
            //If the bool useRealitvePixelScale is true we want to count diagonal pixels only 0.25 of the times as the main pixel etc

            int newImageWidth = combinedImage.Width / 2;
            int newImageHeight = combinedImage.Height / 2;

            var outputBytes = new byte[newImageWidth * newImageHeight * 3];
            var scaledOutputImage = new PretzelImage(outputBytes, newImageWidth, newImageHeight);

            int combinedImageWidth = combinedImage.Width;
            int combinedImageHeight = combinedImage.Height;

            for (int y = 0; y < scaledOutputImage.Height; y++)
            {
                for (int x = 0; x < scaledOutputImage.Width; x++)
                {
                    int r = 0;
                    int g = 0;
                    int b = 0;
                    int count = 0;

                    int scaledX = x * 2;
                    int scaledY = y * 2;

                    //Diagonal pixels only count once
                    if (scaledX > 0 && scaledY > 0)
                    {
                        //TopLeft
                        AddPixelNumberOfTimes(combinedImage, 1, scaledX - 1, scaledY - 1, ref r, ref g, ref b, ref count);
                    }
                    if (scaledX > 0 && scaledY < combinedImageHeight)
                    {
                        //BottomLeft
                        AddPixelNumberOfTimes(combinedImage, 1, scaledX - 1, scaledY + 1, ref r, ref g, ref b, ref count);
                    }
                    if (scaledX < combinedImageWidth && scaledY > 0)
                    {
                        //TopRight
                        AddPixelNumberOfTimes(combinedImage, 1, scaledX + 1, scaledY - 1, ref r, ref g, ref b, ref count);
                    }
                    if (scaledX < combinedImageWidth && scaledY < combinedImageHeight)
                    {
                        //BottomRight
                        AddPixelNumberOfTimes(combinedImage, 1, scaledX + 1, scaledY + 1, ref r, ref g, ref b, ref count);
                    }

                    //Pixels on the side count twice
                    if (scaledX > 0)
                    {
                        //Left
                        AddPixelNumberOfTimes(combinedImage, useRealitvePixelScale ? 2 : 1, scaledX - 1, scaledY, ref r, ref g, ref b, ref count);
                    }
                    if (scaledX < combinedImageWidth)
                    {
                        //Right
                        AddPixelNumberOfTimes(combinedImage, useRealitvePixelScale ? 2 : 1, scaledX + 1, scaledY, ref r, ref g, ref b, ref count);
                    }
                    if (scaledY > 0)
                    {
                        //Top
                        AddPixelNumberOfTimes(combinedImage, useRealitvePixelScale ? 2 : 1, scaledX, scaledY - 1, ref r, ref g, ref b, ref count);
                    }
                    if (scaledY < combinedImageHeight)
                    {
                        //Bottom
                        AddPixelNumberOfTimes(combinedImage, useRealitvePixelScale ? 2 : 1, scaledX, scaledY + 1, ref r, ref g, ref b, ref count);
                    }

                    //Pixel itself 4 times
                    AddPixelNumberOfTimes(combinedImage, useRealitvePixelScale ? 4 : 1, scaledX, scaledY, ref r, ref g, ref b, ref count);

                    byte averageR = (byte)(r / count);
                    byte averageG = (byte)(g / count);
                    byte averageB = (byte)(b / count);

                    var startPos = y * 3 * scaledOutputImage.Width + x * 3;

                    scaledOutputImage.Data[startPos + 0] = averageR;
                    scaledOutputImage.Data[startPos + 1] = averageG;
                    scaledOutputImage.Data[startPos + 2] = averageB;
                }
            }

            return scaledOutputImage;
        }

        private static void AddPixelNumberOfTimes(PretzelImageCombined combinedImage, int timesToAdd, int scaledX, int scaledY, ref int r, ref int g, ref int b, ref int count)
        {
            var pixelhere = combinedImage.GetPixel(scaledX, scaledY);

            r += pixelhere.R * timesToAdd;
            g += pixelhere.G * timesToAdd;
            b += pixelhere.B * timesToAdd;
            count += timesToAdd;
        }
    }
}
