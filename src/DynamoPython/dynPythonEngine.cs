using System;
using System.Collections.Generic;
using Dynamo;
using IronPython.Hosting;
using IronPython.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Python = IronPython.Hosting.Python;

namespace DynamoPython
{
    internal class DynPythonEngine
    {
        private readonly ScriptEngine _engine;
        private ScriptSource _source;

        public DynPythonEngine()
        {
            _engine = Python.CreateEngine();
        }

        public void ProcessCode(string code)
        {
            _source = _engine.CreateScriptSourceFromString(code, SourceCodeKind.Statements);
        }

        public FScheme.Value Evaluate(IEnumerable<KeyValuePair<string, dynamic>> bindings, IEnumerable<KeyValuePair<string, FScheme.Value>> inputs)
        {
            var scope = _engine.CreateScope();

            foreach (var bind in bindings)
            {
                scope.SetVariable(bind.Key, bind.Value);
            }

            var ops = scope.Engine.CreateOperations();

            foreach (var input in inputs)
            {
                scope.SetVariable(input.Key, Converters.convertFromValue(input.Value, ops));
            }

            try
            {
                _source.Execute(scope);
            }
            catch (Exception e)
            {
                var eo = _engine.GetService<ExceptionOperations>();
                string error = eo.FormatException(e);
                return FScheme.Value.NewString(error);
            }

            FScheme.Value result = FScheme.Value.NewNumber(1);

            if (scope.ContainsVariable("OUT"))
            {
                dynamic output = scope.GetVariable("OUT");

                result = Converters.convertToValue(output, ops);
            }

            return result;
        }
    }
}
