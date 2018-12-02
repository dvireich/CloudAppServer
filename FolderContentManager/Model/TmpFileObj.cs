using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderContentManager.Model
{
    public class TmpFileObj: FileObj, ITmpFile
    {
        public string TmpCreationPath { get; set; }

        public TmpFileObj(
            string name, 
            string path, 
            string fileType, 
            long size, 
            string tmpCreationPath) : base(name, path, fileType, size)
        {
            TmpCreationPath = tmpCreationPath;
        }

        public TmpFileObj(
            string name,
            string path,
            string fileType,
            long size) : base(name, path, fileType, size)
        {
            TmpCreationPath = string.Empty;
        }

        public TmpFileObj(
            string name,
            string path,
            string fileType,
            long size,
            string creationTime,
            string modificationTime,
            string tmpCreationPath) : base(name, path, fileType, size, creationTime, modificationTime)
        {
            TmpCreationPath = tmpCreationPath;
        }

        public TmpFileObj()
        {
            TmpCreationPath = string.Empty;
        }
    }
}
