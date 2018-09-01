using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CloudAppServer.ServiceModel
{
    [DataContract]
    public class FolderContentRenameObj
    {
        [DataMember]
        public string Name;
        [DataMember]
        public string Path;
        [DataMember]
        public string Type;
        [DataMember]
        public string NewName;
    }
}
