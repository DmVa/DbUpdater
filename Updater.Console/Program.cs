using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBUpdater;
using DBUpdater.Configuration;
using log4net;
using LogWrapper;

namespace Updater.Console
{
    class Program
    {
        static int Main(string[] args)
        {
            ApplicationSettings.Current.Logger = new Log4NetWrapper(LogManager.GetLogger(typeof(Program)));
            ApplicationSettings.Current.AutoClose = true;
            ApplicationSettings.Current.CommandLineParams = CreateCommandLineArguments(args);

            var settings = ConfigurationManager.GetSection("DbUpdater") as DbUpdaterConfigurationSection;
            var logger = ApplicationSettings.Current.Logger;
            var commandLineParams = ApplicationSettings.Current.CommandLineParams;

            var updater = new UpdateManager(settings, logger, commandLineParams);
            updater.UpdateProgress += (s, e) =>
            {
                Update_ProgressChanged(0, new ProgressChangedEventArgs(0, e));
            };

            try
            {
                updater.Update();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"{ex.Message}.{ex.InnerException?.Message ?? ""}");

                return 1;
            }

            return 0;
        }

        /// <summary>
        /// commandline should look like : /closeonerror=true /connectionstring="aaa";
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static CommandLineParams CreateCommandLineArguments(string[] args)
        {
            var result = new CommandLineParams();
            foreach (var param in args)
            {
                CommandLineArgument arg = CreateCommandLineArgument(param);
                if (arg == null)
                    continue;
                if (arg.Key.ToLower() == "RunFromConsole".ToLower())
                {
                    result.RunFromConsole = Convert.ToBoolean(arg.Value);
                }

                if (arg.Key.ToLower() == "ConnectionString".ToLower())
                {
                    result.ConnectionString = arg.Value;
                }
            }
            return result;
        }

        private static CommandLineArgument CreateCommandLineArgument(string param)
        {
            if (string.IsNullOrEmpty(param))
                return null;
            param = param.TrimStart('/');
            string[] pr = param.Split(new[] {'='}, 2);
            if (pr.Length < 1)
                return null;
            CommandLineArgument result = new CommandLineArgument();
            result.Key = pr[0];
            if (pr.Length > 1)
                result.Value = pr[1];
            return result;

        }

        private static void Update_ProgressChanged(object sender, ProgressChangedEventArgs progressArgs)
        {
            UpdateProgressEventArgs e = (UpdateProgressEventArgs)progressArgs.UserState;
            CommandLineParams commandLineParams = ApplicationSettings.Current.CommandLineParams;
            System.Console.WriteLine(e.Message);
        }
    }
}
