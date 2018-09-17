using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FolderContentManager.Interfaces;

namespace FolderContentManager
{
    public class Constance : IConstance
    {
        public string HomeFolderName { get; } = "home";
        public int MaxFolderContentOnPage { get; } = 136;
    }
}
