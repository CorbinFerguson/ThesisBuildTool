using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml.Linq;

namespace L5XAutomationTool
{
    public interface IMessageService
    {
        void Show(string message, string title);
        void ShowError(string message, string title);
        bool Confirm(string message, string title); // Yes/No
    }

    public interface IUserPromptService
    {
        Form OwnerForm { get; set; }
        string SelectOne(string prompt, List<string> options, string title);
        List<string> SelectMany(string prompt, List<string> options, string title);
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
        String ChooseParentFor(string elementType, List<string> candidateParents);

        void ClashResolution(IEnumerable<XElement> clashingElements, XElement element);
    }
}
