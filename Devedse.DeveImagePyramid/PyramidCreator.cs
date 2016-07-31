using Devedse.DeveImagePyramid.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devedse.DeveImagePyramid
{
    public class PyramidCreator
    {
        private readonly ILogger _logger;
        private readonly ImageWriter _imageWriter;
        private readonly ImageReader _imageReader;

        public PyramidCreator(ImageWriter imageWriter, ImageReader imageReader, ILogger logger)
        {
            _imageWriter = imageWriter;
            _imageReader = imageReader;
            _logger = logger;
        }

        public int MoveInputToOutputAndConvert(string inputFolder, string outputFolder, string desiredExtension, bool useParallel)
        {
            _logger.Write("Counting all files from input directory...");

            var filesInDirectoryEnumerable = Directory.EnumerateFiles(inputFolder).Where(t => FileExtensionHelper.IsValidImageFileExtension(Path.GetExtension(t)));

            var filesInInputCount = 0;
            var foundExtensions = new HashSet<string>();

            foreach (var foundFile in filesInDirectoryEnumerable)
            {
                filesInInputCount++;

                var extension = Path.GetExtension(foundFile);
                foundExtensions.Add(extension);
            }

            _logger.Write($"Found {filesInInputCount} files in {inputFolder}");

            if (foundExtensions.Count != 1)
            {
                var exceptionString = $"Did not find exactly 1 file extension. Found '{foundExtensions.Count}' extensions instead. Extensions: {string.Join(",", foundExtensions)}.";
                _logger.WriteError(exceptionString, LogLevel.Exception);
                throw new InvalidOperationException(exceptionString);
            }

            var foundExtension = foundExtensions.Single();

            _logger.Write($"Found extension: '{foundExtension}'");

            var firstImageFileName = $"0_0{foundExtension}";
            var firstImagePath = Path.Combine(inputFolder, firstImageFileName);
            var firstImage = _imageReader.ReadImage(firstImagePath);

            int filesInWidthAndHeight = (int)Math.Sqrt(filesInInputCount);
            int sizeInWidthAndHeight = filesInWidthAndHeight * firstImage.Width;

            int deepestFolderNumber = (int)Math.Log(sizeInWidthAndHeight, 2);

            var outputForThis = Path.Combine(outputFolder, deepestFolderNumber.ToString());
            _logger.Write($"Creating output directory: '{outputForThis}'");
            Directory.CreateDirectory(outputForThis);
            var fileConversionAction = new Action<string>(filePath =>
            {
                var extension = Path.GetExtension(filePath);
                if (FileExtensionHelper.IsValidImageFileExtension(extension))
                {
                    var readImage = _imageReader.ReadImage(filePath);

                    var outputFileName = Path.GetFileNameWithoutExtension(filePath);
                    var outputFileNameWithExtension = outputFileName + desiredExtension;
                    var totalOutputPath = Path.Combine(outputForThis, outputFileNameWithExtension);

                    _logger.Write($"Writing: {outputFileNameWithExtension}", LogLevel.Verbose);
                    _imageWriter.WriteImage(totalOutputPath, readImage);
                }
            });

            _logger.Write("Starting conversion/copy of images...", color: ConsoleColor.Yellow);

            if (useParallel)
            {
                Parallel.ForEach(filesInDirectoryEnumerable, fileConversionAction);
            }
            else
            {
                foreach (var filePath in filesInDirectoryEnumerable)
                {
                    fileConversionAction(filePath);
                }
            }

            _logger.Write("Completed conversion/copy of images.", color: ConsoleColor.Green);

            return deepestFolderNumber;
        }

        public void CreatePyramid(string inputFolder, string outputFolder, string desiredExtension, bool useParallel)
        {
            _logger.Write("Counting all files from input directory...");

            var allFilesInInputEnumerable = Directory.EnumerateFiles(inputFolder).Where(t => FileExtensionHelper.IsValidImageFileExtension(Path.GetExtension(t)));

            var filesInInputCount = 0;
            var foundExtensions = new HashSet<string>();

            foreach (var foundFile in allFilesInInputEnumerable)
            {
                filesInInputCount++;

                var extension = Path.GetExtension(foundFile);
                foundExtensions.Add(extension);
            }

            _logger.Write($"Found {filesInInputCount} files in {inputFolder}");

            if (foundExtensions.Count != 1)
            {
                var exceptionString = $"Did not find exactly 1 file extension. Found '{foundExtensions.Count}' extensions instead. Extensions: {string.Join(",", foundExtensions)}.";
                _logger.WriteError(exceptionString, LogLevel.Exception);
                throw new InvalidOperationException(exceptionString);
            }

            var foundExtension = foundExtensions.Single();

            _logger.Write($"Found extension: '{foundExtension}'");

            if (!MathHelper.IsPowerOfTwo(filesInInputCount))
            {
                var exceptionString = "Amount of files in input directory is not a power of 2";
                _logger.WriteError(exceptionString, LogLevel.Exception);
                throw new InvalidOperationException(exceptionString);
            }

            _logger.Write($"Creating output directory: '{outputFolder}'");
            Directory.CreateDirectory(outputFolder);

            var folderName = Path.GetFileName(outputFolder);

            _logger.Write("Starting scaling of images...", color: ConsoleColor.Yellow);

            if (filesInInputCount == 1)
            {
                var inputFileName = $"0_0{foundExtension}";
                var inputTotalPath = Path.Combine(inputFolder, inputFileName);
                var singleImage = _imageReader.ReadImage(inputTotalPath);

                var combinedImage = new PretzelImageCombined(singleImage);
                var scaledCombinedImage = ImageZoomOuter.ScaleV2(combinedImage);

                var outputFileName = $"0_0{foundExtension}";
                var outputTotalPath = Path.Combine(outputFolder, outputFileName);

                _logger.Write($"Writing scaled: {Path.Combine(folderName, outputFileName)}", LogLevel.Verbose);
                _imageWriter.WriteImage(outputTotalPath, scaledCombinedImage);
            }
            else
            {
                var expectedFilesInOutput = filesInInputCount / 4;
                //var filesInWidth = (int)Math.Sqrt(expectedFilesInOutput);
                var filesInHeight = (int)Math.Sqrt(expectedFilesInOutput);

                var scaleAction = new Action<int>(i =>
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

                    var topLeft = _imageReader.ReadImage(topLeftTotalPath);
                    var bottomLeft = _imageReader.ReadImage(bottomLeftTotalPath);
                    var topRight = _imageReader.ReadImage(topRightTotalPath);
                    var bottomRight = _imageReader.ReadImage(bottomRightTotalPath);

                    var combinedImage = new PretzelImageCombined(topLeft, bottomLeft, topRight, bottomRight);
                    var scaledCombinedImage = ImageZoomOuter.ScaleV2(combinedImage);

                    var outputFileName = $"{xStart / 2}_{yStart / 2}{desiredExtension}";
                    var outputTotalPath = Path.Combine(outputFolder, outputFileName);

                    _logger.Write($"Writing scaled: {Path.Combine(folderName, outputFileName)}", LogLevel.Verbose);
                    _imageWriter.WriteImage(outputTotalPath, scaledCombinedImage);
                });

                if (useParallel)
                {
                    Parallel.For(0, expectedFilesInOutput, scaleAction);
                }
                else
                {
                    for (int i = 0; i < expectedFilesInOutput; i++)
                    {
                        scaleAction(i);
                    }
                }
            }

            _logger.Write("Completed scaling of images.", color: ConsoleColor.Green);
        }
    }
}
