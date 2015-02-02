using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace OmidID.IO.SaveMedia {

    public delegate void SaveFileEventHandler(object sender, SavingFileEventArgs e);
    public class SavingFileEventArgs : EventArgs {

        public SavingFileEventArgs(string filename, Stream stream) {
            this.Stream = stream;
            this.FileName = filename;
        }

        public string FileName { get; private set; }
        public string Path { get; set; }
        public string SaveAs { get; set; }
        public string AppPath { get; set; }

        public Stream Stream { get; private set; }
        public bool Handled { get; set; }

    }
}
