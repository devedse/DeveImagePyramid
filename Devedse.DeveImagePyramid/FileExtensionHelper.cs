using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devedse.DeveImagePyramid
{
    public static class FileExtensionHelper
    {
        public static bool IsValidImageFileExtension(string extension)
        {
            var lowerExtension = extension.ToLower();
            if (lowerExtension == ".png" || lowerExtension == ".tif" || lowerExtension == ".tiff")
            {
                return true;
            }
            return false;
        }
    }
}
