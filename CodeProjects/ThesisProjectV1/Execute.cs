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
            // Save the document to a file
            for (int i = 0; File.Exists(outputPath + outputName + ".L5X"); i++)
                outputName = Regex.Replace(outputName, @"\d", string.Empty) + i.ToString();

            TextInput fileName = new TextInput("File name", outputName);
            fileName.ShowDialog(out outputName);

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
            string typeSelected = xmlHandler.GetElementTypes(doc);
            List<string> namesAvailable = doc.Descendants(typeSelected).Select(i => i.Attribute("Name").Value.ToString()).Distinct().ToList();
            MultiSelectDropdown nameSelect = new MultiSelectDropdown(namesAvailable, "Select Names of elements to modify");
            nameSelect.ShowDialog(out List<string> namesSelected);

            foreach (string elementName in namesSelected)
            {
                IEnumerable<XElement> elements = doc.Descendants(typeSelected).Where(i => i.Attribute("Name") != null).Where(i => i.Attribute("Name").Value == elementName);
                if (elements.Count() > 1)
                {
                    // Multiple revisions
                    MultiSelectDropdown revisionSelect = new MultiSelectDropdown(elements.Select(i => i.Attribute("Revision").Value.ToString()).ToList(), "Select Revisions to modify");
                    revisionSelect.ShowDialog(out List<string> revisionsModify);
                    elements = elements.Where(i => revisionsModify.Contains(i.Attribute("Revision").Value));
                }
                foreach(XElement element in elements)
                {
                    xmlHandler.GetSetAttributes(element);
                }
            }
        }

        public static void DeleteElement()
        {
            string typeSelected = xmlHandler.GetElementTypes(doc);
            List<string> namesAvailable = doc.Descendants(typeSelected).Select(i => i.Attribute("Name").Value.ToString()).ToList();
            MultiSelectDropdown nameSelect = new MultiSelectDropdown(namesAvailable, "Select Names of elements to remove");
            nameSelect.ShowDialog(out List<string> namesSelected);

            // For all selected element names, remove the associated element
            foreach (string name in namesSelected)
            {
                List<XElement> removeElements = doc.Descendants(typeSelected).Where(i => i.Attribute("Name").Value.Equals(name)).ToList();
                // Multiple elements by that name. Must have multiple revisions.(assumes it is working with a valid document)
                if (removeElements.Count() > 1)
                {
                    MultiSelectDropdown revisionSelect = new MultiSelectDropdown(removeElements.Select(i => i.Attribute("Revision").Value.ToString()).ToList(), "Select Revisions to remove");
                    revisionSelect.ShowDialog(out List<string> revisionsRemove);
                    removeElements = removeElements.Where(i => revisionsRemove.Contains(i.Attribute("Revision").Value.ToString())).ToList();
                }
                removeElements.Remove();
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
                string typeOfElement = xmlHandler.GetTypeAndSelect();

                // Get all elements in file of the type
                List<string> availableElements = xmlHandler.inputFile.Descendants(typeOfElement).Select(i => i.Attribute("Name").Value.ToString()).ToList();
                MultiSelectDropdown selectElement = new MultiSelectDropdown(availableElements, "Select elements to insert");
                selectElement.ShowDialog(out List<string> nameOfElement);
                List<XElement> returnedElement = xmlHandler.GetElementFromFile(typeOfElement, nameOfElement);
                doc = xmlHandler.InsertElement(doc, returnedElement);
                DialogResult newElementFile = MessageBox.Show("Add another element from file?", "Element Select", MessageBoxButtons.YesNo);
                if (newElementFile == DialogResult.No)
                    break;
            }
        }

        public static void GenerateElement()
        {
            // Prompt user for what template file they would like to pull from
            openFileSearch.InitialDirectory = "../TemplateFiles/";
            try
            {
                // Select File being imported from
                if (openFileSearch.ShowDialog() == DialogResult.OK)
                    xmlHandler.inputFile = XDocument.Load(openFileSearch.FileName);
                else
                    return;

                // Prompt user to select the element to insert
                string typeOfElement = xmlHandler.GetTypeAndSelect();
                // Get all elements in file of the type
                List<string> availableElements = xmlHandler.inputFile.Descendants(typeOfElement).Select(i => i.Attribute("Name").Value.ToString()).ToList();
                MultiSelectDropdown selectElement = new MultiSelectDropdown(availableElements, "Select elements to insert");
                selectElement.ShowDialog(out List<string> nameOfElement);

                IEnumerable<XElement> elementList = xmlHandler.GetElementFromFile(typeOfElement, nameOfElement);

                // Prompt user to select the subelements/values to change(required elements are not selectable)
                foreach (XElement element in elementList)
                {
                    XElement setElement = xmlHandler.GetSetAttributes(element);

                    xmlHandler.InsertElement(doc, setElement);
                }
            }
            catch (EmptyListException ex)
            {
                // Create a popup telling user what happened
                MessageBox.Show(ex.Message, "Exception Creating List", MessageBoxButtons.OK);
            }
        }
    }
}
