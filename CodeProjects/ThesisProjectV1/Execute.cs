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
            XMLHandler handler = new XMLHandler();
            XDocument doc = handler.loadBasicFile();

            string nameOfElement="";
            string filePath = "";
            // Take in user input

            while (!nameOfElement.Equals("Exit")){
                Console.WriteLine("Input the type of the element being added or Exit to exit the program");
                nameOfElement = Console.ReadLine();

                if (!nameOfElement.Equals("Exit"))
                {
                    Console.WriteLine("Please input the file to be accessed");
                    filePath = Console.ReadLine();
                    XElement parentElement = new XElement(nameOfElement);
                    XElement returnedElement = handler.GetElementFromFile(parentElement, filePath);
                    doc = handler.InsertElement(doc, returnedElement);
                }

            }

            // Save the document to a file
            string genFilePath = "../../../GenFile.L5X";
            doc.Save(genFilePath);

            Console.WriteLine($"XML file created at: {genFilePath}");
        }
    }
}
