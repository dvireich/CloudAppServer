﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CloudAppServer.ServiceModel
{
    [DataContract]
    public class FolderContentFileObj
    {
        [DataMember]
        public string Name;
        [DataMember]
        public string Path;
        [DataMember]
        public string FileType;
        [DataMember]
        public string NewValue;
        [DataMember]
        public int RequestId;
        [DataMember]
        public long Size;
        [DataMember]
        public long Sent;
    }
}