using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using ThesisProjectV1.Forms;

namespace ThesisProjectV1
{
    public class Execute
    {
        #region Fields
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
        #endregion

        [STAThread]
        #region Functions
        private static void Main()
        {
            // Default to generated basic file
            doc = xmlHandler.LoadBasicFile();

            // Take in user input
            ActionSelect actionSelect = new ActionSelect();
            actionSelect.ShowDialog();
        }

        public static void ValidateFile(bool showNoError=true)
        {
            // Validate the document against the xml Schema, if successful, complete transaction
            List<string> errorList = xmlHandler.GetValidator().ValidateL5XFile(doc);
            string errors = string.Join(Environment.NewLine, errorList);

            if (errors.Length > 0)
            {
                MessageBox.Show(errors, "Detected Errors ", MessageBoxButtons.OK);
            }
            else if(showNoError)
            {
                MessageBox.Show("No errors detected!", "Detected Errors");
            }
        }

        public static void NewFile()
        {
            DialogResult dialogResult = MessageBox.Show("Are you sure you want to overwrite your existing file?", "Verify File Creation", MessageBoxButtons.YesNo);
            if (dialogResult.Equals(DialogResult.Yes))
            {
                doc = xmlHandler.LoadBasicFile();
                MessageBox.Show("New Empty File Created.");
            }
        }

        public static void SaveFile()
        {
            // Save the document to a file
            for (int i = 0; File.Exists(outputPath + outputName + ".L5X"); i++)
                outputName = Regex.Replace(outputName, @"\d", string.Empty) + i.ToString();

            SaveFileDialog save = new SaveFileDialog();
            save.Filter = " L5X Files(*.L5X)|*.L5X|XML Files(*.XML)|*.xml|All Files(*.*)|*.*";
            save.FileName = outputName;
            save.DefaultExt = "L5X";
            if (save.ShowDialog() == DialogResult.OK)
            {
                doc.Save(save.FileName);
            }
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
            List<string> namesAvailable = doc.Descendants(typeSelected).Select(i => i.Attribute("Name")?.Value.ToString() ?? i.Attribute("CatalogNumber").Value.ToString()).Distinct().ToList();
            MultiSelectDropdown nameSelect = new MultiSelectDropdown(namesAvailable, "Select Names of elements to modify");
            nameSelect.ShowDialog(out List<string> namesSelected);

            xmlHandler.inputFile = doc;

            IEnumerable<XElement> elements = xmlHandler.GetElementFromFile(typeSelected, namesSelected);
            foreach (XElement element in elements)
            {
                xmlHandler.GetSetAttributes(element);
            }

            ValidateFile(false);
        }

        public static void DeleteElement()
        {
            string typeSelected = xmlHandler.GetElementTypes(doc);
            List<string> namesAvailable = doc.Descendants(typeSelected).Select(i => i.Attribute("Name")?.Value.ToString() ?? i.Attribute("CatalogNumber").Value).ToList();
            MultiSelectDropdown nameSelect = new MultiSelectDropdown(namesAvailable, "Select Names of elements to remove");
            nameSelect.ShowDialog(out List<string> namesSelected);

            xmlHandler.inputFile = doc;

            IEnumerable<XElement> removeElement = xmlHandler.GetElementFromFile(typeSelected, namesSelected);
            removeElement.Remove();

            ValidateFile(false);
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
                string attributeFilter = "Name";

                if (typeOfElement.Equals("Module"))
                    attributeFilter = "CatalogNumber";

                // Find and display available elements of that type
                List<string> availableElements = xmlHandler.inputFile.Descendants(typeOfElement).Select(i => i.Attribute(attributeFilter)?.Value.ToString()).Distinct().ToList();
                MultiSelectDropdown selectElement = new MultiSelectDropdown(availableElements, "Select elements to insert");
                selectElement.ShowDialog(out List<string> nameOfElement);

                // Get the selected element, then insert it
                List<XElement> returnedElement = xmlHandler.GetElementFromFile(typeOfElement, nameOfElement);
                doc = xmlHandler.InsertElement(doc, returnedElement);

                // Reset the helper
                xmlHandler.ElementInfo.ResetElements();

                ValidateFile(false);

                DialogResult newElementFile = MessageBox.Show("Add another element from file?", "Element Select", MessageBoxButtons.YesNo);
                if (newElementFile == DialogResult.No)
                    break;
            }
        }

        public static void GenerateElement()
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

            // Get the template element
            XElement element = xmlHandler.GetElementFromFile(typeOfElement, nameOfElement).Single();

            // Get the names of all valid parents for the element
            IEnumerable<XElement> ambiguousElements = xmlHandler.GetValidator().GetSchema().Descendants(xmlHandler.Ns + "element").Where(i => i.Attribute("name")?.Value.Equals(typeOfElement + "s") ?? false);
            List<string> parentSchemaType = xmlHandler.GetValidator().GetSchema().Descendants().Where(i => ambiguousElements.Select(x => x.Parent.Parent.Attribute("name")?.Value.ToString()).ToList()?.Contains(i.Attribute("name")?.Value) ?? false).Select(i => i.Attribute("name").Value).ToList();
            List<string> parentType = xmlHandler.GetValidator().GetSchema().Descendants(xmlHandler.Ns + "element").Where(i => parentSchemaType.Contains(i.Attribute("type")?.Value)).Select(i => i.Attribute("name").Value).ToList();

            if (parentType.Count() > 1)
            {
                IEnumerable<XElement> parentElems = doc.Descendants().Where(i => parentType.Contains(i.Name?.ToString()));

                // Get all valid parents of available types
                List<string> parents = parentElems.Select(i => i.Attribute("Name").Value.ToString()).ToList();

                // Prompt the user for the parent
                DropdownGui selectBulkParent = new DropdownGui(parents, "Select parent for the elements being created");
                selectBulkParent.ShowDialog(out string parentName);

                xmlHandler.ElementInfo.ParentElementBulk = parentElems.Single(i => i.Attribute("Name")?.Value.Equals(parentName) ?? false);
            }
            else if (parentType.Count() != 1)
            {
                throw new EmptyListException("Attempted to Create an item with no valid parents");
            }

            // Prompt user to select the subelements/values to change(required elements are not selectable)
            TextInput insertQuantity = new TextInput("How many " + element.Name + " would you like to create?", "1", @"^[1-9]\d*$");
            insertQuantity.ShowDialog(out string itemQuantity);
            int quantity = int.Parse(itemQuantity);

            List<string> bulkNames = new List<string>();
            for (int i = 0; i < quantity; i++)
            {
                if (typeOfElement != "Module" || element.Attribute("Use") == null)
                {
                    // Prompt user for name of item and assign it
                    TextInput nameSelect = new TextInput("Input a name for created " + element.Name + " #" + (i + 1).ToString(), "", @"^[a-zA-Z]+(\w*[A-Za-z0-9])*$");
                    nameSelect.ShowDialog(out string itemName);
                    element.SetAttributeValue("Name", itemName);
                    bulkNames.Add(itemName);
                }

                // Insert the element and reset the path for the next one
                xmlHandler.InsertElement(doc, element);
                xmlHandler.ElementInfo.RootPath.Clear();
            }
            xmlHandler.ElementInfo.ResetElements();

            // Programs have special case for having a parent task or being unassigned
            if (typeOfElement.Equals("Program"))
            {
                // Get a list of all tasks in the document currently
                IEnumerable<XElement> tasks = doc.Descendants("Task");

                if (tasks.Any())
                {
                    // Get list of names
                    List<string> taskNames = tasks.Select(i => i.Attribute("Name").Value).ToList();
                    taskNames.Add("Keep programs unscheduled");

                    // Prompt user for which task to assign the programs to
                    DropdownGui parentTask = new DropdownGui(taskNames, "Select the parent task for inserted programs");
                    parentTask.ShowDialog(out string taskName);

                    if (taskName != "Keep programs unscheduled")
                    {
                        // Get parent task
                        XElement taskElement = tasks.Single(i => i.Attribute("Name").Value.Equals(taskName));
                        XElement scheduledParent = null;
                        if (taskElement.Elements("ScheduledPrograms").Any())
                        {
                            scheduledParent = taskElement.Elements("ScheduledPrograms").Single();
                        }
                        else
                        {
                            scheduledParent = new XElement("ScheduledPrograms");
                        }

                        // Add the program names to the chosen tasks scheduled programs
                        foreach (string programName in bulkNames)
                        {
                            XElement scheduledProgram = new XElement("ScheduledProgram", new XAttribute("Name", programName));
                            scheduledParent.Add(scheduledProgram);
                        }

                        if (!taskElement.Elements("ScheduledPrograms").Any())
                        {
                            taskElement.Add(scheduledParent);
                        }
                    }
                }
            }

            ValidateFile(false);
        }
        
        #endregion
    }
}
