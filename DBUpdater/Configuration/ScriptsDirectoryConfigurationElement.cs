using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBUpdater.Configuration
{
    public class ScriptsDirectoryConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty("DirectoryName", DefaultValue = "", IsRequired = true)]
        public String DirectoryName
        {
            get { return (String)this["DirectoryName"]; }
            set { this["DirectoryName"] = value; }
        }

        [ConfigurationProperty("FileMask", DefaultValue = "*.sql", IsRequired = false)]
        public String FileMask
        {
            get { return (String)this["FileMask"]; }
            set { this["FileMask"] = value; }
        }

        [ConfigurationProperty("DoNotRegisterScriptInDatabase", DefaultValue = false, IsRequired = false)]
        public bool DoNotRegisterScriptInDatabase
        {
            get { return (bool)this["DoNotRegisterScriptInDatabase"]; }
            set { this["DoNotRegisterScriptInDatabase"] = value; }
        }

        [ConfigurationProperty("DoNotStoreFileExtensionInDatabase", DefaultValue = false, IsRequired = false)]
        public bool DoNotStoreFileExtensionInDatabase
        {
            get { return (bool)this["DoNotStoreFileExtensionInDatabase"]; }
            set { this["DoNotStoreFileExtensionInDatabase"] = value; }
        }
    }
}
