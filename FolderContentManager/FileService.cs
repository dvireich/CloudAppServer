using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudAppServer.Model;
using FolderContentHelper.Interfaces;
using FolderContentManager;
using FolderContentManager.Interfaces;
using FolderContentManager.Model;
using PostSharp.Extensibility;
using PostSharp.Patterns.Diagnostics;

namespace FolderContentHelper
{
    [Log(AttributeTargetElements = MulticastTargets.Method, AttributeTargetTypeAttributes = MulticastAttributes.Public, AttributeTargetMemberAttributes = MulticastAttributes.Public)]
    public sealed class FileService : IFileService
    {

        public FileService(IFolderContentConcurrentManager concurrentManager)
        {
            _concurrentManager = concurrentManager;
            _requestIdToFileStream = new ConcurrentDictionary<int, FileDownloadData>();
            this._requestIdToFiles = new ConcurrentDictionary<int, ITmpFile>();
            _requestIdToStreamWriter = new ConcurrentDictionary<int, StreamWriter>();
        }

        private readonly ConcurrentDictionary<int, ITmpFile> _requestIdToFiles;
        private readonly ConcurrentDictionary<int, FileDownloadData> _requestIdToFileStream;
        private readonly ConcurrentDictionary<int, StreamWriter> _requestIdToStreamWriter;
        private readonly IFolderContentConcurrentManager _concurrentManager;

        public void CreateFile(int requestId, ITmpFile file)
        {
            //Synchronization starts here and end in the folder content manager in CreateFile
            _concurrentManager.AcquireSynchronization(new List<IFolderContent>(){new FolderContent(file.Name, file.Path, file.Type)});

            _requestIdToFiles[requestId] = file;
            if (string.IsNullOrEmpty(file.TmpCreationPath))
            {
                file.TmpCreationPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            }

            _requestIdToStreamWriter[requestId] = new StreamWriter(file.TmpCreationPath);
        }

        [Log(AttributeExclude = true)]
        public void UpdateFileValue(int requestId, string value, long sent, long size)
        {
            try
            {
                var sr = _requestIdToStreamWriter[requestId];
                sr.WriteLine(value.ToCharArray());

                if (sent < size) return;
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
            lock (this)
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

        public int GetRequestIdForDownload()
        {
            lock (this)
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
            return _requestIdToFileStream[requestId];
        }
    }
}
