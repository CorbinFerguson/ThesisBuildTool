using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace ThesisProjectV1
{
    public class XMLHandler
    {
        #region Variables
        public string processorType = "1756-L81E";

        public string projectName = "GenProject";

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

        public XDocument loadBasicFile()
        {
            XDocument doc = XDocument.Load("../../../EmptyGenFile.l5X");
            return doc;
        }

        public XElement GetElementFromFile(XElement subElement, string pathItemtoAdd)
        {
            XDocument itemtoAddDoc = XDocument.Load(pathItemtoAdd);
            XElement element = itemtoAddDoc.Descendants(subElement.Name).First(); // Returning Null
            return element;
        }

        public XElement GetElementFromFile(XElement elementType, string pathItemtoAdd, string elementName)
        {
            XDocument itemToAddDoc = XDocument.Load(pathItemtoAdd);
            XElement element = itemToAddDoc.Descendants(elementType.Name).Where(i => i.HasAttributes == true).Where(i => i.Attribute("Name") != null).Where(i => i.Attribute("Name").Value == elementName).First();
            return element;
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
            }

            XElement childNode = inDoc.Descendants(element.Name).First();
            if (childNode.IsEmpty)
            {
                childNode.ReplaceWith(element);
            }
            else
            {
                XElement parentNode = inDoc.Descendants(parentName).First();
                IEnumerable<XAttribute> xAttributes = element.Attributes();

                foreach (XAttribute xAttribute in xAttributes)
                {
                    childNode.SetAttributeValue(xAttribute.Name, xAttribute.Value);
                }
                childNode.Add(element.Elements());
            }
            return inDoc;
        }

        // Returns a list of the names for the nodes leading from the root(RSLogix5000) to element
        public Queue<string> FindPathtoRootSchema(XElement element)
        {
            Queue<string> paths = new Queue<string>();
            paths.Enqueue(element.Name.ToString());
            XDocument schema = XDocument.Load("../../../../RSLogix5000_V35.xsd");

            string name = element.Name.ToString();

            XElement schemaElement = schema.Descendants().Where(i => i.HasAttributes == true).Where(i => i.Attribute("name") != null).Where(i => i.Attribute("name").Value.Equals(name)).FirstOrDefault();
            // Loop until RSLogix5000Content(root of L5X) is found
            while (!schemaElement.FirstAttribute.Value.Equals("RSLogix5000Content"))
            {
                // Go to parent complex type, find name of that
                name = schemaElement.Parent.Parent.Attribute("name").Value;

                // search for something with that type
                schemaElement = schema.Descendants().Where(i => i.HasAttributes == true).Where(i => i.Attribute("type") != null).Where(i => i.Attribute("type").Value.Equals(name)).FirstOrDefault();

                paths.Enqueue(schemaElement.Attribute("name").Value);

                // if it is a schema it has gone too far
            }
            return paths;
        }

        #endregion
    }

}
