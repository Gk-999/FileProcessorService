using System;
using System.Configuration;
using System.IO;
using System.Web;
using System.Web.Caching;
using Gk.Core;
using log4net;

namespace Gk.Host.TraditionalService
{
    public class Worker
    {
        private Cache Cache { get; set; }
        private FileSystemWatcher Watcher { get; set; }
        private FileSystemWatcher SftpWatcher { get; set; }
        public ILog Log { get; set; }
        public FileOperations FileOperations { get; set; }
        public DirectoryMaster DirectoryMaster { get; set; }
        public Worker(ILog log,FileOperations fileOperations, DirectoryMaster directoryMaster)
        {
            Cache = HttpRuntime.Cache;
            Log = log;
            FileOperations = fileOperations;
            DirectoryMaster = directoryMaster;
        }

        public FileSystemWatcher CreateWatcher()
        {
            var targetDir = DirectoryMaster.TargetDirectoryA;
            var watcher = new FileSystemWatcher(targetDir, "*.csv")
            {
                NotifyFilter = NotifyFilters.FileName,
                IncludeSubdirectories = true
            };

            watcher.Created += OnFileAdded;
            return watcher;
        }

        public FileSystemWatcher CreateWatcherForSftp()
        {
            var targetDir = DirectoryMaster.TargetDirectoryB;
            var watcher = new FileSystemWatcher(targetDir, "*.csv")
            {
                NotifyFilter = NotifyFilters.FileName,
                IncludeSubdirectories = true
            };

            watcher.Created += OnFileAdded;
            watcher.Error += new ErrorEventHandler(OnError);
            return watcher;
        }

        protected void OnFileAdded(object source, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Created)
            {
                return;
            }

            //string cacheKeyPrefix = FileOperations.GetCachePrefixForPath(e.FullPath);

            //if (!string.IsNullOrWhiteSpace(cacheKeyPrefix))
            //{
                Cache.Add(Guid.NewGuid().ToString(), e.FullPath, null, DateTime.Now.AddSeconds(10), Cache.NoSlidingExpiration, CacheItemPriority.Normal, OnCacheItemRemoved);
                Log.InfoFormat("Cached file {0} {1}", Path.GetFileName(e.FullPath), Environment.NewLine);
            //}
        }

        private void OnCacheItemRemoved(string key, object value, CacheItemRemovedReason reason)
        {
            if (reason != CacheItemRemovedReason.Expired)
            {
                return;
            }
            FileOperations.PerformOperationForKey(key,(string)value);
        }

        private void OnError(object source, ErrorEventArgs e)
        {
            if (e.GetException().GetType() == typeof(InternalBufferOverflowException))
            {
                Log.WarnFormat("SFTP File System Watcher internal buffer overflow at '{0}'", DateTime.Now);
            }
            else
            {
                //Log.WarnFormat("SFTP Watcher directory not accessible at '{0}'. Trying to restart watcher...", DateTime.Now);
            }
            NotAccessibleError(SftpWatcher, e);
        }

        void NotAccessibleError(FileSystemWatcher source, ErrorEventArgs e)
        {
            source.EnableRaisingEvents = false;
            int iMaxAttempts = 120;
            int iTimeOut = 30000;
            int i = 0;
            while (source.EnableRaisingEvents == false && i < iMaxAttempts)
            {
                i += 1;
                try
                {
                    source.Dispose();
                    source = null;
                    source = new System.IO.FileSystemWatcher();
                    ((System.ComponentModel.ISupportInitialize)(source)).BeginInit();
                    source.EnableRaisingEvents = true;
                    source.Filter = "*.csv";
                    source.Path = DirectoryMaster.TargetDirectoryB;
                    source.NotifyFilter = System.IO.NotifyFilters.FileName;
                    source.IncludeSubdirectories = true;
                    source.Created += new System.IO.FileSystemEventHandler(OnFileAdded);
                    source.Error += new ErrorEventHandler(OnError);
                    ((System.ComponentModel.ISupportInitialize)(source)).EndInit();
                    //Log.WarnFormat("SFTP Watcher restarted successfully at '{0}'", DateTime.Now);
                }
                catch
                {
                    source.EnableRaisingEvents = false;
                    System.Threading.Thread.Sleep(iTimeOut);
                }
            }

        }

        public void Start()
        {
            Log.InfoFormat("{0} Starting Worker Start method with 'DaysBefore' value of {1} {2}", DateTime.Now, ConfigurationManager.AppSettings["DaysBefore"], Environment.NewLine);
            var targetDir = DirectoryMaster.TargetDirectoryA;
            if (!Directory.Exists(targetDir))
            {
                Log.ErrorFormat("Please make sure the target directory '{0}' exists, then restart the service. {1}", targetDir, Environment.NewLine);
                return;
            }
            Watcher = CreateWatcher();
            Watcher.EnableRaisingEvents = true;

            var targetDirSftp = DirectoryMaster.TargetDirectoryB;
            if (!Directory.Exists(targetDirSftp))
            {
                Log.ErrorFormat("Please make sure the target directory '{0}' exists, then restart the service. {1}", targetDirSftp, Environment.NewLine);
                return;
            }
            SftpWatcher = CreateWatcherForSftp();
            SftpWatcher.EnableRaisingEvents = true;
        }

        public void Stop()
        {
            Log.InfoFormat("Stopping worker {0}", Environment.NewLine);
            Watcher.EnableRaisingEvents = false;
            SftpWatcher.EnableRaisingEvents = false;
        }
    }
}
