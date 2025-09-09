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

        private readonly List<string> elementsWithRevision = new List<string>()
        {
            "AddOnInstructionDefinition",
            "DataType",
            "Program"
        };

        private readonly List<string> acceptedTypes = new List<string>()
        {
            "AddOnInstructionDefinition",
            "Program",
            "Datatype",
            "Dependency",
            "Parameter",
            "Structure",
            "Routine",
            "Tag",
            "LocalTag",
            "Module"

        };

        public XDocument inputFile;

        public XNamespace Ns { get; } = XNamespace.Get(@"http://www.w3.org/2001/XMLSchema");

        #endregion

        #region Constructors
        #endregion

        #region functions
        public XDocument CheckForDependencies(XDocument docToInsert, XElement element)
        {
            // Check if there are any dependencies in the inserted element
            if (element.Attributes("Dependencies") != null)
            {
                List<XElement> dependencies = element.Descendants("Dependencies").Elements().ToList();
                foreach (XElement dependency in dependencies)
                {
                    // Check that element doesnt exist
                    IEnumerable<XElement> clashingElements = docToInsert.Descendants().Where(i => i.Attribute("Name")?.Value.Equals(dependency.Attribute("Name")?.Value) ?? false);
                    if (!clashingElements.Any())
                    {
                        // If it doesn't, insert dependency into file
                        XElement dependentElement = inputFile.Descendants(dependency.Attribute("Type").Value).Single(i => i.Attribute("Name").Value.Equals(dependency.Attribute("Name").Value));
                        docToInsert = InsertElement(docToInsert, dependentElement);
                    }
                }
            }

            // Check if there is a referenced Parent module(I/O objects) not already in the document and add it
            if (!element.Attribute("ParentModule")?.Value.ToString().Equals("Local") ?? false)
            {
                string parentModule = element.Attribute("ParentModule").Value;
                XElement moduleParentEl = inputFile.Descendants("Module").Single(i => i.Attribute("Name")?.Value.ToString().Equals(parentModule) ?? false);
                IEnumerable<XElement> existingParents = docToInsert.Descendants().Where(i => i.Attribute("Name")?.Value.Equals(moduleParentEl.Attribute("Name")?.Value) ?? false);
                if (!existingParents.Any())
                {
                    // If it doesn't, insert dependency into file
                    docToInsert = InsertElement(docToInsert, moduleParentEl);
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
                schemaElement = validator.GetSchema().Descendants().Where(i => i.Name.Equals(Ns + "element")).Where(i => i.Attribute(attributeFilter)?.Value.Equals(name) ?? false).Single();

                // Loop until RSLogix5000Content(root of L5X) is found
                while (!schemaElement.Attribute("name").Value.Equals("RSLogix5000Content"))
                {
                    // Go to parent complex type, find name of that
                    attributeFilter = "name";
                    name = schemaElement.Parent.Parent.Attribute(attributeFilter).Value;

                    // search for something with that type
                    attributeFilter = "type";
                    schemaElement = validator.GetSchema().Descendants().Where(i => i.Name.Equals(Ns + "element")).Where(i => i.Attribute(attributeFilter)?.Value.Equals(name) ?? false).Single();

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
                    IEnumerable<XElement> ambiguousElements = validator.GetSchema().Descendants(Ns + "element").Where(i => i.Attribute(attributeFilter)?.Value.Equals(name) ?? false);

                    List<string> parentNames = new List<string>();
                    parentNames = validator.GetSchema().Descendants().Where(i => ambiguousElements.Select(x => x.Parent.Parent.Attribute("name")?.Value.ToString()).ToList()?.Contains(i.Attribute(attributeFilter)?.Value) ?? false).Select(i => i.Attribute("name").Value).ToList();

                    // Prompt user to select grandparent for the element
                    DropdownGui selectElement = new DropdownGui(parentNames, "Select grandparent for " + element.Name);
                    selectElement.ShowDialog(out string nameOfElement);
                    string disambiguousParent = validator.GetSchema().Descendants().Where(i => i.Attribute("type")?.Value.ToString().Equals(name) ?? false).Select(i => i.Attribute("name").Value).Distinct().Single().ToString();
                    paths.Enqueue(disambiguousParent);

                    // Create the path queue
                    Queue<string> rootPath = new Queue<string>();
                    // Use the parent of the last element in the queue
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
            string attributeSearch = "Name";
            // IO Modules do not require a name. Must search by catalog number instead
            if (elementType.Equals("Module"))
                attributeSearch = "CatalogNumber";
            XElement element = null;
            try
            {
                element = inputFile.Descendants(elementType).Single(i => i.Attribute(attributeSearch)?.Value == elementName);
            }
            catch (InvalidOperationException)
            {

                // There exists more than one element of that name
                IEnumerable<XElement> elementsToChoose = inputFile.Descendants(elementType).Where(i => i.Attribute(attributeSearch)?.Value.Equals(elementName) ?? false);
                if (!elementType.Equals("Module"))
                {
                    // Create a popup prompting user to select grandparent to disambiguate
                    List<string> parentOptions = inputFile.Descendants(elementType).Select(i => i.Parent.Parent.Name.ToString()).Distinct().ToList();
                    DropdownGui selectElement = new DropdownGui(parentOptions, "Select intended grandparent type for element you are accessing");
                    selectElement.ShowDialog(out string nameOfElement);

                    // Insert
                    try
                    {
                        element = inputFile.Descendants(nameOfElement).Single();
                    }
                    catch (InvalidOperationException)
                    {
                        List<string> typeObjects = inputFile.Descendants(nameOfElement).Select(i => i.Attribute(attributeSearch).Value.ToString()).ToList();
                        DropdownGui elementSelect = new DropdownGui(typeObjects, "Select specific object parent");
                        elementSelect.ShowDialog(out string selectedObject);
                        XElement parentElement = inputFile.Descendants().Single(i => i.Attribute(attributeSearch)?.Value.ToString().Equals(selectedObject) ?? false);
                        element = parentElement.Descendants().Single(i => i.Attribute(attributeSearch)?.Value.ToString().Equals(elementName) ?? false);
                    }
                }
                else
                {
                    // Create a popup telling user what happened
                    List<string> parentOptions = elementsToChoose.Select(i => i.Attribute("Name")?.Value ?? elementName + " with no Name at port " + i.Descendants("Port")?.First().Attribute("Address")?.Value ?? elementName + "With no Name or Port Address to distinguish").Distinct().ToList();
                    DropdownGui selectElement = new DropdownGui(parentOptions, "Select the name of the Module you are accessing");
                    selectElement.ShowDialog(out string nameOfElement);

                    // Insert the first element if there is no name or port address to use.
                    if (nameOfElement.Contains("or Port"))
                    {
                        // just take the first element of that type
                        element = elementsToChoose.Where(i => i.Attribute("CatalogNumber").Value.Equals(elementName)).First();
                    }
                    // If there is just no name but there is a port, use that to differentiate.
                    else if (nameOfElement.Contains("no Name"))
                    {
                        throw new NotImplementedException();
                    }
                    // There is a name. Use that to differentiate.
                    else
                    {
                        try
                        {

                            element = elementsToChoose.Where(i => i.Attribute("Name")?.Value.ToString().Equals(nameOfElement) ?? false).Single();
                        }
                        catch (InvalidOperationException)
                        {
                            List<string> typeObjects = inputFile.Descendants(nameOfElement).Select(i => i.Attribute(attributeSearch).Value.ToString()).ToList();
                            DropdownGui elementSelect = new DropdownGui(typeObjects, "Select specific object parent");
                            elementSelect.ShowDialog(out string selectedObject);
                            XElement parentElement = inputFile.Descendants().Single(i => i.Attribute(attributeSearch)?.Value.ToString().Equals(selectedObject) ?? false);
                            element = parentElement.Descendants().Single(i => i.Attribute(attributeSearch)?.Value.ToString().Equals(elementName) ?? false);
                        }
                    }
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
            List<string> elementTypes = inputFile.Descendants().Where(i => acceptedTypes.Contains(i.Name.ToString())).Select(i => i.Name.ToString()).Distinct().ToList();

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

        public XDocument InsertElement(XDocument inDoc, XElement element, Queue<string> rootPath = null)
        {
            inDoc = CheckForDependencies(inDoc, element);
            if (rootPath == null)
                rootPath = FindPathtoRootSchema(element);
            XName parentType = rootPath.Dequeue();
            XElement parentNode = null;
            string searchFilter = "Name";

            // If the element is a module, use CatalogNumber instead of Name
            if (element.Name.ToString().Equals("Module"))
            {
                if (element.Attribute("Name") == null)
                    searchFilter = "CatalogNumber";
            }

            // Add Revision num 1.0 if it doesnt have a revision
            if (element.Attribute("Revision") == null && elementsWithRevision.Contains(element.Name.ToString()))
                element.SetAttributeValue("Revision", "1.0");

            // Check that parent node exists in document using the schema
            while (!inDoc.Descendants(parentType).Any())
            {
                element = new XElement(parentType, element);

                try
                {
                    // Verify that the parent element has all required attributes
                    string complexType = validator.GetSchema().Descendants().Where(i => i.Name.Equals(Ns + "element")).Where(i => i.Attribute("name")?.Value.ToString().Equals(element.Name.ToString()) ?? false).Single().Attribute("type").Value;
                    XElement schemaElement = validator.GetSchema().Descendants(Ns + "complexType").Single(i => i.Attribute("name")?.Value.Equals(complexType) ?? false);
                    IEnumerable<XElement> requiredAttributes = schemaElement.Descendants().Where(i => i.Name.Equals(Ns + "attribute")).Where(i => i.Attribute("use")?.Value.Equals("required") ?? false);
                    foreach (XElement requiredAttribute in requiredAttributes)
                    {
                        TextInput input = new TextInput($"Input user value for {requiredAttribute.Attribute("name").Value} of {parentType}");
                        input.ShowDialog(out string attributeValue);
                        element.SetAttributeValue(requiredAttribute.Attribute("name").Value, attributeValue);
                    }

                    if (schemaElement.Descendants().Where(i => i.Name.Equals(Ns + "attribute")).Where(i => i.Attribute("EditedDate") != null).Any())
                    {
                        //Ensure edit information is up to date
                        XAttribute editedDate = new XAttribute("EditedDate", DateTime.Now);
                        element.SetAttributeValue(editedDate.Name, editedDate.Value);
                    }

                    parentType = rootPath.Dequeue();
                }
                catch (InvalidOperationException)
                {
                    // Multiple options for parent element, already handled in findRoot
                    parentType = rootPath.Dequeue();
                }
            }

            try
            {
                parentNode = inDoc.Descendants(parentType).Single();
            }
            catch (InvalidOperationException)
            {////TODO: this needs to be updated to handle when there is multiple parents of the same type, it should offer the user a selection
                // Multiple elements of chosen type, prompt user to select which element should be the parent
                string grandparentName = rootPath.Dequeue();
                IEnumerable<XElement> parentNodes = inDoc.Descendants(grandparentName);
                if (parentNodes.Count() == 1)
                    parentNode = parentNodes.Single();
                else if (parentNodes.Count() > 1)
                {
                    DropdownGui nameSelect = new DropdownGui(parentNodes.Select(i => i.Attribute("Name").Value.ToString()).ToList(), "Select Parent Element");
                    nameSelect.ShowDialog(out string name);
                    parentNode = parentNodes.Single(i => i.Attribute("Name").Value.ToString() == name);
                }
                else
                    throw new EmptyListException("No Parent Nodes found");
            }

            if (element.Name.ToString().Equals("Module"))
            {
                try
                {
                    int portNum = inDoc.Descendants("Module").Where(i => i.Attribute("ParentModule").Value.Equals(element.Attribute("ParentModule").Value)).Count();
                    if (inDoc.Descendants("Module").Where(i => i.Attribute("Name")?.Value.Equals(element.Attribute("ParentModule").Value) ?? false).Descendants("Port").Where(i => i.Attribute("Address")?.Value.ToString().Equals(portNum.ToString()) ?? false).Any())
                        portNum++;
                    element.Descendants("Port").Single(i => i.Attribute("Type").Value.Equals("ICP")).Attribute("Address").SetValue(portNum);
                }
                catch (InvalidOperationException)
                {
                    // Prompt user for ethernet address or hostname value
                    string ipRegex = @"^((25[0-5]|(2[0-4]|1\d|[1-9]|)\d)\.?\b){4}$|^HostName$";
                    TextInput ipPrompt = new TextInput("Input User IP Address or 'HostName'", "192.168.1.1", ipRegex);
                    ipPrompt.ShowDialog(out string chosenIP);

                    // Set the value of IP
                    element.Descendants("Port").Single(i => i.Attribute("Type").Value.Equals("Ethernet")).Attribute("Address").SetValue(chosenIP);
                }
            }

            // Check that the element being added doesn't already exist
            IEnumerable<XElement> clashingElements = parentNode.Descendants(element.Name).Where(i => i.Attribute(searchFilter)?.Value.Equals(element.Attribute(searchFilter).Value) ?? false);
            XElement revisionClashes = clashingElements.SingleOrDefault(i => i.Attribute("Revision") == null || i.Attribute("Revision").Value.Equals(element.Attribute("Revision").Value));

            if (clashingElements.Count() > 0 && revisionClashes != null && !revisionClashes.IsEmpty)
            {
                List<string> actionOps = new List<string>() { "Cancel", "Replace", "Name" };
                if (elementsWithRevision.Contains(element.Name.ToString()))
                    actionOps.Add("Revision");
                DropdownGui elementExists = new DropdownGui(actionOps, $"Element already exists. What would you like to change for {element.Attribute(searchFilter).Value}?");
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
                            renameElement = new TextInput($"Element {element.Attribute(searchFilter).Value} already exists. Input a new {selected}.", element.Attribute(selected)?.Value ?? "");
                        }
                        else
                        {
                            renameElement = new TextInput($"Element {element.Attribute(searchFilter).Value} already exists. Input a new {selected}.", element.Attribute(selected)?.Value ?? "", @"^\d+\.\d$");
                        }

                        // Prompt user for new value, then set it
                        string newAtrVal = element.Attribute(selected)?.Value.ToString() ?? "";
                        while (newAtrVal.Equals(element.Attribute(selected)?.Value.ToString() ?? ""))
                            renameElement.ShowDialog(out newAtrVal);
                        element.SetAttributeValue(selected, newAtrVal);

                        // Insert the changed element and return the updated document
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

            // If attributes are the same just add it
            if (elementAttributes.Equals(parentAttributes))
            {
                parentNode.Add(element.Elements());
            }
            // if Attributes are different, add the missing attributes to the parent
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
            Queue<string> unchangedPath = FindPathtoRootSchema(returnedElement.First());
            foreach (XElement element in returnedElement)
            {
                Queue<string> usedpath = new Queue<string>(unchangedPath);
                InsertElement(doc, element, usedpath);
            }
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
            XElement basicSchemaElement = GetValidator().GetSchema().Descendants(Ns + "element").Where(i => i.Attribute("name")?.Value.ToString().Equals(element.Name.ToString()) ?? false).DescendantsAndSelf().Single();
            elementAttr = GetValidator().GetSchema().Descendants(Ns + "complexType").Single(i => i.Attribute("name")?.Value.ToString().Equals(basicSchemaElement.Attribute("type").Value.ToString()) ?? false);

            IEnumerable<XElement> attributesEl = elementAttr.Elements(Ns + "attribute");
            List<XAttribute> attributesTochange = new List<XAttribute>();

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
                attributesTochange.RemoveAll(i => i.Name == attributeName);
                TextInput input = new TextInput($"Input a value for {attributeName} attribute of {element?.Name}" ?? "Input a value.", "");
                input.ShowDialog(out string attributeValue);
                element.SetAttributeValue(attributeName, attributeValue);
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
            List<string> uniqueTypes = doc.Descendants().Where(i => i.Attribute("Name") != null && acceptedTypes.Contains(i.Name.ToString())).Select(i => i.Name.ToString()).Distinct().ToList();

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
