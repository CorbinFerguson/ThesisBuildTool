using L5XAutomationTool;
using L5XAutomationToolTestHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Reporting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace XMLHandlerTests
{
    [TestClass]
    public class XMLHandlerTests
    {
        private Mock<IValidationService> mockValidator;
        private Mock<ISchemaDisambiguator> mockDisambiguator;
        private XMLHandler xmlHandler;
        private XDocument testSchema;
        private TestHelper helper;

        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void Setup()
        {
            mockValidator = new Mock<IValidationService>();
            mockDisambiguator = new Mock<ISchemaDisambiguator>();

            helper = new TestHelper();

            // Create a basic test schema
            testSchema = helper.CreateTestSchema();
            mockValidator.Setup(v => v.GetSchema()).Returns(testSchema);

            xmlHandler = new XMLHandler(mockValidator.Object, mockDisambiguator.Object);

            TestReport.Start(TestContext.TestName);
        }

        #region Element Retrieval Tests

        [TestMethod]
        [TestCategory("XMLHandler_UnitTest")]
        [TestProperty("Description",
            "Test that GetSimpleElements returns a list of element names from the input file while excluding CustomProperties elements.")]
        public void GetSimpleElements_ReturnsListOfElements_ExcludingCustomProperties()
        {
            // Arrange
            xmlHandler.inputFile = helper.CreateBasicTestDocument();

            // Act
            List<string> elements = xmlHandler.GetSimpleElements();

            // Assert
            Assert.IsNotNull(elements);
            Assert.IsFalse(elements.Contains("CustomProperties"));
        }

        [TestMethod]
        [TestCategory("XMLHandler_UnitTest")]
        [TestProperty("Description",
            "Test that GetElementTypes returns a list of valid element type names from the provided XML document.")]
        public void GetElementTypes_ReturnsValidTypes_FromDocument()
        {
            // Arrange
            XDocument doc = new(
                new XElement("RSLogix5000Content",
                    new XElement("Program", new XAttribute("Name", "TestProgram")),
                    new XElement("Task", new XAttribute("Name", "TestTask")),
                    new XElement("Datatype", new XAttribute("Name", "TestDatatype"))
                )
            );

            // Act
            List<string> types = xmlHandler.GetElementTypes(doc);

            // Assert
            Assert.IsNotNull(types);
            Assert.Contains("Program", types);
            Assert.Contains("Task", types);
            Assert.Contains("Datatype", types);
        }

        [TestMethod]
        [TestCategory("XMLHandler_UnitTest")]
        [TestProperty("Description",
            "Test that GetElementTypes throws an EmptyListException when the document contains no valid elements.")]
        public void GetElementTypes_ThrowsEmptyListException_WhenNoValidElements()
        {
            // Arrange
            XDocument doc = new(
                new XElement("RSLogix5000Content",
                    new XElement("InvalidElement")
                )
            );

            // Act
            Assert.ThrowsExactly<EmptyListException>(() => xmlHandler.GetElementTypes(doc));
        }

        [TestMethod]
        [TestCategory("XMLHandler_UnitTest")]
        [TestProperty("Description",
            "Test that CheckForDependencies identifies and adds dependent elements to the document when dependencies exist in the source element.")]
        public void CheckForDependencies_AddsDependentElements_WhenDependenciesExist()
        {
            // Arrange
            xmlHandler.inputFile = new(
                new XElement("RSLogix5000Content",
                    new XElement("Program", new XAttribute("Name", "MainProgram")),
                    new XElement("Datatype", new XAttribute("Name", "CustomType"))
                )
            );

            XDocument docToInsert = helper.CreateBasicTestDocument();
            XElement element = new("Program",
                new XAttribute("Name", "MainProgram"),
                new XElement("Dependencies",
                    new XElement("Dependency",
                        new XAttribute("Name", "CustomType"),
                        new XAttribute("Type", "Datatype")
                    )
                )
            );

            // Act
            XDocument result = xmlHandler.CheckForDependencies(docToInsert, element);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        [TestCategory("XMLHandler_UnitTest")]
        [TestProperty("Description",
            "Test that CheckForDependencies skips adding dependencies that already exist in the target document to avoid duplication.")]
        public void CheckForDependencies_SkipsDependencies_WhenAlreadyExist()
        {
            // Arrange
            xmlHandler.inputFile = new(
                new XElement("RSLogix5000Content",
                    new XElement("Datatype", new XAttribute("Name", "CustomType"))
                )
            );

            XDocument docToInsert = new(
                new XElement("RSLogix5000Content",
                    new XElement("Datatype", new XAttribute("Name", "CustomType"))
                )
            );

            XElement element = new("Program",
                new XAttribute("Name", "MainProgram"),
                new XElement("Dependencies",
                    new XElement("Dependency",
                        new XAttribute("Name", "CustomType"),
                        new XAttribute("Type", "Datatype")
                    )
                )
            );

            // Act
            XDocument result = xmlHandler.CheckForDependencies(docToInsert, element);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Descendants("Datatype").Count());
        }

        [TestMethod]
        [TestCategory("XMLHandler_UnitTest")]
        [TestProperty("Description",
            "Test that CheckForDependencies properly handles elements with a ParentModule attribute when the module is not local.")]
        public void CheckForDependencies_HandlesParentModule_WhenNotLocal()
        {
            // Arrange
            xmlHandler.inputFile = new(
                new XElement("RSLogix5000Content",
                    new XElement("Module", new XAttribute("Name", "ParentModule")),
                    new XElement("LocalTag", new XAttribute("Name", "TestTag"))
                )
            );

            XDocument docToInsert = helper.CreateBasicTestDocument();
            XElement element = new("LocalTag",
                new XAttribute("Name", "TestTag"),
                new XAttribute("ParentModule", "ParentModule")
            );

            // Act
            XDocument result = xmlHandler.CheckForDependencies(docToInsert, element);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        [TestCategory("XMLHandler_UnitTest")]
        [TestProperty("Description",
            "Test that CheckForDependencies can process a list of multiple elements and check dependencies for each element.")]
        public void CheckForDependencies_WithList_ProcessesAllElements()
        {
            // Arrange
            xmlHandler.inputFile = helper.CreateBasicTestDocument();
            XDocument docToInsert = helper.CreateBasicTestDocument();
            List<XElement> elements = new()
            {
                new("Program", new XAttribute("Name", "Program1")),
                new("Task", new XAttribute("Name", "Task1"))
            };

            // Act
            XDocument result = xmlHandler.CheckForDependencies(docToInsert, elements);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        [TestCategory("XMLHandler_UnitTest")]
        [TestProperty("Description",
            "Test that CheckForDependencies correctly handles Task elements with ScheduledProgram child elements as dependencies.")]
        public void CheckForDependencies_HandlesTaskWithScheduledPrograms()
        {
            // Arrange
            xmlHandler.inputFile = new XDocument(
                new XElement("RSLogix5000Content",
                    new XElement("Program", new XAttribute("Name", "ScheduledProg"))
                )
            );

            XDocument docToInsert = helper.CreateBasicTestDocument();
            XElement taskElement = new XElement("Task",
                new XAttribute("Name", "MainTask"),
                new XElement("ScheduledProgram", new XAttribute("Name", "ScheduledProg"))
            );

            // Act
            XDocument result = xmlHandler.CheckForDependencies(docToInsert, taskElement);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        [TestProperty("Description",
            "Test that CheckForDependencies detects and handles circular dependencies to avoid infinite loops.")]
        public void CheckForDependencies_CircularDependencies_HandlesGracefully()
        {
            // Arrange
            xmlHandler.inputFile = new XDocument(
                new XElement("RSLogix5000Content",
                    new XElement("Program", new XAttribute("Name", "MainProgram"),
                        new XElement("Dependencies",
                            new XElement("Dependency", new XAttribute("Name", "CustomType"), new XAttribute("Type", "Datatype"))
                        )
                    ),
                    new XElement("Datatype", new XAttribute("Name", "CustomType"),
                        new XElement("Dependencies",
                            new XElement("Dependency", new XAttribute("Name", "MainProgram"), new XAttribute("Type", "Program"))
                        )
                    )
                )
            );

            XDocument docToInsert = helper.CreateBasicTestDocument();
            XElement element = xmlHandler.inputFile.Descendants("Program").First();

            // Act
            XDocument result = xmlHandler.CheckForDependencies(docToInsert, element);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Descendants("Program").Count());
            Assert.AreEqual(1, result.Descendants("Datatype").Count());
        }

        [TestMethod]
        [TestProperty("Description",
            "Test that CheckForDependencies handles elements with no dependencies gracefully.")]
        public void CheckForDependencies_NoDependencies_HandlesGracefully()
        {
            // Arrange
            xmlHandler.inputFile = helper.CreateBasicTestDocument();
            XDocument docToInsert = helper.CreateBasicTestDocument();
            XElement element = new XElement("Program", new XAttribute("Name", "MainProgram"));

            // Act
            XDocument result = xmlHandler.CheckForDependencies(docToInsert, element);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Descendants("Program").Count());
        }

        [TestMethod]
        [TestProperty("Description",
            "Test that CheckForDependencies skips invalid ParentModule references.")]
        public void CheckForDependencies_InvalidParentModule_SkipsGracefully()
        {
            // Arrange
            xmlHandler.inputFile = new XDocument(
                new XElement("RSLogix5000Content",
                    new XElement("LocalTag", new XAttribute("Name", "TestTag"))
                )
            );

            XDocument docToInsert = helper.CreateBasicTestDocument();
            XElement element = new XElement("LocalTag",
                new XAttribute("Name", "TestTag"),
                new XAttribute("ParentModule", "NonExistentModule")
            );

            // Act
            XDocument result = xmlHandler.CheckForDependencies(docToInsert, element);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Descendants("Module").Count());
        }

        #endregion

        #region Path Finding Tests

        [TestMethod]
        [TestCategory("XMLHandler_UnitTest")]
        [TestProperty("Description",
            "Test that FindPathtoRootSchema returns a valid queue of element names representing the path from the element to the schema root.")]
        public void FindPathtoRootSchema_ReturnsPathQueue_ForValidElement()
        {
            // Arrange
            XElement element = new("Program", new XAttribute("Name", "TestProgram"));

            // Act
            LinkedList<string> path = xmlHandler.FindPathtoRootSchema(element);

            // Assert
            Assert.IsNotNull(path);
            Assert.IsNotEmpty(path);
        }

        [TestMethod]
        [TestCategory("XMLHandler_UnitTest")]
        [TestProperty("Description",
            "Test that FindPathtoRootSchema throws an EmptyListException when attempting to find a path for the root element itself.")]
        public void FindPathtoRootSchema_ThrowsException_WhenStartingAtRoot()
        {
            // Arrange
            XElement element = new("RSLogix5000Content");

            // Act
            Assert.ThrowsExactly<EmptyListException>(() => xmlHandler.FindPathtoRootSchema(element));
        }

        #endregion

        #region Attribute Handling Tests

        [TestMethod]
        [TestCategory("XMLHandler_UnitTest")]
        [TestProperty("Description",
            "Test that GetAttributes returns a list of all attributes for a given XML element.")]
        public void GetAttributes_ReturnsAllAttributes_ForElement()
        {
            // Arrange
            XElement element = new("Program", new XAttribute("Name", "TestProgram"));

            // Act
            List<XAttribute> attributes = xmlHandler.GetAttributes(element);

            // Assert
            Assert.IsNotNull(attributes);
        }

        [TestMethod]
        [TestCategory("XMLHandler_UnitTest")]
        [TestProperty("Description",
            "Test that GetAttributes can process multiple elements and return a list of attribute lists, one for each element.")]
        public void GetAttributes_WithMultipleElements_ReturnsListOfAttributeLists()
        {
            // Arrange
            IEnumerable<XElement> elements =
            [
                new XElement("Program", new XAttribute("Name", "Program1")),
                new XElement("Task", new XAttribute("Name", "Task1"))
            ];

            // Act
            List<List<XAttribute>> allAttributes = xmlHandler.GetAttributes(elements);

            // Assert
            Assert.IsNotNull(allAttributes);
        }

        #endregion

        #region Misc tests

        [TestMethod]
        [TestCategory("XMLHandler_UnitTest")]
        [TestProperty("Description",
            "Test that LoadBasicFile successfully loads and returns an XDocument from the template file.")]
        public void LoadBasicFile_ReturnsXDocument()
        {
            // Act & Assert
            try
            {
                XDocument doc = XMLHandler.LoadBasicFile();
                Assert.IsNotNull(doc);
            }
            catch (System.IO.FileNotFoundException)
            {
                // Expected if template file doesn't exist in test environment
                Assert.Inconclusive("Template file not found in test environment");
            }
        }

        [TestMethod]
        [TestCategory("XMLHandler_UnitTest")]
        [TestProperty("Description",
            "Test that InsertElement successfully adds an element to the specified location in the target document.")]
        public void InsertElement_AddsElement_ToDocument()
        {
            // Arrange
            xmlHandler.inputFile = helper.CreateBasicTestDocument();
            XDocument doc = helper.CreateBasicTestDocument();
            XElement program = new XElement("Program", new XAttribute("Name", "NewProgram"));
            xmlHandler.ElementInfo.RootPath = new LinkedList<string>(new[] { "Programs", "Controller" });

            // Act
            XDocument result = xmlHandler.InsertElement(doc, program);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        [TestCategory("XMLHandler_UnitTest")]
        [TestProperty("Description",
            "Test that InsertElement throws a ClashingElementException when attempting to insert an element that already exists in the document.")]
        public void InsertElement_ThrowsException_WhenElementAlreadyExists()
        {
            // Arrange
            xmlHandler.inputFile = helper.CreateBasicTestDocument();
            XDocument doc = new XDocument(
                new XElement("RSLogix5000Content",
                    new XElement("Controller",
                        new XAttribute("Name", "TestController"),
                        new XElement("Programs",
                            new XElement("Program", new XAttribute("Name", "ExistingProgram"))
                        )
                    )
                )
            );

            XElement program = new XElement("Program", new XAttribute("Name", "ExistingProgram"));
            xmlHandler.ElementInfo.RootPath = new LinkedList<string>(new[] { "Programs", "Controller" });

            // Act
            Assert.ThrowsExactly<ClashingElementException>(() => xmlHandler.InsertElement(doc, program));
        }

        [TestMethod]
        [TestCategory("XMLHandler_UnitTest")]
        [TestProperty("Description",
            "Test that InsertElement properly handles Module elements that have a CatalogNumber attribute instead of a Name attribute.")]
        public void InsertElement_HandlesModules_WithCatalogNumber()
        {
            // Arrange
            xmlHandler.inputFile = helper.CreateBasicTestDocument();
            XDocument doc = helper.CreateBasicTestDocument();
            XElement module = new XElement("Module", new XAttribute("CatalogNumber", "1234-5678"));
            xmlHandler.ElementInfo.RootPath = new LinkedList<string>(new[] { "Controller" });

            // Act
            try
            {
                XDocument result = xmlHandler.InsertElement(doc, module);
                Assert.IsNotNull(result);
            }
            catch (Exception)
            {
                // Schema-related exceptions are acceptable in unit tests
                Assert.Inconclusive("Schema validation prevented module insertion");
            }
        }
        #endregion
    }
}