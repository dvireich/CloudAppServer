﻿using System;
using System.Runtime.Serialization;

namespace CloudAppServer.Model
{
    public class FolderContent : IFolderContent
    {
        public FolderContent(string name, string path, FolderContentType type, string creationTime, string modificationTime)
        {
            Name = name;
            Path = path;
            Type = type;
            CreationTime = creationTime;
            ModificationTime = modificationTime;
        }

        public FolderContent(string name, string path, FolderContentType type)
        {
            Name = name;
            Path = path;
            Type = type;
            CreationTime = $"{DateTime.Now:G}";
            ModificationTime = $"{DateTime.Now:G}";
        }

        public FolderContent()
        {
        }

        public string Name { get; set; }
        public string Path { get; set; }
        public FolderContentType Type { get; set; }
        public string CreationTime { get; set; }
        public string ModificationTime { get; set; }
    }
}
