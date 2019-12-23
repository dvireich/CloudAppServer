using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ContentManager.Helpers.Path_helpers;
using ContentManager.Helpers.Result;
using Void = ContentManager.Helpers.Result.InternalTypes.Void;

namespace ContentManager.Helpers.File_helpers
{
    using File = Pri.LongPath.File;

    public class FileManager : IFileManager
    {
        #region Members

        private readonly IPathManager _pathManager;

        #endregion

        #region Ctor

        public FileManager()
        {
            _pathManager = new PathManager();
        }

        #endregion

        #region Public methods

        public IResult<bool> Exists(string path)
        {
            try
            {
                var isExists = File.Exists(path);

                return new SuccessResult<bool>(isExists);
            }
            catch (Exception e)
            {
                return new FailureResult<bool>(e);
            }
        }

        public async Task<IResult<Void>> CreateFileAsync(string path, Stream content, bool leaveOpen = false)
        {
            try
            {
                ValidateNameLength(path);

                using (var fileStream = File.Create(path))
                {
                    await content.CopyToAsync(fileStream);
                }

                return new SuccessResult();

            }
            catch (Exception e)
            {
                return new FailureResult(e);
            }
            finally
            {
                if (!leaveOpen)
                {
                    content.Dispose();
                }
            }
        }

        public IResult<Stream> GetFileStream(string path)
        {
            try
            {
                var stream = File.Open(path, FileMode.Append, FileAccess.ReadWrite);

                return new SuccessResult<Stream>(stream);
            }
            catch (Exception e)
            {
                return new FailureResult<Stream>(e);
            }
        }

        public IResult<Result.InternalTypes.Void> Delete(string path)
        {
            try
            {
                File.Delete(path);

                return new SuccessResult();
            }
            catch (Exception e)
            {
                return new FailureResult(e);
            }
        }

        public IResult<Result.InternalTypes.Void> RenameFile(string basePath, string oldName, string newName)
        {
            var oldPathResult = _pathManager.Combine(basePath, oldName);

            if (!oldPathResult.IsSuccess)
            {
                return new FailureResult(oldPathResult.Exception);
            }

            var newPathResult = _pathManager.Combine(basePath, newName);

            if (!newPathResult.IsSuccess)
            {
                return new FailureResult(newPathResult.Exception);
            }

            try
            {
                ValidateNameLength(newPathResult.Data);
                Directory.Move(oldPathResult.Data, newPathResult.Data);

                return new SuccessResult();
            }
            catch (Exception e)
            {
                return new FailureResult(e);
            }
        }

        public IResult<DateTime> GetCreationTime(string path)
        {
            try
            {
                return new SuccessResult<DateTime>(File.GetCreationTime(path));
            }
            catch (Exception e)
            {
                return new FailureResult<DateTime>(e);
            }
        }

        public IResult<DateTime> GetModificationTime(string path)
        {
            try
            {
                return new SuccessResult<DateTime>(File.GetLastWriteTime(path));
            }
            catch (Exception e)
            {
                return new FailureResult<DateTime>(e);
            }
        }

        public IResult<long> GetSize(string path)
        {
            try
            {
                var fileSize = new FileInfo(path).Length;

                return new SuccessResult<long>(fileSize);
            }
            catch (Exception e)
            {
                return new FailureResult<long>(e);
            }
        }

        #endregion

        #region Private methods

        private void ValidateNameLength(string path)
        {
            if (path.Split('\\').Last().Length >= 250)
            {
                throw new ArgumentException("The given name is too long. Please give name less than 200 characters");
            }
        }

        #endregion
    }
}
