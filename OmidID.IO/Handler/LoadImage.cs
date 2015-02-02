using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace OmidID.IO.SaveMedia.Handler {
    public class LoadImage : IHttpHandler {

        public bool IsReusable {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context) {
            var Settings = Config.UploadSettings.GetSettings();
            foreach (var item in Settings.Modules) {
                var url = item.CheckDomain ? context.Request.Url.ToString() : context.Request.RawUrl;
                if (item.Regex.IsMatch(url)) {

                    var itemName = item.Regex.Replace(url, item.Item);
                    var settingName = item.Regex.Replace(url, item.ImageSetting);
                    var filename = item.Regex.Replace(url, item.Filename);

                    Tools.CheckFile(Settings.SectionInformation.Name, itemName, settingName, filename, context.Response.OutputStream);
                }
            }
        }
    }
}
