using System.Xml.Linq;

namespace L5XAutomationTool
{
    public class XMLHandler(IValidationService validation, ISchemaDisambiguator disambiguator)
    {
        #region Variables
        private readonly IValidationService validator = validation;
        private readonly ISchemaDisambiguator disambiguator = disambiguator;

        private readonly List<string> acceptedTypes =
        [
            "AddOnInstructionDefinition",
            "Program",
            "DataType",
            "Routine",
            "Tag",
            "LocalTag",
            "Module",
            "Task"
        ];

        public XDocument inputFile;

        public XNamespace Ns { get; } = XNamespace.Get(@"http://www.w3.org/2001/XMLSchema");

        public ElementHelper ElementInfo = new();

        #endregion

        #region Functions

        /// <summary>
        /// Checks the specified element for dependencies and inserts them into the provided document.
        /// </summary>
        /// <param name="docToInsert">The document to insert dependencies into.</param>
        /// <param name="element">The element to check for dependencies.</param>
        /// <returns>The updated document with dependencies inserted.</returns>
        internal XDocument CheckForDependencies(XDocument docToInsert, XElement element)
        {
            if (element.Element("Dependencies") != null)
            {
                List<XElement> dependencies = [.. element.Descendants("Dependencies").Elements()];
                foreach (XElement dependency in dependencies)
                {
                    // Check that the dependent element does not already exist in the document
                    IEnumerable<XElement> clashingElements = docToInsert.Descendants().Where(i => i.Attribute("Name")?.Value.Equals(dependency.Attribute("Name")?.Value) ?? false);
                    if (!clashingElements.Any())
                    {
                        // If it doesn't exist, insert the dependency into the document
                        XElement dependentElement = inputFile.Descendants(dependency.Attribute("Type").Value).Single(i => i.Attribute("Name").Value.Equals(dependency.Attribute("Name").Value));

                        docToInsert = InsertElement(docToInsert, dependentElement);
                        ElementInfo.RootPath.Clear();
                    }
                }
            }

            // Check if there is a referenced Parent module (I/O objects) not already in the document and add it
            if (!element.Attribute("ParentModule")?.Value.ToString().Equals("Local") ?? false)
            {
                string parentModule = element.Attribute("ParentModule").Value;
                XElement moduleParentEl;
                try
                {
                    moduleParentEl = inputFile.Descendants("Module").Single(i => i.Attribute("Name")?.Value.ToString().Equals(parentModule) ?? false);

                }
                catch (InvalidOperationException ex)
                {
                    throw new InvalidOperationException("Attempted to create element from dependency, but no dependency found.", ex); ;
                }                
                IEnumerable<XElement> existingParents = docToInsert.Descendants().Where(i => i.Attribute("Name")?.Value.Equals(moduleParentEl?.Attribute("Name")?.Value) ?? false);
                if (!existingParents.Any())
                {
                    ElementHelper temp = new(ElementInfo);
                    // Insert the parent module into the document
                    docToInsert = InsertElement(docToInsert, moduleParentEl);
                    ElementInfo = temp;
                }
            }

            // If the element is a task and it has any scheduled programs, insert them
            if (element.Name.ToString().Equals("Task") && element.Descendants("ScheduledProgram").Any())
            {
                // Save the current state of ElementInfo for the task
                ElementInfo.BulkProgramParentGen = element.Attribute("Name").Value;
                ElementHelper unmodified = new(ElementInfo);

                // Insert the programs scheduled in the task
                IEnumerable<string> programNames = element.Descendants("ScheduledProgram").Select(i => i.Attribute("Name").Value);
                List<XElement> programs = [.. inputFile.Descendants("Program")
                    .Where(i => programNames.Contains(i.Attribute("Name").Value.ToString()))
                    .Where(i => !docToInsert.Descendants("Program").Select(j => j.Attribute("Name").Value).Contains(i.Attribute("Name").Value))];
                if (programs.Count != 0)
                    InsertElement(docToInsert, programs);

                // Restore the previous state of ElementInfo
                ElementInfo.ResetElements();
                ElementInfo = unmodified;
            }

            return docToInsert;
        }

        /// <summary>
        /// Checks a list of elements for dependencies and inserts them into the provided document.
        /// </summary>
        /// <param name="docToInsert">The document to insert dependencies into.</param>
        /// <param name="elements">The list of elements to check for dependencies.</param>
        /// <returns>The updated document with dependencies inserted.</returns>
        internal XDocument CheckForDependencies(XDocument docToInsert, List<XElement> elements)
        {
            // Iterate through each element and check for dependencies
            foreach (XElement element in elements)
                docToInsert = CheckForDependencies(docToInsert, element);
            return docToInsert;
        }

        /// <summary>
        /// Finds the path from the specified element to the root schema element.
        /// </summary>
        /// <param name="element">The element to find the path for.</param>
        /// <returns>A linked list of element names representing the path to the root schema.</returns>
        /// <exception cref="EmptyListException">Thrown when the path to the root is empty.</exception>
        internal LinkedList<string> FindPathtoRootSchema(XElement element)
        {
            LinkedList<string> paths = new();
            string name = element.Name.ToString();
            XElement schemaElement = null;

            try
            {
                // Locate the schema element corresponding to the provided element
                schemaElement = validator.GetSchema().Descendants(Ns + "element").Single(i => i.Attribute("name")?.Value.Equals(name) ?? false);
                while (!schemaElement.Attribute("name").Value.Equals("RSLogix5000Content"))
                {
                    // Traverse up the schema hierarchy to find the path to the root
                    name = schemaElement.Parent.Parent.Attribute("name").Value;
                    schemaElement = validator.GetSchema().Descendants().Where(i => i.Name.Equals(Ns + "element")).Single(i => i.Attribute("type")?.Value.Equals(name) ?? false);
                    paths.AddLast(schemaElement.Attribute("name").Value);
                }
                if (paths.Count == 0)
                    throw new EmptyListException("Empty path to root. Started at RSLogix5000Content");
            }
            // Handle ambiguity in the schema path
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("Sequence contains more than one"))
                {
                    // Resolve ambiguity by selecting the appropriate parent type
                    IEnumerable<XElement> ambiguousElements = validator.GetSchema().Descendants(Ns + "element").Where(i => i.Attribute("type")?.Value.Equals(name) ?? false);
                    List<string> parentSchemaType = [.. validator.GetSchema().Descendants()
                        .Where(i => ambiguousElements.Select(x => x.Parent.Parent.Attribute("name")?.Value.ToString()).ToList()?.Contains(i.Attribute("name")?.Value) ?? false)
                        .Select(i => i.Attribute("name").Value)];
                    List<string> parentTypes = [.. validator.GetSchema().Descendants()
                        .Where(i => i.Name.Equals(Ns + "element"))
                        .Where(i => parentSchemaType.Contains(i.Attribute("type")?.Value))
                        .Select(i => i.Attribute("name").Value)];

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

                    // Recursively find the path to the root for the chosen parent type
                    LinkedList<string> grandparentToRoot = FindPathtoRootSchema(new XElement(chosenParentType));
                    foreach (string node in grandparentToRoot)
                        paths.AddLast(node);

                    return paths;
                }
                else
                {
                    throw;
                }
            }
            return paths;
        }

        /// <summary>
        /// Retrieves a list of simple element names from the XML schema, excluding "CustomProperties".
        /// </summary>
        /// <returns>A list of simple element names.</returns>
        internal List<String> GetSimpleElements()
        {
            IEnumerable<XElement> elements = validator.GetSchema().Descendants(Ns + "element");

            List<String> elementsInList = [];

            foreach (XElement element in elements)
            {
                // Exclude "CustomProperties" from the list of elements
                if (!element.Attribute("name").Value.Equals("CustomProperties"))
                    elementsInList.Add(element.Attribute("name").Value);
            }
            return elementsInList;
        }

        /// <summary>
        /// Retrieves the validation service instance.
        /// </summary>
        /// <returns>The validation service instance.</returns>
        internal IValidationService GetValidator() { return validator; }

        /// <summary>
        /// Inserts an element into the specified document, including its dependencies.
        /// </summary>
        /// <param name="docToInsert">The document to insert the element into.</param>
        /// <param name="insertEl">The element to insert.</param>
        /// <returns>The updated document with the element inserted.</returns>
        /// <exception cref="ClashingElementException">Thrown when the element already exists in the document.</exception>
        internal XDocument InsertElement(XDocument docToInsert, XElement insertEl)
        {
            XElement element = new(insertEl);
            docToInsert = CheckForDependencies(docToInsert, element);
            if ((ElementInfo.RootPath?.Count ?? 0) <= 1)
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

            // Check that the parent node exists in the document using the schema
            while (!docToInsert.Descendants(parentType).Any())
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
                        // Ensure edit information is up to date
                        XAttribute editedDate = new("EditedDate", DateTime.Now);
                        element.SetAttributeValue(editedDate.Name, editedDate.Value);
                    }

                    parentType = ElementInfo.GetFirst();
                }
                catch (InvalidOperationException)
                {
                    // Multiple options for parent element, already handled in FindPathtoRootSchema
                    parentType = ElementInfo.GetFirst();
                }
            }

            XElement parentNode = null;
            string grandparentType = ElementInfo.RootPath.First.Value;
            IEnumerable<XElement> grandParentNodes = docToInsert.Descendants(grandparentType);
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
                return InsertElement(docToInsert, parentNode);
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
            // If attributes are different, add the missing attributes to the parent
            else
            {
                foreach (XAttribute attribute in parentAttributes)
                {
                    if (!parentAttributes.Contains(attribute))
                        parentNode.Add(attribute);
                }
                parentNode.Add(element);
            }

            return docToInsert;
        }

        /// <summary>
        /// Inserts a list of elements into the specified document.
        /// </summary>
        /// <param name="doc">The document to insert elements into.</param>
        /// <param name="returnedElement">The list of elements to insert.</param>
        /// <returns>The updated document with the elements inserted.</returns>
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

        /// <summary>
        /// Loads a premade blank file containing the basic structure for the program.
        /// </summary>
        /// <returns>The loaded blank XML document.</returns>
        internal static XDocument LoadBasicFile()
        {
            XDocument doc = XDocument.Load("../../../../L5XFiles/TemplateFiles/EmptyTemplate.l5X");
            return doc;
        }

        /// <summary>
        /// Retrieves all possible attributes for the specified XML element from the schema.
        /// </summary>
        /// <param name="element">The element to retrieve attributes for.</param>
        /// <returns>A list of attributes for the element.</returns>
        internal List<XAttribute> GetAllAttributes(XElement element)
        {
            XElement basicSchemaElement = GetValidator().GetSchema().Descendants(Ns + "element").Where(i => i.Attribute("name")?.Value.ToString().Equals(element.Name.ToString()) ?? false).DescendantsAndSelf().Single();
            XElement elementAttr = GetValidator().GetSchema().Descendants(Ns + "complexType").Single(i => i.Attribute("name")?.Value.ToString().Equals(basicSchemaElement.Attribute("type").Value.ToString()) ?? false);

            IEnumerable<XElement> attributesEl = elementAttr.Elements(Ns + "attribute");
            List<XAttribute> attributesTochange = [];

            // Iterate through each attribute and add it to the list with 
            foreach (XElement attribute in attributesEl)
            {
                string attributeValue = "";
                // Add the required elements to list of attributes
                if (element.Attribute(attribute.Attribute("name").Value) != null)
                    attributeValue = element.Attribute(attribute.Attribute("name").Value).Value;

                XAttribute wantedAttribute = new(attribute.Attribute("name").Value.ToString(), attributeValue);
                attributesTochange.Add(wantedAttribute);
            }

            return attributesTochange;
        }

        /// <summary>
        /// Retrieves attributes for multiple XML elements.
        /// </summary>
        /// <param name="elements">The elements to retrieve attributes for.</param>
        /// <returns>A list of attribute lists, one for each element.</returns>
        internal List<List<XAttribute>> GetAttributes(IEnumerable<XElement> elements)
        {
            List<List<XAttribute>> allAttr = [];
            foreach (XElement el in elements)
                allAttr.Add(GetAllAttributes(el));
            return allAttr;
        }

        /// <summary>
        /// Retrieves a list of valid element types contained in the specified document.
        /// </summary>
        /// <param name="doc">The document to retrieve element types from.</param>
        /// <returns>A list of valid element types.</returns>
        /// <exception cref="EmptyListException">Thrown when no valid elements are found.</exception>
        internal List<string> GetElementTypes(XDocument doc)
        {
            // Select unique element types from the document
            List<string> uniqueTypes = [.. doc.Descendants().Where(i => acceptedTypes.Contains(i.Name?.ToString())).Select(i => i.Name.ToString()).Distinct()];

            if (uniqueTypes.Count == 0)
            {
                throw new EmptyListException("No Valid Elements Found");
            }
            return uniqueTypes;
        }

        #endregion
    }
}