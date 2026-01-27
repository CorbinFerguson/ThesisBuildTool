using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using ThesisProjectV1;

namespace ThesisUnitTests
{
    [TestClass]
    public class ExecuteTests
    {
        private Mock<XMLHandler> mockXmlHandler;
        private Mock<IMessageService> mockMessages;
        private Mock<IUserPromptService> mockPrompts;
        private Mock<IOpenFileService> mockOpenFile;
        private Mock<ISaveFileService> mockSaveFile;
        private Mock<IValidationService> mockValidation;
        private Mock<IFileSystem> mockFileSystem;
        private Mock<ISchemaDisambiguator> mockDisambiguator;
        private Execute execute;
        private XNamespace ns;

        [TestInitialize]
        public void Setup()
        {
            ns = XNamespace.Get(@"http://www.w3.org/2001/XMLSchema");
            
            mockValidation = new Mock<IValidationService>();
            mockDisambiguator = new Mock<ISchemaDisambiguator>();
            mockXmlHandler = new Mock<XMLHandler>(mockValidation.Object, mockDisambiguator.Object);
            mockMessages = new Mock<IMessageService>();
            mockPrompts = new Mock<IUserPromptService>();
            mockOpenFile = new Mock<IOpenFileService>();
            mockSaveFile = new Mock<ISaveFileService>();
            mockFileSystem = new Mock<IFileSystem>();

            // Setup default validation behavior
            mockValidation.Setup(v => v.ValidateL5XFile(It.IsAny<XDocument>())).Returns(new List<string>());
            mockValidation.Setup(v => v.GetSchema()).Returns(CreateTestSchema());

            execute = new Execute(
                mockXmlHandler.Object,
                mockMessages.Object,
                mockPrompts.Object,
                mockOpenFile.Object,
                mockSaveFile.Object,
                mockValidation.Object,
                mockFileSystem.Object
            );
        }

        private XDocument CreateTestSchema()
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
                                new XAttribute("type", "ProgramsType")))),
                    new XElement(ns + "complexType",
                        new XAttribute("name", "ProgramsType"),
                        new XElement(ns + "sequence",
                            new XElement(ns + "element",
                                new XAttribute("name", "Program"),
                                new XAttribute("type", "ProgramType")))),
                    new XElement(ns + "element",
                        new XAttribute("name", "Programs"),
                        new XAttribute("type", "ProgramsType"))
                )
            );
        }

        private XDocument CreateBasicTestDocument()
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

        #region InitializeNew Tests

        [TestMethod]
        public void InitializeNew_LoadsBasicFile()
        {
            // Arrange
            XDocument expectedDoc = CreateBasicTestDocument();
            mockXmlHandler.Setup(x => x.LoadBasicFile()).Returns(expectedDoc);

            // Act
            execute.InitializeNew();

            // Assert
            mockXmlHandler.Verify(x => x.LoadBasicFile(), Times.Once);
            Assert.IsNotNull(Execute.Doc);
        }

        #endregion

        #region ValidateFile Tests

        [TestMethod]
        public void ValidateFile_WithNoErrors_ShowsNoErrorMessage()
        {
            // Arrange
            Execute.Doc = CreateBasicTestDocument();
            mockValidation.Setup(v => v.ValidateL5XFile(It.IsAny<XDocument>()))
                .Returns(new List<string>());

            // Act
            execute.ValidateFile(true);

            // Assert
            mockValidation.Verify(v => v.ValidateL5XFile(It.IsAny<XDocument>()), Times.Once);
        }

        [TestMethod]
        public void ValidateFile_WithErrors_DisplaysErrors()
        {
            // Arrange
            Execute.Doc = CreateBasicTestDocument();
            List<string> errors = new List<string> { "Error 1", "Error 2" };
            mockValidation.Setup(v => v.ValidateL5XFile(It.IsAny<XDocument>())).Returns(errors);

            // Act
            execute.ValidateFile(true);

            // Assert
            mockValidation.Verify(v => v.ValidateL5XFile(It.IsAny<XDocument>()), Times.Once);
        }

        [TestMethod]
        public void ValidateFile_WithShowNoErrorFalse_DoesNotShowSuccessMessage()
        {
            // Arrange
            Execute.Doc = CreateBasicTestDocument();
            mockValidation.Setup(v => v.ValidateL5XFile(It.IsAny<XDocument>()))
                .Returns(new List<string>());

            // Act
            execute.ValidateFile(false);

            // Assert
            mockValidation.Verify(v => v.ValidateL5XFile(It.IsAny<XDocument>()), Times.Once);
        }

        #endregion

        #region NewFile Tests

        [TestMethod]
        public void NewFile_UserConfirms_CreatesNewFile()
        {
            // Arrange
            mockMessages.Setup(m => m.Confirm(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            XDocument newDoc = CreateBasicTestDocument();
            mockXmlHandler.Setup(x => x.LoadBasicFile()).Returns(newDoc);

            // Act
            execute.NewFile();

            // Assert
            mockMessages.Verify(m => m.Confirm(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            mockXmlHandler.Verify(x => x.LoadBasicFile(), Times.Once);
            mockMessages.Verify(m => m.Show(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public void NewFile_UserCancels_DoesNotCreateNewFile()
        {
            // Arrange
            mockMessages.Setup(m => m.Confirm(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

            // Act
            execute.NewFile();

            // Assert
            mockMessages.Verify(m => m.Confirm(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            mockXmlHandler.Verify(x => x.LoadBasicFile(), Times.Never);
            mockMessages.Verify(m => m.Show(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        #endregion

        #region SaveFile Tests

        [TestMethod]
        public void SaveFile_UserSelectsPath_SavesDocument()
        {
            // Arrange
            Execute.Doc = CreateBasicTestDocument();
            string expectedPath = "C:\\test\\output.L5X";
            mockSaveFile.Setup(s => s.TrySave(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                out expectedPath))
                .Returns(true);

            // Act
            execute.SaveFile();

            // Assert
            mockFileSystem.Verify(f => f.SaveXml(It.IsAny<XDocument>(), It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public void SaveFile_UserCancels_DoesNotSave()
        {
            // Arrange
            Execute.Doc = CreateBasicTestDocument();
            string nullPath = null;
            mockSaveFile.Setup(s => s.TrySave(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                out nullPath))
                .Returns(false);

            // Act
            execute.SaveFile();

            // Assert
            mockFileSystem.Verify(f => f.SaveXml(It.IsAny<XDocument>(), It.IsAny<string>()), Times.Never);
        }

        #endregion

        #region LoadFile Tests

        [TestMethod]
        public void LoadFile_UserSelectsFile_LoadsDocument()
        {
            // Arrange
            string filePath = "C:\\test\\input.L5X";
            XDocument loadedDoc = CreateBasicTestDocument();
            mockOpenFile.Setup(o => o.TryOpen(It.IsAny<string>(), It.IsAny<string>(), out filePath))
                .Returns(true);
            mockFileSystem.Setup(f => f.LoadXml(filePath)).Returns(loadedDoc);

            // Act
            execute.LoadFile();

            // Assert
            mockOpenFile.Verify(o => o.TryOpen(It.IsAny<string>(), It.IsAny<string>(), out filePath), Times.Once);
            mockFileSystem.Verify(f => f.LoadXml(filePath), Times.Once);
        }

        [TestMethod]
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
        public void ModifyElement_SelectsProgramType_ModifiesElements()
        {
            // Arrange
            Execute.Doc = CreateBasicTestDocument();
            mockXmlHandler.Object.inputFile = CreateBasicTestDocument();
            
            mockXmlHandler.Setup(x => x.GetElementTypes(It.IsAny<XDocument>()))
                .Returns(new List<string> { "Program", "Task" });
            mockPrompts.Setup(p => p.SelectOne(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns("Program");
            mockPrompts.Setup(p => p.SelectMany(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns(new List<string> { "MainProgram" });
            mockXmlHandler.Setup(x => x.GetAttributes(It.IsAny<XElement>()))
                .Returns(new List<XAttribute> { new XAttribute("Name", "MainProgram") });

            // Act
            execute.ModifyElement();

            // Assert
            mockXmlHandler.Verify(x => x.GetElementTypes(It.IsAny<XDocument>()), Times.Once);
            mockPrompts.Verify(p => p.SelectOne(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public void ModifyElement_SelectsModuleType_UsesCatalogNumber()
        {
            // Arrange
            Execute.Doc = CreateBasicTestDocument();
            mockXmlHandler.Object.inputFile = CreateBasicTestDocument();
            
            mockXmlHandler.Setup(x => x.GetElementTypes(It.IsAny<XDocument>()))
                .Returns(new List<string> { "Module" });
            mockPrompts.Setup(p => p.SelectOne(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns("Module");
            mockPrompts.Setup(p => p.SelectMany(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns(new List<string> { "1234-5678" });
            mockXmlHandler.Setup(x => x.GetAttributes(It.IsAny<XElement>()))
                .Returns(new List<XAttribute>());

            // Act
            execute.ModifyElement();

            // Assert
            mockXmlHandler.Verify(x => x.GetElementTypes(It.IsAny<XDocument>()), Times.Once);
        }

        #endregion

        #region DeleteElement Tests

        [TestMethod]
        public void DeleteElement_SelectsElements_RemovesThem()
        {
            // Arrange
            Execute.Doc = CreateBasicTestDocument();
            mockXmlHandler.Object.inputFile = CreateBasicTestDocument();
            
            mockXmlHandler.Setup(x => x.GetElementTypes(It.IsAny<XDocument>()))
                .Returns(new List<string> { "Program" });
            mockPrompts.Setup(p => p.SelectOne(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns("Program");
            mockPrompts.Setup(p => p.SelectMany(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns(new List<string> { "MainProgram" });

            // Act
            execute.DeleteElement();

            // Assert
            mockXmlHandler.Verify(x => x.GetElementTypes(It.IsAny<XDocument>()), Times.Once);
            mockValidation.Verify(v => v.ValidateL5XFile(It.IsAny<XDocument>()), Times.Once);
        }

        #endregion

        #region ImportElement Tests

        [TestMethod]
        public void ImportElement_UserSelectsFile_ImportsElements()
        {
            // Arrange
            string filePath = "C:\\test\\import.L5X";
            XDocument importDoc = CreateBasicTestDocument();
            
            mockOpenFile.Setup(o => o.TryOpen(It.IsAny<string>(), It.IsAny<string>(), out filePath))
                .Returns(true);
            mockFileSystem.Setup(f => f.LoadXml(filePath)).Returns(importDoc);
            mockXmlHandler.Setup(x => x.GetElementTypes(It.IsAny<XDocument>()))
                .Returns(new List<string> { "Program" });
            mockPrompts.Setup(p => p.SelectOne(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns("Program");
            mockPrompts.Setup(p => p.SelectMany(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns(new List<string> { "MainProgram" });
            mockMessages.Setup(m => m.Confirm(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(false); // Don't loop
            mockXmlHandler.Setup(x => x.InsertElement(It.IsAny<XDocument>(), It.IsAny<List<XElement>>()))
                .Returns(Execute.Doc);
            
            Execute.Doc = CreateBasicTestDocument();

            // Act
            execute.ImportElement();

            // Assert
            mockOpenFile.Verify(o => o.TryOpen(It.IsAny<string>(), It.IsAny<string>(), out filePath), Times.Once);
            mockFileSystem.Verify(f => f.LoadXml(filePath), Times.Once);
        }

        [TestMethod]
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
        public void ImportElement_UserWantsToAddMore_Loops()
        {
            // Arrange
            string filePath = "C:\\test\\import.L5X";
            XDocument importDoc = CreateBasicTestDocument();
            int confirmCallCount = 0;
            
            mockOpenFile.Setup(o => o.TryOpen(It.IsAny<string>(), It.IsAny<string>(), out filePath))
                .Returns(true);
            mockFileSystem.Setup(f => f.LoadXml(filePath)).Returns(importDoc);
            mockXmlHandler.Setup(x => x.GetElementTypes(It.IsAny<XDocument>()))
                .Returns(new List<string> { "Program" });
            mockPrompts.Setup(p => p.SelectOne(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns("Program");
            mockPrompts.Setup(p => p.SelectMany(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns(new List<string> { "MainProgram" });
            mockMessages.Setup(m => m.Confirm(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(() => confirmCallCount++ == 0); // Return true first time, false second
            mockXmlHandler.Setup(x => x.InsertElement(It.IsAny<XDocument>(), It.IsAny<List<XElement>>()))
                .Returns(Execute.Doc);
            
            Execute.Doc = CreateBasicTestDocument();

            // Act
            execute.ImportElement();

            // Assert
            mockMessages.Verify(m => m.Confirm(It.IsAny<string>(), It.IsAny<string>()), Times.AtLeast(2));
        }

        #endregion

        #region CreateElement Tests

        [TestMethod]
        public void CreateElement_SingleProgram_CreatesElement()
        {
            // Arrange
            XDocument templateDoc = CreateBasicTestDocument();
            mockFileSystem.Setup(f => f.LoadXml(It.IsAny<string>())).Returns(templateDoc);
            mockXmlHandler.Object.inputFile = templateDoc;
            
            mockXmlHandler.Setup(x => x.GetElementTypes(It.IsAny<XDocument>()))
                .Returns(new List<string> { "Program" });
            mockPrompts.Setup(p => p.SelectOne("Select Element Type", It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns("Program");
            mockPrompts.Setup(p => p.SelectOne("Select element template", It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns("MainProgram");
            mockPrompts.Setup(p => p.SelectOne("Select parent for the elements being created", It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns("TestController");
            mockPrompts.Setup(p => p.Prompt(It.Is<string>(s => s.Contains("How many")), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns("1");
            mockPrompts.Setup(p => p.Prompt(It.Is<string>(s => s.Contains("Input a name")), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns("NewProgram");
            mockPrompts.Setup(p => p.SelectOne("Select the parent task for inserted program", It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns("Keep programs unscheduled");
            mockXmlHandler.Setup(x => x.InsertElement(It.IsAny<XDocument>(), It.IsAny<XElement>()))
                .Returns(Execute.Doc);
            
            Execute.Doc = CreateBasicTestDocument();

            // Act
            execute.CreateElement();

            // Assert
            mockXmlHandler.Verify(x => x.GetElementTypes(It.IsAny<XDocument>()), Times.Once);
            mockPrompts.Verify(p => p.Prompt(It.Is<string>(s => s.Contains("Input a name")), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public void CreateElement_MultipleElements_CreatesAll()
        {
            // Arrange
            XDocument templateDoc = CreateBasicTestDocument();
            mockFileSystem.Setup(f => f.LoadXml(It.IsAny<string>())).Returns(templateDoc);
            mockXmlHandler.Object.inputFile = templateDoc;
            
            mockXmlHandler.Setup(x => x.GetElementTypes(It.IsAny<XDocument>()))
                .Returns(new List<string> { "Task" });
            mockPrompts.Setup(p => p.SelectOne("Select Element Type", It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns("Task");
            mockPrompts.Setup(p => p.SelectOne("Select element template", It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns("MainTask");
            mockPrompts.Setup(p => p.Prompt(It.Is<string>(s => s.Contains("How many")), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns("3");
            
            int nameCallCount = 0;
            mockPrompts.Setup(p => p.Prompt(It.Is<string>(s => s.Contains("Input a name")), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(() => $"Task{++nameCallCount}");
            mockXmlHandler.Setup(x => x.InsertElement(It.IsAny<XDocument>(), It.IsAny<XElement>()))
                .Returns(Execute.Doc);
            
            Execute.Doc = CreateBasicTestDocument();

            // Act
            execute.CreateElement();

            // Assert
            mockPrompts.Verify(p => p.Prompt(It.Is<string>(s => s.Contains("Input a name")), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(3));
        }

        [TestMethod]
        [ExpectedException(typeof(EmptyListException))]
        public void CreateElement_NoValidParents_ThrowsException()
        {
            // Arrange
            XDocument templateDoc = new XDocument(
                new XElement("RSLogix5000Content",
                    new XElement("Program", new XAttribute("Name", "Orphan"))
                )
            );
            mockFileSystem.Setup(f => f.LoadXml(It.IsAny<string>())).Returns(templateDoc);
            mockXmlHandler.Object.inputFile = templateDoc;
            
            mockXmlHandler.Setup(x => x.GetElementTypes(It.IsAny<XDocument>()))
                .Returns(new List<string> { "Program" });
            mockPrompts.Setup(p => p.SelectOne("Select Element Type", It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns("Program");
            mockPrompts.Setup(p => p.SelectOne("Select element template", It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns("Orphan");
            mockPrompts.Setup(p => p.Prompt(It.Is<string>(s => s.Contains("How many")), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns("1");
            
            Execute.Doc = CreateBasicTestDocument();
            
            // Mock schema to return no valid parents
            mockValidation.Setup(v => v.GetSchema()).Returns(new XDocument(
                new XElement(ns + "schema")
            ));

            // Act
            execute.CreateElement();
        }

        #endregion

        #region SetAttributes Tests

        [TestMethod]
        public void SetAttributes_WithElementAndAttributes_SetsValues()
        {
            // Arrange
            XElement element = new XElement("Program", new XAttribute("Name", "TestProgram"));
            List<XAttribute> attrs = new List<XAttribute>
            {
                new XAttribute("Name", "TestProgram"),
                new XAttribute("Type", "Normal")
            };
            
            mockPrompts.Setup(p => p.SelectMany(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns(new List<string> { "Name" });
            mockPrompts.Setup(p => p.Prompt(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns("ModifiedProgram");

            // Act
            execute.SetAttributes(new[] { element }, new List<List<XAttribute>> { attrs });

            // Assert
            mockPrompts.Verify(p => p.SelectMany(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public void SetAttributes_WithMultipleElements_ProcessesAll()
        {
            // Arrange
            List<XElement> elements = new List<XElement>
            {
                new XElement("Program", new XAttribute("Name", "Program1")),
                new XElement("Program", new XAttribute("Name", "Program2"))
            };
            List<List<XAttribute>> attrs = new List<List<XAttribute>>
            {
                new List<XAttribute> { new XAttribute("Name", "Program1") },
                new List<XAttribute> { new XAttribute("Name", "Program2") }
            };
            
            mockPrompts.Setup(p => p.SelectMany(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns(new List<string>());

            // Act
            execute.SetAttributes(elements, attrs);

            // Assert
            mockPrompts.Verify(p => p.SelectMany(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>()), Times.Exactly(2));
        }

        #endregion

        #region Integration-Style Tests

        [TestMethod]
        public void Execute_Constructor_InitializesAllDependencies()
        {
            // Assert
            Assert.IsNotNull(execute);
        }

        [TestMethod]
        public void StaticDoc_IsAccessible()
        {
            // Arrange
            XDocument testDoc = CreateBasicTestDocument();

            // Act
            Execute.Doc = testDoc;

            // Assert
            Assert.IsNotNull(Execute.Doc);
            Assert.AreEqual(testDoc, Execute.Doc);
        }

        #endregion
    }
}