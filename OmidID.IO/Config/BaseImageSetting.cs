using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using OmidID.Drawing;

namespace OmidID.IO.SaveMedia.Config {
    public class BaseImageSetting : ConfigurationElement {

        [ConfigurationProperty("width", DefaultValue = "800", IsRequired = true)]
        [IntegerValidator(MinValue = 10)]
        public int Width { get { return (int)this["width"]; } set { this["width"] = value; } }

        [ConfigurationProperty("height", DefaultValue = "600", IsRequired = true)]
        [IntegerValidator(MinValue = 10)]
        public int Height { get { return (int)this["height"]; } set { this["height"] = value; } }

        [ConfigurationProperty("x", DefaultValue = "0", IsRequired = false)]
        public int X { get { return (int)this["x"]; } set { this["x"] = value; } }

        [ConfigurationProperty("y", DefaultValue = "0", IsRequired = false)]
        public int Y { get { return (int)this["y"]; } set { this["y"] = value; } }

        [ConfigurationProperty("zoom", DefaultValue = "Zoom", IsRequired = false)]
        public ZoomType Zoom { get { return (ZoomType)this["zoom"]; } set { this["zoom"] = value; } }

        [ConfigurationProperty("alpha", DefaultValue = "1", IsRequired = false)]
        public float Alpha { get { return (float)this["alpha"]; } set { this["alpha"] = value; } }

        [ConfigurationProperty("resolutionX", DefaultValue = "96", IsRequired = false)]
        [IntegerValidator(MinValue = 10)]
        public int ResolutionX { get { return (int)this["resolutionX"]; } set { this["resolutionX"] = value; } }

        [ConfigurationProperty("resolutionY", DefaultValue = "96", IsRequired = false)]
        [IntegerValidator(MinValue = 10)]
        public int ResolutionY { get { return (int)this["resolutionY"]; } set { this["resolutionY"] = value; } }

        [ConfigurationProperty("changeResolution", DefaultValue = "false", IsRequired = false)]
        public bool ChangeResolution { get { return (bool)this["changeResolution"]; } set { this["changeResolution"] = value; } }

    }
}
