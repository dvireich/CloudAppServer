﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderContentManager.Model
{
    public interface IFolderContentTrackInfo
    {
        string CreationTime { get; set; }
        string ModificationTime { get; set; }
    }
}