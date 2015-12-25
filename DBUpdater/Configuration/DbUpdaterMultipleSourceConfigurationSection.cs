using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBUpdater.Configuration
{
    public class DbUpdaterMultipleSourceConfigurationSection : ConfigurationSection
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

        [ConfigurationProperty("Sources", IsRequired = false, IsDefaultCollection=false)]
        [ConfigurationCollection(typeof(DbUpdaterConfigurationElementCollection), AddItemName = "DbUpdater")]
        public DbUpdaterConfigurationElementCollection DbUpdaterConfigurations
        {
            get { return (DbUpdaterConfigurationElementCollection)this["Sources"]; }
        }
    }
}
