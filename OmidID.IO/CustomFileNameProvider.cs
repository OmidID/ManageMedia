using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OmidID.IO.SaveMedia {
    public abstract class CustomFileNameProvider {

        public abstract void GenerateFileName(SavingFileEventArgs args);

    }
}
