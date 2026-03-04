using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace L5XAutomationTool
{
    public class XMLHandler
    {
        #region Variables
        private readonly IValidationService validator;
        private readonly ISchemaDisambiguator disambiguator;

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

        #region Constructors
        public XMLHandler(IValidationService validation, ISchemaDisambiguator disambiguator)
        {
            validator = validation;
            this.disambiguator = disambiguator;
        }
        #endregion

        #region Functions

        // Function to check the element for dependent elements contained in docToInsert, and insert them
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
                    ElementHelper temp = new ElementHelper(ElementInfo);
                    // If it doesn't, insert dependency into file
                    docToInsert = InsertElement(docToInsert, moduleParentEl);
                    ElementInfo = temp;
                }
            }

            // If the element is a task, and it has any scheduled programs, insert them
            if (element.Name.ToString().Equals("Task") && element.Descendants("ScheduledProgram").Any())
            {
                // Save elementInfo state for task
                ElementInfo.BulkProgramParentGen = element.Attribute("Name").Value;
                ElementHelper unmodified = new ElementHelper(ElementInfo);

                // Insert the programs in the task
                IEnumerable<string> programNames = element.Descendants("ScheduledProgram").Select(i => i.Attribute("Name").Value);
                List<XElement> programs = inputFile.Descendants("Program").Where(i => programNames.Contains(i.Attribute("Name").Value.ToString())).Where(i => !docToInsert.Descendants("Program").Select(j => j.Attribute("Name").Value).Contains(i.Attribute("Name").Value)).ToList();
                if (programs.Any())
                    InsertElement(docToInsert, programs);

                // Return to ElementInfo state for task
                ElementInfo.ResetElements();
                ElementInfo = unmodified;
            }

            return docToInsert;
        }

        // Function to check a list of elements for dependencies
        internal XDocument CheckForDependencies(XDocument docToInsert, List<XElement> elements)
        {
            foreach (XElement element in elements)
                docToInsert = CheckForDependencies(docToInsert, element);
            return docToInsert;
        }

        // Function to get a list of the names for the nodes leading from the root(RSLogix5000) to element using the XSD schema document
        internal LinkedList<string> FindPathtoRootSchema(XElement element)
        {
            LinkedList<string> paths = new LinkedList<string>();

            string name = element.Name.ToString();
            XElement schemaElement = null;

            // Attempt to find the elements leading from the chosen elements to root
            try
            {
                schemaElement = validator.GetSchema().Descendants().Single(i => i.Name.Equals(Ns + "element") && (i.Attribute("name")?.Value.Equals(name) ?? false));

                // Loop until RSLogix5000Content(root of L5X) is found
                while (!schemaElement.Attribute("name").Value.Equals("RSLogix5000Content"))
                {
                    // Go to parent complex type, find name of that
                    name = schemaElement.Parent.Parent.Attribute("name").Value;

                    // search for something with that type
                    schemaElement = validator.GetSchema().Descendants().Where(i => i.Name.Equals(Ns + "element")).Single(i => i.Attribute("type")?.Value.Equals(name) ?? false);

                    paths.AddLast(schemaElement.Attribute("name").Value);
                }
                if (paths.Count == 0)
                    throw new EmptyListException("Empty path to root. Started at RSLogix5000Content");
            }
            // Catch the error thrown when there is ambiguity in finding the path
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("Sequence contains more than one"))
                {
                    // Get the parent options from the schema
                    IEnumerable<XElement> ambiguousElements = validator.GetSchema().Descendants(Ns + "element").Where(i => i.Attribute("type")?.Value.Equals(name) ?? false);
                    List<string> parentSchemaType = validator.GetSchema().Descendants().Where(i => ambiguousElements.Select(x => x.Parent.Parent.Attribute("name")?.Value.ToString()).ToList()?.Contains(i.Attribute("name")?.Value) ?? false).Select(i => i.Attribute("name").Value).ToList();
                    List<string> parentTypes = validator.GetSchema().Descendants().Where(i => i.Name.Equals(Ns + "element")).Where(i => parentSchemaType.Contains(i.Attribute("type")?.Value)).Select(i => i.Attribute("name").Value).ToList();

                    string chosenParentType = null;
                    if (ElementInfo.ParentElementBulk != null)
                        chosenParentType = ElementInfo.ParentElementBulk.Name.ToString();
                    else if (disambiguator != null)
                    {
                        chosenParentType = disambiguator.ChooseParentFor(name, parentTypes);
                        if (string.IsNullOrEmpty(chosenParentType) || !parentTypes.Contains(chosenParentType))
                            throw new AmbiguousSchemaPathException(name, parentTypes);
                        ElementInfo.ParentElementBulk = new XElement(chosenParentType);
                    }
                    else
                        throw new AmbiguousSchemaPathException(name, parentTypes);

                    string disambiguousParent = validator.GetSchema().Descendants().Where(i => i.Attribute("type")?.Value.ToString().Equals(name) ?? false).Select(i => i.Attribute("name").Value).Distinct().Single().ToString();
                    paths.AddLast(disambiguousParent);
                    paths.AddLast(chosenParentType);

                    // Use the parent of the last element in the queue
                    LinkedList<string> grandparentToRoot = FindPathtoRootSchema(new XElement(chosenParentType));

                    foreach (string node in grandparentToRoot)
                        paths.AddLast(node);

                    return paths;
                }
            }
            return paths;
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

        internal IValidationService GetValidator() { return validator; }

        // Function to insert element into a document. Inserts the element's dependent elements as well
        internal XDocument InsertElement(XDocument inDoc, XElement insertEl)
        {
            XElement element = new XElement(insertEl);
            inDoc = CheckForDependencies(inDoc, element);
            if ((ElementInfo.RootPath?.Count() ?? 0) <= 1)
                ElementInfo.RootPath = FindPathtoRootSchema(element);

            XName parentType = ElementInfo.GetFirst();

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

                    // The parent element has required attributes that must be added
                    if (requiredAttributes.Any())
                    {
                        throw new ParentMissingException(element, requiredAttributes);
                    }

                    if (schemaElement.Descendants().Where(i => i.Name.Equals(Ns + "attribute") && i.Attribute("EditedDate") != null).Any())
                    {
                        //Ensure edit information is up to date
                        XAttribute editedDate = new XAttribute("EditedDate", DateTime.Now);
                        element.SetAttributeValue(editedDate.Name, editedDate.Value);
                    }

                    parentType = ElementInfo.GetFirst();
                }
                catch (InvalidOperationException)
                {
                    // Multiple options for parent element, already handled in findRoot
                    parentType = ElementInfo.GetFirst();
                }
            }

            XElement parentNode = null;
            string grandparentType = ElementInfo.RootPath.First.Value;
            IEnumerable<XElement> grandParentNodes = inDoc.Descendants(grandparentType);
            if (grandParentNodes.Count() == 1)
            {
                // If there is only one valid grandparent, then use that
                parentNode = grandParentNodes.Elements(parentType).SingleOrDefault();
            }
            else if (ElementInfo.ParentElementBulk != null)
            {
                // If creating in bulk, then the parent type is already solved for
                if (ElementInfo.ParentElementBulk.Attribute("Name") is null)
                {
                    throw new ClashingParentException(grandParentNodes);
                }
                XElement elnode = grandParentNodes.SingleOrDefault(i => ElementInfo.ParentElementBulk.Attribute("Name").Value.Equals(i.Attribute("Name").Value));
                parentNode = elnode.Elements(parentType).SingleOrDefault();

            }
            else if (grandParentNodes.Count() > 1)
            {
                throw new ClashingParentException(grandParentNodes);
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
                throw new ClashingElementException(clashingElements, parentNode);
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

            return inDoc;
        }

        // Function to insert a list of elements
        internal XDocument InsertElement(XDocument doc, List<XElement> returnedElement)
        {
            LinkedList<string> unchangedPath = FindPathtoRootSchema(returnedElement.First());
            foreach (XElement element in returnedElement)
            {
                ElementInfo.RootPath = new LinkedList<string>(unchangedPath);
                InsertElement(doc, element);
            }
            return doc;
        }

        // Loads a premade blank file containing basic structure for the program to build off of
        internal XDocument LoadBasicFile()
        {
            XDocument doc = XDocument.Load("../../../L5XFiles/TemplateFiles/EmptyTemplate.l5X");
            return doc;
        }

        // Non-UI Utilizing function for testing
        internal List<XAttribute> GetAttributes(XElement element)
        {
            XElement basicSchemaElement = GetValidator().GetSchema().Descendants(Ns + "element").Where(i => i.Attribute("name")?.Value.ToString().Equals(element.Name.ToString()) ?? false).DescendantsAndSelf().Single();
            XElement elementAttr = GetValidator().GetSchema().Descendants(Ns + "complexType").Single(i => i.Attribute("name")?.Value.ToString().Equals(basicSchemaElement.Attribute("type").Value.ToString()) ?? false);

            IEnumerable<XElement> attributesEl = elementAttr.Elements(Ns + "attribute");
            List<XAttribute> attributesTochange = new List<XAttribute>();

            // Foreach subelement/value
            foreach (XElement attribute in attributesEl)
            {
                string attributeValue = "";
                // Add the required elements to list of attributes
                if (element.Attribute(attribute.Attribute("name").Value) != null)
                    attributeValue = element.Attribute(attribute.Attribute("name").Value).Value;

                XAttribute wantedAttribute = new XAttribute(attribute.Attribute("name").Value.ToString(), attributeValue);
                attributesTochange.Add(wantedAttribute);
            }

            return attributesTochange;
        }

        internal List<List<XAttribute>> GetAttributes(IEnumerable<XElement> elements)
        {
            List<List<XAttribute>> allAttr = new List<List<XAttribute>>();
            foreach (XElement el in elements)
                allAttr.Add(GetAttributes(el));
            return allAttr;
        }

        // Function to get all valid types contained in the document
        internal List<string> GetElementTypes(XDocument doc)
        {
            // Select Element Types
            List<string> uniqueTypes = doc.Descendants().Where(i => acceptedTypes.Contains(i.Name?.ToString())).Select(i => i.Name.ToString()).Distinct().ToList();

            if (uniqueTypes.Count == 0)
            {
                throw new EmptyListException("No Valid Elements Found");
            }
            return uniqueTypes;
        }

        #endregion
    }
}