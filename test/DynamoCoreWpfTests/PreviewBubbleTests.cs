using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CoreNodeModels;
using Dynamo.Controls;
using Dynamo.Graph.Nodes;
using Dynamo.Models;
using Dynamo.Utilities;
using DynamoCoreWpfTests.Utility;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    class PreviewBubbleTests : DynamoTestUIBase
    {
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

        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("VMDataBridge.dll");
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("FFITarget.dll");
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        public void PreviewBubbleVisible_MouseMoveOverNode()
        {
            Open(@"core\DetailedPreviewMargin_Test.dyn");
            var nodeView = NodeViewWithGuid("7828a9dd-88e6-49f4-9ed3-72e355f89bcc");
            nodeView.PreviewControl.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));

            RaiseMouseEnterOnNode(nodeView);

            Assert.IsTrue(nodeView.PreviewControl.IsCondensed);
        }

        [Test]
        public void PreviewBubbleVisible_MouseMoveOverNode_InCustomWorkspace()
        {
            Open(@"core\custom_node_saving\Constant2.dyf");
            var nodeView = NodeViewWithGuid("9ce91e89-c087-49cd-9fd9-540cca086475");
            nodeView.PreviewControl.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));

            RaiseMouseEnterOnNode(nodeView);

            Assert.IsTrue(nodeView.PreviewControl.IsHidden);
        }

        [Test]
        public void PreviewBubbleVisible_MouseMoveOutOfNode()
        {
            Open(@"core\DetailedPreviewMargin_Test.dyn");
            var nodeView = NodeViewWithGuid("7828a9dd-88e6-49f4-9ed3-72e355f89bcc");
            nodeView.PreviewControl.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
            
            RaiseMouseEnterOnNode(nodeView);

            Assert.IsTrue(nodeView.PreviewControl.IsCondensed);

            RaiseMouseLeaveNode(nodeView);

            Assert.IsTrue(nodeView.PreviewControl.IsHidden);
        }

        [Test]
        public void PreviewBubbleHiiden_OnFrozenNode()
        {
            Open(@"core\DetailedPreviewMargin_Test.dyn");
            var nodeView = NodeViewWithGuid("7828a9dd-88e6-49f4-9ed3-72e355f89bcc");
            nodeView.ViewModel.IsFrozen = true;
            nodeView.PreviewControl.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));

            RaiseMouseEnterOnNode(nodeView);

            Assert.IsTrue(nodeView.PreviewControl.IsHidden);
        }

        [Test]
        public void PreviewBubble_ListMargin()
        {
            OpenAndRun(@"core\DetailedPreviewMargin_Test.dyn");

            var nodeView = NodeViewWithGuid("81c94fd0-35a0-4680-8535-00aff41192d3");
            nodeView.PreviewControl.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));

            // Fire transition on dynamo main ui thread.
            View.Dispatcher.Invoke(() =>
            {
                nodeView.PreviewControl.BindToDataSource();
                nodeView.PreviewControl.TransitionToState(Dynamo.UI.Controls.PreviewControl.State.Condensed);
                nodeView.PreviewControl.TransitionToState(Dynamo.UI.Controls.PreviewControl.State.Expanded);
            });

            DispatcherUtil.DoEvents();
            var watchTree = nodeView.PreviewControl.ChildOfType<WatchTree>();
            Assert.NotNull(watchTree);
            Assert.AreEqual(new System.Windows.Thickness(5, 0, 5, 5), watchTree.ChildOfType<VirtualizingPanel>().Margin);
        }

        [Test]
        public void PreviewBubble_NumberMargin()
        {
            OpenAndRun(@"core\DetailedPreviewMargin_Test.dyn");

            var nodeView = NodeViewWithGuid("7828a9dd-88e6-49f4-9ed3-72e355f89bcc");
            nodeView.PreviewControl.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));

            // Fire transition on dynamo main ui thread.
            View.Dispatcher.Invoke(() =>
            {
                nodeView.PreviewControl.BindToDataSource();
                nodeView.PreviewControl.TransitionToState(Dynamo.UI.Controls.PreviewControl.State.Condensed);
                nodeView.PreviewControl.TransitionToState(Dynamo.UI.Controls.PreviewControl.State.Expanded);
            });

            DispatcherUtil.DoEvents();
            var watchTree = nodeView.PreviewControl.ChildOfType<WatchTree>();
            Assert.NotNull(watchTree);
            Assert.AreEqual(new System.Windows.Thickness(-5, 0, 0, 0), watchTree.ChildOfType<VirtualizingPanel>().Margin);
        }

        [Test]
        public void PreviewBubble_CloseWhenFocusInCodeBlock()
        {
            OpenAndRun(@"core\DetailedPreviewMargin_Test.dyn");

            var nodeView = NodeViewWithGuid("1382aaf9-9432-4cf0-86ae-c586d311767e");
            nodeView.PreviewControl.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));

            View.Dispatcher.Invoke(() =>
            {
                nodeView.PreviewControl.BindToDataSource();
                nodeView.PreviewControl.TransitionToState(Dynamo.UI.Controls.PreviewControl.State.Condensed);
            });
            DispatcherUtil.DoEvents();
            nodeView.ChildOfType<ICSharpCode.AvalonEdit.Editing.TextArea>().Focus();

            Assert.IsTrue(nodeView.PreviewControl.IsHidden);
        }

        [Test]
        public void PreviewBubble_NoCrashWithCodeBlock()
        {
            var code = new CodeBlockNodeModel(this.Model.LibraryServices);

            this.Model.AddNodeToCurrentWorkspace(code, true);
            this.Run();
            var nodeView = NodeViewWithGuid(code.GUID.ToString());
            
            Assert.IsNotNull(nodeView);
            View.Dispatcher.Invoke(() =>
            {
                nodeView.RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, 0) 
                { RoutedEvent = Mouse.MouseEnterEvent });
                nodeView.PreviewControl.RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, 0)
                { RoutedEvent = Mouse.MouseEnterEvent });
            });
            DispatcherUtil.DoEvents();            

            Assert.IsTrue(nodeView.PreviewControl.IsHidden);
        }

        [Test]
        public void PreviewBubble_CodeBlockIsNotInFocus()
        {
            var code = new CodeBlockNodeModel(this.Model.LibraryServices);

            Model.AddNodeToCurrentWorkspace(code, true);
            Run();

            // Click on DragCanvas.
            View.ChildOfType<DragCanvas>().RaiseEvent(
                new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
            {
                RoutedEvent = Mouse.MouseDownEvent
            });
            var nodeView = NodeViewWithGuid(code.GUID.ToString());
            Assert.IsNotNull(nodeView);

            // Move mouse on code node.
            var dispatcherOperation = View.Dispatcher.BeginInvoke(new Action(
            () =>
            {
                nodeView.RaiseEvent(
                    new MouseEventArgs(Mouse.PrimaryDevice, 0) { RoutedEvent = Mouse.MouseEnterEvent });
            }));
            DispatcherUtil.DoEvents();

            dispatcherOperation.Completed += (s, args) =>
                Assert.IsTrue(nodeView.PreviewControl.IsCondensed);

            // Move mouse on code node preview.
            dispatcherOperation = View.Dispatcher.BeginInvoke(new Action(
            () =>
            {
                nodeView.PreviewControl.RaiseEvent(
                    new MouseEventArgs(Mouse.PrimaryDevice, 0) { RoutedEvent = Mouse.MouseEnterEvent });
            }));
            DispatcherUtil.DoEvents();

            dispatcherOperation.Completed += (s, args) =>
            Assert.IsTrue(nodeView.PreviewControl.IsExpanded);
        }

        [Test]
        public void PreviewBubble_IsExpandedChangedInWatchTree()
        {
            OpenAndRun(@"core\DetailedPreviewMargin_Test.dyn");

            var nodeView = NodeViewWithGuid("81c94fd0-35a0-4680-8535-00aff41192d3");
            nodeView.PreviewControl.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));

            // Fire transition on dynamo main ui thread.
            View.Dispatcher.Invoke(() =>
            {
                nodeView.PreviewControl.BindToDataSource();
                nodeView.PreviewControl.TransitionToState(Dynamo.UI.Controls.PreviewControl.State.Condensed);
                nodeView.PreviewControl.TransitionToState(Dynamo.UI.Controls.PreviewControl.State.Expanded);
            });

            DispatcherUtil.DoEvents();
            var watchTree = nodeView.PreviewControl.ChildOfType<WatchTree>();
            Assert.NotNull(watchTree);

            // Save old values.
            var oldHeight = nodeView.PreviewControl.ActualHeight;
            var oldWidth = nodeView.PreviewControl.ActualWidth;

            var dispatcherOperation = View.Dispatcher.BeginInvoke(new Action(
                () =>
                {
                    // Collapse root tree view item.
                    watchTree.ChildOfType<TreeViewItem>().IsExpanded = false;
                }));
            DispatcherUtil.DoEvents();

            // Wait until operation is completed.
            dispatcherOperation.Completed += (s, args) =>
            {
                // Width of preview bubble should stay the same, but height should be smaller.
                Assert.AreEqual(oldWidth, nodeView.PreviewControl.ActualWidth);
                Assert.Greater(oldHeight, nodeView.PreviewControl.ActualHeight);
            };
        }

        #region Watch PreviewBubble

        [Test]
        public void Watch_PreviewAllowanceDisabled()
        {
            OpenAndRun(@"core\WatchPreviewBubble.dyn");

            var nodeView = NodeViewWithGuid("456e57f3-d06f-4a53-9771-27188ee9cb40");

            RaiseMouseEnterOnNode(nodeView);

            Assert.IsTrue(nodeView.PreviewControl.IsHidden);
        }

        [Test]
        public void Watch_DefaultSize()
        {
            OpenAndRun(@"core\WatchTree.dyn");

            var emptyWatchNode = NodeViewWithGuid("edc4b956-1cae-4dbf-b304-0742d1cff282");
            WatchTree rawEmptyWatchNode = emptyWatchNode.ChildOfType<WatchTree>();

            // Fire transition on dynamo main ui thread.
            View.Dispatcher.Invoke(() =>
            {
                emptyWatchNode.PreviewControl.BindToDataSource();
                emptyWatchNode.PreviewControl.TransitionToState(Dynamo.UI.Controls.PreviewControl.State.Condensed);
                emptyWatchNode.PreviewControl.TransitionToState(Dynamo.UI.Controls.PreviewControl.State.Expanded);
            });

            DispatcherUtil.DoEvents();

            Assert.NotNull(rawEmptyWatchNode);
            Assert.AreEqual(rawEmptyWatchNode.Width, WatchTree.DefaultWidthSize);
        }

        [Test]
        public void Watch_SingleValueSize()
        {
            OpenAndRun(@"core\WatchTree.dyn");
            
            string watchNodeGuid = "6d19ccc0-0d5e-41e0-a53e-53852a1f428a";
            var singleValueWatchNode = NodeViewWithGuid(watchNodeGuid);
            WatchTree rawSingleValueWatchNode = singleValueWatchNode.ChildOfType<WatchTree>();

            var singleValueWatchNodeModel = GetNodeModel(watchNodeGuid);
            string watchNodeValue = singleValueWatchNodeModel.CachedValue.Data.ToString();

            // Fire transition on dynamo main ui thread.
            View.Dispatcher.Invoke(() =>
            {
                singleValueWatchNode.PreviewControl.BindToDataSource();
                singleValueWatchNode.PreviewControl.TransitionToState(Dynamo.UI.Controls.PreviewControl.State.Condensed);
                singleValueWatchNode.PreviewControl.TransitionToState(Dynamo.UI.Controls.PreviewControl.State.Expanded);
            });

            DispatcherUtil.DoEvents();

            Assert.NotNull(rawSingleValueWatchNode);
            Assert.AreEqual(rawSingleValueWatchNode.Width, (watchNodeValue.Length * rawSingleValueWatchNode.WidthPerCharacter) + rawSingleValueWatchNode.ExtratWidthSize);
        }

        [Test]
        public void Watch_ListSize()
        {
            OpenAndRun(@"core\WatchTree.dyn");

            string watchNodeGuid = "2f08cce2-ad82-434b-ad94-7e9ad71ac34b";
            var listWatchNode = NodeViewWithGuid(watchNodeGuid);
            WatchTree rawListWatchNode = listWatchNode.ChildOfType<WatchTree>();

            var listWatchNodeModel = GetNodeModel(watchNodeGuid);

            CodeBlockNodeModel codeBlockNodeModel = (CodeBlockNodeModel)listWatchNodeModel.InputNodes[0].Item2;
            VirtualizingPanel Panel = rawListWatchNode.ChildOfType<VirtualizingPanel>();

            bool isTheCodeAList = false;

            if (codeBlockNodeModel.Code.Length > 2 && codeBlockNodeModel.Code.Substring(0, 1) == "[" &&
                (codeBlockNodeModel.Code.Substring(codeBlockNodeModel.Code.Length - 1, 1) == "]" ||
                codeBlockNodeModel.Code.Substring(codeBlockNodeModel.Code.Length - 2, 1) == "]"))
            {
                isTheCodeAList = true;
            }

            // Fire transition on dynamo main ui thread.
            View.Dispatcher.Invoke(() =>
            {
                listWatchNode.PreviewControl.BindToDataSource();
                listWatchNode.PreviewControl.TransitionToState(Dynamo.UI.Controls.PreviewControl.State.Condensed);
                listWatchNode.PreviewControl.TransitionToState(Dynamo.UI.Controls.PreviewControl.State.Expanded);
            });

            DispatcherUtil.DoEvents();

            Assert.NotNull(listWatchNode);
            Assert.IsTrue(isTheCodeAList);
            Assert.AreEqual(rawListWatchNode.Height, WatchTree.DefaultHeightSize);
        }

        [Test]
        public void Watch_MaxWidthSize()
        {
            OpenAndRun(@"core\WatchTree.dyn");

            string watchNodeGuid = "d1ec99db-4c9a-4255-a0c3-1316e11c0a24";
            var longLineWatchNode = NodeViewWithGuid(watchNodeGuid);
            WatchTree rawLongLinetWatchNode = longLineWatchNode.ChildOfType<WatchTree>();

            var listWatchNodeModel = GetNodeModel(watchNodeGuid);

            CodeBlockNodeModel codeBlockNodeModel = (CodeBlockNodeModel)listWatchNodeModel.InputNodes[0].Item2;
            VirtualizingPanel Panel = rawLongLinetWatchNode.ChildOfType<VirtualizingPanel>();

            string watchNodeValue = listWatchNodeModel.CachedValue.Data.ToString();

            double expectedWidth = (watchNodeValue.Length * rawLongLinetWatchNode.WidthPerCharacter);
            // Fire transition on dynamo main ui thread.
            View.Dispatcher.Invoke(() =>
            {
                longLineWatchNode.PreviewControl.BindToDataSource();
                longLineWatchNode.PreviewControl.TransitionToState(Dynamo.UI.Controls.PreviewControl.State.Condensed);
                longLineWatchNode.PreviewControl.TransitionToState(Dynamo.UI.Controls.PreviewControl.State.Expanded);
            });

            DispatcherUtil.DoEvents();

            Assert.NotNull(longLineWatchNode);
            Assert.IsTrue(expectedWidth > rawLongLinetWatchNode.MaxWidthSize);
            Assert.AreEqual(rawLongLinetWatchNode.MaxWidthSize + rawLongLinetWatchNode.ExtratWidthSize, rawLongLinetWatchNode.Width);
        }

        [Test]
        public void Watch_ColorRange()
        {
            OpenAndRun(@"core\WatchTree.dyn");

            string watchNodeGuid = "a781a6fb-dbe8-4d7e-9ccf-2dfcf56a6ec4";
            var colorRangeWatchNode = NodeViewWithGuid(watchNodeGuid);

            NodeModel colorRangeWatchNodeModel = GetNodeModel(watchNodeGuid);
            string nodeDescription = colorRangeWatchNodeModel.Description;

            // Fire transition on dynamo main ui thread.
            View.Dispatcher.Invoke(() =>
            {
                colorRangeWatchNode.PreviewControl.BindToDataSource();
                colorRangeWatchNode.PreviewControl.TransitionToState(Dynamo.UI.Controls.PreviewControl.State.Condensed);
                colorRangeWatchNode.PreviewControl.TransitionToState(Dynamo.UI.Controls.PreviewControl.State.Expanded);
            });

            DispatcherUtil.DoEvents();

            Assert.NotNull(colorRangeWatchNode);
            Assert.IsTrue(nodeDescription == CoreNodeModels.Properties.Resources.ColorRangeDescription);
            Assert.AreEqual(34, colorRangeWatchNodeModel.OutPorts[0].Height);
        }

        [Test]
        public void Watch_MultilineString()
        {
            OpenAndRun(@"core\WatchTree.dyn");

            string watchNodeGuid = "c93374ef-5b1f-488e-9303-91b87ad20a73";
            var multiLineStringWatchNode = NodeViewWithGuid(watchNodeGuid);
            WatchTree rawMultiLineStringtWatchNode = multiLineStringWatchNode.ChildOfType<WatchTree>();

            var multiLineStringWatchNodeModel = GetNodeModel(watchNodeGuid);            
            string watchNodeValue = multiLineStringWatchNodeModel.CachedValue.Data.ToString();
            bool containsNewLine = watchNodeValue.Contains(Environment.NewLine);

            // Fire transition on dynamo main ui thread.
            View.Dispatcher.Invoke(() =>
            {
                multiLineStringWatchNode.PreviewControl.BindToDataSource();
                multiLineStringWatchNode.PreviewControl.TransitionToState(Dynamo.UI.Controls.PreviewControl.State.Condensed);
                multiLineStringWatchNode.PreviewControl.TransitionToState(Dynamo.UI.Controls.PreviewControl.State.Expanded);
            });

            DispatcherUtil.DoEvents();

            Assert.NotNull(multiLineStringWatchNode);
            Assert.IsTrue(containsNewLine);
            Assert.AreEqual(WatchTree.DefaultHeightSize, rawMultiLineStringtWatchNode.Height);
        }

        [Test]
        public void Watch_Dictionary()
        {
            OpenAndRun(@"core\WatchTree.dyn");

            string watchNodeGuid = "a3b57ebd-5fff-413d-a6c5-ecee20e80e4e";
            var dictionaryWatchNode = NodeViewWithGuid(watchNodeGuid);
            WatchTree rawDictionaryWatchNode = dictionaryWatchNode.ChildOfType<WatchTree>();

            bool isDictionary = rawDictionaryWatchNode.NodeLabel.ToUpper() == nameof(Dynamo.ViewModels.WatchViewModel.DICTIONARY);

            // Fire transition on dynamo main ui thread.
            View.Dispatcher.Invoke(() =>
            {
                dictionaryWatchNode.PreviewControl.BindToDataSource();
                dictionaryWatchNode.PreviewControl.TransitionToState(Dynamo.UI.Controls.PreviewControl.State.Condensed);
                dictionaryWatchNode.PreviewControl.TransitionToState(Dynamo.UI.Controls.PreviewControl.State.Expanded);
            });

            DispatcherUtil.DoEvents();

            Assert.NotNull(dictionaryWatchNode);
            Assert.IsTrue(isDictionary);
            Assert.AreEqual(WatchTree.DefaultHeightSize, rawDictionaryWatchNode.Height);
        }

        #endregion

        [Test]
        public void PreviewBubble_HiddenDummyVerticalBoundaries()
        {
            Open(@"core\DetailedPreviewMargin_Test.dyn");

            var nodeView = NodeViewWithGuid("1382aaf9-9432-4cf0-86ae-c586d311767e");
            nodeView.PreviewControl.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
                        
            // preview is hidden
            Assert.IsTrue(ElementIsInContainer(nodeView.PreviewControl.HiddenDummy, nodeView.PreviewControl, 0));

            View.Dispatcher.Invoke(() =>
            {
                nodeView.PreviewControl.BindToDataSource();
                nodeView.PreviewControl.TransitionToState(Dynamo.UI.Controls.PreviewControl.State.Condensed);
            });

            DispatcherUtil.DoEvents();

            // preview is condensed
            Assert.IsTrue(ElementIsInContainer(nodeView.PreviewControl.HiddenDummy, nodeView.PreviewControl, 0));

            View.Dispatcher.Invoke(() =>
            {
                nodeView.PreviewControl.TransitionToState(Dynamo.UI.Controls.PreviewControl.State.Expanded);
            });

            DispatcherUtil.DoEvents();

            // preview is expanded, and its size will be slighly larger than the size of node view.
            // See PR: https://github.com/DynamoDS/Dynamo/pull/6799
            Assert.IsTrue(ElementIsInContainerWithEpsilonCompare(nodeView.PreviewControl.HiddenDummy, nodeView, 10));
        }

        [Test]
        public void PreviewBubble_ToggleShowPreviewBubbles()
        {
            Open(@"core\DetailedPreviewMargin_Test.dyn");
            var nodeView = NodeViewWithGuid("7828a9dd-88e6-49f4-9ed3-72e355f89bcc");
            Assert.IsTrue(ViewModel.PreferencesViewModel.ShowPreviewBubbles, "Preview bubbles are turned off");

            nodeView.PreviewControl.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));

            RaiseMouseEnterOnNode(nodeView);
            Assert.IsTrue(nodeView.PreviewControl.IsCondensed, "Compact preview bubble is not shown");

            RaiseMouseLeaveNode(nodeView);
            Assert.IsTrue(nodeView.PreviewControl.IsHidden, "Preview bubble is not hidden");

            // turn off preview bubbles
            ViewModel.PreferencesViewModel.ShowPreviewBubbles = false;
            Assert.IsFalse(ViewModel.PreferencesViewModel.ShowPreviewBubbles, "Preview bubbles have not been turned off");

            RaiseMouseEnterOnNode(nodeView);

            Assert.IsTrue(nodeView.PreviewControl.IsHidden, "Preview bubble is not hidden");
        }

        [Test]
        public void PreviewBubble_ShowExpandedPreviewOnPinIconHover()
        {
            Open(@"core\DetailedPreviewMargin_Test.dyn");
            var nodeView = NodeViewWithGuid("7828a9dd-88e6-49f4-9ed3-72e355f89bcc");

            var previewBubble = nodeView.PreviewControl;
            previewBubble.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
            previewBubble.bubbleTools.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));

            // open preview bubble
            RaiseMouseEnterOnNode(nodeView);
            Assert.IsTrue(previewBubble.IsCondensed, "Compact preview bubble should be shown");
            Assert.AreEqual(Visibility.Collapsed, previewBubble.bubbleTools.Visibility, "Pin icon should not be shown");

            // hover preview bubble to see pin icon
            RaiseMouseEnterOnNode(previewBubble);
            Assert.AreEqual(Visibility.Visible, previewBubble.bubbleTools.Visibility, "Pin icon should be shown");

            // expand preview bubble
            RaiseMouseEnterOnNode(previewBubble.bubbleTools);
            Assert.IsTrue(previewBubble.IsExpanded, "Expanded preview bubble should be shown");
        }

        [Test]
        public void PreviewBubble_ShowExpandedPreview_MultiReturnNode()
        {
            Open(@"core\multireturnnode_preview.dyn");
            var nodeView = NodeViewWithGuid("587d7494-e764-41fb-8b5d-a4229f7294ee");

            var previewBubble = nodeView.PreviewControl;
            previewBubble.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
            previewBubble.bubbleTools.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));

            // open preview bubble
            RaiseMouseEnterOnNode(nodeView);
            Assert.IsTrue(previewBubble.IsCondensed, "Compact preview bubble should be shown");
            Assert.AreEqual(Visibility.Collapsed, previewBubble.bubbleTools.Visibility, "Pin icon should not be shown");

            // hover preview bubble to see pin icon
            RaiseMouseEnterOnNode(previewBubble);
            Assert.AreEqual(Visibility.Visible, previewBubble.bubbleTools.Visibility, "Pin icon should be shown");

            // expand preview bubble
            RaiseMouseEnterOnNode(previewBubble.bubbleTools);
            Assert.IsTrue(previewBubble.IsExpanded, "Expanded preview bubble should be shown");
        }

        [Test]
        public void PreviewBubble_ShownForColorRange()
        {
            var colorRange = new ColorRange();
            Model.AddNodeToCurrentWorkspace(colorRange, true);
            DispatcherUtil.DoEvents();
            var nodeView = NodeViewWithGuid(colorRange.GUID.ToString());
            nodeView.PreviewControl.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));

            // open preview bubble
            RaiseMouseEnterOnNode(nodeView);
            Assert.IsFalse(nodeView.PreviewControl.IsHidden, "Preview bubble for color range should be shown");
        }

        [Test]
        public void InfoBubble_AvoidDuplicatedWarningMessages()
        {
            Open(@"core\watch\WatchDuplicatedWarningMessages.dyn");
            var nodeView = NodeViewWithGuid("67b46c3f-b60f-49d7-a8cf-bb9cc4a03450");

            Assert.AreEqual(1, nodeView.ViewModel.ErrorBubble.NodeMessages.Count);

            var selectNodeCommand =
             new DynamoModel.SelectModelCommand(nodeView.ViewModel.Id.ToString(), System.Windows.Input.ModifierKeys.None.AsDynamoType());

            Model.ExecuteCommand(selectNodeCommand);

            Model.Copy();
            Model.Paste();

            DispatcherUtil.DoEvents();

            Assert.AreEqual(1, nodeView.ViewModel.ErrorBubble.NodeMessages.Count);
        }

        [Test]
        public void InfoBubble_ShowsWarningOnCopyPastedCBN()
        {
            Open(@"core\watch\ShowsWarningOnCopyPastedCBN.dyn");
            var nodeView = NodeViewWithGuid("686892b3-ea1d-4a1a-9c50-a891eb4479f6");

            Assert.AreEqual(2, nodeView.ViewModel.ErrorBubble.NodeMessages.Count);

            var selectNodeCommand =
             new DynamoModel.SelectModelCommand(nodeView.ViewModel.Id.ToString(), System.Windows.Input.ModifierKeys.None.AsDynamoType());

            Model.ExecuteCommand(selectNodeCommand);

            Model.Copy();
            Model.Paste();

            DispatcherUtil.DoEvents();

            var nodes = Model.CurrentWorkspace.Nodes;
            Assert.AreEqual(2, nodes.Count());
            foreach (var node in nodes)
            {
                var nView = NodeViewWithGuid(node.GUID.ToString());
                Assert.AreEqual(2, nView.ViewModel.ErrorBubble.NodeMessages.Count);
            }
        }

        [Test]
        public void InfoBubble_ShowsWarningOnNode2Code()
        {
            Open(@"core\watch\infobubble_warning_on_n2c.dyn");

            Dynamo.Selection.DynamoSelection.Instance.Selection.AddRange(Model.CurrentWorkspace.Nodes);

            ViewModel.CurrentSpaceViewModel.NodeToCodeCommand.Execute(null);

            DispatcherUtil.DoEvents();

            var nodes = Model.CurrentWorkspace.Nodes;
            Assert.AreEqual(1, nodes.Count());

            var nView = NodeViewWithGuid(nodes.ElementAt(0).GUID.ToString());
            var msgs = nView.ViewModel.ErrorBubble.NodeMessages;
            Assert.IsTrue(msgs.Any());
            Assert.IsTrue(msgs[0].Message.Contains("You cannot define a variable more than once."));
        }

        [Test]
        public void InfoBubble_ShowsWarningOnDupVariableInCodeBlock()
        {
            Open(@"core\watch\cbn_dup_variable_open_file.dyn");

            var nodeView = NodeViewWithGuid("2bea01fa-1534-4101-9b11-e6000cd17045");

            Assert.AreEqual(2, nodeView.ViewModel.ErrorBubble.NodeMessages.Count);
            Assert.IsTrue(nodeView.ViewModel.ErrorBubble.NodeMessages[0].Message.Contains("You cannot define a variable more than once."));
        }

        [Test]
        public void InfoBubble_ShowsWarningOnObsoleteZeroTouchNode()
        {
            Open(@"core\watch\obsolete_zero_touch_node.dyn");

            var nodeView = NodeViewWithGuid("cb146e7e-22ad-4e96-bfe4-5e506d3669d1");

            Assert.AreEqual(1, nodeView.ViewModel.ErrorBubble.NodeMessages.Count);
            Assert.IsTrue(nodeView.ViewModel.ErrorBubble.NodeMessages[0].Message.Contains("Obsolete"));
        }

        [Test]
        public void InfoBubble_ShowsWarningErrorCBN()
        {
            Open(@"core\watch\ShowsWarningOnCopyPastedCBN.dyn");
            var guid = "686892b3-ea1d-4a1a-9c50-a891eb4479f6";
            var nodeView = NodeViewWithGuid(guid);

            Assert.AreEqual(2, nodeView.ViewModel.ErrorBubble.NodeMessages.Count);

            var cbnGuid = Guid.Parse(guid);
            var command = new DynamoModel.UpdateModelValueCommand(
                Guid.Empty, cbnGuid, "Code", @"a=0;
                                               a="";
                                               b=0..1;
                                               c=b[2];
                                               %$#^$@;");
            Model.ExecuteCommand(command);

            var msgs = nodeView.ViewModel.ErrorBubble.NodeMessages;
            Assert.AreEqual(3, msgs.Count);
            var warnings = msgs.Where(x => x.Style == Dynamo.ViewModels.InfoBubbleViewModel.Style.Warning);
            Assert.AreEqual(2, warnings.Count());

            var errors = msgs.Where(x => x.Style == Dynamo.ViewModels.InfoBubbleViewModel.Style.Error);
            Assert.AreEqual(1, errors.Count());
        }

        [Test]
        public void InfoBubble_ShowsWarningError_CopyPastedCBN()
        {
            Open(@"core\watch\ShowsWarningOnCopyPastedCBN.dyn");
            var guid = "686892b3-ea1d-4a1a-9c50-a891eb4479f6";
            var nodeView = NodeViewWithGuid(guid);

            Assert.AreEqual(2, nodeView.ViewModel.ErrorBubble.NodeMessages.Count);

            var cbnGuid = Guid.Parse(guid);
            var command = new DynamoModel.UpdateModelValueCommand(
                Guid.Empty, cbnGuid, "Code", @"a=0;
                                               a="";
                                               b=0..1;
                                               c=b[2];
                                               %$#^$@;");
            Model.ExecuteCommand(command);

            var selectNodeCommand =
             new DynamoModel.SelectModelCommand(nodeView.ViewModel.Id.ToString(), System.Windows.Input.ModifierKeys.None.AsDynamoType());

            Model.ExecuteCommand(selectNodeCommand);

            Model.Copy();
            Model.Paste();

            DispatcherUtil.DoEvents();

            var nodes = Model.CurrentWorkspace.Nodes;
            Assert.AreEqual(2, nodes.Count());

            var msgs = nodeView.ViewModel.ErrorBubble.NodeMessages;
            Assert.AreEqual(3, msgs.Count);
            var warnings = msgs.Where(x => x.Style == Dynamo.ViewModels.InfoBubbleViewModel.Style.Warning);
            Assert.AreEqual(2, warnings.Count());

            var errors = msgs.Where(x => x.Style == Dynamo.ViewModels.InfoBubbleViewModel.Style.Error);
            Assert.AreEqual(1, errors.Count());

            var copy = nodes.Where(n => n.GUID.ToString() != guid).FirstOrDefault();
            nodeView = NodeViewWithGuid(copy.GUID.ToString());
            msgs = nodeView.ViewModel.ErrorBubble.NodeMessages;
            Assert.AreEqual(1, msgs.Count);

            errors = msgs.Where(x => x.Style == Dynamo.ViewModels.InfoBubbleViewModel.Style.Error);
            Assert.AreEqual(1, errors.Count());
        }

        [Test]
        public void InfoBubble_ShowsError_CopyPastedCBN_UndoRedo()
        {
            Open(@"core\watch\ShowsWarningOnCopyPastedCBN.dyn");
            var guid = "686892b3-ea1d-4a1a-9c50-a891eb4479f6";
            var nodeView = NodeViewWithGuid(guid);

            Assert.AreEqual(2, nodeView.ViewModel.ErrorBubble.NodeMessages.Count);

            var cbnGuid = Guid.Parse(guid);
            var command = new Dynamo.Models.DynamoModel.UpdateModelValueCommand(
                Guid.Empty, cbnGuid, "Code", @"a=0;
                                               a="";
                                               b=0..1;
                                               c=b[2];
                                               %$#^$@;");
            Model.ExecuteCommand(command);

            var selectNodeCommand =
             new DynamoModel.SelectModelCommand(nodeView.ViewModel.Id.ToString(), System.Windows.Input.ModifierKeys.None.AsDynamoType());

            Model.ExecuteCommand(selectNodeCommand);

            Model.Copy();
            Model.Paste();

            var undoCommand = new DynamoModel.UndoRedoCommand(DynamoModel.UndoRedoCommand.Operation.Undo);
            Model.ExecuteCommand(undoCommand);

            var redoCommand = new DynamoModel.UndoRedoCommand(DynamoModel.UndoRedoCommand.Operation.Redo);
            Model.ExecuteCommand(redoCommand);

            DispatcherUtil.DoEvents();

            var nodes = Model.CurrentWorkspace.Nodes;
            Assert.AreEqual(2, nodes.Count());

            var msgs = nodeView.ViewModel.ErrorBubble.NodeMessages;
            Assert.AreEqual(3, msgs.Count);
            var warnings = msgs.Where(x => x.Style == Dynamo.ViewModels.InfoBubbleViewModel.Style.Warning);
            Assert.AreEqual(2, warnings.Count());

            var errors = msgs.Where(x => x.Style == Dynamo.ViewModels.InfoBubbleViewModel.Style.Error);
            Assert.AreEqual(1, errors.Count());

            var copy = nodes.Where(n => n.GUID.ToString() != guid).FirstOrDefault();
            nodeView = NodeViewWithGuid(copy.GUID.ToString());
            msgs = nodeView.ViewModel.ErrorBubble.NodeMessages;
            Assert.AreEqual(1, msgs.Count);

            errors = msgs.Where(x => x.Style == Dynamo.ViewModels.InfoBubbleViewModel.Style.Error);
            Assert.AreEqual(1, errors.Count());
        }

        [Test]
        public void InfoBubble_ShowsWarningOnDummyNodes()
        {
            Open(@"core\watch\selectbycat.dyn");

            var nodes = Model.CurrentWorkspace.Nodes;
            Assert.AreEqual(4, nodes.Count());
            foreach (var node in nodes)
            {
                var nView = NodeViewWithGuid(node.GUID.ToString());
                var msgs = nView.ViewModel.ErrorBubble.NodeMessages;
                Assert.AreEqual(1, msgs.Count());
                Assert.AreEqual(Dynamo.ViewModels.InfoBubbleViewModel.Style.Warning, msgs[0].Style);
                Assert.True(msgs[0].Message.Contains("cannot be resolved."));
            }
        }

        [Test]
        public void InfoBubble_ShowsWarningOnDeprecatedNode()
        {
            Open(@"core\watch\obsolete_if_node.dyn");

            var node = Model.CurrentWorkspace.Nodes.FirstOrDefault();
            Assert.AreEqual(ElementState.Warning, node.State);

            var nView = NodeViewWithGuid(node.GUID.ToString());
            var msgs = nView.ViewModel.ErrorBubble.NodeMessages;
            Assert.AreEqual(2, msgs.Count());
            Assert.AreEqual(Dynamo.ViewModels.InfoBubbleViewModel.Style.Warning, msgs[0].Style);
        }

        [Test]
        [Category("Failure")]
        public void PreviewBubble_CopyToClipboard()
        {
            // Arrange
            Open(@"core\watch\WatchViewModelGetNodeLabelTree.dyn");
            string singleItemTreeExpected = "Hello, world!";
            var nodeView = NodeViewWithGuid("d653b1b0-ac60-4e26-b73c-627a39c5694a");
            Clipboard.SetText($"Resetting clipboard for {nameof(PreviewBubble_CopyToClipboard)} test");

            // Act
            var previewBubble = nodeView.PreviewControl;
            previewBubble.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
            previewBubble.bubbleTools.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
            // Open preview bubble
            RaiseMouseEnterOnNode(nodeView);
            // Hover preview bubble to see pin icon
            RaiseMouseEnterOnNode(previewBubble);
            // Expand preview bubble
            RaiseMouseEnterOnNode(previewBubble.bubbleTools);

            // Find the MenuItem for Copy in the context menu
            var grid = previewBubble.bubbleTools.Parent as Grid;
            var menuItem = grid.ContextMenu.Items
                .OfType<MenuItem>()
                .First(x => x.Header.ToString() == Dynamo.Wpf.Properties.Resources.ContextMenuCopy);
            
            // Click the MenuItem
            menuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            DispatcherUtil.DoEvents();
            string clipboardContent = Clipboard.GetText();

            // Assert
            Assert.AreEqual(singleItemTreeExpected, clipboardContent);
        }

        private bool ElementIsInContainer(FrameworkElement element, FrameworkElement container, int offset)
        {
            var relativePosition = element.TranslatePoint(new Point(), container);
            relativePosition.X += offset;
            
            return (relativePosition.X == 0) && (element.ActualWidth <= container.ActualWidth);
        }

        /// <summary>
        /// Similar to ElementIsInContainer but allows for an epsilon difference
        /// when comparing equality.
        /// </summary>
        private bool ElementIsInContainerWithEpsilonCompare(FrameworkElement element, FrameworkElement container, int offset)
        {
            const double Epsilon = 1e-10;
            Func<double, double, bool> epsilonEqual = (a, b) => a >= b - Epsilon && a <= b + Epsilon;
            var relativePosition = element.TranslatePoint(new Point(), container);
            relativePosition.X += offset;

            return epsilonEqual(relativePosition.X, 0) && (element.ActualWidth <= container.ActualWidth);
        }

        private void RaiseMouseEnterOnNode(IInputElement nv)
        {
            View.Dispatcher.Invoke(() =>
            {
                nv.RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, 0) { RoutedEvent = Mouse.MouseEnterEvent });
            });

            DispatcherUtil.DoEvents();
        }

        private void RaiseMouseLeaveNode(IInputElement nv)
        {
            View.Dispatcher.Invoke(() =>
            {
                nv.RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, 0) { RoutedEvent = Mouse.MouseLeaveEvent });
            });

            DispatcherUtil.DoEvents();
        }

        protected NodeModel GetNodeModel(string guid)
        {            
            return Model.CurrentWorkspace.Nodes.First(n => n.GUID == Guid.Parse(guid));
        }
    }
}
