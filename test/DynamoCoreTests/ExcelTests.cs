using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.Tests
{
    internal class ExcelTests
    {
        [Category("DSExecution")]
        class DSEvaluationModelTest : DynamoModelTestBase
        {
            protected override void GetLibrariesToPreload(List<string> libraries)
            {
                libraries.Add("DesignScriptBuiltin.dll");
                libraries.Add("DSCoreNodes.dll");
                libraries.Add("DSOffice.dll");
                libraries.Add("FunctionObject.ds");
                libraries.Add("BuiltIn.ds");
                base.GetLibrariesToPreload(libraries);
            }

            [Test]
            public void TestExcel()
            {
                // (1..5) + 1;
                RunModel(@"core\excel\dyn-5703.dyn");
                AssertPreviewValue("7f12214c-0714-4725-9f9f-2e874427b4c0", new object[] { new string[] { "dyn-5703", null, null, null, null, null },
                    new string[] { null, null, null, null, null, null },
                    new string[] { null, null, null, null, null, null },
                    new string[] { null, null, null, null, null, null },
                    new string[] { null, null, null, null, null, null },
                    new string[] { null, null, null, null, null, "done" }
                });
                // cleanup
                foreach (var process in Process.GetProcessesByName("EXCEL"))
                {
                    if (process.MainWindowTitle.Equals("dyn-5703 - Excel"))
                    {
                        process.Kill();
                        break;
                    }
                }
            }
        }
    }
}
