//Copyright 2011 Ian Keough

//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at

//http://www.apache.org/licenses/LICENSE-2.0

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.


using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.IO;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Events;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Data;
using TextBox = System.Windows.Controls.TextBox;
using System.Windows.Forms;
using Dynamo.Controls;
using Dynamo.Connectors;
using Dynamo.Utilities;
using System.IO.Ports;
using Microsoft.Research.Kinect.Nui;
using System.Windows;
using System.Windows.Media.Imaging;
using Coding4Fun.Kinect.Wpf;
using System.Windows.Media;

namespace Dynamo.Elements
{
    
    #region interfaces
    public interface IDynamic
    {
        void Draw();
        void Destroy();
    }
    #endregion

    public abstract class dynDouble : dynElement
    {
        public dynDouble()
        {
            OutPortData.Add(new PortData(0.0, "", "dbl", typeof(dynDouble)));
        }

        public override void Draw()
        {
            base.Draw();
        }

        public override void Destroy()
        {

            base.Destroy();
        }

        public override void Update()
        {
            //raise the event for the base class
            //to build, sending this as the 
            OnDynElementReadyToBuild(EventArgs.Empty);
        }
    }

    public abstract class dynBool : dynElement, IDynamic
    {
        public dynBool()
        {
            OutPortData.Add(new PortData(false, "", "Boolean", typeof(dynBool)));

            
        }

        public override void Draw()
        {
            base.Draw();
        }

        public override void Destroy()
        {

            base.Destroy();
        }

        public override void Update()
        {
            //raise the event for the base class
            //to build, sending this as the 
            OnDynElementReadyToBuild(EventArgs.Empty);
        }
    }
   
    public abstract class dynInt : dynElement
    {
        public dynInt()
        {
            OutPortData.Add(new PortData(0, "", "int", typeof(dynInt)));  
        }

        public override void Draw()
        {
            base.Draw();
        }

        public override void Destroy()
        {

            base.Destroy();
        }

        public override void Update()
        {
            //raise the event for the base class
            //to build, sending this as the 
            OnDynElementReadyToBuild(EventArgs.Empty);
        }
    }

    public abstract class dynString : dynElement, IDynamic
    {
        public dynString()
        {
            OutPortData.Add(new PortData(0, "", "st", typeof(dynString)));

            base.RegisterInputsAndOutputs();
        }

        public override void Draw()
        {
            base.Draw();
        }

        public override void Destroy()
        {

            base.Destroy();
        }

        public override void Update()
        {
            //raise the event for the base class
            //to build, sending this as the 
            OnDynElementReadyToBuild(EventArgs.Empty);
        }
    }

    public abstract class dynCurve : dynElement
    {
        public dynCurve()
        {
            OutPortData.Add(new PortData(null, "Cv", "Cv", typeof(dynCurve)));

            base.RegisterInputsAndOutputs();
        }

        public override void Draw()
        {
            base.Draw();
        }

        public override void Destroy()
        {

            base.Destroy();
        }

        public override void Update()
        {
            //raise the event for the base class
            //to build, sending this as the 
            OnDynElementReadyToBuild(EventArgs.Empty);
        }
    }

    public class dynAction : dynElement, IDynamic
    {
        public dynAction()
        {
            InPortData.Add(new PortData(null, "act", "The action to perform.", typeof(dynAction)));
        }

        public virtual void PerformAction()
        {

        }

        public override void Draw()
        {
            base.Draw();
        }

        public override void Destroy()
        {

            base.Destroy();
        }

        public override void Update()
        {
            //raise the event for the base class
            //to build, sending this as the 
            OnDynElementReadyToBuild(EventArgs.Empty);
        }
    }

    [ElementName("Double")]
    [ElementDescription("An element which creates an unsigned floating point number.")]
    [RequiresTransaction(false)]
    public class dynDoubleInput : dynDouble
    {
        TextBox tb;

        public dynDoubleInput()
        {

            //add a text box to the input grid of the control
            tb = new System.Windows.Controls.TextBox();
            tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            inputGrid.Children.Add(tb);
            System.Windows.Controls.Grid.SetColumn(tb, 0);
            System.Windows.Controls.Grid.SetRow(tb, 0);
            tb.Text = "0.0";
            tb.KeyDown += new System.Windows.Input.KeyEventHandler(tb_KeyDown);
            tb.LostFocus += new System.Windows.RoutedEventHandler(tb_LostFocus);

            //turn off the border
            SolidColorBrush backgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
            tb.Background = backgroundBrush;
            tb.BorderThickness = new Thickness(0);

            OutPortData[0].Object = 0.0;

            base.RegisterInputsAndOutputs();
        }

        void tb_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                OutPortData[0].Object = Convert.ToDouble(tb.Text);

                //trigger the ready to build event here
                //because there are no inputs
                OnDynElementReadyToBuild(EventArgs.Empty);
            }
            catch
            {
                OutPortData[0].Object = 0.0;
            }
        }

        void tb_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            //if enter is pressed, update the value
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                TextBox tb = sender as TextBox;

                try
                {
                    OutPortData[0].Object = Convert.ToDouble(tb.Text);

                    //trigger the ready to build event here
                    //because there are no inputs
                    OnDynElementReadyToBuild(EventArgs.Empty);
                }
                catch
                {
                    OutPortData[0].Object = 0.0;
                }

            }

        }

        public override void Update()
        {
            tb.Text = OutPortData[0].Object.ToString();

            OnDynElementReadyToBuild(EventArgs.Empty);
        }
    }

    [ElementName("BooleanSwitch")]
    [ElementDescription("An element which allows selection between a true and false.")]
    [RequiresTransaction(false)]
    public class dynBoolSelector : dynBool
    {
        System.Windows.Controls.RadioButton rbTrue;
        System.Windows.Controls.RadioButton rbFalse;

        public dynBoolSelector()
        {
            //inputGrid.Margin = new System.Windows.Thickness(5,5,20,5);

            //add a text box to the input grid of the control
            rbTrue = new System.Windows.Controls.RadioButton();
            rbFalse = new System.Windows.Controls.RadioButton();
            rbTrue.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            rbFalse.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            
            //use a unique name for the button group
            //so other instances of this element don't get confused
            string groupName = Guid.NewGuid().ToString();
            rbTrue.GroupName = groupName;
            rbFalse.GroupName = groupName;

            rbTrue.Content = "1";
            rbFalse.Content = "0";

            RowDefinition rd = new RowDefinition();
            ColumnDefinition cd1 = new ColumnDefinition();
            ColumnDefinition cd2 = new ColumnDefinition();
            inputGrid.ColumnDefinitions.Add(cd1);
            inputGrid.ColumnDefinitions.Add(cd2);
            inputGrid.RowDefinitions.Add(rd);

            inputGrid.Children.Add(rbTrue);
            inputGrid.Children.Add(rbFalse);

            System.Windows.Controls.Grid.SetColumn(rbTrue, 0);
            System.Windows.Controls.Grid.SetRow(rbTrue, 0);
            System.Windows.Controls.Grid.SetColumn(rbFalse, 1);
            System.Windows.Controls.Grid.SetRow(rbFalse, 0);

            rbFalse.IsChecked = true;
            rbTrue.Checked += new System.Windows.RoutedEventHandler(rbTrue_Checked);
            rbFalse.Checked += new System.Windows.RoutedEventHandler(rbFalse_Checked);
            OutPortData[0].Object = false;

            base.RegisterInputsAndOutputs();
        }

        void rbFalse_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            OutPortData[0].Object = false;

            OnDynElementReadyToBuild(EventArgs.Empty);
        }

        void rbTrue_Checked(object sender, System.Windows.RoutedEventArgs e)
        {

            OutPortData[0].Object = true;

            OnDynElementReadyToBuild(EventArgs.Empty);
        }

        public override void Update()
        {
            OnDynElementReadyToBuild(EventArgs.Empty);
        }
    }

    [ElementName("Int")]
    [ElementDescription("An element which creates a signed integer value.")]
    [RequiresTransaction(false)]
    public class dynIntInput : dynInt
    {
        TextBox tb;

        public dynIntInput()
        {

            //add a text box to the input grid of the control
            tb = new System.Windows.Controls.TextBox();
            tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            inputGrid.Children.Add(tb);
            System.Windows.Controls.Grid.SetColumn(tb, 0);
            System.Windows.Controls.Grid.SetRow(tb, 0);
            tb.Text = "0";
            tb.KeyDown += new System.Windows.Input.KeyEventHandler(tb_KeyDown);
            tb.LostFocus += new System.Windows.RoutedEventHandler(tb_LostFocus);

            //turn off the border
            SolidColorBrush backgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
            tb.Background = backgroundBrush;
            tb.BorderThickness = new Thickness(0);

            OutPortData[0].Object = 0;

            base.RegisterInputsAndOutputs();
        }

        void tb_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                OutPortData[0].Object = Convert.ToInt32(tb.Text);

                //trigger the ready to build event here
                //because there are no inputs
                OnDynElementReadyToBuild(EventArgs.Empty);
            }
            catch
            {
                OutPortData[0].Object = 0;
            }
        }

        void tb_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            //if enter is pressed, update the value
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                TextBox tb = sender as TextBox;

                try
                {
                    OutPortData[0].Object = Convert.ToInt32(tb.Text);

                    //trigger the ready to build event here
                    //because there are no inputs
                    OnDynElementReadyToBuild(EventArgs.Empty);
                }
                catch
                {
                    OutPortData[0].Object = 0;
                }

            }

        }

        public override void Update()
        {
            tb.Text = OutPortData[0].Object.ToString();

            OnDynElementReadyToBuild(EventArgs.Empty);
        }
    }

    #region element types

    public enum COMPort { COM3, COM4 };

    [ElementName("Arduino")]
    [ElementDescription("An element which allows you to read from an Arduino microcontroller.")]
    [RequiresTransaction(false)]
    public class dynArduino : dynElement, IDynamic
    {
        SerialPort port;
        string lastData = "";
        COMPort portState;
        System.Windows.Controls.MenuItem com4Item;
        System.Windows.Controls.MenuItem com3Item;

        public dynArduino()
        {
            //InPortData.Add(new PortData(null, "loop", "The loop to execute.", typeof(dynLoop)));
            InPortData.Add(new PortData(null, "i/o", "Switch Arduino on?", typeof(dynBool)));
            InPortData.Add(new PortData(null, "tim", "How often to receive updates.", typeof(dynTimer)));

            OutPortData.Add(new PortData(null, "S", "Serial output", typeof(dynDouble)));
            //OutPortData[0].Object = this.Tree;

            base.RegisterInputsAndOutputs();

            port = new SerialPort("COM3", 9600);
            port.NewLine = "\r\n";
            port.DtrEnable = true;
            //port.DataReceived += new SerialDataReceivedEventHandler(serialPort1_DataReceived);

            com3Item = new System.Windows.Controls.MenuItem();
            com3Item.Header = "COM3";
            com3Item.IsCheckable = true;
            com3Item.IsChecked = true;
            com3Item.Checked += new System.Windows.RoutedEventHandler(com3Item_Checked);

            com4Item = new System.Windows.Controls.MenuItem();
            com4Item.Header = "COM4";
            com4Item.IsCheckable = true;
            com4Item.IsChecked = false;
            com4Item.Checked += new System.Windows.RoutedEventHandler(com4Item_Checked);

            this.MainContextMenu.Items.Add(com3Item);
            this.MainContextMenu.Items.Add(com4Item);
            portState = COMPort.COM3;
            port.PortName = "COM3";
        }

        void com4Item_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            portState = COMPort.COM4;
            com4Item.IsChecked = true;
            com3Item.IsChecked = false;
        }

        void com3Item_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            portState = COMPort.COM3;
            com4Item.IsChecked = false;
            com3Item.IsChecked = true;
        }

        public override void Draw()
        {
            if(CheckInputs())
            {
                //add one branch
                //this.Tree.Trunk.Branches.Add(new DataTreeBranch());
                //this.Tree.Trunk.Branches[0].Leaves.Add(null);

                if (port != null)
                {
                    bool isOpen = Convert.ToBoolean(InPortData[0].Object);

                    if(isOpen == true)
                    {
                        if (!port.IsOpen)
                        {
                            if (portState == COMPort.COM3)
                                port.PortName = "COM3";
                            else
                                port.PortName = "COM4";

                            port.Open();
                        }

                        //get the analog value from the serial port
                        GetArduinoData();

                        //i don't know why this works 
                        //but OnDynElementReadyToBuild doesn't
                        this.UpdateOutputs();
                        //OnDynElementReadyToBuild(EventArgs.Empty);
                    }
                    else if(isOpen == false)
                    {
                        if (port.IsOpen)
                            port.Close();
                    }

                }

            }
        }

        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            if (CheckInputs())
            {
                //add one branch
                //this.Tree.Trunk.Branches.Add(new DataTreeBranch());
                //this.Tree.Trunk.Branches[0].Leaves.Add(null);

                if (port != null)
                {
                    bool isOpen = Convert.ToBoolean(InPortData[0].Object);

                    if (isOpen == true)
                    {
                        if (!port.IsOpen)
                        {
                            port.Open();
                        }

                        //get the analog value from the serial port
                        GetArduinoData();

                        //i don't know why this works 
                        //but OnDynElementReadyToBuild doesn't
                        this.UpdateOutputs();
                        //OnDynElementReadyToBuild(EventArgs.Empty);
                    }
                    else if (isOpen == false)
                    {
                        if (port.IsOpen)
                            port.Close();
                    }

                }

            }
        }

        private void GetArduinoData()
        {
            //string data = port.ReadExisting();
            //lastData += data;
            //string[] allData = lastData.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            //if (allData.Length > 0)
            //{
                //lastData = allData[allData.Length - 1];
                //this.Tree.Trunk.Branches[0].Leaves[0] = lastData;
                //this.OutPortData[0].Object = Convert.ToDouble(lastData);
            //}

            int data = 255;
            while (port.BytesToRead > 0)
            {
                data = port.ReadByte();
            }

            this.OutPortData[0].Object = Convert.ToDouble(data);
        }

        public override void Update()
        {
            OnDynElementReadyToBuild(EventArgs.Empty);
        }

    }

    [ElementName("Kinect")]
    [ElementDescription("An element which allows you to read from a Kinect.")]
    [RequiresTransaction(true)]
    public class dynKinect : dynElement, IDynamic
    {
        //Kinect Runtime
        Runtime nui;
        Image image1;
        PlanarImage planarImage;
        XYZ rightHandLoc = new XYZ();
        ReferencePoint rightHandPt;
        System.Windows.Shapes.Ellipse rightHandEllipse;

        public dynKinect()
        {
            InPortData.Add(new PortData(null, "tim", "How often to receive updates.", typeof(dynTimer)));
            InPortData.Add(new PortData(null, "X scale", "The amount to scale the skeletal measurements in the X direction.", typeof(dynDouble)));
            InPortData.Add(new PortData(null, "Y scale", "The amount to scale the skeletal measurements in the Y direction.", typeof(dynDouble)));
            InPortData.Add(new PortData(null, "Z scale", "The amount to scale the skeletal measurements in the Z direction.", typeof(dynDouble)));

            OutPortData.Add(new PortData(null, "Hand", "Reference point representing hand location", typeof(dynReferencePoint)));
            OutPortData[0].Object = this.Tree;

            image1 = new Image();
            image1.Width = 320;
            image1.Height = 240;
            image1.Margin = new Thickness(5);
            image1.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            image1.Name = "image1";
            image1.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            //image1.Margin = new Thickness(0, 0, 0, 0);

            Canvas trackingCanvas = new Canvas();
            trackingCanvas.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            trackingCanvas.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;

            //add an ellipse to track the hand
            rightHandEllipse = new System.Windows.Shapes.Ellipse();
            rightHandEllipse.Height = 10;
            rightHandEllipse.Width = 10;
            rightHandEllipse.Name = "rightHandEllipse";
            SolidColorBrush yellowBrush = new SolidColorBrush(System.Windows.Media.Colors.OrangeRed);
            rightHandEllipse.Fill = yellowBrush;
            
            this.inputGrid.Children.Add(image1);
            this.inputGrid.Children.Add(trackingCanvas);
            trackingCanvas.Children.Add(rightHandEllipse);

            base.RegisterInputsAndOutputs();

            this.Width = 450;
            this.Height = 240 + 5;

            this.Loaded += new RoutedEventHandler(topControl_Loaded);

            
        }

        void topControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetupKinect();
            //nui.VideoFrameReady += new EventHandler<ImageFrameReadyEventArgs>(nui_VideoFrameReady);
            //nui.DepthFrameReady += new EventHandler<ImageFrameReadyEventArgs>(nui_DepthFrameReady);
            //nui.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(nui_SkeletonFrameReady);
            //nui.VideoStream.Open(ImageStreamType.Video, 2, Microsoft.Research.Kinect.Nui.ImageResolution.Resolution640x480, ImageType.Color);
            nui.DepthStream.Open(ImageStreamType.Depth, 2, Microsoft.Research.Kinect.Nui.ImageResolution.Resolution320x240, ImageType.Depth);
        }

        public override void Draw()
        {
            if (rightHandPt == null)
            {
                //create a reference point to track the right hand
                rightHandPt = dynElementSettings.SharedInstance.Doc.Document.FamilyCreate.NewReferencePoint(rightHandLoc);
                System.Windows.Point relativePoint = rightHandEllipse.TransformToAncestor(dynElementSettings.SharedInstance.Bench.workBench)
                              .Transform(new System.Windows.Point(0, 0));
                Canvas.SetLeft(rightHandEllipse, relativePoint.X);
                Canvas.SetTop(rightHandEllipse, relativePoint.Y);

                //add the right hand point at the base of the tree
                this.Tree.Trunk.Leaves[0]=rightHandPt;
            }

            if (CheckInputs())
            {
                double xScale = Convert.ToDouble(InPortData[1].Object);
                double yScale = Convert.ToDouble(InPortData[2].Object);
                double zScale = Convert.ToDouble(InPortData[3].Object);

                //update the image
                image1.Source = nui.DepthStream.GetNextFrame(0).ToBitmapSource();

                //get skeletonData
                SkeletonFrame allSkeletons = nui.SkeletonEngine.GetNextFrame(0);

                if (allSkeletons != null)
                {
                    //get the first tracked skeleton
                    SkeletonData skeleton = (from s in allSkeletons.Skeletons
                                             where s.TrackingState == SkeletonTrackingState.Tracked
                                             select s).FirstOrDefault();

                    if (skeleton != null)
                    {
                        Joint HandRight = skeleton.Joints[JointID.HandRight];
                        rightHandLoc = new XYZ(HandRight.Position.X * xScale, HandRight.Position.Z*zScale, HandRight.Position.Y*yScale);

                        SetEllipsePosition(rightHandEllipse, HandRight);

                        XYZ vec = rightHandLoc - rightHandPt.Position;
                        Debug.WriteLine(vec.ToString());

                        //move the reference point
                        dynElementSettings.SharedInstance.Doc.Document.Move(rightHandPt, vec);

                        dynElementSettings.SharedInstance.Doc.RefreshActiveView();
                    }
                }
            }

        }

        public override void Destroy()
        {
            //don't call destroy
            //base.Destroy();
        }
        public override void Update()
        {
            OnDynElementReadyToBuild(EventArgs.Empty);
        }

        private void SetEllipsePosition(FrameworkElement ellipse, Joint joint)
        {
            var scaledJoint = joint.ScaleTo(320, 240, .5f, .5f);

            //System.Windows.Point relativePoint = ellipse.TransformToAncestor(dynElementSettings.SharedInstance.Bench.workBench)
            //                  .Transform(new System.Windows.Point(scaledJoint.Position.X, scaledJoint.Position.Y));

            //Canvas.SetLeft(ellipse, relativePoint.X);
            //Canvas.SetTop(ellipse, relativePoint.Y);

            Canvas.SetLeft(ellipse, scaledJoint.Position.X);
            Canvas.SetTop(ellipse, scaledJoint.Position.Y);
        }

        //void nui_VideoFrameReady(object sender, ImageFrameReadyEventArgs e)
        //{
        //    PlanarImage image = e.ImageFrame.Image;
        //    image1.Source = e.ImageFrame.ToBitmapSource();
        //}

        void nui_DepthFrameReady(object sender, ImageFrameReadyEventArgs e)
        {
            planarImage = e.ImageFrame.Image;
            image1.Source = e.ImageFrame.ToBitmapSource();
            
        }

        void nui_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            SkeletonFrame allSkeletons = e.SkeletonFrame;

            //get the first tracked skeleton
            SkeletonData skeleton = (from s in allSkeletons.Skeletons
                                     where s.TrackingState == SkeletonTrackingState.Tracked
                                     select s).FirstOrDefault();

            Joint HandRight = skeleton.Joints[JointID.HandRight];
            rightHandLoc = new XYZ(HandRight.Position.X, HandRight.Position.Y, HandRight.Position.Z);

            //move the reference point
            dynElementSettings.SharedInstance.Doc.Document.Move(rightHandPt, rightHandLoc - rightHandPt.Position);

            
        }

        private void SetupKinect()
        {
            if (Runtime.Kinects.Count == 0)
            {
                this.inputGrid.ToolTip = "No Kinect connected";
            }
            else
            {
                //use first Kinect
                nui = Runtime.Kinects[0];         //Initialize to return both Color & Depth images
                //nui.Initialize(RuntimeOptions.UseColor | RuntimeOptions.UseDepth);
                nui.Initialize(RuntimeOptions.UseDepth | RuntimeOptions.UseSkeletalTracking);
            }
 
        }
    }

    [ElementName("Timer")]
    [ElementDescription("An element which allows you to specify an update frequency in milliseconds.")]
    [RequiresTransaction(false)]
    public class dynTimer : dynElement, IDynamic
    {
        Stopwatch sw;
        bool timing = false;

        public dynTimer()
        {
            InPortData.Add(new PortData(null, "n", "How often to receive updates in milliseconds.", typeof(dynInt)));
            InPortData.Add(new PortData(null, "i/o", "Turn the timer on or off", typeof(dynBool)));
            OutPortData.Add(new PortData(null, "tim", "The timer, counting in milliseconds.", typeof(dynTimer)));
            OutPortData[0].Object = this;

            base.RegisterInputsAndOutputs();

            sw = new Stopwatch();

        }

        void KeepTime()
        {
            if (CheckInputs())
            {
                bool on = Convert.ToBoolean(InPortData[1].Object);
                if (on)
                {
                    int interval = Convert.ToInt16(InPortData[0].Object);

                    if (sw.ElapsedMilliseconds > interval)
                    {
                        sw.Stop();
                        sw.Reset();
                        OnDynElementReadyToBuild(EventArgs.Empty);
                        sw.Start();
                    }
                }
            }
        }

        public override void Draw()
        {
            if (CheckInputs())
            {
                bool isTiming = Convert.ToBoolean(InPortData[0].Object);

                if (timing)
                {
                    if (!isTiming)  //if you are timing and we turn off the timer
                    {
                        timing = false; //stop
                        sw.Stop();
                        sw.Reset();
                    }
                }
                else
                {
                    if (isTiming)
                    {
                        timing = true;  //if you are not timing and we turn on the timer
                        sw.Start();
                        while (timing)
                        {
                            KeepTime();
                        }
                    }
                }
            }
        }

        public override void Update()
        {
            OnDynElementReadyToBuild(EventArgs.Empty);
        }

    }

    //[ElementName("Move")]
    //[ElementDescription("Create an element which moves other elements by a fixed distance in each step.")]
    //[RequiresTransaction(true)]
    //public class dynMove : dynAction, IDynamic
    //{
    //    public dynMove(string nickName)
    //        : base(nickName)
    //    {
    //        InPortData.Add(new PortData(null, "XYZ", "Movement vector.", typeof(dynXYZ)));
    //        InPortData.Add(new PortData(null, "El", "The element to move.", typeof(dynElement)));

    //        base.RegisterInputsAndOutputs();
    //    }

    //    /// <summary>
    //    /// Called by dynLoop elements to create iterative behaviors
    //    /// </summary>
    //    public override void PerformAction()
    //    {
    //        dynElementSettings.SharedInstance.Doc.Document.Move(InPortData[1].Object as Element, InPortData[0].Object as XYZ);
    //    }

    //    public override void Draw()
    //    {
    //        OutPortData[0].Object = this;
    //    }

    //    public override void Destroy()
    //    {
    //        base.Destroy();
    //    }

    //    public override void Update()
    //    {
    //        OnDynElementReadyToBuild(EventArgs.Empty);
    //    }
    //}

    [ElementName("Watch")]
    [ElementDescription("Create an element for watching the results of other operations.")]
    [RequiresTransaction(false)]
    public class dynWatch : dynElement, IDynamic, INotifyPropertyChanged
    {
        //System.Windows.Controls.Label label;
        //System.Windows.Controls.ListBox listBox;
        System.Windows.Controls.TextBox tb;

        string watchValue;

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public string WatchValue
        {
            get { return watchValue; }
            set
            {
                watchValue = value;
                NotifyPropertyChanged("WatchValue");
            }
        }

        public dynWatch()
        {

            //add a list box
            System.Windows.Controls.TextBox tb = new System.Windows.Controls.TextBox();
            tb.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;

            //turn off the border
            SolidColorBrush backgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
            tb.Background = backgroundBrush;
            tb.BorderThickness = new Thickness(0);

            WatchValue = "Ready to watch!";
            
            //http://learnwpf.com/post/2006/06/12/How-can-I-create-a-data-binding-in-code-using-WPF.aspx

            System.Windows.Data.Binding b = new System.Windows.Data.Binding("WatchValue");
            b.Source = this;
            //label.SetBinding(System.Windows.Controls.Label.ContentProperty, b);
            tb.SetBinding(System.Windows.Controls.TextBox.TextProperty, b);

            //this.inputGrid.Children.Add(label);
            this.inputGrid.Children.Add(tb);
            tb.TextWrapping = System.Windows.TextWrapping.Wrap;
            tb.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            //tb.AcceptsReturn = true;

            InPortData.Add(new PortData(null, "", "The Element to watch", typeof(dynElement)));


            base.RegisterInputsAndOutputs();

            //resize the panel
            this.topControl.Height = 100;
            this.topControl.Width = 300;
            UpdateLayoutDelegate uld = new UpdateLayoutDelegate(CallUpdateLayout);
            Dispatcher.Invoke(uld, System.Windows.Threading.DispatcherPriority.Background, new object[] { this }); 
            //this.UpdateLayout();
        }

        public override void Draw()
        {
            if (CheckInputs())
            {
                WatchValue = InPortData[0].Object.ToString();
                UpdateLayoutDelegate uld = new UpdateLayoutDelegate(CallUpdateLayout);
                Dispatcher.Invoke(uld, System.Windows.Threading.DispatcherPriority.Background, new object[] { this });

            }
        }


        public override void Destroy()
        {
            base.Destroy();
        }

        public override void Update()
        {
            //this.topControl.Height = 400;
            OnDynElementReadyToBuild(EventArgs.Empty);
        }
    }

    #endregion

    #region class attributes
    [AttributeUsage(AttributeTargets.All)]
    public class ElementNameAttribute : System.Attribute
    {
        private string elementName;
        

        public string ElementName
        {
            get { return elementName; }
            set { elementName = value; }
        }
        
        public ElementNameAttribute(string elementName)
        {
            this.elementName = elementName;
        }
    }

    [AttributeUsage(AttributeTargets.All)]
    public class RequiresTransactionAttribute : System.Attribute
    {
        private bool requiresTransaction;

        public bool RequiresTransaction
        {
            get { return requiresTransaction; }
            set { requiresTransaction = value; }
        }

        public RequiresTransactionAttribute(bool requiresTransaction)
        {
            this.requiresTransaction = requiresTransaction;
        }
    }

    [AttributeUsage(AttributeTargets.All)]
    public class ElementDescriptionAttribute : System.Attribute
    {   
        private string description;
        
        public string ElementDescription
        {
            get { return description; }
            set { description = value; }
        }

        public ElementDescriptionAttribute(string description)
        {
            this.description = description;
        }
    }
    #endregion

}
