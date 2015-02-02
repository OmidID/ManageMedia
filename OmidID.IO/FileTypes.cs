using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OmidID.IO.SaveMedia {
    public enum FileType : byte {
        Documents = 1,
        Pictures = 2,
        Videos = 3,
        Software = 4,
        Compress,
        Other = 5
    }

}
