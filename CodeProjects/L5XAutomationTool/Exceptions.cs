using System.Xml.Linq;

namespace L5XAutomationTool
{
    public class EmptyListException(string message) : Exception(message)
    {
    }

    public class AbortedElementException : Exception
    {
        public AbortedElementException() { }
    }

    public class ClashingElementException(IEnumerable<XElement> clashingElem, XElement parent) : Exception
    {
        public IEnumerable<XElement> clashingElements = clashingElem;
        public XElement parentNode = parent;
    }

    public class ClashingParentException(IEnumerable<XElement> clashingElem, IEnumerable<XElement> clashingPar = null) : Exception
    {
        public IEnumerable<XElement> clashingElements = clashingElem;
        public IEnumerable<XElement> clashingParents = clashingPar;
    }

    public class AmbiguousSchemaPathException(string elementType, List<string> candidateParentNames) : Exception("Ambiguous schema path for element type '" + elementType + "'. " + "Caller must choose one of the candidate parents.")
    {
        public readonly string ElementType = elementType;
        public readonly List<string> CandidateParentNames = candidateParentNames ?? new List<string>();
    }

    public class ParentMissingException(XElement parentNode, IEnumerable<XElement> missingAttributes) : Exception
    {
        public XElement parentNode = parentNode;
        public IEnumerable<XElement> missingSchemaAttributes = missingAttributes;
    }

}
