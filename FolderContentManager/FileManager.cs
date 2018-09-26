using System;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;

using File = Pri.LongPath.File;

namespace FolderContentHelper
{
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

        public void WriteFileContent(string path, string[] value)
        {
            var name = path.Split('\\').Last();
            ValidateNameLength(name);

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            using (var file = new BinaryWriter(File.Create(path)))
            {
                foreach (var s in value)
                {
                    var bytesToWrite = Convert.FromBase64String(s);
                    file.Write(bytesToWrite);
                    file.Flush();
                }
            }
        }

        public Stream GetFile(string path)
        {
            var f = new FileStream(path, FileMode.Open);
            int length = (int) f.Length;
            byte[] buffer = new byte[length];
            int sum = 0;
            int count;
            while ((count = f.Read(buffer, sum, length - sum)) > 0)
            {
                sum += count;
            }

            f.Close();
            return new MemoryStream(buffer);
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
    }
}
