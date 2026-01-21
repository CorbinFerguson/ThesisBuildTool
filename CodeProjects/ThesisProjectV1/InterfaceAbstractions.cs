using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ThesisProjectV1.Forms;

namespace ThesisProjectV1
{
    public interface IMessageService
    {
        void Show(string message, string title);
        void ShowError(string message, string title);
        bool Confirm(string message, string title); // Yes/No
    }

    public interface IUserPromptService
    {
        string SelectOne(string prompt, IList<string> options, string title);
        IList<string> SelectMany(string prompt, IList<string> options, string title);
        string Prompt(string prompt, string defaultValue, string regex, string title);
    }

    public interface IOpenFileService
    {
        bool TryOpen(string filter, string initialDirectory, out string filePath);
    }

    public interface IFileSystem
    {
        bool FileExists(string path);
        void SaveXml(XDocument doc, string path);
        XDocument LoadXml(string path);
    }

    public interface ISaveFileService
    {
        bool TrySave(string suggestedName, string filter, string defaultExt, out string filePath);
    }

    public interface IValidationService
    {
        List<string> ValidateL5XFile(XDocument doc);
        XDocument GetSchema();
    }

    public interface ISchemaDisambiguator
    {
        String ChooseParentFor(string elementType, IList<string> candidateParents);

        void ClashResolution(IEnumerable<XElement> clashingElements, XElement element);
    }
}
