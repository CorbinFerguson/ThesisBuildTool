using Microsoft.VisualStudio.TestTools.UnitTesting;
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

            // Check that ActionSelect launches
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
        }

        [TestCleanup]
        public void Teardown()
        {
            appProc?.Close();
        }

        [TestMethod]
        public void ActionSelectLayout()
        {
            Assert.IsNull(appProc);
        }
    }
}
