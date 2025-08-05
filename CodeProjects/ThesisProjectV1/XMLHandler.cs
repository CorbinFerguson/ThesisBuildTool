using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using ThesisProjectV1.Forms;

namespace ThesisProjectV1
{
    public class XMLHandler
    {
        #region Variables
        private readonly string processorType = "1756-L81E";

        private readonly string projectName = "GenProject";

        private static readonly string schemaPath = "../../../../RSLogix5000_V35.xsd";

        private readonly XNamespace ns = XNamespace.Get(@"http://www.w3.org/2001/XMLSchema");

        public static XDocument schema;

        private readonly XmlSchemaSet validationSchemaSet;

        #endregion

        #region Constructors
        public XMLHandler() 
        {
            schema = XDocument.Load(schemaPath);
            validationSchemaSet = new XmlSchemaSet();
        }

        #endregion

        #region functions

        public XDocument CreateBasicDocument()
        {
            XDocument doc = new XDocument();

            // RSLogix5000Content
            XAttribute[] logixContentAttr =
            {
                new XAttribute("SchemaRevision", "1.0"),
                new XAttribute("SoftwareRevision", "32.01"),
                new XAttribute("TargetName",projectName),
                new XAttribute("TargetType","Controller"),
                new XAttribute("ContainsContext",false),
                new XAttribute("ExportDate",DateTime.Now),
                new XAttribute("ExportOptions","NoRawData L5KData DecoratedData ForceProtectedEncoding AllProjDocTrans")
            };
            XElement logixContent = new XElement("RSLogix5000Content", logixContentAttr);
            doc.Add(logixContent);

            #region RSLogix5000Content Subelements

            // Controller
            XAttribute[] controllerAttributes = {
                new XAttribute("Use", "Target"),
                new XAttribute("Name", projectName),
                new XAttribute("ProcessorType", this.processorType),
                new XAttribute("MajorRev", "32"),
                new XAttribute("MinorRev", "11"),
                new XAttribute("ProjectCreationDate", DateTime.Now),
                new XAttribute("LastModifiedDate", DateTime.Now),
                new XAttribute("SFCExecutionControl", "CurrentActive"),
                new XAttribute("SFCRestartPosition", "MostRecent"),
                new XAttribute("SFCLastScan", "DontScan"),
                new XAttribute("ProjectSN", "16#0000_0000"),
                new XAttribute("MatchProjectToController", "false"),
                new XAttribute("CanUseRPIFromProducer", "false"),
                new XAttribute("InhibitAutomaticFirmwareUpdate", "0"),
                new XAttribute("PassThroughConfiguration", "EnabledWithAppend"),
                new XAttribute("DownloadProjectDocumentationAndExtendedProperties", "true"),
                new XAttribute("DownloadProjectCustomProperties", "true"),
                new XAttribute("ReportMinorOverflow", "false")
            };
            XElement controller = new XElement("Controller", controllerAttributes);
            logixContent.Add(controller);

                #region ControllSubElements
                //Redundancy Info
                XAttribute[] redundancyAttributes = {
                    new XAttribute("Enabled", "false"),
                    new XAttribute("KeepTestEditsOnSwitchOver", "false")
                };
                XElement redundancyInfo = new XElement("RedundancyInfo", redundancyAttributes);
                controller.Add(redundancyInfo);

                //Security
                XAttribute[] securityAttributes = {
                    new XAttribute("Code", "0"),
                    new XAttribute("ChangesToDetect", "16#ffff_ffff_ffff_ffff")
                };
                XElement security = new XElement("Security", securityAttributes);
                controller.Add(security);

                //Safety
                XAttribute[] safetyInfoAttributes = {
                };
                XElement safetyInfo = new XElement("SafetyInfo", safetyInfoAttributes);
                controller.Add(safetyInfo);

                //DataTypes
                XAttribute[] dataTypesAttributes = {
                };
                XElement dataTypes = new XElement("DataTypes", dataTypesAttributes);
                controller.Add(dataTypes);

                //Modules
                XAttribute[] modulesAttributes = {
                };
                XElement modules = new XElement("Modules", modulesAttributes);
                controller.Add(modules);

                //AOI
                XAttribute[] addOnInstructionsAttributes = {
                };
                XElement addOnInstructions = new XElement("AddOnInstructionDefinitions", addOnInstructionsAttributes);
                controller.Add(addOnInstructions);

                //Tags
                XAttribute[] tagsAttributes = {
                };
                XElement tags = new XElement("Tags", tagsAttributes);
                controller.Add(tags);

                //Programs
                XAttribute[] programsAttributes = {
                };
                XElement programs = new XElement("Programs", programsAttributes);
                controller.Add(programs);

                //Tasks
                XAttribute[] tasksAttributes = {
                };
                XElement tasks = new XElement("Tasks", tasksAttributes);
                controller.Add(tasks);

                //CST
                XAttribute[] cstAttributes = {
                    new XAttribute("MasterID", "0")
                };
                XElement cst = new XElement("CST", cstAttributes);
                controller.Add(cst);

                //WallClockTime
                XAttribute[] wallClockTimeAttributes = {
                    new XAttribute("LocalTimeAdjustment", "0"),
                    new XAttribute("TimeZone", "0")
                };
                XElement wallClockTime = new XElement("WallClockTime", wallClockTimeAttributes);
                controller.Add(wallClockTime);

                //Trends
                XAttribute[] trendsAttributes = {
                };
                XElement trends = new XElement("Trends", trendsAttributes);
                controller.Add(trends);

                //DataLogs
                XAttribute[] dataLogsAttributes = {
                };
                XElement dataLogs = new XElement("DataLogs", dataLogsAttributes);
                controller.Add(dataLogs);

                //TimeSync
                XAttribute[] timeSynchronizeAttributes = {
                    new XAttribute("Priority1", "128"),
                    new XAttribute("Priority2", "128"),
                    new XAttribute("PTPEnable", "false")
                };
                XElement timeSynchronize = new XElement("TimeSynchronize", timeSynchronizeAttributes);
                controller.Add(timeSynchronize);

                //EthernetPorts
                XElement ethernetPorts = new XElement("EthernetPorts", null);
                controller.Add(ethernetPorts);

            #endregion

            #endregion

            return doc;
        }

        public XDocument LoadBasicFile()
        {
            XDocument doc = XDocument.Load("../../../EmptyGenFile.l5X");
            return doc;
        }

        // Get all distinct types in the document. They must have a name to be addable.
        public List<string> GetDistinctTypes(string path)
        {
            XDocument doc = XDocument.Load(path);
            List<string> types = new List<string>();

            foreach (XElement type in doc.Descendants())
            {
                if(type.Attribute("Name")!=null && type.Name.ToString()!="Controller")
                    if(!types.Contains(type.Name.ToString()))
                        types.Add(type.Name.ToString());
            }

            return types;
        }

        public XElement GetElementFromFile(XElement subElement, string filePath)
        {
            XDocument itemtoAddDoc = XDocument.Load(filePath);
            XElement element = itemtoAddDoc.Descendants(subElement.Name).First();
            return element;
        }

        public XElement GetElementFromFile(XElement elementType, string filePath, string elementName)
        {
            XElement element;
            if (elementName.Equals(""))
                element = GetElementFromFile(elementType, filePath);
            else
            {
                XDocument itemToAddDoc = XDocument.Load(filePath);
                element = itemToAddDoc.Descendants(elementType.Name).Where(i => i.HasAttributes == true).Where(i => i.Attribute("Name") != null).Where(i => i.Attribute("Name").Value == elementName).First();
            }
            return element;
        }

        public List<XElement> GetElementFromFile(XElement elementType, string filePath, List<string> elementNames)
        {
            List<XElement> elementList = new List<XElement>();
            foreach (string elementName in elementNames)
                elementList.Add(GetElementFromFile(elementType, filePath, elementName));
            return elementList;
        }

        public List<string> GetElementsOfType(string elementType, string filePath)
        {
            List<String> names = new List<String>();
            XDocument doc = XDocument.Load(filePath);
            foreach (XElement element in doc.Descendants(elementType))
                names.Add(element.Attribute("Name").Value.ToString());  

            return names;
        }

        public XDocument InsertElement(XDocument inDoc, XElement element)
        {
            Queue<string> rootPath = FindPathtoRootSchema(element);
            XName parentName = rootPath.Dequeue();

            // Check that parent node exists in document

            while(!(inDoc.Descendants(parentName).Any()))
            {
                parentName = rootPath.Dequeue();
                element = new XElement(parentName, element);

                string complexType = schema.Descendants().Where(i => i.Attribute("name") != null).Where(i => i.Attribute("name").Value.ToString().Equals(element.Name.ToString())).First().Attribute("type").Value;
                XElement schemaElement = schema.Descendants(ns + "complexType").Where(i => i.Attribute("name") != null).Where(i => i.Attribute("name").Value.Equals(complexType)).Single();
                IEnumerable<XElement> requiredAttributes = schemaElement.Descendants().Where(i => i.Name.Equals(ns + "attribute")).Where(i => i.Attribute("use") != null).Where(i => i.Attribute("use").Value.Equals("required"));

                foreach (XElement requiredAttribute in requiredAttributes)
                {
                    // Set attributes of element
                    Console.WriteLine("Input user value for " + requiredAttribute.Attribute("name").Value + " of " + parentName);
                    string attributeValue = Console.ReadLine();
                    element.SetAttributeValue(requiredAttribute.Attribute("name").Value, attributeValue);
                }
            }

            XElement childNode = inDoc.Descendants(element.Name).First();

            if (childNode.IsEmpty)
            {
                childNode.ReplaceWith(element);
            }
            else
            {
                XElement parentNode = inDoc.Descendants(parentName).Ancestors().First();
                IEnumerable<XAttribute> elementAttributes = element.Attributes();
                IEnumerable<XAttribute> parentAttributes = parentNode.Attributes();

                // If attributes are the same:
                if (elementAttributes.Equals(parentAttributes))
                {
                    parentNode.Add(element.Elements());
                }
                // if Attributes are different
                else
                {
                    foreach (XAttribute attribute in parentAttributes) 
                    {
                        if (!parentAttributes.Contains(attribute))
                            parentNode.Add(attribute);
                    }
                    parentNode.Add(element);
                }

            }

            // Validate XML structure
            inDoc.Validate(validationSchemaSet, null);

            return inDoc;
        }

        public XDocument InsertElement(XDocument doc, List<XElement> returnedElement)
        {
            foreach(XElement element in returnedElement)
                InsertElement(doc, element);
            return doc;
        }

        // Returns a list of the names for the nodes leading from the root(RSLogix5000) to element
        public Queue<string> FindPathtoRootSchema(XElement element)
        {
            Queue<string> paths = new Queue<string>();
            paths.Enqueue(element.Name.ToString());
            string attributeFilter=null;

            string name = element.Name.ToString();
            XElement schemaElement = null;

            try
            {
                attributeFilter = "name";
                schemaElement = schema.Descendants().Where(i => i.Attribute(attributeFilter) != null).Where(i => i.Attribute(attributeFilter).Value.Equals(name)).Where(i => !i.Name.Equals(ns + "attribute")).Single();

                // Loop until RSLogix5000Content(root of L5X) is found
                while (!schemaElement.FirstAttribute.Value.Equals("RSLogix5000Content"))
                {
                    // Go to parent complex type, find name of that
                    attributeFilter = "name";
                    name = schemaElement.Parent.Parent.Attribute(attributeFilter).Value;

                    // search for something with that type
                    attributeFilter = "type";
                    schemaElement = schema.Descendants().Where(i => i.Attribute(attributeFilter) != null).Where(i => i.Attribute(attributeFilter).Value.Equals(name)).Where(i => i.Name.Equals(ns + "element")).Single();

                    paths.Enqueue(schemaElement.Attribute("name").Value);

                    // if it is a schema it has gone too far
                }

            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("Sequence contains more than one element"))
                {
                    // Create a popup telling user what happened
                    MessageBox.Show("Ambiguous Parent, please select intended parent", ex.Message, MessageBoxButtons.OK);
                    IEnumerable<XElement> ambiguousElements = schema.Descendants().Where(i => i.Attribute(attributeFilter) != null).Where(i => i.Attribute(attributeFilter).Value.Equals(name));
                    List<string> parentNames = new List<string>();
                    foreach (XElement ambiguousElement in ambiguousElements)
                    {
                        if (attributeFilter.Equals("type"))
                            schemaElement = schema.Descendants().Where(i => i.Attribute(attributeFilter) != null).Where(i => i.Attribute(attributeFilter).Value.Equals(ambiguousElement.Parent.Parent.Attribute("name").Value.ToString())).Where(i => i.Name.Equals(ns + "element")).Single();
                        else
                            throw;
                        parentNames.Add(schemaElement.Attribute("name").Value);
                    }
                    DropdownGui selectElement = new DropdownGui(parentNames, "Select intended parent");
                    selectElement.ShowDialog(out string nameOfElement);
                    XElement unambiguousParent = new XElement(nameOfElement);
                    Queue<string> rootPath = FindPathtoRootSchema(unambiguousParent);
                    foreach (string parent in rootPath)
                        paths.Enqueue(parent);
                    return paths;
                }
                else
                { MessageBox.Show("how did you hit this", ex.Message, MessageBoxButtons.OK); }
            }
            return paths;
        }

        // Gets all the simple elements in the XML Schema
        public List<String> GetSimpleElements()
        {
            IEnumerable<XElement> elements = schema.Descendants(ns + "element");

            List<String> elementsInList= new List<String>();

            foreach (XElement element in elements)
            {
                if (!element.Attribute("name").Value.Equals("CustomProperties"))
                    elementsInList.Add(element.Attribute("name").Value);
            }
            return elementsInList;
        }

        #endregion
    }

}
