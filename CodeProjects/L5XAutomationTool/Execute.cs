using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Linq;

namespace L5XAutomationTool
{
    public class Execute
    {
        #region Fields
        private readonly XMLHandler _xml;
        private readonly IMessageService _messages;
        private readonly IUserPromptService _prompts;
        private readonly IOpenFileService _openFile;
        private readonly ISaveFileService _saveFile;
        private readonly IValidationService _validation;
        private readonly IFileSystem _fs;

        public static XDocument Doc = new XDocument();
        private static readonly string outputPath = "../../../L5XFiles/GeneratedFiles/";
        private static string outputName = "GenFile";

        public Execute(XMLHandler xml, IMessageService messages, IUserPromptService prompts, IOpenFileService openFile, ISaveFileService saveFile, IValidationService validation, IFileSystem fs)
        {
            _xml = xml;
            _messages = messages;
            _prompts = prompts;
            _openFile = openFile;
            _saveFile = saveFile;
            _validation = validation;
            _fs = fs;
        }


        #endregion

        #region Functions

        public void SetOwner(Form owner)
        {
            _prompts.OwnerForm = owner;
        }

        public void InitializeNew()
        {
            Doc = _xml.LoadBasicFile();
        }

        public void ValidateFile(bool showNoError = true)
        {
            // Validate the document against the xml Schema, if successful, complete transaction
            List<string> errorList = _validation.ValidateL5XFile(Doc);
            string errors = string.Join(Environment.NewLine, errorList);

            if (errors.Length > 0)
            {
                _messages.Show(errors, "Detected Errors");
            }
            else if (showNoError)
            {
                _messages.Show("No errors detected!", "Detected Errors");
            }
        }

        public void NewFile()
        {
            bool ok = _messages.Confirm("Are you sure you want to overwrite your existing file?", "Verify File Creation");
            if (ok)
            {
                Doc = _xml.LoadBasicFile();
                _messages.Show("New Empty File Created.", "Info");
            }
        }

        public void SaveFile()
        {
            // Save the document to a file
            for (int i = 0; File.Exists(outputPath + outputName + ".L5X"); i++)
                outputName = Regex.Replace(outputName, @"\d", string.Empty) + i.ToString();

            string filter = " L5X Files(*.L5X)|*.L5X|XML Files(*.XML)|*.xml|All Files(*.*)|*.*";
            if (_saveFile.TrySave(outputName, filter, "L5X", out string filePath))
                _fs.SaveXml(Doc, filePath);
        }

        public void LoadFile()
        {
            // Select File being loaded to modifiy
            if (_openFile.TryOpen("L5x Files(*.L5x)|*.L5X", null, out string filePath))
                Doc = _fs.LoadXml(filePath);
        }

        public void ModifyElement()
        {
            // Select Type from current document
            List<string> types = _xml.GetElementTypes(Doc);
            string typeSelected = _prompts.SelectOne("Select Element Type", types, "Modify Element");

            // Modules use a different attribute to search elements
            string attributeKey = (typeSelected == "Module") ? "CatalogNumber" : "Name";

            // Search for element
            List<string> namesAvailable = Doc.Descendants(typeSelected).Select(i => i.Attribute(attributeKey)?.Value.ToString()).Distinct().ToList();

            // Let user select elements to modify
            List<string> namesSelected = _prompts.SelectMany("Select names of elements to modify", namesAvailable);

            _xml.inputFile = Doc;

            IEnumerable<XElement> elements = ResolveElementFromFile(typeSelected, namesSelected);
            foreach (XElement element in elements)
            {
                List<XAttribute> attrToSet = _xml.GetAttributes(element);
                SetAttributes(element, attrToSet);
            }

            ValidateFile(false);
        }

        public void DeleteElement()
        {
            List<string> types = _xml.GetElementTypes(Doc);
            string typeSelected = _prompts.SelectOne("Select Element Type", types, "Delete Element");

            List<string> namesAvailable = Doc.Descendants(typeSelected).Select(i => i.Attribute("Name")?.Value.ToString() ?? i.Attribute("CatalogNumber").Value).ToList();

            List<string> namesSelected = _prompts.SelectMany("Select names of elements to delete", namesAvailable);

            _xml.inputFile = Doc;

            IEnumerable<XElement> removeElement = ResolveElementFromFile(typeSelected, namesSelected);
            removeElement.Remove();

            ValidateFile(false);
        }

        // Function for taking in an element from a file
        public void ImportElement()
        {
            // Prompt user to select file
            bool picked = _openFile.TryOpen("L5X Files (*.L5X)|*.L5X", "../", out string filePath);

            if (!picked || String.IsNullOrWhiteSpace(filePath))
                return; // canceled

            // Load File being imported from
            _xml.inputFile = _fs.LoadXml(filePath);

            // While loop to contain importing elements from chosen file
            bool insertElement = true;
            while (insertElement)
            {
                // Pick element type to import
                List<string> types = _xml.GetElementTypes(_xml.inputFile);
                string typeSelected = _prompts.SelectOne("Select Element Type", types, "Import Element");

                string attributeFilter = "Name";

                if (typeSelected.Equals("Module"))
                    attributeFilter = "CatalogNumber";

                // Find and display available elements of that type
                List<string> availableElements = _xml.inputFile.Descendants(typeSelected).Select(i => i.Attribute(attributeFilter)?.Value.ToString()).Distinct().ToList();
                List<string> chosenIds = _prompts.SelectMany("Select elements to insert", availableElements);

                List<XElement> returnedElement = ResolveElementFromFile(typeSelected, chosenIds);
                bool retry = false;
                do
                {
                    try
                    {
                        // Get the selected element, then insert it
                        Doc = _xml.InsertElement(Doc, returnedElement);
                        retry = false;
                    }
                    catch (ParentMissingException ex)
                    {
                        List<XAttribute> requiredAttr = new List<XAttribute>();
                        foreach (XElement schemAttr in ex.missingSchemaAttributes)
                        {
                            string attributeName = schemAttr.Attribute("name").Value;
                            string displayName = (ex.parentNode.Attribute("Name") != null) ? ex.parentNode.Attribute("Name").Value : (ex.parentNode.Attribute("CatalogNumber") != null ? ex.parentNode.Attribute("CatalogNumber").Value : ex.parentNode.Name.ToString());

                            string currentVal = ex.parentNode.Attribute(attributeName) != null ? ex.parentNode.Attribute(attributeName).Value : "";
                            string newVal = _prompts.Prompt("Input a value for " + attributeName + " attribute of auto-generated " + displayName, currentVal, null, "Modify Element");

                            ex.parentNode.SetAttributeValue(attributeName, newVal);
                        }
                        // Remove all elements in returned elements up to that broken element
                        int indexElement = returnedElement.FindIndex(el => ex.parentNode.Descendants(typeSelected).Any(deep => XNode.DeepEquals(el, deep)));
                        returnedElement.RemoveRange(0, indexElement + 1);

                        Doc = _xml.InsertElement(Doc, ex.parentNode);

                        // Continue
                        retry = true;
                    }
                } while (retry);

                // Reset the helper
                _xml.ElementInfo.ResetElements();

                ValidateFile(false);

                insertElement = _messages.Confirm("Add another element from this file?", "Import Element");
            }
        }

        public void CreateElement()
        {
            // Use template as input file
            _xml.inputFile = XDocument.Load("../../../L5XFiles/TemplateFiles/TemplateProjectV1.L5X");

            // Pick element type to import
            List<string> types = _xml.GetElementTypes(_xml.inputFile);
            string typeSelected = _prompts.SelectOne("Select Element Type", types, "Create Element");

            string attributeFilter = "Name";

            if (typeSelected.Equals("Module"))
                attributeFilter = "CatalogNumber";

            // Find and display template elements of that type
            List<string> availableElements = _xml.inputFile.Descendants(typeSelected).Select(i => i.Attribute(attributeFilter)?.Value.ToString()).Distinct().ToList();
            string chosenId = _prompts.SelectOne("Select element template", availableElements, "Create Element");

            // Get the selected element, then insert it
            XElement element = ResolveElementFromFile(typeSelected, chosenId);

            // I/O Modules need the Port address to exist but be different than any other I/O modules under the parent module
            if (element.Name.ToString().Equals("Module"))
            {
                if (element.Descendants("Port").Where(i => i.Attribute("Type").Value.Equals("ICP")).Any())
                {
                    int portNum = Doc.Descendants("Module").Where(i => i.Attribute("ParentModule").Value.Equals(element.Attribute("ParentModule").Value)).Count() + 1;
                    if (Doc.Descendants("Module").Where(i => i.Attribute("Name")?.Value.Equals(element.Attribute("ParentModule").Value) ?? false).Descendants("Port").Any())
                        portNum++;
                    element.Descendants("Port").Single(i => i.Attribute("Type").Value.Equals("ICP")).Attribute("Address").SetValue(portNum);
                }
                if (element.Descendants("Port").Where(i => i.Attribute("Type").Value.Equals("Ethernet") && i.Attribute("Address") != null).Any() && element.Attribute("ParentModule").Value.Equals("Local"))
                {
                    // Get all already used IP addresses
                    List<string> takenIPs = Doc.Descendants("Module").Where(i => i.Attribute("ParentModule")?.Value.Equals("Local") ?? false).Descendants("Port").Where(i => i.Attribute("Type")?.Value.ToString().Equals("Ethernet") ?? false).Select(i => i.Attribute("Address").Value.ToString()).ToList();

                    // Prompt user for ethernet address or hostname value
                    string ipRegex = @"^((25[0-5]|(2[0-4]|1\d|[1-9]|)\d)\.?\b){4}$|^HostName$";
                    string chosenIP = element.Descendants("Port").Single(i => i.Attribute("Type").Value.ToString().Equals("Ethernet")).Attribute("Address")?.Value ?? "192.168.1.1";
                    while (takenIPs.Contains(chosenIP))
                    {
                        _prompts.Prompt("Input User IP Address or 'HostName' for " + element.Attribute("Name")?.Value ?? element.Attribute("CatalogNumber").Value, "192.168.1.1", ipRegex, "IP Value");
                    }
                    // Set the value of IP
                    element.Descendants("Port").Single(i => i.Attribute("Type").Value.Equals("Ethernet")).Attribute("Address").SetValue(chosenIP);
                }
            }

            // Get the names of all valid parents for the element
            XDocument schema = _validation.GetSchema();

            IEnumerable<XElement> ambiguousElements = schema.Descendants(_xml.Ns + "element").Where(i => i.Attribute("name")?.Value.Equals(typeSelected + "s") ?? false);
            List<string> parentSchemaType = schema.Descendants().Where(i => ambiguousElements.Select(x => x.Parent.Parent.Attribute("name")?.Value.ToString()).ToList()?.Contains(i.Attribute("name")?.Value) ?? false).Select(i => i.Attribute("name").Value).ToList();
            List<string> parentType = schema.Descendants(_xml.Ns + "element").Where(i => parentSchemaType.Contains(i.Attribute("type")?.Value)).Select(i => i.Attribute("name").Value).ToList();

            if (parentType.Count() > 1)
            {
                IEnumerable<XElement> parentElems = Doc.Descendants().Where(i => parentType.Contains(i.Name?.ToString()));

                // Get all valid parents of available types
                List<string> parents = parentElems.Select(i => i.Attribute("Name").Value.ToString()).ToList();

                // Prompt the user for the parent
                string parentName = _prompts.SelectOne("Select parent for the elements being created", parents, "Create Element");

                _xml.ElementInfo.ParentElementBulk = parentElems.Single(i => i.Attribute("Name")?.Value.Equals(parentName) ?? false);
            }
            else if (parentType.Count() != 1)
            {
                throw new EmptyListException("Attempted to Create an item with no valid parents");
            }

            // Prompt user to select the subelements/values to change(required elements are not selectable)
            string itemQuantity = _prompts.Prompt("How many " + element.Name + " would you like to create?", "1", @"^[1-9]\d*$", "Create Element");

            int quantity = int.Parse(itemQuantity);

            // Create and insert elements
            List<string> bulkNames = new List<string>();
            for (int i = 0; i < quantity; i++)
            {
                if (typeSelected != "Module" || element.Attribute("Use") == null)
                {
                    // Prompt user for name of item and assign it
                    string itemName = _prompts.Prompt("Input a name for created " + element.Name + " #" + (i + 1).ToString(), "", @"^[a-zA-Z]+(\w*[A-Za-z0-9])*$", "Create Element");
                    element.SetAttributeValue("Name", itemName);
                    bulkNames.Add(itemName);
                }

                // I/O Modules need the Port address to exist but be different than any other I/O modules under the parent module
                if (element.Name.ToString().Equals("Module"))
                {
                    if (element.Descendants("Port").Where(el => el.Attribute("Type").Value.Equals("ICP")).Any())
                    {
                        int portNum = Doc.Descendants("Module").Where(el => el.Attribute("ParentModule").Value.Equals(element.Attribute("ParentModule").Value)).Count() + 1;
                        if (Doc.Descendants("Module").Where(el => el.Attribute("Name")?.Value.Equals(element.Attribute("ParentModule").Value) ?? false).Descendants("Port").Any())
                            portNum++;
                        element.Descendants("Port").Single(el => el.Attribute("Type").Value.Equals("ICP")).Attribute("Address").SetValue(portNum);
                    }
                    if (element.Descendants("Port").Where(el => el.Attribute("Type").Value.Equals("Ethernet") && el.Attribute("Address") != null).Any() && element.Attribute("ParentModule").Value.Equals("Local"))
                    {
                        // Get all already used IP addresses
                        List<string> takenIPs = Doc.Descendants("Module").Where(el => el.Attribute("ParentModule")?.Value.Equals("Local") ?? false).Descendants("Port").Where(el => el.Attribute("Type")?.Value.ToString().Equals("Ethernet") ?? false).Select(el => el.Attribute("Address").Value.ToString()).ToList();

                        // Prompt user for ethernet address or hostname value
                        string ipRegex = @"^((25[0-5]|(2[0-4]|1\d|[1-9]|)\d)\.?\b){4}$|^HostName$";
                        string chosenIP = element.Descendants("Port").Single(el => el.Attribute("Type").Value.ToString().Equals("Ethernet")).Attribute("Address")?.Value ?? "192.168.1.1";
                        while (takenIPs.Contains(chosenIP))
                        {
                            _prompts.Prompt("Input User IP Address or 'HostName' for " + element.Attribute("Name")?.Value ?? element.Attribute("CatalogNumber").Value, "192.168.1.1", ipRegex, "IP Value");
                        }
                        // Set the value of IP
                        element.Descendants("Port").Single(el => el.Attribute("Type").Value.Equals("Ethernet")).Attribute("Address").SetValue(chosenIP);
                    }
                }

                // Insert the element and reset the path for the next one
                bool retry = false;
                do
                {
                    try
                    {
                        Doc = _xml.InsertElement(Doc, element);
                        retry = false;
                    }
                    catch (ClashingElementException clashEx)
                    {
                        retry = HandleClashes(clashEx.clashingElements, clashEx.parentNode, element);
                    }
                } while (retry);
                _xml.ElementInfo.RootPath.Clear();
            }
            _xml.ElementInfo.ResetElements();

            // Programs have special case for having a parent task or being unassigned
            if (typeSelected.Equals("Program"))
            {
                // Get a list of all tasks in the document currently
                IEnumerable<XElement> tasks = Doc.Descendants("Task");

                if (tasks.Any())
                {
                    // Get list of names
                    List<string> taskNames = tasks.Select(i => i.Attribute("Name").Value).ToList();
                    taskNames.Add("Keep programs unscheduled");

                    // Prompt user for which task to assign the programs to
                    string taskName = _prompts.SelectOne("Select the parent task for inserted program", taskNames, "Create Element");

                    if (taskName != "Keep programs unscheduled")
                    {
                        // Get parent task
                        XElement taskElement = tasks.Single(i => i.Attribute("Name").Value.Equals(taskName));
                        XElement scheduledParent = null;
                        if (taskElement.Elements("ScheduledPrograms").Any())
                        {
                            scheduledParent = taskElement.Elements("ScheduledPrograms").Single();
                        }
                        else
                        {
                            scheduledParent = new XElement("ScheduledPrograms");
                        }

                        // Add the program names to the chosen tasks scheduled programs
                        foreach (string programName in bulkNames)
                        {
                            XElement scheduledProgram = new XElement("ScheduledProgram", new XAttribute("Name", programName));
                            scheduledParent.Add(scheduledProgram);
                        }

                        if (!taskElement.Elements("ScheduledPrograms").Any())
                        {
                            taskElement.Add(scheduledParent);
                        }
                    }
                }
            }

            // Validate silently
            ValidateFile(false);
        }

        private XElement ResolveElementFromFile(string typeOfElement, string id)
        {
            string attributeFilter = "Name";
            if (typeOfElement == "Module")
                attributeFilter = "CatalogNumber";

            IEnumerable<XElement> candidates = _xml.inputFile.Descendants(typeOfElement).Where(i => i.Attribute(attributeFilter) != null && i.Attribute(attributeFilter).Value == id);
            if (candidates.Count() == 0)
            {
                // No element with that ID, skip
                return null;
            }
            else if (candidates.Count() == 1)
            {
                // Only one element found, no ambiguity
                return candidates.Single();
            }

            // Multiple elements with same Name -> disambiguate by grandparent Name
            List<string> parentOptions = _xml.inputFile.Descendants(typeOfElement).Where(i => i.Attribute(attributeFilter).Value.Equals(id)).Select(i => i.Parent.Parent.Attribute("Name").Value.ToString()).Distinct().ToList();


            string grandparentName = _prompts.SelectOne("Multiple elements named '" + id + "'. Select parent element you are accessing", parentOptions, "Import Element");
            XElement parentElement = _xml.inputFile.Descendants().Single(i => i.Attribute("Name") != null && i.Attribute("Name").Value == grandparentName);
            XElement resolved = parentElement.Descendants(typeOfElement).Single(i => i.Attribute("Name") != null && i.Attribute("Name").Value == id);

            return resolved;
        }

        private List<XElement> ResolveElementFromFile(string typeOfElement, List<string> identifiers)
        {
            string attributeFilter = "Name";
            if (typeOfElement == "Module")
                attributeFilter = "CatalogNumber";

            List<XElement> elements = new List<XElement>();

            foreach (string id in identifiers)
            {
                if (typeOfElement == "Module")
                {
                    // Get all valid options. If there is no name, use port number instead
                    List<string> parentOptions = new List<string>();
                    IEnumerable<XElement> candidates = _xml.inputFile.Descendants(typeOfElement).Where(i => i.Attribute(attributeFilter) != null && i.Attribute(attributeFilter).Value == id);

                    foreach (XElement elem in candidates)
                    {

                        XElement firstPort = elem.Descendants("Port").FirstOrDefault();
                        string portAddr = (firstPort != null && firstPort.Attribute("Address") != null) ? firstPort.Attribute("Address").Value : "?";
                        parentOptions.Add(id + " with no Name at port " + portAddr);

                        List<string> chosen = _prompts.SelectMany("Select specific Module(s) of type " + id, parentOptions);

                        foreach (string nameOfElement in chosen)
                        {
                            XElement element;
                            // Using port to differentiate
                            if (nameOfElement.Contains("at port"))
                            {
                                int portIndex = nameOfElement.IndexOf("port ") + 5;
                                string selectedPort = nameOfElement.Substring(portIndex);
                                element = candidates.Single(el => el.Descendants("Port")?.Where(j => j.Attribute("Address").Value.ToString().Equals(selectedPort)).Any() ?? false);
                            }
                            // Using name to differentiate
                            else
                            {
                                element = candidates.Single(el => el.Attribute("Name")?.Value.ToString().Equals(nameOfElement) ?? false);
                            }
                            elements.Add(element);
                        }
                    }
                }
                else
                {
                    elements.Add(ResolveElementFromFile(typeOfElement, id));
                }
            }
            return elements;
        }

        private void SetAttributes(XElement element, List<XAttribute> attributesToChange)
        {
            List<string> selectedAttributeNames = _prompts.SelectMany("Select attributes to manually set value. (NO INPUT VALIDATION. USE WITH CAUTION)", attributesToChange.Select(i => i.Name.ToString()).ToList());

            foreach (string attributeName in selectedAttributeNames)
            {

                // so default setter below won't overwrite manual input
                for (int i = attributesToChange.Count - 1; i >= 0; i--)
                {
                    if (attributesToChange[i].Name.ToString() == attributeName)
                    {
                        attributesToChange.RemoveAt(i);
                    }
                }

                string displayName = (element.Attribute("Name") != null) ? element.Attribute("Name").Value : (element.Attribute("CatalogNumber") != null ? element.Attribute("CatalogNumber").Value : element.Name.ToString());

                string currentVal = element.Attribute(attributeName) != null ? element.Attribute(attributeName).Value : "";
                string newVal = _prompts.Prompt("Input a value for " + attributeName + " attribute of " + displayName, currentVal, null, "Modify Element");

                element.SetAttributeValue(attributeName, newVal);
            }

            // Set Default Values
            foreach (XAttribute setDefaultAttribute in attributesToChange)
            {
                if (element.Attribute(setDefaultAttribute.Name) != null)
                    element.Attribute(setDefaultAttribute.Name).SetValue(setDefaultAttribute.Value);
                else if (!setDefaultAttribute.Value.Equals("") && element.Attribute(setDefaultAttribute.Name) == null)
                    element.Add(setDefaultAttribute);
            }

            // Prompt user to select any children to modify
            IEnumerable<XElement> childElements = element.Elements();
            if (childElements.Any())
            {
                List<string> childNames = childElements.Select(i => i.Attribute("Name")?.ToString() ?? i.Name.ToString()).Distinct().ToList();
                List<string> selectedChildren = _prompts.SelectMany("Select children elements to modify (hit confirm with none selected or X to skip this step)", childNames, true);

                // Access the elements selected and modify them recursively
                childElements = childElements.Where(i => selectedChildren.Contains(i.Attribute("Name")?.ToString() ?? i.Name.ToString())).Elements();
                List<List<XAttribute>> attributes = _xml.GetAttributes(childElements);
                SetAttributes(childElements, attributes);
            }
        }

        internal void SetAttributes(IEnumerable<XElement> elements, List<List<XAttribute>> attrs)
        {
            foreach (var (element, attr) in elements.Zip(attrs, (n, p) => (n, p)))
                SetAttributes(element, attr);
        }

        internal bool HandleClashes(IEnumerable<XElement> clashingElements, XElement parentNode, XElement insertElement = null)
        {
            string attributeFilter = "Name";
            if (parentNode.Name.Equals("Module"))
                attributeFilter = "CatalogNumber";
            bool retry = true;

            // Clash Resolution
            List<string> actionOps = new List<string>() { "Cancel", "Replace", "Rename" };
            string selected = _prompts.SelectOne($"Element already exists. What would you like to change for {insertElement.Attribute(attributeFilter).Value}?", actionOps, "Clashing Elements");
            switch (selected)
            {
                case "Replace":
                    // Replace the already existing element
                    insertElement = clashingElements.Single();
                    clashingElements.Remove();
                    break;
                case "Rename":
                    string renameElement;
                    // Rename the element being inserted to not clash with existing element

                    string newAtrVal = insertElement.Attribute(selected)?.Value.ToString() ?? "";
                    do
                    {
                        if (attributeFilter == "Name")
                        {
                            renameElement = _prompts.Prompt($"Element {insertElement.Attribute(attributeFilter).Value} already exists. Input a new {selected}.", insertElement.Attribute(selected)?.Value ?? "", "", "New Name");
                        }
                        else
                        {
                            renameElement = _prompts.Prompt($"Element {insertElement.Attribute(attributeFilter).Value} already exists. Input a new {selected}.", insertElement.Attribute(selected)?.Value ?? "", @"^\d+\.\d$", $"New {attributeFilter}");
                        }
                    }
                    while (parentNode.Descendants(insertElement.Name).Where(el => renameElement.ToLower().Equals(el.Attribute(attributeFilter)?.Value.ToLower().ToString())).Any());

                    insertElement.SetAttributeValue(attributeFilter, renameElement);
                    break;
                default:
                    // Cancel insertion
                    insertElement.Remove();
                    retry = false;
                    break;
            }
            _xml.ElementInfo.RootPath.Clear();
            return retry;
        }

        #endregion
    }
}
