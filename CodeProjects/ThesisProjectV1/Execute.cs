using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
                    MessageBox.Show(errors, "Validation error: undoing action", MessageBoxButtons.OK);
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
                    MessageBox.Show(errors, "Error: aborting action", MessageBoxButtons.OK);
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
                    xmlHandler.ElementInfo.ResetElements();

                    // Validate the document against the xml Schema, if successful, complete transaction
                    List<string> errorList = xmlHandler.GetValidator().ValidateL5XFile(doc);
                    string errors = string.Join(Environment.NewLine, errorList);

                    if (errors.Length > 0)
                    {
                        var t = Task.Run(() => { MessageBox.Show(errors, "Error: aborting action", MessageBoxButtons.OK); });
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
                // Select File being imported from
                xmlHandler.inputFile = XDocument.Load("../../../L5XFiles/TemplateFiles/TemplateProjectV1.L5X");

                // Prompt user to select the element to insert
                string typeOfElement = xmlHandler.GetTypeAndSelect();
                // Get all elements in file of the type
                string attributeSearch = "Name";
                // IO Modules do not require a name. Must search by catalog number instead
                if (typeOfElement.Equals("Module"))
                    attributeSearch = "CatalogNumber";

                // Find and display available elements of that type
                List<string> availableElements = xmlHandler.inputFile.Descendants(typeOfElement).Select(i => i.Attribute(attributeSearch).Value.ToString()).Distinct().ToList();
                DropdownGui selectElement = new DropdownGui(availableElements, "Select template " + typeOfElement + " to use");
                selectElement.ShowDialog(out string nameOfElement);

                IEnumerable<XElement> elementList = xmlHandler.GetElementFromFile(typeOfElement, nameOfElement);

                // Prompt the user for the parent
                string parentType = "Program"; // TODO
                List<XElement> parents = doc.Descendants(parentType).ToList();
                DropdownGui selectBulkParent = new DropdownGui(parents.Select(i => i.Attribute("Name").Value).ToList(), "Select parent for the elements being created");
                selectBulkParent.ShowDialog(out string parentName);

                xmlHandler.ElementInfo.ParentElementBulk = doc.Descendants(parentType).Single(i => i.Attribute("Name").Value.Equals(parentName));

                // Prompt user to select the subelements/values to change(required elements are not selectable)
                foreach (XElement element in elementList)
                {
                    TextInput insertQuantity = new TextInput("How many " + element.Name + " would you like to create?", "1", @"^[1-9]\d*$");
                    insertQuantity.ShowDialog(out string itemQuantity);
                    int quantity = int.Parse(itemQuantity);

                    for (int i = 0; i < quantity; i++)
                    {
                        // Prompt user for name of item
                        TextInput nameSelect = new TextInput("Input a name for created " + element.Name + " #" + (i+1).ToString());
                        nameSelect.ShowDialog(out string itemName);

                        element.SetAttributeValue("Name", itemName);
                        xmlHandler.InsertElement(doc, element);
                        xmlHandler.ElementInfo.RootPath.Clear();
                    }
                    xmlHandler.ElementInfo.ResetElements();
                }

                // Validate the document against the xml Schema, if successful, complete transaction
                List<string> errorList = xmlHandler.GetValidator().ValidateL5XFile(doc);
                string errors = string.Join(Environment.NewLine, errorList);

                if (errors.Length > 0)
                {
                    var t = Task.Run(() => { MessageBox.Show(errors, "Error: aborting action", MessageBoxButtons.OK); });
                }
                else
                {
                    transaction.Complete();
                }
            }
        }
    }
}
