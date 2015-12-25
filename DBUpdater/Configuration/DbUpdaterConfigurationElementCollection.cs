using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBUpdater.Configuration
{
    public class DbUpdaterConfigurationElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new DbUpdaterConfigurationSection();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((DbUpdaterConfigurationSection)element).GetHashCode();
        }
        public DbUpdaterConfigurationSection this[int index]
        {
            get { return (DbUpdaterConfigurationSection)BaseGet(index); }
        }

    }
}
