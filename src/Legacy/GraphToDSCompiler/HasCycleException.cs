using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;

namespace GraphToDSCompiler
{
    /// <summary>
    /// A class for CycleExceptions
    /// </summary>
    class HasCycleException : System.ApplicationException
    {
        private List<Node> cycle;
        public HasCycleException(string message, List<Node> cycle)
            : base(message)
        {
            this.cycle = cycle;
        }
    }
}
