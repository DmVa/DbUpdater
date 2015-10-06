using System;
using System.Configuration;

namespace DBUpdater.Configuration
{
    public class ScriptsFileConfigurationElement: ConfigurationElement
    {
        [ConfigurationProperty("FileName", DefaultValue = "", IsRequired = true)]
        public String FileName
        {
            get { return (String)this["FileName"]; }
            set { this["FileName"] = value; }
        }

        [ConfigurationProperty("SchemaFile", DefaultValue = "", IsRequired = false)]
        public String SchemaFile
        {
            get { return (String)this["SchemaFile"]; }
            set { this["SchemaFile"] = value; }
        }

        [ConfigurationProperty("ScriptsRootFolder", DefaultValue = "", IsRequired = false)]
        public String ScriptsRootFolder
        {
            get { return (String)this["ScriptsRootFolder"]; }
            set { this["ScriptsRootFolder"] = value; }
        }
    }
}
