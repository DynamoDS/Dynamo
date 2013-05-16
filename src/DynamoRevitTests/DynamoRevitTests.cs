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
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Dynamo.Utilities;
using NUnit.Core;
using MessageBox = System.Windows.MessageBox;

namespace Dynamo.Tests
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    internal class DynamoRevitTestsLoader : IExternalCommand
    {
        public static ExternalCommandData RevitCommandData;

        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            RevitCommandData = revit;

            // Run tests
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
                if (dynSettings.Writer != null)
                {
                    dynSettings.Writer.WriteLine(ex.Message);
                    dynSettings.Writer.WriteLine(ex.StackTrace);
                    dynSettings.Writer.WriteLine("Dynamo log ended " + DateTime.Now.ToString());
                }
            }
        
            return Result.Succeeded;

        }
    }
}