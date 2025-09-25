using System.Collections.Generic;
using System.Xml.Linq;

namespace ThesisProjectV1
{
    public class ElementHelper
    {
        public Queue<string> RootPath = null;

        public string ElementType = null;

        // Used for when generating a collection of programs with the same parent task
        public string BulkProgramParentGen = null;

        // Parent Elemtn for bulk generation
        public XElement ParentElementBulk = null;

        public ElementHelper()
        {
        }

        public ElementHelper(ElementHelper elementInfo)
        {
            RootPath = elementInfo.RootPath;
            ElementType = elementInfo.ElementType;
            BulkProgramParentGen = elementInfo.BulkProgramParentGen;
            ParentElementBulk = elementInfo.ParentElementBulk;
        }

        public ElementHelper(Queue<string> rootPath, string elementType, string collectionParent, XElement parentEl)
        {
            RootPath = rootPath;
            ElementType = elementType;
            BulkProgramParentGen = collectionParent;
            ParentElementBulk = parentEl;
        }

        public void ResetElements()
        {
            RootPath.Clear();
            ElementType = null;
            BulkProgramParentGen = null;
            ParentElementBulk = null;
        }
    }
}
