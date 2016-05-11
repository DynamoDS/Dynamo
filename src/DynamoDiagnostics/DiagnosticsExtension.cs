using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Dynamo.Controls;
using Dynamo.Events;
using Dynamo.Extensions;
using Dynamo.Graph.Workspaces;
using Dynamo.Session;
using Dynamo.Wpf.Extensions;

namespace Dynamo.Diagnostics
{
    public class DiagnosticsExtension : IViewExtension
    {
        private ViewLoadedParams viewLoadedParams;
        private MenuItem diagnosticsMenuItem;
        private MenuItem inputMenuItem;
        private MenuItem outputMenuItem;
        private static bool enableDiagnostics = false;
        private static bool inputDiagnostics = false;
        private static bool outputDiagnostics = false;

        private DiagnosticsSession session;
        private static PerformanceStatistics statistics = new PerformanceStatistics();
        private string statfile;

        private DiagnosticsAdorner adorner;
        public static bool EnableDiagnostics { get { return enableDiagnostics; } }
        public static bool InputDiagnostics { get { return inputDiagnostics; } }
        public static bool OutputDiagnostics { get { return outputDiagnostics; } }

        public string UniqueId
        {
            get { return "9B0E3A50-D1F5-4379-86F9-22626BFCEDCE"; }
        }

        public string Name
        {
            get { return "DynamoDiagnostics"; }
        }

        public static IQueryNodePerformance NodePerformance { get { return statistics; } }

        public void Startup(ViewStartupParams p)
        {
            statfile = Path.Combine(p.PathManager.UserDataDirectory, "Statistics.xml");
            if (File.Exists(statfile))
                statistics = PerformanceStatistics.Load(statfile);
        }

        public void Loaded(ViewLoadedParams p)
        {
            viewLoadedParams = p;
            AddMenuItem(p);
            RegisterEventHandlers(p);
        }

        private void RegisterEventHandlers(ViewLoadedParams p)
        {
            //viewLoadedParams.CurrentWorkspaceChanged += OnCurrentWorkspaceChanged;
            WorkspaceEvents.WorkspaceAdded += OnWorkspaceAdded;
            WorkspaceEvents.WorkspaceCleared += OnWorkspaceCleared;
            WorkspaceEvents.WorkspaceRemoved += OnWorkspaceRemoved;
            ExecutionEvents.GraphPreExecution += OnGraphPreExecution;
            ExecutionEvents.GraphPostExecution += OnGraphPostExecution;
        }

        void OnWorkspaceCleared()
        {
            DetachAdorner();
        }

        void OnWorkspaceRemoved(WorkspacesModificationEventArgs args)
        {
            DetachAdorner();
        }

        void OnGraphPreExecution(IExecutionSession session)
        {
            DetachAdorner();
        }

        private void DetachAdorner()
        {
            if (adorner != null)
            {
                viewLoadedParams.DynamoWindow.Dispatcher.InvokeAsync(adorner.Detach);
            }
            adorner = null;
        }

        void OnGraphPostExecution(IExecutionSession session)
        {
            if (!EnableDiagnostics) return;

            AttachAdorner();
        }

        private void AttachAdorner()
        {
            viewLoadedParams.DynamoWindow.Dispatcher.InvokeAsync(
                () => adorner = DiagnosticsAdorner.CreateAdorner(viewLoadedParams.DynamoWindow, this.session)
            );
        }

        void OnWorkspaceAdded(WorkspacesModificationEventArgs args)
        {
            if (args.Type != typeof(HomeWorkspaceModel))
                return;

            var ws = viewLoadedParams.WorkspaceModels.OfType<HomeWorkspaceModel>().First();
            SetupDiagnosticsSession(ws);
        }

        void OnCurrentWorkspaceChanged(IWorkspaceModel model)
        {
            SetupDiagnosticsSession(model);
        }

        private void SetupDiagnosticsSession(IWorkspaceModel model)
        {
            if (session != null)
            {
                if (session.WorkSpace == model) return;

                session.Dispose();
                session = null;
            }
            if (model != null)
            {
                session = new DiagnosticsSession(model, statistics);
            }
        }

        private void AddMenuItem(ViewLoadedParams p)
        {
            p.AddSeparator(MenuBarType.Settings, new Separator());
            diagnosticsMenuItem = new MenuItem() { Header = "Enable Diagnostics", Name = "Diagnostics", IsCheckable = true, IsChecked = false};
            diagnosticsMenuItem.Checked += (object sender, RoutedEventArgs e) => OnDiagnostics(diagnosticsMenuItem.IsChecked);
            p.AddMenuItem(MenuBarType.Settings, diagnosticsMenuItem);

            inputMenuItem = new MenuItem() { Header = "Show Input Data Graph", Name = "InputDiagnostics", IsCheckable = true };
            inputMenuItem.Checked += (object sender, RoutedEventArgs e) => OnDiagnosticsGraph(true);
            p.AddMenuItem(MenuBarType.Settings, inputMenuItem);
            
            outputMenuItem = new MenuItem() { Header = "Show Output Data Graph", Name = "OutputDiagnostics", IsCheckable = true };
            outputMenuItem.Checked += (object sender, RoutedEventArgs e) => OnDiagnosticsGraph(false);
            p.AddMenuItem(MenuBarType.Settings, outputMenuItem);
        }

        private void OnDiagnosticsGraph(bool input)
        {
            if(input)
            {
                inputDiagnostics = inputMenuItem.IsChecked;
                if (outputMenuItem.IsChecked)
                {
                    outputMenuItem.IsChecked = false;
                    outputDiagnostics = false;
                }
            }
            else
            {
                outputDiagnostics = outputMenuItem.IsChecked;
                if (inputMenuItem.IsChecked)
                {
                    inputMenuItem.IsChecked = false;
                    inputDiagnostics = false;
                }
            }
            if(enableDiagnostics)
            {
                DetachAdorner();
                AttachAdorner();
            }
        }

        private void OnDiagnostics(bool enable)
        {
            enableDiagnostics = enable;
            SetupDiagnosticsSession(enable ? viewLoadedParams.CurrentWorkspaceModel : null);
            DetachAdorner();
            if (enable) AttachAdorner();
        }

        private void UnregisterEventHandlers()
        {
            //viewLoadedParams.CurrentWorkspaceChanged -= OnCurrentWorkspaceChanged;
            WorkspaceEvents.WorkspaceAdded -= OnWorkspaceAdded;
            WorkspaceEvents.WorkspaceCleared -= OnWorkspaceCleared;
            WorkspaceEvents.WorkspaceRemoved -= OnWorkspaceRemoved;
            ExecutionEvents.GraphPreExecution -= OnGraphPreExecution;
            ExecutionEvents.GraphPostExecution -= OnGraphPostExecution;
        }

        public void Shutdown()
        {
            statistics.Save(statfile);
            Dispose();
        }

        public void Dispose()
        {
            SetupDiagnosticsSession(null);
            UnregisterEventHandlers();
        }
    }
}
