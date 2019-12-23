using System;
using System.IO;
using System.Threading.Tasks;
using ContentManager.Helpers.Result;
using Void = ContentManager.Helpers.Result.InternalTypes.Void;

namespace ContentManager.Helpers.File_helpers
{
    public interface IFileManager
    {
        IResult<Result.InternalTypes.Void> Delete(string path);

        IResult<bool> Exists(string path);

        Task<IResult<Result.InternalTypes.Void>> CreateFileAsync(string path, Stream content, bool leaveOpen = false);

        IResult<Result.InternalTypes.Void> RenameFile(string basePath, string oldName, string newName);

        IResult<DateTime> GetCreationTime(string path);

        IResult<DateTime> GetModificationTime(string path);

        IResult<long> GetSize(string path);

        IResult<Stream> GetFileStream(string path);
    }
}
