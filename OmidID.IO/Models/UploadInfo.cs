using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace OmidID.IO.SaveMedia.Models {

    [Serializable]
    [DataContract]
    public class UploadInfo {

        internal UploadInfo(string id) {
            UploadID = id;
        }

        [DataMember]
        public UploadState Status { get; set; }

        [DataMember]
        public int ContentLength { get; set; }

        [DataMember]
        public int UploadedLength { get; set; }

        [DataMember]
        public string FileName { get; set; }

        [DataMember]
        public double Speed { get; set; }

        [DataMember]
        public int ValidateType { get; set; }

        [DataMember]
        public TimeSpan Remaining { get; set; }

        [DataMember]
        public string UploadID { get; private set; }

        [DataMember]
        public List<int> SetCookie { get; set; }
    }

    public enum UploadState : byte {
        Init = 1,
        Receiving = 2,
        Finished = 3,
        InvalidData = 4,
        Canceled = 5,
        ValidationFaild = 6,
        Error = 255
    }

}