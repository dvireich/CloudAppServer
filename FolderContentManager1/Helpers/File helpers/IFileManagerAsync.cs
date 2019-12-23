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
    public interface IFileManagerAsync
    {
        Task<IResult<Void>> DeleteAsync(string path);

        Task<IResult<bool>> ExistsAsync(string path);

        Task<IResult<Void>> CreateFileAsync(string path, Stream content, bool leaveOpen = false);

        Task<IResult<Void>> RenameFileAsync(string basePath, string oldName, string newName);

        Task<IResult<DateTime>> GetCreationTimeAsync(string path);

        Task<IResult<DateTime>> GetModificationTimeAsync(string path);

        Task<IResult<long>> GetSizeAsync(string path);

        Task<IResult<Stream>> GetFileStreamAsync(string path);
    }
}
