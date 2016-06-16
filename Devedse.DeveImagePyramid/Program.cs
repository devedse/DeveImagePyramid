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
            var testImagePath = @"0_0.png";

            var w = Stopwatch.StartNew();
            for (int i = 0; i < 1000; i++)
            {
                var readImage = ImageReader.ReadImage(testImagePath);
            }
            w.Stop();
            Console.WriteLine("Elapsed: " + w.Elapsed);

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }
}
