using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA2;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reporting;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace L5XAutomationToolTests
{
    [TestClass]
    public class GUITesting
    {
        public Application app;
        public static Window actionSelect;
        public TestContext TestContext { get; set; }
        private readonly string TestAppPath = typeof(L5XAutomationTool.Program).Assembly.Location;
        private readonly int longTimeoutMS = 2000;
        private readonly int shortTimeoutMS = 350;
        private UIA2Automation automation;
        private readonly string TestXMLsPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "L5XFiles", "TestingFiles"));

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
            app = Application.Launch(TestAppPath);
            TestHelper.WaitMilliseconds(longTimeoutMS);

            // Start test reporting for the current test
            TestReport.Start(TestContext.TestName);

            // Get the main window of the application
            actionSelect = app.GetAllTopLevelWindows(automation).First();
            TestReport.IsNotNull(actionSelect, "Find ActionSelect window");
        }

        [TestCleanup]
        public void Teardown()
        {
            // End test reporting and close the application
            TestReport.End();
            app?.Close();
        }

        [TestMethod]
        [TestCategory("File Manipulation")]
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
                TestReport.IsFalse(button.IsOffscreen, "Button not onscreen");
            }

            TestReport.Section("Verify that all buttons are there");
            // Collect button names for comparison
            List<string> buttonNames = buttons.Select(but => but.Name).ToList();

            TestReport.IsTrue(buttonNames.Any(), "Button names not found");

            // List of expected button names
            List<string> expectedButtons = new List<string>() { "ImportElementButton", "CreateElementButton", "ModifyElementButton", "DeleteElementButton", "NewFileButton", "LoadFileButton", "ValidateFileButton", "SaveFileButton", "ExitActionSelect", "Minimize", "Maximize", "Close" };

            // Check that the number of buttons matches expectation
            TestReport.IsTrue(expectedButtons.Count == buttonNames.Count, "Incorrect quantity of buttons");

            // Verify that the actual button names match the expected names
            buttonNames.Sort();
            expectedButtons.Sort();
            bool buttonsCorrect = expectedButtons.SequenceEqual(buttonNames);
            TestReport.IsTrue(buttonsCorrect, "Button names not as expected");

            TestReport.Section("Exit");
            // Find and click the exit button to close the window
            AutomationElement exitButton = actionSelect.FindAllDescendants(but => but.ByName("ExitActionSelect")).SingleOrDefault();
            exitButton.AsButton().Invoke();

            // Wait and verify that the application has closed
            TestHelper.WaitMilliseconds(longTimeoutMS);
            TestReport.IsTrue(app.GetAllTopLevelWindows(automation).Length == 0, "Application failed to close");
        }

        [TestMethod]
        [TestCategory("File Manipulation")]
        [TestProperty("Description",
        "Test that action select cannot be manipulated during operations.")]
        public void ActionSelectLoseControl()
        {
            // List of buttons that should not be tested for control loss
            List<string> prohibNames = new List<string>() { "ExitActionSelect", "Minimize", "Maximize", "Close" };
            // Get all main menu buttons except prohibited ones
            IEnumerable<AutomationElement> mainMenuButtons = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Button)).Where(but => !prohibNames.Contains(but.Name));

            foreach (AutomationElement button in mainMenuButtons)
            {
                // Ensure ActionSelect is enabled before operation
                TestReport.IsTrue(actionSelect.IsEnabled, "Action select found");

                // Invoke button to open a new window
                button.AsButton().Invoke();

                // Find the popup window that appears
                Window popup = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Window)).SingleOrDefault()?.AsWindow();
                TestReport.IsNotNull(popup, "Popup window not found for: " + button?.Name ?? "button is null");

                // ActionSelect should be disabled while popup is open
                TestReport.IsFalse(actionSelect.IsEnabled, "Action select did not yield control");

                // Close the popup window using appropriate button
                if (button.Name.Equals("NewFileButton"))
                {
                    Button no = popup.FindAllDescendants(win => win.ByName("No")).SingleOrDefault().AsButton();
                    TestReport.IsNotNull(no, "Failed to find close button for new file");
                    no.Invoke();
                }
                else
                {
                    Button close = popup.FindAllDescendants(win => win.ByName("Close")).SingleOrDefault().AsButton();
                    TestReport.IsNotNull(close, "Failed to find close button");
                    close.Invoke();
                }
            }
        }

        [TestMethod]
        [TestCategory("File Manipulation")]
        [TestProperty("Description",
        "Test that the validate button creates popups upon button press.")]
        public void FileManagementTesting()
        {
            // VALIDATE W/OUT ERROR
            // Find and invoke the validate button
            Button validateButton = actionSelect.FindAllDescendants(val => val.ByName("ValidateFileButton")).SingleOrDefault()?.AsButton();

            // Validate blank document to test errorless scenario
            validateButton.Invoke();

            // Find the popup for detected errors
            AutomationElement popup = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("Detected Errors"))).SingleOrDefault();
            TestReport.IsNotNull(popup, "Popup not found");

            // Check that the popup text indicates no errors
            string noErrors = popup.FindAllDescendants(win => win.ByControlType(ControlType.Text)).SingleOrDefault()?.Name;
            TestReport.IsTrue(noErrors.Equals("No errors detected!"), "Validation text box popup incorrect");

            // Find and click OK button to close popup
            Button ok = popup.FindAllDescendants(win => win.ByControlType(ControlType.Button).And(win.ByName("OK"))).SingleOrDefault()?.AsButton();
            TestReport.IsNotNull(ok, "Ok button not found");
            ok.Invoke();

            TestHelper.CheckHomePage();

            // LOAD FILE
            // Find and invoke the load button
            Button loadButton = actionSelect.FindAllDescendants(but => but.ByName("LoadFileButton")).SingleOrDefault()?.AsButton();
            TestReport.IsNotNull(loadButton, "Load button not found");

            loadButton.Invoke();
            TestHelper.WaitMilliseconds(500);

            // Find file explorer window
            Window fileExplorer = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("Open"))).SingleOrDefault()?.AsWindow();
            TestReport.IsNotNull(fileExplorer, "File explorer not found");

            // Find address bar and input file path
            AutomationElement filePathPane = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.ToolBar).And(win.ByName("Address:", PropertyConditionFlags.MatchSubstring))).SingleOrDefault();
            filePathPane.FindAllDescendants(win => win.ByName("All locations")).SingleOrDefault().AsButton().Click();
            AutomationElement filePathEdit = fileExplorer.FindAllDescendants(win => win.ByControlType(ControlType.Edit).And(win.ByName("Address", PropertyConditionFlags.MatchSubstring))).SingleOrDefault();
            filePathEdit.AsTextBox().Enter(TestXMLsPath + "\n");

            // Select file to load
            AutomationElement[] files = fileExplorer.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByFrameworkId(FrameworkType.Win32.ToString()).Not()));
            AutomationElement templateBroken = files.SingleOrDefault(fil => fil.Name.Equals("BrokenXML.L5X"));
            TestReport.IsNotNull(templateBroken, "Template file with error(BrokenXML.L5X) not found");

            // Double-click to load the broken template file
            templateBroken.DoubleClick();

            TestHelper.CheckHomePage();

            // Wait for application to catch up
            TestHelper.WaitMilliseconds(100);

            // VALIDATE ERROR FILE
            // Validate loaded file to test error scenario
            validateButton.Invoke();

            // Find error popup
            AutomationElement errors = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("Detected Errors"))).SingleOrDefault();
            TestReport.IsNotNull(errors, "Popup not found");

            // Check that error message contains schema error
            string errorsFound = errors.FindAllDescendants(win => win.ByControlType(ControlType.Text)).SingleOrDefault()?.Name;
            TestReport.IsTrue(errorsFound.Contains("SCHEMA ERROR: "), "Validation text box popup incorrect, instead found: " + errorsFound);

            // Find and click OK button to close error popup
            Button accept = errors.FindAllDescendants(win => win.ByControlType(ControlType.Button).And(win.ByName("OK"))).SingleOrDefault()?.AsButton();
            TestReport.IsNotNull(accept, "Ok button not found");
            accept.Invoke();

            TestHelper.CheckHomePage();

            // NEW FILE DENY CREATION
            // Find and invoke new file button
            Button newButtonNo = actionSelect.FindAllDescendants(win => win.ByName("NewFileButton")).SingleOrDefault()?.AsButton();
            newButtonNo.Invoke();

            // Verify confirmation window appears
            Window verifyCreate = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("Verify File Creation"))).SingleOrDefault()?.AsWindow();
            TestReport.IsNotNull(verifyCreate, "New file overwrite window not found");

            // Select No to cancel new file creation
            Button selectNo = verifyCreate.FindAllDescendants(win => win.ByControlType(ControlType.Button).And(win.ByName("No"))).SingleOrDefault()?.AsButton();
            TestReport.IsNotNull(selectNo, "Cancel new file creation button not found");
            selectNo.Invoke();

            TestHelper.CheckHomePage();

            // SAVE FILE
            // Find and invoke save button
            Button saveButton = actionSelect.FindAllDescendants(win => win.ByName("SaveFileButton")).SingleOrDefault()?.AsButton();
            saveButton.Invoke();

            // Find Save As file explorer window
            Window fileExp = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("Save As"))).SingleOrDefault()?.AsWindow();
            TestReport.IsNotNull(fileExp, "File explorer not found");

            // Find address bar and input save path
            filePathPane = fileExp.FindAllDescendants(win => win.ByControlType(ControlType.ToolBar).And(win.ByName("Address:", PropertyConditionFlags.MatchSubstring))).SingleOrDefault();
            filePathPane.FindAllDescendants(win => win.ByName("All locations")).SingleOrDefault().AsButton().Click();
            filePathEdit = fileExp.FindAllDescendants(win => win.ByControlType(ControlType.Edit).And(win.ByName("Address", PropertyConditionFlags.MatchSubstring))).SingleOrDefault();

            string saveResultPath = Path.Combine(TestXMLsPath, "TestResults");
            filePathEdit.AsTextBox().Enter(saveResultPath + "\n");
            TestHelper.WaitMilliseconds(100);

            // Set file name for saving, ensure uniqueness
            AutomationElement fileName = fileExp.FindAllDescendants(win => win.ByControlType(ControlType.ComboBox).And(win.ByName("File name:"))).SingleOrDefault()?.AsTextBox();
            TestReport.IsNotNull(fileName, "File name field not found in saveFile");

            string nameToSave = "TestSaveResult";
            for (int i = 0; File.Exists(Path.Combine(saveResultPath, nameToSave + ".L5X")); i++)
                nameToSave = Regex.Replace(nameToSave, @"\d", string.Empty) + i.ToString();

            fileName.Click();
            Keyboard.Type(nameToSave + "\n");

            TestHelper.WaitMilliseconds(shortTimeoutMS);

            // Verify file was saved
            TestReport.IsTrue(File.Exists(Path.Combine(saveResultPath, nameToSave + ".L5X")), "File not saved at location");

            // NEW FILE CONFIRM CREATION
            // Find and invoke new file button
            Button newButtonYes = actionSelect.FindAllDescendants(win => win.ByName("NewFileButton")).SingleOrDefault()?.AsButton();
            newButtonYes.Invoke();

            // Verify confirmation window appears
            Window confirmCreate = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("Verify File Creation"))).SingleOrDefault()?.AsWindow();
            TestReport.IsNotNull(confirmCreate, "New file overwrite window not found");

            // Select Yes to confirm new file creation
            Button selectYes = confirmCreate.FindAllDescendants(win => win.ByControlType(ControlType.Button).And(win.ByName("Yes"))).SingleOrDefault()?.AsButton();
            TestReport.IsNotNull(selectYes, "Confirm new file creation button not found");
            selectYes.Invoke();

            TestHelper.WaitMilliseconds(shortTimeoutMS);

            // Click OK to close confirmation window
            Button buttonOK = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Button).And(win.ByName("OK"))).SingleOrDefault()?.AsButton();
            TestReport.IsNotNull(buttonOK, "Accept new file creation button not found");
            buttonOK.Invoke();

            TestHelper.CheckHomePage();
        }

        [TestMethod]
        [TestCategory("File Manipulation")]
        [TestProperty("Description",
        "Test that when the user modifies the file in a way that is not valid for the schema, an error window appears.")]
        public void ValidationErrorWindow()
        {
            // CREATE
            // Find and invoke create element button
            Button createElementButton = actionSelect.FindAllDescendants(val => val.ByName("CreateElementButton")).SingleOrDefault()?.AsButton();
            createElementButton.Invoke();
            TestHelper.WaitMilliseconds(shortTimeoutMS);

            // Select element type to create
            Window elementTypeSelect = TestHelper.GetStandardInput(TestHelper.InputType.Dropdown);
            AutomationElement selectedItem = elementTypeSelect.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("Task"))).SingleOrDefault();
            TestReport.IsNotNull(selectedItem, "Desired element type not found in dropdown.");
            selectedItem.DoubleClick();

            // Select element template to use
            Window elementTemplate = TestHelper.GetStandardInput(TestHelper.InputType.Dropdown);
            ListBoxItem selectedTemplate = elementTemplate.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("TemplateContinuous"))).SingleOrDefault()?.AsListBoxItem();
            TestReport.IsNotNull(selectedTemplate, "Desired element type not found in dropdown.");
            selectedTemplate.DoubleClick();

            // Input quantity and names for elements to create
            int quantityToAdd = 2;
            TestHelper.TextboxSetValue(actionSelect, quantityToAdd.ToString());
            TestHelper.TextboxSetValue(actionSelect, "genTaskWithError");
            TestHelper.TextboxSetValue(actionSelect, "illegal2ndTask");

            // Find detected errors window and verify error message
            Window detectErrors = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("Detected Errors"))).SingleOrDefault().AsWindow();
            TestReport.IsNotNull(detectErrors, "Detected Errors window not found");

            TextBox errorsBox = detectErrors.FindAllDescendants(win => win.ByControlType(ControlType.Text).And(win.ByName("ERROR:", PropertyConditionFlags.MatchSubstring))).SingleOrDefault()?.AsTextBox();
            TestReport.IsNotNull(errorsBox, "Error msg text box");
            string errorMsg = errorsBox.Text;

            // Click OK to close error window
            Button ok = detectErrors.FindAllDescendants(win => win.ByControlType(ControlType.Button).And(win.ByName("OK"))).SingleOrDefault()?.AsButton();
            ok.Invoke();

            TestHelper.CheckHomePage();
        }

        [TestMethod]
        [TestCategory("GUI_Create")]
        [TestProperty("Description",
        "Test that the creation of multiple elements that clash works.")]
        public void ClashingElementCreation()
        {
            // CREATE
            // Find and invoke create element button
            Button createElementButton = actionSelect.FindAllDescendants(val => val.ByName("CreateElementButton")).SingleOrDefault()?.AsButton();
            createElementButton.Invoke();
            TestHelper.WaitMilliseconds(shortTimeoutMS);

            // Select element type to create
            Window elementTypeSelect = TestHelper.GetStandardInput(TestHelper.InputType.Dropdown);
            AutomationElement selectedItem = elementTypeSelect.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("AddOnInstructionDefinition"))).SingleOrDefault();
            TestReport.IsNotNull(selectedItem, "Desired element type not found in dropdown.");
            selectedItem.DoubleClick();

            // Select element template to use
            Window elementTemplate = TestHelper.GetStandardInput(TestHelper.InputType.Dropdown);
            ListBoxItem selectedTemplate = elementTemplate.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("TemplateFBAOI"))).SingleOrDefault()?.AsListBoxItem();
            TestReport.IsNotNull(selectedTemplate, "Desired element type not found in dropdown.");
            selectedTemplate.DoubleClick();

            // Input quantity to create and handle clash resolution for each
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
                        // Resolution style: Replace
                        ListBoxItem replace = resolutionStyle.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("Replace"))).SingleOrDefault()?.AsListBoxItem();
                        TestReport.IsNotNull(replace, "Replace item not found in clash resolution");
                        replace.DoubleClick();
                    }
                    else if (i == 2)
                    {
                        // Resolution style: Rename
                        ListBoxItem rename = resolutionStyle.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("Rename"))).SingleOrDefault()?.AsListBoxItem();
                        TestReport.IsNotNull(rename, "Rename item not found in clash resolution");
                        rename.DoubleClick();

                        TestHelper.TextboxSetValue(actionSelect, "TestSaveFile");
                        TestHelper.TextboxSetValue(actionSelect, "TestSaveFile2");
                    }
                    else if (i == 3)
                    {
                        // Resolution style: Cancel
                        ListBoxItem cancel = resolutionStyle.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("Cancel"))).SingleOrDefault()?.AsListBoxItem();
                        TestReport.IsNotNull(cancel, "Cancel item not found in clash resolution");
                        cancel.DoubleClick();
                    }
                }
            }

            TestHelper.CheckHomePage();
        }

        [TestMethod]
        [TestCategory("GUI_Create")]
        [TestProperty("Description",
        "Test that the standard creation of multiple elements works.")]
        public void ElementQuantityCreation()
        {
            // CREATE
            // Find and invoke create element button
            Button createElementButton = actionSelect.FindAllDescendants(val => val.ByName("CreateElementButton")).SingleOrDefault()?.AsButton();
            createElementButton.Invoke();
            TestHelper.WaitMilliseconds(shortTimeoutMS);

            // Select element type to create
            Window elementTypeSelect = TestHelper.GetStandardInput(TestHelper.InputType.Dropdown);
            AutomationElement selectedItem = elementTypeSelect.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("AddOnInstructionDefinition"))).SingleOrDefault();
            TestReport.IsNotNull(selectedItem, "Desired element type not found in dropdown.");
            selectedItem.DoubleClick();

            // Select element template to use
            Window elementTemplate = TestHelper.GetStandardInput(TestHelper.InputType.Dropdown);
            AutomationElement selectedTemplate = elementTemplate.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("TemplateFBAOI"))).SingleOrDefault();
            TestReport.IsNotNull(selectedTemplate, "Desired element type not found in dropdown.");
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
                TestReport.IsNotNull(nameField, "Text input field for name not found. Run: " + i);

                string foundInput;
                string errorMsg;

                // Test both submit and enter for name input
                if (i % 1 == 1)
                {
                    nameField.Click();
                    nameField.Enter("");
                    nameField.Enter("TestSaveFile" + i);

                    foundInput = nameField.Text;
                    errorMsg = string.Concat("Input name not equal to expected, found: ", foundInput);
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
                    errorMsg = string.Concat("Input name not equal to expected, found: ", foundInput);
                    TestReport.IsTrue(foundInput.Equals("TestSaveFile" + i), errorMsg);

                    TestHelper.Confirm(nameInput);
                }
            }

            TestHelper.CheckHomePage();
        }

        [TestMethod]
        [TestCategory("GUI_Create")]
        [TestProperty("Description",
        "Test that the creation of modules with no name works.")]
        public void NamelessModuleCreation()
        {
            // CREATE
            // Find and invoke create element button
            Button createElementButton = actionSelect.FindAllDescendants(val => val.ByName("CreateElementButton")).SingleOrDefault()?.AsButton();
            createElementButton.Invoke();
            TestHelper.WaitMilliseconds(shortTimeoutMS);

            // Select element type to create
            Window elementTypeSelect = TestHelper.GetStandardInput(TestHelper.InputType.Dropdown);
            AutomationElement selectedItem = elementTypeSelect.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("Module"))).SingleOrDefault();
            TestReport.IsNotNull(selectedItem, "Desired element type not found in dropdown.");
            selectedItem.DoubleClick();

            // Select module template to use
            Window elementTemplate = TestHelper.GetStandardInput(TestHelper.InputType.Dropdown);
            AutomationElement selectedTemplate = elementTemplate.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("1756-IA16"))).SingleOrDefault();
            TestReport.IsNotNull(selectedTemplate, "Desired element type not found in dropdown.");
            selectedTemplate.DoubleClick();

            // Input quantity to create
            int quantityToAdd = 2;
            TestHelper.TextboxSetValue(actionSelect, quantityToAdd.ToString());

            TestHelper.CheckHomePage();
        }

        [TestMethod]
        [TestCategory("GUI_Delete")]
        [TestProperty("Description",
        "Test that deleting an element that exists in the file and has no abnormal behavior works.")]
        public void DeleteElementFound()
        {
            // Load default file for deletion test
            TestHelper.LoadDefault();

            // Find and invoke delete element button
            Button deleteButton = actionSelect.FindAllDescendants(val => val.ByName("DeleteElementButton")).SingleOrDefault()?.AsButton();
            deleteButton.Invoke();

            // Select element type to delete
            Window dropdown = TestHelper.GetStandardInput(TestHelper.InputType.Dropdown);
            ListBoxItem typesListItem = dropdown.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("AddOnInstructionDefinition"))).SingleOrDefault()?.AsListBoxItem();
            TestReport.IsNotNull(typesListItem, "Failed to find element in list");
            typesListItem.DoubleClick();

            // Select templates to delete
            Window deleteElems = TestHelper.GetStandardInput(TestHelper.InputType.MultiSelect);
            AutomationElement[] delElems = deleteElems.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("TemplateFBAOI").Or(win.ByName("TemplateLadderAOI")))).ToArray();
            TestReport.IsTrue(delElems.Length == 2, "Did not find two elements to delete");

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
            TestReport.IsNotNull(detectedErrors, "No popup for erroneous deletion of elements");

            string errorMsg = detectedErrors.FindAllDescendants(win => win.ByControlType(ControlType.Text)).SingleOrDefault()?.AsTextBox().Name;
            TestReport.IsTrue(errorMsg.Contains("WARNING:"), "No 'warning' found in text of popup");

            Button oK = detectedErrors.FindAllDescendants(win => win.ByControlType(ControlType.Button).And(win.ByName("OK"))).SingleOrDefault()?.AsButton();
            TestReport.IsNotNull(oK, "Ok button not found in error popup");
            oK.Invoke();

            TestHelper.CheckHomePage();
        }

        [TestMethod]
        [TestCategory("GUI_Delete")]
        [TestProperty("Description",
        "Testing that attempting to delete an element when no element exists causes a popup.")]
        public void DeleteNoElement()
        {
            // Find and invoke delete element button
            Button deleteButton = actionSelect.FindAllDescendants(val => val.ByName("DeleteElementButton")).SingleOrDefault()?.AsButton();
            deleteButton.Invoke();

            // Check for error window indicating no elements found
            Window errorDialog = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("Error", PropertyConditionFlags.MatchSubstring))).SingleOrDefault()?.AsWindow();
            TestReport.IsNotNull(errorDialog, "Error window failed to appear");

            string errorMsg = errorDialog.FindAllDescendants(win => win.ByControlType(ControlType.Text)).SingleOrDefault()?.Name?.ToString();
            TestReport.IsTrue(errorMsg.Equals("No Valid Elements Found"), "Error msg does not contain 'No elements found', instead: " + errorMsg);

            Button oK = errorDialog.FindAllDescendants(win => win.ByControlType(ControlType.Button).And(win.ByName("OK"))).SingleOrDefault()?.AsButton();
            TestReport.IsNotNull(oK, "oK button not found");

            oK.Invoke();

            TestHelper.CheckHomePage();
        }

        [TestMethod]
        [TestCategory("GUI_Delete")]
        [TestProperty("Description",
        "Test that modules can be deleted using the port as a filter instead of name.")]
        public void DeleteModuleByPort()
        {
            // Load default file for module deletion test
            TestHelper.LoadDefault();

            // Find and invoke delete element button
            Button deleteButton = actionSelect.FindAllDescendants(val => val.ByName("DeleteElementButton")).SingleOrDefault()?.AsButton();
            deleteButton.Invoke();

            // Select module type to delete
            Window dropdown = TestHelper.GetStandardInput(TestHelper.InputType.Dropdown);
            ListBoxItem typesListItem = dropdown.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("Module"))).SingleOrDefault()?.AsListBoxItem();
            TestReport.IsNotNull(typesListItem, "Failed to find element in list");
            typesListItem.DoubleClick();

            // Select module to delete
            string moduleName = "1756-IA16";
            Window deleteName = TestHelper.GetStandardInput(TestHelper.InputType.MultiSelect);
            ListBoxItem moduleType = deleteName.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName(moduleName))).SingleOrDefault()?.AsListBoxItem();
            TestReport.IsNotNull(moduleType, "Module of type " + moduleName + " not found");
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
            TestReport.IsNotNull(detectedErrors, "No popup for erroneous deletion of elements");

            string errorMsg = detectedErrors.FindAllDescendants(win => win.ByControlType(ControlType.Text)).SingleOrDefault()?.AsTextBox().Name;
            TestReport.IsTrue(errorMsg.Contains("WARNING:"), "No 'warning' found in text of popup");

            Button oK = detectedErrors.FindAllDescendants(win => win.ByControlType(ControlType.Button).And(win.ByName("OK"))).SingleOrDefault()?.AsButton();
            TestReport.IsNotNull(oK, "Ok button not found in error popup");
            oK.Invoke();

            TestHelper.CheckHomePage();
        }

        [TestMethod]
        [TestCategory("GUI_Modify")]
        [TestProperty("Description",
        "Test attempting to modify a file with no elements.")]
        public void NoElementToModify()
        {
            // Find and invoke modify element button
            Button modifyButton = actionSelect.FindAllDescendants(val => val.ByName("ModifyElementButton")).SingleOrDefault()?.AsButton();
            modifyButton.Invoke();

            // Check for error popup indicating no elements to modify
            Window popupNoElements = actionSelect.FindAllDescendants(win => win.ByName("Error", PropertyConditionFlags.MatchSubstring).And(win.ByControlType(ControlType.Window))).SingleOrDefault()?.AsWindow();
            TestReport.IsNotNull(popupNoElements, "Popup window not found");

            AutomationElement popupText = popupNoElements.FindAllDescendants(win => win.ByControlType(ControlType.Text)).SingleOrDefault();
            TestReport.IsNotNull(popupText, "Error message text not found");
            TestReport.IsTrue(popupText.Name.Equals("No Valid Elements Found"), "Error Message for modify empty element not equal to expected");

            Button ok = popupNoElements.FindAllDescendants(win => win.ByControlType(ControlType.Button).And(win.ByName("OK"))).SingleOrDefault()?.AsButton();
            TestReport.IsNotNull(ok, "Ok button not found in popup");
            ok.Invoke();

            TestHelper.CheckHomePage();
        }

        [TestMethod]
        [TestCategory("GUI_Modify")]
        [TestProperty("Description",
        "Test the modification of a normal behaving element.")]
        public void RedundantModifyElement()
        {
            // Load default file for modification test
            TestHelper.LoadDefault();

            // Find and invoke modify element button
            Button modifyButton = actionSelect.FindAllDescendants(val => val.ByName("ModifyElementButton")).SingleOrDefault()?.AsButton();
            modifyButton.Invoke();

            // Select element type to modify
            Window dropdown = TestHelper.GetStandardInput(TestHelper.InputType.Dropdown);
            ListBoxItem typesListItem = dropdown.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("AddOnInstructionDefinition"))).SingleOrDefault()?.AsListBoxItem();
            TestReport.IsNotNull(typesListItem, "Failed to find element in list");
            typesListItem.DoubleClick();

            // Select template to modify
            Window modifyElems = TestHelper.GetStandardInput(TestHelper.InputType.MultiSelect);
            AutomationElement templateElem = modifyElems.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("TemplateFBAOI"))).SingleOrDefault();
            TestReport.IsNotNull(templateElem, "Failed to find TemplateFBAOI as element to modify");
            templateElem.Click();
            TestHelper.Confirm(modifyElems);

            // Select attribute to modify
            Window attrMod = TestHelper.GetStandardInput(TestHelper.InputType.MultiSelect);
            AutomationElement selectedAttr = attrMod.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("Name"))).SingleOrDefault();
            TestReport.IsNotNull(selectedAttr, "Attribute 'Name' not found in attribute list");
            selectedAttr.Click();
            TestHelper.Confirm(attrMod);

            // Set new name for TemplateFBAOI
            TestHelper.TextboxSetValue(actionSelect, "TemplateLadderAOI");

            // Resolve clashing by selecting replace
            Window clash = TestHelper.GetStandardInput(TestHelper.InputType.Dropdown);
            AutomationElement replace = clash.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("Replace"))).SingleOrDefault();
            TestReport.IsNotNull(replace, "Did not find replace element in clash window");
            replace.DoubleClick();

            TestHelper.CheckHomePage();
        }

        [TestMethod]
        [TestCategory("GUI_Modify")]
        [TestProperty("Description",
        "Test the modification of a normal behaving element.")]
        public void StandardElementModify()
        {
            // Load default file for modification test
            TestHelper.LoadDefault();

            // Find and invoke modify element button
            Button modifyButton = actionSelect.FindAllDescendants(val => val.ByName("ModifyElementButton")).SingleOrDefault()?.AsButton();
            modifyButton.Invoke();

            // Select element type to modify
            Window dropdown = TestHelper.GetStandardInput(TestHelper.InputType.Dropdown);
            ListBoxItem typesListItem = dropdown.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("AddOnInstructionDefinition"))).SingleOrDefault()?.AsListBoxItem();
            TestReport.IsNotNull(typesListItem, "Failed to find element in list");
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
            close.Invoke();

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
            ladderClose.Invoke();

            TestHelper.WaitMilliseconds(shortTimeoutMS);

            // Check for schema error popup and close it
            Window popupSchema = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("Detected Errors"))).SingleOrDefault()?.AsWindow();
            TestReport.IsNotNull(popupSchema, "Schema Error popup failed to appear");

            Button ok = popupSchema.FindAllDescendants(win => win.ByControlType(ControlType.Button).And(win.ByName("OK"))).SingleOrDefault()?.AsButton();
            ok.Invoke();

            TestHelper.CheckHomePage();
        }

        [TestMethod]
        [TestCategory("GUI_Import")]
        [TestProperty("Description",
        "Test the functionality of the Import Element block.")]
        public void ImportElement()
        {
            // Press "Import Element" Button
            Button importButton = actionSelect.FindAllDescendants(val => val.ByName("ImportElementButton")).SingleOrDefault()?.AsButton();
            importButton.Invoke();

            TestHelper.WaitMilliseconds(shortTimeoutMS);

            // Select file to import from
            Window fileExplorer = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("Open"))).SingleOrDefault()?.AsWindow();
            TestReport.IsNotNull(fileExplorer, "File explorer not found");

            AutomationElement filePathPane = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.ToolBar).And(win.ByName("Address:", PropertyConditionFlags.MatchSubstring))).SingleOrDefault();
            filePathPane.FindAllDescendants(win => win.ByName("All locations")).SingleOrDefault().AsButton().Click();
            AutomationElement filePathEdit = fileExplorer.FindAllDescendants(win => win.ByControlType(ControlType.Edit).And(win.ByName("Address", PropertyConditionFlags.MatchSubstring))).SingleOrDefault();
            filePathEdit.AsTextBox().Enter(TestXMLsPath + "\n");

            // Select file to load
            AutomationElement[] files = fileExplorer.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByFrameworkId(FrameworkType.Win32.ToString()).Not()));
            AutomationElement templateBroken = files.SingleOrDefault(fil => fil.Name.Equals("TemplateProjectV1.L5X"));
            TestReport.IsNotNull(templateBroken, "Template file with error(TemplateProjectV1.L5X) not found.");
            templateBroken.DoubleClick();

            // Select element type to import (AOI)
            Window typeSelect = TestHelper.GetStandardInput(TestHelper.InputType.Dropdown);
            ListBoxItem elemType = typeSelect.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("AddOnInstructionDefinition"))).SingleOrDefault()?.AsListBoxItem();
            TestReport.IsNotNull(elemType, "Element type AddOnInstructionDefinition not found.");
            elemType.DoubleClick();

            // Select element of type to import
            Window elementSelect = TestHelper.GetStandardInput(TestHelper.InputType.MultiSelect);
            ListBoxItem element = elementSelect.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("TemplateFBAOI"))).SingleOrDefault()?.AsListBoxItem();
            TestReport.IsNotNull(element, "Element TemplateFBAOI not found.");
            element.Click();

            TestHelper.Confirm(elementSelect);

            TestHelper.WaitMilliseconds(shortTimeoutMS);

            // Select Yes to add another from file
            Window addAnotherYes = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("Import Element"))).SingleOrDefault()?.AsWindow();
            TestReport.IsNotNull(addAnotherYes, "Add another element window not found");

            Button yesButton = addAnotherYes.FindAllDescendants(win => win.ByControlType(ControlType.Button).And(win.ByName("Yes"))).SingleOrDefault()?.AsButton();
            TestReport.IsNotNull(yesButton, "Yes button not found in add another");
            yesButton.Invoke();

            // Select element type with ambiguous parent to import
            Window ambigSelect = TestHelper.GetStandardInput(TestHelper.InputType.Dropdown);
            ListBoxItem ambigType = ambigSelect.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("Routine"))).SingleOrDefault()?.AsListBoxItem();
            TestReport.IsNotNull(ambigType, "Element type Routine not found.");
            ambigType.DoubleClick();

            // Select element of type 'Routine' to import
            Window elementAmbig = TestHelper.GetStandardInput(TestHelper.InputType.MultiSelect);
            ListBoxItem ambigRout = elementAmbig.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("Logic"))).SingleOrDefault()?.AsListBoxItem();
            TestReport.IsNotNull(ambigRout, "Element not found.");
            ambigRout.Click();

            TestHelper.Confirm(elementAmbig);

            // Select parent element for disambiguation
            Window disambig = TestHelper.GetStandardInput(TestHelper.InputType.Dropdown);
            ListBoxItem parentElem = disambig.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("TemplateFBAOI"))).SingleOrDefault()?.AsListBoxItem();
            TestReport.IsNotNull(parentElem, "Element type Routine not found.");
            parentElem.Click();

            TestHelper.Confirm(disambig);

            // Select parent type from extract
            Window parentExtract = TestHelper.GetStandardInput(TestHelper.InputType.Dropdown);
            ListBoxItem parentOut = parentExtract.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("AddOnInstructionDefinition"))).SingleOrDefault()?.AsListBoxItem();
            TestReport.IsNotNull(parentOut, "Parent element for disambiguation not found in source file");
            parentOut.Click();

            TestHelper.Confirm(parentExtract);

            // Select parent element for insert
            Window parentInsert = TestHelper.GetStandardInput(TestHelper.InputType.Dropdown);
            ListBoxItem parentIn = parentInsert.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("AddOnInstructionDefinition"))).SingleOrDefault()?.AsListBoxItem();
            TestReport.IsNotNull(parentIn, "Parent element for disambiguation not found in master file");
            parentIn.Click();

            TestHelper.Confirm(parentInsert);

            // Select No to add another from file
            Window addAnotherNo = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("Import Element"))).SingleOrDefault()?.AsWindow();
            TestReport.IsNotNull(addAnotherNo, "Add another element window not found");

            Button noButton = addAnotherNo.FindAllDescendants(win => win.ByControlType(ControlType.Button).And(win.ByName("no"))).SingleOrDefault()?.AsButton();
            TestReport.IsNotNull(noButton, "Yes button not found in add another");
            noButton.Invoke();

            TestHelper.CheckHomePage();
        }
    }
}
