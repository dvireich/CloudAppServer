using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderContentHelper.Model
{
    public interface IFileTrackInfo : IFolderContentTrackInfo
    {
        long Size { get; set; }
    }
}
