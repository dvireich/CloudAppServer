using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderContentHelper
{
    public interface IFileManager
    {
        T ReadJson<T>(string path);

        void WriteJson(string path, object obj);

        void WriteFileContent(string path, string[] value);

        Stream GetFile(string path);

        void Delete(string path);

        void Move(string fromPath, string toPath);

        void Copy(string fromPath, string toPath);

        bool Exists(string path);
    }
}
