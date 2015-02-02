using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OmidID.IO.SaveMedia.Exceptions {
    public class MaximumFileSizeException : BaseValidateException {

        public MaximumFileSizeException(int Max) : base(string.Format("Maximum size can be {0}kb", Max)) { }

    }
}
