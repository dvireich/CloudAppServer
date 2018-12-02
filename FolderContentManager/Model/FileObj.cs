using System;

namespace FolderContentManager.Model
{
    public class FileObj : FolderContent, IFile
    {

        public string FileType { get; set; }
        public FileObj(string name, string path, string fileType, long size, string creationTime, string modificationTime) : 
            base(name, path, FolderContentType.File, creationTime, modificationTime, size)
        {
            FileType = fileType;
        }

        public FileObj(string name, string path, string fileType, long size) :
            base(name, path, FolderContentType.File, size)
        {
            FileType = fileType;
        }

        public FileObj() : base(null, null, FolderContentType.File, DateTime.Now.ToLongDateString(), DateTime.Now.ToLongDateString(), 0)
        {
        }
    }
}
