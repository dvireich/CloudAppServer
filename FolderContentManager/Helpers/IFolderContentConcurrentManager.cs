using System;
using System.Collections.Generic;
using FolderContentManager.Model;

namespace FolderContentManager.Helpers
{
    public interface IFolderContentConcurrentManager
    {
        //Blocks and Release all the sub-tree for operation
        void PerformWithSynchronization(ICollection<IFolderContent> folderContents, Action task);

        //Blocks and Release all the sub-tree for operation
        T PerformWithSynchronization<T>(ICollection<IFolderContent> folderContents, Func<T> task);

        //Blocks all the sub-tree for operation
        void AcquireSynchronization(ICollection<IFolderContent> folderContents);

        //Release all the sub-tree for operation
        void ReleaseSynchronization(ICollection<IFolderContent> folderContents);
    }
}
