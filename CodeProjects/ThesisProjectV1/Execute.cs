using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Transactions;
using System.Windows.Forms;
using System.Xml.Linq;
using ThesisProjectV1.Forms;

namespace ThesisProjectV1
{
    internal class Execute
    {
        private static readonly XMLHandler xmlHandler = new XMLHandler();
        public static XDocument doc = new XDocument();
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
            // Default to generated basic file
            NewFile();

            // Take in user input
            ActionSelect actionSelect = new ActionSelect();
            actionSelect.ShowDialog();
        }

        public static void NewFile()
        {
            doc = xmlHandler.LoadBasicFile();
        }

        public static void SaveFile()
        {
            TextInput fileName = new TextInput("File name", outputName, @"^\w+$");
            fileName.ShowDialog(out outputName);

            // Save the document to a file
            for (int i = 0; File.Exists(outputPath + outputName + ".L5X"); i++)
                outputName = Regex.Replace(outputName, @"\d", string.Empty) + i.ToString();

            doc.Save(outputPath + outputName + ".L5X");
            Console.WriteLine($"XML file created at: {outputPath}");
        }

        public static void LoadFile()
        {
            // Select File being loaded to modifiy
            if (openFileSearch.ShowDialog() == DialogResult.OK)
                doc = XDocument.Load(openFileSearch.FileName);
        }

        public static void ModifyElement()
        {
            using (TransactionScope transaction = new TransactionScope())
            {
                string typeSelected = xmlHandler.GetElementTypes(doc);
                List<string> namesAvailable = doc.Descendants(typeSelected).Select(i => i.Attribute("Name")?.Value.ToString() ?? i.Attribute("CatalogNumber").Value.ToString()).Distinct().ToList();
                MultiSelectDropdown nameSelect = new MultiSelectDropdown(namesAvailable, "Select Names of elements to modify");
                nameSelect.ShowDialog(out List<string> namesSelected);

                foreach (string elementName in namesSelected)
                {
                    IEnumerable<XElement> elements = doc.Descendants(typeSelected).Where(i => (i.Attribute("Name")?.Value ?? i.Attribute("CatalogNumber").Value) == elementName);
                    foreach (XElement element in elements)
                    {
                        xmlHandler.GetSetAttributes(element);
                    }
                }

                // Validate the document against the xml Schema, if successful, complete transaction
                List<string> errorList = xmlHandler.GetValidator().ValidateL5XFile(doc);
                string errors = string.Join(Environment.NewLine, errorList);

                if (errors.Length > 0)
                {
                    MessageBox.Show(errors, "Error: undoing action", MessageBoxButtons.OK);
                    transaction.Dispose();
                }
                else
                {
                    transaction.Complete();
                }
            }
        }

        public static void DeleteElement()
        {
            using(TransactionScope transaction = new TransactionScope()) 
            {
                string typeSelected = xmlHandler.GetElementTypes(doc);
                List<string> namesAvailable = doc.Descendants(typeSelected).Select(i => i.Attribute("Name")?.Value.ToString() ?? i.Attribute("CatalogNumber").Value).ToList();
                MultiSelectDropdown nameSelect = new MultiSelectDropdown(namesAvailable, "Select Names of elements to remove");
                nameSelect.ShowDialog(out List<string> namesSelected);

                // For all selected element names, remove the associated element
                foreach (string name in namesSelected)
                {
                    XElement removeElement = doc.Descendants(typeSelected).Single(i => i.Attribute("Name")?.Value.Equals(name) ?? i.Attribute("CatalogNumber").Value.Equals(name));
                    removeElement.Remove();
                }
                // Validate the document against the xml Schema, if successful, complete transaction
                List<string> errorList = xmlHandler.GetValidator().ValidateL5XFile(doc);
                string errors = string.Join(Environment.NewLine, errorList);

                if (errors.Length > 0)
                {
                    MessageBox.Show(errors, "Error: undoing action", MessageBoxButtons.OK);
                    transaction.Dispose();
                }
                else
                {
                    transaction.Complete();
                }
            }
        }

        // Function for taking in an element from a file
        public static void ImportElement()
        {
            openFileSearch.InitialDirectory = "../";
            bool insertElement = true;

            // Select File being imported from
            if (openFileSearch.ShowDialog() == DialogResult.OK)
                xmlHandler.inputFile = XDocument.Load(openFileSearch.FileName);
            else
                insertElement = false;

            // While loop to contain importing elements from chosen file
            while (insertElement)
            {
                using (TransactionScope transaction = new TransactionScope())
                {
                    string typeOfElement = xmlHandler.GetTypeAndSelect();
                    string attributeFilter = "Name";

                    if (typeOfElement.Equals("Module"))
                        attributeFilter = "CatalogNumber";

                    // Get all elements in file of the type
                    List<string> availableElements = xmlHandler.inputFile.Descendants(typeOfElement).Select(i => i.Attribute(attributeFilter)?.Value.ToString()).Distinct().ToList();
                    MultiSelectDropdown selectElement = new MultiSelectDropdown(availableElements, "Select elements to insert");
                    selectElement.ShowDialog(out List<string> nameOfElement);
                    List<XElement> returnedElement = xmlHandler.GetElementFromFile(typeOfElement, nameOfElement);
                    doc = xmlHandler.InsertElement(doc, returnedElement);

                    // Validate the document against the xml Schema, if successful, complete transaction
                    List<string> errorList = xmlHandler.GetValidator().ValidateL5XFile(doc);
                    string errors = string.Join(Environment.NewLine, errorList);

                    if (errors.Length > 0)
                    {
                        MessageBox.Show(errors, "Error: undoing action", MessageBoxButtons.OK);
                        transaction.Dispose();
                    }
                    else
                    {
                        transaction.Complete();
                    }

                    DialogResult newElementFile = MessageBox.Show("Add another element from file?", "Element Select", MessageBoxButtons.YesNo);
                    if (newElementFile == DialogResult.No)
                        break;
                }
            }
        }

        public static void GenerateElement()
        {
            using (TransactionScope transaction = new TransactionScope())
            {
                // Prompt user for what template file they would like to pull from
                openFileSearch.InitialDirectory = "../TemplateFiles/";
                // Select File being imported from
                if (openFileSearch.ShowDialog() == DialogResult.OK)
                    xmlHandler.inputFile = XDocument.Load(openFileSearch.FileName);
                else
                    return;

                // Prompt user to select the element to insert
                string typeOfElement = xmlHandler.GetTypeAndSelect();
                // Get all elements in file of the type
                string attributeSearch = "Name";
                // IO Modules do not require a name. Must search by catalog number instead
                if (typeOfElement.Equals("Module"))
                    attributeSearch = "CatalogNumber";

                // Find and display available elements of that type
                List<string> availableElements = xmlHandler.inputFile.Descendants(typeOfElement).Select(i => i.Attribute(attributeSearch).Value.ToString()).ToList();
                MultiSelectDropdown selectElement = new MultiSelectDropdown(availableElements, "Select elements to insert");
                selectElement.ShowDialog(out List<string> nameOfElement);

                IEnumerable<XElement> elementList = xmlHandler.GetElementFromFile(typeOfElement, nameOfElement);

                // Prompt user to select the subelements/values to change(required elements are not selectable)
                foreach (XElement element in elementList)
                {
                    XElement setElement = xmlHandler.GetSetAttributes(element);

                    xmlHandler.InsertElement(doc, setElement);
                }
                // Validate the document against the xml Schema, if successful, complete transaction
                List<string> errorList = xmlHandler.GetValidator().ValidateL5XFile(doc);
                string errors = string.Join(Environment.NewLine, errorList);

                if (errors.Length > 0)
                {
                    MessageBox.Show(errors, "Error: undoing action", MessageBoxButtons.OK);
                    transaction.Dispose();
                }
                else
                {
                    transaction.Complete();
                }
            }
        }
    }
}
