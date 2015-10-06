using System;
using System.Configuration;

namespace DBUpdater.Configuration
{
    public class XmlDatafigurationElement : ConfigurationElement
    {
        [ConfigurationProperty("FileField", DefaultValue = "", IsRequired = true)]
        public String FileField
        {
            get { return (String)this["FileField"]; }
            set { this["FileField"] = value; }
        }

        [ConfigurationProperty("DescriptionField", DefaultValue = "", IsRequired = false)]
        public String DescriptionField
        {
            get { return (String)this["DescriptionField"]; }
            set { this["DescriptionField"] = value; }
        }

        [ConfigurationProperty("AuthorField", DefaultValue = "", IsRequired = false)]
        public String AuthorField
        {
            get { return (String)this["AuthorField"]; }
            set { this["AuthorField"] = value; }
        }
    }
}
