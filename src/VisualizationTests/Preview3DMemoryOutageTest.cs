using Dynamo.Models;
using Dynamo.ViewModels;
using Dynamo.Wpf.Rendering;
using Dynamo.Wpf.ViewModels.Watch3D;
using DynamoCoreWpfTests.Utility;
using HelixToolkit.Wpf.SharpDX;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfVisualizationTests
{
    [TestFixture]
    public class Preview3DMemoryOutageTest : VisualizationTest
    {
        private Mock<DynamoModel> modelMock;

        /// <summary>
        /// Creates a mock DynamoModel that does not show a task dialog but instead sets its call
        /// as a verifiable call.
        /// </summary>
        /// <param name="configuration">Default DynamoModel configuration</param>
        /// <returns>Mock DynamoModel</returns>
        protected override DynamoModel CreateModel(DynamoModel.IStartConfiguration configuration)
        {
            modelMock = new Mock<DynamoModel>(configuration)
            {
                CallBase = true
            };
            modelMock.Setup(m => m.OnRequestTaskDialog(It.IsAny<object>(), It.IsAny<TaskDialogEventArgs>()))
                .Callback(() => { }) // Prevent dialog from blocking the test
                .Verifiable();

            return modelMock.Object;
        }

        /// <summary>
        /// Sets up a mock HelixWatch3DViewModel which will throw OutOfMemoryException when rendering.
        /// </summary>
        /// <returns>DynamoViewModel configuration referencing the mock 3D preview</returns>
        protected override DynamoViewModel.StartConfiguration CreateViewModelStartConfiguration()
        {
            var watch3DMock = new Mock<HelixWatch3DViewModel>(null, new Watch3DViewModelStartupParams(Model))
            {
                CallBase = true
            };
            watch3DMock.Protected().Setup("AggregateRenderPackages", ItExpr.IsAny<IEnumerable<HelixRenderPackage>>()).Throws<OutOfMemoryException>();
            return new DynamoViewModel.StartConfiguration()
            {
                Watch3DViewModel = watch3DMock.Object
            };
        }

        /// <summary>
        /// Opens any file that produces geometry and checks that the 3D preview is disabled after the error
        /// and also that a dialog is shown.
        /// </summary>
        [Test]
        public void HandlesRenderMemoryOutageGracefully()
        {
            Assert.Greater(ViewModel.Watch3DViewModels.Count(), 0);
            Assert.True(ViewModel.Watch3DViewModels.All(w => w.Active));

            OpenVisualizationTest("ASM_cuboid.dyn");

            Assert.True(ViewModel.Watch3DViewModels.All(w => !w.Active));
            modelMock.Verify();
        }
    }
}
