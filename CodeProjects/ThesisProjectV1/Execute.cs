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

            // Try to insert an AOI which is easily accessed
            XElement topLevelElement = new XElement("AddOnInstructionDefinitions");
            XElement elementsReturned = handler.GetElementFromFile(topLevelElement, "../../../TestAOI.L5X");
            doc = handler.InsertElement(doc, elementsReturned);

            // Insert an AOI with name
            XElement aoiElement1 = new XElement("AddOnInstructionDefinition");
            XElement aoiFirst = handler.GetElementFromFile(aoiElement1, "../../../TestFileWith2AOI.L5X", "testAOIV1");
            doc = handler.InsertElement(doc, aoiFirst);

            XElement aoiElement2 = new XElement("AddOnInstructionDefinition");
            XElement aoiSec = handler.GetElementFromFile(aoiElement2, "../../../TestFileWith2AOI.L5X", "testAOIV2");
            doc = handler.InsertElement(doc, aoiSec);


            // Try to insert a routine, which is below the programs section
            XElement routines = new XElement("Program");
            XElement routineSubElements = handler.GetElementFromFile(routines, "../../../EmptyProgram.L5X");
            doc = handler.InsertElement(doc, routineSubElements);

            // Try to insert a module, which is formatted uniquely in the file
            XElement modules = new XElement("Module");
            XElement returnedModules = handler.GetElementFromFile(modules, "../../../TestAQ.L5X", "AQ");
            doc = handler.InsertElement(doc, returnedModules);

            // Save the document to a file
            string filePath = "../../../GenFile.L5X";
            doc.Save(filePath);

            Console.WriteLine($"XML file created at: {filePath}");
        }
    }
}
