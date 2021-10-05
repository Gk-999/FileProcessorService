using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Web.Caching;
using System.ServiceProcess;
using Gk.Core;
using log4net;
using Ninject;

namespace Gk.Host.TraditionalService
{
    public class Program
    {
        #region Nested classes to support running as service
        public const string ServiceName = "FileProcessorService";
        private Cache Cache { get; set; }
        private FileSystemWatcher Watcher { get; set; }
        
        public class Service : ServiceBase
        {
            public Worker Worker { get; set; }
            public Service(Worker worker)
            {
                ServiceName = Program.ServiceName;
                Worker = worker;
            }

            protected override void OnStart(string[] args)
            {
                Worker.Start();
            }

            protected override void OnStop()
            {
                Worker.Stop();
            }
        }
        #endregion
        static void Main(string[] args)
        {
            InitialiseLogging();
            var kernel = new StandardKernel(new Bindings());
            var log = kernel.Get<ILog>();
            
            if (!Environment.UserInteractive)
                // running as service
                using (var service = kernel.Get<Service>())
                    ServiceBase.Run(service);
            else
            {
                // running as console app
                var w = kernel.Get<Worker>();
                var dm = kernel.Get<DirectoryMaster>();
                w.Start();

                log.InfoFormat("Monitoring for specific .csv files being added to specific sub-directories of {0} & {1} with 'DaysBefore' value of {2}", dm.TargetDirectoryA, dm.TargetDirectoryB, ConfigurationManager.AppSettings["DaysBefore"]);
                Console.ReadKey(true);

                w.Stop();
            }
        }

        private static void InitialiseLogging()
        {
            var loggingConfig = new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log4net.config"));

            log4net.Config.XmlConfigurator.ConfigureAndWatch(loggingConfig);
        }
    }
}
