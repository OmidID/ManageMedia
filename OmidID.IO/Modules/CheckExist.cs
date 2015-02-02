using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.IO;
using System.Drawing;

namespace OmidID.IO.SaveMedia.Modules {
    public class CheckExist : IHttpModule {
        HttpApplication app;

        public void Dispose() {
        }

        public void Init(HttpApplication context) {
            app = context;
            app.BeginRequest += new EventHandler(app_PreRequestHandlerExecute);
        }

        public void app_PreRequestHandlerExecute(object sender, EventArgs e) {
            var Settings = Config.UploadSettings.GetSettings();
            foreach (var item in Settings.Modules) {
                var url = item.CheckDomain ? app.Request.Url.ToString() : app.Request.AppRelativeCurrentExecutionFilePath;
                if (item.Regex.IsMatch(url)) {

                    var itemName = item.Regex.Replace(url, item.Item);
                    var settingName = item.Regex.Replace(url, item.ImageSetting);
                    var filename = item.Regex.Replace(url, item.Filename);

                    Tools.CheckFile(Settings.SectionInformation.Name, itemName, settingName, filename, null);
                }
            }
        }        

    }
}
