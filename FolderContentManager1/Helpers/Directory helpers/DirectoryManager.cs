using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ContentManager.Helpers.Path_helpers;
using ContentManager.Helpers.Result;
using Directory = Pri.LongPath.Directory;
using DirectoryInfo = Pri.LongPath.DirectoryInfo;
using Void = ContentManager.Helpers.Result.InternalTypes.Void;

namespace ContentManager.Helpers.Directory_helpers
{
    public  class DirectoryManager : IDirectoryManager
    {
        #region Members

        private readonly IPathManager _pathManager;

        #endregion

        #region Ctor

        public DirectoryManager()
        {
            _pathManager = new PathManager();
        }

        #endregion

        #region Public methods

        public IResult<Result.InternalTypes.Void> DeleteDirectory(string path, bool recursive)
        {
            try
            {
                Directory.Delete(path, recursive);

                return new SuccessResult();
            }
            catch (Exception e)
            {
                return new FailureResult(e);
            }
        }

        public IResult<Result.InternalTypes.Void> CreateDirectory(string path)
        {
            try
            {
                ValidateNameLength(path);
                Directory.CreateDirectory(path);

                return new SuccessResult();
            }
            catch (Exception e)
            {
                return new FailureResult(e);
            }
        }

        public IResult<bool> Exists(string path)
        {
            try
            {
                var isExists = Directory.Exists(path);

                return new SuccessResult<bool>(isExists);
            }
            catch (Exception e)
            {
                return new FailureResult<bool>(e);
            }
        }

        public IResult<Result.InternalTypes.Void> RenameDirectory(string basePath, string oldName, string newName)
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

        public IResult<IEnumerable<string>> GetAllDirectories(string path)
        {
            try
            {
                var directories = Directory.GetDirectories(path);

                return new SuccessResult<IEnumerable<string>>(directories);
            }
            catch (Exception e)
            {
              return new FailureResult<IEnumerable<string>>(e);
            }
        }

        public IResult<IEnumerable<string>> GetAllFiles(string path)
        {
            try
            {
                var files = Directory.GetFiles(path);

                return new SuccessResult<IEnumerable<string>>(files);
            }
            catch (Exception e)
            {
                return new FailureResult<IEnumerable<string>>(e);
            }
        }

        public IResult<DateTime> GetCreationTime(string path)
        {
            try
            {
                return new SuccessResult<DateTime>(Directory.GetCreationTime(path));
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
                return new SuccessResult<DateTime>(Directory.GetLastWriteTime(path));
            }
            catch (Exception e)
            {
                return new FailureResult<DateTime>(e);
            }
        }

        #endregion

        #region Private methods

        private void ValidateNameLength(string path)
        {
            if (path.Split('\\').Last().Length >= 250)
            {
                throw new ArgumentException("The given name is too long. Please give name less than 250 characters");
            }
        }

        #endregion
    }
}
