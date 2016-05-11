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
using Dynamo.Graph.Workspaces;
using System.IO;
using Dynamo.Views;
using Dynamo.ViewModels;

namespace PluginManager
{
    public static class PluginManagerIronPythonEvaluator
    {
        public static void EvaluatePythonString(string str, PluginManagerExtension pluginManagerContext)
        {
            var engine = Python.CreateEngine();
            try {
                
                ScriptSource script = engine.CreateScriptSourceFromString(str);
                CompiledCode code = script.Compile();
                ScriptScope scope = engine.CreateScope();
               //pluginManagerContext.WorkspaceModel.
                scope.SetVariable("workspaceModel", (WorkspaceModel) pluginManagerContext.WorkspaceModel);
                scope.SetVariable("commandExecutive", pluginManagerContext.CommandExecutive);
                scope.SetVariable("watch3DViewModel",pluginManagerContext.Watch3DViewModel);
                scope.SetVariable("Logger", pluginManagerContext.DynamoViewModel.Model.Logger);
                scope.SetVariable("dynamoViewModel", pluginManagerContext.DynamoViewModel);
                scope.SetVariable("dynamoView",pluginManagerContext.DynamoView);
                scope.SetVariable("workspaceView", pluginManagerContext.WorkspaceView);
                scope.SetVariable("workspaceViewModel", (WorkspaceViewModel)pluginManagerContext.DynamoViewModel.CurrentSpaceViewModel);
               // System.Windows.MessageBox.Show(String.Format(pluginManagerContext.WorkspaceModel.FileName, MessageBoxButtons.OK, MessageBoxIcon.Warning));
                code.Execute(scope);
            }
            catch(Exception e)
            {

                var eo = engine.GetService<ExceptionOperations>();
                string error = eo.FormatException(e);
                pluginManagerContext.DynamoViewModel.Model.Logger.Log(error);
//                System.Windows.MessageBox.Show(String.Format(ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning));
            }
        }
        public static void EvaluatePythonFile(string path, PluginManagerExtension pluginManagerContext)
        {
            EvaluatePythonString(File.ReadAllText(path), pluginManagerContext);
        }
    }
}
