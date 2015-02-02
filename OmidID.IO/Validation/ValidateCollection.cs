using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OmidID.IO.SaveMedia.Exceptions;

namespace OmidID.IO.SaveMedia.Validation {
    public class ValidateCollection : List<InvalidItem> {

        public void ThrowException() {
            var exp = new ValidateException();
            foreach (var item in this)
                exp.Execptions.Add(item.GetException());

            throw exp;
        }

    }
}
