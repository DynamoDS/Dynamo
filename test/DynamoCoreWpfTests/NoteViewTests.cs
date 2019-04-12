using System.Linq;
using System.Windows.Input;
using DynamoCoreWpfTests.Utility;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    public class NoteViewTests : DynamoTestUIBase
    {
        // adapted from NodeViewTests.cs

        public override void Open(string path)
        {
            base.Open(path);

            DispatcherUtil.DoEvents();
        }

        public override void Run()
        {
            base.Run();

            DispatcherUtil.DoEvents();
        }

        [Test]
        public void T01_ZIndex_Test_MouseDown()
        {
            // Reset zindex to start value.
            Dynamo.ViewModels.NoteViewModel.StaticZIndex = 3;
            Open(@"UI\UINotes.dyn");

            var noteView = NoteViewWithGuid("4677e999-d5f5-4bb2-9706-a97bf3a86711");

            // Index of first note == 5.
            Assert.AreEqual(5, noteView.ViewModel.ZIndex);
            Assert.AreEqual(3 + ViewModel.HomeSpace.Notes.Count() + ViewModel.HomeSpace.Nodes.Count(), Dynamo.ViewModels.NoteViewModel.StaticZIndex);

            noteView.noteText.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
            {
                RoutedEvent = Mouse.PreviewMouseDownEvent,
                Source = this,
            });
            Assert.AreEqual(noteView.ViewModel.ZIndex, Dynamo.ViewModels.NoteViewModel.StaticZIndex);
        }

        [Test]
        public void T02_ZIndex_Test_NoteGreaterThanNode()
        {
            //Reset ZIndex to start value for both NoteViewModel and NodeViewModel
            Dynamo.ViewModels.NoteViewModel.StaticZIndex = 3;
            Dynamo.ViewModels.NodeViewModel.StaticZIndex = 3;

            Open(@"UI\UINotes.dyn");
            var noteView = NoteViewWithGuid("4677e999-d5f5-4bb2-9706-a97bf3a86711");

            // Index of First Note (initially) == 5
            Assert.AreEqual(5, noteView.ViewModel.ZIndex);
            Assert.AreEqual(3 + ViewModel.HomeSpace.Notes.Count() + ViewModel.HomeSpace.Nodes.Count(), Dynamo.ViewModels.NoteViewModel.StaticZIndex);

            // Index of First Node (initally) == 4
            var nodeView = NodeViewWithGuid("bbc16882-75c2-4a50-a4e4-5e50e191af8f");
            Assert.AreEqual(4, nodeView.ViewModel.ZIndex);
            Assert.AreEqual(3 + ViewModel.HomeSpace.Nodes.Count(), Dynamo.ViewModels.NodeViewModel.StaticZIndex);


            Assert.Greater(Dynamo.ViewModels.NoteViewModel.StaticZIndex, Dynamo.ViewModels.NodeViewModel.StaticZIndex);
        }

        [Test]
        public void T03_Mousedown_NoteGreaterThanNode()
        {
            //Reset ZIndex to start value for both NoteViewModel and NodeViewModel
            Dynamo.ViewModels.NoteViewModel.StaticZIndex = 3;
            Dynamo.ViewModels.NodeViewModel.StaticZIndex = 3;

            Open(@"UI\UINotes.dyn");
            var noteView = NoteViewWithGuid("4677e999-d5f5-4bb2-9706-a97bf3a86711");
            var nodeView = NodeViewWithGuid("bbc16882-75c2-4a50-a4e4-5e50e191af8f");

            // Click on First Node
            nodeView.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
            {
                RoutedEvent = Mouse.PreviewMouseDownEvent,
                Source = this,
            });
            Assert.AreEqual(5, nodeView.ViewModel.ZIndex);

            // Check if First Note's ZIndex is greater than First Node after click
            Assert.Greater(noteView.ViewModel.ZIndex, nodeView.ViewModel.ZIndex);
        }
    }
}
