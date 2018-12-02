using System;
using System.IO;
using System.Linq;
using PostSharp.Extensibility;
using PostSharp.Patterns.Diagnostics;
using File = Pri.LongPath.File;

namespace FolderContentManager.Helpers
{
    [Log(AttributeTargetElements = MulticastTargets.Method, AttributeTargetTypeAttributes = MulticastAttributes.Public, AttributeTargetMemberAttributes = MulticastAttributes.Public)]
    public class FileManager : IFileManager
    {
        private readonly IPathManager _pathManager;

        public FileManager()
        {
            _pathManager = new PathManager();
        }

        private void ValidateNameLength(string name)
        {
            if(name.Length < 250) return;
            throw new Exception("The given name is too long. Please give name less than 200 characters");
        }

        public Stream GetFile(string path)
        {
            return File.OpenRead(path);
        }

        public void Delete(string path)
        {
            File.Delete(path);
        }

        public void Move(string fromPath, string toPath)
        {
            var name = toPath.Split('\\').Last();
            ValidateNameLength(name);
            File.Move(fromPath, toPath);
        }

        public void Copy(string fromPath, string toPath)
        {
            File.Copy(fromPath, toPath);
        }

        public bool Exists(string path)
        {
            return File.Exists(path);
        }

        public Stream Create(string path)
        {
            var name = path.Split('\\').Last();
            ValidateNameLength(name);
            return File.Create(path);
        }

        public StreamReader OpenText(string path)
        {
            return File.OpenText(path);
        }

        public StreamWriter CreateText(string path)
        {
            var name = path.Split('\\').Last();
            ValidateNameLength(name);
            return File.CreateText(path);
        }

        public string GetFileExtension(string fileName)
        {
            return _pathManager.GetFileExtension(fileName);
        }
    }
}
