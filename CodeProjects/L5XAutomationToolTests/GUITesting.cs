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
            // Kill Existing applications
            Process[] running = Process.GetProcessesByName("L5XAutomationTool");
            foreach (Process proc in running)
            {
                proc.Kill();
            }

            automation = new UIA2Automation();
            // Launch the application
            app = Application.Launch(TestAppPath);
            TestHelper.WaitMilliseconds(longTimeoutMS);

            TestReport.Start(TestContext.TestName);

            actionSelect = app.GetAllTopLevelWindows(automation).First();
            TestReport.IsNotNull(actionSelect, "Find ActionSelect window");

        }

        [TestCleanup]
        public void Teardown()
        {
            TestReport.End();
            app?.Close();
        }

        [TestMethod]
        [TestCategory("File Manipulation")]
        [TestProperty("Description",
        "Test that the ActionSelect window launches with all necessary buttons, and that the exit button closes the window.")]
        public void ActionSelectLayout()
        {
            AutomationElement[] buttons = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Button));

            TestReport.Section("Verify all buttons are onscreen");

            foreach (AutomationElement button in buttons)
            {
                // Verify that all buttons are onscreen
                TestReport.IsFalse(button.IsOffscreen, "Button not onscreen");
            }

            TestReport.Section("Verify that all buttons are there");
            List<string> buttonNames = buttons.Select(but => but.Name).ToList();

            TestReport.IsTrue(buttonNames.Any(), "Button names not found");

            List<string> expectedButtons = new List<string>() { "ImportElementButton", "CreateElementButton", "ModifyElementButton", "DeleteElementButton", "NewFileButton", "LoadFileButton", "ValidateFileButton", "SaveFileButton", "ExitActionSelect", "Minimize", "Maximize", "Close" };

            TestReport.IsTrue(expectedButtons.Count == buttonNames.Count, "Incorrect quantity of buttons");

            // Verify that the button names found are the same as the names expected
            buttonNames.Sort();
            expectedButtons.Sort();
            bool buttonsCorrect = expectedButtons.SequenceEqual(buttonNames);
            TestReport.IsTrue(buttonsCorrect, "Button names not as expected");

            TestReport.Section("Exit");
            // Press exit
            AutomationElement exitButton = actionSelect.FindAllDescendants(but => but.ByName("ExitActionSelect")).SingleOrDefault();
            exitButton.AsButton().Invoke();

            // Application should close upon pressing exit button
            TestHelper.WaitMilliseconds(longTimeoutMS);
            TestReport.IsTrue(app.GetAllTopLevelWindows(automation).Length == 0, "Application failed to close");

        }

        [TestMethod]
        [TestCategory("File Manipulation")]
        [TestProperty("Description",
        "Test that action select cannot be manipulated during operations.")]
        public void ActionSelectLoseControl()
        {
            List<string> prohibNames = new List<string>() { "ExitActionSelect", "Minimize", "Maximize", "Close" };
            IEnumerable<AutomationElement> mainMenuButtons = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Button)).Where(but => !prohibNames.Contains(but.Name));

            foreach (AutomationElement button in mainMenuButtons)
            {
                TestReport.IsTrue(actionSelect.IsEnabled, "Action select found");

                // Press button to open new window
                button.AsButton().Invoke();

                // New window should appear
                Window popup = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Window)).SingleOrDefault()?.AsWindow();
                TestReport.IsNotNull(popup, "Popup window not found for: " + button?.Name ?? "button is null");

                // ActionSelect should not be available
                TestReport.IsFalse(actionSelect.IsEnabled, "Action select did not yield control");

                if (button.Name.Equals("NewFileButton"))
                {
                    Button no = popup.FindAllDescendants(win => win.ByName("No")).SingleOrDefault().AsButton();
                    TestReport.IsNotNull(no, "Failed to find close button for new file");
                    no.Invoke();
                }
                else
                {
                    // Close window
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
            Button validateButton = actionSelect.FindAllDescendants(val => val.ByName("ValidateFileButton")).SingleOrDefault()?.AsButton();

            // Validate blank doc to test errorless doc
            validateButton.Invoke();

            // Get the popup
            AutomationElement popup = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("Detected Errors"))).SingleOrDefault();
            TestReport.IsNotNull(popup, "Popup not found");

            // Read no errors
            string noErrors = popup.FindAllDescendants(win => win.ByControlType(ControlType.Text)).SingleOrDefault()?.Name;
            TestReport.IsTrue(noErrors.Equals("No errors detected!"), "Validation text box popup incorrect");

            // Press OK button to accept no errors
            Button ok = popup.FindAllDescendants(win => win.ByControlType(ControlType.Button).And(win.ByName("OK"))).SingleOrDefault()?.AsButton();
            TestReport.IsNotNull(ok, "Ok button not found");
            ok.Invoke();

            TestHelper.CheckHomePage();

            // LOAD FILE
            Button loadButton = actionSelect.FindAllDescendants(but => but.ByName("LoadFileButton")).SingleOrDefault()?.AsButton();
            TestReport.IsNotNull(loadButton, "Load button not found");

            loadButton.Invoke();
            TestHelper.WaitMilliseconds(500);

            Window fileExplorer = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("Open"))).SingleOrDefault()?.AsWindow();
            TestReport.IsNotNull(fileExplorer, "File explorer not found");

            AutomationElement filePathPane = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.ToolBar).And(win.ByName("Address:", PropertyConditionFlags.MatchSubstring))).SingleOrDefault();

            // Click button to get file path
            filePathPane.FindAllDescendants(win => win.ByName("All locations")).SingleOrDefault().AsButton().Click();
            AutomationElement filePathEdit = fileExplorer.FindAllDescendants(win => win.ByControlType(ControlType.Edit).And(win.ByName("Address", PropertyConditionFlags.MatchSubstring))).SingleOrDefault();

            // Input path
            filePathEdit.AsTextBox().Enter(TestXMLsPath + "\n");

            // Select file to load
            AutomationElement[] files = fileExplorer.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByFrameworkId(FrameworkType.Win32.ToString()).Not()));

            AutomationElement templateBroken = files.SingleOrDefault(fil => fil.Name.Equals("BrokenXML.L5X"));
            TestReport.IsNotNull(templateBroken, "Template file with error(BrokenXML.L5X) not found");

            // Select the broken template file as the file to load
            templateBroken.DoubleClick();

            TestHelper.CheckHomePage();

            // Sleep for so the tool can catch up
            TestHelper.WaitMilliseconds(100);

            // VALIDATE ERROR FILE
            // Validate blank doc to test errorless doc
            validateButton.Invoke();

            // Get the popup
            AutomationElement errors = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("Detected Errors"))).SingleOrDefault();
            TestReport.IsNotNull(errors, "Popup not found");

            // Read no errors
            string errorsFound = errors.FindAllDescendants(win => win.ByControlType(ControlType.Text)).SingleOrDefault()?.Name;
            TestReport.IsTrue(errorsFound.Contains("SCHEMA ERROR: "), "Validation text box popup incorrect, instead found: " + errorsFound);

            // Press OK button to accept no errors
            Button accept = errors.FindAllDescendants(win => win.ByControlType(ControlType.Button).And(win.ByName("OK"))).SingleOrDefault()?.AsButton();
            TestReport.IsNotNull(accept, "Ok button not found");
            accept.Invoke();

            TestHelper.CheckHomePage();

            // NEW FILE DENY CREATION
            Button newButtonNo = actionSelect.FindAllDescendants(win => win.ByName("NewFileButton")).SingleOrDefault()?.AsButton();
            newButtonNo.Invoke();

            // Verify that confirmation window appears
            Window verifyCreate = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("Verify File Creation"))).SingleOrDefault()?.AsWindow();
            TestReport.IsNotNull(verifyCreate, "New file overwrite window not found");

            // Select No on overwriting existing
            Button selectNo = verifyCreate.FindAllDescendants(win => win.ByControlType(ControlType.Button).And(win.ByName("No"))).SingleOrDefault()?.AsButton();
            TestReport.IsNotNull(selectNo, "Cancel new file creation button not found");
            selectNo.Invoke();

            TestHelper.CheckHomePage();

            // SAVE FILE
            Button saveButton = actionSelect.FindAllDescendants(win => win.ByName("SaveFileButton")).SingleOrDefault()?.AsButton();
            saveButton.Invoke();

            Window fileExp = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("Save As"))).SingleOrDefault()?.AsWindow();
            TestReport.IsNotNull(fileExp, "File explorer not found");

            filePathPane = fileExp.FindAllDescendants(win => win.ByControlType(ControlType.ToolBar).And(win.ByName("Address:", PropertyConditionFlags.MatchSubstring))).SingleOrDefault();

            // Click button to get file path
            filePathPane.FindAllDescendants(win => win.ByName("All locations")).SingleOrDefault().AsButton().Click();

            filePathEdit = fileExp.FindAllDescendants(win => win.ByControlType(ControlType.Edit).And(win.ByName("Address", PropertyConditionFlags.MatchSubstring))).SingleOrDefault();

            string saveResultPath = Path.Combine(TestXMLsPath, "TestResults");
            // Input path
            filePathEdit.AsTextBox().Enter(saveResultPath + "\n");
            TestHelper.WaitMilliseconds(100); // Wait a bit for application to catch up to test

            // Set file name
            AutomationElement fileName = fileExp.FindAllDescendants(win => win.ByControlType(ControlType.ComboBox).And(win.ByName("File name:"))).SingleOrDefault()?.AsTextBox();
            TestReport.IsNotNull(fileName, "File name field not found in saveFile");

            string nameToSave = "TestSaveResult";
            for (int i = 0; File.Exists(Path.Combine(saveResultPath, nameToSave + ".L5X")); i++)
                nameToSave = Regex.Replace(nameToSave, @"\d", string.Empty) + i.ToString();

            fileName.Click();
            Keyboard.Type(nameToSave + "\n");

            // Wait for the tool to catch up
            TestHelper.WaitMilliseconds(shortTimeoutMS);

            TestReport.IsTrue(File.Exists(Path.Combine(saveResultPath, nameToSave + ".L5X")), "File not saved at location");

            // NEW FILE CONFIRM CREATION
            Button newButtonYes = actionSelect.FindAllDescendants(win => win.ByName("NewFileButton")).SingleOrDefault()?.AsButton();
            newButtonYes.Invoke();

            // Verify that confirmation window appears
            Window confirmCreate = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("Verify File Creation"))).SingleOrDefault()?.AsWindow();
            TestReport.IsNotNull(confirmCreate, "New file overwrite window not found");

            // Click Yes on overwriting existing
            Button selectYes = confirmCreate.FindAllDescendants(win => win.ByControlType(ControlType.Button).And(win.ByName("Yes"))).SingleOrDefault()?.AsButton();
            TestReport.IsNotNull(selectYes, "Confirm new file creation button not found");
            selectYes.Invoke();

            // Wait for the tool to catch up
            TestHelper.WaitMilliseconds(shortTimeoutMS);

            // Click OK to close the file creation confirmation window
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
            Button createElementButton = actionSelect.FindAllDescendants(val => val.ByName("CreateElementButton")).SingleOrDefault()?.AsButton();

            // Begin element creation
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

            // Input quantity to create
            int quantityToAdd = 2;
            TestHelper.TextboxSetValue(actionSelect, quantityToAdd.ToString());

            // Input name for first element
            TestHelper.TextboxSetValue(actionSelect, "genTaskWithError");

            // Input name for second element
            TestHelper.TextboxSetValue(actionSelect, "illegal2ndTask");

            // Find detected errors window
            Window detectErrors = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("Detected Errors"))).SingleOrDefault().AsWindow();
            TestReport.IsNotNull(detectErrors, "Detected Errors window not found");

            TextBox errorsBox = detectErrors.FindAllDescendants(win => win.ByControlType(ControlType.Text).And(win.ByName("ERROR:", PropertyConditionFlags.MatchSubstring))).SingleOrDefault()?.AsTextBox();
            TestReport.IsNotNull(errorsBox, "Error msg text box");
            string errorMsg = errorsBox.Text;

            // Press Ok
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

            // Input quantity to create
            int quantityToAdd = 4;
            TestHelper.TextboxSetValue(actionSelect, quantityToAdd.ToString());

            for (int i = 0; i < quantityToAdd; i++)
            {
                TestHelper.TextboxSetValue(actionSelect, "TestSaveFile");

                if (i != 0)
                {
                    // Test each of the clashing element resolution methods
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
            Button createElementButton = actionSelect.FindAllDescendants(val => val.ByName("CreateElementButton")).SingleOrDefault()?.AsButton();

            // Begin element creation
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

            // Wait for next window to appear
            TestHelper.WaitMilliseconds(longTimeoutMS);

            // Input quantity to create
            int quantityToAdd = 2;
            TestHelper.TextboxSetValue(actionSelect, quantityToAdd.ToString());

            for (int i = 0; i < quantityToAdd; i++)
            {
                TestHelper.WaitMilliseconds(shortTimeoutMS);
                Window nameInput = TestHelper.GetStandardInput(TestHelper.InputType.TextInput);

                // find the text input field
                TextBox nameField = nameInput.FindAllDescendants(win => win.ByControlType(ControlType.Edit).And(win.ByName("TextField"))).SingleOrDefault()?.AsTextBox();
                TestReport.IsNotNull(nameField, "Text input field for name not found. Run: " + i);

                string foundInput;
                string errorMsg;

                // test both submit and enter
                if (i % 1 == 1)
                {
                    // Input file name to save
                    nameField.Click();
                    nameField.Enter("");
                    nameField.Enter("TestSaveFile" + i);

                    // Verify text was input
                    foundInput = nameField.Text;
                    errorMsg = string.Concat("Input name not equal to expected, found: ", foundInput);
                    TestReport.IsTrue(foundInput.Equals("TestSaveFile" + i), errorMsg);

                    // Submit
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
            Button createElementButton = actionSelect.FindAllDescendants(val => val.ByName("CreateElementButton")).SingleOrDefault()?.AsButton();

            // Begin element creation
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
            TestHelper.LoadDefault();

            // Delete button
            Button deleteButton = actionSelect.FindAllDescendants(val => val.ByName("DeleteElementButton")).SingleOrDefault()?.AsButton();
            deleteButton.Invoke();

            // Select AddOnInstruction to delete
            Window dropdown = TestHelper.GetStandardInput(TestHelper.InputType.Dropdown);

            ListBoxItem typesListItem = dropdown.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("AddOnInstructionDefinition"))).SingleOrDefault()?.AsListBoxItem();
            TestReport.IsNotNull(typesListItem, "Failed to find element in list");

            typesListItem.DoubleClick();

            // select TemplateFBAOI and TemplateLadderAOI to delete
            Window deleteElems = TestHelper.GetStandardInput(TestHelper.InputType.MultiSelect);

            AutomationElement[] delElems = deleteElems.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("TemplateFBAOI").Or(win.ByName("TemplateLadderAOI")))).ToArray();
            TestReport.IsTrue(delElems.Length == 2, "Did not find two elements to delete");

            foreach (AutomationElement elem in delElems)
            {
                using (Keyboard.Pressing(VirtualKeyShort.LSHIFT))
                {
                    elem.AsListBoxItem().Click();
                }
            }
            TestHelper.Confirm(deleteElems);

            TestHelper.WaitMilliseconds(shortTimeoutMS);

            // Detect errors from erroneously deleted element
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
            // Delete button
            Button deleteButton = actionSelect.FindAllDescendants(val => val.ByName("DeleteElementButton")).SingleOrDefault()?.AsButton();
            deleteButton.Invoke();

            // Check that window for "no element found" appears
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
            TestHelper.LoadDefault();

            // Delete button
            Button deleteButton = actionSelect.FindAllDescendants(val => val.ByName("DeleteElementButton")).SingleOrDefault()?.AsButton();
            deleteButton.Invoke();

            // Select Module to delete
            Window dropdown = TestHelper.GetStandardInput(TestHelper.InputType.Dropdown);

            ListBoxItem typesListItem = dropdown.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("Module"))).SingleOrDefault()?.AsListBoxItem();
            TestReport.IsNotNull(typesListItem, "Failed to find element in list");

            typesListItem.DoubleClick();

            // Select module '1756-IA16' to delete
            string moduleName = "1756-IA16";
            Window deleteName = TestHelper.GetStandardInput(TestHelper.InputType.MultiSelect);

            ListBoxItem moduleType = deleteName.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName(moduleName))).SingleOrDefault()?.AsListBoxItem();
            TestReport.IsNotNull(moduleType, "Module of type " + moduleName + " not found");
            moduleType.Click();

            TestHelper.Confirm(deleteName);

            // select element at port X to delete
            Window modulePortDelWin = TestHelper.GetStandardInput(TestHelper.InputType.MultiSelect);

            AutomationElement module = modulePortDelWin.FindAllDescendants(win => win.ByControlType(ControlType.ListItem)).First();
            TestReport.IsNotNull(module, "Module list item not found");
            module.AsListBoxItem().Click();

            TestHelper.Confirm(modulePortDelWin);

            TestHelper.WaitMilliseconds(shortTimeoutMS);

            // Detect errors from erroneously deleted element
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
            // Modify button press
            Button modifyButton = actionSelect.FindAllDescendants(val => val.ByName("ModifyElementButton")).SingleOrDefault()?.AsButton();
            modifyButton.Invoke();

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
            TestHelper.LoadDefault();

            // Modify button
            Button modifyButton = actionSelect.FindAllDescendants(val => val.ByName("ModifyElementButton")).SingleOrDefault()?.AsButton();
            modifyButton.Invoke();

            // Select AddOnInstruction to modify
            Window dropdown = TestHelper.GetStandardInput(TestHelper.InputType.Dropdown);

            ListBoxItem typesListItem = dropdown.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("AddOnInstructionDefinition"))).SingleOrDefault()?.AsListBoxItem();
            TestReport.IsNotNull(typesListItem, "Failed to find element in list");

            typesListItem.DoubleClick();

            // select TemplateFBAOI to modify
            Window modifyElems = TestHelper.GetStandardInput(TestHelper.InputType.MultiSelect);

            AutomationElement templateElem = modifyElems.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("TemplateFBAOI"))).SingleOrDefault();
            TestReport.IsNotNull(templateElem, "Failed to find TemplateFBAOI as element to modify");
            templateElem.Click();
            TestHelper.Confirm(modifyElems);

            // Select attributes to modify(name)
            Window attrMod = TestHelper.GetStandardInput(TestHelper.InputType.MultiSelect);

            AutomationElement selectedAttr = attrMod.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("Name"))).SingleOrDefault();
            TestReport.IsNotNull(selectedAttr, "Attribute 'Name' not found in attribute list");
            selectedAttr.Click();
            TestHelper.Confirm(attrMod);

            // Set name for TemplateFBAOI
            TestHelper.TextboxSetValue(actionSelect, "TemplateLadderAOI");

            // Resolve clashing, select replace
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
            TestHelper.LoadDefault();

            // Modify button
            Button modifyButton = actionSelect.FindAllDescendants(val => val.ByName("ModifyElementButton")).SingleOrDefault()?.AsButton();
            modifyButton.Invoke();

            // Select AddOnInstruction to modify
            Window dropdown = TestHelper.GetStandardInput(TestHelper.InputType.Dropdown);

            ListBoxItem typesListItem = dropdown.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("AddOnInstructionDefinition"))).SingleOrDefault()?.AsListBoxItem();
            TestReport.IsNotNull(typesListItem, "Failed to find element in list");

            typesListItem.DoubleClick();

            // select TemplateFBAOI and TemplateLadderAOI to modify
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

            // Select attributes for TemplateFBAOI to modify
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

            // Set CreatedBy of TemplateFBAOI
            TestHelper.TextboxSetValue(actionSelect, "TestFBUser");

            // Set Name of TemplateFBAOI
            TestHelper.TextboxSetValue(actionSelect, "TestFBName");

            // Select routine subelement of templateFBAOI to modify
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

            // Close subelement select of routine
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

            // Close subelement select of TemplateLadderAOI
            Window ladderSubSelect = TestHelper.GetStandardInput(TestHelper.InputType.MultiSelect);
            Button ladderClose = ladderSubSelect.FindAllDescendants(win => win.ByControlType(ControlType.Button).And(win.ByName("Close"))).SingleOrDefault()?.AsButton();
            ladderClose.Invoke();

            TestHelper.WaitMilliseconds(shortTimeoutMS);

            // Schema errors
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

            // Click button to get file path
            filePathPane.FindAllDescendants(win => win.ByName("All locations")).SingleOrDefault().AsButton().Click();
            AutomationElement filePathEdit = fileExplorer.FindAllDescendants(win => win.ByControlType(ControlType.Edit).And(win.ByName("Address", PropertyConditionFlags.MatchSubstring))).SingleOrDefault();

            // Input path
            filePathEdit.AsTextBox().Enter(TestXMLsPath + "\n");

            // Select file to load
            AutomationElement[] files = fileExplorer.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByFrameworkId(FrameworkType.Win32.ToString()).Not()));

            AutomationElement templateBroken = files.SingleOrDefault(fil => fil.Name.Equals("TemplateProjectV1.L5X"));
            TestReport.IsNotNull(templateBroken, "Template file with error(TemplateProjectV1.L5X) not found.");

            // Select the broken template file as the file to load
            templateBroken.DoubleClick();

            // Select element type to import(AOI)
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

            // Wait for the tool to catch up
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
