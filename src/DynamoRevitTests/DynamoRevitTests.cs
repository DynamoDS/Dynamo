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
using System.Reflection;
using System.Windows;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using NUnit.Core;

namespace Dynamo.Tests
{
    //[Transaction(TransactionMode.Automatic)]
    //[Regeneration(RegenerationOption.Manual)]
    //public class DynamoRevitApp : IExternalApplication
    //{
    //    public Result OnStartup(UIControlledApplication application)
    //    {
    //        return Result.Succeeded;
    //    }

    //    public Result OnShutdown(UIControlledApplication application)
    //    {
    //        return Result.Succeeded;
    //    }
    //}

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    internal class DynamoRevitTestsLoader : IExternalCommand
    {
        private UIDocument m_doc;
        private UIApplication m_revit;

        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            try
            {
                
                CoreExtensions.Host.InitializeService();
                var runner = new SimpleTestRunner();
                var package = new TestPackage("Test");
                string loc = Assembly.GetExecutingAssembly().Location;
                package.Assemblies.Add(loc);
                if (runner.Load(package))
                {
                    TestResult result = runner.Run(new NullListener(), TestFilter.Empty, true, LoggingThreshold.All);
                }

                MessageBox.Show("Bla diddy bla bla");
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
}