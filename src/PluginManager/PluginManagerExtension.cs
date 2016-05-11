using Dynamo.Extensions;
using Dynamo.Graph.Workspaces;
using Dynamo.Logging;
using Dynamo.PluginManager.View;
using Dynamo.PluginManager.ViewModel;
using Dynamo.Wpf.Extensions;
using Dynamo.Wpf.ViewModels.Watch3D;
using PluginManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using Dynamo.Controls;
using System.Collections.Generic;
using Dynamo.PluginManager.Model;
using System.Windows.Input;
using Dynamo.ViewModels;
using Dynamo.Views;

namespace Dynamo.PluginManager
{
    public class PluginManagerExtension : IViewExtension, ILogSource
    {
        private ViewStartupParams startupParams;
        private ViewLoadedParams loadedParams;
        internal HelixWatch3DViewModel Watch3DViewModel { get; set; }
        private PluginManagerViewModel pluginManagerViewModel;
        internal Menu dynamoMenu;
        private MenuItem pluginManagerMainMenuItem;
        private Separator separator = new Separator();
        internal DynamoView DynamoView;
        internal WorkspaceView WorkspaceView;
        private IWorkspaceModel workspaceModel;
        internal IWorkspaceModel WorkspaceModel
        {
            get { return workspaceModel; }
            private set { SetWorkSpaceModel(value); }
        }
        
        internal ICommandExecutive CommandExecutive { get; private set; }
        internal DynamoViewModel DynamoViewModel;

        /// <summary>
        /// Sets the workspace model property and updates event handlers accordingly.
        /// </summary>
        /// <param name="wsm">Workspace Model to set</param>
        private void SetWorkSpaceModel(IWorkspaceModel wsm)
        {
            workspaceModel = wsm;
        }
        #region IViewExtension implementation


        public string Name { get { return "DynamoPluginManager"; } }

        public string UniqueId
        {
            get { return "7c936911-bd25-4ae5-bc78-5b96e86bb30d"; }
        }

        public void Startup(ViewStartupParams p)
        {
            this.startupParams = p;
           pluginManagerViewModel = new PluginManagerViewModel(this);

        }

        public void Loaded(ViewLoadedParams p)
        {
            this.loadedParams = p;

            //TODO: Shift to GenerateMenuItem
            dynamoMenu = p.dynamoMenu;
            pluginManagerMainMenuItem = GenerateMenuItem();
            //p.AddMenuItem(MenuBarType.File, loadPythonScriptMenuItem, 11);
            dynamoMenu.Items.Add(pluginManagerMainMenuItem);
            MenuItem pluginManagerSetting = new MenuItem();

            pluginManagerSetting.Header = "Pluging Manager Settings";
            pluginManagerSetting.Click += ShowPluginManagerWindow;

            var dynamoItem = SearchForMenuItem();
            dynamoItem.Items.Add(pluginManagerSetting);


            p.CurrentWorkspaceChanged += CurrentWorkspaceChanged;

            CommandExecutive = p.CommandExecutive;
            WorkspaceModel = p.CurrentWorkspaceModel;
            Watch3DViewModel = (HelixWatch3DViewModel) p.BackgroundPreviewViewModel;
            DynamoViewModel = (DynamoViewModel) p.DynamoWindow.DataContext;
            DynamoView = (DynamoView) p.DynamoWindow;

            pluginManagerViewModel.ImportPlugins();
            
        }
        internal void AddPluginMenuItem(PluginModel pluginModel)
        {
            MenuItem newItem = new MenuItem();
            newItem.Header = pluginModel.PluginName;
            newItem.Command = pluginManagerViewModel.RunScriptCommand;
            newItem.CommandParameter = pluginModel.FilePath;
            if (!String.IsNullOrWhiteSpace(pluginModel.ShortcutKey))
            {
                AddKeyBinding(pluginModel.ShortcutKey, pluginModel.FilePath);
                newItem.InputGestureText = pluginModel.ShortcutKey;
            }
            //TODO: Implement a with a sorted list
            var dynamoItem = SearchForMenuItem();
            dynamoItem.Items.Add(newItem);


        }
        internal void RemovePluginMenuItem(PluginModel pluginModel)
        {
            //Remove the inputBinding first
            RemoveDynamoViewKeyBinding(pluginModel);
            //Remove the menuItem
            var dynamoItem = SearchForMenuItem();
            foreach(MenuItem item in dynamoItem.Items)
            {
                if(item.HasHeader && item.Header.Equals(pluginModel.PluginName))
                {
                    dynamoItem.Items.Remove(item);
                    break;
                }
            }
        }
        internal void ChangeShortcutKey(PluginModel pluginModel, string newShortcutKey)
        {
                        //remove the binding first
                   if(pluginModel.ShortcutKey!= null)
            {
                RemoveDynamoViewKeyBinding(pluginModel);
            }
                    //add new binding
            if (!String.IsNullOrWhiteSpace(newShortcutKey))
            {
                AddKeyBinding(newShortcutKey, pluginModel.FilePath);
            }
            //change InputGestureText of the corresponding menuItem
            MenuItem dynamoItem = SearchForMenuItem();
            for(int i = 0; i< dynamoItem.Items.Count;i++)
            {
                MenuItem item = (MenuItem)dynamoItem.Items.GetItemAt(i);
                if (item.HasHeader && item.Header.Equals(pluginModel.PluginName))
                {
                    ((MenuItem)dynamoItem.Items.GetItemAt(i)).InputGestureText = newShortcutKey;
                    break;
                }
            }
        }
        private void RemoveDynamoViewKeyBinding(PluginModel pluginModel)
        {
            foreach(var item in DynamoView.InputBindings)
            {
                if(item.GetType() == typeof( KeyBinding))
                {
                    if (pluginModel.FilePath.Equals(((KeyBinding)item).CommandParameter))
                    { 
                        DynamoView.InputBindings.Remove((KeyBinding)item);
                        break;
                    }
                }
                
            }
          //  dynamoView.InputBindings.Remove(keyBinding);

        }
        private void AddKeyBinding(string shortcutKey, string commandParam)
        {
            KeyGestureConverter keyConverter = new KeyGestureConverter();
            KeyGesture key = (KeyGesture)keyConverter.ConvertFromString(shortcutKey);
            KeyBinding keyBinding = new KeyBinding(pluginManagerViewModel.RunScriptCommand, key);
            keyBinding.CommandParameter =commandParam;
            DynamoView.InputBindings.Add(keyBinding);
        }
        private MenuItem SearchForMenuItem()
        {
            var dynamoMenuItems = dynamoMenu.Items.OfType<MenuItem>();
            return dynamoMenuItems.First(item => item.Header.ToString() == (string)pluginManagerMainMenuItem.Header);
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
            item.Header = "Plugin Manager";
          
            //= PluginManagerImportScript.ImportPythonScript();
            // item.CommandBindings.Add()


           
       //    item.Click += ShowPluginManagerWindow ;
         //  item.Click += (sender, args) => { PluginManagerImportScript.ImportPythonScript(this); };
           return item;
        }
        private void ShowPluginManagerWindow(object sender, RoutedEventArgs e)
        {
           
           PluginManagerView pluginManagerView = new PluginManagerView(pluginManagerViewModel,this);
            //OnRequestPluginManagerWindow(this, EventArgs.Empty);
            pluginManagerView.Show();
        }

        #endregion


    }
}