<<<<<<< HEAD
﻿//Copyright © Autodesk, Inc. 2012. All rights reserved.
=======
//Copyright © Autodesk, Inc. 2012. All rights reserved.
>>>>>>> 2ec6f35c8f2f9655bb27eff3fb81c69c167a56c6
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.
//
// Author:  Hans Kellner
// Created: 2013.01.28
//
// Requirements: The Leap Motion SDK (http://www.leapmotion.com/) - v0.7.3_2234
//
// Install the following Leap DLLs into the folder containing the DynamoRevit.dll:
//
// _LeapCSharp.dll, (Leap.dll | Leapd.dll), (LeapCSharp.NET4.0.dll | LeapCSharp.NET3.5.dll)
//

using System;
<<<<<<< HEAD
using System.Windows.Media.Media3D;
using Microsoft.FSharp.Collections;
using Dynamo.Connectors;
using Value = Dynamo.FScheme.Value;
using Dynamo.Controls;

=======
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Media.Media3D;
using Microsoft.FSharp.Collections;

using Dynamo.Connectors;
using Dynamo.Utilities;
using Dynamo.FSchemeInterop;
using Value = Dynamo.FScheme.Value;
using Dynamo.Controls;

using Leap;
>>>>>>> 2ec6f35c8f2f9655bb27eff3fb81c69c167a56c6

namespace Dynamo.Nodes
{
    [NodeName("Leap")]
    [NodeCategory(BuiltinNodeCategories.COMMUNICATION)]
    [NodeDescription("Manages connection to a Leap Motion ViewModel.")]
    public class dynLeapController : dynNodeWithOneOutput
    {
        System.Windows.Controls.MenuItem menuItemLeapEnabled = null;

        static object controllerLock = new object();
        static Leap.Controller leapController = null;

        public dynLeapController()
        {
            // This input will drive the acquire of a frame of data from the Leap.
            // Attaching a timer node to it will then generate a flow of data from
            // the Leap into the system.
            InPortData.Add(new PortData("Read", "Read a frame of data from the Leap", typeof(object)));

            // Output from this node is the data from a single frame
            OutPortData.Add(new PortData("Leap", "The Leap ViewModel", typeof(object)));

            RegisterAllPorts();

            LeapEnable(true);
        }

        public override void SetupCustomUIElements(dynNodeView NodeUI)
        {
            // Create a menuitem to enable/disable the Leap device
            menuItemLeapEnabled = new System.Windows.Controls.MenuItem();
            menuItemLeapEnabled.Header = "Enable Leap";
            menuItemLeapEnabled.IsCheckable = true;
            menuItemLeapEnabled.IsChecked = (leapController != null);
            menuItemLeapEnabled.Checked += new System.Windows.RoutedEventHandler(menuItemLeapEnabled_Checked);
<<<<<<< HEAD
=======

>>>>>>> 2ec6f35c8f2f9655bb27eff3fb81c69c167a56c6
            NodeUI.MainContextMenu.Items.Add(menuItemLeapEnabled);
        }

        void menuItemLeapEnabled_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            menuItemLeapEnabled.IsChecked = !menuItemLeapEnabled.IsChecked; // toggle state

            if (menuItemLeapEnabled.IsChecked)
                menuItemLeapEnabled.Header = "Disable Leap";
            else
                menuItemLeapEnabled.Header = "Enable Leap";

            LeapEnable(menuItemLeapEnabled.IsChecked);
        }

        public override void Cleanup()
        {
            LeapEnable(false);
            base.Cleanup();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            return Value.NewContainer(leapController);
        }

        public static Leap.Controller CurrentLeapController
        {
            get { return leapController; }
        }

        private static void LeapEnable(bool bEnable)
        {
            lock (controllerLock)
            {
                bool bEnabled = (leapController != null);
                if (bEnable == bEnabled)
                    return;

                if (bEnable)
                {
                    leapController = new Leap.Controller();
                }
                else
                {
                    leapController.Dispose();
                    leapController = null;
                }
            }
        }
    }

    [NodeName("Leap Frame N")]
    [NodeCategory(BuiltinNodeCategories.COMMUNICATION)]
    [NodeDescription("Frame from the Leap Motion ViewModel.")]
    public class dynLeapFrameN : dynNodeWithOneOutput
    {
        public dynLeapFrameN()
        {
            InPortData.Add(new PortData("Leap", "The Leap ViewModel", typeof(object)));
            InPortData.Add(new PortData("Age", "The age of the frame to retrieve (0 == current, 1..N previous).", typeof(int)));

            OutPortData.Add(new PortData("Frame", "The frame of data", typeof(object)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Leap.Controller controller = (Leap.Controller)((Value.Container)args[0]).Item;
            if (controller == null)
                throw new Exception("No Leap ViewModel attached.");

            int age = (int)((Value.Number)args[1]).Item;
            if (age < 0)
                throw new Exception("Leap Frame Age must be >= 0");

            Leap.Frame frame = controller.Frame(age);

            return Value.NewContainer(frame);
        }
    }

    [NodeName("Leap Frame")]
    [NodeCategory(BuiltinNodeCategories.COMMUNICATION)]
    [NodeDescription("Current Frame from the Leap Motion ViewModel.")]
    public class dynLeapFrame : dynNodeWithOneOutput
    {
        public dynLeapFrame()
        {
            InPortData.Add(new PortData("Leap", "The Leap ViewModel", typeof(object)));

            OutPortData.Add(new PortData("Frame", "The frame of data", typeof(object)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Leap.Controller controller = (Leap.Controller)((Value.Container)args[0]).Item;
            if (controller == null)
                throw new Exception("No Leap ViewModel attached.");

            Leap.Frame frame = controller.Frame();

            //string str;
            //if (frame != null)
            //    str = "   Frame id : " + frame.Id
            //        + "\ntimestamp : " + frame.Timestamp
            //        + "\n    hands : " + frame.Hands.Count
            //        + "\n  fingers : " + frame.Fingers.Count
            //        + "\n    tools : " + frame.Tools.Count;
            //else
            //    str = "Leap Disabled";

            return Value.NewContainer(frame);
        }
    }

    [NodeName("Leap Frame Scale Factor")]
    [NodeCategory(BuiltinNodeCategories.COMMUNICATION)]
    [NodeDescription("The scale factor derived from the overall motion between the current frame and the specified frame.")]
    public class dynLeapFrameScaleFactor : dynNodeWithOneOutput
    {
        public dynLeapFrameScaleFactor()
        {
            InPortData.Add(new PortData("Frame", "A Frame from a Leap ViewModel", typeof(object)));
            InPortData.Add(new PortData("Age", "The age of the previous frame (1..N).", typeof(int)));

            OutPortData.Add(new PortData("ScaleFactor", "The scale factor", typeof(double)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Leap.Frame frame = (Leap.Frame)((Value.Container)args[0]).Item;
            if (frame == null)
                throw new Exception("No Leap Frame.");

            int age = 1;
            if (args.Length > 1 && args[1].IsNumber)
            {
                age = (int)((Value.Number)args[1]).Item;
                if (age < 0)
                    throw new Exception("Leap Frame Age must be >= 0");
            }

            Leap.Controller controller = dynLeapController.CurrentLeapController;
            if (controller == null)
                throw new Exception("No Leap ViewModel node.");

            Leap.Frame sinceFrame = controller.Frame(age);

            return Value.NewNumber(frame.ScaleFactor(sinceFrame));
        }
    }

    [NodeName("Leap Frame Translation")]
    [NodeCategory(BuiltinNodeCategories.COMMUNICATION)]
    [NodeDescription("The change of position derived from the overall linear motion between the current frame and the specified frame.")]
    public class dynLeapFrameTranslation : dynNodeWithOneOutput
    {
        public dynLeapFrameTranslation()
        {
            InPortData.Add(new PortData("Frame", "A Frame from a Leap ViewModel", typeof(object)));
            InPortData.Add(new PortData("Age", "The age of the previous frame.", typeof(int)));

            OutPortData.Add(new PortData("Translation", "The translation vector", typeof(Point3D)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Leap.Frame frame = (Leap.Frame)((Value.Container)args[0]).Item;
            if (frame == null)
                throw new Exception("No Leap Frame.");

            // Age is optional;  Defaults to 1
            int age = 1;
            if (args.Length > 1 && args[1].IsNumber)
            {
                age = (int)((Value.Number)args[1]).Item;
                if (age < 0)
                    throw new Exception("Leap Frame Age must be >= 0");
            }

            Leap.Controller controller = dynLeapController.CurrentLeapController;
            if (controller == null)
                throw new Exception("No Leap ViewModel node.");

            Leap.Vector v;
            Leap.Frame sinceFrame = controller.Frame(age);
            if (sinceFrame != null && sinceFrame.IsValid)
                v = frame.Translation(sinceFrame);
            else
                v = new Leap.Vector();

            return Value.NewContainer(new Point3D(v.x, v.y, v.z));
        }
    }

    [NodeName("Leap Frame Rotation")]
    [NodeCategory(BuiltinNodeCategories.COMMUNICATION)]
    [NodeDescription("The angle of rotation around the XYZ axis' derived from the overall rotational motion between the current frame and the specified frame.")]
    public class dynLeapFrameRotation : dynNodeWithOneOutput
    {
        public dynLeapFrameRotation()
        {
            InPortData.Add(new PortData("Frame", "A Frame from a Leap ViewModel", typeof(object)));
            InPortData.Add(new PortData("Age", "The age of the previous frame.", typeof(int)));

            OutPortData.Add(new PortData("Rotation", "The XYZ rotation in radians", typeof(Point3D)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Leap.Frame frame = (Leap.Frame)((Value.Container)args[0]).Item;
            if (frame == null)
                throw new Exception("No Leap Frame.");

            // Age is optional;  Defaults to 1
            int age = 1;
            if (args.Length > 1 && args[1].IsNumber)
            {
                age = (int)((Value.Number)args[1]).Item;
                if (age < 0)
                    throw new Exception("Leap Frame Age must be >= 0");
            }

            Leap.Controller controller = dynLeapController.CurrentLeapController;
            if (controller == null)
                throw new Exception("No Leap ViewModel node.");

            Leap.Frame sinceFrame = controller.Frame(age);

            float angleX = frame.RotationAngle(sinceFrame, new Leap.Vector((float)1.0, (float)0.0, (float)0.0));
            float angleY = frame.RotationAngle(sinceFrame, new Leap.Vector((float)0.0, (float)1.0, (float)0.0));
            float angleZ = frame.RotationAngle(sinceFrame, new Leap.Vector((float)0.0, (float)0.0, (float)1.0));

            return Value.NewContainer(new Point3D(angleX, angleY, angleZ));
        }
    }

    [NodeName("Leap Hand")]
    [NodeCategory(BuiltinNodeCategories.COMMUNICATION)]
    [NodeDescription("Reads a hand from a Leap Motion frame.")]
    public class dynLeapHand : dynNodeWithOneOutput
    {
        public dynLeapHand()
        {
            InPortData.Add(new PortData("Frame", "The frame of data from the Leap device", typeof(object)));
            InPortData.Add(new PortData("Index", "The index of the hand to read (1..N).", typeof(int)));

            OutPortData.Add(new PortData("Hand", "The hand read from the frame", typeof(object)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Leap.Frame frame = (Leap.Frame)((Value.Container)args[0]).Item;

            // Index of the hand
            int idx = -1;
            if (args.Length > 1 && args[1].IsNumber)
                idx = ((int)((Value.Number)args[1]).Item) - 1;

            Leap.Hand hand = Leap.Hand.Invalid;

            if (frame != null)
            {
                if (idx >= 0 && idx < frame.Hands.Count)
                    hand = frame.Hands[idx];
            }

            return Value.NewContainer(hand);
        }
    }

    [NodeName("Leap Hand 1")]
    [NodeCategory(BuiltinNodeCategories.COMMUNICATION)]
    [NodeDescription("Reads a hand #1 from a Leap Motion frame.")]
    public class dynLeapHand1 : dynNodeWithOneOutput
    {
        public dynLeapHand1()
        {
            InPortData.Add(new PortData("Frame", "The frame of data from the Leap device", typeof(object)));

            OutPortData.Add(new PortData("Hand", "The hand read from the frame", typeof(object)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Leap.Frame frame = (Leap.Frame)((Value.Container)args[0]).Item;

            Leap.Hand hand = Leap.Hand.Invalid;

            if (frame != null)
            {
                if (frame.Hands.Count > 0)
                    hand = frame.Hands[0];
            }

            return Value.NewContainer(hand);
        }
    }

    [NodeName("Leap Hand 2")]
    [NodeCategory(BuiltinNodeCategories.COMMUNICATION)]
    [NodeDescription("Reads a hand #1 from a Leap Motion frame.")]
    public class dynLeapHand2 : dynNodeWithOneOutput
    {
        public dynLeapHand2()
        {
            InPortData.Add(new PortData("Frame", "The frame of data from the Leap device", typeof(object)));

            OutPortData.Add(new PortData("Hand", "The hand read from the frame", typeof(object)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Leap.Frame frame = (Leap.Frame)((Value.Container)args[0]).Item;

            Leap.Hand hand = Leap.Hand.Invalid;

            if (frame != null)
            {
                if (frame.Hands.Count > 1)
                    hand = frame.Hands[1];
            }

            return Value.NewContainer(hand);
        }
    }

    [NodeName("Leap Fingers")]
    [NodeCategory(BuiltinNodeCategories.COMMUNICATION)]
    [NodeDescription("Reads the list of fingers from a Leap Motion hand.")]
    public class dynLeapFingers : dynNodeWithOneOutput
    {
        public dynLeapFingers()
        {
            InPortData.Add(new PortData("Hand", "The hand containing the fingers.", typeof(object)));

            OutPortData.Add(new PortData("Fingers", "The list of fingers.", typeof(object)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            FSharpList<Value> fingers = FSharpList<Value>.Empty;

            Leap.Hand hand = (Leap.Hand)((Value.Container)args[0]).Item;

            if (hand != null && hand.IsValid)
            {
                foreach (Leap.Finger finger in hand.Fingers)
                    fingers = FSharpList<Value>.Cons(Value.NewContainer(finger), fingers);
            }

            return Value.NewList(fingers);
        }
    }

    public abstract class dynLeapFinger : dynNodeWithOneOutput
    {
        public dynLeapFinger()
        {
            FingerIndex = 0;

            InPortData.Add(new PortData("Hand", "The hand containing the finger.", typeof(object)));

            OutPortData.Add(new PortData("Finger", "The finger data.", typeof(object)));
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Leap.Hand hand = (Leap.Hand)((Value.Container)args[0]).Item;

            Leap.Finger finger = Leap.Finger.Invalid;

            if (hand != null && hand.IsValid)
            {
                if (FingerIndex >= 0 && FingerIndex < hand.Fingers.Count)
                    finger = hand.Fingers[FingerIndex];
            }

            return Value.NewContainer(finger);
        }

        public int FingerIndex
        {
            get { return _fingerIndex; }
            protected set { _fingerIndex = value; }
        }

        protected int _fingerIndex;
    }

    [NodeName("Leap Finger N")]
    [NodeCategory(BuiltinNodeCategories.COMMUNICATION)]
    [NodeDescription("Reads a finger with a specified index from a Leap Motion hand.")]
    public class dynLeapFingerN : dynLeapFinger
    {
        public dynLeapFingerN()
        {
            InPortData.Add(new PortData("Index", "The index of the finger to read (1..N).", typeof(int)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            // Grab and set the finger index
            if (args.Length > 1 && args[1].IsNumber)
                FingerIndex = ((int)((Value.Number)args[1]).Item) - 1;
            else
                FingerIndex = -1;

            return base.Evaluate(args);
        }
    }

    [NodeName("Leap Finger 1")]
    [NodeCategory(BuiltinNodeCategories.COMMUNICATION)]
    [NodeDescription("Reads finger #1 from a Leap Motion hand.")]
    public class dynLeapFinger1 : dynLeapFinger
    {
        public dynLeapFinger1()
        {
            FingerIndex = 0;    // 1st finger

            RegisterAllPorts();
        }
    }

    [NodeName("Leap Finger 2")]
    [NodeCategory(BuiltinNodeCategories.COMMUNICATION)]
    [NodeDescription("Reads finger #2 from a Leap Motion hand.")]
    public class dynLeapFinger2 : dynLeapFinger
    {
        public dynLeapFinger2()
        {
            FingerIndex = 1;    // 2nd finger

            RegisterAllPorts();
        }
    }

    [NodeName("Leap Finger 3")]
    [NodeCategory(BuiltinNodeCategories.COMMUNICATION)]
    [NodeDescription("Reads finger #3 from a Leap Motion hand.")]
    public class dynLeapFinger3 : dynLeapFinger
    {
        public dynLeapFinger3()
        {
            FingerIndex = 2;    // 3rd finger

            RegisterAllPorts();
        }
    }

    [NodeName("Leap Finger 4")]
    [NodeCategory(BuiltinNodeCategories.COMMUNICATION)]
    [NodeDescription("Reads finger #4 from a Leap Motion hand.")]
    public class dynLeapFinger4 : dynLeapFinger
    {
        public dynLeapFinger4()
        {
            FingerIndex = 3;    // 4th finger

            RegisterAllPorts();
        }
    }

    [NodeName("Leap Finger 5")]
    [NodeCategory(BuiltinNodeCategories.COMMUNICATION)]
    [NodeDescription("Reads finger #5 from a Leap Motion hand.")]
    public class dynLeapFinger5 : dynLeapFinger
    {
        public dynLeapFinger5()
        {
            FingerIndex = 4;    // 5th finger

            RegisterAllPorts();
        }
    }

    public class dynLeapTool : dynNodeWithOneOutput
    {
        public dynLeapTool()
        {
            ToolIndex = 0;

            InPortData.Add(new PortData("Frame", "The frame of data from the Leap Motion ViewModel", typeof(object)));

            OutPortData.Add(new PortData("Tool", "The tool data.", typeof(object)));
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Leap.Frame frame = (Leap.Frame)((Value.Container)args[0]).Item;

            Leap.Tool tool = Leap.Tool.Invalid;

            if (frame != null && frame.IsValid)
            {
                if (ToolIndex >= 0 && ToolIndex < frame.Tools.Count)
                    tool = frame.Tools[ToolIndex];
            }

            return Value.NewContainer(tool);
        }

        public int ToolIndex
        {
            get { return _toolIndex; }
            protected set { _toolIndex = value; }
        }

        protected int _toolIndex;
    }

    [NodeName("Leap Tool N")]
    [NodeCategory(BuiltinNodeCategories.COMMUNICATION)]
    [NodeDescription("Reads a tool with a specified index from a Leap Motion ViewModel.")]
    public class dynLeapToolN : dynLeapTool
    {
        public dynLeapToolN()
        {
            InPortData.Add(new PortData("Index", "The index of the tool to read (1..N).", typeof(int)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            // Grab and set the tool index
            if (args.Length > 1 && args[1].IsNumber)
                ToolIndex = ((int)((Value.Number)args[1]).Item) - 1;
            else
                ToolIndex = -1;

            return base.Evaluate(args);
        }
    }

    [NodeName("Leap Tool 1")]
    [NodeCategory(BuiltinNodeCategories.COMMUNICATION)]
    [NodeDescription("Reads tool #1 from a Leap Motion ViewModel.")]
    public class dynLeapTool1 : dynLeapTool
    {
        public dynLeapTool1()
        {
            ToolIndex = 0;

            RegisterAllPorts();
        }
    }

    [NodeName("Leap Position")]
    [NodeCategory(BuiltinNodeCategories.COMMUNICATION)]
    [NodeDescription("Reads the position of a Leap Hand, Finger, or Tool.")]
    public class dynLeapPosition : dynNodeWithOneOutput
    {
        public dynLeapPosition()
        {
            InPortData.Add(new PortData("Object", "The Hand, Finger, or Tool from the Leap ViewModel", typeof(object)));

            OutPortData.Add(new PortData("XYZ", "The XYZ position in mms", typeof(Point3D)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            double x = 0.0;
            double y = 0.0;
            double z = 0.0;

            object item = ((Value.Container)args[0]).Item;

            if (item is Leap.Hand)
            {
                Leap.Hand hand = (Leap.Hand)item;

                x = hand.PalmPosition.x;
                z = hand.PalmPosition.y;        // NOTE: Leap coords Z+ to viewer so flip Z & Y
                y = hand.PalmPosition.z;
            }
            else if (item is Leap.Pointable)
            {
                Leap.Pointable pointable = (Leap.Pointable)item;

                x = pointable.TipPosition.x;
                z = pointable.TipPosition.y;        // NOTE: Leap coords Z+ to viewer so flip Z & Y
                y = pointable.TipPosition.z;
            }

            return Value.NewContainer(new Point3D(x, y, z));
        }
    }

} // namespace Dynamo.Nodes