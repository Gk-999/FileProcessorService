using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Security.Authentication;
using Gk.Core.SFTP;
using Gk.Core.SMTP;
using Gk.Core.Utilities;
using log4net;
using Renci.SshNet;

namespace Gk.Core.Files
{
    public class FileBase
    {
        private ILog Log { get; set; }
        private CSVOperations CsvOperations { get; set; }
        private int DaysBefore { get; set; }

        public FileBase(ILog log, CSVOperations csvOperations)
        {
            Log = log;
            CsvOperations = csvOperations;
            DaysBefore = Convert.ToInt32(ConfigurationManager.AppSettings["DaysBefore"]);
        }
        public virtual string FileNamePrefix { get; set; }

        public virtual string DateFormatFrom { get; set; }

        public virtual string DateFormatTo { get; set; }

        public virtual string Path { get; set; }
        public virtual string ChangeExtensionFrom { get; set; }
        public virtual string ChangeExtensionTo { get; set; }

        public virtual string NewPathAfterDateReplace
        {
            get
            {
                var newFilePath = Path.ReplaceInsensitive(FileName, NewFileName);
                return newFilePath;
            }
        }

        public virtual string NewPathAfterChangeExtension
        {
            get
            {
                var newFilePath = Path.ReplaceInsensitive(FileName, NewFileName);

                if (!string.IsNullOrWhiteSpace(ChangeExtensionTo))
                    newFilePath = newFilePath.ReplaceInsensitive(ChangeExtensionFrom, ChangeExtensionTo);

                return newFilePath;
            }
        }

        public virtual string FileName { get { return System.IO.Path.GetFileName(Path); } }

        public virtual string NewFileName { get { return FileName.ReplaceInsensitive(DateFormatFrom, DateTime.Now.AddDays(DaysBefore).ToString(DateFormatTo)); } }

        public virtual SftpSettingsBase SftpSettingsBase { get; set; }

        public virtual SmtpConfig SmtpConfig { get; set; }
        public virtual string FromEmailAddress { get; set; }
        public virtual string ToEmailAddress { get; set; }
        public virtual string EmailSubject { get; set; }
        public virtual string EmailBody { get; set; }
        public bool DoRename()
        {
            bool isSuccess = false;
            var filename = System.IO.Path.GetFileName(Path);

            if (filename.StartsWith(FileNamePrefix) && filename.Contains(DateFormatFrom, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    FileInfo file = new FileInfo(Path);
                    var newFileName = filename.ReplaceInsensitive(DateFormatFrom, DateTime.Now.AddDays(DaysBefore).ToString(DateFormatTo));
                    file.Rename(newFileName);
                    Log.InfoFormat("File {0} renamed successfully to {1} {2}", System.IO.Path.GetFileName(Path), newFileName, Environment.NewLine);
                    isSuccess = true;
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat("Error when renaming the file {0} - {1} {2}", System.IO.Path.GetFileName(Path), ex.Message, Environment.NewLine);
                }
            }
            else
            {
                Log.WarnFormat("File {0} skipped either because file name does not start with {1} or file name does not contain substring {2} {3}", System.IO.Path.GetFileName(Path), FileNamePrefix, DateFormatFrom, Environment.NewLine);
            }

            return isSuccess;
        }

        public void FixHeaders_RemoveUnderscores()
        {
            var pathToNewFile = Path.ReplaceInsensitive(FileName, NewFileName);
            CsvOperations.Update_Headers_Remove_Underscores(pathToNewFile);
            Log.InfoFormat("Headers updated in the file {0} {1}", NewFileName, Environment.NewLine);
        }

        public void ChangeExtensionToTxt()
        {
            var filename = System.IO.Path.GetFileName(Path);
            File.Move(NewPathAfterDateReplace, System.IO.Path.ChangeExtension(NewPathAfterDateReplace, ".txt"));
            Log.InfoFormat("File extension of {0} changed to '.txt' {1}", filename, Environment.NewLine);
        }

        public void UploadFile(bool useKeyFile = false)
        {
            var newFilePath = string.IsNullOrWhiteSpace(ChangeExtensionTo) ? NewPathAfterDateReplace : NewPathAfterChangeExtension;
            try
            {
                if (string.IsNullOrWhiteSpace(SftpSettingsBase.Host))
                {
                    Log.InfoFormat("Uploading file {0} over sftp cancelled as host url not supplied", System.IO.Path.GetFileName(newFilePath));
                    return;
                }
                Log.InfoFormat("Uploading file {0} over sftp", System.IO.Path.GetFileName(newFilePath));
                using (var client = useKeyFile? SftpSettingsBase.CreateClientUsingKeyFile() : SftpSettingsBase.CreateClient())
                {
                    using (var fileStream = new FileStream(newFilePath, FileMode.Open))
                    {
                        DoUpload(newFilePath, client, fileStream, SftpSettingsBase.SftpTargetDirectory);
                    }
                }
                Log.InfoFormat("File {0} uploaded successfully over sftp", System.IO.Path.GetFileName(newFilePath));
                //DeleteIfExist(fromLocation);
            }
            catch (Exception ex)
            {
                Log.FatalFormat("Error when transferring file {0} over sftp - {1}", System.IO.Path.GetFileName(newFilePath), ex.Message);
            }
        }

        private static void DoUpload(string fromLocation, SftpClient client, FileStream fileStream, string targetDirectoryPath)
        {
            try
            {
                client.Connect();
                client.BufferSize = 4 * 1024; // bypass Payload error large files
                if (!string.IsNullOrEmpty(targetDirectoryPath))
                {
                    client.ChangeDirectory(targetDirectoryPath);
                }

                client.UploadFile(fileStream, System.IO.Path.GetFileName(fromLocation));
            }
            finally
            {
                client.Disconnect();
            }
        }

        public void UploadFileToFtp()
        {
            var newFilePath = string.IsNullOrWhiteSpace(ChangeExtensionTo) ? NewPathAfterDateReplace : NewPathAfterChangeExtension;
            try
            {
                if (string.IsNullOrWhiteSpace(SftpSettingsBase.Host))
                {
                    Log.InfoFormat("Uploading file {0} over FTP cancelled as host url not supplied", System.IO.Path.GetFileName(newFilePath));
                    return;
                }
                Log.InfoFormat("Uploading file {0} over FTP", System.IO.Path.GetFileName(newFilePath));

                DoUploadFileToFTP(newFilePath, SftpSettingsBase.Host + SftpSettingsBase.SftpTargetDirectory, SftpSettingsBase.Username, SftpSettingsBase.Password);

                Log.InfoFormat("File {0} uploaded successfully over FTP", System.IO.Path.GetFileName(newFilePath));
                //DeleteIfExist(fromLocation);
            }
            catch (Exception ex)
            {
                Log.FatalFormat("Error when transferring file {0} over FTP - {1}", System.IO.Path.GetFileName(newFilePath), ex.Message);
            }
        }

        private static void DoUploadFileToFTP(string fromLocation, string ftpHost, string ftpUsername, string ftpPassword)
        {
            SetMethodRequiresCWD();     // We need to call this before we instantiate FtpWebRequest https://stackoverflow.com/a/9641870
            string filename = System.IO.Path.GetFileName(fromLocation);
            string ftpfullpath = ftpHost + "/" + filename;
            FtpWebRequest ftp = (FtpWebRequest)FtpWebRequest.Create(ftpfullpath);
            ftp.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

            ftp.KeepAlive = true;
            ftp.UseBinary = true;
            ftp.EnableSsl = true;
            //ftp.UsePassive = true;
            ftp.Method = WebRequestMethods.Ftp.UploadFile;
            ServicePointManager.ServerCertificateValidationCallback += (o, c, ch, er) => true;

            FileStream fs = File.OpenRead(fromLocation);
            byte[] buffer = new byte[fs.Length];
            fs.Read(buffer, 0, buffer.Length);
            fs.Close();

            Stream ftpstream = ftp.GetRequestStream();
            ftpstream.Write(buffer, 0, buffer.Length);
            ftpstream.Close();
        }

        private static void SetMethodRequiresCWD()
        {
            Type requestType = typeof(FtpWebRequest);
            FieldInfo methodInfoField = requestType.GetField("m_MethodInfo", BindingFlags.NonPublic | BindingFlags.Instance);
            Type methodInfoType = methodInfoField.FieldType;
            FieldInfo knownMethodsField = methodInfoType.GetField("KnownMethodInfo", BindingFlags.Static | BindingFlags.NonPublic);
            Array knownMethodsArray = (Array)knownMethodsField.GetValue(null);
            FieldInfo flagsField = methodInfoType.GetField("Flags", BindingFlags.NonPublic | BindingFlags.Instance);
            int MustChangeWorkingDirectoryToPath = 0x100;
            foreach (object knownMethod in knownMethodsArray)
            {
                int flags = (int)flagsField.GetValue(knownMethod);
                flags |= MustChangeWorkingDirectoryToPath;
                flagsField.SetValue(knownMethod, flags);
            }
        }

        public static void DeleteIfExist(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public void SendEmailWithAttachment(string attachmentPath)
        {
            try
            {
                var client = SmtpConfig.CreateClient();

                using (var message = new MailMessage(FromEmailAddress, ToEmailAddress)
                {
                    Subject = EmailSubject,
                    Body = EmailBody
                })
                {
                    message.IsBodyHtml = true;
                    message.ReplyToList.Add(new MailAddress(FromEmailAddress));
                    message.Sender = new MailAddress(FromEmailAddress);
                    message.Headers.Add("List-Unsubscribe", string.Format("<mailto:{0}?subject=Unsubscribe>", "cs@" + message.From.Host));
                    message.Attachments.Add(new Attachment(attachmentPath));
                    client.Send(message);
                }

                Log.InfoFormat("Email sent to {0} with the file {1} attached to the email", ToEmailAddress, System.IO.Path.GetFileName(attachmentPath));
            }
            catch (Exception ex)
            {
                Log.FatalFormat("Error when emailing file {0} to {1} - {2}", System.IO.Path.GetFileName(attachmentPath), ToEmailAddress, ex.Message);
            }
        }

        public void CopyLatestReceivedFileToServer()
        {
            File.Copy(Path, "C:\\FileProcessorService\\" + FileName);
        }
    }
}
