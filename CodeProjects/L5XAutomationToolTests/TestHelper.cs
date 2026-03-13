using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using Reporting;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace L5XAutomationToolTestHelpers
{
    /// <summary>
    /// Small, focused helpers for GUI-driven tests and XML test data creation.
    /// NOTE: Methods intentionally block/wait using polling because the AUT (app under test)
    /// relies on Windows UI timing; keep waits modest to avoid flakiness.
    /// </summary>
    internal class TestHelper
    {
        // Standardized small waits (milliseconds) to stabilize UI timing in tests.
        private static readonly int stdWait = 750;
        private static readonly int shortWait = 75;

        /// <summary>Common input dialog types exposed by the AUT.</summary>
        public enum InputType
        {
            Dropdown,
            MultiSelect,
            TextInput
        }

        // XSD namespace used when building minimal schemas for validation tests.
        public XNamespace ns;

        public TestHelper()
        {
            ns = XNamespace.Get(@"http://www.w3.org/2001/XMLSchema");
        }

        #region XML Test Data Builders

        /// <summary>
        /// Builds a minimal XSD schema representing a small subset of an L5X structure.
        /// Used by validation tests that require a schema without depending on external files.
        /// </summary>
        public static XDocument CreateTestSchema()
        {
            string schemaPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../../../../RSLogix5000_V35.xsd"));
            return XDocument.Load(schemaPath);
        }

        /// <summary>
        /// Creates a compact, valid L5X document for smoke tests that need a baseline file.
        /// </summary>
        public static XDocument CreateBasicTestDocument()
        {
            return new XDocument(
                new XElement("RSLogix5000Content",
                    new XElement("Controller",
                        new XAttribute("Name", "TestController"),
                        new XElement("AddOnInstructionDefinitions",
                            new XElement("AddOnInstructionDefinition",
                                new XAttribute("Name", "TestAOI"))
                        ),
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

        /// <summary>
        /// Creates an empty L5X document for tests that need an empty document to modify
        /// </summary>
        /// 
        public static XDocument CreateEmptyDocument()
        {
            return new XDocument(
                new XElement("RSLogix5000Content",
                    new XElement("Controller",
                        new XAttribute("Name", "TestController"))));
        }

        #endregion

        #region UI Timing and Common Interactions

        /// <summary>
        /// Busy-wait for the specified duration. Used in places where UI synchronization
        /// primitives are not available or reliable in the AUT.
        /// </summary>
        public static void WaitMilliseconds(int ms)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < ms)
            {
                // Intentionally empty loop to avoid Thread.Sleep side-effects in some runners.
            }
        }

        /// <summary>
        /// Clicks a standard "Confirm" button in the provided window and asserts presence.
        /// </summary>
        public static void Confirm(Window parent)
        {
            Button confirm = parent
                .FindAllDescendants(win => win.ByControlType(ControlType.Button).And(win.ByName("Confirm")))
                .SingleOrDefault()
                ?.AsButton();

            TestReport.IsNotNull(confirm, "Find confirm button in: " + parent.Name);
            confirm.Click();
            WaitMilliseconds(shortWait * 2);
        }

        /// <summary>
        /// Sets the value of a standard single-line text input dialog and confirms it.
        /// Verifies that the text echoed in the box matches the requested input.
        /// </summary>
        public static void TextboxSetValue(Window parent, string inputText)
        {
            // Locate the "TextInput" dialog window.
            WaitMilliseconds(stdWait);
            Window textWindow = parent
                .FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("TextInput")))
                .SingleOrDefault()
                ?.AsWindow();

            TestReport.IsNotNull(textWindow, "Verify TextInput window appears");

            // Locate the input field inside the dialog.
            TextBox textField = textWindow
                .FindAllDescendants(win => win.ByControlType(ControlType.Edit).And(win.ByName("TextField")))
                .SingleOrDefault()
                ?.AsTextBox();

            TestReport.IsNotNull(textField, "Verify the field for text input appears");

            // Enter and verify the text.
            textField.Click();
            textField.Enter(string.Empty);
            textField.Enter(inputText);

            string foundInput = textField.Text;
            string errorMsg = $"Input Expected: {inputText}  | Input found: {foundInput}";
            TestReport.IsTrue(foundInput.Equals(inputText), errorMsg);

            // Submit and ensure the dialog closes.
            Confirm(textWindow);
            WaitMilliseconds(shortWait);
        }

        /// <summary>
        /// Loads a default L5X file into the AUT via its "Load File" flow.
        /// This navigates the native File Open dialog to select a known test template.
        /// </summary>
        public static void LoadDefault()
        {
            string TestXMLsPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "L5XFiles", "TestingFiles"));

            // Open the File Open dialog.
            Button loadButton = GuiTesting.GUITesting.actionSelect
                .FindAllDescendants(but => but.ByName("LoadFileButton"))
                .SingleOrDefault()
                ?.AsButton();

            loadButton.Click();
            TestHelper.WaitMilliseconds(stdWait);

            // Target the Windows "Open" dialog and its address bar.
            Window fileExplorer = GuiTesting.GUITesting.actionSelect
                .FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("Open")))
                .SingleOrDefault()
                ?.AsWindow();

            AutomationElement filePathPane = GuiTesting.GUITesting.actionSelect
                .FindAllDescendants(win => win.ByControlType(ControlType.ToolBar).And(win.ByName("Address:", PropertyConditionFlags.MatchSubstring)))
                .SingleOrDefault();

            // Switch the address control to an editable state and enter the target folder.
            filePathPane.FindAllDescendants(win => win.ByName("All locations"))
                .SingleOrDefault()
                .AsButton()
                .Click();

            AutomationElement filePathEdit = fileExplorer
                .FindAllDescendants(win => win.ByControlType(ControlType.Edit).And(win.ByName("Address", PropertyConditionFlags.MatchSubstring)))
                .SingleOrDefault();

            filePathEdit.AsTextBox().Enter(TestXMLsPath);
            Keyboard.Press(VirtualKeyShort.ENTER);

            // Choose the specific template file and open it.
            AutomationElement[] files = fileExplorer
                .FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByFrameworkId(FrameworkType.Win32.ToString()).Not()));

            AutomationElement loadedFile = files.SingleOrDefault(fil => fil.Name.Equals("TemplateProjectV1.L5X")) ?? files.SingleOrDefault(fil => fil.Name.Equals("TemplateProjectV1"));

            loadedFile.DoubleClick();
        }

        /// <summary>
        /// Returns a handle to a standard input dialog (Dropdown / MultiSelect / TextInput),
        /// asserting that the expected window appears before returning.
        /// </summary>
        public static Window GetStandardInput(InputType input)
        {
            WaitMilliseconds(stdWait);

            switch (input)
            {
                case InputType.Dropdown:
                    Window dropdown = GuiTesting.GUITesting.actionSelect
                        .FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("DropdownGui")))
                        .SingleOrDefault()
                        ?.AsWindow();

                    TestReport.IsNotNull(dropdown, "Verify dropdown window appears");
                    return dropdown;

                case InputType.MultiSelect:
                    Window multiSelect = GuiTesting.GUITesting.actionSelect
                        .FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("MultiSelectDropdown")))
                        .SingleOrDefault()
                        ?.AsWindow();

                    TestReport.IsNotNull(multiSelect, "Verify multiselect dropdown window appears");
                    return multiSelect;

                case InputType.TextInput:
                    Window textInput = GuiTesting.GUITesting.actionSelect
                        .FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("TextInput")))
                        .SingleOrDefault()
                        ?.AsWindow();

                    TestReport.IsNotNull(textInput, "Verify TextInput window appears");
                    return textInput;

                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Verifies that the test has returned to the home screen (ActionSelect is enabled).
        /// Intended to be called after closing modal dialogs/flows.
        /// </summary>
        public static void CheckHomePage()
        {
            TestHelper.WaitMilliseconds(100);
            TestReport.IsTrue(GuiTesting.GUITesting.actionSelect.IsEnabled, "Verify control returns to homepage(Action Select)");
        }

        #endregion

        #region Screenshot handler

        public static string TakeScreenshot(string imgPath)
        {
            string imagePathAndName = Enumerable.Range(0, int.MaxValue).Select(i => Path.Combine(imgPath, $"FailImage_{i}.jpg")).First(p => !File.Exists(p));

            Directory.CreateDirectory(imgPath);

            AutomationElement screenshot = GuiTesting.GUITesting.actionSelect.FindFirstDescendant(win => win.ByControlType(ControlType.Window)) ?? GuiTesting.GUITesting.actionSelect;
            screenshot.CaptureToFile(imagePathAndName);

            return imagePathAndName;
        }

        #endregion
    }
}