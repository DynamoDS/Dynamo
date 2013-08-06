using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.IO;
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

            string header = "";

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            if ( assemblies.Any(x => x.FullName.Contains("RevitAPI")) && assemblies.Any(x => x.FullName.Contains("RevitAPIUI")) )
            {
                header = header + "import clr\nclr.AddReference('RevitAPI')\nclr.AddReference('RevitAPIUI')\nfrom Autodesk.Revit.DB import *\nimport Autodesk\n";
            }

            if (assemblies.Any(x => x.FullName.Contains("LibGNet")))
            {
                string current_dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                header = header + "import sys\npath = r'C:\\Autodesk\\Dynamo\\Core'" + "\nexec_path = r'" + current_dir + "'\nsys.path.append(path)\nsys.path.append(exec_path)\nimport clr\nclr.AddReference('LibGNet')\nfrom Autodesk.LibG import *\n";
                
            }

            code = header + code;

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
