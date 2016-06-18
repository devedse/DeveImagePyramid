using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devedse.DeveImagePyramid
{
    public static class PyramidCreator
    {
        public static void MoveInputToOutputAndConvert(string inputFolder, string outputFolder, string desiredExtension, int deepestFolderNumber)
        {
            var outputForThis = Path.Combine(outputFolder, deepestFolderNumber.ToString());
            Directory.CreateDirectory(outputForThis);

            var filesInDirectory = Directory.GetFiles(inputFolder);


            Parallel.ForEach(filesInDirectory, filePath =>
            {
                var extension = Path.GetExtension(filePath);
                if (FileExtensionHelper.IsValidImageFileExtension(extension))
                {
                    var readImage = ImageReader.ReadImage(filePath);

                    var outputFileName = Path.GetFileNameWithoutExtension(filePath);
                    var outputFileNameWithExtension = outputFileName + desiredExtension;
                    var totalOutputPath = Path.Combine(outputForThis, outputFileNameWithExtension);

                    Console.WriteLine($"Writing: {outputFileNameWithExtension}");
                    ImageWriter.WriteImage(totalOutputPath, readImage);
                }
            });
        }

        public static void CreatePyramid(string inputFolder, string outputFolder, string desiredExtension, bool useRealitvePixelScale)
        {
            var allFilesInInput = Directory.GetFiles(inputFolder).Where(t => FileExtensionHelper.IsValidImageFileExtension(Path.GetExtension(t))).ToList();
            if (!MathHelper.IsPowerOfTwo(allFilesInInput.Count))
            {
                throw new InvalidOperationException("Amount of files in input directory is not a power of 2");
            }
            var foundExtension = allFilesInInput.Select(t => Path.GetExtension(t)).Distinct().SingleOrDefault();

            Directory.CreateDirectory(outputFolder);

            var folderName = Path.GetFileName(outputFolder);

            var expectedFilesInOutput = allFilesInInput.Count / 4;
            int filesInWidth = (int)Math.Sqrt(expectedFilesInOutput);
            int filesInHeight = (int)Math.Sqrt(expectedFilesInOutput);

            Parallel.For(0, expectedFilesInOutput, i =>
            {
                int xStart = (i % filesInHeight) * 2;
                int yStart = (i / filesInHeight) * 2;

                var topLeftFileName = $"{xStart}_{yStart}{foundExtension}";
                var bottomLeftFileName = $"{xStart}_{yStart + 1}{foundExtension}";
                var topRightFileName = $"{xStart + 1}_{yStart}{foundExtension}";
                var bottomRightFileName = $"{xStart + 1}_{yStart + 1}{foundExtension}";

                var topLeftTotalPath = Path.Combine(inputFolder, topLeftFileName);
                var bottomLeftTotalPath = Path.Combine(inputFolder, bottomLeftFileName);
                var topRightTotalPath = Path.Combine(inputFolder, topRightFileName);
                var bottomRightTotalPath = Path.Combine(inputFolder, bottomRightFileName);

                var topLeft = ImageReader.ReadImage(topLeftTotalPath);
                var bottomLeft = ImageReader.ReadImage(bottomLeftTotalPath);
                var topRight = ImageReader.ReadImage(topRightTotalPath);
                var bottomRight = ImageReader.ReadImage(bottomRightTotalPath);

                var combinedImage = new PretzelImageCombined(topLeft, bottomLeft, topRight, bottomRight);
                var scaledCombinedImage = ImageZoomOuter.Scale(combinedImage, useRealitvePixelScale);

                var outputFileName = $"{xStart / 2}_{yStart / 2}{desiredExtension}";
                var outputTotalPath = Path.Combine(outputFolder, outputFileName);

                Console.WriteLine($"Writing scaled: {Path.Combine(folderName, outputFileName)}");
                ImageWriter.WriteImage(outputTotalPath, scaledCombinedImage);
            });
        }
    }
}
