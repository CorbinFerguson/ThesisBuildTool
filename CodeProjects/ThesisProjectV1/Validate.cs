using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace ThesisProjectV1
{
    internal class Validate
    {

        #region Fields
        private readonly XmlSchemaSet validationSchemaSet;

        private static readonly string schemaPath = "../../../RSLogix5000_V35.xsd";

        private static XDocument schema;

        #endregion

        #region Constructors

        public Validate()
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
            Console.WriteLine("Validating document");

            // Validate using XML schema, misses some things however
            doc.Validate(validationSchemaSet, (sender, error) => { invalidElements.Add("Parent: " + (((XElement)sender).Parent.Attribute("Name")?.Value ?? ((XElement)sender).Parent.Name )+ ". " + error.Message); }, true);
            
            // Validate that task types are correct
            if(doc.Descendants("Task").Any())
            {
                IEnumerable<XElement> continuous = doc.Descendants("Task").Where(i => i.Attribute("Type").Value.Equals("CONTINUOUS"));
                if(continuous.Count() > 1 )
                {
                    invalidElements.Add("Multiple tasks of type 'Continuous'. Please delete one of the following " + string.Join(" ", continuous.Select(i => i.Attribute("Name").Value.ToString()).ToList()));
                }
            }

            return invalidElements;
        }
        #endregion

    }
}
