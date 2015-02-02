using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace OmidID.IO.SaveMedia.Config {
    public class ImageSetting : BaseImageSetting, IKeyElement {

        [ConfigurationProperty("name", DefaultValue = "Key", IsRequired = true, IsKey = true)]
        public string Name { get { return (string)this["name"]; } set { this["name"] = value; } }

        [ConfigurationProperty("saveAs", DefaultValue = "PNG", IsRequired = false)]
        public SaveType SaveAs { get { return (SaveType)this["saveAs"]; } set { this["saveAs"] = value; } }

        [ConfigurationProperty("background", DefaultValue = "", IsRequired = false)]
        public string Background { get { return (string)this["background"]; } set { this["background"] = value; } }

        [ConfigurationProperty("foreground")]
        public Foreground Foreground { get { return (Foreground)this["foreground"]; } set { this["foreground"] = value; } }

        [ConfigurationProperty("saveImmidately", DefaultValue = "true", IsRequired = false)]
        public bool SaveImmidately { get { return (bool)this["saveImmidately"]; } set { this["saveImmidately"] = value; } }

    }

    public enum SaveType {

        PNG = 1,
        JPEG = 2,
        BMP = 3,
        GIF = 4

    }
}
