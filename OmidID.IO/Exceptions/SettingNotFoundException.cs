using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OmidID.IO.SaveMedia.Exceptions {
    public class SettingNotFoundException : Exception {

        public SettingNotFoundException(string message) : base(message) { }

    }
}
