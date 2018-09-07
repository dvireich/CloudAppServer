using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudAppServer.Model;

namespace FolderContentManager
{
    public sealed class FileService : IFileService
    {
        #region Singelton
        private static FileService _instance = null;
        private static readonly object Padlock = new object();

        private FileService()
        {
            this._requestIdToFiles = new ConcurrentDictionary<int, IFile>();
        }

        public static FileService Instance
        {
            get
            {
                lock (Padlock)
                {
                    return _instance ?? (_instance = new FileService());
                }
            }
        }
        #endregion Singelton

        private readonly ConcurrentDictionary<int, IFile> _requestIdToFiles;

        public void CreateFile(int requestId, IFile file)
        {
            _requestIdToFiles[requestId] = file;
        }

        public void UpdateFileValue(int requestId, int index,  string value)
        {
            var file = _requestIdToFiles[requestId];
            file.Value[index] = value;
        }

        public IFile GetFile(int requestId)
        {
            return _requestIdToFiles[requestId];
        }

        public void Finish(int requestId)
        {
            _requestIdToFiles.TryRemove(requestId, out var file);
        }

        public int GetRequestId()
        {
            lock (Padlock)
            {
                var takenIds = new HashSet<int>(_requestIdToFiles.Keys);
                for (var i = 0; i < int.MaxValue; i++)
                {
                    if (!takenIds.Contains(i))
                    {
                        _requestIdToFiles[i] = null;
                        return i;
                    }
                }

                return -1;
            } 
        }

        public bool IsFileFullyUploaded(int requestId)
        {
            if (!_requestIdToFiles.ContainsKey(requestId)) return true;

            var file = _requestIdToFiles[requestId];
            return file.Value.All(element => !string.IsNullOrEmpty(element));
        }
    }
}
