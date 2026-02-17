using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA2;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        private readonly string TestAppPath = typeof(L5XAutomationTool.Program).Assembly.Location;
        private readonly int longTimeoutMS = 2000;
        private readonly int shortTimeoutMS = 500;
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
            actionSelect = app.GetAllTopLevelWindows(automation).First();
            Assert.IsNotNull(actionSelect, "Find ActionSelect window");
        }

        [TestCleanup]
        public void Teardown()
        {
            app?.Close();
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
            TestHelper.WaitMilliseconds(longTimeoutMS);
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
            TestHelper.WaitMilliseconds(500);

            Window fileExplorer = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("Open"))).SingleOrDefault()?.AsWindow();
            Assert.IsNotNull(fileExplorer, "File explorer not found");

            AutomationElement filePathPane = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.ToolBar).And(win.ByName("Address:", PropertyConditionFlags.MatchSubstring))).SingleOrDefault();

            // Click button to get file path
            filePathPane.FindAllDescendants(win => win.ByName("All locations")).SingleOrDefault().AsButton().Click();
            AutomationElement filePathEdit = fileExplorer.FindAllDescendants(win => win.ByControlType(ControlType.Edit).And(win.ByName("Address", PropertyConditionFlags.MatchSubstring))).SingleOrDefault();

            // Input path
            filePathEdit.AsTextBox().Enter(TestXMLsPath + "\n");

            // Select file to load
            AutomationElement[] files = fileExplorer.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByFrameworkId(FrameworkType.Win32.ToString()).Not()));

            AutomationElement templateBroken = files.SingleOrDefault(fil => fil.Name.Equals("BrokenXML.L5X"));
            Assert.IsNotNull(templateBroken, "Template file with error(BrokenXML.L5X) not found");

            // Select the broken template file as the file to load
            templateBroken.DoubleClick();

            // Should return to homepage
            Assert.IsTrue(actionSelect.IsAvailable, "Did not return to homepage");

            // Sleep for so the tool can catch up
            TestHelper.WaitMilliseconds(100);

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

            // Click button to get file path
            filePathPane.FindAllDescendants(win => win.ByName("All locations")).SingleOrDefault().AsButton().Click();

            filePathEdit = fileExp.FindAllDescendants(win => win.ByControlType(ControlType.Edit).And(win.ByName("Address", PropertyConditionFlags.MatchSubstring))).SingleOrDefault();

            string saveResultPath = Path.Combine(TestXMLsPath, "TestResults");
            // Input path
            filePathEdit.AsTextBox().Enter(saveResultPath + "\n");

            // Set file name
            AutomationElement fileName = fileExp.FindAllDescendants(win => win.ByControlType(ControlType.ComboBox).And(win.ByName("File name:"))).SingleOrDefault()?.AsTextBox();

            string nameToSave = "TestSaveResult";
            for (int i = 0; File.Exists(Path.Combine(saveResultPath, nameToSave + ".L5X")); i++)
                nameToSave = Regex.Replace(nameToSave, @"\d", string.Empty) + i.ToString();

            fileName.Click();
            Keyboard.Type(nameToSave + "\n");

            // Wait for the tool to catch up
            TestHelper.WaitMilliseconds(250);

            Assert.IsTrue(File.Exists(Path.Combine(saveResultPath, nameToSave + ".L5X")), "File not saved at location");

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
            TestHelper.WaitMilliseconds(shortTimeoutMS);

            // Select element type to create
            Window elementTypeSelect = TestHelper.GetStandardInput(TestHelper.InputType.Dropdown);

            AutomationElement selectedItem = elementTypeSelect.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("AddOnInstructionDefinition"))).SingleOrDefault();
            Assert.IsNotNull(selectedItem, "Desired element type not found in dropdown.");

            selectedItem.DoubleClick();

            // Select element template to use
            Window elementTemplate = TestHelper.GetStandardInput(TestHelper.InputType.Dropdown);

            AutomationElement selectedTemplate = elementTemplate.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("TemplateFBAOI"))).SingleOrDefault();
            Assert.IsNotNull(selectedTemplate, "Desired element type not found in dropdown.");

            selectedTemplate.Click();

            Button confirm = elementTemplate.FindAllDescendants(win => win.ByControlType(ControlType.Button).And(win.ByName("Confirm"))).SingleOrDefault()?.AsButton();
            Assert.IsNotNull(confirm, "Confirm button not found in dropdown");

            confirm.Invoke();

            // Wait for next window to appear
            TestHelper.WaitMilliseconds(longTimeoutMS);

            // Input quantity to create
            int quantityToAdd = 2;
            TestHelper.TextboxSetValue(actionSelect, quantityToAdd.ToString());

            for (int i = 0; i < quantityToAdd; i++)
            {
                TestHelper.WaitMilliseconds(shortTimeoutMS);
                AutomationElement nameInput = TestHelper.GetStandardInput(TestHelper.InputType.TextInput);

                // find the text input field
                TextBox nameField = nameInput.FindAllDescendants(win => win.ByControlType(ControlType.Edit).And(win.ByName("TextField"))).SingleOrDefault()?.AsTextBox();
                Assert.IsNotNull(nameField, "Text input field for name not found. Run: " + i);

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
                    Assert.IsTrue(foundInput.Equals("TestSaveFile" + i), errorMsg);

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
                    Assert.IsTrue(foundInput.Equals("TestSaveFile" + i), errorMsg);

                    Button submit = nameInput.FindAllDescendants(win => win.ByControlType(ControlType.Button).And(win.ByName("Submit"))).SingleOrDefault()?.AsButton();
                    Assert.IsNotNull(submit, "Confirm button not found in text input for name select");
                    submit.Invoke();
                }
            }

            // Should return to homepage
            Assert.IsTrue(actionSelect.IsAvailable, "Did not return to homepage");
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
            Assert.IsNotNull(selectedItem, "Desired element type not found in dropdown.");

            selectedItem.DoubleClick();

            // Select module template to use
            Window elementTemplate = TestHelper.GetStandardInput(TestHelper.InputType.Dropdown);

            AutomationElement selectedTemplate = elementTemplate.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("1756-IA16"))).SingleOrDefault();
            Assert.IsNotNull(selectedTemplate, "Desired element type not found in dropdown.");

            selectedTemplate.Click();

            Button confirm = elementTemplate.FindAllDescendants(win => win.ByControlType(ControlType.Button).And(win.ByName("Confirm"))).SingleOrDefault()?.AsButton();
            Assert.IsNotNull(confirm, "Confirm button not found in dropdown");

            // Input quantity to create
            int quantityToAdd = 2;
            TestHelper.TextboxSetValue(actionSelect, quantityToAdd.ToString());

            // Should return to homepage
            Assert.IsTrue(actionSelect.IsAvailable, "Did not return to homepage");
        }

        [TestMethod]
        [TestCategory("GUI_Create")]
        [TestProperty("Description",
        "Test that the standard creation of multiple elements works.")]
        public void RedundantElementCreation()
        {
            // CREATE
            Button createElementButton = actionSelect.FindAllDescendants(val => val.ByName("CreateElementButton")).SingleOrDefault()?.AsButton();

            // Begin element creation
            createElementButton.Invoke();
            TestHelper.WaitMilliseconds(shortTimeoutMS);

            // Select element type to create
            Window elementTypeSelect = TestHelper.GetStandardInput(TestHelper.InputType.Dropdown);

            AutomationElement selectedItem = elementTypeSelect.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("AddOnInstructionDefinition"))).SingleOrDefault();
            Assert.IsNotNull(selectedItem, "Desired element type not found in dropdown.");

            selectedItem.DoubleClick();

            // Select element template to use
            Window elementTemplate = TestHelper.GetStandardInput(TestHelper.InputType.Dropdown);

            AutomationElement selectedTemplate = elementTemplate.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("TemplateFBAOI"))).SingleOrDefault();
            Assert.IsNotNull(selectedTemplate, "Desired element type not found in dropdown.");

            selectedTemplate.Click();

            Button confirm = elementTemplate.FindAllDescendants(win => win.ByControlType(ControlType.Button).And(win.ByName("Confirm"))).SingleOrDefault()?.AsButton();
            Assert.IsNotNull(confirm, "Confirm button not found in dropdown");

            confirm.Invoke();

            // Wait for next window to appear
            TestHelper.WaitMilliseconds(longTimeoutMS);

            // Input quantity to create
            int quantityToAdd = 4;
            TestHelper.TextboxSetValue(actionSelect, quantityToAdd.ToString());


            for (int i = 0; i < quantityToAdd; i++)
            {
                TestHelper.WaitMilliseconds(shortTimeoutMS);

                TestHelper.TextboxSetValue(actionSelect, "TestSaveFile");

                if (i != 0)
                {
                    TestHelper.WaitMilliseconds(shortTimeoutMS);
                    // Test each of the clashing element resolution methods
                    Window resolutionStyle = TestHelper.GetStandardInput(TestHelper.InputType.Dropdown);

                    if (i == 1)
                    {
                        // Resolution style: Replace
                        ListBoxItem replace = resolutionStyle.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("Replace"))).SingleOrDefault()?.AsListBoxItem();
                        Assert.IsNotNull(replace, "Replace item not found in clash resolution");
                        replace.DoubleClick();
                    }
                    else if (i == 2)
                    {
                        // Resolution style: Cancel
                        ListBoxItem cancel = resolutionStyle.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("Cancel"))).SingleOrDefault()?.AsListBoxItem();
                        Assert.IsNotNull(cancel, "Cancel item not found in clash resolution");
                        cancel.DoubleClick();
                    }

                    else if (i == 3)
                    {
                        // Resolution style: Rename

                        TestHelper.TextboxSetValue(actionSelect, "TestSaveFile");

                        TestHelper.TextboxSetValue(actionSelect, "TestSaveFile2");
                    }
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
            List<string> prohibNames = new List<string>() { "ExitActionSelect", "Minimize", "Maximize", "Close" };
            IEnumerable<AutomationElement> mainMenuButtons = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Button)).SkipWhile(but => prohibNames.Contains(but.Name));

            foreach (AutomationElement button in mainMenuButtons)
            {
                // Press button to open new window
                button.AsButton().Invoke();

                // New window should appear
                Window popup = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Window)).SingleOrDefault()?.AsWindow();
                Assert.IsNotNull(popup, "Popup window not found");

                // ActionSelect should not be available
                Assert.IsFalse(actionSelect.IsAvailable, "Action select did not yield control");

                // Close window
                Button close = popup.FindAllDescendants(win => win.ByName("Close")).SingleOrDefault().AsButton();
                close.Invoke();
            }
        }

        [TestMethod]
        [TestCategory("GUI_Delete")]
        [TestProperty("Description",
        "Test that modules can be deleted using the port as a filter instead of name.")]
        public void DeleteModuleByPort()
        {
            TestHelper.LoadDefault(actionSelect);

            // Delete button
            Button deleteButton = actionSelect.FindAllDescendants(val => val.ByName("DeleteElementButton")).SingleOrDefault()?.AsButton();
            deleteButton.Invoke();

            // Select Module to delete
            Window dropdown = TestHelper.GetStandardInput(TestHelper.InputType.Dropdown);

            ListBoxItem typesListItem = dropdown.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("Module"))).SingleOrDefault()?.AsListBoxItem();
            Assert.IsNotNull(typesListItem, "Failed to find element in list");

            typesListItem.DoubleClick();

            // Select module '1756-IA16' to delete
            string moduleName = "1756-IA16";
            Window deleteName = TestHelper.GetStandardInput(TestHelper.InputType.Dropdown);

            ListBoxItem moduleType = deleteName.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName(moduleName))).SingleOrDefault()?.AsListBoxItem();
            Assert.IsNotNull(moduleType, "Module of type " + moduleName + " not found");
            moduleType.DoubleClick();

            // select element at port X to delete
            Window modulePortDelWin = TestHelper.GetStandardInput(TestHelper.InputType.Dropdown);

            AutomationElement module = modulePortDelWin.FindAllDescendants(win => win.ByControlType(ControlType.ListItem)).First();
            Assert.IsNotNull(module, "Module list item not found");
            module.DoubleClick();

            // Detect errors from erroneously deleted element
            Window detectedErrors = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("Detected Errors"))).SingleOrDefault()?.AsWindow();
            Assert.IsNotNull(detectedErrors, "No popup for erroneous deletion of elements");

            string errorMsg = detectedErrors.FindAllDescendants(win => win.ByControlType(ControlType.Text)).SingleOrDefault()?.AsTextBox().Name;
            Assert.IsTrue(errorMsg.Contains("WARNING:"), "No 'warning' found in text of popup");

            Button oK = detectedErrors.FindAllDescendants(win => win.ByControlType(ControlType.Button)).SingleOrDefault()?.AsButton();
            Assert.IsNotNull(oK, "Ok button not found in error popup");
            oK.Invoke();

            // Should return to homepage
            Assert.IsTrue(actionSelect.IsAvailable, "Did not return to homepage");
        }

        [TestMethod]
        [TestCategory("GUI_Delete")]
        [TestProperty("Description",
        "Test that deleting an element that exists in the file and has no abnormal behavior works.")]
        public void DeleteElementFound()
        {
            TestHelper.LoadDefault(actionSelect);

            // Delete button
            Button deleteButton = actionSelect.FindAllDescendants(val => val.ByName("DeleteElementButton")).SingleOrDefault()?.AsButton();
            deleteButton.Invoke();

            // Select AddOnInstruction to delete
            Window dropdown = TestHelper.GetStandardInput(TestHelper.InputType.Dropdown);

            ListBoxItem typesListItem = dropdown.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("AddOnInstructionDefinition"))).SingleOrDefault()?.AsListBoxItem();
            Assert.IsNotNull(typesListItem, "Failed to find element in list");

            typesListItem.DoubleClick();

            // select TemplateFBAOI and TemplateLadderAOI to delete
            Window deleteElems = TestHelper.GetStandardInput(TestHelper.InputType.Dropdown);

            ListBoxItem[] delElems = deleteElems.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("TemplateFBAOI").Or(win.ByName("TemplateLadderAOI")))).Cast<ListBoxItem>().ToArray();
            Assert.IsTrue(delElems.Length == 2, "Did not find two elements to delete");

            foreach (ListBoxItem elem in delElems)
            {
                using (Keyboard.Pressing(VirtualKeyShort.LSHIFT))
                {
                    elem.Click();
                }
            }

            // Detect errors from erroneously deleted element
            Window detectedErrors = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("Detected Errors"))).SingleOrDefault()?.AsWindow();
            Assert.IsNotNull(detectedErrors, "No popup for erroneous deletion of elements");

            string errorMsg = detectedErrors.FindAllDescendants(win => win.ByControlType(ControlType.Text)).SingleOrDefault()?.AsTextBox().Name;
            Assert.IsTrue(errorMsg.Contains("WARNING:"), "No 'warning' found in text of popup");

            Button oK = detectedErrors.FindAllDescendants(win => win.ByControlType(ControlType.Button)).SingleOrDefault()?.AsButton();
            Assert.IsNotNull(oK, "Ok button not found in error popup");
            oK.Invoke();

            // Should return to homepage
            Assert.IsTrue(actionSelect.IsAvailable, "Did not return to homepage");
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
            Assert.IsNotNull(errorDialog, "Error window failed to appear");

            string errorMsg = errorDialog.FindAllDescendants(win => win.ByControlType(ControlType.Text)).SingleOrDefault()?.ToString();
            Assert.IsTrue(errorMsg.Equals("No elements found"), "Error msg does not contain 'No elements found', instead: " + errorMsg);

            Button oK = errorDialog.FindAllDescendants(win => win.ByControlType(ControlType.Button)).SingleOrDefault()?.AsButton();
            Assert.IsNotNull(oK, "oK button not found");

            oK.Invoke();

            // Should return to homepage
            Assert.IsTrue(actionSelect.IsAvailable, "Did not return to homepage");
        }

        [TestMethod]
        [TestCategory("GUI_Modify")]
        [TestProperty("Description",
        "Test the modification of a normal behaving element.")]
        public void StandardElementModify()
        {
            TestHelper.LoadDefault(actionSelect);

            // Modify button
            Button modifyButton = actionSelect.FindAllDescendants(val => val.ByName("ModifyElementButton")).SingleOrDefault()?.AsButton();
            modifyButton.Invoke();

            // Select AddOnInstruction to modify
            Window dropdown = TestHelper.GetStandardInput(TestHelper.InputType.Dropdown);

            ListBoxItem typesListItem = dropdown.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("AddOnInstructionDefinition"))).SingleOrDefault()?.AsListBoxItem();
            Assert.IsNotNull(typesListItem, "Failed to find element in list");

            typesListItem.DoubleClick();

            // select TemplateFBAOI and TemplateLadderAOI to modify
            Window modifyElems = TestHelper.GetStandardInput(TestHelper.InputType.MultiSelect);

            AutomationElement[] templateElemes = modifyElems.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("TemplateFBAOI").Or(win.ByName("TemplateLadderAOI"))));
            Assert.IsTrue(templateElemes?.Length == 2, "Template elements in dropdown not found. Found: " + string.Join(", ", templateElemes.Select(a => a.Name)));
            foreach (AutomationElement templateSelect in templateElemes)
            {
                templateSelect.Click();
            }

            Button submitTemplate = modifyElems.FindAllDescendants(win => win.ByControlType(ControlType.Button).And(win.ByName("Confirm"))).SingleOrDefault()?.AsButton();
            Assert.IsNotNull(submitTemplate, "Confirm button not found in text input for name select");
            submitTemplate.Invoke();

            // Select attributes for TemplateFBAOI to modify
            Window attrmod = TestHelper.GetStandardInput(TestHelper.InputType.MultiSelect);

            AutomationElement[] attrs = attrmod.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("Name").Or(win.ByName("CreatedBy"))));
            Assert.IsTrue(attrs?.Length == 2, "Attributes in dropdown not found. Found: " + string.Join(", ", attrs.Select(a => a.Name)));
            foreach (AutomationElement templateAttribute in attrs)
            {
                templateAttribute.Click();
            }

            Button submit = attrmod.FindAllDescendants(win => win.ByControlType(ControlType.Button).And(win.ByName("Confirm"))).SingleOrDefault()?.AsButton();
            Assert.IsNotNull(submit, "Confirm button not found in text input for name select");
            submit.Invoke();

            // Set CreatedBy of TemplateFBAOI
            TestHelper.TextboxSetValue(actionSelect, "TestFBUser");

            // Set Name of TemplateFBAOI
            TestHelper.TextboxSetValue(actionSelect, "TestFBName");

            // Select routine subelement of templateFBAOI to modify
            Window subElem = TestHelper.GetStandardInput(TestHelper.InputType.MultiSelect);

            ListBoxItem subElement = subElem.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("Routines"))).SingleOrDefault()?.AsListBoxItem();
            Assert.IsNotNull(subElement, "Subelement Routines not found");
            subElement.Click();

            Button subelementSubmit = attrmod.FindAllDescendants(win => win.ByControlType(ControlType.Button).And(win.ByName("Confirm"))).SingleOrDefault()?.AsButton();
            Assert.IsNotNull(subelementSubmit, "Confirm button not found in text input for name select");
            subelementSubmit.Invoke();

            // Select attributes of routine subelement to modify
            Window subAttr = TestHelper.GetStandardInput(TestHelper.InputType.MultiSelect);

            ListBoxItem attribute = subAttr.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("Name"))).SingleOrDefault()?.AsListBoxItem();
            Assert.IsNotNull(attribute, "Attributes in dropdown not found");
            attribute.Click();

            Button submitSub = subAttr.FindAllDescendants(win => win.ByControlType(ControlType.Button).And(win.ByName("Confirm"))).SingleOrDefault()?.AsButton();
            Assert.IsNotNull(submitSub, "Confirm button not found in text input for name select");
            submitSub.Invoke();

            // Set Name of routine subelement
            TestHelper.TextboxSetValue(actionSelect, "BrokenLogic");

            // Close subelement select of routine
            Window subSelect = TestHelper.GetStandardInput(TestHelper.InputType.MultiSelect);
            Button close = subSelect.FindAllDescendants(win => win.ByControlType(ControlType.Button).And(win.ByName("Close"))).SingleOrDefault()?.AsButton();
            close.Invoke();

            // Select attributes of TemplateLadderAOI to modify
            Window ladderAttrs = TestHelper.GetStandardInput(TestHelper.InputType.MultiSelect);

            ListBoxItem ladderAttribute = ladderAttrs.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByName("Name"))).SingleOrDefault()?.AsListBoxItem();
            Assert.IsNotNull(ladderAttribute, "Attributes in dropdown not found");
            ladderAttribute.Click();

            Button submitLadder = ladderAttrs.FindAllDescendants(win => win.ByControlType(ControlType.Button).And(win.ByName("Confirm"))).SingleOrDefault()?.AsButton();
            Assert.IsNotNull(submitLadder, "Confirm button not found in text input for name select");
            submitLadder.Invoke();

            // Set CreatedBy of TemplateLadderAOI
            TestHelper.TextboxSetValue(actionSelect, "BrokenLogic");

            // Close subelement select of TemplateLadderAOI
            Window ladderSubSelect = TestHelper.GetStandardInput(TestHelper.InputType.MultiSelect);
            Button ladderClose = ladderSubSelect.FindAllDescendants(win => win.ByControlType(ControlType.Button).And(win.ByName("Close"))).SingleOrDefault()?.AsButton();
            ladderClose.Invoke();

            // Schema errors
            Window popupSchema = actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("WARNING:", PropertyConditionFlags.MatchSubstring))).SingleOrDefault()?.AsWindow();
            Assert.IsNotNull(popupSchema, "Schema Error popup failed to appear");

            // Return to homepage
            Assert.IsTrue(actionSelect.IsAvailable, "Did not return to homepage");
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
            Assert.IsNotNull(popupNoElements, "Popup window not found");

            AutomationElement popupText = popupNoElements.FindAllDescendants(win => win.ByControlType(ControlType.Text)).SingleOrDefault();
            Assert.IsNotNull(popupText, "Error message text not found");
            Assert.IsTrue(popupText.Equals("No Valid Elements Found"), "Error Message for modify empty element not equal to expected");

            Button ok = popupNoElements.FindAllDescendants(win => win.ByControlType(ControlType.Button).And(win.ByName("Ok"))).SingleOrDefault()?.AsButton();
            Assert.IsNotNull(ok, "Ok button not found in popup");

            // Should return to homepage
            Assert.IsTrue(actionSelect.IsAvailable, "Did not return to homepage");
        }
    }
}
