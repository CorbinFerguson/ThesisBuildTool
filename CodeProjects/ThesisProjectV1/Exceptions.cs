using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace ThesisProjectV1
{
    public class EmptyListException : Exception
    {
        // Constructor that takes a message
        public EmptyListException(string message) : base(message) { }

    }

    public class AbortedElementException : Exception
    {
        public AbortedElementException() { }
    }

    public class ClashingElementException : Exception
    {
        public IEnumerable<XElement> clashingElements;
        public XElement parentNode;
        public ClashingElementException(IEnumerable<XElement> clashingElem, XElement parent)
        {
            clashingElements = clashingElem;
            parentNode = parent;
        }
    }

    public class AmbiguousSchemaPathException : Exception
    {
        public readonly string ElementType;
        public readonly List<string> CandidateParentNames;

        public AmbiguousSchemaPathException(string elementType, List<string> candidateParentNames): base("Ambiguous schema path for element type '" + elementType + "'. " + "Caller must choose one of the candidate parents.")
        {
            ElementType = elementType;
            CandidateParentNames = candidateParentNames ?? new List<string>();
        }
    }

}
