using Dynamo.Graph.Workspaces;
using Dynamo.Logging;
using Dynamo.Wpf.Extensions;
using PluginManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Dynamo.PluginManager
{
    public class PluginManagerExtension : IViewExtension, ILogSource
    {
        private ViewStartupParams startupParams;
        private ViewLoadedParams loadedParams;

        private Menu dynamoMenu;
        private MenuItem loadPythonScriptMenuItem;
        private Separator separator = new Separator();

        #region IViewExtension implementation


        public string Name { get { return "DynamoPluginManager"; } }

        public string UniqueId
        {
            get { return "7c936911-bd25-4ae5-bc78-5b96e86bb30d"; }
        }

        public void Startup(ViewStartupParams p)
        {
            this.startupParams = p;
        }

        public void Loaded(ViewLoadedParams p)
        {
            this.loadedParams = p;

            dynamoMenu = p.dynamoMenu;
            loadPythonScriptMenuItem = GenerateMenuItem();
            p.AddMenuItem(MenuBarType.File, loadPythonScriptMenuItem, 11);

            p.CurrentWorkspaceChanged += CurrentWorkspaceChanged;

        }

        private void CurrentWorkspaceChanged(IWorkspaceModel ws)
        {

        }

        public void Shutdown()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            loadedParams.CurrentWorkspaceChanged -= CurrentWorkspaceChanged;
        }
        #endregion

        #region ILogSource implementation
        public event Action<ILogMessage> MessageLogged;

        private void OnMessageLogged(ILogMessage msg)
        {
            if (this.MessageLogged != null)
            {
                this.MessageLogged(msg);
            }
        }

        #endregion
        #region Helper methods
        private MenuItem GenerateMenuItem()
        {
            MenuItem item = new MenuItem();
            //item.InputGestureText = "Alt+D";
            item.Header = "Load Python Script";
           //  = PluginManagerImportScript.ImportPythonScript();
           // item.CommandBindings.Add()
          


        // item.Click += (sender, args) => { PluginManagerImportScript.ImportPythonScript(); };
           return item;
        }
       

        #endregion


    }
}