using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using DBUpdater.Configuration;
using log4net;
using LogWrapper;

namespace Updater
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            ApplicationSettings.Current.Logger = new Log4NetWrapper(LogManager.GetLogger(this.GetType()));
            ApplicationSettings.Current.AutoClose = Updater.Properties.Settings.Default.CloseAfterUpdate;
            ApplicationSettings.Current.CommandLineParams = CreateCommandLineArguments(e.Args);
        }

        /// <summary>
        /// commandline should look like : /closeonerror=true /connectionstring="aaa";
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private CommandLineParams CreateCommandLineArguments(string[] args)
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

        private CommandLineArgument CreateCommandLineArgument(string param)
        {
            if (string.IsNullOrEmpty(param))
                return null;
            param = param.TrimStart('/');
            string[] pr = param.Split('=');
            if (pr.Length < 1)
                return null;
            CommandLineArgument result = new CommandLineArgument();
            result.Key = pr[0];
            if (pr.Length > 1)
                result.Value = pr[1];
            return result;

        }

        private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            string message = e.Exception.InnerException != null ? e.Exception.InnerException.Message : e.Exception.Message;

            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            ApplicationSettings.Current.Logger.LogError("unhandled", e.Exception);
            if (ApplicationSettings.Current.AutoClose)
            {
                e.Handled = true;
            }
        }
    }
}
