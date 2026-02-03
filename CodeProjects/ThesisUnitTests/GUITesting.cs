using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA2;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ThesisUnitTests
{
    [TestClass]
    public class GUITesting
    {
        private readonly string TestAppPath = typeof(ThesisProjectV1.Program).Assembly.Location;
        public Application app;
        private int timeoutMS = 5000;

        [TestInitialize]
        public void Startup()
        {
            app = FlaUI.Core.Application.Launch(TestAppPath);
        }

        [TestCleanup]
        public void Teardown()
        {
            app?.Close();
        }

        [TestMethod]
        [TestProperty("TestID", "15")]
        public void ActionSelectLayout()
        {
            using (var automation = new UIA2Automation())
            {
                Thread.Sleep(timeoutMS);
                Window actionSelect = app.GetAllTopLevelWindows(automation).First();

                Assert.IsNotNull(actionSelect, "Find ActionSelect window");

                AutomationElement[] buttons = actionSelect.FindAllDescendants(win => win.ByControlType(FlaUI.Core.Definitions.ControlType.Button));
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
        }
    }
}
