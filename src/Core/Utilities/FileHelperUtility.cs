using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileHelpers;
using log4net;

namespace Gk.Core.Utilities
{
    public class FileHelperUtility<T> where T:class
    {
        public static void ChangeDelimeterOfCsvFile(string filePath)
        {
            #region Changing csv delimeter from comma to tabs using FileHelpers library

            var engine = new FileHelperEngine<T>();

            var result = engine.ReadFile(filePath);

            var delEngine = new DelimitedFileEngine<T>(Encoding.UTF8);

            delEngine.Options.Delimiter = "\t";
            delEngine.HeaderText = delEngine.GetFileHeader();
            delEngine.WriteFile(filePath, result);
            #endregion
        }

        public static List<T> ReadCSV(string filePath)
        {
            var engine = new FileHelperEngine<T>();

            var result = engine.ReadFile(filePath);

            return result.ToList();
        }
    }
}
