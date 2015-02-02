using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OmidID.IO.SaveMedia.Exceptions {
    public class InvalidConfigSourceException : Exception {

        public InvalidConfigSourceException(string message) : base (message) {
        }

    }
}
