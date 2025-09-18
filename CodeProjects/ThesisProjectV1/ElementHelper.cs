using System.Collections.Generic;

namespace ThesisProjectV1
{
    public class ElementHelper
    {
        public Queue<string> RootPath = null;

        public string ElementType = null;

        // Used for when generating a collection of programs with the same parent task
        public string BulkProgramParentGen = null;

        public ElementHelper()
        {
        }

        public ElementHelper(ElementHelper elementInfo)
        {
            RootPath = elementInfo.RootPath;
            ElementType = elementInfo.ElementType;
            BulkProgramParentGen= elementInfo.BulkProgramParentGen;
        }

        public ElementHelper(Queue<string> rootPath, string elementType, string collectionParent)
        {
            RootPath = rootPath;
            ElementType = elementType;
            BulkProgramParentGen = collectionParent;
        }

        public void ResetElements()
        {
            RootPath.Clear();
            ElementType = null;
            BulkProgramParentGen = null;
        }
    }
}
