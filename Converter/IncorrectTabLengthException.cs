// Copyright Stefan Nychka, BSD 3-Clause license, COPYRIGHT.txt
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Text2Html
{
    public class IncorrectTabLengthException : Exception
    {
        public IncorrectTabLengthException() : base("Incorrect number of spaces used")
        {
        }

        public IncorrectTabLengthException(string message)
            : base(message)
        {
        }

    }
}
