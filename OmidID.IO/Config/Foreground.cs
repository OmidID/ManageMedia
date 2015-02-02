using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace OmidID.IO.SaveMedia.Config {
    public class Foreground : BaseImageSetting {

        [ConfigurationProperty("path", DefaultValue = "", IsRequired = true)]
        public string Path { get { return (string)this["path"]; } set { this["path"] = value; } }

    }
}
