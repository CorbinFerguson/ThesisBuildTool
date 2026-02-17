using L5XAutomationTool.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace L5XAutomationTool.GUIAccessors
{
    public sealed class MessageService : IMessageService
    {
        public void Show(string message, string title)
        {
            MessageBox.Show(message, title ?? "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void ShowError(string message, string title)
        {
            MessageBox.Show(message, title ?? "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public bool Confirm(string message, string title)
        {
            return MessageBox.Show(message, title ?? "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }
    }

    public sealed class OpenFileService : IOpenFileService
    {
        public bool TryOpen(string filter, string initialDirectory, out string filePath)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = filter;
                ofd.FilterIndex = 0;
                ofd.RestoreDirectory = true;
                if (!string.IsNullOrWhiteSpace(initialDirectory))
                    ofd.InitialDirectory = initialDirectory;

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    filePath = ofd.FileName;
                    return true;
                }
            }
            filePath = string.Empty;
            return false;
        }
    }

    public sealed class SaveFileService : ISaveFileService
    {
        public bool TrySave(string suggestedName, string filter, string defaultExt, out string filePath)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = filter;
                sfd.FileName = suggestedName;
                sfd.DefaultExt = defaultExt;

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    filePath = sfd.FileName;
                    return true;
                }
            }
            filePath = string.Empty;
            return false;
        }
    }

    public sealed class UserPromptService : IUserPromptService
    {
        public Form OwnerForm { get; set; }
        public string SelectOne(string prompt, List<string> options, string title)
        {
            var dlg = new DropdownGui(options.ToList(), prompt);
            dlg.ShowDialog(OwnerForm, out string selected);
            return selected;
        }

        public List<string> SelectMany(string prompt, List<string> options, string title)
        {
            var dlg = new MultiSelectDropdown(options.ToList(), prompt);
            dlg.ShowDialog(OwnerForm, out List<string> selected);
            return selected;
        }

        public string Prompt(string prompt, string defaultValue, string regex, string title)
        {
            var dlg = new TextInput(prompt, defaultValue, regex, title);
            dlg.ShowDialog(OwnerForm, out string value);
            return value;
        }
    }

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

    public sealed class SchemaDisambiguator : ISchemaDisambiguator
    {
        private readonly IUserPromptService _prompts;
        public SchemaDisambiguator(IUserPromptService prompts)
        {
            _prompts = prompts;
        }

        public string ChooseParentFor(string elementType, List<string> candidateParents)
        {
            return _prompts.SelectOne("Select a parent type for " + elementType, candidateParents, "Resolve Parent");
        }

        public void SetAttributeValues(IEnumerable<XElement> requiredAttributes, XElement element)
        {
            foreach (XElement requiredAttribute in requiredAttributes)
            {
                string attributeValue = _prompts.Prompt($"Input user value for {requiredAttribute.Attribute("name").Value}", "", "", "Set Value");
                element.SetAttributeValue(requiredAttribute.Attribute("name").Value, attributeValue);
            }
        }

        public void ClashResolution(IEnumerable<XElement> clashingElements, XElement element)
        {
            string searchFilter = "Name";
            if (element.Name.ToString().Equals("Module") && element.Attribute("Name") == null)
                searchFilter = "CatalogNumber";

            List<string> actionOps = new List<string>() { "Cancel", "Replace", "Name" };
            string selected = _prompts.SelectOne($"Element already exists. What would you like to change for {element.Attribute(searchFilter).Value}?", actionOps, "Conflict Resolution");

            switch (selected)
            {
                case "Replace":
                    // Replace the already existing element
                    clashingElements.Single();
                    break;
                case "Name":
                    string renameHeader;
                    string renameDefault;
                    string renameReg = "";
                    // Rename the element being inserted to not clash with existing element
                    if (selected == "Name")
                    {
                        renameHeader = $"Element {element.Attribute(searchFilter).Value} already exists. Input a new {selected}.";
                        renameDefault = element.Attribute(selected)?.Value ?? "";
                    }
                    else
                    {
                        renameHeader = $"Element {element.Attribute(searchFilter).Value} already exists. Input a new {selected}.";
                        renameDefault = element.Attribute(selected)?.Value ?? "";
                        renameReg = @"^\d+\.\d$";
                    }

                    // Prompt user for new value, then set it
                    string newAtrVal = element.Attribute(selected)?.Value.ToString() ?? "";
                    while (element.Parent.Descendants(element.Name).Where(i => newAtrVal.ToLower().Equals(i.Attribute(selected)?.Value.ToLower().ToString())).Any())
                        newAtrVal = _prompts.Prompt(renameHeader, renameDefault, renameReg, "Element Exists");
                    element.SetAttributeValue(selected, newAtrVal);
                    break;
                default:
                    // Cancel insertion
                    Console.WriteLine("Canceling Insertion");
                    break;
            }
        }
    }
}