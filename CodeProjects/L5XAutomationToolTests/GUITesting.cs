using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Tools;
using FlaUI.UIA2;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace L5XAutomationToolTests
{
    [TestClass]
    public class GUITesting
    {
        public Application app;
        private readonly string TestAppPath = typeof(L5XAutomationTool.Program).Assembly.Location;
        private readonly int longTimeoutMS = 5000;
        private readonly int shortTimeoutMS = 500;
        private UIA2Automation automation;
        private Window actionSelect;
        private TestHelper TestHelper;
        private readonly string TestXMLsPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "L5XFiles", "TestingFiles"));

        [TestInitialize]
        public void Startup()
        {
            automation = new UIA2Automation();
            TestHelper = new TestHelper();
            // Launch the application
            app = Application.Launch(TestAppPath);
            Thread.Sleep(longTimeoutMS);
            actionSelect = app.GetAllTopLevelWindows(automation).First();
            Assert.IsNotNull(actionSelect, "Find ActionSelect window");
        }

        [TestCleanup]
        public void Teardown()
        {
            app?.Kill();
        }

        [TestMethod]
        [TestCategory("File Manipulation")]
        [TestCategory("GUI Test")]
        [TestProperty("Description",
        "Test that the ActionSelect window launches with all necessary buttons, and that the exit button closes the window.")]
        public void ActionSelectLayout()
        {
            AutomationElement[] buttons = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Button));
            foreach (AutomationElement button in buttons)
            {
                // Verify that all buttons are onscreen
                Assert.IsFalse(button.IsOffscreen);
            }

            List<string> buttonNames = buttons.Select(but => but.Name).ToList();

            Assert.IsTrue(buttonNames.Any(), "Button names not found");

            List<string> expectedButtons = new List<string>() { "ImportElementButton", "CreateElementButton", "ModifyElementButton", "DeleteElementButton", "NewFileButton", "LoadFileButton", "ValidateFileButton", "SaveFileButton", "ExitActionSelect", "Minimize", "Maximize", "Close" };

            Assert.AreEqual(expectedButtons.Count, buttonNames.Count, "Incorrect quantity of buttons");

            // Verify that the button names found are the same as the names expected
            buttonNames.Sort();
            expectedButtons.Sort();
            bool buttonsCorrect = expectedButtons.SequenceEqual(buttonNames);
            Assert.IsTrue(buttonsCorrect, "Button names not as expected");

            // Press exit
            AutomationElement exitButton = actionSelect.FindAllDescendants(but => but.ByName("ExitActionSelect")).SingleOrDefault();
            exitButton.AsButton().Invoke();

            // Application should close upon pressing exit button
            Thread.Sleep(longTimeoutMS);
            Assert.IsTrue(app.GetAllTopLevelWindows(automation).Length == 0, "Application failed to close");

        }

        [TestMethod]
        [TestCategory("File Manipulation")]
        [TestProperty("Description",
        "Test that the validate button creates popups upon button press.")]
        public void FileManagementTesting()
        {
            // VALIDATE W/OUT ERROR
            Button validateButton = actionSelect.FindAllDescendants(val => val.ByName("ValidateFileButton")).SingleOrDefault()?.AsButton();

            // Validate blank doc to test errorless doc
            validateButton.Invoke();

            // Get the popup
            AutomationElement popup = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("Detected Errors"))).SingleOrDefault();
            Assert.IsNotNull(popup, "Popup not found");

            // Read no errors
            string noErrors = popup.FindAllDescendants(win => win.ByControlType(ControlType.Text)).SingleOrDefault()?.Name;
            Assert.IsTrue(noErrors.Equals("No errors detected!"), "Validation text box popup incorrect");

            // Press OK button to accept no errors
            Button ok = popup.FindAllDescendants(win => win.ByControlType(ControlType.Button).And(win.ByName("OK"))).SingleOrDefault()?.AsButton();
            Assert.IsNotNull(ok, "Ok button not found");
            ok.Invoke();

            // Should return to homepage
            Assert.IsTrue(actionSelect.IsAvailable, "Did not return to homepage");

            // LOAD FILE
            Button loadButton = actionSelect.FindAllDescendants(but => but.ByName("LoadFileButton")).SingleOrDefault()?.AsButton();
            Assert.IsNotNull(loadButton, "Load button not found");

            loadButton.Invoke();

            Window fileExplorer = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("Open"))).SingleOrDefault()?.AsWindow();
            Assert.IsNotNull(fileExplorer, "File explorer not found");

            AutomationElement filePathPane = fileExplorer.FindAllDescendants(win => win.ByName("Address ", PropertyConditionFlags.MatchSubstring)).SingleOrDefault();

            // Input path
            filePathPane.Click();
            FlaUI.Core.Input.Keyboard.Type(TestXMLsPath + "\n");

            // Select file to load
            AutomationElement[] files = fileExplorer.FindAllDescendants(win => win.ByControlType(ControlType.ListItem));

            AutomationElement templateBroken = files.SingleOrDefault(fil => fil.Name.Equals("BrokenXML.L5X"));
            Assert.IsNotNull(templateBroken, "Template file with error(BrokenXML.L5X) not found");

            // Select the broken template file as the file to load
            templateBroken.DoubleClick();

            // Should return to homepage
            Assert.IsTrue(actionSelect.IsAvailable, "Did not return to homepage");

            // Sleep for so the tool can catch up
            Thread.Sleep(100);

            // VALIDATE ERROR FILE
            // Validate blank doc to test errorless doc
            validateButton.Invoke();

            // Get the popup
            AutomationElement errors = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("Detected Errors"))).SingleOrDefault();
            Assert.IsNotNull(errors, "Popup not found");

            // Read no errors
            string errorsFound = errors.FindAllDescendants(win => win.ByControlType(ControlType.Text)).SingleOrDefault()?.Name;
            Assert.IsTrue(errorsFound.Contains("SCHEMA ERROR: "), "Validation text box popup incorrect, instead found: " + errorsFound);

            // Press OK button to accept no errors
            Button accept = errors.FindAllDescendants(win => win.ByControlType(ControlType.Button).And(win.ByName("OK"))).SingleOrDefault()?.AsButton();
            Assert.IsNotNull(accept, "Ok button not found");
            accept.Invoke();

            // Should return to homepage
            Assert.IsTrue(actionSelect.IsAvailable, "Did not return to homepage");

            // NEW FILE DENY CREATION
            Button newButtonNo = actionSelect.FindAllDescendants(win => win.ByName("NewFileButton")).SingleOrDefault()?.AsButton();
            newButtonNo.Invoke();

            // Verify that confirmation window appears
            Window verifyCreate = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("Verify File Creation"))).SingleOrDefault()?.AsWindow();
            Assert.IsNotNull(verifyCreate, "New file overwrite window not found");

            // Select No on overwriting existing
            Button selectNo = verifyCreate.FindAllDescendants(win => win.ByControlType(ControlType.Button).And(win.ByName("No"))).SingleOrDefault()?.AsButton();
            Assert.IsNotNull(selectNo, "Cancel new file creation button not found");
            selectNo.Invoke();

            // Should return to homepage
            Assert.IsTrue(actionSelect.IsAvailable, "Did not return to homepage");

            // SAVE FILE
            Button saveButton = actionSelect.FindAllDescendants(win => win.ByName("SaveFileButton")).SingleOrDefault()?.AsButton();
            saveButton.Invoke();

            Window fileExp = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("Save As"))).SingleOrDefault()?.AsWindow();
            Assert.IsNotNull(fileExp, "File explorer not found");

            filePathPane = fileExp.FindAllDescendants(win => win.ByControlType(ControlType.ToolBar).And(win.ByName("Address:", PropertyConditionFlags.MatchSubstring))).SingleOrDefault();

            // Input path
            filePathPane.Click();
            FlaUI.Core.Input.Keyboard.Type(TestXMLsPath + "\n");

            // Set file name
            AutomationElement fileName = fileExp.FindAllDescendants(win => win.ByControlType(ControlType.ComboBox).And(win.ByName("File name:"))).SingleOrDefault()?.AsTextBox();

            string nameToSave = "TestSaveResult";
            for (int i = 0; File.Exists(Path.Combine(TestXMLsPath, nameToSave + ".L5X")); i++)
                nameToSave = Regex.Replace(nameToSave, @"\d", string.Empty) + i.ToString();

            fileName.Click();
            FlaUI.Core.Input.Keyboard.Type(nameToSave + "\n");

            // Wait a so the tool can catch up
            Thread.Sleep(100);

            // NEW FILE CONFIRM CREATION
            Button newButtonYes = actionSelect.FindAllDescendants(win => win.ByName("NewFileButton")).SingleOrDefault()?.AsButton();
            newButtonYes.Invoke();

            // Verify that confirmation window appears
            Window confirmCreate = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("Verify File Creation"))).SingleOrDefault()?.AsWindow();
            Assert.IsNotNull(confirmCreate, "New file overwrite window not found");

            // Select No on overwriting existing
            Button selectYes = confirmCreate.FindAllDescendants(win => win.ByControlType(ControlType.Button).And(win.ByName("Yes"))).SingleOrDefault()?.AsButton();
            Assert.IsNotNull(selectYes, "Confirm new file creation button not found");
            selectYes.Invoke();

            // Should return to homepage
            Assert.IsTrue(actionSelect.IsAvailable, "Did not return to homepage");
        }

        [TestMethod]
        [TestCategory("GUI_Create")]
        [TestProperty("Description",
        "Test that the standard creation of multiple elements works.")]
        public void ElementQuantityCreation()
        {
            // CREATE
            Button createElementButton = actionSelect.FindAllDescendants(val => val.ByName("CreateElementButton")).SingleOrDefault()?.AsButton();

            // Begin element creation
            createElementButton.Invoke();
            Thread.Sleep(shortTimeoutMS);

            // Select element type to create
            Window elementTypeSelect = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByAutomationId("DropdownGui", PropertyConditionFlags.IgnoreCase))).SingleOrDefault()?.AsWindow();
            Assert.IsNotNull(elementTypeSelect, "Element type select window not found");

            AutomationElement selectedItem = elementTypeSelect.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("AddOnInstructionDefinition"))).SingleOrDefault();
            Assert.IsNotNull(selectedItem, "Desired element type not found in dropdown.");

            selectedItem.DoubleClick();

            // Select element template to use
            Window elementTemplate = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("DropdownGui"))).SingleOrDefault()?.AsWindow();
            Assert.IsNotNull(elementTemplate, "Element template select window not found");

            AutomationElement selectedTemplate = elementTemplate.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("TemplateFBAOI"))).SingleOrDefault();
            Assert.IsNotNull(selectedTemplate, "Desired element type not found in dropdown.");

            selectedTemplate.Click();

            Button confirm = elementTemplate.FindAllDescendants(win => win.ByControlType(ControlType.Button).And(win.ByName("Confirm"))).SingleOrDefault()?.AsButton();
            Assert.IsNotNull(confirm, "Confirm button not found in dropdown");

            confirm.Invoke();
            // Wait for next window to appear
            Thread.Sleep(shortTimeoutMS); 

            // Input quantity to create
            AutomationElement textInput = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("TextInput"))).SingleOrDefault();
            Assert.IsNotNull(textInput, "Text input window for quantity select not found.");

            AutomationElement textField = textInput.FindAllDescendants(win => win.ByControlType(ControlType.Edit).And(win.ByName("TextField"))).SingleOrDefault();
            Assert.IsNotNull(textField, "Text input field for quantity select not found.");

            int quantityToAdd = 2;
            textField.Click();
            textField.AsTextBox().Enter("");
            textField.AsTextBox().Enter(quantityToAdd.ToString());
            string foundInput = textField.AsTextBox().Text;
            string errorMsg = string.Concat("Input value not equal to expected, found: ", foundInput);

            Assert.IsTrue(foundInput.Equals(quantityToAdd.ToString()),  errorMsg);

            for(int i=0; i<quantityToAdd; i++)
            {
                Window nameInput = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("TextInput"))).SingleOrDefault()?.AsWindow();
                Assert.IsNotNull(nameInput, "Text input window for name not found.");

                // find the text input field
                TextBox nameField = nameInput.FindAllDescendants(win => win.ByControlType(ControlType.Edit).And(win.ByName("TextField"))).SingleOrDefault()?.AsTextBox();
                Assert.IsNotNull(nameField, "Text input field for name not found.");

                // test both submit and enter
                if(i%1==1)
                {
                    nameField.Click();
                    FlaUI.Core.Input.Keyboard.Type("testElement"+quantityToAdd.ToString() + "\n");
                    Assert.IsTrue(nameField.Text.Equals(quantityToAdd), "Input value not equal to expected");
                }
                else
                {
                    nameField.Click();
                    FlaUI.Core.Input.Keyboard.Type("testElement" + quantityToAdd.ToString());
                    Assert.IsTrue(nameField.Text.Equals(quantityToAdd), "Input value not equal to expected");

                    Button submit = elementTemplate.FindAllDescendants(win => win.ByControlType(ControlType.Button).And(win.ByName("Confirm"))).SingleOrDefault()?.AsButton();
                    Assert.IsNotNull(submit, "Confirm button not found in text input for name select");
                    submit.Invoke();
                }

            }

            // Should return to homepage
            Assert.IsTrue(actionSelect.IsAvailable, "Did not return to homepage");
        }

        [TestMethod]
        [TestCategory("File Manipulation")]
        [TestProperty("Description",
        "Test that action select cannot be manipulated during operations.")]
        public void ActionSelectLoseControl()
        {
            Assert.Inconclusive("Not Implemented");
        }

        [TestMethod]
        [TestCategory("GUI_Delete")]
        [TestProperty("Description",
        "Test that modules can be deleted using the port as a filter instead of name.")]
        public void DeleteModuleByPort()
        {
            Assert.Inconclusive("Not Implemented");
        }

        [TestMethod]
        [TestCategory("GUI_Delete")]
        [TestProperty("Description",
        "Test that deleting an element that exists in the file and has no abnormal behavior works.")]
        public void DeleteElementFound()
        {
            Assert.Inconclusive("Not Implemented");
        }

        [TestMethod]
        [TestCategory("GUI_Delete")]
        [TestProperty("Description",
        "Testing that attempting to delete an element when no element exists causes a popup.")]
        public void DeleteNoElement()
        {
            Assert.Inconclusive("Not Implemented");
        }

    }
}
