<<<<<<< HEAD
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Controls;
using System.Diagnostics;

=======
using System.Windows;
>>>>>>> 2ec6f35c8f2f9655bb27eff3fb81c69c167a56c6
using Microsoft.FSharp.Collections;
using Microsoft.Kinect;

using Dynamo.Connectors;
<<<<<<< HEAD
using Dynamo.FSchemeInterop;
using Dynamo.FSchemeInterop.Node;
using Dynamo.Utilities;
=======
>>>>>>> 2ec6f35c8f2f9655bb27eff3fb81c69c167a56c6

using Value = Dynamo.FScheme.Value;

namespace Dynamo.Nodes
{
    [NodeName("Kinect")]
    [NodeCategory(BuiltinNodeCategories.COMMUNICATION)]
    [NodeDescription("Read  from a Kinect.")]
    public class dynKinect : dynNodeWithOneOutput
    {
        //Kinect Runtime
        //Image image1;
        //ColorImageFrame planarImage;
        //XYZ rightHandLoc = new XYZ();
        //ReferencePoint rightHandPt;
        //Point3D rightHandPt;
        //System.Windows.Shapes.Ellipse rightHandEllipse;

        public dynKinect()
        {
            InPortData.Add(new PortData("exec", "Execution Interval", typeof(object)));
            InPortData.Add(new PortData("X scale", "The amount to scale the skeletal measurements in the X direction.", typeof(double)));
            InPortData.Add(new PortData("Y scale", "The amount to scale the skeletal measurements in the Y direction.", typeof(double)));
            InPortData.Add(new PortData("Z scale", "The amount to scale the skeletal measurements in the Z direction.", typeof(double)));
            OutPortData.Add(new PortData("kinect data", "position data from the kinect", typeof(object)));


            //int width = 320;
            //int height = 240;

            //WriteableBitmap wbitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);

            //// Create an array of pixels to contain pixel color values
            //uint[] pixels = new uint[width * height];

            //int red;
            //int green;
            //int blue;
            //int alpha;

            //for (int x = 0; x < width; ++x)
            //{
            //    for (int y = 0; y < height; ++y)
            //    {
            //        int i = width * y + x;

            //        red = 0;
            //        green = 255 * y / height;
            //        blue = 255 * (width - x) / width;
            //        alpha = 255;

            //        pixels[i] = (uint)((blue << 24) + (green << 16) + (red << 8) + alpha);
            //    }
            //}

            //// apply pixels to bitmap
            //wbitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, width * 4, 0);

            //image1 = new Image();
            //image1.Width = width;
            //image1.Height = height;
            //image1.Margin = new Thickness(5);
            //image1.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            //image1.Name = "image1";
            //image1.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            //image1.Source = wbitmap;
            ////image1.Margin = new Thickness(0, 0, 0, 0);

            //Canvas trackingCanvas = new Canvas();
            //trackingCanvas.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            //trackingCanvas.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;

            ////add an ellipse to track the hand
            //rightHandEllipse = new System.Windows.Shapes.Ellipse();
            //rightHandEllipse.Height = 10;
            //rightHandEllipse.Width = 10;
            //rightHandEllipse.Name = "rightHandEllipse";
            //SolidColorBrush yellowBrush = new SolidColorBrush(System.Windows.Media.Colors.OrangeRed);
            //rightHandEllipse.Fill = yellowBrush;

            //NodeUI.inputGrid.Children.Add(image1);
            //NodeUI.inputGrid.Children.Add(trackingCanvas);
            //trackingCanvas.Children.Add(rightHandEllipse);

<<<<<<< HEAD
            //RegisterAllPorts();
=======
            //NodeUI.RegisterAllPorts();
>>>>>>> 2ec6f35c8f2f9655bb27eff3fb81c69c167a56c6

            //NodeUI.Width = width + 120;// 450;
            //NodeUI.Height = height + 5;

            //NodeUI.Loaded += new RoutedEventHandler(topControl_Loaded);
        }

        //void topControl_Loaded(object sender, RoutedEventArgs e)
        //{
        //    SetupKinect();

        //    nui.VideoFrameReady += new EventHandler<ImageFrameReadyEventArgs>(nui_VideoFrameReady);
        //    nui.DepthFrameReady += new EventHandler<ImageFrameReadyEventArgs>(nui_DepthFrameReady);
        //    nui.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(nui_SkeletonFrameReady);
        //    nui.VideoStream.Open(ImageStreamType.Video, 2, Microsoft.Research.Kinect.Nui.ImageResolution.Resolution640x480, ImageType.Color);

        //    nui.DepthStream.Open(ImageStreamType.Depth, 2, Microsoft.Kinect.ImageResolution.Resolution320x240, ImageType.Depth);
        //}

        public override Value Evaluate(FSharpList<Value> args)
        {
            //if (rightHandPt == null)
            //{
            //    //create a reference point to track the right hand
            //    //rightHandPt = dynSettings.Instance.Doc.Document.FamilyCreate.NewReferencePoint(rightHandLoc);
            //    System.Windows.Point relativePoint = rightHandEllipse.TransformToAncestor(dynSettings.Bench.WorkBench)
            //                  .Transform(new System.Windows.Point(0, 0));
            //    Canvas.SetLeft(rightHandEllipse, relativePoint.X);
            //    Canvas.SetTop(rightHandEllipse, relativePoint.Y);

            //    //add the right hand point at the base of the tree
            //    //this.Tree.Trunk.Leaves.Add(rightHandPt);
            //}

            //if (((Value.Number)args[0]).Item == 1)
            //{
            //    var input = (Element)((Value.Container)args[0]).Item;
            //    double xScale = (double)((Value.Container)args[1]).Item;
            //    double yScale = (double)((Value.Container)args[2]).Item;
            //    double zScale = (double)((Value.Container)args[3]).Item;

            //    //update the image
            //    image1.Source = nui.DepthStream.GetNextFrame(0).ToBitmapSource();

            //    //get skeletonData
            //    SkeletonFrame allSkeletons = nui.SkeletonEngine.GetNextFrame(0);

            //    if (allSkeletons != null)
            //    {
            //        //get the first tracked skeleton
            //        Skeleton skeleton = (from s in allSkeletons.Skeletons
            //                                 where s.TrackingState == SkeletonTrackingState.Tracked
            //                                 select s).FirstOrDefault();

            //        if (skeleton != null)
            //        {
            //            Joint HandRight = skeleton.Joints[JointID.HandRight];
            //            rightHandLoc = new Point3D(HandRight.Position.X * xScale, HandRight.Position.Z * zScale, HandRight.Position.Y * yScale);

            //            SetEllipsePosition(rightHandEllipse, HandRight);

            //            Point3D vec = rightHandLoc - rightHandPt.Position;
            //            Debug.WriteLine(vec.ToString());

            //            //move the reference point
            //            dynSettings.Instance.Doc.Document.Move(rightHandPt, vec);

            //            dynSettings.Instance.Doc.RefreshActiveView();
            //        }
            //    }
            //}


            return Value.NewNumber(0);
        }

        private void SetEllipsePosition(FrameworkElement ellipse, Joint joint)
        {
            //var scaledJoint = joint.ScaleTo(320, 240, .5f, .5f);

            //System.Windows.Point relativePoint = ellipse.TransformToAncestor(dynSettings.Instance.Bench.workBench)
            //                  .Transform(new System.Windows.Point(scaledJoint.Position.X, scaledJoint.Position.Y));

            //Canvas.SetLeft(ellipse, relativePoint.X);
            //Canvas.SetTop(ellipse, relativePoint.Y);

           // Canvas.SetLeft(ellipse, scaledJoint.Position.X);
           // Canvas.SetTop(ellipse, scaledJoint.Position.Y);
        }

        //void nui_VideoFrameReady(object sender, ImageFrameReadyEventArgs e)
        //{
        //    PlanarImage image = e.ImageFrame.Image;
        //    image1.Source = e.ImageFrame.ToBitmapSource();
        //}

        //void nui_DepthFrameReady(object sender, ImageFrameReadyEventArgs e)
        //{
        //    planarImage = e.ImageFrame.Image;
        //    image1.Source = e.ImageFrame.ToBitmapSource();

        //}

        //void nui_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        //{
        //    SkeletonFrame allSkeletons = e.SkeletonFrame;

        //    //get the first tracked skeleton
        //    SkeletonData skeleton = (from s in allSkeletons.Skeletons
        //                             where s.TrackingState == SkeletonTrackingState.Tracked
        //                             select s).FirstOrDefault();

        //    Joint HandRight = skeleton.Joints[JointID.HandRight];
        //    rightHandLoc = new XYZ(HandRight.Position.X, HandRight.Position.Y, HandRight.Position.Z);

        //    //move the reference point
        //    dynSettings.Instance.Doc.Document.Move(rightHandPt, rightHandLoc - rightHandPt.Position);


        //}

        //private void SetupKinect()
        //{
        //    if (Microsoft.Kinect.KinectSensorCollection == 0)
        //    {
        //        this.NodeUI.ToolTipText = "No Kinect connected";
        //    }
        //    else
        //    {
        //        //use first Kinect
        //        nui = Runtime.Kinects[0];         //Initialize to return both Color & Depth images
        //        //nui.Initialize(RuntimeOptions.UseColor | RuntimeOptions.UseDepth);
        //        nui.Initialize(RuntimeOptions.UseDepth | RuntimeOptions.UseSkeletalTracking);
        //    }

        //}
    }
}