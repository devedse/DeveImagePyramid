using Devedse.DeveImagePyramid.Logging;
using Devedse.DeveImagePyramid.Logging.Appenders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Devedse.DeveImagePyramid
{
    class Program
    {
        static void Main(string[] args)
        {
            bool useDifferentFileNamesForLogs = false;

            string inputFolder = @"F:\Maze";
            string outputFolder = @"F:\MazeV\mydz_files";
            string desiredExtension = ".png";
            bool useParallel = true;

            ExecuteImagePyramidGeneration(useDifferentFileNamesForLogs, inputFolder, outputFolder, desiredExtension, useParallel);
        }

        private static void ExecuteImagePyramidGeneration(bool useDifferentFileNamesForLogs, string inputFolder, string outputFolder, string desiredExtension, bool useParallel)
        {
            using (var logger = CreateLogger(useDifferentFileNamesForLogs))
            {
                var retryHandler = new RetryHandler(logger);
                var imageReader = new ImageReader(logger);
                var imageWriter = new ImageWriter(retryHandler, logger);
                var pyramidCreator = new PyramidCreator(imageWriter, imageReader, logger);


                logger.Write($"Starting generation of lowest level folder (+ conversion to {desiredExtension})", color: ConsoleColor.Yellow);
                int deepestFolderNumber = pyramidCreator.MoveInputToOutputAndConvert(inputFolder, outputFolder, desiredExtension, useParallel);
                logger.Write("Done with generation of lowest level.", color: ConsoleColor.Green);
                logger.EmptyLine();

                logger.Write("Starting the scaling process...");
                for (int i = deepestFolderNumber - 1; i >= 1; i--)
                {
                    var destFolder = Path.Combine(outputFolder, i.ToString());
                    var srcFolder = Path.Combine(outputFolder, (i + 1).ToString());

                    logger.Write($"Starting with scale {i}", color: ConsoleColor.Yellow);
                    pyramidCreator.CreatePyramid(srcFolder, destFolder, desiredExtension, useParallel);
                    logger.Write($"Done with scale {i}", color: ConsoleColor.Green);
                    logger.EmptyLine();
                }

                logger.Write("Completed, press any key to continue...");
                Console.ReadKey();
            }
        }

        private static DateTimeLoggerAppender CreateLogger(bool useDifferentFileNamesForLogs)
        {
            var consoleLogger = new ConsoleLogger(LogLevel.Verbose);

            var logFileName = useDifferentFileNamesForLogs ? DateTime.Now.ToString("yyyy-MM-dd--HH-mm-ss") + ".txt" : "Log.txt";
            var fileLoggerPath = Path.Combine(AssemblyDirectory, "Logs", logFileName);
            var fileLogger = new FileLogger(fileLoggerPath, LogLevel.Verbose);

            var multiLogger = new MultiLoggerAppender(new List<ILogger>() { consoleLogger, fileLogger });

            var loggingLevelAppender = new LoggingLevelLoggerAppender(multiLogger, " <:>");
            var dateTimeAppender = new DateTimeLoggerAppender(loggingLevelAppender, ":");
            return dateTimeAppender;
        }

        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }


        //private static void ScaleTest()
        //{
        //    var topLeft = ImageReader.ReadImage("0_0.tiff");
        //    var bottomLeft = ImageReader.ReadImage("0_1.tiff");
        //    var topRight = ImageReader.ReadImage("1_0.tiff");
        //    var bottomRight = ImageReader.ReadImage("1_1.tiff");

        //    var combinedImage = new PretzelImageCombined(topLeft, bottomLeft, topRight, bottomRight);

        //    var scaledImage = ImageZoomOuter.ScaleV2(combinedImage);

        //    ImageWriter.WriteImage("scaledImage.tiff", scaledImage);
        //}

        //private static void PerfTest()
        //{
        //    var ext = ".tiff";

        //    ImageWriter.WriteImage("testje.png", ImageReader.ReadImage("0_0.png"));
        //    ImageWriter.WriteImage("testje.tiff", ImageReader.ReadImage("0_0.tiff"));

        //    Console.WriteLine("Test completed.");

        //    var testImagePath = $"0_0{ext}";

        //    var w = Stopwatch.StartNew();

        //    for (int i = 0; i < 1000; i++)
        //    {
        //        ImageReader.ReadImage(testImagePath);
        //    }
        //    w.Stop();
        //    Console.WriteLine("Read: " + w.Elapsed);

        //    var theImage = ImageReader.ReadImage(testImagePath);

        //    w.Restart();

        //    for (int i = 0; i < 1000; i++)
        //    {
        //        ImageWriter.WriteImage($"test{ext}", theImage);
        //    }

        //    w.Stop();

        //    Console.WriteLine("Write: " + w.Elapsed);
        //}
    }
}
