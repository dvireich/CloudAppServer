using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CloudAppServer.ServiceModel
{
    [DataContract]
    public class FolderContentCopyObj
    {
        [DataMember]
        public string FolderContentName;
        [DataMember]
        public string FolderContentPath;
        [DataMember]
        public string FolderContentType;

        [DataMember]
        public string CopyToName;
        [DataMember]
        public string CopyToPath;
    }
}
