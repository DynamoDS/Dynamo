using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dynamo.Graph.Nodes;
using Dynamo.Wpf.ViewModels.Watch3D;
using HelixToolkit.Wpf.SharpDX;
using NUnit.Framework;
using SystemTestServices;

namespace AnalysisTests
{
    [TestFixture, RequiresSTA]
    class LabelTests : SystemTestBase
    {
        protected void OpenVisualizationTest(string fileName)
        {
            string relativePath = Path.Combine(
                GetTestDirectory(ExecutingDirectory),
                string.Format(@"core\visualization\{0}", fileName));

            if (!File.Exists(relativePath))
            {
                throw new FileNotFoundException("The specified .dyn file could not be found.");
            }

            ViewModel.OpenCommand.Execute(relativePath);
        }

        protected IEnumerable<Element3D> BackgroundPreviewGeometry
        {
            get { return ((HelixWatch3DViewModel)ViewModel.BackgroundPreviewViewModel).SceneItems; }
        }

        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DSCPython.dll");
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("GeometryColor.dll");
            libraries.Add("VMDataBridge.dll");
            libraries.Add("Analysis.dll");
            base.GetLibrariesToPreload(libraries);
        }

        /// <summary>
        /// This test method will execute the next methods from the Label class:
        /// private Label(Point point, string label)
        /// public static Label ByPointAndString(Point point, string label)
        /// public void Tessellate(IRenderPackage package, TessellationParameters parameters)
        /// </summary>
        [Test, Category("UnitTests")]
        public void ByPointAndString_ShowLabel_Test()
        {
            //Arrange
            OpenVisualizationTest("Test_CBN_Label.dyn");
            Assert.IsTrue(ViewModel.HomeSpace.Nodes.All(x => x.DisplayLabels != true));

            var cbn =
              Model.CurrentWorkspace.Nodes.FirstOrDefault(
                  x => x.GUID.ToString() == "6087ac20-e784-4ffb-8391-35b627625c9f") as
                  CodeBlockNodeModel;

            //Check that the Code Block node was found
            Assert.IsNotNull(cbn);
            //This will make visible the labels in the Background Preview
            cbn.DisplayLabels = true;

            //Act
            //Internally it will call the Label.ByPointAndString method located inside the CodeBlock node
            ViewModel.HomeSpace.Run();

            //It won't have any warnings then it will be false
            bool hasWarnings = ViewModel.HomeSpace.Nodes.Any(n => n.State == ElementState.Warning || n.State == ElementState.PersistentWarning);

            //This will get the text elements in the Background Preview
            var textElements = BackgroundPreviewGeometry
                               .Where(g => g is BillboardTextModel3D)
                               .Cast<BillboardTextModel3D>()
                               .Select(bb => bb.Geometry).Cast<BillboardText3D>()
                               .ToArray();

            //Assert
            //Be Sure that we have no warning
            Assert.IsFalse(hasWarnings);

            //Check that the Label contains the right text
            Assert.That(textElements.First().TextInfo.First().Text, Is.EqualTo("test"));
        }


        /// <summary>
        /// This test method will execute the Label.ByPointAndString method and will raise the exceptions due to the parameters passed
        /// </summary>
        [Test, Category("UnitTests")]
        [TestCase("Test_CBN_Label_InvalidPoint.dyn", Description = "Passing a NULL Point to the Label.ByPointAndString method")]
        [TestCase("Test_CBN_Label_EmptyText.dyn", Description = "Passing an empty text to the Label.ByPointAndString method")]
        public void ByPointAndString_EmptyLabel_ThrowArgumentNullException(string dynFileName)
        {
            //Arrange
            OpenVisualizationTest(dynFileName);
            Assert.IsTrue(ViewModel.HomeSpace.Nodes.All(x => x.DisplayLabels != true));

            var cbn =
              Model.CurrentWorkspace.Nodes.FirstOrDefault(
                  x => x.GUID.ToString() == "6087ac20-e784-4ffb-8391-35b627625c9f") as
                  CodeBlockNodeModel;

            //Check that the Code Block node was found
            Assert.IsNotNull(cbn);
            cbn.DisplayLabels = true;

            //Act
            //Internally it will call the Label.ByPointAndString method located inside the CodeBlock node
            ViewModel.HomeSpace.Run();
            bool hasWarnings = ViewModel.HomeSpace.Nodes.Any(n => n.State == ElementState.Warning || n.State == ElementState.PersistentWarning);

            //Assert
            //It will have warnings due that the graph is not compiling
            Assert.IsTrue(hasWarnings);
            //Due that the parameters passed to the Label.ByPointAndString method are invalid it will raise an exception (that cannot be catched) but is recorded in the ToolTipText parameter
            Assert.That(cbn.Infos.Any(x => x.Message.Contains("ArgumentNullException")));
        }
    }
}
