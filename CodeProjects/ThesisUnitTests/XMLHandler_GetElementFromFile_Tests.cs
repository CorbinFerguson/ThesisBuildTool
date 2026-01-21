
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace ThesisProjectV1.Tests
{
    [TestClass]
    public class XMLHandler_GetElementFromFile_Tests
    {
        private PrivateObject CreateHandler()
        {
            // Get the XMLHandler type from the production assembly
            var handlerType = Type.GetType("ThesisProjectV1.XMLHandler, ThesisProjectV1", throwOnError: true);
            var instance = Activator.CreateInstance(handlerType, nonPublic: true);
            return new PrivateObject(instance);
        }

        private void SetInputFile(PrivateObject handler, XDocument xdoc)
        {
            handler.SetField("inputFile", xdoc);
        }

        /// <summary>
        /// Retrieves a single element by Name for a non-Module type.
        /// WHY: Validates the primary, happy-path lookup logic for named elements (e.g., Program).
        /// </summary>
        [TestMethod]
        [TestCategory("XMLHandler.GetElementFromFile")]
        public void GetElementFromFile_SingleProgramByName_ReturnsOne()
        {
            var handler = CreateHandler();

            var xdoc = new XDocument(
                new XElement("Root",
                    new XElement("Program", new XAttribute("Name", "MainProg")),
                    new XElement("Program", new XAttribute("Name", "Aux"))
                )
            );
            SetInputFile(handler, xdoc);

            var result = (List<XElement>)handler.Invoke("GetElementFromFile", new object[] { "Program", "MainProg" });

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("MainProg", result[0].Attribute("Name")?.Value);
        }

        /// <summary>
        /// For Module, the lookup uses CatalogNumber instead of Name when Name may be absent.
        /// WHY: Ensures the special-case attribute selection is correct for IO Modules.
        /// </summary>
        [TestMethod]
        [TestCategory("XMLHandler.GetElementFromFile")]
        public void GetElementFromFile_Module_ByCatalogNumber_ReturnsExpectedModule()
        {
            var handler = CreateHandler();

            var xdoc = new XDocument(
                new XElement("Root",
                    new XElement("Module", new XAttribute("CatalogNumber", "1734-IB8")),
                    new XElement("Module", new XAttribute("CatalogNumber", "1734-OB8"))
                )
            );
            SetInputFile(handler, xdoc);

            var result = (List<XElement>)handler.Invoke("GetElementFromFile", new object[] { "Module", "1734-OB8" });

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("1734-OB8", result[0].Attribute("CatalogNumber")?.Value);
        }

        /// <summary>
        /// Aggregates multiple named elements via the list overload.
        /// WHY: Verifies the convenience overload returns a flat list containing requested elements.
        /// </summary>
        [TestMethod]
        [TestCategory("XMLHandler.GetElementFromFile")]
        public void GetElementFromFile_MultipleNames_ReturnsAllRequested()
        {
            var handler = CreateHandler();

            var xdoc = new XDocument(
                new XElement("Root",
                    new XElement("Program", new XAttribute("Name", "P1")),
                    new XElement("Program", new XAttribute("Name", "P2")),
                    new XElement("Program", new XAttribute("Name", "P3"))
                )
            );
            SetInputFile(handler, xdoc);

            var request = new List<string> { "P1", "P3" };
            var result = (List<XElement>)handler.Invoke("GetElementFromFile", new object[] { "Program", request });

            var names = result.Select(e => e.Attribute("Name")?.Value).ToList();
            CollectionAssert.AreEquivalent(new List<string> { "P1", "P3" }, names);
        }

        /// <summary>
        /// Verifies XMLHandler initializes ElementInfo by default.
        /// WHY: Downstream logic in insertion depends on ElementInfo existing.
        /// </summary>
        [TestMethod]
        [TestCategory("XMLHandler")]
        public void ElementInfo_IsInitialized_ByDefault()
        {
            var handler = CreateHandler();
            var elementInfo = handler.GetField("ElementInfo");

            Assert.IsNotNull(elementInfo);
            Assert.AreEqual("ThesisProjectV1.ElementHelper", elementInfo.GetType().FullName);
        }
    }
}
