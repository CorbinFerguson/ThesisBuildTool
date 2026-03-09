using System.Xml.Linq;

namespace L5XAutomationTool
{
    public class ElementHelper
    {
        #region Fields
        public LinkedList<string> RootPath = null;

        public string ElementType = null;

        // Used for when generating a collection of programs with the same parent task
        public string BulkProgramParentGen = null;

        // Parent Elemtn for bulk generation
        public XElement ParentElementBulk = null;
        #endregion

        #region Constructors
        public ElementHelper()
        {
        }

        public ElementHelper(ElementHelper elementInfo)
        {
            if (elementInfo.RootPath != null)
                RootPath = new LinkedList<string>(elementInfo.RootPath);
            ElementType = elementInfo.ElementType;
            BulkProgramParentGen = elementInfo.BulkProgramParentGen;
            if (elementInfo.ParentElementBulk != null)
                ParentElementBulk = new XElement(elementInfo.ParentElementBulk);
        }

        public ElementHelper(LinkedList<string> rootPath, string elementType, string collectionParent, XElement parentEl)
        {
            RootPath = rootPath;
            ElementType = elementType;
            BulkProgramParentGen = collectionParent;
            ParentElementBulk = parentEl;
        }

        #endregion

        #region Functions
        public void ResetElements()
        {
            RootPath.Clear();
            ElementType = null;
            BulkProgramParentGen = null;
            ParentElementBulk = null;
        }

        public string GetFirst()
        {
            string back = RootPath.First.Value;
            RootPath.RemoveFirst();
            return back;
        }
        #endregion
    }
}
