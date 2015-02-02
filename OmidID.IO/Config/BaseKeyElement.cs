using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace OmidID.IO.SaveMedia.Config {
    public class BaseKeyElement : ConfigurationElement, IKeyElement {

        [ConfigurationProperty("name", DefaultValue = "Key", IsRequired = true, IsKey = true)]
        public string Name { get { return (string)this["name"]; } set { this["name"] = value; } }


    }
}
