using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OmidID.IO.SaveMedia.Exceptions {
    public class ValidateException : Exception{

        List<BaseValidateException> execptions = new List<BaseValidateException>();

        public List<BaseValidateException> Execptions {
            get { return execptions; }
        }

        public override string Message {
            get {
                var sb = new StringBuilder();
                foreach (var item in Execptions)
                    sb.AppendLine(item.Message);

                return sb.ToString();
            }
        }

    }
}
