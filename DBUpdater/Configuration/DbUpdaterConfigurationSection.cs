using System;
using System.Configuration;

namespace DBUpdater.Configuration
{
    public class DbUpdaterConfigurationSection : ConfigurationSection
    {
        public override bool IsReadOnly()
        {
            return false;
        }

        [ConfigurationProperty("ConnectionString", DefaultValue = "", IsRequired = false)]
        public String ConnectionString
        {
            get { return (String)this["ConnectionString"]; }
            set { this["ConnectionString"] = value; }
        }

        [ConfigurationProperty("UpdateFromXMLEvenIfXMLEmpty", DefaultValue = false,  IsRequired = false)]
        public bool UpdateFromXMLEvenIfXMLEmpty
        {
            get { return (bool)this["UpdateFromXMLEvenIfXMLEmpty"]; }
            set { this["UpdateFromXMLEvenIfXMLEmpty"] = value; }
        }

        [ConfigurationProperty("ScriptSource", DefaultValue = ScriptSource.XmlFile, IsRequired = true)]
        public ScriptSource ScriptSource
        {
            get { return (ScriptSource)this["ScriptSource"]; }
            set { this["ScriptSource"] = value; }
        }

        [ConfigurationProperty("ScriptsFiles", IsRequired = false)]
        public ScriptsFileConfigurationElement ScriptsFiles
        {
            get { return (ScriptsFileConfigurationElement)this["ScriptsFiles"]; }
            set { this["ScriptsFiles"] = value; }
        }

        [ConfigurationProperty("ScriptDirectory", IsRequired = false)]
        public ScriptsDirectoryConfigurationElement ScriptDirectory
        {
            get { return (ScriptsDirectoryConfigurationElement)this["ScriptDirectory"]; }
            set { this["ScriptDirectory"] = value; }
        }

        [ConfigurationProperty("ScriptsDbTable", IsRequired = true)]
        public ScriptsDbTableConfigurationElement ScriptsDbTable
        {
            get { return (ScriptsDbTableConfigurationElement)this["ScriptsDbTable"]; }
            set { this["ScriptsDbTable"] = value; }
        }
    }
}
