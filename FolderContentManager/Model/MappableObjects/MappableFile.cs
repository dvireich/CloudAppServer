using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using FolderContentManager.Repositories;

namespace CloudAppServer.Model
{
    public class MappableFile : IMappable<IFile>
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public FolderContentType Type { get; set; }
        public string CreationTime { get; set; }
        public string ModificationTime { get; set; }
        public long Size { get; set; }
        public string FileType { get; set; }

        public IFile Map()
        {
            return new FileObj(Name, Path, FileType, Size, CreationTime, ModificationTime);
        }
    }
}
