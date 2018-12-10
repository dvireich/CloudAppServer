using System;
using System.Collections.Generic;
using System.Linq;
using FolderContentManager.Model;
using FolderContentManager.Services;

namespace FolderContentManager.Helpers
{
    public class FolderContentConcurrentManager : IFolderContentConcurrentManager
    {

        private readonly IFolderContentFolderService _folderContentFolderService;
        private readonly IFolderContentPageService _folderContentPageService;
        private readonly Dictionary<IFolderContent, ICollection<IFolderContent>> _concurrentOperationToFolderContent;

        public FolderContentConcurrentManager(IConstance constance)
        {
            _folderContentPageService = new FolderContentPageService(constance);
            _folderContentFolderService = new FolderContentFolderService(constance);
            _concurrentOperationToFolderContent = new Dictionary<IFolderContent, ICollection<IFolderContent>>();
        }

        private const string ConcurrentMessageError =
            "Changes are made by you or by another user who is logged in. Please refresh the page";

        private bool CanAcquire(string name, string path, FolderContentType folderContentType)
        {
            foreach (var list in _concurrentOperationToFolderContent.Values)
            {
                if (list.Any(fc =>
                    fc.Path == path &&
                    fc.Name == name &&
                    fc.Type == folderContentType)) return false;

            }

            return true;
        }

        public void PerformWithSynchronization(ICollection<IFolderContent> folderContents, Action task)
        {
            try
            {
                AcquireSynchronization(folderContents);
                task();
            }
            finally
            {
                ReleaseSynchronization(folderContents);
            }
        }

        public T PerformWithSynchronization<T>(ICollection<IFolderContent> folderContents , Func<T> task)
        {
            try
            {
                AcquireSynchronization(folderContents);
                return task();
            }
            finally
            {
                ReleaseSynchronization(folderContents);
            }
        }

        public void AcquireSynchronization(ICollection<IFolderContent> folderContents)
        {
            lock (this)
            {
                if (!folderContents.All(fc => CanAcquire(fc.Name, fc.Path, fc.Type))) throw new Exception(ConcurrentMessageError);
                foreach (var fc in folderContents)
                {
                    //Get all the sub children of the folder content because we may updates the children folder content in changes on the parent folder
                    var forbiddenFolderContents = GetSubs(fc);
                    var parent = _folderContentFolderService.GetParentFolder(fc);
                    if (parent != null)
                    {
                        //Add the parent because we updates the parent folder in changes on the folder content
                        forbiddenFolderContents.Add(parent);
                    }
                    _concurrentOperationToFolderContent[fc] = forbiddenFolderContents;
                }
            }
        }

        public void ReleaseSynchronization(ICollection<IFolderContent> folderContents)
        {
            lock (this)
            {
                var keys = _concurrentOperationToFolderContent.Keys.ToArray();
                foreach (var fc in folderContents)
                {
                    foreach (var key in keys)
                    {
                        if (key.Path != fc.Path || key.Name != fc.Name ||  key.Type != fc.Type) continue;
                        _concurrentOperationToFolderContent.Remove(key);
                    }
                }
            }
        }

        private ICollection<IFolderContent> GetSubs(IFolderContent folderContent)
        {
            void GetAllSubs(IFolderContent fc, ICollection<IFolderContent> subs)
            {
                subs.Add(fc);
                if (fc.Type != FolderContentType.Folder) return;

                var folder = _folderContentFolderService.GetFolder(fc.Name, fc.Path);
                for (var i = 1; (folder != null && i <= folder.NumOfPhysicalPages); i++)
                {
                    var page = _folderContentPageService.GetPhysicalFolderPage(folder, i);
                    foreach (var content in page.Content)
                    {
                        GetAllSubs(content, subs);
                    }
                }
            }

            var allSubs = new List<IFolderContent>();
            GetAllSubs(folderContent, allSubs);
            return allSubs;
        }
    }
}
