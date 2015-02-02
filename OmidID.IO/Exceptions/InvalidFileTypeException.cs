using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OmidID.IO.SaveMedia.Exceptions {
    public class InvalidFileTypeException : BaseValidateException {

        public InvalidFileTypeException(string AllowFileTypes) : base(string.Format("File type can be \"{0}\"", AllowFileTypes)) { }

    }
}
