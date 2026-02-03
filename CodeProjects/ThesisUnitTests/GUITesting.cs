using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.UIA2;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using ThesisProjectV1;

namespace ThesisUnitTests
{
    [TestClass]
    public class GUITesting
    {
        public Application app;
        private readonly string TestAppPath = typeof(ThesisProjectV1.Program).Assembly.Location;
        private readonly int timeoutMS = 5000;
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
            Thread.Sleep(timeoutMS);
            actionSelect = app.GetAllTopLevelWindows(automation).First();
            Assert.IsNotNull(actionSelect, "Find ActionSelect window");
        }

        [TestCleanup]
        public void Teardown()
        {
            app?.Close();
        }

        [TestMethod]
        [TestProperty("TestID", "15")]
        [TestProperty("Description", 
        "Test that the ActionSelect window launches with all necessary buttons, and that the exit button closes the window.")]
        public void ActionSelectLayout()
        {
            AutomationElement[] buttons = actionSelect.FindAllDescendants(win => win.ByControlType(FlaUI.Core.Definitions.ControlType.Button));
            foreach (Button button in buttons.Cast<Button>())
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
            AutomationElement exitButton = actionSelect.FindAllDescendants(but => but.ByName("ExitActionSelect")).Single();
            exitButton.AsButton().Invoke();

            // Application should close upon pressing exit button
            Thread.Sleep(timeoutMS);
            Assert.IsTrue(app.GetAllTopLevelWindows(automation).Length == 0, "Application failed to close");
            
        }

        [TestMethod]
        [TestProperty("TestID", "14")]
        [TestProperty("Description",
        "Test that the validate button creates popups upon button press.")]
        public void FileManagementTesting()
        {
            Button validateButton = actionSelect.FindAllDescendants(val => val.ByName("ValidateFileButton")).Single().AsButton();

            // Validate blank doc to test errorless doc
            validateButton.Invoke();

            // Get the popup
            AutomationElement popup = actionSelect.FindAllDescendants(win => win.ByControlType(FlaUI.Core.Definitions.ControlType.Window).And(win.ByName("Detected Errors"))).Single();
            Assert.IsNotNull(popup, "Popup not found");

            string noErrors = popup.FindAllDescendants(win => win.ByControlType(FlaUI.Core.Definitions.ControlType.Text)).Single().Name;
            Assert.IsTrue(noErrors.Equals("No errors detected!"), "Validation text box popup incorrect");

            Button ok = popup.FindAllDescendants(win => win.ByControlType(FlaUI.Core.Definitions.ControlType.Button).And(win.ByName("OK"))).Single().AsButton();
            Assert.IsNotNull(ok, "Ok button not found");
            ok.Invoke();

            Button loadButton = actionSelect.FindAllDescendants(but => but.ByName("LoadFileButton")).Single().AsButton();
            Assert.IsNotNull(loadButton, "Load button not found");

            loadButton.Invoke();

            Window fileExplorer = actionSelect.FindAllDescendants(win => win.ByControlType(FlaUI.Core.Definitions.ControlType.Window).And(win.ByName("Open"))).Single().AsWindow();
            Assert.IsNotNull(fileExplorer, "File explorer not found");

            AutomationElement filePathPane = fileExplorer.FindAllDescendants(win => win.ByControlType(FlaUI.Core.Definitions.ControlType.ToolBar).And(win.ByName("Address:", PropertyConditionFlags.MatchSubstring))).Single();

            // Input path
            filePathPane.Click();
            FlaUI.Core.Input.Keyboard.Type(TestXMLsPath + "\n");

            // Select file to load

        }
    }
}
