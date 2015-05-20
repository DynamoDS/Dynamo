﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Selection;
using Dynamo.Utilities;
using Dynamo.Views;
using Dynamo.Wpf.Controls;
using DynamoCoreWpfTests.Utility;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    public class AnnotationViewTests : DynamoTestUIBase
    {      
        public AnnotationView NodeViewWithGuid(string guid)
        {
            //dynamoView.WorkspaceTabs.ChildrenOfType<WorkspaceView>().First().ChildNodeViews();
            var annotationView =
                View.WorkspaceTabs.ChildrenOfType<WorkspaceView>().First().ChildrenOfType<AnnotationView>();
            var annotationViewOfType = annotationView.Where(x => x.ViewModel.AnnotationModel.GUID.ToString() == guid);
            Assert.AreEqual(1, annotationViewOfType.Count(), "Expected a single Annotation View with guid: " + guid);

            return annotationViewOfType.First();
        }

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
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        public  void TestAnnotationMouseClickEvent()
        {
            Open(@"UI\GroupTest.dyn");

            var annotationView = NodeViewWithGuid("a432d63f-7a36-45ad-b30a-7924beb20e90");
            //Raise a click event to check whether the group and the models are selected
            annotationView.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left)
            {
                RoutedEvent = UserControl.MouseLeftButtonDownEvent
            });

            //The group and the node should be selected
            Assert.AreEqual(2, DynamoSelection.Instance.Selection.Count());

            //Clear the selection
            DynamoSelection.Instance.ClearSelection();
           
            //Change the Zoom
            annotationView.ViewModel.WorkspaceViewModel.SetZoomCommand.Execute(0.4);

            //Group textblock should be collapsed.
            Assert.IsFalse(annotationView.GroupTextBlock.IsVisible);

            //Change the Zoom
            annotationView.ViewModel.WorkspaceViewModel.SetZoomCommand.Execute(0.5);

            //Group textblock should be visible.
            Assert.IsTrue(annotationView.GroupTextBlock.IsVisible);

            //Change the Zoom
            annotationView.ViewModel.WorkspaceViewModel.SetZoomCommand.Execute(0.4);

            //Group textblock should be collapsed.
            Assert.IsFalse(annotationView.GroupTextBlock.IsVisible);

            //Now change the font size - note: group textblock is collapsed now
            annotationView.ViewModel.FontSize = 48;

            //Group textblock should be visible.
            Assert.IsTrue(annotationView.GroupTextBlock.IsVisible);

        }
    }
}
