//Copyright © Autodesk, Inc. 2012. All rights reserved.
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using Microsoft.FSharp.Collections;

using Dynamo.Connectors;
using Dynamo.Utilities;
using Dynamo.FSchemeInterop;
using Value = Dynamo.FScheme.Value;
using Autodesk.Revit.DB;

namespace Dynamo.Elements
{
    static class LeapManager
    {
        private static Leap.Controller controller;
        public static Leap.Controller Controller
        {
            get
            {
                if (!LeapControllerEnabled)
                    EnableController();
                return controller;
            }
            set
            {
                controller = value;
            }
        }

        public static void EnableController()
        {
            leapEnable(true);
        }

        public static bool LeapControllerEnabled { get { return controller != null; } }

        private static object controllerLock = new object();

        private static void leapEnable(bool enable)
        {
            lock (controllerLock)
            {
                if (enable == LeapControllerEnabled)
                    return;

                if (enable)
                {
                    Controller = new Leap.Controller();
                }
                else
                {
                    Controller.Dispose();
                    Controller = null;
                }
            }
        }
    }


    [ElementName("Leap")]
    [ElementCategory(BuiltinElementCategories.COMMUNICATION)]
    [ElementDescription("Manages connection to a Leap Motion controller.")]
    [RequiresTransaction(false)]
    public class dynLeapController : dynNode
    {
        System.Windows.Controls.MenuItem menuItemLeapEnabled = null;

        public dynLeapController()
        {
            // This input will drive the acquire of a frame of data from the Leap.
            // Attaching a timer node to it will then generate a flow of data from
            // the Leap into the system.
            InPortData.Add(new PortData("Read", "Read a frame of data from the Leap", typeof(object)));

            // Output from this node is the data from a single frame
            OutPortData = new PortData("Leap", "The Leap controller", typeof(object));

            base.RegisterInputsAndOutputs();

            //this.dynElementDestroyed += new dynElementDestroyedHandler(OnDynLeapMotionDestroyed);
            //this.dynElementReadyToDestroy += new dynElementReadyToDestroyHandler(OnDynLeapMotionReadyToDestroy);

            // Create a menuitem to enable/disable the Leap device
            menuItemLeapEnabled = new System.Windows.Controls.MenuItem();
            menuItemLeapEnabled.Header = "Enable Leap";
            menuItemLeapEnabled.IsCheckable = true;
            menuItemLeapEnabled.IsChecked = false;
            menuItemLeapEnabled.Checked += new System.Windows.RoutedEventHandler(menuItemLeapEnabled_Checked);
            this.MainContextMenu.Items.Add(menuItemLeapEnabled);
        }

        void menuItemLeapEnabled_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            menuItemLeapEnabled.IsChecked = !menuItemLeapEnabled.IsChecked; // toggle state

            if (menuItemLeapEnabled.IsChecked)
                menuItemLeapEnabled.Header = "Disable Leap";
            else
                menuItemLeapEnabled.Header = "Enable Leap";

            if (menuItemLeapEnabled.IsChecked)
            {
                try
                {
                    LeapManager.EnableController();
                }
                catch
                {
                    this.Error("Could not enable Leap Controller");
                    menuItemLeapEnabled.IsChecked = false;
                }
            }
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            return Value.NewContainer(LeapManager.Controller);
        }
    }

    [ElementName("Leap Frame N")]
    [ElementCategory(BuiltinElementCategories.COMMUNICATION)]
    [ElementDescription("Frame from the Leap Motion controller.")]
    [RequiresTransaction(false)]
    public class dynLeapFrameN : dynNode
    {
        public dynLeapFrameN()
        {
            InPortData.Add(new PortData("Leap", "The Leap controller", typeof(object)));
            InPortData.Add(new PortData("Age", "The age of the frame to retrieve (0 == current, 1..N previous).", typeof(int)));

            OutPortData = new PortData("Frame", "The frame of data", typeof(object));

            base.RegisterInputsAndOutputs();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Leap.Controller controller = (Leap.Controller)((Value.Container)args[0]).Item;
            if (controller == null)
                throw new Exception("No Leap Controller attached.");

            int age = (int)((Value.Number)args[1]).Item;
            if (age < 0)
                throw new Exception("Leap Frame Age must be >= 0");

            Leap.Frame frame = controller.Frame(age);

            return Value.NewContainer(frame);
        }
    }

    [ElementName("Leap Frame")]
    [ElementCategory(BuiltinElementCategories.COMMUNICATION)]
    [ElementDescription("Current Frame from the Leap Motion controller.")]
    [RequiresTransaction(false)]
    public class dynLeapFrame : dynNode
    {
        public dynLeapFrame()
        {
            InPortData.Add(new PortData("Leap", "The Leap controller", typeof(object)));

            OutPortData = new PortData("Frame", "The frame of data", typeof(object));

            base.RegisterInputsAndOutputs();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Leap.Controller controller = (Leap.Controller)((Value.Container)args[0]).Item;
            if (controller == null)
                throw new Exception("No Leap Controller attached.");

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

    [ElementName("Leap Frame Scale Factor")]
    [ElementCategory(BuiltinElementCategories.COMMUNICATION)]
    [ElementDescription("The scale factor derived from the overall motion between the current frame and the specified frame.")]
    [RequiresTransaction(false)]
    public class dynLeapFrameScaleFactor : dynNode
    {
        public dynLeapFrameScaleFactor()
        {
            InPortData.Add(new PortData("Frame", "A Frame from a Leap controller", typeof(object)));
            InPortData.Add(new PortData("Age", "The age of the previous frame (1..N).", typeof(int)));

            OutPortData = new PortData("ScaleFactor", "The scale factor", typeof(double));

            base.RegisterInputsAndOutputs();
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

            Leap.Controller controller = LeapManager.Controller;
            if (controller == null)
                throw new Exception("No Leap Controller node.");

            Leap.Frame sinceFrame = controller.Frame(age);

            return Value.NewNumber(frame.ScaleFactor(sinceFrame));
        }
    }

    [ElementName("Leap Frame Translation")]
    [ElementCategory(BuiltinElementCategories.COMMUNICATION)]
    [ElementDescription("The change of position derived from the overall linear motion between the current frame and the specified frame.")]
    [RequiresTransaction(false)]
    public class dynLeapFrameTranslation : dynNode
    {
        public dynLeapFrameTranslation()
        {
            InPortData.Add(new PortData("Frame", "A Frame from a Leap controller", typeof(object)));
            InPortData.Add(new PortData("Age", "The age of the previous frame.", typeof(int)));

            OutPortData = new PortData("Translation", "The translation vector", typeof(XYZ));

            base.RegisterInputsAndOutputs();
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

            Leap.Controller controller = LeapManager.Controller;
            if (controller == null)
                throw new Exception("No Leap Controller node.");

            Leap.Vector v;
            Leap.Frame sinceFrame = controller.Frame(age);
            if (sinceFrame != null && sinceFrame.IsValid)
                v = frame.Translation(sinceFrame);
            else
                v = new Leap.Vector();

            return Value.NewContainer(new XYZ(v.x, v.y, v.z));
        }
    }

    [ElementName("Leap Frame Rotation")]
    [ElementCategory(BuiltinElementCategories.COMMUNICATION)]
    [ElementDescription("The angle of rotation around the XYZ axis' derived from the overall rotational motion between the current frame and the specified frame.")]
    [RequiresTransaction(false)]
    public class dynLeapFrameRotation : dynNode
    {
        public dynLeapFrameRotation()
        {
            InPortData.Add(new PortData("Frame", "A Frame from a Leap controller", typeof(object)));
            InPortData.Add(new PortData("Age", "The age of the previous frame.", typeof(int)));

            OutPortData = new PortData("Rotation", "The XYZ rotation in radians", typeof(XYZ));

            base.RegisterInputsAndOutputs();
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

            Leap.Controller controller = LeapManager.Controller;
            if (controller == null)
                throw new Exception("No Leap Controller node.");

            Leap.Frame sinceFrame = controller.Frame(age);

            float angleX = frame.RotationAngle(sinceFrame, new Leap.Vector((float)1.0, (float)0.0, (float)0.0));
            float angleY = frame.RotationAngle(sinceFrame, new Leap.Vector((float)0.0, (float)1.0, (float)0.0));
            float angleZ = frame.RotationAngle(sinceFrame, new Leap.Vector((float)0.0, (float)0.0, (float)1.0));

            return Value.NewContainer(new XYZ(angleX, angleY, angleZ));
        }
    }

    [ElementName("Leap Hand")]
    [ElementCategory(BuiltinElementCategories.COMMUNICATION)]
    [ElementDescription("Reads a hand from a Leap Motion frame.")]
    [RequiresTransaction(false)]
    public class dynLeapHand : dynNode
    {
        public dynLeapHand()
        {
            InPortData.Add(new PortData("Frame", "The frame of data from the Leap device", typeof(object)));
            InPortData.Add(new PortData("Index", "The index of the hand to read (1..N).", typeof(int)));

            OutPortData = new PortData("Hand", "The hand read from the frame", typeof(object));

            base.RegisterInputsAndOutputs();
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

    [ElementName("Leap Hand 1")]
    [ElementCategory(BuiltinElementCategories.COMMUNICATION)]
    [ElementDescription("Reads a hand #1 from a Leap Motion frame.")]
    [RequiresTransaction(false)]
    public class dynLeapHand1 : dynNode
    {
        public dynLeapHand1()
        {
            InPortData.Add(new PortData("Frame", "The frame of data from the Leap device", typeof(object)));

            OutPortData = new PortData("Hand", "The hand read from the frame", typeof(object));

            base.RegisterInputsAndOutputs();
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

    [ElementName("Leap Hand 2")]
    [ElementCategory(BuiltinElementCategories.COMMUNICATION)]
    [ElementDescription("Reads a hand #1 from a Leap Motion frame.")]
    [RequiresTransaction(false)]
    public class dynLeapHand2 : dynNode
    {
        public dynLeapHand2()
        {
            InPortData.Add(new PortData("Frame", "The frame of data from the Leap device", typeof(object)));

            OutPortData = new PortData("Hand", "The hand read from the frame", typeof(object));

            base.RegisterInputsAndOutputs();
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

    [ElementName("Leap Fingers")]
    [ElementCategory(BuiltinElementCategories.COMMUNICATION)]
    [ElementDescription("Reads the list of fingers from a Leap Motion hand.")]
    [RequiresTransaction(false)]
    public class dynLeapFingers : dynNode
    {
        public dynLeapFingers()
        {
            InPortData.Add(new PortData("Hand", "The hand containing the fingers.", typeof(object)));

            OutPortData = new PortData("Fingers", "The list of fingers.", typeof(object));

            base.RegisterInputsAndOutputs();
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

    public class dynLeapFinger : dynNode
    {
        public dynLeapFinger()
        {
            FingerIndex = 0;

            InPortData.Add(new PortData("Hand", "The hand containing the finger.", typeof(object)));

            OutPortData = new PortData("Finger", "The finger data.", typeof(object));
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

    [ElementName("Leap Finger N")]
    [ElementCategory(BuiltinElementCategories.COMMUNICATION)]
    [ElementDescription("Reads a finger with a specified index from a Leap Motion hand.")]
    [RequiresTransaction(false)]
    public class dynLeapFingerN : dynLeapFinger
    {
        public dynLeapFingerN()
        {
            InPortData.Add(new PortData("Index", "The index of the finger to read (1..N).", typeof(int)));

            base.RegisterInputsAndOutputs();
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

    [ElementName("Leap Finger 1")]
    [ElementCategory(BuiltinElementCategories.COMMUNICATION)]
    [ElementDescription("Reads finger #1 from a Leap Motion hand.")]
    [RequiresTransaction(false)]
    public class dynLeapFinger1 : dynLeapFinger
    {
        public dynLeapFinger1()
        {
            FingerIndex = 0;    // 1st finger

            base.RegisterInputsAndOutputs();
        }
    }

    [ElementName("Leap Finger 2")]
    [ElementCategory(BuiltinElementCategories.COMMUNICATION)]
    [ElementDescription("Reads finger #2 from a Leap Motion hand.")]
    [RequiresTransaction(false)]
    public class dynLeapFinger2 : dynLeapFinger
    {
        public dynLeapFinger2()
        {
            FingerIndex = 1;    // 2nd finger

            base.RegisterInputsAndOutputs();
        }
    }

    [ElementName("Leap Finger 3")]
    [ElementCategory(BuiltinElementCategories.COMMUNICATION)]
    [ElementDescription("Reads finger #3 from a Leap Motion hand.")]
    [RequiresTransaction(false)]
    public class dynLeapFinger3 : dynLeapFinger
    {
        public dynLeapFinger3()
        {
            FingerIndex = 2;    // 3rd finger

            base.RegisterInputsAndOutputs();
        }
    }

    [ElementName("Leap Finger 4")]
    [ElementCategory(BuiltinElementCategories.COMMUNICATION)]
    [ElementDescription("Reads finger #4 from a Leap Motion hand.")]
    [RequiresTransaction(false)]
    public class dynLeapFinger4 : dynLeapFinger
    {
        public dynLeapFinger4()
        {
            FingerIndex = 3;    // 4th finger

            base.RegisterInputsAndOutputs();
        }
    }

    [ElementName("Leap Finger 5")]
    [ElementCategory(BuiltinElementCategories.COMMUNICATION)]
    [ElementDescription("Reads finger #5 from a Leap Motion hand.")]
    [RequiresTransaction(false)]
    public class dynLeapFinger5 : dynLeapFinger
    {
        public dynLeapFinger5()
        {
            FingerIndex = 4;    // 5th finger

            base.RegisterInputsAndOutputs();
        }
    }

    public class dynLeapTool : dynNode
    {
        public dynLeapTool()
        {
            ToolIndex = 0;

            InPortData.Add(new PortData("Frame", "The frame of data from the Leap Motion controller", typeof(object)));

            OutPortData = new PortData("Tool", "The tool data.", typeof(object));
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

    [ElementName("Leap Tool N")]
    [ElementCategory(BuiltinElementCategories.COMMUNICATION)]
    [ElementDescription("Reads a tool with a specified index from a Leap Motion controller.")]
    [RequiresTransaction(false)]
    public class dynLeapToolN : dynLeapTool
    {
        public dynLeapToolN()
        {
            InPortData.Add(new PortData("Index", "The index of the tool to read (1..N).", typeof(int)));

            base.RegisterInputsAndOutputs();
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

    [ElementName("Leap Tool 1")]
    [ElementCategory(BuiltinElementCategories.COMMUNICATION)]
    [ElementDescription("Reads tool #1 from a Leap Motion controller.")]
    [RequiresTransaction(false)]
    public class dynLeapTool1 : dynLeapTool
    {
        public dynLeapTool1()
        {
            ToolIndex = 0;

            base.RegisterInputsAndOutputs();
        }
    }

    [ElementName("Leap Position")]
    [ElementCategory(BuiltinElementCategories.COMMUNICATION)]
    [ElementDescription("Reads the position of a Leap Hand, Finger, or Tool.")]
    [RequiresTransaction(false)]
    public class dynLeapPosition : dynNode
    {
        public dynLeapPosition()
        {
            InPortData.Add(new PortData("Object", "The Hand, Finger, or Tool from the Leap controller", typeof(object)));

            OutPortData = new PortData("XYZ", "The XYZ position in mms", typeof(XYZ));

            base.RegisterInputsAndOutputs();
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

            return Value.NewContainer(new XYZ(x, y, z));
        }
    }

} // namespace Dynamo.Elements
