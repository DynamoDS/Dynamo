using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronPython.Hosting;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;

namespace PluginManager
{
    public static class PluginManagerIronPythonEvaluator
    {
        public static void EvaluatePythonFile(string path )
        {
            var engine = Python.CreateEngine();
            ScriptSource script = engine.CreateScriptSourceFromFile(path);
            CompiledCode code = script.Compile();
            ScriptScope scope = engine.CreateScope();
            code.Execute(scope);
        }
    }
}
