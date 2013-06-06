using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo;
using Dynamo.Nodes;

namespace DynamoPython
{
    public static class PythonEngine
    {
        public delegate FScheme.Value EvaluationDelegate(bool dirty, string script, IEnumerable<Binding> bindings);
        public delegate void DrawDelegate(FScheme.Value val, RenderDescription rd);

        public static EvaluationDelegate Evaluator;

        public static DrawDelegate Drawing;

        private static DynPythonEngine engine = new DynPythonEngine();

        static PythonEngine()
        {
            Evaluator = delegate(bool dirty, string script, IEnumerable<Binding> bindings)
            {
                if (dirty)
                {
                    engine.ProcessCode(script);
                    dirty = false;
                }

                return engine.Evaluate(PythonBindings.Bindings.Concat(bindings));
            };

            Drawing = delegate(FScheme.Value val, RenderDescription rd) { };
        }
    }

}
