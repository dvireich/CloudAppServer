using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ContentManager.Helpers.Result;
using Void = ContentManager.Helpers.Result.InternalTypes.Void;

namespace ContentManager.Helpers.Directory_helpers
{
    public interface IDirectoryManagerAsync
    {
        Task<IResult<Result.InternalTypes.Void>> CreateDirectoryAsync(string path);

        Task<IResult<Result.InternalTypes.Void>> DeleteDirectoryAsync(string path, bool recursive);

        Task<IResult<Result.InternalTypes.Void>> RenameDirectoryAsync(string basePath, string oldName, string newName);

        Task<IResult<IEnumerable<string>>> GetAllDirectoriesAsync(string path);

        Task<IResult<IEnumerable<string>>> GetAllFilesAsync(string path);

        Task<IResult<bool>> ExistsAsync(string path);

        Task<IResult<DateTime>> GetModificationTimeAsync(string path);

        Task<IResult<DateTime>> GetCreationTimeAsync(string path);
    }
}
