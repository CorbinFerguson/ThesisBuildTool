using FlaUI.Core;
using FlaUI.UIA2;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace ThesisUnitTests
{
    [TestClass]
    public class GUITesting
    {
        private readonly Uri WindowsApplicationDriverUrl = new Uri("http://127.0.0.1:4723");
        private readonly string TestAppPath = typeof(ThesisProjectV1.Program).Assembly.Location;
        public Application app;

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
        public void ActionSelectLayout()
        {
            using (var automation = new UIA2Automation())
            {
                var actionSelect = app.GetAllTopLevelWindows(automation);
            }
            Assert.IsNull(app);
        }
    }
}
