using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudAppServer.Model;
using FolderContentManager.Interfaces;

namespace FolderContentManager.Model
{
    public class TmpFileObj: FileObj, ITmpFile
    {
        public bool[] ValueChunks { get; set; }
        public string TmpCreationPath { get; set; }

        public TmpFileObj(
            string name, 
            string path, 
            string fileType, 
            long size, 
            bool[] valueChunks, 
            string tmpCreationPath) : base(name, path, fileType, size)
        {
            ValueChunks = valueChunks;
            TmpCreationPath = tmpCreationPath;
        }

        public TmpFileObj(
            string name,
            string path,
            string fileType,
            long size,
            bool[] valueChunks) : base(name, path, fileType, size)
        {
            ValueChunks = valueChunks;
            TmpCreationPath = string.Empty;
        }

        public TmpFileObj(
            string name,
            string path,
            string fileType,
            long size,
            string creationTime,
            string modificationTime,
            bool[] valueChunks,
            string tmpCreationPath) : base(name, path, fileType, size, creationTime, modificationTime)
        {
            ValueChunks = valueChunks;
            TmpCreationPath = tmpCreationPath;
        }

        public TmpFileObj()
        {
            ValueChunks = new bool[0];
            TmpCreationPath = string.Empty;
        }
    }
}
