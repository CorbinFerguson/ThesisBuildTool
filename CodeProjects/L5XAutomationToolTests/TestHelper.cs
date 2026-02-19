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
    internal class TestHelper
    {
        private static readonly int stdWait = 500;
        private static readonly int shortWait = 50;

        public enum InputType
        {
            Dropdown, MultiSelect, TextInput
        }

        public XNamespace ns;

        public TestHelper()
        {
            ns = XNamespace.Get(@"http://www.w3.org/2001/XMLSchema");
        }

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

        public static void WaitMilliseconds(int ms)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            while (sw.ElapsedMilliseconds < ms)
            {
            }
        }

        public static void Confirm(Window parent)
        {
            Button confirm = parent.FindAllDescendants(win => win.ByControlType(ControlType.Button).And(win.ByName("Confirm"))).SingleOrDefault()?.AsButton();
            TestReport.IsNotNull(confirm, "Confirm button not found in window " + parent.Name);
            confirm.Invoke();
        }

        public static void TextboxSetValue(Window parent, string inputText)
        {
            // Wait for window to appear
            WaitMilliseconds(stdWait);

            Window textWindow = parent.FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("TextInput"))).SingleOrDefault()?.AsWindow();
            TestReport.IsNotNull(textWindow, "Text input window not found.");

            // find the text input field
            TextBox textField = textWindow.FindAllDescendants(win => win.ByControlType(ControlType.Edit).And(win.ByName("TextField"))).SingleOrDefault()?.AsTextBox();
            TestReport.IsNotNull(textField, "Text input field not found.");

            // Submit a name that already exists
            textField.Click();
            textField.Enter("");
            textField.Enter(inputText);

            string foundInput = textField.Text;
            string errorMsg = string.Concat("Input quantity not equal to expected, found: ", foundInput);
            TestReport.IsTrue(foundInput.Equals(inputText), errorMsg);

            Confirm(textWindow);

            WaitMilliseconds(shortWait);

            // Check that expected quantity inserted
            TestReport.IsFalse(textWindow.IsEnabled, "Text window did not disappear");
        }

        public static void LoadDefault()
        {
            string TestXMLsPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "L5XFiles", "TestingFiles"));

            // LOAD FILE
            Button loadButton = GUITesting.actionSelect.FindAllDescendants(but => but.ByName("LoadFileButton")).SingleOrDefault()?.AsButton();

            loadButton.Invoke();
            TestHelper.WaitMilliseconds(stdWait);

            Window fileExplorer = GUITesting.actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("Open"))).SingleOrDefault()?.AsWindow();
            AutomationElement filePathPane = GUITesting.actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.ToolBar).And(win.ByName("Address:", PropertyConditionFlags.MatchSubstring))).SingleOrDefault();

            // Click button to get file path
            filePathPane.FindAllDescendants(win => win.ByName("All locations")).SingleOrDefault().AsButton().Click();
            AutomationElement filePathEdit = fileExplorer.FindAllDescendants(win => win.ByControlType(ControlType.Edit).And(win.ByName("Address", PropertyConditionFlags.MatchSubstring))).SingleOrDefault();

            // Input path
            filePathEdit.AsTextBox().Enter(TestXMLsPath + "\n");

            // Select file to load
            AutomationElement[] files = fileExplorer.FindAllDescendants(win => win.ByControlType(ControlType.ListItem).And(win.ByFrameworkId(FrameworkType.Win32.ToString()).Not()));
            AutomationElement loadedFile = files.SingleOrDefault(fil => fil.Name.Equals("TemplateProjectV1.L5X"));

            // Select the template file as the file to load
            loadedFile.DoubleClick();
        }

        public static Window GetStandardInput(InputType input)
        {
            // Wait for window to appear
            WaitMilliseconds(stdWait);

            switch (input)
            {
                case InputType.Dropdown:
                    Window dropdown = GUITesting.actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("DropdownGui"))).SingleOrDefault()?.AsWindow();
                    TestReport.IsNotNull(dropdown, "Dropdown Gui was not found");
                    return dropdown;
                case InputType.MultiSelect:
                    Window multiSelect = GUITesting.actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("MultiSelectDropdown"))).SingleOrDefault()?.AsWindow();
                    TestReport.IsNotNull(multiSelect, "MultiSelect dropdown was not found");
                    return multiSelect;
                case InputType.TextInput:
                    Window textInput = GUITesting.actionSelect.FindAllDescendants(win => win.ByControlType(ControlType.Window).And(win.ByName("TextInput"))).SingleOrDefault()?.AsWindow();
                    TestReport.IsNotNull(textInput, "Text input was not found");
                    return textInput;
                default:
                    throw new NotImplementedException();
            }
        }

        public static void CheckHomePage()
        {
            TestHelper.WaitMilliseconds(100);
            // Return to homepage
            TestReport.IsTrue(GUITesting.actionSelect.IsEnabled, "Did not return to homepage");
        }
    }
}
