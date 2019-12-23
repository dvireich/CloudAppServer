using System;
using System.Collections.Generic;
using ContentManager.Helpers.Result;
using Void = ContentManager.Helpers.Result.InternalTypes.Void;

namespace ContentManager.Helpers.Directory_helpers
{
    public interface IDirectoryManager
    {
        IResult<Result.InternalTypes.Void> CreateDirectory(string path);

        IResult<Result.InternalTypes.Void> DeleteDirectory(string path, bool recursive);

        IResult<Result.InternalTypes.Void> RenameDirectory(string basePath, string oldName, string newName);

        IResult<IEnumerable<string>> GetAllDirectories(string path);

        IResult<IEnumerable<string>> GetAllFiles(string path);

        IResult<bool> Exists(string path);

        IResult<DateTime> GetModificationTime(string path);

        IResult<DateTime> GetCreationTime(string path);
    }
}
