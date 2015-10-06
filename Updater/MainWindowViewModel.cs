using System;
using System.ComponentModel;
using System.Configuration;
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
            var bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.DoWork += (sender, args) =>
                             {
                                 var settings = ConfigurationManager.GetSection("DbUpdater") as DbUpdaterConfigurationSection;
                                 var updater = new UpdateManager(settings, ApplicationSettings.Current.Logger);
                                 updater.UpdateProgress += (s, e) => bw.ReportProgress(0, e);
                                 updater.Update();
                             };

            bw.ProgressChanged += Update_ProgressChanged;
            bw.RunWorkerCompleted += (s, e) =>
                                         {
                                             IsUpdateInProgress = false;
                                             if (e.Result is Exception)
                                                 throw (Exception) e.Result;
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
