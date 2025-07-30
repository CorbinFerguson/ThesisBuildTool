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

            

            string typeOfElement;
            string filePath = "";
            string nameOfElement;
            // Take in user input
            while (true)
            {
                // User input:
                Console.WriteLine("Please input the path of the file to be accessed");
                OpenFileDialog openFileSearch = new OpenFileDialog();
                openFileSearch.InitialDirectory = "../";
                openFileSearch.Filter = "L5X Files (*.L5X)|*.L5X|All files (*.*)|*.*";
                openFileSearch.FilterIndex = 2;
                openFileSearch.RestoreDirectory = true;
                if (openFileSearch.ShowDialog() == DialogResult.OK)
                {
                    filePath = openFileSearch.FileName;
                }
                else
                {
                    filePath = "Exit";
                }

                if (!filePath.Equals("Exit"))
                {
                    Console.WriteLine("Select the type of the element being added or Exit to exit the program");
                    typeOfElement = cmdHandler.Navigate();

                    // Get all elements in file of the type
                    List<string> availableElements = xmlHandler.GetElementsOfType(typeOfElement, filePath);
                    DropdownGui selectElement = new DropdownGui(availableElements);
                    selectElement.ShowDialog(out nameOfElement);

                    try
                    {
                        XElement parentElement = new XElement(typeOfElement);
                        XElement returnedElement = xmlHandler.GetElementFromFile(parentElement, filePath, nameOfElement);
                        doc = xmlHandler.InsertElement(doc, returnedElement);
                        Console.WriteLine("Inserted: " + returnedElement.Name.ToString());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine("Stack trace: " + ex.StackTrace);
                    }
                }

                EndSelectionForm endSelect = new EndSelectionForm();
                if (endSelect.ShowDialog() == DialogResult.Yes)
                    break;
            }

            // Validate function 

            // Save the document to a file
            string genFilePath = "../../../GenFile.L5X";
            doc.Save(genFilePath);

            Console.WriteLine($"XML file created at: {genFilePath}");
        }
    }
}
