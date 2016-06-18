using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devedse.DeveImagePyramid
{
    class Program
    {
        static void Main(string[] args)
        {
            string inputFolder = @"C:\TheFolder\Github\DeveMazeGenerator\DeveMazeGeneratorGui\bin\Release\MegaTerrorMaze-DynamicPath-WithTiles-NoColorMap";
            string outputFolder = @"C:\#ImageScalerOutput";
            string desiredExtension = ".tiff";
            int deepestFolderNumber = 15;
            bool useRealitvePixelScale = true;
            bool useParallel = true;

            Console.WriteLine($"Starting generation of lowest level folder (+ conversion to {desiredExtension})");
            //PyramidCreator.MoveInputToOutputAndConvert(inputFolder, outputFolder, desiredExtension, deepestFolderNumber, useParallel);
            Console.WriteLine();

            Console.WriteLine("Starting the scaling process...");
            for (int i = deepestFolderNumber - 1; i >= 1; i--)
            {
                var destFolder = Path.Combine(outputFolder, i.ToString());
                var srcFolder = Path.Combine(outputFolder, (i + 1).ToString());

                Console.WriteLine($"Starting with scale {i}");
                PyramidCreator.CreatePyramid(srcFolder, destFolder, desiredExtension, useRealitvePixelScale, useParallel);
                Console.WriteLine();
            }

            Console.WriteLine("Completed, press any key to continue...");
            Console.ReadKey();
        }



        private static void ScaleTest()
        {
            var topLeft = ImageReader.ReadImage("0_0.tiff");
            var bottomLeft = ImageReader.ReadImage("0_1.tiff");
            var topRight = ImageReader.ReadImage("1_0.tiff");
            var bottomRight = ImageReader.ReadImage("1_1.tiff");

            var combinedImage = new PretzelImageCombined(topLeft, bottomLeft, topRight, bottomRight);

            var scaledImage = ImageZoomOuter.Scale(combinedImage, false);

            ImageWriter.WriteImage("scaledImage.tiff", scaledImage);
        }

        private static void PerfTest()
        {
            var ext = ".tiff";

            ImageWriter.WriteImage("testje.png", ImageReader.ReadImage("0_0.png"));
            ImageWriter.WriteImage("testje.tiff", ImageReader.ReadImage("0_0.tiff"));

            Console.WriteLine("Test completed.");

            var testImagePath = $"0_0{ext}";

            var w = Stopwatch.StartNew();

            for (int i = 0; i < 1000; i++)
            {
                var readImage = ImageReader.ReadImage(testImagePath);
            }
            w.Stop();
            Console.WriteLine("Read: " + w.Elapsed);

            var theImage = ImageReader.ReadImage(testImagePath);

            w.Restart();

            for (int i = 0; i < 1000; i++)
            {
                ImageWriter.WriteImage($"test{ext}", theImage);
            }

            w.Stop();

            Console.WriteLine("Write: " + w.Elapsed);
        }
    }
}
