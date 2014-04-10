// Copyright Stefan Nychka, BSD 3-Clause license, COPYRIGHT.txt
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Text2Html
{
    public class IncorrectTabIncreaseException : Exception
    {
        public IncorrectTabIncreaseException() : base("Increased by more than tab")
        {
        }

        public IncorrectTabIncreaseException(string message)
            : base(message)
        {
        }

    }
}
