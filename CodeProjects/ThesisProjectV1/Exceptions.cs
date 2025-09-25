using System;

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
}
