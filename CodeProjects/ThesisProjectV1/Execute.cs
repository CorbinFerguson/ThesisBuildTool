using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using ThesisProjectV1.Forms;

namespace ThesisProjectV1
{
    internal class Execute
    {
        private static readonly XMLHandler xmlHandler = new XMLHandler();
        private static XDocument doc = new XDocument();
        private static readonly OpenFileDialog openFileSearch = new OpenFileDialog
        {
            Filter = "L5X Files (*.L5X)|*.L5X|All files (*.*)|*.*",
            FilterIndex = 0,
            RestoreDirectory = true
        };

        [STAThread]
        private static void Main()
        {

            // Take in user input
            while (true)
            {
                // Prompt user for selection
                ImportElement();
                break;
            }

            // Save the document to a file
            string genFilePath = "../../../L5XFiles/GenFile.L5X";
            doc.Save(genFilePath);

            Console.WriteLine($"XML file created at: {genFilePath}");
        }

        // Function for taking in an element from a file
        private static void ImportElement()
        {
            string filePath = "";
            bool insertElement = true;
            doc = xmlHandler.LoadBasicFile();
            openFileSearch.InitialDirectory = "../";
            while (true)
            {
                try
                {
                    // Select File being imported from
                    if (openFileSearch.ShowDialog() == DialogResult.OK)
                    {
                        filePath = openFileSearch.FileName;
                    }
                    else
                    {
                        insertElement = false;
                    }

                    while (insertElement)
                    {
                        // Prompt user to select type of element to insert
                        List<string> elementTypes = xmlHandler.GetDistinctTypes(filePath);
                        IEnumerable<XElement> typesWDataStruc = xmlHandler.GetValidator().GetSchema().Descendants().Where(i => i.Attribute("name") != null).Where(i => i.Attribute("name").Value.ToString().Equals("DataStructure")).Descendants();
                        elementTypes = elementTypes.Where(name => !typesWDataStruc.Any(x => (string)x.Attribute("name") == name)).ToList();

                        DropdownGui selectType = new DropdownGui(elementTypes, "Select type of the element to insert");
                        selectType.ShowDialog(out string typeOfElement);

                        // Get all elements in file of the type
                        List<string> availableElements = xmlHandler.GetElementsOfType(typeOfElement, filePath);
                        MultiSelectDropdown selectElement = new MultiSelectDropdown(availableElements, "Select elements to insert");
                        selectElement.ShowDialog(out List<string> nameOfElement);
                        XElement parentElement = new XElement(typeOfElement);
                        List<XElement> returnedElement = xmlHandler.GetElementFromFile(parentElement, filePath, nameOfElement);
                        doc = xmlHandler.InsertElement(doc, returnedElement);
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
    }
}
