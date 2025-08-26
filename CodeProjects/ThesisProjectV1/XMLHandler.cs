using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using ThesisProjectV1.Forms;

namespace ThesisProjectV1
{
    public class XMLHandler
    {
        #region Variables
        private readonly string processorType = "1756-L81E";

        private readonly string projectName = "GenProject";
        private readonly Validate validator = new Validate();

        public XNamespace Ns { get; } = XNamespace.Get(@"http://www.w3.org/2001/XMLSchema");

        #endregion

        #region Constructors
        #endregion

        #region functions
        // Outdated document creation function //TODO: update
        public void CreateBasicDocument()
        {
            XDocument doc = new XDocument();

            // RSLogix5000Content
            XAttribute[] logixContentAttr =
            {
                new XAttribute("SchemaRevision", "1.0"),
                new XAttribute("SoftwareRevision", "32.01"),
                new XAttribute("TargetName",projectName),
                new XAttribute("TargetType","Controller"),
                new XAttribute("ContainsContext",false),
                new XAttribute("ExportDate",DateTime.Now),
                new XAttribute("ExportOptions","NoRawData L5KData DecoratedData ForceProtectedEncoding AllProjDocTrans")
            };
            XElement logixContent = new XElement("RSLogix5000Content", logixContentAttr);
            doc.Add(logixContent);

            #region RSLogix5000Content Subelements

            // Controller
            XAttribute[] controllerAttributes = {
                new XAttribute("Use", "Target"),
                new XAttribute("Name", projectName),
                new XAttribute("ProcessorType", this.processorType),
                new XAttribute("MajorRev", "32"),
                new XAttribute("MinorRev", "11"),
                new XAttribute("ProjectCreationDate", DateTime.Now),
                new XAttribute("LastModifiedDate", DateTime.Now),
                new XAttribute("SFCExecutionControl", "CurrentActive"),
                new XAttribute("SFCRestartPosition", "MostRecent"),
                new XAttribute("SFCLastScan", "DontScan"),
                new XAttribute("ProjectSN", "16#0000_0000"),
                new XAttribute("MatchProjectToController", "false"),
                new XAttribute("CanUseRPIFromProducer", "false"),
                new XAttribute("InhibitAutomaticFirmwareUpdate", "0"),
                new XAttribute("PassThroughConfiguration", "EnabledWithAppend"),
                new XAttribute("DownloadProjectDocumentationAndExtendedProperties", "true"),
                new XAttribute("DownloadProjectCustomProperties", "true"),
                new XAttribute("ReportMinorOverflow", "false")
            };
            XElement controller = new XElement("Controller", controllerAttributes);
            logixContent.Add(controller);

            #region ControllSubElements
            //Redundancy Info
            XAttribute[] redundancyAttributes = {
                    new XAttribute("Enabled", "false"),
                    new XAttribute("KeepTestEditsOnSwitchOver", "false")
                };
            XElement redundancyInfo = new XElement("RedundancyInfo", redundancyAttributes);
            controller.Add(redundancyInfo);

            //Security
            XAttribute[] securityAttributes = {
                    new XAttribute("Code", "0"),
                    new XAttribute("ChangesToDetect", "16#ffff_ffff_ffff_ffff")
                };
            XElement security = new XElement("Security", securityAttributes);
            controller.Add(security);

            //Safety
            XAttribute[] safetyInfoAttributes = {
                };
            XElement safetyInfo = new XElement("SafetyInfo", safetyInfoAttributes);
            controller.Add(safetyInfo);

            //DataTypes
            XAttribute[] dataTypesAttributes = {
                };
            XElement dataTypes = new XElement("DataTypes", dataTypesAttributes);
            controller.Add(dataTypes);

            //Modules
            XAttribute[] modulesAttributes = {
                };
            XElement modules = new XElement("Modules", modulesAttributes);
            controller.Add(modules);

            //AOI
            XAttribute[] addOnInstructionsAttributes = {
                };
            XElement addOnInstructions = new XElement("AddOnInstructionDefinitions", addOnInstructionsAttributes);
            controller.Add(addOnInstructions);

            //Tags
            XAttribute[] tagsAttributes = {
                };
            XElement tags = new XElement("Tags", tagsAttributes);
            controller.Add(tags);

            //Programs
            XAttribute[] programsAttributes = {
                };
            XElement programs = new XElement("Programs", programsAttributes);
            controller.Add(programs);

            //Tasks
            XAttribute[] tasksAttributes = {
                };
            XElement tasks = new XElement("Tasks", tasksAttributes);
            controller.Add(tasks);

            //CST
            XAttribute[] cstAttributes = {
                    new XAttribute("MasterID", "0")
                };
            XElement cst = new XElement("CST", cstAttributes);
            controller.Add(cst);

            //WallClockTime
            XAttribute[] wallClockTimeAttributes = {
                    new XAttribute("LocalTimeAdjustment", "0"),
                    new XAttribute("TimeZone", "0")
                };
            XElement wallClockTime = new XElement("WallClockTime", wallClockTimeAttributes);
            controller.Add(wallClockTime);

            //Trends
            XAttribute[] trendsAttributes = {
                };
            XElement trends = new XElement("Trends", trendsAttributes);
            controller.Add(trends);

            //DataLogs
            XAttribute[] dataLogsAttributes = {
                };
            XElement dataLogs = new XElement("DataLogs", dataLogsAttributes);
            controller.Add(dataLogs);

            //TimeSync
            XAttribute[] timeSynchronizeAttributes = {
                    new XAttribute("Priority1", "128"),
                    new XAttribute("Priority2", "128"),
                    new XAttribute("PTPEnable", "false")
                };
            XElement timeSynchronize = new XElement("TimeSynchronize", timeSynchronizeAttributes);
            controller.Add(timeSynchronize);

            //EthernetPorts
            XElement ethernetPorts = new XElement("EthernetPorts", null);
            controller.Add(ethernetPorts);

            #endregion

            #endregion

            // Save the document to a file
            string genFilePath = "../../../L5XFiles/EmptyGenFile.L5X";
            doc.Save(genFilePath);
        }

        public XDocument CheckForDependencies(XDocument docToInsert, string elementFilepath, XElement element)
        {
            XDocument docForElement = XDocument.Load(elementFilepath);
            // Check if there are any dependencies in the inserted element
            if (element.Attributes("Dependencies") != null)
            {
                List<XElement> dependencies = element.Descendants("Dependencies").Elements().ToList();
                foreach (XElement dependency in dependencies)
                {
                    XElement dependentElement = docForElement.Descendants(dependency.Attribute("Type").Value).Where(i => i.Attribute("Name").Value.Equals(dependency.Attribute("Name").Value)).Single();
                    docToInsert = this.InsertElement(docToInsert, dependentElement);
                }
            }
            return docToInsert;
        }

        public XDocument CheckForDependencies(XDocument docToInsert, string elementFilepath, List<XElement> elements)
        {
            foreach (XElement element in elements)
                docToInsert = this.CheckForDependencies(docToInsert, elementFilepath, element);
            return docToInsert;
        }

        // Returns a list of the names for the nodes leading from the root(RSLogix5000) to element
        public Queue<string> FindPathtoRootSchema(XElement element)
        {
            Queue<string> paths = new Queue<string>();
            string attributeFilter = null;

            string name = element.Name.ToString();
            XElement schemaElement = null;

            try
            {
                attributeFilter = "name";
                schemaElement = validator.GetSchema().Descendants().Where(i => i.Attribute(attributeFilter) != null).Where(i => i.Name.Equals(Ns + "element")).Where(i => i.Attribute(attributeFilter).Value.Equals(name)).Single();

                // Loop until RSLogix5000Content(root of L5X) is found
                while (!schemaElement.Attribute("name").Value.Equals("RSLogix5000Content"))
                {
                    // Go to parent complex type, find name of that
                    attributeFilter = "name";
                    name = schemaElement.Parent.Parent.Attribute(attributeFilter).Value;

                    // search for something with that type
                    attributeFilter = "type";
                    schemaElement = validator.GetSchema().Descendants().Where(i => i.Attribute(attributeFilter) != null).Where(i => i.Name.Equals(Ns + "element")).Where(i => i.Attribute(attributeFilter).Value.Equals(name)).Single();

                    paths.Enqueue(schemaElement.Attribute("name").Value);
                }
                if (paths.Count == 0)
                    throw new EmptyListException("Empty path to root. Started at RSLogix5000Content");
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("Sequence contains more than one element"))
                {

                    // Create a popup telling user what happened
                    MessageBox.Show("Multiple options for parent of " + element.Attribute("Name").Value + ", please select intended grandparent type", ex.Message, MessageBoxButtons.OK);
                    IEnumerable<XElement> ambiguousElements = validator.GetSchema().Descendants(Ns + "element").Where(i => i.Attribute(attributeFilter) != null).Where(i => i.Attribute(attributeFilter).Value.Equals(name));

                    List<string> parentNames = new List<string>();
                    parentNames = validator.GetSchema().Descendants().Where(i => i.Attribute(attributeFilter) != null).Where(i => ambiguousElements.Select(x => x.Parent.Parent.Attribute("name").Value.ToString()).ToList().Contains(i.Attribute(attributeFilter).Value)).Select(i => i.Attribute("name").Value).ToList();

                    // Prompt user to select grandparent for the element
                    DropdownGui selectElement = new DropdownGui(parentNames, "Select intended grandparent for " + element.Name + ": " + element.Attribute("Name").Value);
                    selectElement.ShowDialog(out string nameOfElement);
                    string disambiguousParent = validator.GetSchema().Descendants().Where(i => i.Attribute("type") != null && i.Attribute("type").Value.ToString().Equals(name)).Select(i => i.Attribute("name").Value).Distinct().Single().ToString();
                    paths.Enqueue(disambiguousParent);

                    // Create the path queue
                    Queue<string> rootPath = new Queue<string>();
                    // If paths is empty, use element parent
                    if (paths.Any() == false)
                        rootPath.Enqueue(element.Parent.Name.ToString());
                    // If it is not empty, use the parent of the last element in the queue
                    rootPath.Enqueue(nameOfElement);
                    Queue<string> grandparentToRoot = FindPathtoRootSchema(new XElement(nameOfElement));

                    foreach (string node in grandparentToRoot)
                        rootPath.Enqueue(node);

                    if (rootPath.Count == 0)
                        return rootPath;
                    if (rootPath.Peek() == paths.Last())
                        rootPath.Dequeue();
                    foreach (string parent in rootPath)
                        paths.Enqueue(parent);
                    return paths;
                }
                else
                { MessageBox.Show(ex.Message + "\nStack Trace: " + ex.StackTrace, "how did you hit this", MessageBoxButtons.OK); }
            }
            catch (EmptyListException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
            return paths;
        }

        public XElement GetElementFromFile(string subElement, string filePath)
        {
            throw new NotImplementedException();
        }

        public XElement GetElementFromFile(string elementType, string filePath, string elementName)
        {
            XElement element = null;
            if (elementName.Equals(""))
                element = GetElementFromFile(elementType, filePath);
            else
            {
                XDocument itemToAddDoc = XDocument.Load(filePath);
                try
                {
                    element = itemToAddDoc.Descendants(elementType).Where(i => i.Attribute("Name") != null).Where(i => i.Attribute("Name").Value == elementName).Single();
                }
                catch (InvalidOperationException ex)
                {
                    if (ex.Message.Contains("Sequence contains more than one element"))
                    {
                        // There exists more than one element of that name
                        IEnumerable<XElement> elementsToChoose = itemToAddDoc.Descendants(elementType).Where(i => i.Attribute("Name") != null).Where(i => i.Attribute("Name").Value.Equals(elementName));
                        // Create a popup telling user what happened
                        MessageBox.Show("Ambiguous parent type for " + elementName + ", please select intended grandparent type", "Element from File Selection", MessageBoxButtons.OK);
                        List<string> parentOptions = itemToAddDoc.Descendants(elementType).Select(i => i.Parent.Parent.Name.ToString()).Distinct().ToList();
                        DropdownGui selectElement = new DropdownGui(parentOptions, "Select intended parent type");
                        selectElement.ShowDialog(out string nameOfElement);
                        try
                        {
                            element = itemToAddDoc.Descendants(nameOfElement).Single();
                        }
                        catch (InvalidOperationException)
                        {
                            MessageBox.Show("Multiple objects of chosen type", "Disambiguate Element from File Selection", MessageBoxButtons.OK);
                            List<string> typeObjects = itemToAddDoc.Descendants(nameOfElement).Select(i => i.Attribute("Name").Value.ToString()).ToList();
                            DropdownGui elementSelect = new DropdownGui(typeObjects, "Select specific object parent");
                            elementSelect.ShowDialog(out string selectedObject);
                            XElement parentElement = itemToAddDoc.Descendants().Where(i => i.Attribute("Name") != null).Where(i => i.Attribute("Name").Value.ToString().Equals(selectedObject)).Single();
                            element = parentElement.Descendants().Where(i => i.Attribute("Name") != null).Where(i => i.Attribute("Name").Value.ToString().Equals(elementName)).Single();
                        }
                    }
                }
            }
            return element;
        }

        public List<XElement> GetElementFromFile(string elementType, string filePath, List<string> elementNames)
        {
            List<XElement> elementList = new List<XElement>();
            foreach (string elementName in elementNames)
                elementList.Add(GetElementFromFile(elementType, filePath, elementName));
            return elementList;
        }

        public List<string> GetElementsOfType(string elementType, string filePath)
        {
            List<String> names = new List<String>();
            XDocument doc = XDocument.Load(filePath);
            foreach (XElement element in doc.Descendants(elementType))
                names.Add(element.Attribute("Name").Value.ToString());

            return names;
        }

        // Get all distinct types in the document. They must have a name to be addable.
        public List<string> GetDistinctTypes(string path)
        {
            XDocument doc = XDocument.Load(path);
            List<string> types = new List<string>();

            foreach (XElement type in doc.Descendants())
            {
                if (type.Attribute("Name") != null && type.Name.ToString() != "RSLogix5000Content")
                    if (!types.Contains(type.Name.ToString()))
                        types.Add(type.Name.ToString());
            }

            return types;
        }

        public string GetTypeAndSelect(string filePath)
        {
            // Prompt user to select type of element to insert
            List<string> elementTypes = this.GetDistinctTypes(filePath);
            List<string> namesOfTypesToIgnore = new List<string>()
            {
                "Controller",
                "DataValueMember",
                "StructureMember",
                "Dependency",
                "Member",
                "Trend",
                "Pen"
            };
            List<XElement> typesToIgnore = this.GetValidator().GetSchema().Descendants(Ns + "element").Where(i => i.Attribute("name") != null).Where(i => namesOfTypesToIgnore.Contains(i.Attribute("name").Value)).ToList();

            elementTypes = elementTypes.Where(name => !typesToIgnore.Any(x => (string)x.Attribute("name") == name)).ToList();

            DropdownGui selectType = new DropdownGui(elementTypes, "Select type of the element to insert");
            selectType.ShowDialog(out string typeOfElement);
            return typeOfElement;
        }

        // Gets all the simple elements in the XML Schema
        public List<String> GetSimpleElements()
        {
            IEnumerable<XElement> elements = validator.GetSchema().Descendants(Ns + "element");

            List<String> elementsInList = new List<String>();

            foreach (XElement element in elements)
            {
                if (!element.Attribute("name").Value.Equals("CustomProperties"))
                    elementsInList.Add(element.Attribute("name").Value);
            }
            return elementsInList;
        }

        internal Validate GetValidator() { return validator; }

        public XDocument InsertElement(XDocument inDoc, XElement element)
        {
            Queue<string> rootPath = FindPathtoRootSchema(element);
            XName parentName = rootPath.Dequeue();
            bool multOptions = false;
            XElement parentNode = null;

            //Ensure edit information is up to date
            XAttribute editedDate = new XAttribute("EditedDate", DateTime.Now);
            XAttribute editedBy = new XAttribute("EditedBy", "XMLGenerator");
            element.SetAttributeValue(editedDate.Name, editedDate.Value);
            element.SetAttributeValue(editedBy.Name, editedBy.Value);

            // Check that parent node exists in document using the schema
            while (!inDoc.Descendants(parentName).Any())
            {
                element = new XElement(parentName, element);

                try
                {
                    // Verify that the parent element has all required attributes
                    string complexType = validator.GetSchema().Descendants().Where(i => i.Name.Equals(Ns + "element")).Where(i => i.Attribute("name") != null).Where(i => i.Attribute("name").Value.ToString().Equals(element.Name.ToString())).Single().Attribute("type").Value;
                    XElement schemaElement = validator.GetSchema().Descendants(Ns + "complexType").Where(i => i.Attribute("name") != null).Where(i => i.Attribute("name").Value.Equals(complexType)).Single();
                    IEnumerable<XElement> requiredAttributes = schemaElement.Descendants().Where(i => i.Name.Equals(Ns + "attribute")).Where(i => i.Attribute("use") != null).Where(i => i.Attribute("use").Value.Equals("required"));
                    foreach (XElement requiredAttribute in requiredAttributes)
                    {
                        TextInput input = new TextInput($"Input user value for {requiredAttribute.Attribute("name").Value} of {parentName}", "");
                        input.ShowDialog(out string attributeValue);
                        element.SetAttributeValue(requiredAttribute.Attribute("name").Value, attributeValue);
                    }
                    parentName = rootPath.Dequeue();
                }
                catch (InvalidOperationException)
                {
                    // Multiple options for parent element, already handled in findRoot
                    parentName = rootPath.Dequeue();
                }
            }

            // Check that the element being added doesn't already exist
            IEnumerable<XElement> elementToAdd = inDoc.Descendants(element.Name).Where(i => i.Attribute("Name") != null && i.Attribute("Name").Value.Equals(element.Attribute("Name").Value));
            if (elementToAdd.Count() > 0)
            {
                if (elementToAdd.Select(i => i.Attribute("Revision").Value.ToString().Equals(element.Attribute("Revision").Value.ToString())).Contains(true))
                {
                    List<string> actionOps = new List<string>() { "Replace", "Revision Increment", "Rename", "Cancel" };
                    DropdownGui elementExists = new DropdownGui(actionOps, $"Element by that name already exists. What action would you like to take for {element.Attribute("Name").Value}?");
                    elementExists.ShowDialog(out string selected);
                    switch (selected)
                    {
                        case "Replace":
                            // Replace the already existing element
                            DropdownGui elementReplace = new DropdownGui(elementToAdd.Select(i => i.Attribute("Revision").Value.ToString()).ToList(), "Select Element revision to replace");
                            elementReplace.ShowDialog(out string replaceVersion);
                            element.Attribute("Revision").SetValue(replaceVersion);
                            inDoc.Descendants(element.Name).Where(i => i.Attribute("Revision").Value.ToString().Equals(replaceVersion) && i.Attribute("Name").Value.ToString().Equals(element.Attribute("Name").Value)).Single().ReplaceWith(element);
                            return inDoc;
                        case "Rename":
                            // Rename the element being inserted to not clash with existing element
                            TextInput renameElement = new TextInput($"Element by that name already exists. Input a new name for {element.Attribute("Name").Value}", element.Attribute("Name").Value.ToString());
                            string newName = element.Attribute("Name").Value.ToString();
                            while (newName.Equals(element.Attribute("Name").Value.ToString()))
                                renameElement.ShowDialog(out newName);
                            element.Attribute("Name").SetValue(newName);
                            inDoc = InsertElement(inDoc, element);
                            return inDoc;
                        case "Revision Increment":
                            // Increment the element being inserted to not clash
                            TextInput version = new TextInput($"Element by that name already exists. Input a new revision for {element.Attribute("Name").Value}", element.Attribute("Revision").Value.ToString());
                            string newVersion = element.Attribute("Revision").Value.ToString();
                            while (newVersion.Equals(element.Attribute("Revision").Value.ToString()))
                                version.ShowDialog(out newVersion);
                            element.Attribute("Revision").SetValue(newVersion);
                            inDoc = InsertElement(inDoc, element);
                            return inDoc;
                        default:
                            // Cancel insertion
                            return inDoc;
                    }
                }
            }

            try
            {
                parentNode = inDoc.Descendants(parentName).Single();
            }
            catch (InvalidOperationException)
            {
                multOptions = true;
            }

            if (parentNode != null && parentNode.IsEmpty)
            {
                parentNode.Add(element);
                Console.WriteLine("Inserted: " + element.Attribute("Name").Value);
                return inDoc;
            }
            else if (multOptions) // If there exists multiple options for insertion location
            {
                MessageBox.Show("Multiple options for parent element", "Parent Element Options", MessageBoxButtons.OK);
                List<string> parentOptions = inDoc.Descendants(parentName).Select(i => i.Parent.Attribute("Name").Value.ToString()).ToList();
                DropdownGui parentSelect = new DropdownGui(parentOptions, "Select the required parent to insert the element under");
                parentSelect.ShowDialog(out string selectedName);
                parentNode = inDoc.Descendants().Where(i => i.Attribute("Name") != null).Where(i => i.Attribute("Name").Value.ToString().Equals(selectedName)).Single();
            }

            IEnumerable<XAttribute> elementAttributes = element.Attributes();
            IEnumerable<XAttribute> parentAttributes = parentNode.Attributes();
            parentAttributes = parentAttributes.Except(elementAttributes);

            // If attributes are the same:
            if (elementAttributes.Equals(parentAttributes))
            {
                parentNode.Add(element.Elements());
            }
            // if Attributes are different
            else
            {
                foreach (XAttribute attribute in parentAttributes)
                {
                    if (!parentAttributes.Contains(attribute))
                        parentNode.Add(attribute);
                }
                parentNode.Add(element);
            }

            Console.WriteLine("Inserted: " + element.Name);

            return inDoc;
        }

        public XDocument InsertElement(XDocument doc, List<XElement> returnedElement)
        {
            foreach (XElement element in returnedElement)
                InsertElement(doc, element);
            return doc;
        }

        // Loads a premade blank file containig basic structure for the program to build off
        public XDocument LoadBasicFile()
        {
            XDocument doc = XDocument.Load("../../../L5XFiles/EmptyGenFile.l5X");
            return doc;
        }

        #endregion
    }


}
