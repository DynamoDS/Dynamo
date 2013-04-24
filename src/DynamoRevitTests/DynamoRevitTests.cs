//Copyright 2013 Ian Keough

//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at

//http://www.apache.org/licenses/LICENSE-2.0

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Dynamo.Applications;
using Dynamo.Utilities;
using NUnit.Core;
using Dynamo.Controls;
using IWin32Window = System.Windows.Interop.IWin32Window;
using MessageBox = System.Windows.MessageBox;
using Rectangle = System.Drawing.Rectangle;

namespace Dynamo.Tests
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    internal class DynamoRevitTestsLoader : IExternalCommand
    {
        //private UIDocument m_doc;
        //private UIApplication m_revit;

        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            try
            {
                
                CoreExtensions.Host.InitializeService();
                var runner = new SimpleTestRunner();
                var package = new TestPackage("Test");
                string loc = Assembly.GetExecutingAssembly().Location;
                package.Assemblies.Add(loc);

                TestResult result;
                if (runner.Load(package))
                {
                    result = runner.Run(new NullListener(), TestFilter.Empty, true, LoggingThreshold.All);
                    
                    MessageBox.Show(result.FullName);
                    MessageBox.Show(result.IsSuccess.ToString());
                    MessageBox.Show(result.Message);
                    MessageBox.Show(result.Results.Count.ToString());
  
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                if (Dynamo.Utilities.dynSettings.Writer != null)
                {
                    Dynamo.Utilities.dynSettings.Writer.WriteLine(ex.Message);
                    Dynamo.Utilities.dynSettings.Writer.WriteLine(ex.StackTrace);
                    Dynamo.Utilities.dynSettings.Writer.WriteLine("Dynamo log ended " + DateTime.Now.ToString());
                }
                return Result.Failed;
            }

            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    internal class DynamoRevit : IExternalCommand
    {
        private static dynBench _dynamoBench;
        private UIDocument _mDoc;
        private UIApplication _mRevit;

        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            if (_dynamoBench != null)
            {
                _dynamoBench.Focus();
                return Result.Succeeded;
            }

            dynSettings.StartLogging();

            try
            {
                _mRevit = revit.Application;
                _mDoc = _mRevit.ActiveUIDocument;

                #region default level

                Level defaultLevel = null;
                var fecLevel = new FilteredElementCollector(_mDoc.Document);
                fecLevel.OfClass(typeof(Level));
                defaultLevel = fecLevel.ToElements()[0] as Level;

                #endregion

                //dynRevitSettings.Revit = _mRevit;
                //dynRevitSettings.Doc = _mDoc;
                //dynRevitSettings.DefaultLevel = defaultLevel;

                IdlePromise.ExecuteOnIdle(delegate
                {
                    //get window handle
                    IntPtr mwHandle = Process.GetCurrentProcess().MainWindowHandle;

                    //show the window
                    var dynamoController = new DynamoController_Revit(DynamoRevitApp.env, DynamoRevitApp.updater);
                    _dynamoBench = dynamoController.Bench;

                    //set window handle and show dynamo
                    new WindowInteropHelper(_dynamoBench).Owner = mwHandle;

                    _dynamoBench.WindowStartupLocation = WindowStartupLocation.Manual;

                    Rectangle bounds = Screen.PrimaryScreen.Bounds;
                    _dynamoBench.Left = bounds.X;
                    _dynamoBench.Top = bounds.Y;
                    _dynamoBench.Loaded += dynamoForm_Loaded;

                    _dynamoBench.Show();

                    _dynamoBench.Closed += dynamoForm_Closed;
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                if (dynSettings.Writer != null)
                {
                    dynSettings.Writer.WriteLine(ex.Message);
                    dynSettings.Writer.WriteLine(ex.StackTrace);
                    dynSettings.Writer.WriteLine("Dynamo log ended " + DateTime.Now.ToString());
                }
                return Result.Failed;
            }

            return Result.Succeeded;
        }

        private void dynamoForm_Closed(object sender, EventArgs e)
        {
            _dynamoBench = null;
        }

        private void dynamoForm_Loaded(object sender, RoutedEventArgs e)
        {
            ((dynBench)sender).WindowState = WindowState.Maximized;
        }
    }

    internal class WindowHandle : IWin32Window
    {
        private readonly IntPtr _hwnd;

        public WindowHandle(IntPtr h)
        {
            Debug.Assert(IntPtr.Zero != h,
                         "expected non-null window handle");

            _hwnd = h;
        }

        public IntPtr Handle
        {
            get { return _hwnd; }
        }
    }
}