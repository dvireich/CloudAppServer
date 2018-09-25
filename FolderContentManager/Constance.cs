using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FolderContentHelper.Interfaces;

namespace FolderContentHelper
{
    public class Constance : IConstance
    {
        public string HomeFolderName { get; } = "home";
        public string HomeFolderPath { get; } = "";
        public int MaxFolderContentOnPage { get; } = 136;
        public string BaseFolderPath { get; set; } = "C:\\foldercontentmanager";
    }
}
