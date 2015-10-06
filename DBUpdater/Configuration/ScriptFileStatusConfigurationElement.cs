using System;
using System.Configuration;

namespace DBUpdater.Configuration
{
    public class ScriptFileStatusConfigurationElement: ConfigurationElement
    {
        [ConfigurationProperty("OkValue", DefaultValue = "", IsRequired = true)]
        public String OkValue
        {
            get { return (String)this["OkValue"]; }
            set { this["OkValue"] = value; }
        }

        [ConfigurationProperty("ErrorValue", DefaultValue = "", IsRequired = false)]
        public String ErrorValue
        {
            get { return (String)this["ErrorValue"]; }
            set { this["ErrorValue"] = value; }
        }

        [ConfigurationProperty("InProgressValue", DefaultValue = "", IsRequired = false)]
        public String InProgressValue
        {
            get { return (String)this["InProgressValue"]; }
            set { this["InProgressValue"] = value; }
        }
    }
}
