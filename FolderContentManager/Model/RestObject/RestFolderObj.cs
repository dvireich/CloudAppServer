using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using CloudAppServer.Model;
using FolderContentManager.Model;

namespace CloudAppServer.Model
{
    public class RestFolderObj
    {

        public RestFolderObj()
        {
            Type = FolderContentType.Folder;
            NumOfPages = NextPageToWrite = 1;
        }

        public string Name { get; set; }
        public string Path { get; set; }
        public FolderContentType Type { get; set; }
        public int NumOfPages { get; set; }
        public int NextPageToWrite { get; set; }
        DateTime CreationTime { get; set; }
        DateTime ModificationTime { get; set; }
        public SortType SortType { get; set; }


        public IFolder MapToIFolder()
        {
            return new FolderObj(Name, Path, NumOfPages, NextPageToWrite, CreationTime, ModificationTime, SortType);
        }
    }
}
