using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA2;
using L5XAutomationToolTestHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reporting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace GuiTesting
{
    [STATestClass]
    public class GUITesting
    {
        public Application app;
        public static Window actionSelect;
        public TestContext TestContext { get; set; }

        private readonly string TestAppPath = Path.ChangeExtension(typeof(L5XAutomationTool.Program).Assembly.Location, ".exe");
        private readonly int longTimeoutMS = 2000;
        private readonly int shortTimeoutMS = 550;
        private UIA2Automation automation;
        private readonly string TestXMLsPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "L5XFiles", "TestingFiles"));

        [TestInitialize]
        public void Startup()
        {
            // Terminate any running instances of the application before starting a new one
            Process[] running = Process.GetProcessesByName("L5XAutomationTool");
            foreach (Process proc in running)
            {
                proc.Kill();
            }
            // Initialize automation and launch the application
            automation = new UIA2Automation();

            // Start test reporting for the current test
            TestReport.Start(TestContext.TestName);

            app = Application.Launch(TestAppPath);
            TestReport.IsNotNull(app, "Launch application");
            TestHelper.WaitMilliseconds(longTimeoutMS);


            // Get the main window of the application
            actionSelect = app.GetAllTopLevelWindows(automation).Single(win => win.Name.Equals("ActionSelector"));
            TestReport.IsNotNull(actionSelect, "Find ActionSelect window");
        }

        [TestCleanup]
        public void Teardown()
        {
            // Close the application and end test reporting
            app?.Close();
            TestReport.End();
        }

        [TestMethod]
        [TestCategory("File Manipulation")]
        [TestCategory("GUI_Test")]
        [TestProperty("Description",
        "Test that the ActionSelect window launches with all necessary buttons, and that the exit button closes the window.")]
        public void ActionSelectLayout()
        {
            // Find all button elements in the ActionSelect window
            AutomationElement[] buttons = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Button));

            TestReport.Section("Verify all buttons are onscreen");

            // Check that each button is visible
            foreach (AutomationElement button in buttons)
            {
                TestReport.IsFalse(button.IsOffscreen, "Button is onscreen");
            }

            TestReport.Section("Verify that all buttons are there");
            // Collect button names for comparison
            List<string> buttonNames = [.. buttons.Select(but => but.Name)];

            TestReport.IsTrue(buttonNames.Count != 0, "Collect button names");

            // List of expected button names
            List<string> expectedButtons = ["ImportElementButton", "CreateElementButton", "ModifyElementButton", "DeleteElementButton", "NewFileButton", "LoadFileButton", "ValidateFileButton", "SaveFileButton", "ExitActionSelect", "Minimize", "Maximize", "Close"];

            // Check that the number of buttons matches expectation
            TestReport.IsTrue(expectedButtons.Count == buttonNames.Count, "Expected quantity of buttons");

            // Verify that the actual button names match the expected names
            buttonNames.Sort();
            expectedButtons.Sort();
            bool buttonsCorrect = expectedButtons.SequenceEqual(buttonNames);
            TestReport.IsTrue(buttonsCorrect, "Button names match expected");

            TestReport.Section("Exit");
            // Find and click the exit button to close the window
            AutomationElement exitButton = actionSelect.FindAllDescendants(but => but.ByName("ExitActionSelect")).SingleOrDefault();
            exitButton.AsButton().Click();

            // Wait and verify that the application has closed
            TestHelper.WaitMilliseconds(longTimeoutMS);
            TestReport.IsTrue(app.GetAllTopLevelWindows(automation).Length == 0, "Application closes");
        }

        [TestMethod]
        [TestCategory("File Manipulation")]
        [TestCategory("GUI_Test")]
        [TestProperty("Description",
        "Test that action select cannot be manipulated during operations.")]
        public void ActionSelectLoseControl()
        {
            // List of buttons that should not be tested for control loss
            List<string> prohibNames = ["ExitActionSelect", "Minimize", "Maximize", "Close"];
            // Get all main menu buttons except prohibited ones
            IEnumerable<AutomationElement> mainMenuButtons = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Button)).Where(but => !prohibNames.Contains(but.Name));

            foreach (AutomationElement button in mainMenuButtons)
            {
                TestReport.Section(button.Name);
                TestHelper.WaitMilliseconds(shortTimeoutMS);
                // Ensure ActionSelect is enabled before operation
                TestReport.IsTrue(actionSelect.IsEnabled, "Action select found");

                // click button to open a new window
                button.AsButton().Click();

                TestHelper.WaitMilliseconds((int)(shortTimeoutMS * 1.5));

                // Find the popup window that appears
                Window popup = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Window)).SingleOrDefault()?.AsWindow();
                TestReport.IsNotNull(popup, "Open popup window for: " + button?.Name ?? "button is null");

                // ActionSelect should be disabled while popup is open
                TestReport.IsFalse(actionSelect.IsEnabled, "Action select yields control");

                // Close the popup window using appropriate button
                if (button.Name.Equals("NewFileButton"))
                {
                    Button no = popup.FindAllDescendants(win => win.ByName("No")).SingleOrDefault().AsButton();
                    TestReport.IsNotNull(no, "Select 'No' to cancel new file");
                    no.Click();
                }
                else
                {
                    Button close = popup.FindAllDescendants(win => win.ByName("Close")).SingleOrDefault().AsButton();
                    TestReport.IsNotNull(close, "Press close button");
                    close.Click();
                }

            }
        }

        [TestMethod]
        [TestCategory("File Manipulation")]
        [TestCategory("GUI_Test")]
        [TestProperty("Description",
        "Test that the validate button creates popups upon button press.")]
        public void FileManagementTesting()
        {
            TestReport.Section("VALIDATE W/OUT ERROR");
            // Find and click the validate button
            Button validateButton = actionSelect.FindAllDescendants(val => val.ByName("ValidateFileButton")).SingleOrDefault()?.AsButton();

            // Validate blank document to test errorless scenario
            validateButton.Click();

            // Find the popup for detected errors
            AutomationElement popup = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("Detected Errors"))).SingleOrDefault();
            TestReport.IsNotNull(popup, "Open 'Detected Errors' popup");

            // Check that the popup text indicates no errors
            string noErrors = popup.FindAllDescendants(win => win.ByControlType(ControlType.Text)).SingleOrDefault()?.Name;
            TestReport.IsTrue(noErrors.Equals("No errors detected!"), "Show 'No errors detected!' message");

            // Find and click OK button to close popup
            Button ok = popup.FindAllDescendants(win => win.ByControlType(ControlType.Button).And(win.ByName("OK"))).SingleOrDefault()?.AsButton();
            TestReport.IsNotNull(ok, "Press OK");
            ok.Click();

            TestHelper.CheckHomePage();

            TestReport.Section("LOAD FILE");
            // Find and click the load button
            Button loadButton = actionSelect.FindAllDescendants(but => but.ByName("LoadFileButton")).SingleOrDefault()?.AsButton();
            TestReport.IsNotNull(loadButton, "Press Load");

            loadButton.Click();
            TestHelper.WaitMilliseconds(shortTimeoutMS * 2);

            // Find file explorer window
            Window fileExplorer = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("Open"))).SingleOrDefault()?.AsWindow();
            TestReport.IsNotNull(fileExplorer, "Open File Explorer");

            // Find address bar and input file path
            AutomationElement filePathPane = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.ToolBar).And(win.ByName("Address:", PropertyConditionFlags.MatchSubstring))).SingleOrDefault();
            filePathPane.FindAllDescendants(win => win.ByName("All locations")).SingleOrDefault().AsButton().Click();
            AutomationElement filePathEdit = fileExplorer.FindAllDescendants(win => win.ByControlType(ControlType.Edit).And(win.ByName("Address", PropertyConditionFlags.MatchSubstring))).SingleOrDefault();
            filePathEdit.AsTextBox().Enter(TestXMLsPath);
            Keyboard.Press(VirtualKeyShort.ENTER);

            // Select file to load
            AutomationElement[] files = fileExplorer.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByFrameworkId(FrameworkType.Win32.ToString()).Not()));
            AutomationElement templateBroken = files.SingleOrDefault(fil => fil.Name.Equals("BrokenXML.L5X"))
                                       ?? files.SingleOrDefault(fil => fil.Name.Equals("BrokenXML"));
            TestReport.IsNotNull(templateBroken, "Select BrokenXML.L5X");

            // Double-click to load the broken template file
            templateBroken.DoubleClick();

            // Wait for application to catch up
            TestHelper.WaitMilliseconds(100);

            TestHelper.CheckHomePage();

            TestReport.Section("VALIDATE ERROR FILE");
            // Validate loaded file to test error scenario
            validateButton.Click();

            // Find error popup
            AutomationElement errors = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("Detected Errors"))).SingleOrDefault();
            TestReport.IsNotNull(errors, "Open 'Detected Errors' popup");

            // Check that error message contains schema error
            string errorsFound = errors.FindAllDescendants(win => win.ByControlType(ControlType.Text)).SingleOrDefault()?.Name;
            TestReport.IsTrue(errorsFound.Contains("SCHEMA ERROR: "), "Show 'No errors detected!' message, instead found: " + errorsFound);

            // Find and click OK button to close error popup
            Button accept = errors.FindAllDescendants(win => win.ByControlType(ControlType.Button).And(win.ByName("OK"))).SingleOrDefault()?.AsButton();
            TestReport.IsNotNull(accept, "Press OK");
            accept.Click();

            TestHelper.CheckHomePage();

            TestReport.Section("NEW FILE DENY CREATION");
            // Find and click new file button
            Button newButtonNo = actionSelect.FindAllDescendants(win => win.ByName("NewFileButton")).SingleOrDefault()?.AsButton();
            newButtonNo.Click();

            // Verify confirmation window appears
            Window verifyCreate = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("Verify File Creation"))).SingleOrDefault()?.AsWindow();
            TestReport.IsNotNull(verifyCreate, "Open Verify File Creation window");

            // Select No to cancel new file creation
            Button selectNo = verifyCreate.FindAllDescendants(win => win.ByControlType(ControlType.Button).And(win.ByName("No"))).SingleOrDefault()?.AsButton();
            TestReport.IsNotNull(selectNo, "Press No to cancel");
            selectNo.Click();

            TestHelper.CheckHomePage();

            TestReport.Section("SAVE FILE");
            // Find and click save button
            Button saveButton = actionSelect.FindAllDescendants(win => win.ByName("SaveFileButton")).SingleOrDefault()?.AsButton();
            saveButton.Click();

            TestHelper.WaitMilliseconds(shortTimeoutMS);

            // Find Save As file explorer window
            Window fileExp = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("Save As"))).SingleOrDefault()?.AsWindow();
            TestReport.IsNotNull(fileExp, "Open File Explorer");

            // Find address bar and input save path
            filePathPane = fileExp.FindAllDescendants(win => win.ByControlType(ControlType.ToolBar).And(win.ByName("Address:", PropertyConditionFlags.MatchSubstring))).SingleOrDefault();
            filePathPane.FindAllDescendants(win => win.ByName("All locations")).SingleOrDefault().AsButton().Click();

            filePathEdit = fileExp.FindAllDescendants(win => win.ByControlType(ControlType.Edit).And(win.ByName("Address", PropertyConditionFlags.MatchSubstring))).SingleOrDefault();

            string saveResultPath = Path.GetFullPath(Path.Combine(TestXMLsPath, "TestResults"));
            filePathEdit.AsTextBox().Enter(saveResultPath);
            Keyboard.Press(VirtualKeyShort.ENTER);
            TestHelper.WaitMilliseconds(shortTimeoutMS);

            // Set file name for saving, ensure uniqueness
            AutomationElement fileName = fileExp.FindAllDescendants(win => win.ByControlType(ControlType.ComboBox).And(win.ByName("File name:"))).SingleOrDefault()?.AsTextBox();
            TestReport.IsNotNull(fileName, "Enter file name in Save As");

            string nameToSave = "TestSaveResult";
            for (int i = 0; File.Exists(Path.Combine(saveResultPath, nameToSave + ".L5X")); i++)
                nameToSave = Regex.Replace(nameToSave, @"\d", string.Empty) + i.ToString();

            fileName.Click();
            AutomationElement fileNameField = fileName.FindAllDescendants(win => win.ByName("File name:").And(win.ByControlType(ControlType.Edit))).SingleOrDefault();
            TestReport.IsNotNull(fileNameField, "Find file name field");
            fileNameField.AsTextBox().Enter(nameToSave);

            TestReport.IsTrue(fileNameField.AsTextBox().Text == nameToSave, "Text input as expected");

            Keyboard.Press(VirtualKeyShort.ENTER);

            TestHelper.WaitMilliseconds(shortTimeoutMS);

            TestHelper.CheckHomePage();

            // Verify file was saved
            TestReport.IsTrue(File.Exists(Path.Combine(saveResultPath, nameToSave + ".L5X")), "Verify file saved at location", false);

            TestReport.Section("NEW FILE CONFIRM CREATION");
            // Find and click new file button
            Button newButtonYes = actionSelect.FindAllDescendants(win => win.ByName("NewFileButton")).SingleOrDefault()?.AsButton();
            newButtonYes.Click();

            // Verify confirmation window appears
            Window confirmCreate = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("Verify File Creation"))).SingleOrDefault()?.AsWindow();
            TestReport.IsNotNull(confirmCreate, "Open Verify File Creation window");

            // Select Yes to confirm new file creation
            Button selectYes = confirmCreate.FindAllDescendants(win => win.ByControlType(ControlType.Button).And(win.ByName("Yes"))).SingleOrDefault()?.AsButton();
            TestReport.IsNotNull(selectYes, "Press Yes to create new file");
            selectYes.Click();

            TestHelper.WaitMilliseconds(shortTimeoutMS);

            // Click OK to close confirmation window
            Button buttonOK = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Button).And(win.ByName("OK"))).SingleOrDefault()?.AsButton();
            TestReport.IsNotNull(buttonOK, "Press OK to confirm creation");
            buttonOK.Click();

            TestHelper.CheckHomePage();
        }

        [TestMethod]
        [TestCategory("File Manipulation")]
        [TestCategory("GUI_Test")]
        [TestProperty("Description",
        "Test that when the user modifies the file in a way that is not valid for the schema, an error window appears.")]
        public void ValidationErrorWindow()
        {
            // CREATE
            // Find and click create element button
            Button createElementButton = actionSelect.FindAllDescendants(val => val.ByName("CreateElementButton")).SingleOrDefault()?.AsButton();
            createElementButton.Click();
            TestHelper.WaitMilliseconds(shortTimeoutMS);

            // Select element type to create
            Window elementTypeSelect = TestHelper.GetStandardInput(TestHelper.InputType.Dropdown);
            AutomationElement selectedItem = elementTypeSelect.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("Task"))).SingleOrDefault();
            TestReport.IsNotNull(selectedItem, "Select desired element type from dropdown");
            selectedItem.DoubleClick();

            // Select element template to use
            Window elementTemplate = TestHelper.GetStandardInput(TestHelper.InputType.Dropdown);
            ListBoxItem selectedTemplate = elementTemplate.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("TemplateContinuous"))).SingleOrDefault()?.AsListBoxItem();
            TestReport.IsNotNull(selectedTemplate, "Select desired element type from dropdown");
            selectedTemplate.DoubleClick();

            // Input quantity and names for elements to create
            int quantityToAdd = 2;
            TestHelper.TextboxSetValue(actionSelect, quantityToAdd.ToString());
            TestHelper.TextboxSetValue(actionSelect, "genTaskWithError");
            TestHelper.TextboxSetValue(actionSelect, "illegal2ndTask");

            // Find detected errors window and verify error message
            Window detectErrors = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("Detected Errors"))).SingleOrDefault().AsWindow();
            TestReport.IsNotNull(detectErrors, "Open Detected Errors window");

            TextBox errorsBox = detectErrors.FindAllDescendants(win => win.ByControlType(ControlType.Text).And(win.ByName("ERROR:", PropertyConditionFlags.MatchSubstring))).SingleOrDefault()?.AsTextBox();
            TestReport.IsNotNull(errorsBox, "Read error message");
            string errorMsg = errorsBox.Text;

            // Click OK to close error window
            Button ok = detectErrors.FindAllDescendants(win => win.ByControlType(ControlType.Button).And(win.ByName("OK"))).SingleOrDefault()?.AsButton();
            ok.Click();

            TestHelper.CheckHomePage();
        }

        [TestMethod]
        [TestCategory("GUI_Create")]
        [TestCategory("GUI_Test")]
        [TestProperty("Description",
        "Test that the creation of multiple elements that clash works.")]
        public void ClashingElementCreation()
        {
            // CREATE
            // Find and click create element button
            TestReport.Section("Find and click create element button");
            Button createElementButton = actionSelect.FindAllDescendants(val => val.ByName("CreateElementButton")).SingleOrDefault()?.AsButton();
            createElementButton.Click();
            TestHelper.WaitMilliseconds(shortTimeoutMS);

            // Select element type to create
            TestReport.Section("Select element type to create");
            Window elementTypeSelect = TestHelper.GetStandardInput(TestHelper.InputType.Dropdown);
            AutomationElement selectedItem = elementTypeSelect.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("AddOnInstructionDefinition"))).SingleOrDefault();
            TestReport.IsNotNull(selectedItem, "Select desired element type from dropdown");
            selectedItem.DoubleClick();

            // Select element template to use
            TestReport.Section("Select element template to use");
            Window elementTemplate = TestHelper.GetStandardInput(TestHelper.InputType.Dropdown);
            ListBoxItem selectedTemplate = elementTemplate.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("TemplateFBAOI"))).SingleOrDefault()?.AsListBoxItem();
            TestReport.IsNotNull(selectedTemplate, "Select desired element type from dropdown");
            selectedTemplate.DoubleClick();

            // Input quantity to create and handle clash resolution for each
            TestReport.Section("Input quantity to create and handle clash resolution for each");
            int quantityToAdd = 4;
            TestHelper.TextboxSetValue(actionSelect, quantityToAdd.ToString());

            for (int i = 0; i < quantityToAdd; i++)
            {
                TestHelper.TextboxSetValue(actionSelect, "TestSaveFile");

                if (i != 0)
                {
                    // Handle clash resolution for subsequent elements
                    Window resolutionStyle = TestHelper.GetStandardInput(TestHelper.InputType.Dropdown);

                    if (i == 1)
                    {
                        TestReport.Section("Resolution style: Replace");
                        // Resolution style: Replace
                        ListBoxItem replace = resolutionStyle.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("Replace"))).SingleOrDefault()?.AsListBoxItem();
                        TestReport.IsNotNull(replace, "Choose Replace in clash resolution");
                        replace.DoubleClick();
                    }
                    else if (i == 2)
                    {
                        TestReport.Section("Resolution style: Rename");
                        // Resolution style: Rename
                        ListBoxItem rename = resolutionStyle.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("Rename"))).SingleOrDefault()?.AsListBoxItem();
                        TestReport.IsNotNull(rename, "Choose Rename in clash resolution");
                        rename.DoubleClick();

                        TestHelper.TextboxSetValue(actionSelect, "TestSaveFile");
                        TestHelper.TextboxSetValue(actionSelect, "TestSaveFile2");
                    }
                    else if (i == 3)
                    {
                        TestReport.Section("Resolution style: Cancel");
                        // Resolution style: Cancel
                        ListBoxItem cancel = resolutionStyle.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("Cancel"))).SingleOrDefault()?.AsListBoxItem();
                        TestReport.IsNotNull(cancel, "Choose Cancel in clash resolution");
                        cancel.DoubleClick();
                    }
                }
            }

            TestHelper.CheckHomePage();
        }

        [TestMethod]
        [TestCategory("GUI_Create")]
        [TestCategory("GUI_Test")]
        [TestProperty("Description",
        "Test that the standard creation of multiple elements works.")]
        public void ElementQuantityCreation()
        {
            // CREATE
            // Find and click create element button
            Button createElementButton = actionSelect.FindAllDescendants(val => val.ByName("CreateElementButton")).SingleOrDefault()?.AsButton();
            createElementButton.Click();
            TestHelper.WaitMilliseconds(shortTimeoutMS);

            // Select element type to create
            Window elementTypeSelect = TestHelper.GetStandardInput(TestHelper.InputType.Dropdown);
            AutomationElement selectedItem = elementTypeSelect.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("AddOnInstructionDefinition"))).SingleOrDefault();
            TestReport.IsNotNull(selectedItem, "Select desired element type from dropdown");
            selectedItem.DoubleClick();

            // Select element template to use
            Window elementTemplate = TestHelper.GetStandardInput(TestHelper.InputType.Dropdown);
            AutomationElement selectedTemplate = elementTemplate.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("TemplateFBAOI"))).SingleOrDefault();
            TestReport.IsNotNull(selectedTemplate, "Select desired element type from dropdown");
            selectedTemplate.Click();

            TestHelper.Confirm(elementTemplate);

            TestHelper.WaitMilliseconds(longTimeoutMS);

            // Input quantity to create and handle name input for each element
            int quantityToAdd = 2;
            TestHelper.TextboxSetValue(actionSelect, quantityToAdd.ToString());

            for (int i = 0; i < quantityToAdd; i++)
            {
                TestHelper.WaitMilliseconds(shortTimeoutMS);
                Window nameInput = TestHelper.GetStandardInput(TestHelper.InputType.TextInput);

                // Find the text input field for element name
                TextBox nameField = nameInput.FindAllDescendants(win => win.ByControlType(ControlType.Edit).And(win.ByName("TextField"))).SingleOrDefault()?.AsTextBox();
                TestReport.IsNotNull(nameField, "Enter name in text input. Run: " + i);

                string foundInput;
                string errorMsg;

                // Test both submit and enter for name input
                if (i % 1 == 1)
                {
                    nameField.Click();
                    nameField.Enter("");
                    nameField.Enter("TestSaveFile" + i);

                    foundInput = nameField.Text;
                    errorMsg = string.Concat("Input name equals expected, found: ", foundInput);
                    TestReport.IsTrue(foundInput.Equals("TestSaveFile" + i), errorMsg);

                    nameField.Click();
                    FlaUI.Core.Input.Keyboard.Press(FlaUI.Core.WindowsAPI.VirtualKeyShort.RETURN);
                }
                else
                {
                    nameField.Click();
                    nameField.Enter("");
                    nameField.Enter("TestSaveFile" + i);

                    foundInput = nameField.Text;
                    errorMsg = string.Concat("Input name equals expected, found: ", foundInput);
                    TestReport.IsTrue(foundInput.Equals("TestSaveFile" + i), errorMsg);

                    TestHelper.Confirm(nameInput);
                }
            }

            TestHelper.CheckHomePage();
        }

        [TestMethod]
        [TestCategory("GUI_Create")]
        [TestCategory("GUI_Test")]
        [TestProperty("Description",
        "Test that the creation of modules with no name works.")]
        public void NamelessModuleCreation()
        {
            // CREATE
            // Find and click create element button
            Button createElementButton = actionSelect.FindAllDescendants(val => val.ByName("CreateElementButton")).SingleOrDefault()?.AsButton();
            createElementButton.Click();
            TestHelper.WaitMilliseconds(shortTimeoutMS);

            // Select element type to create
            Window elementTypeSelect = TestHelper.GetStandardInput(TestHelper.InputType.Dropdown);
            AutomationElement selectedItem = elementTypeSelect.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("Module"))).SingleOrDefault();
            TestReport.IsNotNull(selectedItem, "Select desired element type from dropdown");
            selectedItem.DoubleClick();

            // Select module template to use
            Window elementTemplate = TestHelper.GetStandardInput(TestHelper.InputType.Dropdown);
            AutomationElement selectedTemplate = elementTemplate.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("1756-IA16"))).SingleOrDefault();
            TestReport.IsNotNull(selectedTemplate, "Select desired element type from dropdown");
            selectedTemplate.DoubleClick();

            // Input quantity to create
            int quantityToAdd = 2;
            TestHelper.TextboxSetValue(actionSelect, quantityToAdd.ToString());

            TestHelper.CheckHomePage();
        }

        [TestMethod]
        [TestCategory("GUI_Delete")]
        [TestCategory("GUI_Test")]
        [TestProperty("Description",
        "Test that deleting an element that exists in the file and has no abnormal behavior works.")]
        public void DeleteElementFound()
        {
            // Load default file for deletion test
            TestHelper.LoadDefault();

            // Find and click delete element button
            Button deleteButton = actionSelect.FindAllDescendants(val => val.ByName("DeleteElementButton")).SingleOrDefault()?.AsButton();
            deleteButton.Click();

            // Select element type to delete
            Window dropdown = TestHelper.GetStandardInput(TestHelper.InputType.Dropdown);
            ListBoxItem typesListItem = dropdown.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("AddOnInstructionDefinition"))).SingleOrDefault()?.AsListBoxItem();
            TestReport.IsNotNull(typesListItem, "Select element in list");
            typesListItem.DoubleClick();

            // Select templates to delete
            Window deleteElems = TestHelper.GetStandardInput(TestHelper.InputType.MultiSelect);
            AutomationElement[] delElems = [.. deleteElems.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("TemplateFBAOI").Or(win.ByName("TemplateLadderAOI"))))];
            TestReport.IsTrue(delElems.Length == 2, "Select two elements to delete");

            // Select both elements for deletion using shift
            foreach (AutomationElement elem in delElems)
            {
                using (Keyboard.Pressing(VirtualKeyShort.LSHIFT))
                {
                    elem.AsListBoxItem().Click();
                }
            }
            TestHelper.Confirm(deleteElems);

            TestHelper.WaitMilliseconds(shortTimeoutMS);

            // Detect errors from deletion and verify warning popup
            Window detectedErrors = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("Detected Errors"))).SingleOrDefault()?.AsWindow();
            TestReport.IsNotNull(detectedErrors, "Open warning popup for deletion");

            string errorMsg = detectedErrors.FindAllDescendants(win => win.ByControlType(ControlType.Text)).SingleOrDefault()?.AsTextBox().Name;
            TestReport.IsTrue(errorMsg.Contains("WARNING:"), "Warning text present in popup");

            Button oK = detectedErrors.FindAllDescendants(win => win.ByControlType(ControlType.Button).And(win.ByName("OK"))).SingleOrDefault()?.AsButton();
            TestReport.IsNotNull(oK, "Press OK in error popup");
            oK.Click();

            TestHelper.CheckHomePage();
        }

        [TestMethod]
        [TestCategory("GUI_Delete")]
        [TestCategory("GUI_Test")]
        [TestProperty("Description",
        "Testing that attempting to delete an element when no element exists causes a popup.")]
        public void DeleteNoElement()
        {
            // Find and click delete element button
            Button deleteButton = actionSelect.FindAllDescendants(val => val.ByName("DeleteElementButton")).SingleOrDefault()?.AsButton();
            deleteButton.Click();

            // Check for error window indicating no elements found
            Window errorDialog = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("Error", PropertyConditionFlags.MatchSubstring))).SingleOrDefault()?.AsWindow();
            TestReport.IsNotNull(errorDialog, "Open error window");

            string errorMsg = errorDialog.FindAllDescendants(win => win.ByControlType(ControlType.Text)).SingleOrDefault()?.Name?.ToString();
            TestReport.IsTrue(errorMsg.Equals("No Valid Elements Found"), "Show 'No Valid Elements Found' message, found: " + errorMsg);

            Button oK = errorDialog.FindAllDescendants(win => win.ByControlType(ControlType.Button).And(win.ByName("OK"))).SingleOrDefault()?.AsButton();
            TestReport.IsNotNull(oK, "Press OK");

            oK.Click();

            TestHelper.CheckHomePage();
        }

        [TestMethod]
        [TestCategory("GUI_Delete")]
        [TestCategory("GUI_Test")]
        [TestProperty("Description",
        "Test that modules can be deleted using the port as a filter instead of name.")]
        public void DeleteModuleByPort()
        {
            // Load default file for module deletion test
            TestHelper.LoadDefault();

            // Find and click delete element button
            Button deleteButton = actionSelect.FindAllDescendants(val => val.ByName("DeleteElementButton")).SingleOrDefault()?.AsButton();
            deleteButton.Click();

            // Select module type to delete
            Window dropdown = TestHelper.GetStandardInput(TestHelper.InputType.Dropdown);
            ListBoxItem typesListItem = dropdown.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("Module"))).SingleOrDefault()?.AsListBoxItem();
            TestReport.IsNotNull(typesListItem, "Select element in list");
            typesListItem.DoubleClick();

            // Select module to delete
            string moduleName = "1756-IA16";
            Window deleteName = TestHelper.GetStandardInput(TestHelper.InputType.MultiSelect);
            ListBoxItem moduleType = deleteName.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName(moduleName))).SingleOrDefault()?.AsListBoxItem();
            TestReport.IsNotNull(moduleType, "Select module type " + moduleName + " not found");
            moduleType.Click();

            TestHelper.Confirm(deleteName);

            // Select element at port to delete
            Window modulePortDelWin = TestHelper.GetStandardInput(TestHelper.InputType.MultiSelect);
            AutomationElement module = modulePortDelWin.FindAllDescendants(win => win.ByControlType(ControlType.ListItem)).First();
            TestReport.IsNotNull(module, "Module list item not found");
            module.AsListBoxItem().Click();

            TestHelper.Confirm(modulePortDelWin);

            TestHelper.WaitMilliseconds(shortTimeoutMS);

            // Detect errors from deletion and verify warning popup
            Window detectedErrors = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("Detected Errors"))).SingleOrDefault()?.AsWindow();
            TestReport.IsNotNull(detectedErrors, "Open warning popup for deletion");

            string errorMsg = detectedErrors.FindAllDescendants(win => win.ByControlType(ControlType.Text)).SingleOrDefault()?.AsTextBox().Name;
            TestReport.IsTrue(errorMsg.Contains("WARNING:"), "Warning text present in popup");

            Button oK = detectedErrors.FindAllDescendants(win => win.ByControlType(ControlType.Button).And(win.ByName("OK"))).SingleOrDefault()?.AsButton();
            TestReport.IsNotNull(oK, "Press OK in error popup");
            oK.Click();

            TestHelper.CheckHomePage();
        }

        [TestMethod]
        [TestCategory("GUI_Modify")]
        [TestCategory("GUI_Test")]
        [TestProperty("Description",
        "Test attempting to modify a file with no elements.")]
        public void NoElementToModify()
        {
            // Find and click modify element button
            Button modifyButton = actionSelect.FindAllDescendants(val => val.ByName("ModifyElementButton")).SingleOrDefault()?.AsButton();
            modifyButton.Click();

            // Check for error popup indicating no elements to modify
            Window popupNoElements = actionSelect.FindAllDescendants(win => win.ByName("Error", PropertyConditionFlags.MatchSubstring).And(win.ByControlType(ControlType.Window))).SingleOrDefault()?.AsWindow();
            TestReport.IsNotNull(popupNoElements, "Popup window not found");

            AutomationElement popupText = popupNoElements.FindAllDescendants(win => win.ByControlType(ControlType.Text)).SingleOrDefault();
            TestReport.IsNotNull(popupText, "Error message text not found");
            TestReport.IsTrue(popupText.Name.Equals("No Valid Elements Found"), "Show expected error message for modify empty element");

            Button ok = popupNoElements.FindAllDescendants(win => win.ByControlType(ControlType.Button).And(win.ByName("OK"))).SingleOrDefault()?.AsButton();
            TestReport.IsNotNull(ok, "Press OK in popup");
            ok.Click();

            TestHelper.CheckHomePage();
        }

        [TestMethod]
        [TestCategory("GUI_Modify")]
        [TestCategory("GUI_Test")]
        [Ignore("This test is currently disabled as this feature is not yet implemented.")]
        [TestProperty("Description",
        "Test the modification of a normal behaving element.")]
        public void RedundantModifyElement()
        {
            // Load default file for modification test
            TestHelper.LoadDefault();

            // Find and click modify element button
            TestReport.Section("Press modify button");
            Button modifyButton = actionSelect.FindAllDescendants(val => val.ByName("ModifyElementButton")).SingleOrDefault()?.AsButton();
            modifyButton.Click();

            // Select element type to modify
            TestReport.Section("Select element type to modify");
            Window dropdown = TestHelper.GetStandardInput(TestHelper.InputType.Dropdown);
            ListBoxItem typesListItem = dropdown.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("AddOnInstructionDefinition"))).SingleOrDefault()?.AsListBoxItem();
            TestReport.IsNotNull(typesListItem, "Select element in list");
            typesListItem.DoubleClick();

            // Select template to modify
            TestReport.Section("Select template to modify");
            Window modifyElems = TestHelper.GetStandardInput(TestHelper.InputType.MultiSelect);
            AutomationElement templateElem = modifyElems.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("TemplateFBAOI"))).SingleOrDefault();
            TestReport.IsNotNull(templateElem, "Failed to find TemplateFBAOI as element to modify");
            templateElem.Click();
            TestHelper.Confirm(modifyElems);

            // Select attribute to modify
            TestReport.Section("Select attribute to modify");
            Window attrMod = TestHelper.GetStandardInput(TestHelper.InputType.MultiSelect);
            AutomationElement selectedAttr = attrMod.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("Name"))).SingleOrDefault();
            TestReport.IsNotNull(selectedAttr, "Attribute 'Name' not found in attribute list");
            selectedAttr.Click();
            TestHelper.Confirm(attrMod);

            // Set new name for TemplateFBAOI
            TestReport.Section("Set new name for TemplateFBAOI");
            TestHelper.TextboxSetValue(actionSelect, "TemplateLadderAOI");

            // Resolve clashing by selecting replace
            TestReport.Section("Resolve clash via replace");
            Window clash = TestHelper.GetStandardInput(TestHelper.InputType.Dropdown);
            AutomationElement replace = clash.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("Replace"))).SingleOrDefault();
            TestReport.IsNotNull(replace, "Choose Replace in clash window");
            replace.DoubleClick();

            TestHelper.CheckHomePage();
        }

        [TestMethod]
        [TestCategory("GUI_Modify")]
        [TestCategory("GUI_Test")]
        [TestProperty("Description",
        "Test the modification of a normal behaving element.")]
        public void StandardElementModify()
        {
            // Load default file for modification test
            TestHelper.LoadDefault();

            // Find and click modify element button
            Button modifyButton = actionSelect.FindAllDescendants(val => val.ByName("ModifyElementButton")).SingleOrDefault()?.AsButton();
            modifyButton.Click();

            // Select element type to modify
            Window dropdown = TestHelper.GetStandardInput(TestHelper.InputType.Dropdown);
            ListBoxItem typesListItem = dropdown.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("AddOnInstructionDefinition"))).SingleOrDefault()?.AsListBoxItem();
            TestReport.IsNotNull(typesListItem, "Select element in list");
            typesListItem.DoubleClick();

            // Select templates to modify
            Window modifyElems = TestHelper.GetStandardInput(TestHelper.InputType.MultiSelect);
            AutomationElement[] templateElemes = modifyElems.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("TemplateFBAOI").Or(win.ByName("TemplateLadderAOI"))));
            TestReport.IsTrue(templateElemes?.Length == 2, "Template elements in dropdown not found. Found: " + string.Join(", ", templateElemes.Select(a => a.Name)));
            using (Keyboard.Pressing(VirtualKeyShort.LSHIFT))
            {
                foreach (AutomationElement templateSelect in templateElemes)
                {
                    templateSelect.Click();
                }
            }

            TestHelper.Confirm(modifyElems);

            // Select attributes to modify for TemplateFBAOI
            Window attrmod = TestHelper.GetStandardInput(TestHelper.InputType.MultiSelect);
            AutomationElement[] attrs = attrmod.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("Name").Or(win.ByName("CreatedBy"))));
            TestReport.IsTrue(attrs?.Length == 2, "Attributes in dropdown not found. Found: " + string.Join(", ", attrs.Select(a => a.Name)));

            using (Keyboard.Pressing(VirtualKeyShort.CONTROL))
            {
                foreach (AutomationElement templateAttribute in attrs)
                {
                    templateAttribute.Patterns.ScrollItem.Pattern?.ScrollIntoView();
                    templateAttribute.Click();
                }
            }

            TestHelper.Confirm(attrmod);

            // Set CreatedBy and Name for TemplateFBAOI
            TestHelper.TextboxSetValue(actionSelect, "TestFBUser");
            TestHelper.TextboxSetValue(actionSelect, "TestFBName");

            // Select routine subelement to modify
            Window subElem = TestHelper.GetStandardInput(TestHelper.InputType.MultiSelect);
            ListBoxItem subElement = subElem.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("Routines"))).SingleOrDefault()?.AsListBoxItem();
            TestReport.IsNotNull(subElement, "Subelement Routines not found");
            subElement.Click();

            TestHelper.Confirm(subElem);

            // Select attributes of routine subelement to modify
            Window subAttr = TestHelper.GetStandardInput(TestHelper.InputType.MultiSelect);
            ListBoxItem attribute = subAttr.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("Name"))).SingleOrDefault()?.AsListBoxItem();
            TestReport.IsNotNull(attribute, "Attributes in dropdown not found");
            attribute.Click();

            TestHelper.Confirm(subAttr);

            // Set Name of routine subelement
            TestHelper.TextboxSetValue(actionSelect, "BrokenLogic");

            // Close subelement select window
            Window subSelect = TestHelper.GetStandardInput(TestHelper.InputType.MultiSelect);
            Button close = subSelect.FindAllDescendants(win => win.ByControlType(ControlType.Button).And(win.ByName("Close"))).SingleOrDefault()?.AsButton();
            close.Click();

            // Select attributes of TemplateLadderAOI to modify
            Window ladderAttrs = TestHelper.GetStandardInput(TestHelper.InputType.MultiSelect);
            ListBoxItem ladderAttribute = ladderAttrs.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("Name"))).SingleOrDefault()?.AsListBoxItem();
            TestReport.IsNotNull(ladderAttribute, "Attributes in dropdown not found");
            ladderAttribute.Click();

            TestHelper.Confirm(ladderAttrs);

            // Set CreatedBy of TemplateLadderAOI
            TestHelper.TextboxSetValue(actionSelect, "BrokenLogic");

            // Close subelement select window for TemplateLadderAOI
            Window ladderSubSelect = TestHelper.GetStandardInput(TestHelper.InputType.MultiSelect);
            Button ladderClose = ladderSubSelect.FindAllDescendants(win => win.ByControlType(ControlType.Button).And(win.ByName("Close"))).SingleOrDefault()?.AsButton();
            ladderClose.Click();

            TestHelper.WaitMilliseconds(shortTimeoutMS);

            // Check for schema error popup and close it
            Window popupSchema = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("Detected Errors"))).SingleOrDefault()?.AsWindow();
            TestReport.IsNotNull(popupSchema, "Open Schema Error popup");

            Button ok = popupSchema.FindAllDescendants(win => win.ByControlType(ControlType.Button).And(win.ByName("OK"))).SingleOrDefault()?.AsButton();
            ok.Click();

            TestHelper.CheckHomePage();
        }

        [TestMethod]
        [TestCategory("GUI_Import")]
        [TestCategory("GUI_Test")]
        [TestProperty("Description",
        "Test the functionality of the Import Element block.")]
        public void ImportElement()
        {
            // Press "Import Element" Button
            Button importButton = actionSelect.FindAllDescendants(val => val.ByName("ImportElementButton")).SingleOrDefault()?.AsButton();
            importButton.Click();

            TestHelper.WaitMilliseconds(shortTimeoutMS);

            // Select file to import from
            TestReport.Section("Select file to import from");
            Window fileExplorer = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("Open"))).SingleOrDefault()?.AsWindow();
            TestReport.IsNotNull(fileExplorer, "Open File Explorer");

            AutomationElement filePathPane = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.ToolBar).And(win.ByName("Address:", PropertyConditionFlags.MatchSubstring))).SingleOrDefault();
            filePathPane.FindAllDescendants(win => win.ByName("All locations")).SingleOrDefault().AsButton().Click();
            AutomationElement filePathEdit = fileExplorer.FindAllDescendants(win => win.ByControlType(ControlType.Edit).And(win.ByName("Address", PropertyConditionFlags.MatchSubstring))).SingleOrDefault();
            filePathEdit.AsTextBox().Enter(TestXMLsPath);
            Keyboard.Press(VirtualKeyShort.ENTER);

            // Select file to load
            AutomationElement[] files = fileExplorer.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByFrameworkId(FrameworkType.Win32.ToString()).Not()));
            AutomationElement templateFile = files.SingleOrDefault(fil => fil.Name.Equals("TemplateProjectV1.L5X"))
                                             ?? files.SingleOrDefault(fil => fil.Name.Equals("TemplateProjectV1"));
            TestReport.IsNotNull(templateFile, "Template file (TemplateProjectV1.L5X) not found.");
            templateFile.DoubleClick();

            // Select element type to import (AOI)
            TestReport.Section("Select element type(AOI) to import");
            Window aoiSelect = TestHelper.GetStandardInput(TestHelper.InputType.Dropdown);
            ListBoxItem aoiType = aoiSelect.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("AddOnInstructionDefinition"))).SingleOrDefault()?.AsListBoxItem();
            TestReport.IsNotNull(aoiType, "Select element type AddOnInstructionDefinition");
            aoiType.DoubleClick();

            // Select element to import
            TestReport.Section("Select AOI to import");
            Window elementSelect = TestHelper.GetStandardInput(TestHelper.InputType.MultiSelect);
            ListBoxItem aoi = elementSelect.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("TemplateFBAOI"))).SingleOrDefault()?.AsListBoxItem();
            TestReport.IsNotNull(aoi, "Select element TemplateFBAOI");
            ListBoxItem element = elementSelect.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("TemplateLadderAOI"))).SingleOrDefault()?.AsListBoxItem();
            TestReport.IsNotNull(element, "Select element TemplateLadderAOI");
            using (Keyboard.Pressing(VirtualKeyShort.LCONTROL))
            {
                element.Select();
                aoi.Select();
            }

            TestHelper.WaitMilliseconds(shortTimeoutMS);

            // Verify items are actually selected
            ListBox listBox = elementSelect.FindAllDescendants(cf => cf.ByControlType(ControlType.List)).FirstOrDefault()?.AsListBox();
            TestReport.IsNotNull(listBox, "Find the list box containing selectable elements");
            IEnumerable<string> selectedItems = listBox.SelectedItems.Select(i => i.Name);
            ListBoxItem[] expectedItems = [aoi, element];
            string[] expected = [.. expectedItems.Select(i => i.Name)];
            Array.Sort(expected);
            TestReport.IsTrue(selectedItems.SequenceEqual<string>(expected), "Verify both template elements are selected");

            TestHelper.Confirm(elementSelect);

            TestHelper.WaitMilliseconds(shortTimeoutMS);

            // Select Yes to add another from file
            TestReport.Section("Add another routine");
            Window addAnotherYes = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("Import Element"))).SingleOrDefault()?.AsWindow();
            TestReport.IsNotNull(addAnotherYes, "Add another element window not found");

            Button yesButton = addAnotherYes.FindAllDescendants(win => win.ByControlType(ControlType.Button).And(win.ByName("Yes"))).SingleOrDefault()?.AsButton();
            TestReport.IsNotNull(yesButton, "Press Yes to add another");
            yesButton.Click();

            // Select element type with ambiguous parent(routine) to import
            TestReport.Section("Select type 'routine' to import");
            Window ambigSelect = TestHelper.GetStandardInput(TestHelper.InputType.Dropdown);
            ListBoxItem ambigType = ambigSelect.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("Routine"))).SingleOrDefault()?.AsListBoxItem();
            TestReport.IsNotNull(ambigType, "Select element type Routine");
            ambigType.DoubleClick();

            // Select element of type 'Routine' to import
            TestReport.Section("Select 'routine' to import");
            Window elementAmbig = TestHelper.GetStandardInput(TestHelper.InputType.MultiSelect);
            ListBoxItem ambigRout = elementAmbig.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("Logic"))).SingleOrDefault()?.AsListBoxItem();
            TestReport.IsNotNull(ambigRout, "Select element");
            ambigRout.Click();

            TestHelper.Confirm(elementAmbig);

            // Select parent type from extract
            TestReport.Section("Select parent element for extract");
            Window parentExtract = TestHelper.GetStandardInput(TestHelper.InputType.Dropdown);
            ListBoxItem parentOut = parentExtract.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("TemplateLadderAOI"))).SingleOrDefault()?.AsListBoxItem();
            TestReport.IsNotNull(parentOut, "Select parent element for disambiguation in source file");
            parentOut.Click();

            TestHelper.Confirm(parentExtract);

            // Select parent element type for insert
            TestReport.Section("Select parent element type for insert");
            Window typeInsert = TestHelper.GetStandardInput(TestHelper.InputType.Dropdown);
            ListBoxItem typeIn = typeInsert.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("AddOnInstructionDefinition"))).SingleOrDefault()?.AsListBoxItem();
            TestReport.IsNotNull(typeIn, "Select parent element type for insertion in master file");
            typeIn.Click();

            TestHelper.Confirm(typeInsert);

            // Select parent element for insert
            TestReport.Section("Select parent element for insert");
            Window elemInsert = TestHelper.GetStandardInput(TestHelper.InputType.Dropdown);
            ListBoxItem elemIn = elemInsert.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("TemplateFBAOI"))).SingleOrDefault()?.AsListBoxItem();
            TestReport.IsNotNull(elemIn, "Select parent element type for insertion in master file");
            elemIn.Click();

            TestHelper.Confirm(elemInsert);

            // Clash resolution
            TestReport.Section("Resolve clashing routines via replace");
            Window resolutionStyle = TestHelper.GetStandardInput(TestHelper.InputType.Dropdown);
            ListBoxItem cancel = resolutionStyle.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("Replace"))).SingleOrDefault()?.AsListBoxItem();
            TestReport.IsNotNull(cancel, "Choose replace in clash resolution");
            cancel.DoubleClick();

            // Select No to add another from file
            Window addAnotherNo = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("Import Element"))).SingleOrDefault()?.AsWindow();
            TestReport.IsNotNull(addAnotherNo, "Add another element window");

            Button noButton = addAnotherNo.FindAllDescendants(win => win.ByControlType(ControlType.Button).And(win.ByName("No"))).SingleOrDefault()?.AsButton();
            TestReport.IsNotNull(noButton, "Press No to add another");
            noButton.Click();

            TestHelper.CheckHomePage();
        }
    }
}
