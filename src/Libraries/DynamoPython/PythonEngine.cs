using System.Collections.Generic;
using System.Linq;
using Dynamo;

namespace DynamoPython
{
    public static class PythonEngine
    {
        public delegate FScheme.Value EvaluationDelegate(
            bool dirty, string script, IEnumerable<KeyValuePair<string, dynamic>> bindings,
            IEnumerable<KeyValuePair<string, FScheme.Value>> inputs);

        public delegate void DrawDelegate(FScheme.Value val, string id);

        public static EvaluationDelegate Evaluator;

        public static DrawDelegate Drawing;

        private static readonly DynPythonEngine Engine = new DynPythonEngine();

        static PythonEngine()
        {
            Evaluator =
                delegate(bool dirty, string script,
                         IEnumerable<KeyValuePair<string, dynamic>> bindings,
                         IEnumerable<KeyValuePair<string, FScheme.Value>> inputs)
                {
                    if (dirty)
                    {
                        Engine.ProcessCode(script);
                        dirty = false;
                    }

                    return Engine.Evaluate(PythonBindings.Bindings.Concat(bindings), inputs);
                };

            Drawing = delegate { };
        }
    }
}
