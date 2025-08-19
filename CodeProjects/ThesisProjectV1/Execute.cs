using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Linq;
using ThesisProjectV1.Forms;

namespace ThesisProjectV1
{
    internal class Execute
    {
        private static readonly XMLHandler xmlHandler = new XMLHandler();
        private static XDocument doc = new XDocument();
        private static string filePath = "";
        private static readonly OpenFileDialog openFileSearch = new OpenFileDialog
        {
            Filter = "L5X Files (*.L5X)|*.L5X",
            FilterIndex = 0,
            RestoreDirectory = true
        };
        private static readonly string outputPath = "../../../L5XFiles/GeneratedFiles/";
        private static string outputName = "GenFile";

        [STAThread]
        private static void Main()
        {
            bool exitLoop = false;
            doc = xmlHandler.LoadBasicFile();
            // Take in user input
            while (!exitLoop)
            {
                ActionSelect actionSelect = new ActionSelect();
                actionSelect.ShowDialog(out ActionSelect.Actions selectedAction);
                switch (selectedAction)
                {
                    case ActionSelect.Actions.Import:
                        ImportElement();
                        break;
                    case ActionSelect.Actions.Generate:
                        GenerateElement();
                        break;
                    default:
                        exitLoop = true;
                        break;
                }
            }

            // Save the document to a file
            for (int i = 0; File.Exists(outputPath + outputName + ".L5X"); i++)
                outputName = Regex.Replace(outputName, @"\d", string.Empty) + i.ToString();
            doc.Save(outputPath + outputName + ".L5X");

            Console.WriteLine($"XML file created at: {outputPath}");
        }

        // Function for taking in an element from a file
        private static void ImportElement()
        {
            openFileSearch.InitialDirectory = "../";
            while (true)
            {
                bool insertElement = true;
                try
                {
                    // Select File being imported from
                    if (openFileSearch.ShowDialog() == DialogResult.OK)
                        filePath = openFileSearch.FileName;
                    else
                        insertElement = false;

                    while (insertElement)
                    {
                        string typeOfElement = xmlHandler.GetTypeAndSelect(filePath);

                        // Get all elements in file of the type
                        List<string> availableElements = xmlHandler.GetElementsOfType(typeOfElement, filePath);
                        MultiSelectDropdown selectElement = new MultiSelectDropdown(availableElements, "Select elements to insert");
                        selectElement.ShowDialog(out List<string> nameOfElement);
                        List<XElement> returnedElement = xmlHandler.GetElementFromFile(typeOfElement, filePath, nameOfElement);
                        doc = xmlHandler.InsertElement(doc, returnedElement);
                        doc = xmlHandler.CheckForDependencies(doc, filePath, returnedElement);
                        DialogResult newElementFile = MessageBox.Show("Add another element from file?", "Element Select", MessageBoxButtons.YesNo);
                        if (newElementFile == DialogResult.No)
                            break;
                    }
                }
                catch (EmptyListException ex)
                {
                    // Create a popup telling user what happened
                    MessageBox.Show(ex.Message, "Exception Creating List", MessageBoxButtons.OK);
                }

                // Validate the document against the xml Schema
                List<string> errorList = xmlHandler.GetValidator().ValidateL5XFile(doc);
                string errors = string.Join(Environment.NewLine, errorList);

                if (errors.Length > 0)
                {
                    MessageBox.Show(errors, "Errors", MessageBoxButtons.OK);
                    // TODO: when validation fails, Fix it
                }

                DialogResult endSelect = MessageBox.Show("End Selection?", "Element Select", MessageBoxButtons.YesNo);
                if (endSelect == DialogResult.Yes)
                    break;
            }
        }

        private static void GenerateElement()
        {
            while (true)
            {
                // Prompt user for what template file they would like to pull from
                openFileSearch.InitialDirectory = "../TemplateFiles/";
                try
                {
                    // Select File being imported from
                    if (openFileSearch.ShowDialog() == DialogResult.OK)
                        filePath = openFileSearch.FileName;
                    else
                        return;

                    // Prompt user to select the element to insert
                    string typeOfElement = xmlHandler.GetTypeAndSelect(filePath);
                    // Get all elements in file of the type
                    List<string> availableElements = xmlHandler.GetElementsOfType(typeOfElement, filePath);
                    MultiSelectDropdown selectElement = new MultiSelectDropdown(availableElements, "Select elements to insert");
                    selectElement.ShowDialog(out List<string> nameOfElement);

                    IEnumerable<XElement> elementList = xmlHandler.GetElementFromFile(typeOfElement, filePath, nameOfElement);

                    // Prompt user to select the subelements/values to change(required elements are not selectable)
                    foreach (XElement element in elementList)
                    {
                        XElement elementAttr;
                        try
                        {
                            XElement basicSchemaElement = xmlHandler.GetValidator().GetSchema().Descendants().Where(i => i.Attribute("name") != null).Where(i => i.Attribute("name").Value.ToString().Equals(element.Name.ToString())).DescendantsAndSelf().Single();
                            elementAttr = xmlHandler.GetValidator().GetSchema().Descendants().Where(i => i.Name.Equals(xmlHandler.Ns + "complexType")).Where(i => i.Attribute("name") != null).Where(i => i.Attribute("name").Value.ToString().Equals(basicSchemaElement.Attribute("type").Value.ToString())).Single();
                        }
                        catch (InvalidOperationException)
                        {
                            // Get tag type from document file
                            XElement grandparent = element.Parent.Parent;
                            elementAttr = xmlHandler.GetValidator().GetSchema().Descendants(grandparent.Name).Elements().Elements().Where(i => i.Attribute("name") != null).Where(i => i.Attribute("name").Value.ToString().Equals(element.Name.ToString())).Single();

                            // Search for complexType with name of type of element
                        }
                        IEnumerable<XElement> attributesEl = elementAttr.Elements().Where(i => i.Name.Equals(xmlHandler.Ns + "attribute")); // TODO: this should be aquired from the schema
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

                        xmlHandler.InsertElement(doc, element);
                        doc = xmlHandler.CheckForDependencies(doc, filePath, element);
                    }
                }
                catch (EmptyListException ex)
                {
                    // Create a popup telling user what happened
                    MessageBox.Show(ex.Message, "Exception Creating List", MessageBoxButtons.OK);
                }

                // Validate the document against the xml Schema
                List<string> errorList = xmlHandler.GetValidator().ValidateL5XFile(doc);
                string errors = string.Join(Environment.NewLine, errorList);

                if (errors.Length > 0)
                {
                    MessageBox.Show(errors, "Errors", MessageBoxButtons.OK);
                    continue;
                }

                DialogResult endSelect = MessageBox.Show("End Creation of Elements?", "Element Select", MessageBoxButtons.YesNo);
                if (endSelect == DialogResult.Yes)
                    break;
            }
        }
    }
}
