using LogWrapper;

namespace Updater
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
        
        private ApplicationSettings(){}
        
    }
}
