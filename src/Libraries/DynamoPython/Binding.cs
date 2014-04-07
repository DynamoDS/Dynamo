using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Utilities;

namespace DynamoPython
{
    public static class PythonBindings
    {
        static PythonBindings()
        {
            Bindings = new Dictionary<string, dynamic> { {"__dynamo__", dynSettings.Controller} };
        }

        public static Dictionary<string, dynamic> Bindings { get; private set; }
    }

}
