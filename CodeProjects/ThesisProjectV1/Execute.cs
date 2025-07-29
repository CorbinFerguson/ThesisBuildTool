using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace ThesisProjectV1
{
    class Execute
    {
        static void Main()
        {
            XMLHandler xmlHandler = new XMLHandler();
            XDocument doc = xmlHandler.loadBasicFile();
            CmdHandler cmdHandler = new CmdHandler();


            string typeOfElement;
            string filePath = "";
            string nameOfElement;
            // Take in user input
            while (!filePath.Equals("Exit"))
            {
                // User input:
                Console.WriteLine("Please input the path of the file to be accessed or Exit to end selection");
                filePath = Console.ReadLine();

                if (!filePath.Equals("Exit"))
                {
                    Console.WriteLine("Select the type of the element being added or Exit to exit the program");
                    typeOfElement = cmdHandler.Navigate();

                    Console.WriteLine("Please input the name of the item");
                    nameOfElement = Console.ReadLine();

                    try
                    {
                        XElement parentElement = new XElement(typeOfElement);
                        XElement returnedElement = xmlHandler.GetElementFromFile(parentElement, filePath);
                        doc = xmlHandler.InsertElement(doc, returnedElement);
                        Console.WriteLine("Inserted: " + returnedElement.Name.ToString());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }

            }

            // Save the document to a file
            string genFilePath = "../../../GenFile.L5X";
            doc.Save(genFilePath);

            Console.WriteLine($"XML file created at: {genFilePath}");
        }
    }
}
