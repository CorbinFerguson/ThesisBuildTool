using System.Xml.Linq;

namespace L5XAutomationToolTests
{
    internal class TestHelper
    {
        public XNamespace ns;

        public TestHelper()
        {
            ns = XNamespace.Get(@"http://www.w3.org/2001/XMLSchema");
        }

        public XDocument CreateTestSchema()
        {
            return new XDocument(
                new XElement(ns + "schema",
                    new XElement(ns + "element",
                        new XAttribute("name", "RSLogix5000Content"),
                        new XAttribute("type", "RSLogix5000ContentType")),
                    new XElement(ns + "complexType",
                        new XAttribute("name", "RSLogix5000ContentType"),
                        new XElement(ns + "sequence",
                            new XElement(ns + "element",
                                new XAttribute("name", "Controller"),
                                new XAttribute("type", "ControllerType")))),
                    new XElement(ns + "complexType",
                        new XAttribute("name", "ControllerType"),
                        new XElement(ns + "sequence",
                            new XElement(ns + "element",
                                new XAttribute("name", "Programs"),
                                new XAttribute("type", "ProgramsType")),
                            new XElement(ns + "element",
                                new XAttribute("name", "Tasks"),
                                new XAttribute("type", "TasksType"))),
                        new XElement(ns + "attribute",
                            new XAttribute("name", "Name"),
                            new XAttribute("use", "required"))),
                    new XElement(ns + "complexType",
                        new XAttribute("name", "ProgramsType"),
                        new XElement(ns + "sequence",
                            new XElement(ns + "element",
                                new XAttribute("name", "Program"),
                                new XAttribute("type", "ProgramType")))),
                    new XElement(ns + "complexType",
                        new XAttribute("name", "ProgramType"),
                        new XElement(ns + "attribute",
                            new XAttribute("name", "Name"),
                            new XAttribute("use", "required"))),
                    new XElement(ns + "complexType",
                        new XAttribute("name", "TasksType"),
                        new XElement(ns + "sequence",
                            new XElement(ns + "element",
                                new XAttribute("name", "Task"),
                                new XAttribute("type", "TaskType")))),
                    new XElement(ns + "complexType",
                        new XAttribute("name", "TaskType"),
                        new XElement(ns + "attribute",
                            new XAttribute("name", "Name"),
                            new XAttribute("use", "required"))),
                    new XElement(ns + "element",
                        new XAttribute("name", "Datatype"),
                        new XAttribute("type", "DatatypeType")),
                    new XElement(ns + "complexType",
                        new XAttribute("name", "DatatypeType"))
                )
            );
        }

        public XDocument CreateBasicTestDocument()
        {
            return new XDocument(
                new XElement("RSLogix5000Content",
                    new XElement("Controller",
                        new XAttribute("Name", "TestController"),
                        new XElement("Programs",
                            new XElement("Program", new XAttribute("Name", "MainProgram"))
                        ),
                        new XElement("Tasks",
                            new XElement("Task", new XAttribute("Name", "MainTask"))
                        ),
                        new XElement("Modules",
                            new XElement("Module",
                                new XAttribute("Name", "TestModule"),
                                new XAttribute("CatalogNumber", "1234-5678"))
                        )
                    )
                )
            );
        }

        public static void WaitMilliseconds(int ms)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            while (sw.ElapsedMilliseconds < ms)
            {
            }
        }
    }
}
