﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CloudAppServer.ServiceModel
{
    [DataContract]
    public class NumberOfPageRequest
    {
        [DataMember]
        public string Name;
        [DataMember]
        public string Path;
        [DataMember]
        public bool SerachMode;
    }
}
