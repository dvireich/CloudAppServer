using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CloudAppServer.ServiceModel
{
    [DataContract]
    public class CreateFolderContentFileObj
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
        public int NewValueIndex;
        [DataMember]
        public int NumOfChunks;
        [DataMember]
        public long Size;
    }
}
