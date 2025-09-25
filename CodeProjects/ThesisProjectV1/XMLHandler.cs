using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Navigation;
using System.Xml.Linq;
using ThesisProjectV1.Forms;

namespace ThesisProjectV1
{
    public class XMLHandler
    {
        #region Variables
        private readonly Validate validator = new Validate();

        private readonly List<string> acceptedTypes = new List<string>()
        {
            "AddOnInstructionDefinition",
            "Program",
            "Datatype",
            "Routine",
            "Tag",
            "LocalTag",
            "Module",
            "Task"

        };

        public XDocument inputFile;

        public XNamespace Ns { get; } = XNamespace.Get(@"http://www.w3.org/2001/XMLSchema");

        public ElementHelper ElementInfo = new ElementHelper();

        #endregion

        #region functions
        internal XDocument CheckForDependencies(XDocument docToInsert, XElement element)
        {
            // Check if there are any dependencies in the inserted element
            if (element.Attribute("Dependencies") != null)
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

                        Console.WriteLine("Dependency element found: " + dependentElement.Attribute("Name").Value);
                        docToInsert = InsertElement(docToInsert, dependentElement);
                    }
                }
            }

            // Check if there is a referenced Parent module(I/O objects) not already in the document and add it
            if (!element.Attribute("ParentModule")?.Value.ToString().Equals("Local") ?? false)
            {
                string parentModule = element.Attribute("ParentModule").Value;
                XElement moduleParentEl = inputFile.Descendants("Module").Single(i => i.Attribute("Name")?.Value.ToString().Equals(parentModule) ?? false);
                IEnumerable<XElement> existingParents = docToInsert.Descendants().Where(i => i.Attribute("Name")?.Value.Equals(moduleParentEl?.Attribute("Name")?.Value) ?? false);
                if (!existingParents.Any())
                {
                    // If it doesn't, insert dependency into file
                    docToInsert = InsertElement(docToInsert, moduleParentEl);
                }
            }

            if(element.Name.ToString().Equals("Task") && element.Descendants("ScheduledProgram").Any())
            {
                // Save elementInfo state for task
                ElementInfo.BulkProgramParentGen = element.Attribute("Name").Value;
                ElementHelper unmodified = new ElementHelper(ElementInfo);

                // Insert the programs in the task
                IEnumerable<string> programNames = element.Descendants("ScheduledProgram").Select(i => i.Attribute("Name").Value);
                List<XElement> programs = inputFile.Descendants("Program").Where(i => programNames.Contains(i.Attribute("Name").Value.ToString())).Where(i => !docToInsert.Descendants("Program").Select(j => j.Attribute("Name").Value).Contains(i.Attribute("Name").Value)).ToList();
                if(programs.Any())
                    InsertElement(docToInsert, programs);

                // Return to ElementInfo state for task
                ElementInfo.ResetElements();
                ElementInfo = unmodified;
            }

            return docToInsert;
        }

        internal XDocument CheckForDependencies(XDocument docToInsert, List<XElement> elements)
        {
            foreach (XElement element in elements)
                docToInsert = CheckForDependencies(docToInsert, element);
            return docToInsert;
        }

        // Returns a list of the names for the nodes leading from the root(RSLogix5000) to element
        internal Queue<string> FindPathtoRootSchema(XElement element)
        {
            Queue<string> paths = new Queue<string>();

            string name = element.Name.ToString();
            XElement schemaElement = null;

            try
            {
                schemaElement = validator.GetSchema().Descendants().Single(i => i.Name.Equals(Ns + "element") && (i.Attribute("name")?.Value.Equals(name) ?? false));

                // Loop until RSLogix5000Content(root of L5X) is found
                while (!schemaElement.Attribute("name").Value.Equals("RSLogix5000Content"))
                {
                    // Go to parent complex type, find name of that
                    name = schemaElement.Parent.Parent.Attribute("name").Value;

                    // search for something with that type
                    schemaElement = validator.GetSchema().Descendants().Where(i => i.Name.Equals(Ns + "element")).Where(i => i.Attribute("type")?.Value.Equals(name) ?? false).Single();

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
                    IEnumerable<XElement> ambiguousElements = validator.GetSchema().Descendants(Ns + "element").Where(i => i.Attribute("type")?.Value.Equals(name) ?? false);

                    List<string> parentSchemaType = validator.GetSchema().Descendants().Where(i => ambiguousElements.Select(x => x.Parent.Parent.Attribute("name")?.Value.ToString()).ToList()?.Contains(i.Attribute("name")?.Value) ?? false).Select(i => i.Attribute("name").Value).ToList();

                    List<string> parentNames = validator.GetSchema().Descendants().Where(i => i.Name.Equals(Ns + "element")).Where(i => parentSchemaType.Contains(i.Attribute("type")?.Value)).Select(i => i.Attribute("name").Value).ToList();

                    string nameOfElement;

                    if (ElementInfo.ParentElementBulk == null)
                    {
                        // Prompt user to select grandparent for the element
                        DropdownGui selectElement = new DropdownGui(parentNames, "Select intended parent type for " + element.Name);
                        selectElement.ShowDialog(out nameOfElement);
                        ElementInfo.ParentElementBulk = new XElement(nameOfElement);
                    }
                    else
                    {
                        nameOfElement = ElementInfo.ParentElementBulk.Name.ToString();
                    }

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
                { MessageBox.Show(ex.Message + "\nStack Trace: " + ex.StackTrace, "Unexpected Error Occurred", MessageBoxButtons.OK); }
            }
            catch (EmptyListException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
            return paths;
        }

        internal List<XElement> GetElementFromFile(string elementType, string elementName)
        {
            string attributeSearch = "Name";
            // IO Modules do not require a name. Must search by catalog number instead
            if (elementType.Equals("Module"))
                attributeSearch = "CatalogNumber";
            XElement element = null;
            List<XElement> elements = new List<XElement>();
            try
            {
                element = inputFile.Descendants(elementType).Single(i => i.Attribute(attributeSearch)?.Value == elementName);
                elements.Add(element);
            }
            catch (InvalidOperationException)
            {
                // There exists more than one element of that name
                IEnumerable<XElement> elementsToChoose = inputFile.Descendants(elementType).Where(i => i.Attribute(attributeSearch)?.Value.Equals(elementName) ?? false);
                if (!elementType.Equals("Module"))
                {
                    // Create a popup prompting user to select grandparent to disambiguate
                    List<string> parentOptions = inputFile.Descendants(elementType).Where(i => i.Attribute(attributeSearch).Value.Equals(elementName)).Select(i => i.Parent.Parent.Attribute("Name").Value.ToString()).Distinct().ToList();
                    DropdownGui selectElement = new DropdownGui(parentOptions, "Select grandparent element you are accessing");
                    selectElement.ShowDialog(out string nameOfElement);

                    // Access disambiguated element
                    XElement parentElement = inputFile.Descendants().Single(i => i.Attribute("Name")?.Value.Equals(nameOfElement) ?? false);
                    element = parentElement.Descendants(elementType).Single(i => i.Attribute("Name")?.Value.Equals(elementName) ?? false);
                    elements.Add(element);

                    // Set parent info, so that it doesn't prompt user again
                    ElementInfo.ParentElementBulk = parentElement;
                }
                else
                {
                    // Get all valid options. 
                    List<string> parentOptions = elementsToChoose.Select(i => i.Attribute("Name")?.Value ?? elementName + " with no Name at port " + i.Descendants("Port")?.First().Attribute("Address").Value).Distinct().ToList();

                    // Create a popup prompting user to select the 
                    MultiSelectDropdown selectElement = new MultiSelectDropdown(parentOptions, "Select the name of the Module " + elementName + " you are accessing");
                    selectElement.ShowDialog(out List<string> nameOfElements);

                    foreach (string nameOfElement in nameOfElements)
                    {
                        // Insert the first element if there is no name or port address to use.
                        if (nameOfElement.Contains("at port"))
                        {
                            int portIndex = nameOfElement.IndexOf("port ") + 5;
                            string selectedPort = nameOfElement.Substring(portIndex);
                            element = elementsToChoose.Single(i => i.Descendants("Port")?.Where(j => j.Attribute("Address").Value.ToString().Equals(selectedPort)).Any() ?? false);
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
                        elements.Add(element);
                    }
                }
            }
            return elements;
        }

        internal List<XElement> GetElementFromFile(string elementType, List<string> elementNames)
        {
            List<XElement> elementList = elementNames.SelectMany(elementName => GetElementFromFile(elementType, elementName)).ToList();
            return elementList;
        }

        internal string GetTypeAndSelect()
        {
            // Prompt user to select type of element to insert
            List<string> elementTypes = inputFile.Descendants().Where(i => acceptedTypes.Contains(i.Name.ToString())).Select(i => i.Name.ToString()).Distinct().ToList();

            DropdownGui selectType = new DropdownGui(elementTypes, "Select type of the element to insert");
            selectType.ShowDialog(out string typeOfElement);
            return typeOfElement;
        }

        // Gets all the simple elements in the XML Schema
        internal List<String> GetSimpleElements()
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

        internal XDocument InsertElement(XDocument inDoc, XElement insertEl)
        {
            XElement element = new XElement(insertEl);
            inDoc = CheckForDependencies(inDoc, element);
            if ((ElementInfo.RootPath?.Count() ?? 0) <= 1)
                ElementInfo.RootPath = FindPathtoRootSchema(element);
            XName parentType = ElementInfo.RootPath.Dequeue();
            XElement parentNode = null;
            string searchFilter = "Name";
            IEnumerable<XElement> clashingElements = null;

            // If the element is a module, use CatalogNumber instead of Name
            if (element.Name.ToString().Equals("Module"))
            {
                if (element.Attribute("Name") == null)
                    searchFilter = "CatalogNumber";
            }

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

                    if (schemaElement.Descendants().Where(i => i.Name.Equals(Ns + "attribute") && i.Attribute("EditedDate") != null).Any())
                    {
                        //Ensure edit information is up to date
                        XAttribute editedDate = new XAttribute("EditedDate", DateTime.Now);
                        element.SetAttributeValue(editedDate.Name, editedDate.Value);
                    }

                    parentType = ElementInfo.RootPath.Dequeue();
                }
                catch (InvalidOperationException)
                {
                    // Multiple options for parent element, already handled in findRoot
                    parentType = ElementInfo.RootPath.Dequeue();
                }
            }

            // Multiple elements of chosen type, prompt user to select which element should be the parent
            string grandparentType = ElementInfo.RootPath.Peek();
            IEnumerable<XElement> grandParentNodes = inDoc.Descendants(grandparentType);
            if (grandParentNodes.Count() == 1)
            {
                parentNode = grandParentNodes.Elements(parentType).SingleOrDefault();
            }
            else if (ElementInfo.ParentElementBulk != null)
            {
                parentNode = ElementInfo.ParentElementBulk.Elements(parentType).SingleOrDefault();
            }
            else if (grandParentNodes.Count() > 1)
            {
                DropdownGui nameSelect = new DropdownGui(grandParentNodes.Select(i => i.Attribute("Name").Value.ToString()).ToList(), "Select Parent Element");
                nameSelect.ShowDialog(out string name);
                parentNode = grandParentNodes.Single(i => i.Attribute("Name").Value.ToString() == name).Elements(parentType).Single();
            }
            else
            {
                parentNode = new XElement(parentType, element);
                return InsertElement(inDoc, parentNode);
            }

            // Check that the element being added doesn't already exist
            if (element.Name.ToString() != "Module" || element.Attribute("Name") != null)
                clashingElements = parentNode.Descendants(element.Name).Where(i => i.Attribute(searchFilter)?.Value.ToLower().Equals(element.Attribute(searchFilter).Value.ToLower()) ?? false);

            // Handle already existing elements, either replace the existing element, rename the inserted element, or cancel the operation
            if (clashingElements?.Any() ?? false)
            {
                List<string> actionOps = new List<string>() { "Cancel", "Replace", "Name" };
                DropdownGui elementExists = new DropdownGui(actionOps, $"Element already exists. What would you like to change for {element.Attribute(searchFilter).Value}?");
                elementExists.ShowDialog(out string selected);
                switch (selected)
                {
                    case "Replace":
                        // Replace the already existing element
                        clashingElements.Single();
                        break;
                    case "Name":
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
                        while (parentNode.Descendants(element.Name).Where(i => newAtrVal.ToLower().Equals(i.Attribute(selected)?.Value.ToLower().ToString())).Any())
                            renameElement.ShowDialog(out newAtrVal);
                        element.SetAttributeValue(selected, newAtrVal);
                        break;
                    default:
                        // Cancel insertion
                        Console.WriteLine("Canceling Insertion");
                        return inDoc;
                }
            }

            // I/O Modules need the Port address to exist but be different than any other I/O modules under the parent module
            if (element.Name.ToString().Equals("Module"))
            {
                if (element.Descendants("Port").Where(i => i.Attribute("Type").Value.Equals("ICP")).Any())
                {
                    int portNum = inDoc.Descendants("Module").Where(i => i.Attribute("ParentModule").Value.Equals(element.Attribute("ParentModule").Value)).Count();
                    if (inDoc.Descendants("Module").Where(i => i.Attribute("Name")?.Value.Equals(element.Attribute("ParentModule").Value) ?? false).Descendants("Port").Any())
                        portNum++;
                    element.Descendants("Port").Single(i => i.Attribute("Type").Value.Equals("ICP")).Attribute("Address").SetValue(portNum);
                }
                if (element.Descendants("Port").Where(i => i.Attribute("Type").Value.Equals("Ethernet") && i.Attribute("Address") != null).Any() && element.Attribute("ParentModule").Value.Equals("Local"))
                {
                    // Get all already used IP addresses
                    List<string> takenIPs = inDoc.Descendants("Module").Where(i => i.Attribute("ParentModule")?.Value.Equals("Local") ?? false).Descendants("Port").Where(i => i.Attribute("Type")?.Value.ToString().Equals("Ethernet") ?? false).Select(i => i.Attribute("Address").Value.ToString()).ToList();

                    // Prompt user for ethernet address or hostname value
                    string ipRegex = @"^((25[0-5]|(2[0-4]|1\d|[1-9]|)\d)\.?\b){4}$|^HostName$";
                    TextInput ipPrompt = new TextInput("Input User IP Address or 'HostName' for " + element.Attribute("Name")?.Value ?? element.Attribute("CatalogNumber").Value, "192.168.1.1", ipRegex);
                    string chosenIP = element.Descendants("Port").Single(i => i.Attribute("Type").Value.ToString().Equals("Ethernet")).Attribute("Address")?.Value ?? "192.168.1.1";
                    while (takenIPs.Contains(chosenIP))
                    {
                        ipPrompt.ShowDialog(out chosenIP);
                    }
                    // Set the value of IP
                    element.Descendants("Port").Single(i => i.Attribute("Type").Value.Equals("Ethernet")).Attribute("Address").SetValue(chosenIP);
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

            Console.WriteLine("Inserted: " + element.Name + " " + element.Attribute("Name")?.Value ?? "");

            return inDoc;
        }

        internal XDocument InsertElement(XDocument doc, List<XElement> returnedElement)
        {
            

            Queue<string> unchangedPath = FindPathtoRootSchema(returnedElement.First());
            foreach (XElement element in returnedElement)
            {
                ElementInfo.RootPath = new Queue<string>(unchangedPath);
                InsertElement(doc, element);
            }
            return doc;
        }

        // Loads a premade blank file containig basic structure for the program to build off
        internal XDocument LoadBasicFile()
        {
            XDocument doc = XDocument.Load("../../../L5XFiles/TemplateFiles/EmptyTemplate.l5X");
            return doc;
        }

        internal void GetSetAttributes(XElement element)
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
            MultiSelectDropdown selectAttributes = new MultiSelectDropdown(attributesTochange.Select(i => i.Name.ToString()).ToList(), "Select attributes to manually set value. (NO INPUT VALIDATION. USE WITH CAUTION)", true);
            selectAttributes.ShowDialog(out List<string> selectedAttributenames);
            foreach (string attributeName in selectedAttributenames)
            {
                attributesTochange.RemoveAll(i => i.Name == attributeName);
                TextInput input = new TextInput($"Input a value for {attributeName} attribute of {element.Attribute("Name")?.Value ?? element.Attribute("CatalogNumber").Value}", element.Attribute(attributeName)?.Value ?? "");
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

            // Prompt user to select any children to modify
            IEnumerable<XElement> childElements = element.Elements();
            if (childElements.Any())
            {
                MultiSelectDropdown selectChildren = new MultiSelectDropdown(childElements.Select(i => i.Attribute("Name")?.ToString() ?? i.Name.ToString()).Distinct().ToList(), "Select Children elements to modify", true);
                selectChildren.ShowDialog(out List<string> selectedChildren);

                // Access the elements selected and modify them recursively
                childElements = childElements.Where(i => selectedChildren.Contains(i.Attribute("Name")?.ToString() ?? i.Name.ToString()));
                GetSetAttributes(childElements);
            }
        }

        internal void GetSetAttributes(IEnumerable<XElement> elements)
        {
            foreach (XElement element in elements)
            {
                GetSetAttributes(element);
            }
        }

        internal string GetElementTypes(XDocument doc)
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