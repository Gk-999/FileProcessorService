﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gk.Core.CsvTypes;
using Gk.Core.Files;
using Gk.Core.SFTP;
using Gk.Core.Utilities;
using log4net;

namespace Gk.Core.Files
{
    public sealed class CustomerFiles : FileBase
    {
        private ILog Log { get; set; }
        private CSVOperations CsvOperations { get; set; }
        public CustomerFiles(ILog log, CSVOperations csvOperations) : base(log, csvOperations)
        {
            Log = log;
            CsvOperations = csvOperations;
        }

        public override string FileNamePrefix { get { return "Customer"; } }

        public override string DateFormatFrom { get { return "yyyy-mm-dd"; } }

        public override string DateFormatTo { get { return "yyyy-MM-dd"; } }

        public override string Path { get; set; }

        public override string ChangeExtensionFrom { get { return "csv"; } }

        public override string ChangeExtensionTo { get { return "txt"; } }

        public void DoOperations(string path)
        {
            Path = path;
            if (DoRename())
            {
                FileHelperUtility<Customer>.ChangeDelimeterOfCsvFile(NewPathAfterDateReplace);
                FixHeaders_RemoveUnderscores();
                ChangeExtensionToTxt();
                //UploadFileToFtp();
                DeleteIfExist(NewPathAfterDateReplace);
            }
        }
    }
}
