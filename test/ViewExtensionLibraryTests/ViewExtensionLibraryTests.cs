using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using CefSharp;
using Dynamo.LibraryUI;
using Dynamo.Extensions;
using System.Windows;
using SystemTestServices;

namespace ViewExtensionLibraryTests
{
    public class ViewExtensionLibraryTests
    {
        [Test]
        [Category("UnitTests")]
        public void ViewExtensionMockCallbacktest()
        {
            var cmd = new Mock<ICommandExecutive>();
            var callback = new Mock<IJavascriptCallback>();
            callback.Setup(c => c.CanExecute).Returns(true);
            var controller = new EventController();

            controller.On("detailsViewContextDataChanged", callback.Object);
            controller.DetailsViewContextData = 5;

            callback.Verify(c => c.ExecuteAsync(5));
        }
    }
}
