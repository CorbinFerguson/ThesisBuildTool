
// ThesisProjectV1/Infrastructure/InfraAdapters.cs
using System.Collections.Generic;
using System.Xml.Linq;
using ThesisProjectV1.Abstractions;

namespace ThesisProjectV1.Infrastructure
{
    public sealed class ValidationService : IValidationService
    {
        private readonly ValidationHandler _inner = new ValidationHandler();

        public List<string> ValidateL5XFile(XDocument doc)
        {
            return _inner.ValidateL5XFile(doc);
        }

        public XDocument GetSchema()
        {
            return _inner.GetSchema();
        }
    }

    public sealed class FileSystem : IFileSystem
    {
        public bool FileExists(string path)
        {
            return System.IO.File.Exists(path);
        }

        public void SaveXml(XDocument doc, string path)
        {
            doc.Save(path);
        }

        public XDocument LoadXml(string path)
        {
            return XDocument.Load(path);
        }
    }
}
