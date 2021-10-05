using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gk.Core.Utilities
{
    public class CSVOperations
    {
        public void Update_Headers_Remove_Underscores(string path)
        {
            var file = File
                .ReadLines(path)
                .SkipWhile(line => string.IsNullOrWhiteSpace(line)) // To be on the safe side
                .Select((line, index) => index == 0 // do we have header? 
                    ? line.Replace('_', ' ') // replace '_' with ' '
                    : line)                  // keep lines as they are 
                .ToList();                  // Materialization, since we write into the same file

            File.WriteAllLines(path, file);
        }

        public void Replace_comma_with_tabs(string path)
        {
            var file = File
                .ReadLines(path)
                .SkipWhile(line => string.IsNullOrWhiteSpace(line)) // To be on the safe side
                .Select((line, index) => line.Replace(',', '\t')) // replace '_' with ' '
                .ToList();                  // Materialization, since we write into the same file

            File.WriteAllLines(path, file);
        }
    }
}
