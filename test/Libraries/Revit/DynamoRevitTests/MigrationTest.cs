using DSCoreNodesUI;
using Dynamo.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Dynamo.Tests
{
    [TestFixture]
    public class MigrationTest : DynamoRevitUnitTestBase
    {
        private void TestMigration(string filename)
        {
            var model = dynSettings.Controller.DynamoModel;

            string testPath = Path.Combine(_testPath, filename);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());


            var nodes = Controller.DynamoModel.Nodes;
            int unresolvedNodeCount = 0;

            foreach (var node in nodes.OfType<DSCoreNodesUI.DummyNode>())
            {
                if (node.NodeNature == DummyNode.Nature.Unresolved) 
                {
                    unresolvedNodeCount++;
                }
            }

            if (unresolvedNodeCount >= 1)
            {
                Assert.Fail("Number of unresolved nodes found in TestCase: " + unresolvedNodeCount);
            }
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void TestMigration_Analyze_Color()
        {
            TestMigration(@".\Migration\TestMigration_Analyze_Color.dyn");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void TestMigration_Analyze_Daylighting()
        {
            TestMigration(@".\Migration\TestMigration_Analyze_Daylighting.dyn");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void TestMigration_Analyze_Display()
        {
            TestMigration(@".\Migration\TestMigration_Analyze_Display.dyn");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void TestMigration_Analyze_Measure()
        {
            TestMigration(@".\Migration\TestMigration_Analyze_Measure.dyn");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void TestMigration_Analyze_Render()
        {
            TestMigration(@".\Migration\TestMigration_Analyze_Render.dyn");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void TestMigration_Analyze_Solar()
        {
            TestMigration(@".\Migration\TestMigration_Analyze_Solar.dyn");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void TestMigration_Analyze_Structure()
        {
            TestMigration(@".\Migration\TestMigration_Analyze_Structure.dyn");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void TestMigration_Core_Evaluate()
        {
            TestMigration(@".\Migration\TestMigration_Core_Evaluate.dyn");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void TestMigration_Core_Functions()
        {
            TestMigration(@".\Migration\TestMigration_Core_Functions.dyn");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void TestMigration_Core_Input()
        {
            TestMigration(@".\Migration\TestMigration_Core_Input.dyn");
        }
             
        [Test]
        [TestModel(@".\empty.rfa")]
        public void TestMigration_Core_List()
        {
            TestMigration(@".\Migration\TestMigration_Core_List.dyn");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void TestMigration_Core_Scripting()
        {
            TestMigration(@".\Migration\TestMigration_Core_Scripting.dyn");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void TestMigration_Core_Strings()
        {
            TestMigration(@".\Migration\TestMigration_Core_Strings.dyn");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void TestMigration_Core_Time()
        {
            TestMigration(@".\Migration\TestMigration_Core_Time.dyn");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void TestMigration_Core_Watch()
        {
            TestMigration(@".\Migration\TestMigration_Core_Watch.dyn");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void TestMigration_Geometry_Curve()
        {
            TestMigration(@".\Migration\TestMigration_Geometry_Curve.dyn");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void TestMigration_Geometry_Experimental()
        {
            TestMigration(@".\Migration\TestMigration_Geometry_Experimental.dyn");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void TestMigration_Geometry_Intersect()
        {
            TestMigration(@".\Migration\TestMigration_Geometry_Intersect.dyn");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void TestMigration_Geometry_Point()
        {
            TestMigration(@".\Migration\TestMigration_Geometry_Point.dyn");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void TestMigration_Geometry_Solid()
        {
            TestMigration(@".\Migration\TestMigration_Geometry_Solid.dyn");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void TestMigration_Geometry_Surface()
        {
            TestMigration(@".\Migration\TestMigration_Geometry_Surface.dyn");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void TestMigration_Geometry_Transform()
        {
            TestMigration(@".\Migration\TestMigration_Geometry_Transform.dyn");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void TestMigration_InputOutput_Excel()
        {
            TestMigration(@".\Migration\TestMigration_InputOutput_Excel.dyn");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void TestMigration_InputOutput_File()
        {
            TestMigration(@".\Migration\TestMigration_InputOutput_File.dyn");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void TestMigration_InputOutput_Hardware()
        {
            TestMigration(@".\Migration\TestMigration_InputOutput_Hardware.dyn");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void TestMigration_Logic_Comparison()
        {
            TestMigration(@".\Migration\TestMigration_Logic_Comparison.dyn");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void TestMigration_Logic_Conditional()
        {
            TestMigration(@".\Migration\TestMigration_Logic_Conditional.dyn");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void TestMigration_Logic_Effect()
        {
            TestMigration(@".\Migration\TestMigration_Logic_Effect.dyn");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void TestMigration_Logic_Math()
        {
            TestMigration(@".\Migration\TestMigration_Logic_Math.dyn");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void TestMigration_Revit_API()
        {
            TestMigration(@".\Migration\TestMigration_Revit_API.dyn");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void TestMigration_Revit_Bake()
        {
            TestMigration(@".\Migration\TestMigration_Revit_Bake.dyn");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void TestMigration_Revit_Datums()
        {
            TestMigration(@".\Migration\TestMigration_Revit_Datums.dyn");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void TestMigration_Revit_Document()
        {
            TestMigration(@".\Migration\TestMigration_Revit_Document.dyn");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void TestMigration_Revit_Families()
        {
            TestMigration(@".\Migration\TestMigration_Revit_Families.dyn");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void TestMigration_Revit_Referemce()
        {
            TestMigration(@".\Migration\TestMigration_Revit_Referemce.dyn");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void TestMigration_Revit_Selection()
        {
            TestMigration(@".\Migration\TestMigration_Revit_Selection.dyn");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void TestMigration_Revit_View()
        {
            TestMigration(@".\Migration\TestMigration_Revit_View.dyn");
        }
    }
}
