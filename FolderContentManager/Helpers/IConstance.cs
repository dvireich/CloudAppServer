using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderContentHelper.Interfaces
{
    public interface IConstance
    {
        string HomeFolderName { get; }

        string HomeFolderPath { get; }

        int MaxFolderContentOnPage { get; }

        string BaseFolderPath { get; set; }
    }
}
