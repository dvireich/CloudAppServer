using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ContentManager.Helpers.Result;
using Void = ContentManager.Helpers.Result.InternalTypes.Void;

namespace ContentManager.Helpers.File_helpers
{
    public class FileManagerAsync : FileManager, IFileManagerAsync
    {
        public Task<IResult<Result.InternalTypes.Void>> DeleteAsync(string path)
        {
            return Task.Run(() => Delete(path));
        }

        public Task<IResult<bool>> ExistsAsync(string path)
        {
            return Task.Run(() => Exists(path));
        }

        public Task<IResult<Result.InternalTypes.Void>> RenameFileAsync(string basePath, string oldName, string newName)
        {
            return Task.Run(() => RenameFile(basePath, oldName, newName));
        }

        public Task<IResult<DateTime>> GetCreationTimeAsync(string path)
        {
            return Task.Run(() => GetCreationTime(path));
        }

        public Task<IResult<DateTime>> GetModificationTimeAsync(string path)
        {
            return Task.Run(() => GetModificationTime(path));
        }

        public Task<IResult<long>> GetSizeAsync(string path)
        {
            return Task.Run(() => GetSize(path));
        }

        public Task<IResult<Stream>> GetFileStreamAsync(string path)
        {
            return Task.Run(() => GetFileStream(path));
        }
    }
}
