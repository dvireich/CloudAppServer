using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using FolderContentManager.Helpers;
using FolderContentManager.Model;
using PostSharp.Extensibility;
using PostSharp.Patterns.Diagnostics;

namespace FolderContentManager.Repositories
{
    [Log(AttributeTargetElements = MulticastTargets.Method, AttributeTargetTypeAttributes = MulticastAttributes.Public, AttributeTargetMemberAttributes = MulticastAttributes.Public)]
    public class GenericFolderContentRepository<TModel,TMappable> where TMappable : IMappable<TModel>
    {
        internal readonly IFileManager FileManager;
        private readonly IConstance _constance;

        private readonly JavaScriptSerializer _serializer;

        public GenericFolderContentRepository(IConstance constance)
        {
            _constance = constance;
            _serializer = new JavaScriptSerializer();
            FileManager = new FileManager();
        }

        public TModel GetByFullPath(string name, string path, FolderContentType type)
        {
            if (!IsFolderContentExist(name, path, type)) return default(TModel);
            return ReadJson(CreateJsonPath(name, path, type));
        }

        public TModel GetByFullPath(string fullPath)
        {
            if (!IsFolderContentExist(fullPath)) return default(TModel);
            return ReadJson(fullPath);
        }

        public void CreateOrUpdate(object obj, string name, string path, FolderContentType type)
        {
            WriteJson(obj, CreateJsonPath(name, path, type));
        }

        public void CreateOrUpdate(object obj, string fullPath)
        {
            WriteJson(obj, fullPath);
        }

        public void Delete(string name, string path, FolderContentType type)
        {
            if(!FileManager.Exists(CreateJsonPath(name, path, type))) return;
            FileManager.Delete(CreateJsonPath(name, path, type));
        }

        public void Delete(string fullPath)
        {
            if (!FileManager.Exists(fullPath)) return;
            FileManager.Delete(fullPath);
        }

        private TModel ReadJson(string path)
        {
            path = path.ToLower();
            using (var jsonFile = FileManager.OpenText(path))
            {
                var jsonText = jsonFile.ReadToEnd();
                var obj = _serializer.Deserialize<TMappable>(jsonText);
                return obj.Map();
            }
        }

        private void WriteJson(object obj, string path)
        {
            var serializedObj = _serializer.Serialize(obj);
            if (FileManager.Exists(path))
            {
                FileManager.Delete(path);
            }
            using (var jsonFile = FileManager.CreateText(path))
            {
                jsonFile.Write(serializedObj);
            }
        }

        internal void ConvertNameAndPathToLower(string name, string path, out string lowerName, out string lowerPath)
        {
            lowerName = name.ToLower();
            lowerPath = path.ToLower();
        }

        private string CreateJsonPath(string name, string path, FolderContentType type)
        {
            ConvertNameAndPathToLower(name, path, out var lowerName, out var lowerPath);
            lowerPath = lowerPath.Replace('/', '\\');
            return
                string.IsNullOrEmpty(path) ?
                    $"{_constance.BaseFolderPath}\\{lowerName}{type.ToString()}.json" :
                    $"{_constance.BaseFolderPath}\\{lowerPath}\\{lowerName}{type.ToString()}.json";
        }

        private bool IsFolderContentExist(string name, string path, FolderContentType type)
        {
            return FileManager.Exists(CreateJsonPath(name, path, type));
        }

        private bool IsFolderContentExist(string fullPath)
        {
            return FileManager.Exists(fullPath);
        }
    }
}
