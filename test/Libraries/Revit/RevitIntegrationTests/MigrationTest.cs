using System.IO;
using System.Linq;

using DSCoreNodesUI;

using NUnit.Framework;

using RTF.Framework;

namespace RevitSystemTests
{
    [TestFixture]
    public class MigrationTest : SystemTest
    {
        private void TestMigration(string filename)
        {
            string testPath = Path.Combine(workingDirectory, filename);

            ViewModel.OpenCommand.Execute(testPath);
            Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());


            var nodes = ViewModel.Model.Nodes;
            int unresolvedNodeCount = 0;
            string str = "\n";

            foreach (var node in nodes.OfType<DSCoreNodesUI.DummyNode>())
            {
                if (node.NodeNature == DummyNode.Nature.Unresolved) 
                {
                    unresolvedNodeCount++;
                    str += node.NickName;
                    str += "\n";
                }
            }

            if (unresolvedNodeCount >= 1)
            {
                Assert.Fail("Number of unresolved nodes found in TestCase: " + unresolvedNodeCount +str);
            }
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

        [Test, Category("Failure")]
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

        [Test, Category("Failure")]
        [TestModel(@".\empty.rfa")]
        public void TestMigration_Geometry_Curve()
        {
            TestMigration(@".\Migration\TestMigration_Geometry_Curve.dyn");
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

        [Test, Category("Failure")]
        [TestModel(@".\empty.rfa")]
        public void TestMigration_Geometry_Surface()
        {
            TestMigration(@".\Migration\TestMigration_Geometry_Surface.dyn");
        }

        [Test, Category("Failure")]
        [TestModel(@".\empty.rfa")]
        public void TestMigration_Geometry_Transform()
        {
            TestMigration(@".\Migration\TestMigration_Geometry_Transform.dyn");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void TestMigration_Revit_API()
        {
            TestMigration(@".\Migration\TestMigration_Revit_API.dyn");
        }

        [Test, Category("Failure")]
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
