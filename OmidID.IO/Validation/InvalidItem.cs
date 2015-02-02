using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OmidID.IO.SaveMedia.Exceptions;

namespace OmidID.IO.SaveMedia.Validation {

    public enum InvalidType {
        MaximumSize = 1,
        FileType = 2
    }

    public class InvalidItem {

        public InvalidType InvalidType { get; private set; }
        public string Message { get; private set; }
        public object Attach { get; private set; }

        public InvalidItem(InvalidType type, string message, object attach) {
            this.InvalidType = type;
            Message = message;
            Attach = attach;
        }

        public void ThrowException() { throw GetException(); }

        public BaseValidateException GetException() {
            switch (InvalidType){
                case InvalidType.MaximumSize:
                    return new MaximumFileSizeException((int)Attach);
                case InvalidType.FileType:
                    return new InvalidFileTypeException((string)Attach);
            }

            return null;
        }

    }

}
