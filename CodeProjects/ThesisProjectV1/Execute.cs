using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ThesisProjectV1
{
    class Execute
    {
        static void Main()
        {
            XMLHandler handler = new XMLHandler();
            XDocument doc = handler.loadBasicFile();

            XElement routines = new XElement("Routines");
            IEnumerable<XElement> routineSubElements = handler.GetSubElements(routines, "../../../EmptyProgram.L5X");

            XElement topLevelElement = new XElement("AddOnInstructionDefinitions");
            IEnumerable<XElement> elementsReturned = handler.GetSubElements(topLevelElement, "../../../TestAOI.L5X");

            XElement modules= new XElement("AddOnInstructionDefinitions", elementsReturned);

            doc = handler.InsertElement(doc, modules);

            // Save the document to a file
            string filePath = "../../../GenFile.L5X";
            doc.Save(filePath);

            Console.WriteLine($"XML file created at: {filePath}");
        }
    }
}
