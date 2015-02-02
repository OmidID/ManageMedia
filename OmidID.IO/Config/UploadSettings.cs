using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace OmidID.IO.SaveMedia.Config {
    public class UploadSettings : ConfigurationSection {

        SaveSettingCollection items = new SaveSettingCollection();

        [ConfigurationProperty("default", IsRequired = false, DefaultValue = "")]
        public string Default { get { return (string)this["default"]; } set { this["default"] = value; } }

        [ConfigurationCollection(typeof(SaveSettingCollection),
                                 AddItemName = "add",
                                 ClearItemsName = "clear",
                                 RemoveItemName = "remove")]
        [ConfigurationProperty("items", IsDefaultCollection = false)]
        public SaveSettingCollection Items {
            get {
                SaveSettingCollection saveSettingCollection =
                (SaveSettingCollection)base["items"];
                return saveSettingCollection;
            }
        }

        [ConfigurationCollection(typeof(RouteUrlCollection),
                                 AddItemName = "add",
                                 ClearItemsName = "clear",
                                 RemoveItemName = "remove")]
        [ConfigurationProperty("modules", IsDefaultCollection = false)]
        public RouteUrlCollection Modules {
            get {
                RouteUrlCollection routeUrlCollection =
                (RouteUrlCollection)base["modules"];
                return routeUrlCollection;
            }
        }

        [ConfigurationCollection(typeof(RouteUrlCollection),
                                 AddItemName = "add",
                                 ClearItemsName = "clear",
                                 RemoveItemName = "remove")]
        [ConfigurationProperty("handlers", IsDefaultCollection = false)]
        public RouteUrlCollection Handlers {
            get {
                RouteUrlCollection routeUrlCollection =
                (RouteUrlCollection)base["handlers"];
                return routeUrlCollection;
            }
        }

        public static UploadSettings GetSettings() {
            return ConfigurationManager.GetSection("upload") as UploadSettings;
        }

        public static UploadSettings GetSettings(string sectionName) {
            return ConfigurationManager.GetSection(string.IsNullOrEmpty(sectionName) ? "upload" : sectionName) as UploadSettings;
        }
    }
}
