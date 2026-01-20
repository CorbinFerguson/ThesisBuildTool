
// ThesisProjectV1/Abstractions/IUiServices.cs
using System.Collections.Generic;
using System.Xml.Linq;

namespace ThesisProjectV1.Abstractions
{
    public interface IMessageService
    {
        void Show(string message, string title);
        void ShowError(string message, string title);
        bool Confirm(string message, string title); // Yes/No
    }

    public interface IUserPromptService
    {
        // Single and multi-select prompts
        string SelectOne(string prompt, IList<string> options, string title);
        IList<string> SelectMany(string prompt, IList<string> options, bool allowNone, string title);

        // Text input with optional regex validation
        string Prompt(string prompt, string defaultValue, string regex, string title);
    }

    public interface IOpenFileService
    {
        // Returns true if a file was chosen; out contains path
        bool TryOpen(string filter, out string filePath, string initialDirectory);
    }

    public interface ISaveFileService
    {
        // Returns true if a file was chosen; out contains path
        bool TrySave(string suggestedName, string filter, string defaultExt, out string filePath);
    }

    public interface IValidationService
    {
        // Returns empty list on success; errors otherwise
        List<string> ValidateL5XFile(XDocument doc);
        XDocument GetSchema();
    }

    public interface IFileSystem
    {
        bool FileExists(string path);
        void SaveXml(XDocument doc, string path);
        XDocument LoadXml(string path);
    }
}
