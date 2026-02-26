using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace L5XAutomationTool
{
    internal class ValidationHandler
    {

        #region Fields
        private readonly XmlSchemaSet validationSchemaSet;

        private static readonly string schemaPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(),"../../../RSLogix5000_V35.xsd"));

        private static XDocument schema;

        #endregion

        #region Constructors

        public ValidationHandler()
        {
            schema = XDocument.Load(schemaPath);
            validationSchemaSet = new XmlSchemaSet();
            validationSchemaSet.Add(null, XmlReader.Create(schemaPath));
        }

        #endregion

        #region Functions

        public XDocument GetSchema() { return schema; }

        public List<string> ValidateL5XFile(XDocument doc)
        {
            List<string> invalidElements = new List<string>();

            // Validate using XML schema, misses some things however
            doc.Validate(validationSchemaSet, (sender, error) => { invalidElements.Add("SCHEMA ERROR: Parent: " + (((XElement)sender).Parent.Attribute("Name")?.Value ?? ((XElement)sender).Parent.Name) + ". " + error.Message); }, true);

            if (doc.Descendants("Task").Any())
            {
                // Validate that only one continuous task exists
                IEnumerable<XElement> continuous = doc.Descendants("Task").Where(i => i.Attribute("Type").Value.Equals("CONTINUOUS"));
                if (continuous.Count() > 1)
                {
                    invalidElements.Add("ERROR: Multiple tasks of type 'Continuous'. Please delete one of the following " + string.Join(" ", continuous.Select(i => i.Attribute("Name").Value.ToString()).ToList()));
                }

                // Validate that each task contains a program
                foreach (XElement task in doc.Descendants("Task"))
                {
                    if (!task.Descendants("ScheduledProgram").Any())
                        invalidElements.Add("WARNING: Task " + task.Attribute("Name").Value + " has no scheduled program");
                }

            }

            if (doc.Descendants("Program").Any())
            {
                foreach (XElement program in doc.Descendants("Program"))
                {
                    if ((!program.Attribute("Type")?.Value.Equals("EquipmentPhase") ?? true) && program.Attribute("MainRoutineName")?.Value == null)
                    {
                        invalidElements.Add("WARNING: Program " + program.Attribute("Name").Value + " has no associated main Routine. Set value of MainRoutineName");
                    }
                }
            }

            return invalidElements;
        }
        #endregion

    }
}
