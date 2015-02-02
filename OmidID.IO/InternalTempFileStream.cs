using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace OmidID.IO.SaveMedia {
    public class TempFileStream : FileStream {

        public string FileName { get; set; }
        static string TempUploadDirectory = Tools.GetPath("~/Temps");

        private static string GetPath() {
            if (!System.IO.Directory.Exists(TempUploadDirectory))
                Directory.CreateDirectory(TempUploadDirectory);

            var file = Path.Combine(TempUploadDirectory, Path.GetRandomFileName());
            return file;
        }

        public TempFileStream()
            : this(GetPath()) {
        }

        public TempFileStream(string path)
            : base(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None) {
            FileName = path;
        }

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);
            try { System.IO.File.Delete(FileName); } catch { }
        }

        public byte[] ToArray() {
            var buff = new byte[Length];
            this.Seek(0, SeekOrigin.Begin);

            Read(buff, 0, buff.Length);
            return buff;
        }

    }
}
