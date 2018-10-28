﻿using System;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using PostSharp.Extensibility;
using PostSharp.Patterns.Diagnostics;
using File = Pri.LongPath.File;

namespace FolderContentHelper
{
    [Log(AttributeTargetElements = MulticastTargets.Method, AttributeTargetTypeAttributes = MulticastAttributes.Public, AttributeTargetMemberAttributes = MulticastAttributes.Public)]
    public class FileManager : IFileManager
    {
        private readonly JavaScriptSerializer _serializer;

        public FileManager()
        {
            _serializer = new JavaScriptSerializer();
        }

        private void ValidateNameLength(string name)
        {
            if(name.Length < 250) return;
            throw new Exception("The given name is too long. Please give name less than 200 characters");
        }

        public T ReadJson<T>(string path)
        {
            path = path.ToLower();
            using (var jsonFile = File.OpenText(path))
            {
                var jsonText = jsonFile.ReadToEnd();
                var obj = _serializer.Deserialize<T>(jsonText);
                return obj;
            }
        }

        public void WriteJson(string path, object obj)
        {
            var name = path.Split('\\').Last();
            ValidateNameLength(name);
            var serializedObj = _serializer.Serialize(obj);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            using (var jsonFile = File.CreateText(path))
            {
                jsonFile.Write(serializedObj);
            }
        }

        public void MoveFileFromTmpPathToPath(string path, string tmpFilePath)
        {
            var name = path.Split('\\').Last();
            ValidateNameLength(name);

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            Move(tmpFilePath, path);
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
            ValidateNameLength(toPath);
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
            return File.Create(path);
        }
    }
}
