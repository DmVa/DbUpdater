using System;
using System.ComponentModel;
using System.Configuration;
using System.Threading;
using System.Windows;
using System.Windows.Input;

using DBUpdater;
using DBUpdater.Configuration;

namespace Updater
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private string _displayText;
        private bool _isUpdateInProgress;
        private BaseCommand _runUpdateCommand;
        private BaseCommand _closeCommand;

        public MainWindowViewModel ()
        {
            _runUpdateCommand = new BaseCommand(DoUpdate, CanDoUpdate);
            _closeCommand = new BaseCommand(DoClose, CanDoClose);
        }

        public ICommand CloseCommand
        {
            get { return _closeCommand; }
        }

        public ICommand UpdateCommand
        {
            get { return _runUpdateCommand; }
        }

        public void DoClose()
        {
            Application.Current.MainWindow.Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string DisplayText
        {
            get { return _displayText; }
            set
            {
                _displayText = value;
                RaisePropertyChanged("DisplayText");
            }
        }

        public void DoUpdate()
        {
            var settings = ConfigurationManager.GetSection("DbUpdater") as DbUpdaterConfigurationSection;
            var multipleSettings = ConfigurationManager.GetSection("DbUpdaterMultipleSource") as DbUpdaterMultipleSourceConfigurationSection;
            var logger = ApplicationSettings.Current.Logger;
            if (settings != null)
            {
                if (multipleSettings != null && string.IsNullOrEmpty(multipleSettings.ConnectionString))
                {
                    multipleSettings.ConnectionString = settings.ConnectionString;
                }
                RunSingleUpdate(settings, logger, multipleSettings, 0);
            }
            else
            {
                RunSingleUpdateFromMultipleInstance(multipleSettings, logger, 0);
            }
        }


        private void RunSingleUpdateFromMultipleInstance(DbUpdaterMultipleSourceConfigurationSection settings, LogWrapper.ILogger logger, int configurationIndex)
        {
            if (settings == null || settings.DbUpdaterConfigurations == null)
            {
                OnFinishUpdateProcess(false);
                return;
            }

            if (configurationIndex >= settings.DbUpdaterConfigurations.Count)
            {
                OnFinishUpdateProcess(false);
                return;
            }

            DbUpdaterConfigurationSection currentSettings = settings.DbUpdaterConfigurations[configurationIndex];
            if (string.IsNullOrEmpty(currentSettings.ConnectionString))
            {
                currentSettings.ConnectionString = settings.ConnectionString;
            }

            RunSingleUpdate(currentSettings, logger, settings, configurationIndex + 1);
        }

        private void  RunSingleUpdate(DbUpdaterConfigurationSection settings, LogWrapper.ILogger logger, DbUpdaterMultipleSourceConfigurationSection multipleSettings, int configurationIndex)
        {
            var bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.DoWork += (sender, args) =>
            {
                var updater = new UpdateManager(settings, logger);
                updater.UpdateProgress += (s, e) => bw.ReportProgress(0, e);
                updater.Update();
            };

            bw.ProgressChanged += Update_ProgressChanged;
            bw.RunWorkerCompleted += (s, e) =>
            {
                IsUpdateInProgress = false;
                if (e.Error != null)
                {
                    DisplayText += "Error: " + e.Error.Message +Environment.NewLine;
                    OnFinishUpdateProcess(true);
                    return;
                }
                if (e.Cancelled)
                {
                    DisplayText += "Cancelled" + Environment.NewLine;
                    OnFinishUpdateProcess(true);
                    return;
                }

                RunSingleUpdateFromMultipleInstance(multipleSettings, logger, configurationIndex);
            };

            IsUpdateInProgress = true;
            bw.RunWorkerAsync();
        }

        private void OnFinishUpdateProcess(bool isError)
        {
            if (isError || !ApplicationSettings.Current.AutoClose)
                return;

            // ugly hack for wait some time before close.
            var bw = new BackgroundWorker();
            bw.WorkerReportsProgress = false;
            bw.DoWork += (sender, args) =>
            {
                Thread.Sleep(1500);   
            };

            bw.ProgressChanged += (s,e) => {};
            bw.RunWorkerCompleted += (s, e) =>
            {
                IsUpdateInProgress = false;
                Application.Current.Shutdown();
            };

            IsUpdateInProgress = true;
            bw.RunWorkerAsync();

           
        }

        protected bool IsUpdateInProgress
        {
            get { return _isUpdateInProgress; }
            set
            {
                _isUpdateInProgress = value;
                _runUpdateCommand.RaiseCanExecuteChanged(null);
                _closeCommand.RaiseCanExecuteChanged(null);
            }
        }

        private void Update_ProgressChanged(object sender, ProgressChangedEventArgs progressArgs)
        {
            UpdateProgressEventArgs e = (UpdateProgressEventArgs)progressArgs.UserState;
            DisplayText += e.Message + Environment.NewLine;
        }

        private void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private bool CanDoUpdate()
        {
            return !_isUpdateInProgress;
        }

        private bool CanDoClose()
        {
            return !_isUpdateInProgress;
        }
    }
}
