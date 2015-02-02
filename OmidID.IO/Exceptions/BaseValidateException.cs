using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OmidID.IO.SaveMedia.Exceptions {
    public class BaseValidateException : Exception {

        public BaseValidateException(string message) : base(message) { }

    }
}
