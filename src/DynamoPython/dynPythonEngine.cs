using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Dynamo;
using Dynamo.Nodes;
using Dynamo.Utilities;
using IronPython.Hosting;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;

namespace DynamoPython
{
    internal class DynPythonEngine
    {
        private ScriptEngine engine;
        private ScriptSource source;

        public DynPythonEngine()
        {
            this.engine = Python.CreateEngine();
        }

        public void ProcessCode(string code)
        {
            // check if we're in the revit context
            // if so, add relevant assemblies

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            if ( assemblies.Any(x => x.FullName.Contains("RevitAPI")) && assemblies.Any(x => x.FullName.Contains("RevitAPIUI")) )
            {
                code = "import clr\nclr.AddReference('RevitAPI')\nclr.AddReference('RevitAPIUI')\nfrom Autodesk.Revit.DB import *\nimport Autodesk\n" + code;
            } 

            this.source = engine.CreateScriptSourceFromString(code, SourceCodeKind.Statements);
        }

        public FScheme.Value Evaluate(IEnumerable<Binding> bindings)
        {
            var scope = this.engine.CreateScope();

            foreach (var bind in bindings)
            {
                scope.SetVariable(bind.Symbol, bind.Value);
            }

            try
            {
                this.source.Execute(scope);
            }
            catch (Exception e)
            {
                var eo = engine.GetService<ExceptionOperations>();
                string error = eo.FormatException(e);
                return FScheme.Value.NewString(error);
            }

            FScheme.Value result = FScheme.Value.NewNumber(1);

            if (scope.ContainsVariable("OUT"))
            {
                dynamic output = scope.GetVariable("OUT");

                result = Converters.convertToValue(output);
            }

            return result;
        }
    }
}
