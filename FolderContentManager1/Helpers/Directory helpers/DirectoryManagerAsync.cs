using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ContentManager.Helpers.Result;
using Void = ContentManager.Helpers.Result.InternalTypes.Void;

namespace ContentManager.Helpers.Directory_helpers
{
    public class DirectoryManagerAsync : DirectoryManager, IDirectoryManagerAsync
    {
        public Task<IResult<Result.InternalTypes.Void>> CreateDirectoryAsync(string path)
        {
            return Task.Run(() => CreateDirectory(path));
        }

        public Task<IResult<Result.InternalTypes.Void>> DeleteDirectoryAsync(string path, bool recursive)
        {
            return Task.Run(() => DeleteDirectory(path, recursive));
        }

        public Task<IResult<Result.InternalTypes.Void>> RenameDirectoryAsync(string basePath, string oldName, string newName)
        {
            return Task.Run(() => RenameDirectory(basePath, oldName, newName));
        }

        public Task<IResult<IEnumerable<string>>> GetAllDirectoriesAsync(string path)
        {
            return Task.Run(() => GetAllDirectories(path));
        }

        public Task<IResult<IEnumerable<string>>> GetAllFilesAsync(string path)
        {
            return Task.Run(() => GetAllFiles(path));
        }

        public Task<IResult<bool>> ExistsAsync(string path)
        {
            return Task.Run(() => Exists(path));
        }

        public Task<IResult<DateTime>> GetModificationTimeAsync(string path)
        {
            return Task.Run(() => GetModificationTime(path));
        }

        public Task<IResult<DateTime>> GetCreationTimeAsync(string path)
        {
            return Task.Run(() => GetCreationTime(path));
        }
    }
}
