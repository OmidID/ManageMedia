using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Text.RegularExpressions;

namespace OmidID.IO.SaveMedia.Config {
    public class RouteUrl : BaseKeyElement {
        
        [ConfigurationProperty("match", DefaultValue = "^/Upload/(.*)/(.*)", IsRequired = false)]
        public string Match { get { return (string)this["match"]; } set { this["match"] = value; } }

        [ConfigurationProperty("item", DefaultValue = "", IsRequired = false)]
        public string Item { get { return (string)this["item"]; } set { this["item"] = value; } }

        [ConfigurationProperty("imageSetting", DefaultValue = "$1", IsRequired = false)]
        public string ImageSetting { get { return (string)this["imageSetting"]; } set { this["imageSetting"] = value; } }

        [ConfigurationProperty("filename", DefaultValue = "$2", IsRequired = false)]
        public string Filename { get { return (string)this["filename"]; } set { this["filename"] = value; } }

        [ConfigurationProperty("checkDomain", DefaultValue = "false", IsRequired = false)]
        public bool CheckDomain { get { return (bool)this["checkDomain"]; } set { this["checkDomain"] = value; } }

        [ConfigurationProperty("generateFile", DefaultValue = "false", IsRequired = false)]
        public bool GenerateFile { get { return (bool)this["generateFile"]; } set { this["generateFile"] = value; } }

        Regex regex;
        public Regex Regex {
            get {
                if (regex == null)
                    regex = new Regex(Match, RegexOptions.IgnoreCase);

                return regex;
            }
        }

    }
}
