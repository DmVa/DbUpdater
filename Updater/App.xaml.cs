using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
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
