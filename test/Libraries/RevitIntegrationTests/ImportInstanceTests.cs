using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.Nodes;
using NUnit.Framework;
using RevitServices.Persistence;
using RTF.Framework;

namespace RevitSystemTests
{
    [TestFixture]
    public class ImportInstanceTests : SystemTest
    {
        [Test]
        [TestModel(@".\empty.rfa")]
        public void ImportCubeAndThenImportSphere()
        {
            //Run the graph to create an ImportInstance
            string dynFilePath = Path.Combine(workingDirectory, @".\ImportInstance\ImportInstance.dyn");
            string testPath = Path.GetFullPath(dynFilePath);

            ViewModel.OpenCommand.Execute(testPath);
            Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());

            //Check the number of instances
            var instances = GetAllImportInstances().OfType<Autodesk.Revit.DB.ImportInstance>();
            Assert.AreEqual(1, instances.Count());

            //Connect the code block node to create sphere to the ImportInstance node
            var importInstanceNode = GetNode<DSFunction>("7b989ec5-eb4b-4c5a-b861-423a0e1cf0e9");
            var createSphereNode = GetNode<CodeBlockNodeModel>("40b63be4-7cb3-4e90-9d1c-7ff4f89df56f");
            var connector = importInstanceNode.InPorts[0].Connectors[0];
            connector.Connect(createSphereNode.OutPorts[0]);

            //Run the graph again
            Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());

            //Check the number of instances
            instances = GetAllImportInstances().OfType<Autodesk.Revit.DB.ImportInstance>();
            Assert.AreEqual(1, instances.Count());

            //Connect the code block node to create cube to the ImportInstance node
            var createCubeNode = GetNode<CodeBlockNodeModel>("d9fc1b61-8985-4264-806f-f60628500b39");
            connector.Connect(createCubeNode.OutPorts[0]);

            //Run the graph again
            Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());

            //Check the number of instances
            instances = GetAllImportInstances().OfType<Autodesk.Revit.DB.ImportInstance>();
            Assert.AreEqual(1, instances.Count());
        }

        /// <summary>
        /// This function gets all the import instances in the current Revit document
        /// </summary>
        /// <returns>the import instances</returns>
        protected static IList<Element> GetAllImportInstances()
        {
            using (var trans = new Transaction(DocumentManager.Instance.CurrentUIDocument.Document, "FilteringElements"))
            {
                trans.Start();

                ElementClassFilter ef = new ElementClassFilter(typeof(Autodesk.Revit.DB.ImportInstance));
                FilteredElementCollector fec = new FilteredElementCollector(DocumentManager.Instance.CurrentUIDocument.Document);
                fec.WherePasses(ef);

                trans.Commit();
                return fec.ToElements();
            }
        }
    }
}
