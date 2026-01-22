using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace ThesisUnitTests
{
    [TestClass]
    public class GUITesting
    {
        private readonly Uri WindowsApplicationDriverUrl = new Uri("http://127.0.0.1:4723");
        private readonly string TestAppPath = @"C:\Users\i01505732\OneDrive - Endress+Hauser\Thesis Work\CodeProjects\ThesisProjectV1\bin\Debug\ThesisProjectV1.exe";
        protected static WindowsDriver session;
        public Process appProc;
        private TimeSpan timeoutInMS = TimeSpan.FromMilliseconds(10000);

        [TestInitialize]
        public void Startup()
        {

            appProc = Process.GetProcessesByName("ThesisProjectV1").FirstOrDefault();
            if (appProc == null)
            {
                appProc = Process.Start(TestAppPath);
                Assert.IsNotNull(appProc);
            }

            if (session == null)
            {
                Process windriver = new Process();
                windriver.StartInfo.FileName = @"C:\Program Files (x86)\Windows Application Driver\WinAppDriver.exe";
                windriver.StartInfo.CreateNoWindow = false;
                windriver.Start();

                string hexHandle = null;
                Stopwatch elapsed = Stopwatch.StartNew();
                while (hexHandle == null && elapsed.Elapsed < timeoutInMS)
                {
                    IntPtr mwhndl = appProc.MainWindowHandle;
                    hexHandle = mwhndl.ToString("x");
                    Thread.Sleep(25);
                }
                elapsed.Stop();

                Assert.IsTrue(elapsed.Elapsed < timeoutInMS, "Timed out getting ActionSelect");

                AppiumOptions options = new AppiumOptions
                {
                    //App = hexHandle,
                    PlatformName = "Windows",
                    
                };
                options.AddAdditionalAppiumOption("appTopLevelWindow", hexHandle);

                session = new WindowsDriver(WindowsApplicationDriverUrl, options);
                Assert.IsNotNull(session, "Failed to create Winforms session");
            }
        }

        [TestCleanup]
        public void Teardown()
        {
            session?.Quit();
            session = null;
            appProc?.Close();
        }

        [TestMethod]
        public void ActionSelectLayout()
        {
            session.FindElement(By.ClassName("ExitActionSelect")).Click();
            Assert.IsNull(appProc);
        }
    }
}
