using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Renci.SshNet;

namespace Gk.Core.SFTP
{
    public class SftpSettingsBase
    {
        public string Host { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Port { get; set; }
        public string SftpTargetDirectory { get; set; }
        public string KeyFileName { get; set; }
        public SftpClient CreateClient()
        { 
            if(string.IsNullOrWhiteSpace(Port))
                return new SftpClient(Host,Username,Password);
            else
            {
                return new SftpClient(Host, Convert.ToInt32(Port), Username, Password);
            }
        }

        public SftpClient CreateClientUsingKeyFile()
        {
            var filename = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, KeyFileName);
            var pk = new PrivateKeyFile(filename);
            var keyFiles = new[] { pk };

            if (string.IsNullOrWhiteSpace(Port))
                return new SftpClient(Host, Username, keyFiles);
            else
            {
                return new SftpClient(Host, Convert.ToInt32(Port), Username, keyFiles);
            }
        }
    }
}
