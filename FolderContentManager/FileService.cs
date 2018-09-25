using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudAppServer.Model;

namespace FolderContentHelper
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
            try
            {
                var file = _requestIdToFiles[requestId];
                file.Value[index] = value;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Could not Update request id: {requestId} for file value content with the following exception: {e.Message}");
            }
        }

        public IFile GetFile(int requestId)
        {
            try
            {
                return _requestIdToFiles[requestId];
            }
            catch (Exception e)
            {
                var message = $"Could not find the file for request id: {requestId}... with the following error: {e.Message}";
                Console.WriteLine(message);
                return null;
            }
            
        }

        public void Finish(int requestId)
        {
            Console.WriteLine($"Finishing upload by removing the request id: {requestId}");
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
            return file.Value.All(element => element != null);
        }
    }
}
