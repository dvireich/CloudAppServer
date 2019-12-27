using System;
using System.IO;
using System.Threading.Tasks;
using ContentManager.Helpers.Configuration;
using ContentManager.Helpers.Directory_helpers;
using ContentManager.Helpers.File_helpers;
using ContentManager.Helpers.Path_helpers;
using ContentManager.Helpers.Result;
using Void = ContentManager.Helpers.Result.InternalTypes.Void;

namespace ContentManager.Model.Folders
{
    public class BufferedFolder : Folder
    {
        #region Ctor

        public BufferedFolder(
            IDirectoryManagerAsync directoryManager,
            IPathManager pathManager,
            IFileManagerAsync fileManager,
            string relativePath,
            IConfiguration configuration)
            : base(directoryManager, pathManager, fileManager, relativePath, configuration)
        {
        }

        #endregion

        public async Task<IResult<Void>> BufferToFileAsync(string fileName, string base64StringContent)
        {
            var getFileResult = await GetChildFileAsync(fileName);

            if (!getFileResult.IsSuccess)
            {
                return new FailureResult(getFileResult.Exception);
            }

            var getFileStream = await getFileResult.Data.GetStreamAsync();

            if (!getFileStream.IsSuccess)
            {
                return new FailureResult(getFileStream.Exception);
            }

            var streamToWrite = new MemoryStream(Convert.FromBase64String(base64StringContent));
            await streamToWrite.CopyToAsync(getFileStream.Data);

            getFileStream.Data.Dispose();

            return new SuccessResult();
        }

    }
}
