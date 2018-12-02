﻿namespace FolderContentManager.Model
{
    public class FolderPage : IFolderPage
    {
        public FolderPage(string name, string path, IFolderContent[] content)
        {
            Name = name;
            Path = path;
            Content = content;
            Type = FolderContentType.FolderPage;
        }

        public FolderPage()
        {
            Type = FolderContentType.FolderPage;
            Content = new IFolderContent[0];
        }

        public string Name { get; set; }
        public string Path { get; set; }
        public FolderContentType Type { get; set; }
        public long Size { get; set; }
        public IFolderContent[] Content { get; set; }
        public string CreationTime { get; set; }
        public string ModificationTime { get; set; }
    }
}
