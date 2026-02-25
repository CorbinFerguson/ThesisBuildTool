using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using Reporting;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace L5XAutomationToolTests
{
    /// <summary>
    /// Small, focused helpers for GUI-driven tests and XML test data creation.
    /// NOTE: Methods intentionally block/wait using polling because the AUT (app under test)
    /// relies on Windows UI timing; keep waits modest to avoid flakiness.
    /// </summary>
    internal class TestHelper
    {
        // Standardized small waits (milliseconds) to stabilize UI timing in tests.
        private static readonly int stdWait = 500;
        private static readonly int shortWait = 50;

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
        public XDocument CreateTestSchema()
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
                                new XAttribute("type", "ProgramsType")),
                            new XElement(ns + "element",
                                new XAttribute("name", "Tasks"),
                                new XAttribute("type", "TasksType"))),
                        new XElement(ns + "attribute",
                            new XAttribute("name", "Name"),
                            new XAttribute("use", "required"))),
                    new XElement(ns + "complexType",
                        new XAttribute("name", "ProgramsType"),
                        new XElement(ns + "sequence",
                            new XElement(ns + "element",
                                new XAttribute("name", "Program"),
                                new XAttribute("type", "ProgramType")))),
                    new XElement(ns + "complexType",
                        new XAttribute("name", "ProgramType"),
                        new XElement(ns + "attribute",
                            new XAttribute("name", "Name"),
                            new XAttribute("use", "required"))),
                    new XElement(ns + "complexType",
                        new XAttribute("name", "TasksType"),
                        new XElement(ns + "sequence",
                            new XElement(ns + "element",
                                new XAttribute("name", "Task"),
                                new XAttribute("type", "TaskType")))),
                    new XElement(ns + "complexType",
                        new XAttribute("name", "TaskType"),
                        new XElement(ns + "attribute",
                            new XAttribute("name", "Name"),
                            new XAttribute("use", "required"))),
                    new XElement(ns + "element",
                        new XAttribute("name", "Datatype"),
                        new XAttribute("type", "DatatypeType")),
                    new XElement(ns + "complexType",
                        new XAttribute("name", "DatatypeType"))
                )
            );
        }

        /// <summary>
        /// Creates a compact, valid L5X document for smoke tests that need a baseline file.
        /// </summary>
        public XDocument CreateBasicTestDocument()
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

            TestReport.IsNotNull(confirm, "Confirm button not found in window " + parent.Name);
            confirm.Invoke();
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

            TestReport.IsNotNull(textWindow, "Text input window not found.");

            // Locate the input field inside the dialog.
            TextBox textField = textWindow
                .FindAllDescendants(win => win.ByControlType(ControlType.Edit).And(win.ByName("TextField")))
                .SingleOrDefault()
                ?.AsTextBox();

            TestReport.IsNotNull(textField, "Text input field not found.");

            // Enter and verify the text.
            textField.Click();
            textField.Enter(string.Empty);
            textField.Enter(inputText);

            string foundInput = textField.Text;
            string errorMsg = $"Input value not equal to expected, found: {foundInput}";
            TestReport.IsTrue(foundInput.Equals(inputText), errorMsg);

            // Submit and ensure the dialog closes.
            Confirm(textWindow);
            WaitMilliseconds(shortWait);
            TestReport.IsFalse(textWindow.IsEnabled, "Text window did not disappear");
        }

        /// <summary>
        /// Loads a default L5X file into the AUT via its "Load File" flow.
        /// This navigates the native File Open dialog to select a known test template.
        /// </summary>
        public static void LoadDefault()
        {
            string TestXMLsPath = Path.GetFullPath(
                Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "L5XFiles", "TestingFiles"));

            // Open the File Open dialog.
            Button loadButton = GUITesting.actionSelect
                .FindAllDescendants(but => but.ByName("LoadFileButton"))
                .SingleOrDefault()
                ?.AsButton();

            loadButton.Invoke();
            TestHelper.WaitMilliseconds(stdWait);

            // Target the Windows "Open" dialog and its address bar.
            Window fileExplorer = GUITesting.actionSelect
                .FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("Open")))
                .SingleOrDefault()
                ?.AsWindow();

            AutomationElement filePathPane = GUITesting.actionSelect
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

            filePathEdit.AsTextBox().Enter(TestXMLsPath + "\\n");

            // Choose the specific template file and open it.
            AutomationElement[] files = fileExplorer
                .FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByFrameworkId(FrameworkType.Win32.ToString()).Not()));

            AutomationElement loadedFile = files.SingleOrDefault(fil => fil.Name.Equals("TemplateProjectV1.L5X"));

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
                    {
                        Window dropdown = GUITesting.actionSelect
                            .FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("DropdownGui")))
                            .SingleOrDefault()
                            ?.AsWindow();

                        TestReport.IsNotNull(dropdown, "Dropdown Gui was not found");
                        return dropdown;
                    }

                case InputType.MultiSelect:
                    {
                        Window multiSelect = GUITesting.actionSelect
                            .FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("MultiSelectDropdown")))
                            .SingleOrDefault()
                            ?.AsWindow();

                        TestReport.IsNotNull(multiSelect, "MultiSelect dropdown was not found");
                        return multiSelect;
                    }

                case InputType.TextInput:
                    {
                        Window textInput = GUITesting.actionSelect
                            .FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("TextInput")))
                            .SingleOrDefault()
                            ?.AsWindow();

                        TestReport.IsNotNull(textInput, "Text input was not found");
                        return textInput;
                    }

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
            TestReport.IsTrue(GUITesting.actionSelect.IsEnabled, "Did not return to homepage");
        }

        #endregion
    }
}