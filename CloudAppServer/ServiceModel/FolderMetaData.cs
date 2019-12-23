using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using ContentManager.Model.Enums;

namespace CloudAppServer.ServiceModel
{
    [DataContract]
    public class FolderMetadata
    {
        [DataMember]
        public string Name;
        [DataMember]
        public string Path;
        [DataMember]
        public SortType SortType;
        [DataMember]
        public int NumberOfPagesPerPage;
    }
}
