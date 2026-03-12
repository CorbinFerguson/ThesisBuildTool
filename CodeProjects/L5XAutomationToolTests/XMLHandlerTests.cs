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
        #region Fields

        private Mock<IValidationService> mockValidator;
        private Mock<ISchemaDisambiguator> mockDisambiguator;
        private XMLHandler xmlHandler;
        private XDocument testSchema;
        private TestHelper helper;

        public TestContext TestContext { get; set; }

        #endregion

        #region Setup and Teardown

        [TestInitialize]
        public void Setup()
        {
            // Initialize mocks for dependencies
            mockValidator = new Mock<IValidationService>();
            mockDisambiguator = new Mock<ISchemaDisambiguator>();

            helper = new TestHelper();

            // Create a basic test schema
            testSchema = TestHelper.CreateTestSchema();
            mockValidator.Setup(v => v.GetSchema()).Returns(testSchema);

            // Initialize the XMLHandler instance
            xmlHandler = new XMLHandler(mockValidator.Object, mockDisambiguator.Object);

            // Start test reporting for the current test
            TestReport.Start(TestContext.TestName);
        }

        #endregion

        #region Element Retrieval Tests

        [TestMethod]
        [TestCategory("XMLHandler_UnitTest")]
        [TestProperty("Description",
            "Test that GetSimpleElements returns a list of element names from the input file while excluding CustomProperties elements.")]
        public void GetSimpleElements_ReturnsListOfElements_ExcludingCustomProperties()
        {
            // Arrange
            xmlHandler.inputFile = TestHelper.CreateBasicTestDocument();

            // Act
            List<string> elements = xmlHandler.GetSimpleElements();

            // Assert
            Assert.IsNotNull(elements);
            Assert.DoesNotContain("CustomProperties", elements);
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
            Assert.HasCount(3, types, "The list should contain exactly 3 types.");
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
                    new XElement("AddOnInstructionDefinition", new XAttribute("Name", "MainAOI")),
                    new XElement("Datatype", new XAttribute("Name", "CustomType"))
                )
            );

            XDocument docToInsert = TestHelper.CreateBasicTestDocument();
            XElement element = new("AddOnInstructionDefinition",
                new XAttribute("Name", "MainAOI"),
                new XElement("Dependencies",
                    new XElement("Dependency",
                        new XAttribute("Type", "Datatype"),
                        new XAttribute("Name", "CustomType")
                    )
                )
            );

            // Act
            XDocument result = xmlHandler.CheckForDependencies(docToInsert, element);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Descendants("Datatype").Count(), "The dependent Datatype element was not added to the document.");
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

            XDocument docToInsert = TestHelper.CreateBasicTestDocument();
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
            xmlHandler.inputFile = TestHelper.CreateBasicTestDocument();
            XDocument docToInsert = new(
                new XElement("RSLogix5000Content",
                    new XElement("Datatype", new XAttribute("Name", "Dependency1")),
                    new XElement("Datatype", new XAttribute("Name", "Dependency2")),
                    new XElement("Program", new XAttribute("Name", "Program1"),
                        new XElement("Dependencies",
                            new XElement("Dependency", new XAttribute("Type", "Datatype"), new XAttribute("Name", "Dependency1")))),
                    new XElement("Task", new XAttribute("Name", "Task1"),
                        new XElement("Dependencies",
                            new XElement("Dependency", new XAttribute("Type", "Datatype"), new XAttribute("Name", "Dependency2")))))
            );

            List<XElement> elements =
            [
                new XElement("Program", new XAttribute("Name", "Program1"),
                    new XElement("Dependencies",
                        new XElement("Dependency", new XAttribute("Type", "Datatype"), new XAttribute("Name", "Dependency1"))
                    )
                ),
                new XElement("Task", new XAttribute("Name", "Task1"),
                    new XElement("Dependencies",
                        new XElement("Dependency", new XAttribute("Type", "Datatype"), new XAttribute("Name", "Dependency2"))
                    )
                )
            ];

            // Act
            XDocument result = xmlHandler.CheckForDependencies(docToInsert, elements);

            // Assert
            Assert.IsNotNull(result);
            Assert.ContainsSingle(p => p.Attribute("Name")?.Value == "Program1",
                result.Descendants("Program"), "The Program element with the correct name was not added to the document.");
            Assert.ContainsSingle(t => t.Attribute("Name")?.Value == "Task1",
                result.Descendants("Task"), "The Task element with the correct name was not added to the document.");
            Assert.ContainsSingle(d => d.Attribute("Name")?.Value == "Dependency1",
                result.Descendants("Datatype"), "The Dependency1 element was not added to the document.");
            Assert.ContainsSingle(d => d.Attribute("Name")?.Value == "Dependency2",
                result.Descendants("Datatype"), "The Dependency2 element was not added to the document.");
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

            XDocument docToInsert = TestHelper.CreateBasicTestDocument();
            XElement taskElement = new("Task",
                new XAttribute("Name", "MainTask"),
                new XElement("ScheduledProgram", new XAttribute("Name", "ScheduledProg"))
            );

            // Act
            XDocument result = xmlHandler.CheckForDependencies(docToInsert, taskElement);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Descendants("ScheduledProgram")
                                      .Count(sp => sp.Attribute("Name")?.Value == "ScheduledProg"),
                            "The ScheduledProgram element with the correct reference was not added to the document.");
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

            XDocument docToInsert = TestHelper.CreateBasicTestDocument();
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
            xmlHandler.inputFile = TestHelper.CreateBasicTestDocument();
            XDocument docToInsert = TestHelper.CreateBasicTestDocument();
            XElement element = new("Program", new XAttribute("Name", "MainProgram"));

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

            XDocument docToInsert = TestHelper.CreateBasicTestDocument();
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
            Assert.HasCount(2, allAttributes, "The number of attribute lists returned is incorrect.");

            Assert.HasCount(38, allAttributes[0], "The number of attributes for the first element is incorrect.");

            Assert.HasCount(17, allAttributes[1], "The number of attributes for the second element is incorrect.");
            Assert.AreEqual("Name", allAttributes[1][0].Name.LocalName, "The attribute name for the second element is incorrect.");
        }

        [TestMethod]
        [TestCategory("XMLHandler_UnitTest")]
        [TestProperty("Description",
            "Test that GetAttributes returns the name of each attribute defined in the schema for a Program element.")]
        public void GetAttributes_ReturnsAllAttributes_ForElement()
        {
            // Arrange
            XElement programElement = new("Program", new XAttribute("Name", "TestProgram"));

            // Act
            List<XAttribute> attributes = xmlHandler.GetAllAttributes(programElement);

            // Assert
            Assert.IsNotNull(attributes);
            Assert.HasCount(38, attributes, "The number of attributes returned is incorrect.");
            Assert.Contains(attr => attr.Name == "Name", attributes, "The 'Name' attribute was not found.");
            Assert.Contains(attr => attr.Name == "UId", attributes, "The 'UId' attribute was not found.");
            Assert.Contains(attr => attr.Name == "ParentUId", attributes, "The 'ParentUId' attribute was not found.");
            Assert.Contains(attr => attr.Name == "Type", attributes, "The 'Type' attribute was not found.");
            Assert.Contains(attr => attr.Name == "TestEdits", attributes, "The 'TestEdits' attribute was not found.");
            Assert.Contains(attr => attr.Name == "MainRoutineName", attributes, "The 'MainRoutineName' attribute was not found.");
            Assert.Contains(attr => attr.Name == "PreStateRoutineName", attributes, "The 'PreStateRoutineName' attribute was not found.");
            Assert.Contains(attr => attr.Name == "FaultRoutineName", attributes, "The 'FaultRoutineName' attribute was not found.");
            Assert.Contains(attr => attr.Name == "ExecutingTaskName", attributes, "The 'ExecutingTaskName' attribute was not found.");
            Assert.Contains(attr => attr.Name == "Verified", attributes, "The 'Verified' attribute was not found.");
            Assert.Contains(attr => attr.Name == "EditsExist", attributes, "The 'EditsExist' attribute was not found.");
            Assert.Contains(attr => attr.Name == "Disabled", attributes, "The 'Disabled' attribute was not found.");
            Assert.Contains(attr => attr.Name == "InitialStepIndex", attributes, "The 'InitialStepIndex' attribute was not found.");
            Assert.Contains(attr => attr.Name == "InitialState", attributes, "The 'InitialState' attribute was not found.");
            Assert.Contains(attr => attr.Name == "CompleteStateIfNotImpl", attributes, "The 'CompleteStateIfNotImpl' attribute was not found.");
            Assert.Contains(attr => attr.Name == "LossOfCommCmd", attributes, "The 'LossOfCommCmd' attribute was not found.");
            Assert.Contains(attr => attr.Name == "ExternalRequestAction", attributes, "The 'ExternalRequestAction' attribute was not found.");
            Assert.Contains(attr => attr.Name == "EquipmentId", attributes, "The 'EquipmentId' attribute was not found.");
            Assert.Contains(attr => attr.Name == "RecipePhaseNames", attributes, "The 'RecipePhaseNames' attribute was not found.");
            Assert.Contains(attr => attr.Name == "LastScanTime", attributes, "The 'LastScanTime' attribute was not found.");
            Assert.Contains(attr => attr.Name == "MaxScanTime", attributes, "The 'MaxScanTime' attribute was not found.");
            Assert.Contains(attr => attr.Name == "TagsUId", attributes, "The 'TagsUId' attribute was not found.");
            Assert.Contains(attr => attr.Name == "RoutinesUId", attributes, "The 'RoutinesUId' attribute was not found.");
            Assert.Contains(attr => attr.Name == "Class", attributes, "The 'Class' attribute was not found.");
            Assert.Contains(attr => attr.Name == "SynchronizeRedundancyDataAfterExecution", attributes, "The 'SynchronizeRedundancyDataAfterExecution' attribute was not found.");
            Assert.Contains(attr => attr.Name == "UseAsFolder", attributes, "The 'UseAsFolder' attribute was not found.");
            Assert.Contains(attr => attr.Name == "AutoValueAssignStepToPhase", attributes, "The 'AutoValueAssignStepToPhase' attribute was not found.");
            Assert.Contains(attr => attr.Name == "AutoValueAssignPhaseToStepOnComplete", attributes, "The 'AutoValueAssignPhaseToStepOnComplete' attribute was not found.");
            Assert.Contains(attr => attr.Name == "AutoValueAssignPhaseToStepOnStopped", attributes, "The 'AutoValueAssignPhaseToStepOnStopped' attribute was not found.");
            Assert.Contains(attr => attr.Name == "AutoValueAssignPhaseToStepOnAborted", attributes, "The 'AutoValueAssignPhaseToStepOnAborted' attribute was not found.");
            Assert.Contains(attr => attr.Name == "Revision", attributes, "The 'Revision' attribute was not found.");
            Assert.Contains(attr => attr.Name == "RevisionExtension", attributes, "The 'RevisionExtension' attribute was not found.");
            Assert.Contains(attr => attr.Name == "UnitID", attributes, "The 'UnitID' attribute was not found.");
            Assert.Contains(attr => attr.Name == "RetainSequenceIDOnReset", attributes, "The 'RetainSequenceIDOnReset' attribute was not found.");
            Assert.Contains(attr => attr.Name == "GenerateSequenceEvents", attributes, "The 'GenerateSequenceEvents' attribute was not found.");
            Assert.Contains(attr => attr.Name == "ValuesToUseOnStart", attributes, "The 'ValuesToUseOnStart' attribute was not found.");
            Assert.Contains(attr => attr.Name == "ValuesToUseOnReset", attributes, "The 'ValuesToUseOnReset' attribute was not found.");
            Assert.Contains(attr => attr.Name == "Use", attributes, "The 'Use' attribute was not found.");
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
            XDocument doc = XMLHandler.LoadBasicFile();
            Assert.IsNotNull(doc);
        }

        [TestMethod]
        [TestCategory("XMLHandler_UnitTest")]
        [TestProperty("Description",
            "Test that InsertElement successfully adds an element to the specified location in the target document.")]
        public void InsertElement_AddsElement_ToDocument()
        {
            // Arrange
            xmlHandler.inputFile = TestHelper.CreateBasicTestDocument();
            XDocument doc = TestHelper.CreateBasicTestDocument();
            XElement program = new("Program", new XAttribute("Name", "NewProgram"));
            xmlHandler.ElementInfo.RootPath = new LinkedList<string>(["Programs", "Controller"]);

            // Act
            XDocument result = xmlHandler.InsertElement(doc, program);

            // Assert
            Assert.IsNotNull(result, "The resulting document should not be null.");
            Assert.ContainsSingle(p => p.Attribute("Name")?.Value == "NewProgram",
                result.Descendants("Program"), "The 'Program' element with the name 'NewProgram' was not added to the document.");
        }

        [TestMethod]
        [TestCategory("XMLHandler_UnitTest")]
        [TestProperty("Description",
            "Test that InsertElement throws a ClashingElementException when attempting to insert an element that already exists in the document.")]
        public void InsertElement_ThrowsException_WhenElementAlreadyExists()
        {
            // Arrange
            xmlHandler.inputFile = TestHelper.CreateBasicTestDocument();
            XDocument doc = new(
                new XElement("RSLogix5000Content",
                    new XElement("Controller",
                        new XAttribute("Name", "TestController"),
                        new XElement("Programs",
                            new XElement("Program", new XAttribute("Name", "ExistingProgram"))
                        )
                    )
                )
            );

            XElement program = new("Program", new XAttribute("Name", "ExistingProgram"));
            xmlHandler.ElementInfo.RootPath = new LinkedList<string>(["Programs", "Controller"]);

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
            xmlHandler.inputFile = TestHelper.CreateBasicTestDocument();
            XDocument doc = TestHelper.CreateBasicTestDocument();
            XElement module = new("Module", new XAttribute("CatalogNumber", "1234-5678"));

            // Act
            XDocument result = xmlHandler.InsertElement(doc, module);
            Assert.IsNotNull(result);
            Assert.Contains(p => p.Attribute("CatalogNumber")?.Value == "1234-5678",
                result.Descendants("Module"), "The 'Module' element with the CatalogNumber '1234-5678' was not added to the document.");
        }
        #endregion
    }
}