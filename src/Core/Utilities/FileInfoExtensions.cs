using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gk.Core.Utilities
{
    public static class FileInfoExtensions
    {
        public static void Rename(this FileInfo fileInfo, string newName)
        {
            fileInfo.MoveTo(Path.Combine(fileInfo.Directory.FullName, newName));
        }

        public static string AppendTimeStamp(this string fileName)
        {
            return string.Concat(
                Path.GetFileNameWithoutExtension(fileName), "_",
                DateTime.Now.ToString("yyyyMMdd"),
                Path.GetExtension(fileName)
            );
        }
    }
}
