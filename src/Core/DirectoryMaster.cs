using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gk.Core
{
    public class DirectoryMaster
    {
        public string TargetDirectoryA { get; set; }

        public string TargetDirectoryB { get; set; }

        public DirectoryMaster(string targetDirectoryA, string targetDirectoryB)
        {
            TargetDirectoryA = targetDirectoryA;
            TargetDirectoryB = targetDirectoryB;
        }
    }
}
