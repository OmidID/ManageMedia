using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Reflection;
using System.Globalization;
using System.Threading;
using System.Collections.Specialized;

namespace OmidID.IO.SaveMedia.Handler {
    public class TurboUpload : IHttpAsyncHandler {

        internal static bool EqualsIgnoreCase(string s1, string s2) {
            if (string.IsNullOrEmpty(s1) && string.IsNullOrEmpty(s2)) {
                return true;
            }
            if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2)) {
                return false;
            }
            if (s2.Length != s1.Length) {
                return false;
            }
            return (0 == string.Compare(s1, 0, s2, 0, s2.Length, StringComparison.OrdinalIgnoreCase));
        }

        internal static string ExtractValueFromContentDispositionHeader(string l, int pos, string name) {
            string str = name + "=\"";
            int startIndex = CultureInfo.InvariantCulture.CompareInfo.IndexOf(l, str, pos, CompareOptions.IgnoreCase);
            if (startIndex < 0) {
                return null;
            }
            startIndex += str.Length;
            int index = l.IndexOf('"', startIndex);
            if (index < 0) {
                return null;
            }
            if (index == startIndex) {
                return string.Empty;
            }
            return l.Substring(startIndex, index - startIndex);
        }

        public HttpWorkerRequest Worker { get; set; }
        public NameValueCollection QueryString { get; private set; }
        public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData) {
            var type = typeof(HttpRequest);
            //Hack the .net here:
            var field = type.GetField("_wr", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            var wr = field.GetValue(System.Web.HttpContext.Current.Request) as HttpWorkerRequest; field.SetValue(System.Web.HttpContext.Current.Request, null);
            var wrType = wr.GetType(); Worker = wr;
            var UpdateRequestCounters = wrType.GetMethod("UpdateRequestCounters", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            Action<int> UpdateRequest = len => { UpdateRequestCounters.Invoke(wr, new object[] { len }); };
            var requestContentType = wr.GetKnownRequestHeader(HttpWorkerRequest.HeaderContentType);

            //Send some data to IIS for validating the request.
            wr.SendCalculatedContentLength(0);
            wr.SendStatus(200, "Ok");
            //wr.FlushResponse(false);

            //IIS6 Required... (Some of the data is here in IIS6)
            byte[] preloadedEntityBody = wr.GetPreloadedEntityBody();
            if (preloadedEntityBody != null) {
                UpdateRequest(preloadedEntityBody.Length);
            }

            //Receive query string and parse it to NameValueCollection
            var query = wr.GetQueryString();
            var hash = HttpUtility.ParseQueryString(query);
            var id = hash["id"];
            QueryString = hash;

            //Add the upload progress info in the collection
            var upload = Tools.NewUpdateInfo(id);
            upload.ContentLength = wr.GetTotalEntityBodyLength();
            upload.UploadedLength = 0;
            upload.Status = Models.UploadState.Init;

            int bufferSize = 1024;
            int total = 0;
            var lastRecOn = DateTime.Now;
            byte[] buffer = new byte[bufferSize];
            string fileName, fieldName, contentType;
            fileName = fieldName = contentType = default(string);

            //IIS response will manage with the InternalStream
            using (var stream = new InternalStream(wr, wr.GetTotalEntityBodyLength(), preloadedEntityBody, UpdateRequest)) {
                //Maybe we need to stop the job here,
                //Some time the validation is not correct on this section
                Action<bool> StopWork = (error) => {
                    wr.FlushResponse(true);
                    wr.EndOfRequest();

                    stream.Close();
                    stream.Dispose();

                    if (error) {
                        wr.SendStatus(404, "Error");
                        throw new Exception("check the status");
                    }
                    context.Response.End();
                    wr.CloseConnection();
                };
                //boundary is the data seperator in the Html Header
                if (!requestContentType.Contains("boundary=")) {
                    upload.Status = Models.UploadState.InvalidData;
                    upload.ValidateType = 101;
                    StopWork(true);
                    goto returnHere;
                }

                var bound = requestContentType.Substring(requestContentType.IndexOf("boundary=") + 9);

                //StreamReader is more easy to read the data line by line
                using (var reader = new StreamReader(stream)) {
                    //ReadLineAgen:
                    //Read the first line of data here
                    var line = reader.ReadLine();
                    if (string.IsNullOrEmpty(line))
                        line = reader.ReadLine();
                    stream.GetLine();
                    //    if (line == null) goto ReadLineAgen;
                    //boundary always start with two (-), we skip here
                    if (EqualsIgnoreCase(bound, line.Substring(2))) {
                        //reading Content-Disposition and Content-Type here

                        line = reader.ReadLine();
                        stream.GetLine();
                        while (!string.IsNullOrEmpty(line)) {
                            int index = line.IndexOf(':');
                            if (index >= 0) {
                                string str2 = line.Substring(0, index);

                                if (EqualsIgnoreCase(str2, "Content-Disposition")) {
                                    fieldName = ExtractValueFromContentDispositionHeader(line, index + 1, "name");
                                    fileName = ExtractValueFromContentDispositionHeader(line, index + 1, "filename");
                                }
                                else if (EqualsIgnoreCase(str2, "Content-Type")) {
                                    contentType = line.Substring(index + 1).Trim();
                                }
                            }

                            line = reader.ReadLine();
                            stream.GetLine();
                        }

                        //Check validation here
                        if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(fieldName) || string.IsNullOrEmpty(contentType)) {
                            upload.Status = Models.UploadState.InvalidData;
                            upload.ValidateType = 102;
                            StopWork(true);
                        }
                        else {
                            upload.FileName = fileName;

                            var save = GetSaveUploadFile();
                            var col = save.Validate(stream, upload.FileName);

                            //Check validation with your configuration
                            if (col.Count > 0) {
                                foreach (var item in col) {
                                    switch (item.InvalidType) {
                                        case OmidID.IO.SaveMedia.Validation.InvalidType.FileType:
                                            upload.Status = Models.UploadState.ValidationFaild;
                                            upload.ValidateType = 1;
                                            StopWork(true);
                                            goto returnHere;
                                        case OmidID.IO.SaveMedia.Validation.InvalidType.MaximumSize:
                                            upload.Status = Models.UploadState.ValidationFaild;
                                            upload.ValidateType = 2;
                                            StopWork(true);
                                            goto returnHere;
                                    }
                                }
                            }

                            //Update your upload progress info
                            upload.FileName = fileName;
                            upload.UploadedLength = Convert.ToInt32(stream.Position);

                            //Generate the temp file to save the file
                            //We can support the ********LARGE******** file NOW
                            using (FileStream fs = GenerateTempFile()) {
                                fs.SetLength(0);
                                upload.Status = Models.UploadState.Receiving;

                                //finilizing file here
                                Action SaveFunc = () => {
                                    OnSaving(upload);

                                    //Remove the two (-) char from the end of file
                                    fs.SetLength(fs.Length - 2);
                                    fs.Seek(0, SeekOrigin.Begin);

                                    try {
                                        upload.FileName = save.Save(fs, upload.FileName);
                                        upload.Status = Models.UploadState.Finished;
                                        OnSaved(upload);
                                        StopWork(false);

                                    } catch (Exception ex) {
                                        upload.FileName = ex.InnerException.Message;
                                        upload.Status = Models.UploadState.Error;
                                        upload.ValidateType = 2;
                                        StopWork(true);
                                    }
                                };

                                do {
                                    //Check the connection
                                    if (!wr.IsClientConnected()) {
                                        upload.Status = Models.UploadState.Canceled;
                                        upload.ValidateType = 1;
                                        StopWork(true);

                                        break;
                                    }
                                    try {
                                        //Read data line by line
                                        line = reader.ReadLine();
                                    }
                                    catch (Exception ex) {
                                        upload.FileName = ex.Message;
                                        upload.Status = Models.UploadState.Error;
                                        upload.ValidateType = 1;
                                        StopWork(true);

                                        break;
                                    }

                                    //check the boundary
                                    if (line.Length >= bound.Length) {
                                        var l = line.Substring(2);
                                        if (l.StartsWith(bound, true, CultureInfo.CurrentCulture)) {
                                            if (bound.Length != l.Length) {
                                                var last = l.Substring(bound.Length);
                                                //if boundary end with the -- we are on the end of the http stream
                                                if (last == "--") {
                                                    SaveFunc();
                                                    break;
                                                }
                                            }
                                            else {
                                                SaveFunc();
                                                break;
                                            }
                                        }
                                    }

                                    var arr = stream.GetLine();
                                    fs.Write(arr, 0, arr.Length);
                                    upload.UploadedLength += arr.Length;
                                    total++;
                                    //Check the Speed, Remaining, .... status
                                    if (total > 100) {
                                        var m = DateTime.Now.Subtract(lastRecOn);
                                        upload.Speed = 10240d / (m.TotalSeconds <= 0 ? 0.00000000001d : m.TotalSeconds);
                                        upload.Remaining = new TimeSpan(0, 0, (upload.ContentLength - upload.UploadedLength) / Convert.ToInt32(upload.Speed));

                                        lastRecOn = DateTime.Now;
                                        Thread.Sleep(1);
                                        total = 0;

                                        OnUpdate(upload);
                                    }
                                } while (line != null);
                                if (line == null) {
                                    upload.Status = Models.UploadState.Canceled;
                                }
                                else {
                                }
                                fs.SetLength(0);
                            }//
                        }
                    }
                    else {
                        upload.Status = Models.UploadState.InvalidData;
                        upload.ValidateType = 255;
                        StopWork(true);
                    }
                }
            }

        returnHere:
            ac = new Action<HttpContext>(ProcessRequest);
            return ac.BeginInvoke(context, cb, extraData);
        }

        protected virtual FileStream GenerateTempFile() {
            return Tools.GetRandomFile();
        }

        protected virtual SaveUploadFile GetSaveUploadFile() {
            return new SaveUploadFile();
        }

        public void EndProcessRequest(IAsyncResult result) {
            ac.EndInvoke(result);
        }

        public bool IsReusable {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context) {
        }

        protected virtual void OnUploaded(Models.UploadInfo info) {
        }

        protected virtual void OnSaving(Models.UploadInfo info) {
        }

        protected virtual void OnSaved(Models.UploadInfo info) {
        }

        protected virtual void OnUpdate(Models.UploadInfo info) {
        }

        Action<HttpContext> ac;
    }

    public class InternalStream : Stream {

        #region Init And Variables

        HttpWorkerRequest Main;
        MemoryStream PreLoadData;
        long pos = 0; long length = 0;
        Action<int> update;

        public InternalStream(HttpWorkerRequest main, long length, byte[] preLoadData, Action<int> update) {
            Main = main;
            LastReadedBuffer = new byte[0];
            this.length = length;
            this.PreLoadData = new MemoryStream(preLoadData == null ? new byte[0] : preLoadData);
            this.update = update;
        }

        #endregion

        #region Properties

        public override bool CanRead {
            get { return true; }
        }

        public override bool CanSeek {
            get { return false; }
        }

        public override bool CanWrite {
            get { return false; }
        }

        public override void Flush() {
            //
        }

        public override long Length {
            get { return length; }
        }

        public override long Position {
            get {
                return pos;
            }
            set {
                throw new NotSupportedException();
            }
        }

        #endregion

        #region Read And Buffer

        public byte[] LastReadedBuffer { get; private set; }
        private byte NewLine = (byte)'\n';

        public byte[] GetLine() {
            var pos = -1;
            var buff = LastReadedBuffer;

            for (var i = 0; i < LastReadedBuffer.Length; i++) {
                if (buff[i] == NewLine) {
                    pos = i; break;
                }
            }

            if (pos == -1) {
                LastReadedBuffer = new byte[0];
                return buff;
            }

            pos++;
            var b = new byte[pos];
            Array.Copy(buff, b, pos);

            var buffer = new byte[buff.Length - pos];
            Array.Copy(buff, pos, buffer, 0, buffer.Length);
            LastReadedBuffer = buffer;

            return b;
        }

        public override int Read(byte[] buffer, int offset, int count) {
            var readed = 0;

            if (PreLoadData.Position < PreLoadData.Length) {
                readed = PreLoadData.Read(buffer, offset, count);
                if (PreLoadData.Position >= PreLoadData.Length) {
                    readed += Main.ReadEntityBody(buffer, offset + readed, count - readed);
                    update(readed);
                }
            }
            else {
                readed = Main.ReadEntityBody(buffer, offset, count);
                update(readed);
            }
            var buff = LastReadedBuffer;


            var tot = buff.Length;
            Array.Resize(ref buff, tot + readed);
            Array.Copy(buffer, offset, buff, tot, readed);
            LastReadedBuffer = buff;

            pos += readed;
            return readed;
        }

        #endregion

        #region Position And Write

        public override long Seek(long offset, SeekOrigin origin) {
            return 0;
        }

        public override void SetLength(long value) {
            //
        }

        public override void Write(byte[] buffer, int offset, int count) {
            //
        }

        #endregion

    }

}
