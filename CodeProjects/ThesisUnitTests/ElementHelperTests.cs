
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Xml.Linq;
using ThesisProjectV1;

namespace ThesisProjectV1.Tests
{
    [TestClass]
    public class ElementHelperTests
    {
        /// <summary>
        /// Verifies the default constructor initializes a "blank" helper.
        /// WHY: Characterizes the initial state used by XML insertion flows; 
        /// prevents future unintended defaults.
        /// </summary>
        [TestMethod]
        [TestCategory("ElementHelper")]
        public void Ctor_Default_InitializesToNullState()
        {
            var helper = new ElementHelper();

            Assert.IsNull(helper.RootPath);
            Assert.IsNull(helper.ElementType);
            Assert.IsNull(helper.BulkProgramParentGen);
            Assert.IsNull(helper.ParentElementBulk);
        }

        /// <summary>
        /// Ensures the copy constructor performs deep copies for mutable members (RootPath, ParentElementBulk)
        /// and preserves scalar values.
        /// WHY: Avoids shared references across operations that could cause subtle bugs during bulk insert.
        /// </summary>
        [TestMethod]
        [TestCategory("ElementHelper")]
        public void Ctor_Copy_PerformsDeepCopyForMutableFields()
        {
            var original = new ElementHelper
            {
                RootPath = new Queue<string>(new[] { "RSLogix5000Content", "Controller", "Programs" }),
                ElementType = "Program",
                BulkProgramParentGen = "Task1",
                ParentElementBulk = new XElement("Controller", new XAttribute("Name", "MainController"))
            };

            var clone = new ElementHelper(original);

            // Different queue instances, same values
            Assert.AreNotSame(original.RootPath, clone.RootPath);
            CollectionAssert.AreEqual(original.RootPath.ToArray(), clone.RootPath.ToArray());

            // Strings are copied by value (reference reuse acceptable)
            Assert.AreEqual("Program", clone.ElementType);
            Assert.AreEqual("Task1", clone.BulkProgramParentGen);

            // XElement deep copy
            Assert.AreNotSame(original.ParentElementBulk, clone.ParentElementBulk);
            Assert.AreEqual("MainController", clone.ParentElementBulk.Attribute("Name")?.Value);

            // Mutate original to ensure clone is independent
            original.RootPath.Enqueue("Routines");
            original.ParentElementBulk.SetAttributeValue("Name", "Changed");
            Assert.AreEqual("MainController", clone.ParentElementBulk.Attribute("Name")?.Value);
            CollectionAssert.AreEqual(new[] { "RSLogix5000Content", "Controller", "Programs" }, clone.RootPath.ToArray());
        }

        /// <summary>
        /// Verifies ResetElements clears everything and leaves RootPath empty (not null), 
        /// when RootPath has been initialized.
        /// WHY: Critical to avoid stale state during multi-element insert operations.
        /// </summary>
        [TestMethod]
        [TestCategory("ElementHelper")]
        public void ResetElements_WhenRootPathInitialized_ClearsAllState()
        {
            var helper = new ElementHelper
            {
                RootPath = new Queue<string>(new[] { "A", "B" }),
                ElementType = "Datatype",
                BulkProgramParentGen = "Task2",
                ParentElementBulk = new XElement("Programs", new XAttribute("Name", "P"))
            };

            helper.ResetElements();

            Assert.IsNotNull(helper.RootPath, "RootPath remains allocated but should be empty.");
            Assert.AreEqual(0, helper.RootPath.Count);
            Assert.IsNull(helper.ElementType);
            Assert.IsNull(helper.BulkProgramParentGen);
            Assert.IsNull(helper.ParentElementBulk);
        }

        /// <summary>
        /// Documents current behavior: calling ResetElements when RootPath is null throws.
        /// WHY: Characterization test to capture current edge behavior and support a future fix.
        /// </summary>
        [TestMethod]
        [TestCategory("ElementHelper")]
        [ExpectedException(typeof(System.NullReferenceException))]
        public void ResetElements_WhenRootPathIsNull_Throws()
        {
            var helper = new ElementHelper
            {
                RootPath = null,
                ElementType = "Tag"
            };

            // Current implementation calls RootPath.Clear() without a null check.
            helper.ResetElements();
        }

        /// <summary>
        /// Parameterized constructor sets all fields as provided.
        /// WHY: Ensures future changes don't accidentally drop inputs during construction.
        /// </summary>
        [TestMethod]
        [TestCategory("ElementHelper")]
        public void Ctor_Parameterized_SetsFields()
        {
            var q = new Queue<string>(new[] { "X", "Y" });
            var parent = new XElement("Modules");
            var helper = new ElementHelper(q, "Module", "ParentGen", parent);

            Assert.AreSame(q, helper.RootPath);
            Assert.AreEqual("Module", helper.ElementType);
            Assert.AreEqual("ParentGen", helper.BulkProgramParentGen);
            Assert.AreSame(parent, helper.ParentElementBulk);
        }
    }
}
