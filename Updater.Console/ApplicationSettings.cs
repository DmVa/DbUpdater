using DBUpdater.Configuration;
using LogWrapper;

namespace Updater.Console
{
    public class ApplicationSettings
    {
        private static ApplicationSettings _instance = new ApplicationSettings();
        public ILogger Logger { get; set; }
        public bool AutoClose { get; set; }

        public static ApplicationSettings Current
        {
            get { return _instance; }
        }

        public CommandLineParams CommandLineParams { get; internal set; }

        private ApplicationSettings(){}
        
    }
}
