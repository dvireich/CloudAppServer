using System;
using ContentManager.Helpers.Configuration;
using ContentManager.Helpers.Directory_helpers;
using ContentManager.Helpers.File_helpers;
using ContentManager.Helpers.Path_helpers;

namespace ContentManager.Model
{
    public abstract class SystemContentBase : ContentBase
    {
        #region Members

        public sealed override string RelativePath { get; protected set; }
        public sealed override string Name { get; protected set; }
        protected readonly IDirectoryManagerAsync DirectoryManager;
        protected readonly IFileManagerAsync FileManager;
        protected readonly IPathManager PathManager;
        protected readonly IConfiguration Configuration;
        public readonly string FullPath;
        
        #endregion

        #region Ctor

        protected SystemContentBase(
            IDirectoryManagerAsync directoryManager, 
            IPathManager pathManager, 
            IFileManagerAsync fileManager, 
            string relativePath, 
            IConfiguration configuration)
        {
            DirectoryManager = directoryManager;
            PathManager = pathManager;
            Configuration = configuration;
            FileManager = fileManager;
            FullPath = GetFullPath(relativePath);
            Name = GetName();
            RelativePath = GetRelativePath();
        }

        #endregion

        #region Private methods

        private string GetFullPath(string relativePath)
        {
            var fullPathResult = PathManager.Combine(Configuration.HomeFolderPath, relativePath);

            if (!fullPathResult.IsSuccess)
            {
                throw new ArgumentException($"Could not create full path from '{Configuration.BaseFolderPath}' and '{relativePath}'", fullPathResult.Exception);
            }

            return fullPathResult.Data;
        }

        private string GetName()
        {
            var parentPathResult = PathManager.GetParent(FullPath);

            if (!parentPathResult.IsSuccess)
            {
                throw new ArgumentException($"Could not get parent path from '{FullPath}'", parentPathResult.Exception);
            }

            return FullPath.Replace(parentPathResult.Data, string.Empty).Trim('\\');
        }

        private string GetRelativePath()
        {
            var relativePath = FullPath.Replace(Configuration.HomeFolderPath, string.Empty).Trim('\\');
            var parentPathResult = PathManager.GetParent(relativePath);

            if (!parentPathResult.IsSuccess)
            {
                throw new ArgumentException($"Could not get parent path from '{FullPath}'", parentPathResult.Exception);
            }

            return parentPathResult.Data;
        }

        #endregion
    }
}
