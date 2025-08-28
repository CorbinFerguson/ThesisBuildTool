using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
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

        private readonly List<string> versionElements = new List<string>()
        {
            "AddOnInstructionDefinition",
            "DataType",
            "Program"
        };

        public XDocument inputFile;

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
                new XAttribute("ProcessorType", processorType),
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

        public XDocument CheckForDependencies(XDocument docToInsert, XElement element)
        {
            // Check if there are any dependencies in the inserted element
            if (element.Attributes("Dependencies") != null)
            {
                List<XElement> dependencies = element.Descendants("Dependencies").Elements().ToList();
                foreach (XElement dependency in dependencies)
                {
                    // Check that element doesnt exist
                    IEnumerable<XElement> clashingElements = docToInsert.Descendants().Where(i => i.Attribute("Name") != null && i.Attribute("Name").Value.Equals(dependency.Attribute("Name").Value));
                    if (!clashingElements.Any())
                    {
                        // If it doesn't, insert dependency into file
                        XElement dependentElement = inputFile.Descendants(dependency.Attribute("Type").Value).Single(i => i.Attribute("Name").Value.Equals(dependency.Attribute("Name").Value));
                        docToInsert = InsertElement(docToInsert, dependentElement);
                    }
                }
            }
            return docToInsert;
        }

        public XDocument CheckForDependencies(XDocument docToInsert, List<XElement> elements)
        {
            foreach (XElement element in elements)
                docToInsert = CheckForDependencies(docToInsert, element);
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

        public XElement GetElementFromFile(string elementType, string elementName)
        {
            XElement element = null;
            try
            {
                element = inputFile.Descendants(elementType).Where(i => i.Attribute("Name") != null).Single(i => i.Attribute("Name").Value == elementName);
            }
            catch (InvalidOperationException)
            {
                // There exists more than one element of that name
                IEnumerable<XElement> elementsToChoose = inputFile.Descendants(elementType).Where(i => i.Attribute("Name") != null).Where(i => i.Attribute("Name").Value.Equals(elementName));
                // Create a popup telling user what happened
                List<string> parentOptions = inputFile.Descendants(elementType).Select(i => i.Parent.Parent.Name.ToString()).Distinct().ToList();
                DropdownGui selectElement = new DropdownGui(parentOptions, "Select intended parent type");
                selectElement.ShowDialog(out string nameOfElement);
                try
                {
                    element = inputFile.Descendants(nameOfElement).Single();
                }
                catch (InvalidOperationException)
                {
                    List<string> typeObjects = inputFile.Descendants(nameOfElement).Select(i => i.Attribute("Name").Value.ToString()).ToList();
                    DropdownGui elementSelect = new DropdownGui(typeObjects, "Select specific object parent");
                    elementSelect.ShowDialog(out string selectedObject);
                    XElement parentElement = inputFile.Descendants().Where(i => i.Attribute("Name") != null).Single(i => i.Attribute("Name").Value.ToString().Equals(selectedObject));
                    element = parentElement.Descendants().Where(i => i.Attribute("Name") != null).Single(i => i.Attribute("Name").Value.ToString().Equals(elementName));
                }
            }
            return element;
        }

        public List<XElement> GetElementFromFile(string elementType, List<string> elementNames)
        {
            List<XElement> elementList = elementNames.Select(elementName => GetElementFromFile(elementType, elementName)).ToList();
            return elementList;
        }

        public string GetTypeAndSelect()
        {
            // Prompt user to select type of element to insert
            List<string> elementTypes = inputFile.Descendants().Where(i => i.Attribute("Name") != null).Select(i => i.Name.ToString()).Distinct().ToList();
            elementTypes.Remove("Controller");

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
            inDoc = CheckForDependencies(inDoc, element);
            Queue<string> rootPath = FindPathtoRootSchema(element);
            XName parentName = rootPath.Dequeue();
            XElement parentNode = null;

            // Add Revision num 1.0 if it doesnt have a revision
            if (element.Attribute("Revision") == null && element.Name.Equals("AddOnInstructionDefinition"))
                element.SetAttributeValue("Revision", "1.0");

            // Check that parent node exists in document using the schema
            while (!inDoc.Descendants(parentName).Any())
            {
                element = new XElement(parentName, element);

                try
                {
                    // Verify that the parent element has all required attributes
                    string complexType = validator.GetSchema().Descendants().Where(i => i.Name.Equals(Ns + "element")).Where(i => i.Attribute("name") != null && i.Attribute("name").Value.ToString().Equals(element.Name.ToString())).Single().Attribute("type").Value;
                    XElement schemaElement = validator.GetSchema().Descendants(Ns + "complexType").Single(i => i.Attribute("name") != null && i.Attribute("name").Value.Equals(complexType));
                    IEnumerable<XElement> requiredAttributes = schemaElement.Descendants().Where(i => i.Name.Equals(Ns + "attribute")).Where(i => i.Attribute("use") != null && i.Attribute("use").Value.Equals("required"));
                    foreach (XElement requiredAttribute in requiredAttributes)
                    {
                        TextInput input = new TextInput($"Input user value for {requiredAttribute.Attribute("name").Value} of {parentName}", "");
                        input.ShowDialog(out string attributeValue);
                        element.SetAttributeValue(requiredAttribute.Attribute("name").Value, attributeValue);
                    }

                    if (schemaElement.Descendants().Where(i => i.Name.Equals(Ns + "attribute")).Where(i => i.Attribute("EditedDate") != null).Any())
                    {
                        //Ensure edit information is up to date
                        XAttribute editedDate = new XAttribute("EditedDate", DateTime.Now);
                        element.SetAttributeValue(editedDate.Name, editedDate.Value);
                    }

                    parentName = rootPath.Dequeue();
                }
                catch (InvalidOperationException)
                {
                    // Multiple options for parent element, already handled in findRoot
                    parentName = rootPath.Dequeue();
                }
            }

            try
            {
                parentNode = inDoc.Descendants(parentName).Single();
            }
            catch (InvalidOperationException)
            {
                // Multiple elements of chosen type, prompt user to select which element should be the parent
                List<string> parentOptions = inDoc.Descendants(parentName).Select(i => i.Parent.Attribute("Name").Value.ToString()).Distinct().ToList();
                DropdownGui parentSelect = new DropdownGui(parentOptions, "Select the required parent to insert the element under");
                parentSelect.ShowDialog(out string selectedName);
                IEnumerable<XElement> parentNodes = inDoc.Descendants().Where(i => i.Attribute("Name") != null).Where(i => i.Attribute("Name").Value.ToString().Equals(selectedName));
                if (parentNodes.Count() == 1)
                    parentNode = parentNodes.Single();
                else if (parentNodes.Count() > 1)
                {
                    DropdownGui revisionSelect = new DropdownGui(parentNodes.Select(i => i.Attribute("Revision").Value.ToString()).ToList(), "Select Revision");
                    revisionSelect.ShowDialog(out string Revision);
                    parentNode = parentNodes.Single(i => i.Attribute("Revision").Value.ToString() == Revision);
                }
                else
                    throw new EmptyListException("No Parent Nodes found");
            }

            // Check that the element being added doesn't already exist
            IEnumerable<XElement> clashingElements = parentNode.Descendants(element.Name).Where(i => i.Attribute("Name") != null && i.Attribute("Name").Value.Equals(element.Attribute("Name").Value));
            XElement revisionClashes = clashingElements.SingleOrDefault(i => i.Attribute("Revision") == null || i.Attribute("Revision").Value.Equals(element.Attribute("Revision").Value));

            if (clashingElements.Count() > 0 && revisionClashes != null && !revisionClashes.IsEmpty)
            {
                List<string> actionOps = new List<string>() { "Cancel", "Replace", "Name" };
                if (versionElements.Contains(element.Name.ToString()))
                    actionOps.Add("Revision");
                DropdownGui elementExists = new DropdownGui(actionOps, $"Element by that name already exists. What would you like to change for {element.Attribute("Name").Value}?");
                elementExists.ShowDialog(out string selected);
                switch (selected)
                {
                    case "Replace":
                        // Replace the already existing element
                        revisionClashes.Remove();
                        break;
                    case "Name":
                    case "Revision":
                        TextInput renameElement;
                        // Rename the element being inserted to not clash with existing element
                        if (selected == "Name")
                        {
                            renameElement = new TextInput($"Element named {element.Attribute("Name").Value} already exists. Input a new {selected}.", element.Attribute(selected).Value.ToString());
                        }
                        else
                        {
                            renameElement = new TextInput($"Element named {element.Attribute("Name").Value} already exists. Input a new {selected}.", element.Attribute(selected).Value.ToString(), @"\d+\.[0-9]");
                        }
                        string newAtrVal = element.Attribute(selected).Value.ToString();
                        /// TODO: Add handling for if element by THAT name exists
                        while (newAtrVal.Equals(element.Attribute(selected).Value.ToString()))
                            renameElement.ShowDialog(out newAtrVal);
                        element.Attribute(selected).SetValue(newAtrVal);
                        inDoc = InsertElement(inDoc, element);
                        return inDoc;
                    default:
                        // Cancel insertion
                        return inDoc;
                }
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
            XDocument doc = XDocument.Load("../../../L5XFiles/TemplateFiles/EmptyTemplate.l5X");
            return doc;
        }

        public XElement GetSetAttributes(XElement element)
        {
            XElement elementAttr;
            try
            {
                XElement basicSchemaElement = GetValidator().GetSchema().Descendants().Where(i => i.Attribute("name") != null).Where(i => i.Attribute("name").Value.ToString().Equals(element.Name.ToString())).DescendantsAndSelf().Single();
                elementAttr = GetValidator().GetSchema().Descendants().Where(i => i.Name.Equals(Ns + "complexType")).Where(i => i.Attribute("name") != null).Single(i => i.Attribute("name").Value.ToString().Equals(basicSchemaElement.Attribute("type").Value.ToString()));
            }
            catch (InvalidOperationException)
            {
                // Get tag type from document file
                XElement grandparent = element.Parent.Parent;
                elementAttr = GetValidator().GetSchema().Descendants(grandparent.Name).Elements().Elements().Where(i => i.Attribute("name") != null).Single(i => i.Attribute("name").Value.ToString().Equals(element.Name.ToString()));

                // Search for complexType with name of type of element
            }
            IEnumerable<XElement> attributesEl = elementAttr.Elements().Where(i => i.Name.Equals(Ns + "attribute")); // TODO: this should be aquired from the schema
            List<XAttribute> attributesTochange = new List<XAttribute>();
            List<XAttribute> nonDefaultAttributes = new List<XAttribute>();

            // Foreach subelement/value
            foreach (XElement attribute in attributesEl)
            {
                string attributeValue = "";
                // Add the required elements to list of attributes to prompt user for
                if (element.Attribute(attribute.Attribute("name").Value) != null)
                    attributeValue = element.Attribute(attribute.Attribute("name").Value).Value;

                XAttribute wantedAttribute = new XAttribute(attribute.Attribute("name").Value.ToString(), attributeValue);
                attributesTochange.Add(wantedAttribute);
            }
            // Prompt user for other attributes to not take default value for
            MultiSelectDropdown selectAttributes = new MultiSelectDropdown(attributesTochange.Select(i => i.Name.ToString()).ToList(), "Select attributes to manually set value", true);
            selectAttributes.ShowDialog(out List<string> selectedAttributenames);
            foreach (string attributeName in selectedAttributenames)
            {
                nonDefaultAttributes.Add(element.Attribute(attributeName));
                attributesTochange.RemoveAll(i => i.Name == attributeName);
            }

            // Get user values for attributes
            foreach (XAttribute changeAttribute in nonDefaultAttributes)
            {
                TextInput input = new TextInput($"Input a value for {changeAttribute.Name} attribute of {element.Name}", changeAttribute.Value);
                input.ShowDialog(out string attributeValue);
                element.Attribute(changeAttribute.Name).SetValue(attributeValue);
            }

            foreach (XAttribute setDefaultAttribute in attributesTochange)
            {
                if (element.Attribute(setDefaultAttribute.Name) != null)
                    element.Attribute(setDefaultAttribute.Name).SetValue(setDefaultAttribute.Value);
                else
                    element.Add(setDefaultAttribute);
                if (setDefaultAttribute.Value.Equals("") && element.Attribute(setDefaultAttribute.Name) != null)
                    element.Attribute(setDefaultAttribute.Name).Remove();
            }
            return element;
        }

        public string GetElementTypes(XDocument doc)
        {
            // Select Element Types
            List<string> uniqueTypes = doc.Descendants().Where(i => i.Attribute("Name")!=null).Select(i => i.Name.ToString()).Distinct().ToList();
            uniqueTypes.Remove("Controller");

            if (uniqueTypes.Count == 0)
            {
                MessageBox.Show("No valid elements found");
                return null;
            }

            DropdownGui typesToRemove = new DropdownGui(uniqueTypes, "Select Element Types");
            typesToRemove.ShowDialog(out string typeSelected);
            return typeSelected;
        }

        #endregion
    }
}
