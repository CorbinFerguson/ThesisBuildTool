
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System;
using ThesisProjectV1.Abstractions;
using ThesisProjectV1;
using System.Linq;

public sealed class Execute
{
    private readonly XMLHandler _xml;
    private readonly IMessageService _messages;
    private readonly IUserPromptService _prompts;
    private readonly IOpenFileService _openFile;
    private readonly ISaveFileService _saveFile;
    private readonly IValidationService _validation;
    private readonly IFileSystem _fs;

    private readonly string _outputPath = "../../../L5XFiles/GeneratedFiles/";
    private string _outputName = "GenFile";

    public XDocument Doc { get; private set; }

    public Execute(
        XMLHandler xml,
        IMessageService messages,
        IUserPromptService prompts,
        IOpenFileService openFile,
        ISaveFileService saveFile,
        IValidationService validation,
        IFileSystem fs)
    {
        _xml = xml;
        _messages = messages;
        _prompts = prompts;
        _openFile = openFile;
        _saveFile = saveFile;
        _validation = validation;
        _fs = fs;

        Doc = new XDocument();
    }

    public void InitializeNew()
    {
        Doc = _xml.LoadBasicFile();
    }

    public void ValidateFile(bool showNoError)
    {
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
        bool ok = _messages.Confirm(
            "Are you sure you want to overwrite your existing file?",
            "Verify File Creation");

        if (ok)
        {
            Doc = _xml.LoadBasicFile();
            _messages.Show("New Empty File Created.", "Info");
        }
    }

    public void SaveFile()
    {
        // Save the document to a file with deduped name
        int i = 0;
        while (_fs.FileExists(System.IO.Path.Combine(_outputPath, _outputName + ".L5X")))
        {
            _outputName = Regex.Replace(_outputName, @"\d", string.Empty) + i.ToString();
            i++;
        }

        var filter =
            "L5X Files (*.L5X)|*.L5X|XML Files (*.XML)|*.xml|All Files (*.*)|*.*";

        string filePath;
        if (_saveFile.TrySave(_outputName, filter, "L5X", out filePath))
        {
            _fs.SaveXml(Doc, filePath);
        }
    }

    public void LoadFile()
    {
        string filePath;
        if (_openFile.TryOpen("L5X Files (*.L5X)|*.L5X", out filePath, null))
        {
            Doc = _fs.LoadXml(filePath);
        }
    }



    public void ModifyElement()
    {
        // 1) Pick the type from the current document
        List<string> types = _xml.GetInteractableTypesFromDocument(Doc);
        if (types == null || types.Count == 0)
            throw new EmptyListException("No Valid Elements Found");

        string typeSelected = _prompts.SelectOne("Select Element Types", types, "Modify Element");

        // 2) Build list of element identifiers to show to the user (Name or CatalogNumber)
        string attributeKey = (typeSelected == "Module") ? "CatalogNumber" : "Name";

        List<string> namesAvailable = Doc
            .Descendants(typeSelected)
            .Select(i =>
            {
                XAttribute keyAttr = i.Attribute(attributeKey);
                if (keyAttr != null && !string.IsNullOrEmpty(keyAttr.Value))
                    return keyAttr.Value;

                // Fallbacks for modules without Name (handled with CatalogNumber already) or odd cases
                XAttribute nameAttr = i.Attribute("Name");
                if (nameAttr != null && !string.IsNullOrEmpty(nameAttr.Value))
                    return nameAttr.Value;

                return null;
            })
            .Where(v => !string.IsNullOrEmpty(v))
            .Distinct()
            .ToList();

        if (namesAvailable.Count == 0)
        {
            _messages.ShowError("No elements of the selected type exist in the document.", "Modify Element");
            return;
        }

        // 3) Let user select one or more element identifiers to modify
        IList<string> namesSelected = _prompts.SelectMany(
            "Select Names of elements to modify",
            namesAvailable,
            false,
            "Modify Element");

        if (namesSelected == null || namesSelected.Count == 0)
            return;

        // 4) Resolve actual XElement instances to modify (including disambiguation)
        List<XElement> elementsToModify = new List<XElement>();

        foreach (string name in namesSelected)
        {
            IEnumerable<XElement> matches = Doc.Descendants(typeSelected)
                .Where(i => i.Attribute(attributeKey) != null && i.Attribute(attributeKey).Value == name);

            if (typeSelected != "Module")
            {
                // Non-Module: if ambiguous (same name under different parents), ask for the grandparent
                int count = matches.Count();
                if (count == 0)
                    continue;

                if (count == 1)
                {
                    elementsToModify.Add(matches.Single());
                }
                else
                {
                    // Build grandparent options (parent.Parent Name)
                    List<string> parentOptions = matches
                        .Select(i => i.Parent != null && i.Parent.Parent != null && i.Parent.Parent.Attribute("Name") != null
                                        ? i.Parent.Parent.Attribute("Name").Value
                                        : null)
                        .Where(v => !string.IsNullOrEmpty(v))
                        .Distinct()
                        .ToList();

                    string selectedParent = _prompts.SelectOne(
                        "Multiple elements named '" + name + "'. Select grandparent element you are accessing",
                        parentOptions,
                        "Modify Element");

                    XElement parentElement = Doc
                        .Descendants()
                        .Single(i => i.Attribute("Name") != null && i.Attribute("Name").Value == selectedParent);

                    XElement resolved = parentElement
                        .Descendants(typeSelected)
                        .Single(i => i.Attribute("Name") != null && i.Attribute("Name").Value == name);

                    elementsToModify.Add(resolved);
                }
            }
            else
            {
                // Module case: may need to distinguish by Name or by Port when Name is missing
                int count = matches.Count();
                if (count == 0)
                    continue;

                if (count == 1)
                {
                    elementsToModify.Add(matches.Single());
                }
                else
                {
                    // Build rich options: either by Name or "CatalogNumber with no Name at port <address>"
                    List<string> options = new List<string>();
                    foreach (XElement m in matches)
                    {
                        XAttribute nameAttr = m.Attribute("Name");
                        if (nameAttr != null && !string.IsNullOrEmpty(nameAttr.Value))
                        {
                            options.Add(nameAttr.Value);
                        }
                        else
                        {
                            // Find first Port Address to disambiguate
                            XElement firstPort = m.Descendants("Port").FirstOrDefault();
                            string portAddr = (firstPort != null && firstPort.Attribute("Address") != null)
                                ? firstPort.Attribute("Address").Value
                                : "?";
                            options.Add(name + " with no Name at port " + portAddr);
                        }
                    }

                    // Allow selecting multiple modules among the ambiguous set
                    IList<string> chosen = _prompts.SelectMany(
                        "Select the name(s) of the Module " + name + " you are accessing",
                        options,
                        false,
                        "Modify Element");

                    if (chosen != null)
                    {
                        foreach (string choice in chosen)
                        {
                            XElement resolved;
                            if (choice.Contains("at port"))
                            {
                                int portIndex = choice.IndexOf("port ");
                                string portStr = portIndex >= 0 ? choice.Substring(portIndex + 5) : "";
                                resolved = matches.Single(i =>
                                    i.Descendants("Port").Where(j => j.Attribute("Address") != null && j.Attribute("Address").Value == portStr).Any());
                            }
                            else
                            {
                                resolved = matches.Single(i => i.Attribute("Name") != null && i.Attribute("Name").Value == choice);
                            }
                            elementsToModify.Add(resolved);
                        }
                    }
                }
            }
        }

        if (elementsToModify.Count == 0)
            return;

        // 5) Delegate to XMLHandler to prompt for attributes/children (via IUserPromptService)
        _xml.GetSetAttributes(elementsToModify, _prompts, _messages);

        // 6) Validate silently
        ValidateFile(false);
    }

    public void DeleteElement()
    {
        // 1) Ask for element type (headless)
        List<string> types = _xml.GetElementTypesFromDoc(Doc);
        if (types == null || types.Count == 0)
        {
            _messages.ShowError("No valid elements found in the current document.", "Delete Element");
            return;
        }

        string typeSelected = _prompts.SelectOne(
            "Select element type to remove",
            types,
            "Delete Element");

        // 2) Build the selectable list of elements for that type
        List<string> options = new List<string>();
        Dictionary<string, XElement> mapByDisplay = new Dictionary<string, XElement>();

        if (typeSelected == "Module")
        {
            // Disambiguate Modules:
            //  - Named modules are listed by Name
            //  - Unnamed modules are listed by "CatalogNumber (no Name at port X)"
            IEnumerable<XElement> modules = Doc.Descendants("Module");

            foreach (var m in modules)
            {
                XAttribute nameAttr = m.Attribute("Name");
                if (nameAttr != null && !string.IsNullOrWhiteSpace(nameAttr.Value))
                {
                    string display = nameAttr.Value;
                    if (!mapByDisplay.ContainsKey(display))
                    {
                        mapByDisplay.Add(display, m);
                        options.Add(display);
                    }
                    else
                    {
                        // Rare duplicate; add a suffix to keep it selectable
                        string displayDup = display + " (duplicate)";
                        mapByDisplay.Add(displayDup, m);
                        options.Add(displayDup);
                    }
                }
                else
                {
                    string cat = (m.Attribute("CatalogNumber") != null) ? m.Attribute("CatalogNumber").Value : "UnknownCatalog";
                    // Try to use the first Port's Address for disambiguation (matches your insertion/disambiguation logic)
                    string portAddr = "?";
                    var firstPort = m.Descendants("Port").FirstOrDefault();
                    if (firstPort != null && firstPort.Attribute("Address") != null)
                        portAddr = firstPort.Attribute("Address").Value;

                    string display = cat + " (no Name at port " + portAddr + ")";
                    // Avoid collisions if multiple unnamed modules share same catalog/port presentation
                    int suffix = 1;
                    string unique = display;
                    while (mapByDisplay.ContainsKey(unique))
                    {
                        suffix++;
                        unique = display + " #" + suffix.ToString();
                    }
                    mapByDisplay.Add(unique, m);
                    options.Add(unique);
                }
            }
        }
        else
        {
            // Non-module types: display by Name
            IEnumerable<XElement> elems = Doc.Descendants(typeSelected)
                .Where(e => e.Attribute("Name") != null);

            foreach (var e in elems)
            {
                string display = e.Attribute("Name").Value;
                if (!mapByDisplay.ContainsKey(display))
                {
                    mapByDisplay.Add(display, e);
                    options.Add(display);
                }
                else
                {
                    // Defensive suffix for any duplicates (should be rare/invalid)
                    string displayDup = display + " (duplicate)";
                    mapByDisplay.Add(displayDup, e);
                    options.Add(displayDup);
                }
            }
        }

        if (options.Count == 0)
        {
            _messages.Show("No elements of the selected type exist in the document.", "Delete Element");
            return;
        }

        // 3) Let the user pick which items to delete (multi-select, none not allowed)
        IList<string> selectedDisplays = _prompts.SelectMany(
            "Select names of elements to remove",
            options,
            false,
            "Delete Element");

        if (selectedDisplays == null || selectedDisplays.Count == 0)
        {
            // User cancelled or selected nothing
            return;
        }

        // 4) Resolve selections into concrete elements
        //    Since we used a dictionary keyed by display label, resolution is direct.
        List<XElement> elementsToRemove = new List<XElement>();
        foreach (var label in selectedDisplays)
        {
            XElement el;
            if (mapByDisplay.TryGetValue(label, out el))
            {
                elementsToRemove.Add(el);
            }
        }

        if (elementsToRemove.Count == 0)
        {
            _messages.Show("No matching elements were found to remove.", "Delete Element");
            return;
        }

        // 5) Optional confirmation
        bool confirmed = _messages.Confirm(
            "Remove " + elementsToRemove.Count.ToString() + " element(s)?",
            "Delete Element");

        if (!confirmed)
            return;

        // 6) Remove the selected elements and validate
        //    (Use the LINQ-to-XML extension Remove() over IEnumerable<XElement>)
        elementsToRemove.Remove(); // extension method removes from their parents

        ValidateFile(false);
    }

    public void ImportElement()
    {
        // 1) Ask user for a source .L5X file (headless)
        string filePath;
        bool picked = _openFile.TryOpen("L5X Files (*.L5X)|*.L5X", out filePath, "../");
        if (!picked || string.IsNullOrWhiteSpace(filePath))
        {
            return; // user canceled
        }

        // Load the chosen file into XMLHandler.inputFile (source file for imports)
        _xml.inputFile = _fs.LoadXml(filePath);

        // 2) Loop to allow the user to import multiple batches from the same file
        bool insertElement = true;
        while (insertElement)
        {
            // a) Pick the element TYPE to import (from the source file)
            List<string> elementTypes = _xml.GetAvailableTypesFromInputFile();
            if (elementTypes == null || elementTypes.Count == 0)
            {
                _messages.ShowError("No valid element types found in the selected file.", "Import Element");
                return;
            }

            string typeOfElement = _prompts.SelectOne(
                "Select type of the element(s) to insert",
                elementTypes,
                "Import Element");

            // b) Build the list of available elements (Name or CatalogNumber)
            string attributeFilter = (typeOfElement == "Module") ? "CatalogNumber" : "Name";

            List<string> availableElements = _xml.inputFile
                .Descendants(typeOfElement)
                .Select(i =>
                {
                    XAttribute a = i.Attribute(attributeFilter);
                    return (a != null && !string.IsNullOrEmpty(a.Value)) ? a.Value : null;
                })
                .Where(v => !string.IsNullOrEmpty(v))
                .Distinct()
                .ToList();

            if (availableElements.Count == 0)
            {
                _messages.ShowError("No elements of the selected type exist in the source file.", "Import Element");
                // Ask if they want to try another type
                insertElement = _messages.Confirm("Try another type from the same file?", "Import Element");
                if (!insertElement) return;
                continue;
            }

            // c) Let the user choose one or more element identifiers to insert
            IList<string> chosenIdentifiers = _prompts.SelectMany(
                "Select elements to insert",
                availableElements,
                false,
                "Import Element");

            if (chosenIdentifiers == null || chosenIdentifiers.Count == 0)
            {
                // No selection; ask whether to continue selecting from this file
                insertElement = _messages.Confirm("No elements selected. Choose another type from this file?", "Import Element");
                if (!insertElement) return;
                continue;
            }

            // d) Resolve actual elements from the source file using prompt‑based disambiguation (no WinForms)
            List<XElement> elementsToInsert = ResolveElementsFromInputFile(typeOfElement, attributeFilter, chosenIdentifiers);

            if (elementsToInsert.Count == 0)
            {
                _messages.ShowError("No matching elements were resolved from the source file.", "Import Element");
                insertElement = _messages.Confirm("Would you like to import other elements from this file?", "Import Element");
                continue;
            }

            // e) Insert into the current Doc (handles dependencies, parents, clashes, etc.)
            Doc = _xml.InsertElement(Doc, elementsToInsert);

            // Reset helper state and validate silently
            _xml.ElementInfo.ResetElements();
            ValidateFile(false);

            // f) Ask whether to import another batch from the same file
            insertElement = _messages.Confirm("Add another element from this file?", "Import Element");
        }
    }


    public void CreateElement()
    {
        // 1) Load the template project the same way, but via IFileSystem (testable)
        _xml.inputFile = _fs.LoadXml("../../../L5XFiles/TemplateFiles/TemplateProjectV1.L5X");

        // 2) Ask for element type using IUserPromptService
        List<string> elementTypes = _xml.GetAvailableTypesFromInputFile();
        if (elementTypes == null || elementTypes.Count == 0)
        {
            _messages.ShowError("No valid element types found in the template file.", "Create Element");
            return;
        }

        string typeOfElement = _prompts.SelectOne(
            "Select type of the element to insert",
            elementTypes,
            "Create Element");

        // 3) Build the list of available templates for this type (by Name or CatalogNumber)
        string attributeSearch = typeOfElement == "Module" ? "CatalogNumber" : "Name";

        List<string> availableElements = _xml.inputFile
            .Descendants(typeOfElement)
            .Select(i => i.Attribute(attributeSearch) != null ? i.Attribute(attributeSearch).Value : null)
            .Where(v => !string.IsNullOrEmpty(v))
            .Distinct()
            .ToList();

        if (availableElements.Count == 0)
        {
            _messages.ShowError("No templates found for the selected type.", "Create Element");
            return;
        }

        string nameOfElement = _prompts.SelectOne(
            "Select template " + typeOfElement + " to use",
            availableElements,
            "Create Element");

        // 4) Get the template element (no UI – Single() as before)
        XElement element = _xml.GetElementFromFile(typeOfElement, nameOfElement).Single();

        // 5) Compute valid parent types from the schema, then (if needed) ask which parent element to use
        XDocument schema = _xml.GetValidator().GetSchema();

        // Example: find <xs:element name="Programs"> when typeOfElement == "Program"
        var ambiguousElements = schema
            .Descendants(_xml.Ns + "element")
            .Where(i => i.Attribute("name") != null &&
                        i.Attribute("name").Value == (typeOfElement + "s"));

        List<string> parentSchemaType = schema
            .Descendants()
            .Where(i =>
            {
                var names = ambiguousElements
                    .Select(x => x.Parent != null && x.Parent.Parent != null && x.Parent.Parent.Attribute("name") != null
                                ? x.Parent.Parent.Attribute("name").Value
                                : null)
                    .Where(v => !string.IsNullOrEmpty(v))
                    .ToList();
                var attr = i.Attribute("name");
                return attr != null && names.Contains(attr.Value);
            })
            .Select(i => i.Attribute("name").Value)
            .ToList();

        List<string> parentType = schema
            .Descendants(_xml.Ns + "element")
            .Where(i => i.Attribute("type") != null && parentSchemaType.Contains(i.Attribute("type").Value))
            .Select(i => i.Attribute("name").Value)
            .ToList();

        if (parentType.Count > 1)
        {
            // Get all valid parent elements of these types from the current document
            var parentElems = Doc
                .Descendants()
                .Where(i => i.Name != null && parentType.Contains(i.Name.ToString()));

            List<string> parents = parentElems
                .Select(i => i.Attribute("Name") != null ? i.Attribute("Name").Value : null)
                .Where(v => !string.IsNullOrEmpty(v))
                .ToList();

            if (parents.Count == 0)
            {
                _messages.ShowError("No valid parent elements exist in the current document.", "Create Element");
                return;
            }

            string parentName = _prompts.SelectOne(
                "Select parent for the elements being created",
                parents,
                "Create Element");

            _xml.ElementInfo.ParentElementBulk = parentElems
                .Single(i => i.Attribute("Name") != null && i.Attribute("Name").Value == parentName);
        }
        else if (parentType.Count != 1)
        {
            throw new EmptyListException("Attempted to Create an item with no valid parents");
        }

        // 6) Quantity prompt
        string qtyStr = _prompts.Prompt(
            "How many " + element.Name + " would you like to create?",
            "1",
            @"^[1-9]\d*$",
            "Create Element");

        int quantity = 1;
        if (!int.TryParse(string.IsNullOrWhiteSpace(qtyStr) ? "1" : qtyStr, out quantity) || quantity < 1)
            quantity = 1;

        // 7) Create and insert the elements; collect program names if needed for scheduling
        List<string> bulkNames = new List<string>();
        for (int i = 0; i < quantity; i++)
        {
            // Set Name if appropriate (Modules with Use attribute may not require Name)
            if (typeOfElement != "Module" || element.Attribute("Use") == null)
            {
                string defaultName = "";
                string itemName = _prompts.Prompt(
                    "Input a name for created " + element.Name + " #" + (i + 1).ToString(),
                    defaultName,
                    @"^[a-zA-Z]+(\w*[A-Za-z0-9])*$",
                    "Create Element");

                // Ensure a value exists (basic guard – original allowed empty only through UI)
                if (string.IsNullOrWhiteSpace(itemName))
                {
                    _messages.ShowError("A non-empty name is required.", "Create Element");
                    return;
                }

                element.SetAttributeValue("Name", itemName);
                bulkNames.Add(itemName);
            }

            // Insert the element
            _xml.InsertElement(Doc, element);

            // Reset path for next insert if available
            if (_xml.ElementInfo.RootPath != null)
                _xml.ElementInfo.RootPath.Clear();
        }

        // Reset ElementInfo for safety (as in original)
        _xml.ElementInfo.ResetElements();

        // 8) Special case: Programs can be scheduled to a Task
        if (typeOfElement == "Program")
        {
            var tasks = Doc.Descendants("Task");
            if (tasks.Any())
            {
                List<string> taskNames = tasks
                    .Select(i => i.Attribute("Name") != null ? i.Attribute("Name").Value : null)
                    .Where(v => !string.IsNullOrEmpty(v))
                    .ToList();

                taskNames.Add("Keep programs unscheduled");

                string taskName = _prompts.SelectOne(
                    "Select the parent task for inserted programs",
                    taskNames,
                    "Create Element");

                if (taskName != "Keep programs unscheduled")
                {
                    XElement taskElement = tasks.Single(i => i.Attribute("Name") != null && i.Attribute("Name").Value == taskName);
                    XElement scheduledParent = null;

                    if (taskElement.Elements("ScheduledPrograms").Any())
                    {
                        scheduledParent = taskElement.Elements("ScheduledPrograms").Single();
                    }
                    else
                    {
                        scheduledParent = new XElement("ScheduledPrograms");
                    }

                    foreach (string programName in bulkNames)
                    {
                        var scheduledProgram = new XElement("ScheduledProgram", new XAttribute("Name", programName));
                        scheduledParent.Add(scheduledProgram);
                    }

                    if (!taskElement.Elements("ScheduledPrograms").Any())
                    {
                        taskElement.Add(scheduledParent);
                    }
                }
            }
        }

        // 9) Validate silently (no "no errors" message)
        ValidateFile(false);
    }


    /// <summary>
    /// Resolves one or more elements from the currently loaded _xml.inputFile by
    /// (type, attributeFilter, identifiers), and performs prompt-based disambiguation:
    ///  - Non-Module: if multiple elements share the same Name, disambiguate by grandparent "Name".
    ///  - Module: when Name is missing, disambiguate by Port Address ("... at port <addr>").
    /// </summary>
    private List<XElement> ResolveElementsFromInputFile(string typeOfElement, string attributeFilter, IList<string> identifiers)
    {
        List<XElement> results = new List<XElement>();

        foreach (string id in identifiers)
        {
            IEnumerable<XElement> candidates = _xml.inputFile
                .Descendants(typeOfElement)
                .Where(i => i.Attribute(attributeFilter) != null && i.Attribute(attributeFilter).Value == id);

            int count = candidates.Count();
            if (count == 0)
            {
                // No match with the chosen identifier; skip it.
                continue;
            }

            if (typeOfElement != "Module")
            {
                if (count == 1)
                {
                    results.Add(candidates.Single());
                }
                else
                {
                    // Multiple elements with same Name -> disambiguate by grandparent Name
                    List<string> parentOptions = candidates
                        .Select(i => i.Parent != null && i.Parent.Parent != null && i.Parent.Parent.Attribute("Name") != null
                                        ? i.Parent.Parent.Attribute("Name").Value
                                        : null)
                        .Where(v => !string.IsNullOrEmpty(v))
                        .Distinct()
                        .ToList();

                    string grandparentName = _prompts.SelectOne(
                        "Multiple elements named '" + id + "'. Select grandparent element you are accessing",
                        parentOptions,
                        "Import Element");

                    XElement parentElement = _xml.inputFile
                        .Descendants()
                        .Single(i => i.Attribute("Name") != null && i.Attribute("Name").Value == grandparentName);

                    XElement resolved = parentElement
                        .Descendants(typeOfElement)
                        .Single(i => i.Attribute("Name") != null && i.Attribute("Name").Value == id);

                    results.Add(resolved);
                }
            }
            else
            {
                // Module disambiguation: Name may be missing; differentiate by Module Name or Port Address
                if (count == 1)
                {
                    results.Add(candidates.Single());
                }
                else
                {
                    List<string> options = new List<string>();
                    foreach (XElement m in candidates)
                    {
                        XAttribute nameAttr = m.Attribute("Name");
                        if (nameAttr != null && !string.IsNullOrEmpty(nameAttr.Value))
                        {
                            options.Add(nameAttr.Value);
                        }
                        else
                        {
                            XElement firstPort = m.Descendants("Port").FirstOrDefault();
                            string portAddr = (firstPort != null && firstPort.Attribute("Address") != null)
                                ? firstPort.Attribute("Address").Value
                                : "?";
                            options.Add(id + " with no Name at port " + portAddr);
                        }
                    }

                    IList<string> chosen = _prompts.SelectMany(
                        "Select the specific Module(s) to insert for " + id,
                        options,
                        false,
                        "Import Element");

                    if (chosen != null)
                    {
                        foreach (string choice in chosen)
                        {
                            XElement resolved;
                            if (choice.Contains("at port"))
                            {
                                int portIndex = choice.IndexOf("port ");
                                string portStr = (portIndex >= 0 && portIndex + 5 <= choice.Length)
                                    ? choice.Substring(portIndex + 5)
                                    : "";

                                resolved = candidates.Single(i =>
                                    i.Descendants("Port")
                                        .Where(j => j.Attribute("Address") != null && j.Attribute("Address").Value == portStr)
                                        .Any());
                            }
                            else
                            {
                                resolved = candidates.Single(i => i.Attribute("Name") != null && i.Attribute("Name").Value == choice);
                            }

                            results.Add(resolved);
                        }
                    }
                }
            }
        }

        return results;
    }

}
