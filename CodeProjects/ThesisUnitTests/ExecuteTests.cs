using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThesisProjectV1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Xml.Linq;

namespace ThesisProjectV1.Tests
{
    [TestClass()]
    public class ExecuteTests
    {
        #region Fields
        private XDocument validL5X = new XDocument();
        private XDocument invalidL5X = new XDocument();
        #endregion

        [TestMethod()]
        public void ValidateFileTest()
        {
            
            Assert.Fail();
        }
    }
}