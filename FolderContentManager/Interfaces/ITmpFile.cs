using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudAppServer.Model;

namespace FolderContentManager.Interfaces
{
    public interface ITmpFile : IFile
    {
        bool[] ValueChunks { get; set; }

        string TmpCreationPath { get; set; }
    }
}
