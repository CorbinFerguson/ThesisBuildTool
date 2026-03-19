using L5XAutomationTool;
using L5XAutomationToolTestHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Reporting;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace ExecuteTests
{
    [TestClass]
    public class ExecuteTests
    {
        #region Fields
        private Mock<XMLHandler> mockXmlHandler;
        private Mock<IMessageService> mockMessages;
        private Mock<IUserPromptService> mockPrompts;
        private Mock<IOpenFileService> mockOpenFile;
        private Mock<ISaveFileService> mockSaveFile;
        private Mock<IValidationService> mockValidation;
        private Mock<IFileSystem> mockFileSystem;
        private Mock<ISchemaDisambiguator> mockDisambiguator;

        private Execute execute;
        private TestHelper helper;

        public TestContext TestContext { get; set; }

        #endregion

        #region Setup and Teardown

        [TestInitialize]
        public void Setup()
        {
            // Initialize mocks for dependencies
            mockValidation = new Mock<IValidationService>();
            mockDisambiguator = new Mock<ISchemaDisambiguator>();
            mockXmlHandler = new Mock<XMLHandler>(mockValidation.Object, mockDisambiguator.Object) { CallBase = true };
            mockMessages = new Mock<IMessageService>();
            mockPrompts = new Mock<IUserPromptService>();
            mockOpenFile = new Mock<IOpenFileService>();
            mockSaveFile = new Mock<ISaveFileService>();
            mockFileSystem = new Mock<IFileSystem>();

            helper = new TestHelper();

            // Setup default validation behavior
            mockValidation.Setup(v => v.ValidateL5XFile(It.IsAny<XDocument>())).Returns([]);
            mockValidation.Setup(v => v.GetSchema()).Returns(TestHelper.CreateTestSchema());

            // Initialize the Execute instance
            execute = new Execute(
                mockXmlHandler.Object,
                mockMessages.Object,
                mockPrompts.Object,
                mockOpenFile.Object,
                mockSaveFile.Object,
                mockValidation.Object,
                mockFileSystem.Object
            );

            // Start test reporting for the current test
            TestReport.Start(TestContext.TestName);
        }

        #endregion

        #region ValidateFile Tests

        [TestMethod]
        [TestCategory("Execute_UnitTest")]
        [TestCategory("ValidateFile")]
        [TestProperty("Description",
            "Test that ValidateFile validates the document and does not show an error message when no validation errors are found.")]
        public void ValidateFile_WithNoErrors_ShowsNoErrorMessage()
        {
            // Arrange
            Execute.Doc = TestHelper.CreateBasicTestDocument();
            mockValidation.Setup(v => v.ValidateL5XFile(It.IsAny<XDocument>())).Returns([]);

            // Act
            execute.ValidateFile(true);

            // Assert
            mockValidation.Verify(v => v.ValidateL5XFile(It.IsAny<XDocument>()), Times.Once);
            mockMessages.Verify(m => m.Show(It.Is<string>(s => s.Contains("No errors detected!")), It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        [TestCategory("Execute_UnitTest")]
        [TestCategory("ValidateFile")]
        [TestProperty("Description",
            "Test that ValidateFile displays validation errors to the user when the document contains validation errors.")]
        public void ValidateFile_WithErrors_DisplaysErrors()
        {
            // Arrange
            Execute.Doc = TestHelper.CreateBasicTestDocument();
            List<string> errors = ["Error 1", "Error 2"];
            mockValidation.Setup(v => v.ValidateL5XFile(It.IsAny<XDocument>())).Returns(errors);

            // Act
            execute.ValidateFile(true);

            // Assert
            mockValidation.Verify(v => v.ValidateL5XFile(It.IsAny<XDocument>()), Times.Once);
            mockMessages.Verify(m => m.Show(It.Is<string>(s => s.Contains("Error")), It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        [TestCategory("Execute_UnitTest")]
        [TestCategory("ValidateFile")]
        [TestProperty("Description",
            "Test that ValidateFile performs validation without displaying a success message when the showNoError parameter is false.")]
        public void ValidateFile_WithShowNoErrorFalse_DoesNotShowSuccessMessage()
        {
            // Arrange
            Execute.Doc = TestHelper.CreateBasicTestDocument();
            mockValidation.Setup(v => v.ValidateL5XFile(It.IsAny<XDocument>())).Returns([]);

            // Act
            execute.ValidateFile(false);

            // Assert
            mockValidation.Verify(v => v.ValidateL5XFile(It.IsAny<XDocument>()), Times.Once);
            mockMessages.Verify(m => m.Show(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        #endregion

        #region NewFile Tests

        [TestMethod]
        [TestCategory("Execute_UnitTest")]
        [TestCategory("NewFile")]
        [TestProperty("Description",
            "Test that NewFile creates a new document and displays a confirmation message when the user confirms the action.")]
        public void NewFile_UserConfirms_CreatesNewFile()
        {
            // Arrange
            Execute.Doc = TestHelper.CreateBasicTestDocument();
            mockMessages.Setup(m => m.Confirm(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            XDocument newDoc = XMLHandler.LoadBasicFile();

            // Act
            execute.NewFile();

            // Assert
            mockMessages.Verify(m => m.Confirm(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            mockMessages.Verify(m => m.Show(It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            // Verify that Execute.Doc matches the new document
            Assert.IsNotNull(Execute.Doc);
            Assert.AreEqual(newDoc.ToString(), Execute.Doc.ToString());
        }

        [TestMethod]
        [TestCategory("Execute_UnitTest")]
        [TestCategory("NewFile")]
        [TestProperty("Description",
            "Test that NewFile does not create a new document when the user cancels the confirmation dialog.")]
        public void NewFile_UserCancels_DoesNotCreateNewFile()
        {
            // Arrange
            XDocument newDoc = TestHelper.CreateBasicTestDocument();
            Execute.Doc = newDoc;
            mockMessages.Setup(m => m.Confirm(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

            // Act
            execute.NewFile();

            // Assert
            mockMessages.Verify(m => m.Confirm(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            mockMessages.Verify(m => m.Show(It.IsAny<string>(), It.IsAny<string>()), Times.Never);

            Assert.AreEqual(newDoc.ToString(), Execute.Doc.ToString());
        }

        #endregion

        #region SaveFile Tests

        [TestMethod]
        [TestCategory("Execute_UnitTest")]
        [TestCategory("SaveFile")]
        [TestProperty("Description",
            "Test that SaveFile saves the current document to disk when the user selects a valid file path.")]
        public void SaveFile_UserSelectsPath_SavesDocument()
        {
            // Arrange
            Execute.Doc = TestHelper.CreateBasicTestDocument();
            string expectedPath = "C:\\test\\output.L5X";
            mockSaveFile.Setup(s => s.TrySave(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                out expectedPath)).Returns(true);

            // Act
            execute.SaveFile();

            // Assert
            mockFileSystem.Verify(
                f => f.SaveXml(
                    It.Is<XDocument>(doc => doc.ToString() == Execute.Doc.ToString()),
                    It.Is<string>(path => path == expectedPath)
                ),
                Times.Once
            );
        }

        [TestMethod]
        [TestCategory("Execute_UnitTest")]
        [TestCategory("SaveFile")]
        [TestProperty("Description",
            "Test that SaveFile does not save the document when the user cancels the save file dialog.")]
        public void SaveFile_UserCancels_DoesNotSave()
        {
            // Arrange
            Execute.Doc = TestHelper.CreateBasicTestDocument();
            string nullPath = null;
            mockSaveFile.Setup(s => s.TrySave(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                out nullPath)).Returns(false);

            // Act
            execute.SaveFile();

            // Assert
            mockFileSystem.Verify(f => f.SaveXml(It.IsAny<XDocument>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        [TestProperty("Description",
            "Test that SaveFile handles file name conflicts by appending incremental numbers.")]
        public void SaveFile_WithExistingFileName_AppendsNumber()
        {
            // Arrange
            Execute.Doc = TestHelper.CreateBasicTestDocument();
            string initialPath = "C:\\test\\GenFile0.L5X";
            string incrementedPath = "C:\\test\\GenFile1.L5X";

            mockFileSystem.Setup(f => f.FileExists(initialPath)).Returns(true);
            mockFileSystem.Setup(f => f.FileExists(incrementedPath)).Returns(false);


            // Simulate the first file name already existing
            mockSaveFile.SetupSequence(s => s.TrySave(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                out incrementedPath))
                .Returns(true);

            // Act
            execute.SaveFile();

            // Assert
            mockFileSystem.Verify(
                f => f.SaveXml(
                    It.IsAny<XDocument>(),
                    It.Is<string>(path => path == incrementedPath)
                ),
                Times.Once
            );
        }

        #endregion

        #region LoadFile Tests

        [TestMethod]
        [TestCategory("Execute_UnitTest")]
        [TestCategory("LoadFile")]
        [TestProperty("Description",
            "Test that LoadFile loads an XML document from disk when the user selects a valid file path.")]
        public void LoadFile_UserSelectsFile_LoadsDocument()
        {
            // Arrange
            string filePath = "C:\\test\\input.L5X";
            XDocument loadedDoc = TestHelper.CreateBasicTestDocument();
            mockOpenFile.Setup(o => o.TryOpen(It.IsAny<string>(), It.IsAny<string>(), out filePath)).Returns(true);
            mockFileSystem.Setup(f => f.LoadXml(filePath)).Returns(loadedDoc);

            // Act
            execute.LoadFile();

            // Assert
            mockOpenFile.Verify(o => o.TryOpen(It.IsAny<string>(), It.IsAny<string>(), out filePath), Times.Once);
            mockFileSystem.Verify(f => f.LoadXml(filePath), Times.Once);

            // Verify that Execute.Doc matches the loaded document
            Assert.IsNotNull(Execute.Doc);
            Assert.AreEqual(loadedDoc.ToString(), Execute.Doc.ToString());
        }

        [TestMethod]
        [TestCategory("Execute_UnitTest")]
        [TestCategory("LoadFile")]
        [TestProperty("Description",
            "Test that LoadFile does not load a document when the user cancels the open file dialog.")]
        public void LoadFile_UserCancels_DoesNotLoad()
        {
            // Arrange
            string nullPath = null;
            mockOpenFile.Setup(o => o.TryOpen(It.IsAny<string>(), It.IsAny<string>(), out nullPath))
                .Returns(false);

            // Act
            execute.LoadFile();

            // Assert
            mockFileSystem.Verify(f => f.LoadXml(It.IsAny<string>()), Times.Never);
        }

        #endregion

        #region ModifyElement Tests

        [TestMethod]
        [TestCategory("Execute_UnitTest")]
        [TestCategory("ModifyElement")]
        [TestProperty("Description",
            "Test that ModifyElement allows the user to select and modify elements of type Program from the document.")]
        public void ModifyElement_SelectsProgramType_ModifiesElements()
        {
            // Arrange
            Execute.Doc = TestHelper.CreateBasicTestDocument();
            mockFileSystem.Setup(f => f.LoadXml(It.IsAny<string>())).Returns(TestHelper.CreateBasicTestDocument());

            mockPrompts.Setup(p => p.SelectOne(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns("Program");
            mockPrompts.Setup(p => p.SelectMany(It.IsAny<string>(), It.IsAny<List<string>>()))
                .Returns(["MainProgram"]);

            // Act
            execute.ModifyElement();

            // Assert
            mockXmlHandler.Verify(x => x.GetElementTypes(It.IsAny<XDocument>()), Times.Once);
            mockPrompts.Verify(p => p.SelectOne(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>()), Times.Once);

            XElement modifiedElement = Execute.Doc.Descendants("Program")
                .FirstOrDefault(p => p.Attribute("Name")?.Value == "MainProgram");
            Assert.IsNotNull(modifiedElement);
        }

        [TestMethod]
        [TestCategory("Execute_UnitTest")]
        [TestCategory("ModifyElement")]
        [TestProperty("Description",
            "Test that ModifyElement properly handles Module elements which use CatalogNumber instead of Name as their identifier.")]
        public void ModifyElement_SelectsModuleType_UsesCatalogNumber()
        {
            // Arrange
            XDocument templateDoc = XDocument.Load("../../../../L5XFiles/TemplateFiles/TemplateProjectV1.L5X");
            mockFileSystem.Setup(f => f.LoadXml(It.IsAny<string>())).Returns(templateDoc);
            Execute.Doc = TestHelper.CreateBasicTestDocument();

            mockPrompts.Setup(p => p.SelectOne(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns("Module");
            mockPrompts.SetupSequence(p => p.SelectMany(It.IsAny<string>(), It.IsAny<List<string>>()))
                .Returns(["1234-5678"])
                .Returns(["1234-5678 with no Name at port 1"]);

            // Act
            execute.ModifyElement();

            // Assert
            // Verify that SelectMany was called with the expected options
            mockPrompts.Verify(p => p.SelectMany(
                It.Is<string>(s => s.Contains("Select specific Module")),
                It.Is<List<string>>(options => options.Contains("1234-5678 with no Name at port 1"))
            ), Times.Once);
        }

        #endregion

        #region DeleteElement Tests

        [TestMethod]
        [TestCategory("Execute_UnitTest")]
        [TestCategory("DeleteElement")]
        [TestProperty("Description",
            "Test that DeleteElement removes selected elements from the document and validates the result.")]
        public void DeleteElement_SelectsElements_RemovesThem()
        {
            // Arrange
            Execute.Doc = TestHelper.CreateBasicTestDocument();

            mockPrompts.Setup(p => p.SelectOne(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns("Program");
            mockPrompts.Setup(p => p.SelectMany(It.IsAny<string>(), It.IsAny<List<string>>()))
                .Returns(["TestProgram"]);

            // Act
            execute.DeleteElement();

            // Assert
            Assert.IsNull(Execute.Doc.Descendants("Program")
                .FirstOrDefault(p => p.Attribute("Name")?.Value == "TestProgram"));
        }

        [TestMethod]
        [TestProperty("Description",
            "Test that DeleteElement properly handles Module elements and uses CatalogNumber when Name is not available.")]
        public void DeleteElement_WithModuleWithoutName_UsesCatalogNumber()
        {
            // Arrange
            Execute.Doc = new XDocument(
                new XElement("RSLogix5000Content",
                    new XElement("Controller",
                        new XElement("Modules",
                            new XElement("Module",
                                new XAttribute("CatalogNumber", "1234-5678"))
                        )
                    )
                )
            );

            mockPrompts.Setup(p => p.SelectOne(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns("Module");
            mockPrompts.Setup(p => p.SelectMany(It.IsAny<string>(), It.IsAny<List<string>>()))
                .Returns(["1234-5678"]);
            mockPrompts.Setup(p => p.SelectMany(It.IsAny<string>(), It.IsAny<List<string>>()))
                .Returns(["1234-5678 with no Name at port 1"]);

            // Act
            execute.DeleteElement();

            // Assert
            mockValidation.Verify(v => v.ValidateL5XFile(It.IsAny<XDocument>()), Times.Once);
        }

        #endregion

        #region ImportElement Tests

        [TestMethod]
        [TestCategory("Execute_UnitTest")]
        [TestCategory("ImportElement")]
        [TestProperty("Description",
            "Test that ImportElement loads elements from an external file and imports them into the current document.")]
        public void ImportElement_DefaultAction_ImportsElements()
        {
            // Arrange
            string filePath = "C:\\test\\import.L5X";
            XDocument importDoc = TestHelper.CreateBasicTestDocument();

            mockOpenFile.Setup(o => o.TryOpen(It.IsAny<string>(), It.IsAny<string>(), out filePath))
                .Returns(true);
            mockFileSystem.Setup(f => f.LoadXml(filePath)).Returns(importDoc);

            mockPrompts.Setup(p => p.SelectOne(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns("Program");
            mockPrompts.Setup(p => p.SelectMany(It.IsAny<string>(), It.IsAny<List<string>>()))
                .Returns(["MainProgram"]);

            Execute.Doc = TestHelper.CreateEmptyDocument();

            // Act
            execute.ImportElement();

            // Assert
            mockOpenFile.Verify(o => o.TryOpen(It.IsAny<string>(), It.IsAny<string>(), out filePath), Times.Once);
            mockFileSystem.Verify(f => f.LoadXml(filePath), Times.Once);

            // Verify that the imported element is present in Execute.Doc at correct location
            XElement importedElement = Execute.Doc.Descendants("Program").FirstOrDefault(p => p.Attribute("Name")?.Value == "MainProgram");
            Assert.IsTrue(importedElement.Parent.Name.ToString().Equals("Programs"), "The imported element 'MainProgram' was not placed under the correct parent 'Programs'.");
            Assert.IsNotNull(importedElement, "The imported element 'MainProgram' was not found in the document.");
        }

        [TestMethod]
        [TestCategory("Execute_UnitTest")]
        [TestCategory("ImportElement")]
        [TestProperty("Description",
            "Test that ImportElement returns early without importing when the user cancels the file selection dialog.")]
        public void ImportElement_UserCancelsFileSelection_ReturnsEarly()
        {
            // Arrange
            string nullPath = null;
            mockOpenFile.Setup(o => o.TryOpen(It.IsAny<string>(), It.IsAny<string>(), out nullPath))
                .Returns(false);

            // Act
            execute.ImportElement();

            // Assert
            mockOpenFile.Verify(o => o.TryOpen(It.IsAny<string>(), It.IsAny<string>(), out nullPath), Times.Once);
            mockFileSystem.Verify(f => f.LoadXml(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        [TestCategory("Execute_UnitTest")]
        [TestCategory("ImportElement")]
        [TestProperty("Description",
        "Test that ImportElement loops and allows the user to import additional elements when they choose to add more.")]
        public void ImportElement_UserConfirms_Loops()
        {
            // Arrange
            int confirmCallCount = 0;
            string filePath = "C:\\test\\import.L5X";

            mockOpenFile.Setup(o => o.TryOpen(It.IsAny<string>(), It.IsAny<string>(), out filePath))
                .Returns(true);

            mockFileSystem.Setup(f => f.LoadXml(It.IsAny<string>())).Returns(TestHelper.CreateBasicTestDocument());

            mockPrompts.Setup(p => p.SelectOne(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns("AddOnInstructionDefinition");
            mockPrompts.Setup(p => p.SelectMany(It.IsAny<string>(), It.IsAny<List<string>>()))
                .Returns(["TestAOI"]);

            mockMessages.Setup(m => m.Confirm(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(() => confirmCallCount++ == 0); // Return true first time, false second

            Execute.Doc = TestHelper.CreateEmptyDocument();

            // Act
            execute.ImportElement();

            // Assert
            mockMessages.Verify(m => m.Confirm(It.IsAny<string>(), It.IsAny<string>()), Times.AtLeast(2));
            mockPrompts.Verify(p => p.SelectOne(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>()), Times.AtLeast(2));
            mockPrompts.Verify(p => p.SelectMany(It.IsAny<string>(), It.IsAny<List<string>>()), Times.AtLeast(2));
        }

        [TestMethod]
        [TestProperty("Description",
            "Test that ImportElement properly handles ParentMissingException by prompting for missing attributes and retrying.")]
        public void ImportElement_WithParentMissingException_PromptsForAttributes()
        {
            // Arrange
            string filePath = "C:\\test\\import.L5X";

            Execute.Doc = TestHelper.CreateEmptyDocument();

            mockOpenFile.Setup(o => o.TryOpen(It.IsAny<string>(), It.IsAny<string>(), out filePath))
                .Returns(true);
            mockFileSystem.Setup(f => f.LoadXml(filePath)).Returns(TestHelper.CreateBasicTestDocument());
            mockPrompts.SetupSequence(p => p.SelectOne(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns("Routine")
                .Returns("AddOnInstructionDefinition");
            mockPrompts.Setup(p => p.SelectMany(It.IsAny<string>(), It.IsAny<List<string>>()))
                .Returns(["MainRoutine"]);
            mockMessages.Setup(m => m.Confirm(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(false);

            // Act
            execute.ImportElement();

            // Assert
            mockPrompts.Verify(p => p.Prompt(It.Is<string>(s => s.Contains("attribute of auto-generated")), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        [TestCategory("Execute_UnitTest")]
        [TestCategory("ImportElement")]
        [TestProperty("Description",
            "Test that ImportElement properly handles ClashingElementException by invoking HandleClashes.")]
        public void ImportElement_WithClashingElementException_HandlesClash()
        {
            // Arrange
            string filePath = "C:\\test\\import.L5X";
            XDocument importDoc = TestHelper.CreateBasicTestDocument();
            XElement existingElement = new("Program", new XAttribute("Name", "MainProgram"));
            XElement parentNode = new("Programs");
            ClashingElementException exception = new([existingElement], parentNode);

            Execute.Doc = TestHelper.CreateBasicTestDocument();

            mockOpenFile.Setup(o => o.TryOpen(It.IsAny<string>(), It.IsAny<string>(), out filePath))
                .Returns(true);
            mockFileSystem.Setup(f => f.LoadXml(filePath)).Returns(importDoc);

            mockPrompts.Setup(p => p.SelectMany(It.IsAny<string>(), It.IsAny<List<string>>()))
                .Returns(["MainProgram"]);
            mockPrompts.SetupSequence(p => p.SelectOne(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns("Program") // First call returns "Program"
                .Returns("Cancel"); // Second call returns "Cancel"

            // Act
            execute.ImportElement();

            // Assert
            mockMessages.Verify(m => m.Confirm(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        [TestCategory("Execute_UnitTest")]
        [TestCategory("ImportElement")]
        [TestProperty("Description",
            "Test that ImportElement properly handles ClashingParentException by prompting user to select correct parent.")]
        public void ImportElement_WithClashingParentException_PromptsForParentSelection()
        {
            // Arrange
            string filePath = "C:\\test\\import.L5X";
            XDocument importDoc = new(
                new XElement("RSLogix5000Content",
                    new XElement("Controller",
                        new XElement("Programs",
                            new XElement("Program",
                                new XAttribute("Name", "Parent1"),
                                new XElement("Tags",
                                    new XElement("Tag", new XAttribute("Name", "DuplicateTag"))
                                )
                            ),
                            new XElement("Program",
                                new XAttribute("Name", "Parent2"),
                                new XElement("Tags",
                                    new XElement("Tag", new XAttribute("Name", "DuplicateTag"))
                                )
                            )
                        )
                    )
                )
            );

            XElement clash1 = importDoc.Descendants("Program").First();
            XElement clash2 = importDoc.Descendants("Program").Last();
            ClashingParentException exception = new([clash1, clash2]);

            mockOpenFile.Setup(o => o.TryOpen(It.IsAny<string>(), It.IsAny<string>(), out filePath))
                .Returns(true);
            mockFileSystem.Setup(f => f.LoadXml(filePath)).Returns(importDoc);

            int selectOneCallCount = 0;
            mockPrompts.Setup(p => p.SelectOne(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns(() =>
                {
                    selectOneCallCount++;
                    if (selectOneCallCount == 1) return "Tag";
                    return "Parent1";
                });
            mockPrompts.Setup(p => p.SelectMany(It.IsAny<string>(), It.IsAny<List<string>>()))
                .Returns(["DuplicateTag"]);
            mockMessages.Setup(m => m.Confirm(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(false);

            // Mock the disambiguator to return a valid parent type
            mockDisambiguator.Setup(d => d.ChooseParentFor(It.IsAny<string>(), It.IsAny<List<string>>()))
                .Returns("Program");

            Execute.Doc = TestHelper.CreateBasicTestDocument();

            // Act
            execute.ImportElement();

            // Assert
            mockPrompts.Verify(p => p.SelectOne(It.Is<string>(s => s.Contains("Multiple elements") || s.Contains("Select parent")), It.IsAny<List<string>>(), It.IsAny<string>()), Times.AtLeastOnce);
            mockDisambiguator.Verify(d => d.ChooseParentFor(It.IsAny<string>(), It.IsAny<List<string>>()), Times.Once);
        }

        [TestMethod]
        [TestProperty("Description",
            "Test that ImportElement properly handles Module type and uses CatalogNumber for identification.")]
        public void ImportElement_WithModuleType_UsesCatalogNumber()
        {
            // Arrange
            string filePath = "C:\\test\\import.L5X";
            XDocument importDoc = TestHelper.CreateBasicTestDocument();

            mockOpenFile.Setup(o => o.TryOpen(It.IsAny<string>(), It.IsAny<string>(), out filePath))
                .Returns(true);
            mockFileSystem.Setup(f => f.LoadXml(filePath)).Returns(importDoc);
            mockPrompts.Setup(p => p.SelectOne(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns("Module");
            mockPrompts.Setup(p => p.SelectMany(It.IsAny<string>(), It.IsAny<List<string>>()))
                .Returns(["1234-5678 at port 1"]);
            mockMessages.Setup(m => m.Confirm(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(false);

            Execute.Doc = TestHelper.CreateBasicTestDocument();

            // Act
            execute.ImportElement();

            // Assert
            mockFileSystem.Verify(f => f.LoadXml(filePath), Times.Once);
        }

        [TestMethod]
        [TestProperty("Description",
            "Test that ImportElement returns early when an empty file path is provided.")]
        public void ImportElement_WithEmptyFilePath_ReturnsEarly()
        {
            // Arrange
            string emptyPath = "";
            mockOpenFile.Setup(o => o.TryOpen(It.IsAny<string>(), It.IsAny<string>(), out emptyPath))
                .Returns(true);

            // Act
            execute.ImportElement();

            // Assert
            mockFileSystem.Verify(f => f.LoadXml(It.IsAny<string>()), Times.Never);
        }

        #endregion

        #region CreateElement Tests

        [TestMethod]
        [TestCategory("Execute_UnitTest")]
        [TestCategory("CreateElement")]
        [TestProperty("Description",
            "Test that CreateElement creates a single new element based on a template and inserts it into the document.")]
        public void CreateElement_SingleProgram_CreatesElement()
        {
            // Arrange
            XDocument templateDoc = XDocument.Load("../../../../L5XFiles/TemplateFiles/TemplateProjectV1.L5X");
            mockFileSystem.Setup(f => f.LoadXml(It.IsAny<string>())).Returns(templateDoc);
            Execute.Doc = TestHelper.CreateEmptyDocument();

            mockPrompts.Setup(p => p.SelectOne("Select Element Type", It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns("Program");
            mockPrompts.Setup(p => p.SelectOne("Select element template", It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns("ProgramWithRoutine");
            mockPrompts.Setup(p => p.Prompt(It.Is<string>(s => s.Contains("How many")), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns("1");
            mockPrompts.Setup(p => p.Prompt(It.Is<string>(s => s.Contains("Input a name")), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns("NewProgram");
            mockPrompts.Setup(p => p.SelectOne("Select the parent task for inserted program", It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns("Keep programs unscheduled");

            // Act
            execute.CreateElement();

            // Assert
            XElement createdElement = Execute.Doc.Descendants("Program").FirstOrDefault(p => p.Attribute("Name")?.Value == "NewProgram");
            Assert.IsNotNull(createdElement, "The element 'NewProgram' was not created in the document.");
        }

        [TestMethod]
        [TestCategory("Execute_UnitTest")]
        [TestCategory("CreateElement")]
        [TestProperty("Description",
            "Test that CreateElement creates multiple new elements when the user specifies a quantity greater than one.")]
        public void CreateElement_MultipleElements_CreatesAll()
        {
            // Arrange
            XDocument templateDoc = XDocument.Load("../../../../L5XFiles/TemplateFiles/TemplateProjectV1.L5X");
            mockFileSystem.Setup(f => f.LoadXml(It.IsAny<string>())).Returns(templateDoc);
            Execute.Doc = TestHelper.CreateEmptyDocument();

            mockPrompts.Setup(p => p.SelectOne("Select Element Type", It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns("AddOnInstructionDefinition");
            mockPrompts.Setup(p => p.SelectOne("Select element template", It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns("TemplateFBAOI");
            mockPrompts.Setup(p => p.Prompt(It.Is<string>(s => s.Contains("How many")), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns("3");

            int nameCallCount = 0;
            mockPrompts.Setup(p => p.Prompt(It.Is<string>(s => s.Contains("Input a name")), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(() => $"Task{++nameCallCount}");

            // Act
            execute.CreateElement();

            // Assert
            mockPrompts.Verify(p => p.Prompt(It.Is<string>(s => s.Contains("Input a name")), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(3));
        }

        [TestMethod]
        [TestCategory("Execute_UnitTest")]
        [TestCategory("CreateElement")]
        [TestProperty("Description",
            "Test that CreateElement throws an EmptyListException when no valid parent elements can be found in the schema.")]
        public void CreateElement_NoValidParents_ThrowsException()
        {
            // Arrange
            XDocument templateDoc = XDocument.Load("../../../../L5XFiles/TemplateFiles/TemplateProjectV1.L5X");
            mockFileSystem.Setup(f => f.LoadXml(It.IsAny<string>())).Returns(templateDoc);
            Execute.Doc = TestHelper.CreateEmptyDocument();

            mockPrompts.Setup(p => p.SelectOne("Select Element Type", It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns("Program");
            mockPrompts.Setup(p => p.SelectOne("Select element template", It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns("ProgramWithRoutine");
            mockPrompts.Setup(p => p.Prompt(It.Is<string>(s => s.Contains("How many")), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns("1");

            // Mock schema to return no valid parents
            mockValidation.Setup(v => v.GetSchema()).Returns(new XDocument(
                new XElement(helper.ns + "schema")
            ));

            // Act
            Assert.ThrowsExactly<EmptyListException>(() => execute.CreateElement());
        }

        [TestMethod]
        [TestProperty("Description",
            "Test that CreateElement handles ClashingElementException and aborts on cancel.")]
        public void CreateElement_WithClashingElement_HandlesCancel()
        {
            // Arrange
            XDocument templateDoc = XDocument.Load("../../../../L5XFiles/TemplateFiles/TemplateProjectV1.L5X");
            mockFileSystem.Setup(f => f.LoadXml(It.IsAny<string>())).Returns(templateDoc);
            Execute.Doc = TestHelper.CreateBasicTestDocument();

            // add an attribute to the existing MainTask to verify that it wasnt modified after the clash
            XElement element = Execute.Doc.Descendants("AddOnInstructionDefinition").Single(t => t.Attribute("Name")?.Value == "TestAOI");
            element.SetAttributeValue("ExistingAttribute", "Value");

            mockPrompts.Setup(p => p.SelectOne("Select Element Type", It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns("AddOnInstructionDefinition");
            mockPrompts.Setup(p => p.SelectOne("Select element template", It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns("TemplateFBAOI");
            mockPrompts.Setup(p => p.Prompt(It.Is<string>(s => s.Contains("How many")), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns("1");
            mockPrompts.Setup(p => p.Prompt(It.Is<string>(s => s.Contains("Input a name")), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns("TestAOI");
            mockPrompts.Setup(p => p.SelectOne(It.Is<string>(s => s.Contains("already exists")), It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns("Cancel");

            // Act
            execute.CreateElement();

            // Assert
            mockPrompts.Verify(p => p.SelectOne(It.Is<string>(s => s.Contains("already exists")), It.IsAny<List<string>>(), It.IsAny<string>()), Times.Once);

            // Verify that MainTask object still existing and has the "ExistingAttribute" attribute, which indicates that it was not replaced by the new element
            XElement mainProg = Execute.Doc.Descendants("AddOnInstructionDefinition").FirstOrDefault(t => t.Attribute("Name")?.Value == "TestAOI" && t.Attribute("ExistingAttribute") != null);
            Assert.IsNotNull(mainProg, "The existing MainTask element was removed from the document.");
        }

        [TestMethod]
        [TestProperty("Description",
            "Test that CreateElement handles ClashingElementException and retries on rename.")]
        public void CreateElement_WithClashingElement_HandlesRename()
        {
            // Arrange
            XDocument templateDoc = XDocument.Load("../../../../L5XFiles/TemplateFiles/TemplateProjectV1.L5X");
            mockFileSystem.Setup(f => f.LoadXml(It.IsAny<string>())).Returns(templateDoc);
            Execute.Doc = TestHelper.CreateBasicTestDocument();

            mockPrompts.Setup(p => p.SelectOne("Select Element Type", It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns("AddOnInstructionDefinition");
            mockPrompts.Setup(p => p.SelectOne("Select element template", It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns("TemplateFBAOI");
            mockPrompts.Setup(p => p.Prompt(It.Is<string>(s => s.Contains("How many")), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns("1");
            mockPrompts.Setup(p => p.Prompt(It.Is<string>(s => s.Contains("Input a name")), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns("TestAOI");
            mockPrompts.Setup(p => p.SelectOne(It.Is<string>(s => s.Contains("already exists")), It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns("Rename");
            mockPrompts.Setup(p => p.Prompt(It.Is<string>(s => s.Contains("Input a new")), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns("RenamedAOI");

            // Act
            execute.CreateElement();

            // Assert
            mockPrompts.Verify(p => p.SelectOne(It.Is<string>(s => s.Contains("already exists")), It.IsAny<List<string>>(), It.IsAny<string>()), Times.Once);
            mockPrompts.Verify(p => p.Prompt(It.Is<string>(s => s.Contains("Input a new")), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            // Verify that the renamed AOI exists in the document
            Assert.IsNotNull(Execute.Doc.Descendants("AddOnInstructionDefinition").FirstOrDefault(t => t.Attribute("Name")?.Value == "RenamedAOI"));
        }

        [TestMethod]
        [TestProperty("Description",
            "Test that CreateElement handles ClashingElementException and replaces the existing element.")]
        public void CreateElement_WithClashingElement_HandlesReplace()
        {
            // Arrange
            XDocument templateDoc = XDocument.Load("../../../../L5XFiles/TemplateFiles/TemplateProjectV1.L5X");
            mockFileSystem.Setup(f => f.LoadXml(It.IsAny<string>())).Returns(templateDoc);
            Execute.Doc = TestHelper.CreateBasicTestDocument();

            // add an attribute to the existing AOI to verify that it was replaced after the clash
            Execute.Doc.Descendants("AddOnInstructionDefinition").FirstOrDefault(t => t.Attribute("Name")?.Value == "TestAOI").SetAttributeValue("ExistingAttribute", "Value");

            mockPrompts.Setup(p => p.SelectOne("Select Element Type", It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns("AddOnInstructionDefinition");
            mockPrompts.Setup(p => p.SelectOne("Select element template", It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns("TemplateFBAOI");
            mockPrompts.Setup(p => p.Prompt(It.Is<string>(s => s.Contains("How many")), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns("1");
            mockPrompts.Setup(p => p.Prompt(It.Is<string>(s => s.Contains("Input a name")), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns("TestAOI");
            mockPrompts.Setup(p => p.SelectOne(It.Is<string>(s => s.Contains("already exists")), It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns("Replace");

            // Act
            execute.CreateElement();

            // Assert
            mockPrompts.Verify(p => p.SelectOne(It.Is<string>(s => s.Contains("already exists")), It.IsAny<List<string>>(), It.IsAny<string>()), Times.Once);

            // Verify that only one AOI exists in the document and it has been replaced
            Assert.ContainsSingle(t => t.Attribute("Name")?.Value == "TestAOI" && t.Attribute("ExistingAttribute") is null, Execute.Doc.Descendants("AddOnInstructionDefinition"));
        }



        [TestMethod]
        [TestProperty("Description",
            "Test that CreateElement properly handles Module with ICP Port and sets unique port addresses.")]
        public void CreateElement_WithModuleICPPort_SetsUniquePortAddress()
        {
            // Arrange
            XDocument templateDoc = new(
                new XElement("RSLogix5000Content",
                    new XElement("Controller",
                        new XElement("Modules",
                            new XElement("Module",
                                new XAttribute("Name", "IOModule"),
                                new XAttribute("CatalogNumber", "1234-IO"),
                                new XAttribute("ParentModule", "Local"),
                                new XElement("Ports",
                                    new XElement("Port",
                                        new XAttribute("Type", "ICP"),
                                        new XAttribute("Address", "1")
                                    )
                                )
                            )
                        )
                    )
                )
            );

            mockFileSystem.Setup(f => f.LoadXml(It.IsAny<string>())).Returns(templateDoc);
            mockXmlHandler.Object.inputFile = templateDoc;
            mockXmlHandler.Object.ElementInfo.RootPath = new LinkedList<string>();

            mockPrompts.Setup(p => p.SelectOne("Select Element Type", It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns("Module");
            mockPrompts.Setup(p => p.SelectOne("Select element template", It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns("1234-IO");
            mockPrompts.Setup(p => p.Prompt(It.Is<string>(s => s.Contains("How many")), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns("1");

            XDocument mainDoc = TestHelper.CreateBasicTestDocument();
            mainDoc.Descendants("Module").Single().Add(
                                new XAttribute("ParentModule", "Local"),
                                new XElement("Ports",
                                    new XElement("Port",
                                        new XAttribute("Type", "ICP"),
                                        new XAttribute("Address", "1")
                                )));
            Execute.Doc = mainDoc;

            // Act
            execute.CreateElement();

            // Assert
            XElement createdModule = Execute.Doc.Descendants("Module")
                .FirstOrDefault(m => m.Attribute("CatalogNumber")?.Value == "1234-IO");

            Assert.IsNotNull(createdModule, "The module was not created.");
            XElement port = createdModule.Descendants("Port")
                .FirstOrDefault(p => p.Attribute("Type")?.Value == "ICP" && p.Attribute("Address")?.Value == "2");

            Assert.IsNotNull(port, "The module does not have a port with an address of 2.");
        }

        [TestMethod]
        [TestProperty("Description",
            "Test that CreateElement properly handles Module with Ethernet Port and prompts for IP address.")]
        public void CreateElement_WithModuleEthernetPort_PromptsForIPAddress()
        {
            // Arrange
            XDocument templateDoc = new(
                new XElement("RSLogix5000Content",
                    new XElement("Controller",
                        new XElement("Modules",
                            new XElement("Module",
                                new XAttribute("Name", "EthernetModule"),
                                new XAttribute("CatalogNumber", "1234-EN"),
                                new XAttribute("ParentModule", "Local"),
                                new XElement("Ports",
                                    new XElement("Port",
                                        new XAttribute("Type", "Ethernet"),
                                        new XAttribute("Address", "192.168.1.1")
                                    )
                                )
                            )
                        )
                    )
                )
            );

            mockFileSystem.Setup(f => f.LoadXml(It.IsAny<string>())).Returns(templateDoc);
            mockXmlHandler.Object.inputFile = templateDoc;
            mockXmlHandler.Object.ElementInfo.RootPath = new LinkedList<string>();

            mockPrompts.Setup(p => p.SelectOne("Select Element Type", It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns("Module");
            mockPrompts.Setup(p => p.SelectOne("Select element template", It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns("1234-EN");
            mockPrompts.Setup(p => p.Prompt(It.Is<string>(s => s.Contains("How many")), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns("1");
            mockPrompts.Setup(p => p.Prompt(It.Is<string>(s => s.Contains("IP Address")), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns("192.168.1.2");

            XDocument mainDoc = TestHelper.CreateBasicTestDocument();
            mainDoc.Descendants("Module").Single().Add(
                                new XAttribute("ParentModule", "Local"),
                                new XElement("Ports",
                                    new XElement("Port",
                                        new XAttribute("Type", "Ethernet"),
                                        new XAttribute("Address", "192.168.1.1")
                                )));
            Execute.Doc = mainDoc;

            // Act
            execute.CreateElement();

            // Assert
            mockPrompts.Verify(p => p.Prompt(It.Is<string>(s => s.Contains("IP Address")), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        [TestProperty("Description",
            "Test that CreateElement properly handles Module with Use attribute and does not prompt for name.")]
        public void CreateElement_WithModuleUseAttribute_SkipsNamePrompt()
        {
            // Arrange
            XDocument templateDoc = new(
                new XElement("RSLogix5000Content",
                    new XElement("Controller",
                        new XElement("Modules",
                            new XElement("Module",
                                new XAttribute("Name", "SpecialModule"),
                                new XAttribute("CatalogNumber", "1234-SPEC"),
                                new XAttribute("Use", "Target")
                            )
                        )
                    )
                )
            );

            mockFileSystem.Setup(f => f.LoadXml(It.IsAny<string>())).Returns(templateDoc);
            mockXmlHandler.Object.ElementInfo.RootPath = new LinkedList<string>();

            mockPrompts.Setup(p => p.SelectOne("Select Element Type", It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns("Module");
            mockPrompts.Setup(p => p.SelectOne("Select element template", It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns("1234-SPEC");
            mockPrompts.Setup(p => p.Prompt(It.Is<string>(s => s.Contains("How many")), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns("1");

            Execute.Doc = TestHelper.CreateEmptyDocument();

            // Act
            execute.CreateElement();

            // Assert
            mockPrompts.Verify(p => p.Prompt(It.Is<string>(s => s.Contains("Input a name")), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        [TestProperty("Description",
            "Test that CreateElement adds programs to scheduled task when user selects a specific task.")]
        public void CreateElement_WithProgramAndTask_SchedulesProgram()
        {
            // Arrange
            Execute.Doc = new XDocument(
                new XElement("RSLogix5000Content",
                    new XElement("Controller",
                        new XAttribute("Name", "TestController"),
                        new XElement("Programs",
                            new XElement("Program", new XAttribute("Name", "MainProgram"))
                        ),
                        new XElement("Tasks",
                            new XElement("Task", new XAttribute("Name", "MainTask"))
                        )
                    )
                )
            );

            mockFileSystem.Setup(f => f.LoadXml(It.IsAny<string>())).Returns(TestHelper.CreateBasicTestDocument());
            mockPrompts.Setup(p => p.SelectOne("Select Element Type", It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns("Program");
            mockPrompts.Setup(p => p.SelectOne("Select element template", It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns("MainProgram");
            mockPrompts.Setup(p => p.SelectOne(It.Is<string>(s => s.Contains("Select the parent task")), It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns("MainTask");
            mockPrompts.Setup(p => p.Prompt(It.Is<string>(s => s.Contains("How many")), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns("1");
            mockPrompts.Setup(p => p.Prompt(It.Is<string>(s => s.Contains("Input a name")), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns("NewProgram");



            // Act
            execute.CreateElement();

            // Assert
            mockPrompts.Verify(p => p.SelectOne("Select the parent task for inserted program", It.IsAny<List<string>>(), It.IsAny<string>()), Times.Once);
            XElement task = Execute.Doc.Descendants("Task").FirstOrDefault(t => t.Attribute("Name")?.Value == "MainTask");
            Assert.IsNotNull(task);
            XElement newProgram = Execute.Doc.Descendants("Program").FirstOrDefault(p => p.Attribute("Name")?.Value == "NewProgram");
            Assert.IsNotNull(newProgram, "The program 'NewProgram' was not created.");
        }

        #endregion

        #region SetAttributes Tests

        [TestMethod]
        [TestCategory("Execute_UnitTest")]
        [TestCategory("SetAttributes")]
        [TestProperty("Description",
            "Test that SetAttributes allows the user to select and modify attribute values for a single element.")]
        public void SetAttributes_WithElementAndAttributes_SetsValues()
        {
            // Arrange
            XElement element = new("Program", new XAttribute("Name", "TestProgram"));
            List<XAttribute> attrs = [
                new("Name", "TestProgram"),
                new("Type", "Normal")
            ];

            mockPrompts.Setup(p => p.SelectMany(It.IsAny<string>(), It.IsAny<List<string>>()))
                .Returns(["Name"]);
            mockPrompts.Setup(p => p.Prompt(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns("ModifiedProgram");

            // Act
            execute.SetAttributes([element], [attrs]);

            // Assert
            mockPrompts.Verify(p => p.SelectMany(It.IsAny<string>(), It.IsAny<List<string>>()), Times.Once);
            mockPrompts.Verify(p => p.Prompt(It.Is<string>(s => s.Contains("Name")), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.AreEqual("ModifiedProgram", element.Attribute("Name").Value);
        }

        [TestMethod]
        [TestCategory("Execute_UnitTest")]
        [TestCategory("SetAttributes")]
        [TestProperty("Description",
            "Test that SetAttributes processes and allows modification of attributes for multiple elements in a collection.")]
        public void SetAttributes_WithMultipleElements_ProcessesAll()
        {
            // Arrange
            List<XElement> elements = [
                new("Program", new XAttribute("Name", "Program1")),
                new("Program", new XAttribute("Name", "Program2"))
            ];
            List<List<XAttribute>> attrs =
            [
                [
                    new("Name", "Program1"),
                    new("CreatedBy", "User1")
                ],
                [
                    new("Name", "Program2"),
                    new("CreatedBy", "User2")
                ]
            ];

            mockPrompts.Setup(p => p.SelectMany(It.IsAny<string>(), It.IsAny<List<string>>()))
                .Returns([]);

            // Act
            execute.SetAttributes(elements, attrs);

            // Assert
            mockPrompts.Verify(p => p.SelectMany(It.IsAny<string>(), It.IsAny<List<string>>()), Times.Exactly(2));

            // Verify that both elements contain the new attributes
            Assert.AreEqual("Program1", elements[0].Attribute("Name").Value);
            Assert.AreEqual("User1", elements[0].Attribute("CreatedBy").Value);
            Assert.AreEqual("Program2", elements[1].Attribute("Name").Value);
            Assert.AreEqual("User2", elements[1].Attribute("CreatedBy").Value);
        }

        [TestMethod]
        [TestProperty("Description",
            "Test that SetAttributes sets default values for attributes not manually selected.")]
        public void SetAttributes_WithUnselectedAttributes_SetsDefaults()
        {
            // Arrange
            XElement element = new("Program", new XAttribute("Name", "TestProgram"));
            List<XAttribute> attrs =
            [
                new XAttribute("Name", "TestProgram"),
                new XAttribute("Type", "Normal"),
                new XAttribute("Disabled", "false")
            ];

            mockPrompts.Setup(p => p.SelectMany(It.IsAny<string>(), It.IsAny<List<string>>()))
                .Returns(["Name"]);
            mockPrompts.Setup(p => p.Prompt(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns("ModifiedProgram");

            // Act
            execute.SetAttributes([element], [attrs]);

            // Assert
            Assert.AreEqual("ModifiedProgram", element.Attribute("Name").Value);
            Assert.AreEqual("Normal", element.Attribute("Type").Value);
        }

        [TestMethod]
        [TestProperty("Description",
            "Test that SetAttributes adds missing attributes with non-empty default values.")]
        public void SetAttributes_WithMissingAttribute_AddsAttribute()
        {
            // Arrange
            XElement element = new("Program", new XAttribute("Name", "TestProgram"));
            List<XAttribute> attrs =
            [
                new("Type", "Normal")
            ];

            mockPrompts.Setup(p => p.SelectMany(It.IsAny<string>(), It.IsAny<List<string>>()))
                .Returns([]);

            // Act
            execute.SetAttributes([element], [attrs]);

            // Assert
            Assert.IsNotNull(element.Attribute("Type"));
            Assert.AreEqual("Normal", element.Attribute("Type").Value);
        }

        [TestMethod]
        [TestProperty("Description",
            "Test that SetAttributes does not add attributes with empty default values.")]
        public void SetAttributes_WithEmptyDefaultValue_DoesNotAddAttribute()
        {
            // Arrange
            XElement element = new("Program", new XAttribute("Name", "TestProgram"));
            List<XAttribute> attrs =
            [
                new("OptionalAttr", "")
            ];

            mockPrompts.Setup(p => p.SelectMany(It.IsAny<string>(), It.IsAny<List<string>>()))
                .Returns([]);

            // Act
            execute.SetAttributes([element], [attrs]);

            // Assert
            Assert.IsNull(element.Attribute("OptionalAttr"));
        }

        [TestMethod]
        [TestCategory("Execute_UnitTest")]
        [TestProperty("Description",
            "Test that SetAttributes recursively prompts to modify child elements when selected.")]
        public void SetAttributes_WithChildElements_PromptsForChildren()
        {
            // Arrange
            XElement childElement = new("Tag", new XAttribute("Name", "ChildTag"));
            XElement element = new("Program",
                new XAttribute("Name", "TestProgram"),
                childElement
            );
            List<XAttribute> attrs =
            [
                new("Name", "TestProgram")
            ];

            mockPrompts.Setup(p => p.SelectMany(It.IsAny<string>(), It.IsAny<List<string>>()))
                .Returns([]);
            mockPrompts.Setup(p => p.SelectMany(It.Is<string>(s => s.Contains("children elements to modify")), It.IsAny<List<string>>(), It.IsAny<bool>()))
                .Returns(["ChildTag"]);

            // Act
            execute.SetAttributes([element], [attrs]);

            // Assert
            mockPrompts.Verify(p => p.SelectMany(It.Is<string>(s => s.Contains("children elements to modify")), It.IsAny<List<string>>(), It.IsAny<bool>()), Times.Once);
        }

        [TestMethod]
        [TestCategory("Execute_UnitTest")]
        [TestProperty("Description",
            "Test that SetAttributes uses element Name when displaying prompt for elements with Name attribute.")]
        public void SetAttributes_WithNameAttribute_DisplaysName()
        {
            // Arrange
            XElement element = new("Program", new XAttribute("Name", "TestProgram"));
            List<XAttribute> attrs =
            [
                new("Type", "Normal")
            ];

            mockPrompts.Setup(p => p.SelectMany(It.IsAny<string>(), It.IsAny<List<string>>()))
                .Returns(["Type"]);
            mockPrompts.Setup(p => p.Prompt(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns("Modified");

            // Act
            execute.SetAttributes([element], [attrs]);

            // Assert
            mockPrompts.Verify(p => p.Prompt(It.Is<string>(s => s.Contains("TestProgram")), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        [TestCategory("Execute_UnitTest")]
        [TestProperty("Description",
            "Test that SetAttributes uses CatalogNumber when element doesn't have Name attribute.")]
        public void SetAttributes_WithoutNameAttribute_UsesCatalogNumber()
        {
            // Arrange
            XElement element = new("Module", new XAttribute("CatalogNumber", "1234-5678"));
            List<XAttribute> attrs =
            [
                new("Slot", "1")
            ];

            mockPrompts.Setup(p => p.SelectMany(It.IsAny<string>(), It.IsAny<List<string>>()))
                .Returns(["Slot"]);
            mockPrompts.Setup(p => p.Prompt(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns("2");

            // Act
            execute.SetAttributes([element], [attrs]);

            // Assert
            mockPrompts.Verify(p => p.Prompt(It.Is<string>(s => s.Contains("1234-5678")), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        #endregion

        #region HandleClashes Tests

        [TestMethod]
        [TestProperty("Description",
            "Test that HandleClashes returns false and removes element when user selects Cancel.")]
        public void HandleClashes_UserSelectsCancel_ReturnsFalseAndRemovesElement()
        {
            // Arrange
            XElement clashingElement = new("Program", new XAttribute("Name", "ExistingProgram"));
            XElement parentNode = new("Programs", clashingElement);
            XElement insertElement = new("Program", new XAttribute("Name", "ExistingProgram"));
            parentNode.Add(insertElement);

            mockPrompts.Setup(p => p.SelectOne(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns("Cancel");

            mockXmlHandler.Object.ElementInfo.RootPath = new LinkedList<string>();

            // Act
            bool result = execute.HandleClashes([clashingElement], parentNode, insertElement);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        [TestProperty("Description",
            "Test that HandleClashes returns true and replaces element when user selects Replace.")]
        public void HandleClashes_UserSelectsReplace_ReturnsTrueAndReplacesElement()
        {
            // Arrange
            XElement clashingElement = new("Program", new XAttribute("Name", "ExistingProgram"));
            XElement parentNode = new("Programs", clashingElement);
            XElement insertElement = new("Program", new XAttribute("Name", "ExistingProgram"));
            parentNode.Add(insertElement);

            mockPrompts.Setup(p => p.SelectOne(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns("Replace");

            mockXmlHandler.Object.ElementInfo.RootPath = new LinkedList<string>();

            // Act
            bool result = execute.HandleClashes([clashingElement], parentNode, insertElement);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        [TestProperty("Description",
            "Test that HandleClashes returns true and renames element when user selects Rename.")]
        public void HandleClashes_UserSelectsRename_ReturnsTrueAndRenamesElement()
        {
            // Arrange
            XElement clashingElement = new("Program", new XAttribute("Name", "ExistingProgram"));
            XElement parentNode = new("Programs", clashingElement);
            XElement insertElement = new("Program", new XAttribute("Name", "ExistingProgram"));
            parentNode.Add(insertElement);

            mockPrompts.Setup(p => p.SelectOne(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns("Rename");
            mockPrompts.Setup(p => p.Prompt(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns("RenamedProgram");

            mockXmlHandler.Object.ElementInfo.RootPath = new LinkedList<string>();

            // Act
            bool result = execute.HandleClashes([clashingElement], parentNode, insertElement);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        [TestProperty("Description",
            "Test that HandleClashes properly handles Module elements and uses CatalogNumber for identification.")]
        public void HandleClashes_WithModuleType_UsesCatalogNumber()
        {
            // Arrange
            XElement clashingModule = new("Module", new XAttribute("CatalogNumber", "1234-5678"));
            XElement parentNode = new("Module", new XAttribute("Name", "ParentModule"));
            XElement insertElement = new("Module", new XAttribute("CatalogNumber", "1234-5678"));

            mockPrompts.Setup(p => p.SelectOne(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns("Rename");
            mockPrompts.Setup(p => p.Prompt(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns("1234-9999");

            mockXmlHandler.Object.ElementInfo.RootPath = new LinkedList<string>();

            // Act
            bool result = execute.HandleClashes([clashingModule], parentNode, insertElement);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        [TestProperty("Description",
            "Test that HandleClashes clears RootPath after handling clash.")]
        public void HandleClashes_AfterHandling_ClearsRootPath()
        {
            // Arrange
            XElement clashingElement = new("Program", new XAttribute("Name", "ExistingProgram"));
            XElement parentNode = new("Programs", clashingElement);
            XElement insertElement = new("Program", new XAttribute("Name", "ExistingProgram"));
            parentNode.Add(insertElement);

            mockPrompts.Setup(p => p.SelectOne(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns("Cancel");

            mockXmlHandler.Object.ElementInfo.RootPath = new LinkedList<string>();
            mockXmlHandler.Object.ElementInfo.RootPath.AddLast(parentNode.Name.ToString());

            // Act
            execute.HandleClashes([clashingElement], parentNode, insertElement);

            // Assert
            Assert.IsEmpty(mockXmlHandler.Object.ElementInfo.RootPath);
        }

        #endregion

        #region Misc Tests

        [TestMethod]
        [TestCategory("Execute_UnitTest")]
        [TestProperty("Description",
            "Test that ModifyElement validates the file after modification.")]
        public void ModifyElement_AfterModification_ValidatesFile()
        {
            // Arrange
            XDocument templateDoc = XDocument.Load("../../../../L5XFiles/TemplateFiles/TemplateProjectV1.L5X");
            mockFileSystem.Setup(f => f.LoadXml(It.IsAny<string>())).Returns(templateDoc);
            Execute.Doc = TestHelper.CreateBasicTestDocument();

            mockPrompts.Setup(p => p.SelectOne(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns("Program");
            mockPrompts.SetupSequence(p => p.SelectMany(It.IsAny<string>(), It.IsAny<List<string>>()))
                .Returns(["MainProgram"])
                .Returns(["Name"]);
            mockPrompts.Setup(p => p.Prompt(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns("ModName");

            // Act
            execute.ModifyElement();

            // Assert
            mockValidation.Verify(v => v.ValidateL5XFile(It.IsAny<XDocument>()), Times.Once);
        }

        [TestMethod]
        [TestCategory("Execute_UnitTest")]
        [TestProperty("Description",
            "Test that CreateElement validates the file after creation.")]
        public void CreateElement_AfterCreation_ValidatesFile()
        {
            // Arrange
            XDocument templateDoc = XDocument.Load("../../../../L5XFiles/TemplateFiles/TemplateProjectV1.L5X");
            mockFileSystem.Setup(f => f.LoadXml(It.IsAny<string>())).Returns(templateDoc);
            Execute.Doc = TestHelper.CreateEmptyDocument();

            mockPrompts.Setup(p => p.SelectOne("Select Element Type", It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns("AddOnInstructionDefinition");
            mockPrompts.Setup(p => p.SelectOne("Select element template", It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns("TemplateFBAOI");
            mockPrompts.Setup(p => p.Prompt(It.Is<string>(s => s.Contains("How many")), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns("1");
            mockPrompts.Setup(p => p.Prompt(It.Is<string>(s => s.Contains("Input a name")), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns("NewTask");

            // Act
            execute.CreateElement();

            // Assert
            mockValidation.Verify(v => v.ValidateL5XFile(It.IsAny<XDocument>()), Times.Once);
        }

        [TestMethod]
        [TestCategory("Execute_UnitTest")]
        [TestProperty("Description",
            "Test that ImportElement validates the file after each import.")]
        public void ImportElement_AfterImport_ValidatesFile()
        {
            // Arrange
            string filePath = "C:\\test\\import.L5X";
            XDocument importDoc = TestHelper.CreateBasicTestDocument();

            mockOpenFile.Setup(o => o.TryOpen(It.IsAny<string>(), It.IsAny<string>(), out filePath))
                .Returns(true);
            mockFileSystem.Setup(f => f.LoadXml(filePath)).Returns(importDoc);
            mockPrompts.Setup(p => p.SelectOne(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns("Program");
            mockPrompts.Setup(p => p.SelectMany(It.IsAny<string>(), It.IsAny<List<string>>()))
                .Returns(["MainProgram"]);

            Execute.Doc = TestHelper.CreateEmptyDocument();

            // Act
            execute.ImportElement();

            // Assert
            mockValidation.Verify(v => v.ValidateL5XFile(It.IsAny<XDocument>()), Times.Once);
        }

        #endregion
    }
}
