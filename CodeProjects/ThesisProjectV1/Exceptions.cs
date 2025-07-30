using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThesisProjectV1
{
    public class EmptyListException : Exception
    {
        public EmptyListException() { }

        // Constructor that takes a message
        public EmptyListException(string message) : base(message) { }

    }
}
