using Dynamo;
using Dynamo.Search.SearchElements;
using Dynamo.ViewModels;
using Dynamo.Views;
using Dynamo.Wpf.ViewModels;
using Dynamo.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows;
using Dynamo.UI.Controls;
using System.Windows.Controls.Primitives;
using System.Reflection;

namespace DynamoCoreWpfTests
{
    class InCanvasSearchTests : DynamoTestUIBase
    {
        List<NodeSearchElementViewModel> members = new List<NodeSearchElementViewModel>();

        [SetUp]
        public void SetUpViewModel()
        {
            members.Add(CreateCustomNodeViewModel("A", "TopCategory"));
            members.Add(CreateCustomNodeViewModel("B", "TopCategory"));
            members.Add(CreateCustomNodeViewModel("C", "TopCategory"));
            members.Add(CreateCustomNodeViewModel("D", "TopCategory"));
        }

        [Test]
        [Category("UnitTests")]
        public void MoveToNextMemberTest()
        {
            var incanvasSearch = new Dynamo.UI.Controls.InCanvasSearchControl();

            var next_index = incanvasSearch.MoveToNextMember(true, members, members.ElementAt(0));
            Assert.AreEqual(1, next_index);
            Assert.AreEqual("B", members[next_index].Name);

            next_index = incanvasSearch.MoveToNextMember(true, members, members.ElementAt(1));
            Assert.AreEqual(2, next_index);
            Assert.AreEqual("C", members[next_index].Name);

            next_index = incanvasSearch.MoveToNextMember(false, members, members.ElementAt(2));
            Assert.AreEqual(1, next_index);
            Assert.AreEqual("B", members[next_index].Name);

            next_index = incanvasSearch.MoveToNextMember(false, members, members.ElementAt(1));
            Assert.AreEqual(0, next_index);
            Assert.AreEqual("A", members[next_index].Name);
        }

        [Test]
        [Category("UnitTests")]
        public void TooltipTest()
        {
            Run();

            var canvas = View.WorkspaceTabs.ChildrenOfType<WorkspaceView>().First().outerCanvas;

            // Get context menu.
            var canvasContextMenu = canvas.ContextMenu;
            canvasContextMenu.DataContext = canvas.DataContext;
            canvasContextMenu.IsOpen = true;

            // Find InCanvasSearch.
            var inCanvasSearch = canvasContextMenu.ChildOfType<StackPanel>().ChildOfType<InCanvasSearchControl>();

            // Try to type something. E.g. letter "L".
            inCanvasSearch.SearchTextBox.Text = "L";
            Assert.IsTrue(inCanvasSearch.ViewModel.SearchText == "L");

            // Forse WPF to draw list box items.
            IItemContainerGenerator generator = inCanvasSearch.MembersListBox.ItemContainerGenerator;
            GeneratorPosition position = generator.GeneratorPositionFromIndex(0);
            using (generator.StartAt(position, GeneratorDirection.Forward, true))
            {
                foreach (object o in inCanvasSearch.MembersListBox.Items)
                {
                    DependencyObject dp = generator.GenerateNext();
                    generator.PrepareItemContainer(dp);
                }
            }

            var firstElement = inCanvasSearch.MembersListBox.ItemContainerGenerator.ContainerFromIndex(0) as FrameworkElement;
            firstElement.Style = (Style)inCanvasSearch.FindResource("SearchMemberStyle");


            // Ensure, that first member in collection and first listbox item are the same.
            var firstMember = inCanvasSearch.MembersListBox.Items[0];

            Assert.NotNull(firstMember);
            Assert.AreSame(firstElement.DataContext, firstMember);

            // Try to move mouse on first found member.
            MouseEventArgs e = new MouseEventArgs(Mouse.PrimaryDevice, 0);
            e.RoutedEvent = Mouse.MouseEnterEvent;
            e.Source = firstElement;

            firstElement.RaiseEvent(e);
            Assert.AreSame(firstElement.DataContext, inCanvasSearch.toolTipPopup.DataContext);

            // Try to move mouse out of first found member.
            e = new MouseEventArgs(Mouse.PrimaryDevice, 0);
            e.RoutedEvent = Mouse.MouseLeaveEvent;
            e.Source = firstElement;

            firstElement.RaiseEvent(e);
            Assert.Null(inCanvasSearch.toolTipPopup.DataContext);

            Exit();
        }

        #region Helpers

        private static NodeSearchElement CreateCustomNode(string name, string category,
            string description = "", string path = "")
        {
            var element = new CustomNodeSearchElement(null,
                new CustomNodeInfo(Guid.NewGuid(), name, category, description, path));

            return element;
        }

        private static NodeSearchElementViewModel CreateCustomNodeViewModel(string name, string category,
            string description = "", string path = "")
        {
            var element = CreateCustomNode(name, category, description, path);
            return new NodeSearchElementViewModel(element, null);
        }

        #endregion
    }
}
