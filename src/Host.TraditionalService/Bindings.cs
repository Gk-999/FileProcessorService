using System;
using System.Configuration;
using System.Net.Mail;
using System.Reflection;
using Gk.Core.SFTP;
using Gk.Core.SMTP;
using Gk.Core;
using Gk.Core.Files;
using Gk.Core.Repository;
using Gk.Core.Utilities;
using log4net;
using Ninject.Modules;

namespace Gk.Host.TraditionalService
{
    public class Bindings :NinjectModule
    {
        public override void Load()
        {
            Bind<DirectoryMaster>()
                .ToSelf()
                .WithConstructorArgument("targetDirectoryA", GetSetting("TargetDirectoryA"))
                .WithConstructorArgument("targetDirectoryB", GetSetting("TargetDirectoryB"));

            Bind<ILog>().ToMethod(c => c.Request.Target == null ? LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType) : GetLogger(c.Request.Target.Member.ReflectedType));   //c.Request.Target is null initially when invoked from Main() method

            Bind<CSVOperations>().ToSelf();

            Bind<FileOperations>().ToSelf();

            Bind<Worker>().ToSelf();

            Bind<Program.Service>().ToSelf();

            Bind<ABSFiles>().ToSelf();

            Bind(typeof(IRepository<>)).To(typeof(Repository<>));
        }

        private static string GetSetting(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        private static ILog GetLogger(Type type)
        {
            return LogManager.GetLogger(type.GetReadableName());
        }
    }
}
