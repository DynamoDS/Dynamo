using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronPython.Hosting;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using System.Windows.Forms;
using Dynamo.PluginManager;

namespace PluginManager
{
    public static class PluginManagerIronPythonEvaluator
    {
        public static void EvaluatePythonFile(string path, PluginManagerExtension pluginManagerContext)
        {
            try {
                var engine = Python.CreateEngine();
                ScriptSource script = engine.CreateScriptSourceFromFile(path);
                CompiledCode code = script.Compile();
                ScriptScope scope = engine.CreateScope();
               //pluginManagerContext.WorkspaceModel.
                scope.SetVariable("workspaceModel", pluginManagerContext.WorkspaceModel);
                scope.SetVariable("dynamoViewModel", pluginManagerContext.CommandExecutive);
               // System.Windows.MessageBox.Show(String.Format(pluginManagerContext.WorkspaceModel.FileName, MessageBoxButtons.OK, MessageBoxIcon.Warning));
                code.Execute(scope);
            }
            catch(Exception ex)
            {
                System.Windows.MessageBox.Show(String.Format(ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning));
            }
        }
    }
}
