using System;
using System.IO;
using System.Text;
using Gk.Core.CsvTypes;
using Gk.Core.SFTP;
using FileHelpers;
using Gk.Core.Files;
using Gk.Core.Utilities;
using log4net;
using Renci.SshNet;

namespace Gk.Core
{
    public class FileOperations
    {
        private DirectoryMaster DirectoryMaster { get; set; }
        public ABSFiles AbsFiles { get; set; }

        private ILog Log { get; set; }
        private CSVOperations CsvOperations { get; set; }
        public FileOperations(DirectoryMaster directoryMaster, ABSFiles absFiles, ILog log, CSVOperations csvOperations)
        {
            DirectoryMaster = directoryMaster;
            AbsFiles = absFiles;

            Log = log;
            CsvOperations = csvOperations;
        }

        //public string GetCachePrefixForPath(string path)
        //{
        //    var directoryPath = Path.GetDirectoryName(path);

        //    //if (directoryPath.Equals(DirectoryMaster.TargetDirectoryA))
        //    //    return "confused_";
        //    //if (directoryPath.Equals(DirectoryMaster.TargetDirectoryB))
        //    //    return "ctm_";
        //    //else
        //    //    return null;
        //}

        public void PerformOperationForKey(string key, string path)
        {
            //if (key.StartsWith("confused"))
            //    new ConfusedFile(Log,CsvOperations,SftpSettingsForConfused).DoOperations(path);     //TODO : Handle when multiple files are dropped at the same time
            //else if (key.StartsWith("ctm"))
            //    CTMDailyFiles.DoOperations(path);

            if (Path.GetDirectoryName(path) == DirectoryMaster.TargetDirectoryA)
            {
                new CustomerFiles(Log,CsvOperations).DoOperations(path);
            }
        }
    }
}
