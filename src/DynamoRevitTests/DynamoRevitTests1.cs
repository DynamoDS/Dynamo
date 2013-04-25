using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Dynamo.Applications;
using Dynamo.Commands;
using Dynamo.Controls;
using Dynamo.Nodes;
using Dynamo.Utilities;
using NUnit.Core;
using NUnit.Framework;
using MessageBox = System.Windows.MessageBox;
using Rectangle = System.Drawing.Rectangle;

namespace Dynamo.Tests
{
    [TestFixture]
    internal class DynamoRevitTests
    {
        [SetUp]
        public void Init()
        {
            string tempPath = Path.GetTempPath();
            var random = new Random();
            string logPath = Path.Combine(tempPath, "dynamoLog" + random.Next() + ".txt");

            TempFolder = Path.Combine(tempPath, "dynamoTmp");

            if (!Directory.Exists(TempFolder))
            {
                Directory.CreateDirectory(TempFolder);
            }
            else
            {
                EmptyTempFolder();
            }

            TextWriter tw = new StreamWriter(logPath);
            tw.WriteLine("Dynamo log started " + DateTime.Now.ToString());
            dynSettings.Writer = tw;
        }

        [TearDown]
        public void Cleanup()
        {
            try
            {
                dynSettings.Writer.Close();
                EmptyTempFolder();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        public void StartDynamo()
        {
            
            dynSettings.StartLogging();

            try
            {
                _mRevit = DynamoRevitTestsLoader.RevitCommandData.Application;
                _mDoc = _mRevit.ActiveUIDocument;

                #region default level

                Level defaultLevel = null;
                var fecLevel = new FilteredElementCollector(_mDoc.Document);
                fecLevel.OfClass(typeof (Level));
                defaultLevel = fecLevel.ToElements()[0] as Level;

                #endregion

                dynRevitSettings.Revit = _mRevit;
                dynRevitSettings.Doc = _mDoc;
                dynRevitSettings.DefaultLevel = defaultLevel;

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

                _dynamoBench.Show();

                _dynamoBench.Closed += dynamoForm_Closed;

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
            }

        }

        private void dynamoForm_Closed(object sender, EventArgs e)
        {
            // _dynamoBench = null;
        }

        private static string TempFolder;
        private static dynBench _dynamoBench;
        private UIDocument _mDoc;
        private UIApplication _mRevit;

        public static void EmptyTempFolder()
        {
            try
            {
                var directory = new DirectoryInfo(TempFolder);
                foreach (FileInfo file in directory.GetFiles()) file.Delete();
                foreach (DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        // OpenCommand

        [Test]
        public void CanCreateNote()
        {
            //create some test note data
            var inputs = new Dictionary<string, object>();
            inputs.Add("x", 200.0);
            inputs.Add("y", 200.0);
            inputs.Add("text", "This is a test note.");
            inputs.Add("workspace", dynSettings.Controller.CurrentSpace);

            dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.AddNoteCmd, inputs));
            dynSettings.Controller.ProcessCommandQueue();

            Assert.AreEqual(dynSettings.Controller.CurrentSpace.Notes.Count, 1);
        }


    }
}