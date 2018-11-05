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
using FolderContentManager.Helpers;
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
            _pathManager = new PathManager();
            _fileManager = new FileManager();
            _requestIdToFileStream = new ConcurrentDictionary<int, FileDownloadData>();
            this._requestIdToFiles = new ConcurrentDictionary<int, ITmpFile>();
            _requestIdToBinaryWriter = new ConcurrentDictionary<int, BinaryWriter>();
        }

        private readonly ConcurrentDictionary<int, ITmpFile> _requestIdToFiles;
        private readonly ConcurrentDictionary<int, FileDownloadData> _requestIdToFileStream;
        private readonly ConcurrentDictionary<int, BinaryWriter> _requestIdToBinaryWriter;
        private readonly IFolderContentConcurrentManager _concurrentManager;
        private readonly IFileManager _fileManager;
        private readonly IPathManager _pathManager;

        public void CreateFile(int requestId, ITmpFile file)
        {
            //Synchronization starts here and end in the folder content manager in CreateFile
            _concurrentManager.AcquireSynchronization(new List<IFolderContent>(){new FolderContent(file.Name, file.Path, file.Type)});

            _requestIdToFiles[requestId] = file;
            if (string.IsNullOrEmpty(file.TmpCreationPath))
            {
                file.TmpCreationPath = _pathManager.Combine(_pathManager.GetTempPath(), Guid.NewGuid().ToString());
            }

            if (_requestIdToBinaryWriter.ContainsKey(requestId))
            {
                var writer = _requestIdToBinaryWriter[requestId];
                writer.Close();
            }

            _requestIdToBinaryWriter[requestId] = new BinaryWriter(_fileManager.Create(file.TmpCreationPath));
        }

        [Log(AttributeExclude = true)]
        public void UpdateFileValue(int requestId, string value, long sent, long size)
        {
            try
            {
                var binaryWriter = _requestIdToBinaryWriter[requestId];
                var bytesToWrite = Convert.FromBase64String(value);
                binaryWriter.Write(bytesToWrite);

                if (sent < size) return;
                _requestIdToBinaryWriter[requestId].Close();
                _requestIdToBinaryWriter.TryRemove(requestId, out var writer);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Could not CreateOrUpdate request id: {requestId} for file value content with the following exception: {e.Message}");
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

        public void Cancel(int requestId)
        {
            _requestIdToFiles.TryRemove(requestId, out var file);
            _concurrentManager.ReleaseSynchronization(new List<IFolderContent>() { new FolderContent(file.Name, file.Path, file.Type) });
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
