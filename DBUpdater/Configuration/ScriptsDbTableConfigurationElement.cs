using System;
using System.Configuration;

namespace DBUpdater.Configuration
{
    public class ScriptsDbTableConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty("TableName", DefaultValue = "", IsRequired = true)]
        public String TableName
        {
            get { return (String)this["TableName"]; }
            set { this["TableName"] = value; }
        }

        [ConfigurationProperty("RunDateField", DefaultValue = "", IsRequired = false)]
        public String RunDateField
        {
            get { return (String)this["RunDateField"]; }
            set { this["RunDateField"] = value; }
        }

        [ConfigurationProperty("UpdateStatusField", DefaultValue = "", IsRequired = false)]
        public String UpdateStatusField
        {
            get { return (String)this["UpdateStatusField"]; }
            set { this["UpdateStatusField"] = value; }
        }

        [ConfigurationProperty("ScriptFileStatus", IsRequired = false, DefaultValue = null)]
        public ScriptFileStatusConfigurationElement ScriptFileStatus
        {
            get { return (ScriptFileStatusConfigurationElement)this["ScriptFileStatus"]; }
            set { this["ScriptFileStatus"] = value; }
        }

        [ConfigurationProperty("XmlMappedData", IsRequired = true)]
        public XmlDatafigurationElement XmlMappedData
        {
            get { return (XmlDatafigurationElement)this["XmlMappedData"]; }
            set { this["XmlMappedData"] = value; }
        }
    }
}
