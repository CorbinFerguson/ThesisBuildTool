using System;
using System.Collections.Generic;
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
            doc.Validate(validationSchemaSet, (sender, error) => { invalidElements.Add("Parent: " + (((XElement)sender).Parent.Attribute("Name")?.Value ?? ((XElement)sender).Parent.Name )+ ". " + error.Message); }, true);
            return invalidElements;
        }
        #endregion

    }
}
