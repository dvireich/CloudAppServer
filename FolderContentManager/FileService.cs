using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudAppServer.Model;
using FolderContentManager.Interfaces;
using FolderContentManager.Model;
using PostSharp.Extensibility;
using PostSharp.Patterns.Diagnostics;

namespace FolderContentHelper
{
    [Log(AttributeTargetElements = MulticastTargets.Method, AttributeTargetTypeAttributes = MulticastAttributes.Public, AttributeTargetMemberAttributes = MulticastAttributes.Public)]
    public sealed class FileService : IFileService
    {
        #region Singelton
        private static FileService _instance = null;
        private static readonly object Padlock = new object();

        private FileService()
        {
            _requestIdToFileStream = new ConcurrentDictionary<int, FileDownloadData>();
            this._requestIdToFiles = new ConcurrentDictionary<int, ITmpFile>();
            _requestIdToStreamWriter = new ConcurrentDictionary<int, StreamWriter>();
        }

        [Log(AttributeExclude = true)]
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

        private readonly ConcurrentDictionary<int, ITmpFile> _requestIdToFiles;
        private readonly ConcurrentDictionary<int, FileDownloadData> _requestIdToFileStream;
        private readonly ConcurrentDictionary<int, StreamWriter> _requestIdToStreamWriter;

        public void CreateFile(int requestId, ITmpFile file)
        {
            _requestIdToFiles[requestId] = file;
            if (string.IsNullOrEmpty(file.TmpCreationPath))
            {
                file.TmpCreationPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            }

            _requestIdToStreamWriter[requestId] = new StreamWriter(file.TmpCreationPath);
        }

        [Log(AttributeExclude = true)]
        public void UpdateFileValue(int requestId, int index,  string value)
        {
            try
            {
                var file = _requestIdToFiles[requestId];
                file.ValueChunks[index] = true;
                var sr = _requestIdToStreamWriter[requestId];
                sr.WriteLine(value.ToCharArray());

                if (!IsFileFullyUploaded(requestId)) return;
                _requestIdToStreamWriter[requestId].Close();
                _requestIdToStreamWriter.TryRemove(requestId, out var streamWriter);

            }
            catch (Exception e)
            {
                Console.WriteLine($"Could not Update request id: {requestId} for file value content with the following exception: {e.Message}");
            }
        }

        public ITmpFile GetFile(int requestId)
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

        [Log(AttributeExclude = true)]
        public bool IsFileFullyUploaded(int requestId)
        {
            if (!_requestIdToFiles.ContainsKey(requestId)) return true;

            var file = _requestIdToFiles[requestId];
            return file.ValueChunks.All(element => element);
        }

        public int GetRequestIdForDownload()
        {
            lock (Padlock)
            {
                var ids = new HashSet<int>(_requestIdToFileStream.Keys);
                for (var i = 0; i < int.MaxValue; i++)
                {
                    if (!ids.Contains(i))
                    {
                        _requestIdToFileStream[i] = null;
                        return i;
                    }
                }

                return -1;
            }
        }

        public void PrepareFileToDownload(int requestId, FileDownloadData fileDownloadData)
        {
            _requestIdToFileStream[requestId] = fileDownloadData;
        }

        public FileDownloadData GetDownloadFileData(int requestId)
        {
            //_requestIdToFileStream.TryRemove(requestId, out var fileDownloadData);
            //return fileDownloadData;

            return _requestIdToFileStream[requestId];
        }
    }
}
