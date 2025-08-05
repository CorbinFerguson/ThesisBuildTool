using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using ThesisProjectV1.Forms;

namespace ThesisProjectV1
{
    class Execute
    {
        [STAThread]
        static void Main()
        {
            XMLHandler xmlHandler = new XMLHandler();
            XDocument doc = xmlHandler.LoadBasicFile();
            CmdHandler cmdHandler = new CmdHandler();
            OpenFileDialog openFileSearch = new OpenFileDialog
            {
                InitialDirectory = "../",
                Filter = "L5X Files (*.L5X)|*.L5X|All files (*.*)|*.*",
                FilterIndex = 0,
                RestoreDirectory = true
            };

            string filePath = "";
            bool insertElement = true;
            // Take in user input
            while (true)
            {
                try
                {
                    // User input:
                    Console.WriteLine("Please input the path of the file to be accessed");
                    if (openFileSearch.ShowDialog() == DialogResult.OK)
                    {
                        filePath = openFileSearch.FileName;
                    }
                    else
                    {
                        insertElement = false;
                    }

                    while(insertElement)
                    {
                        // Prompt user to select type of element to insert
                        List<string> elementTypes = xmlHandler.GetDistinctTypes(filePath);
                        
                        // Filter out any types
                        
                        DropdownGui selectType = new DropdownGui(elementTypes, "Select type of the element to insert");
                        selectType.ShowDialog(out string typeOfElement);

                        // Get all elements in file of the type
                        List<string> availableElements = xmlHandler.GetElementsOfType(typeOfElement, filePath);
                        MultiSelectDropdown selectElement = new MultiSelectDropdown(availableElements, "Select elements to insert");
                        selectElement.ShowDialog(out List<string> nameOfElement);

                        try
                        {
                            XElement parentElement = new XElement(typeOfElement);
                            List<XElement> returnedElement = xmlHandler.GetElementFromFile(parentElement, filePath, nameOfElement);
                            doc = xmlHandler.InsertElement(doc, returnedElement);
                            returnedElement.ForEach(element => { Console.WriteLine("Inserted: " + element.Name); });
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            Console.WriteLine("Stack trace: " + ex.StackTrace);
                        }
                        DialogResult newElementFile= MessageBox.Show("Add another element from file?", "Element Select", MessageBoxButtons.YesNo);
                        if (newElementFile == DialogResult.No)
                            break;
                    }
                }
                catch (EmptyListException ex)
                {
                    // Create a popup telling user what happened
                    MessageBox.Show(ex.Message, "Exception Creating List", MessageBoxButtons.OK);
                }

                DialogResult endSelect = MessageBox.Show("End Selection?", "Element Select", MessageBoxButtons.YesNo);
                if (endSelect == DialogResult.Yes)
                    break;
            }

            // Validate function 

            // Save the document to a file
            string genFilePath = "../../../L5XFiles/GenFile.L5X";
            doc.Save(genFilePath);

            Console.WriteLine($"XML file created at: {genFilePath}");
        }
    }
}
