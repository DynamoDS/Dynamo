using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Utilities;

namespace DynamoPython
{
    public struct Binding
    {
        public string Symbol;
        public dynamic Value;

        public Binding(string sym, dynamic val)
        {
            this.Symbol = sym;
            this.Value = val;
        }
    }

    public static class PythonBindings
    {
        static PythonBindings()
        {
            Bindings = new HashSet<Binding>();
            Bindings.Add(new Binding("__dynamo__", dynSettings.Controller));
            
        }

        public static HashSet<Binding> Bindings { get; private set; }
    }

}
