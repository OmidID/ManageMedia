using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

namespace OmidID.IO.SaveMedia.Handler {
    public class JsonUploadInfo : IHttpHandler {
        #region IHttpHandler Members

        public bool IsReusable {
            get { return true; }
        }

        public void ProcessRequest(HttpContext c) {
            c.Response.ContentType = "application/json";
            var id = c.Request["id"];
            var upload = Tools.GetUpdateInfo(id);
            if (upload == null) {
                c.Response.Write("false");
                return;
            }
            if ((byte)upload.Status > 2) Tools.RemoveUpdateInfo(id);

            var outstr = Tools.FileSize(upload.UploadedLength);
            if (upload.Status == Models.UploadState.Finished) {
                //
            }

            var percent = Convert.ToInt32((Convert.ToInt64(upload.UploadedLength) * 100) / Convert.ToInt64(upload.ContentLength == 0 ? 1 : upload.ContentLength));

            var js = new JavaScriptSerializer();
            c.Response.Write(js.Serialize(new {
                Speed = (upload.Speed / 1024d).ToString("###,##0.00"),
                Total = outstr,
                Status = (byte)upload.Status,
                StatusText = upload.Status.ToString(),
                Remaining = upload.Remaining.ToString(),
                FileLength = upload.ContentLength,
                FileName = upload.FileName,
                Percent = percent,
                UploadID = upload.UploadID,
                upload.ValidateType,
                FileNameNoExt = System.IO.Path.GetFileNameWithoutExtension(upload.FileName)
            }));

        }

        #endregion
    }
}
