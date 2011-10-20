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
        public dynDouble(string nickName)
            : base(nickName)
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
        public dynBool(string nickName)
            : base(nickName)
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
        public dynInt(string nickName)
            : base(nickName)
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
        public dynString(string nickName)
            : base(nickName)
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
        public dynCurve(string nickName)
            : base(nickName)
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
        public dynAction(string nickName)
            : base(nickName)
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

        public dynDoubleInput(string nickName)
            : base(nickName)
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
            //tb.LostFocus += new System.Windows.RoutedEventHandler(tb_LostFocus);

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

        public dynBoolSelector(string nickName)
            : base(nickName)
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

        public dynIntInput(string nickName)
            : base(nickName)
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

    [ElementName("Arduino")]
    [ElementDescription("An element which allows you to read from an Arduino microcontroller.")]
    [RequiresTransaction(false)]
    public class dynArduino : dynElement, IDynamic
    {
        SerialPort port;
        string lastData = "";

        public dynArduino(string nickName): base(nickName)
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

    [ElementName("Timer")]
    [ElementDescription("An element which allows you to specify an update frequency in milliseconds.")]
    [RequiresTransaction(false)]
    public class dynTimer : dynElement, IDynamic
    {
        Stopwatch sw;
        bool timing = false;

        public dynTimer(string nickName)
            : base(nickName)
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

    [ElementName("Move")]
    [ElementDescription("Create an element which moves other elements by a fixed distance in each step.")]
    [RequiresTransaction(true)]
    public class dynMove : dynAction, IDynamic
    {
        public dynMove(string nickName)
            : base(nickName)
        {
            InPortData.Add(new PortData(null, "XYZ", "Movement vector.", typeof(dynXYZ)));
            InPortData.Add(new PortData(null, "El", "The element to move.", typeof(dynElement)));

            base.RegisterInputsAndOutputs();
        }

        /// <summary>
        /// Called by dynLoop elements to create iterative behaviors
        /// </summary>
        public override void PerformAction()
        {
            dynElementSettings.SharedInstance.Doc.Document.Move(InPortData[1].Object as Element, InPortData[0].Object as XYZ);
        }

        public override void Draw()
        {
            OutPortData[0].Object = this;
        }

        public override void Destroy()
        {
            base.Destroy();
        }

        public override void Update()
        {
            OnDynElementReadyToBuild(EventArgs.Empty);
        }
    }

    //[ElementName("Grow")]
    //[ElementDescription("Create an element to grow a value over a fixed number of steps.")]
    //[RequiresTransaction(false)]
    //public class dynDoubleGrow : dynAction, IDynamic
    //{
    //    double growValue;

    //    public dynDoubleGrow(string nickName)
    //        : base(nickName)
    //    {
    //        InPortData.Add(new PortData(0.0, "D", "Value to grow.", typeof(dynDouble)));
    //        InPortData.Add(new PortData(.1, "D", "Step.", typeof(dynDouble)));
    //        OutPortData.Add(new PortData(null, "D", "Larger value.", typeof(dynDouble)));

    //        base.RegisterInputsAndOutputs();
    //    }

    //    /// <summary>
    //    /// Called by dynLoop elements to create iterative behaviors
    //    /// </summary>
    //    public override void PerformAction()
    //    {
    //        OutPortData[1].Object = (double)OutPortData[1].Object + (double)InPortData[1].Object;
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

    //[ElementName("Loop")]
    //[ElementDescription("Create an element to loop through an operation a fixed number of times.")]
    //[RequiresTransaction(false)]
    //public class dynLoop : dynElement, IDynamic
    //{
    //    public dynLoop(string nickName)
    //        : base(nickName)
    //    {
    //        InPortData.Add(new PortData(null, "int", "Iterations", typeof(dynInt)));
    //        OutPortData.Add(new PortData(null, "n", "The loop.", typeof(dynLoop)));

    //        base.RegisterInputsAndOutputs();
    //    }

    //    public override void Draw()
    //    {
    //        //if (CheckInputs())
    //        //{
    //        //    for (int i = 0; i < (int)InPortData[0].Object; i++)
    //        //    {
    //        //        //do whatever it is you came here to do
    //        //        (OutPortData[0].Object as dynAction).PerformAction();

    //        //        //update the graphics
    //        //        //Settings.Doc.Document.Regenerate();
    //        //        dynElementSettings.SharedInstance.Doc.RefreshActiveView();
    //        //    }
    //        //}
    //    }

    //    public override void Destroy()
    //    {
    //        base.Destroy();
    //    }

    //    public override void Update()
    //    {
    //        for (int i = 0; i < (int)InPortData[0].Object; i++)
    //        {
    //            OnDynElementReadyToBuild(EventArgs.Empty);
    //        }
    //    }
    //}

    //[ElementName("Cycle")]
    //[ElementDescription("Create an element for continuously looping through an operation.")]
    //[RequiresTransaction(false)]
    //public class dynCycle : dynElement, IDynamic
    //{
    //    bool isRunning = false;
    //    System.Windows.Controls.Button startCycleButt;
    //    double t = 0.0;

    //    public dynCycle(string nickName)
    //        : base(nickName)
    //    {
    //        InPortData.Add(new PortData(null, "Act", "Action to perform.", typeof(dynAction)));

    //        startCycleButt = new System.Windows.Controls.Button();
    //        this.inputGrid.Children.Add(startCycleButt);
    //        startCycleButt.Margin = new System.Windows.Thickness(0, 0, 0, 0);
    //        startCycleButt.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
    //        startCycleButt.VerticalAlignment = System.Windows.VerticalAlignment.Center;
    //        startCycleButt.Click += new System.Windows.RoutedEventHandler(startCycle_click);
    //        startCycleButt.Content = "Start";

    //        base.RegisterInputsAndOutputs();

    //    }

    //    public override void Draw()
    //    {
    //        if (CheckInputs())
    //        {
    //            if (isRunning)
    //            {
    //                (InPortData[0].Object as dynAction).PerformAction();

    //                dynElementSettings.SharedInstance.Doc.RefreshActiveView();
    //            }

    //        }
    //    }

    //    public override void Destroy()
    //    {
    //        base.Destroy();
    //    }

    //    public override void Update()
    //    {
    //        OnDynElementReadyToBuild(EventArgs.Empty);
    //    }

    //    void startCycle_click(object sender, System.Windows.RoutedEventArgs e)
    //    {
    //        if (!isRunning)
    //        {
    //            startCycleButt.Content = "Stop";
    //            isRunning = true;
    //            (InPortData[0].Object as dynAction).PerformAction();
    //        }
    //        else
    //        {
    //            isRunning = false;
    //            startCycleButt.Content = "Start";
    //        }
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

        public dynWatch(string nickName)
            : base(nickName)
        {
            

            //add a list box
            //label = new System.Windows.Controls.Label();
            System.Windows.Controls.TextBox tb = new System.Windows.Controls.TextBox();
            tb.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;

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
                DataTree tree = InPortData[0].Object as DataTree;

                if (tree != null)
                {
                    WatchValue = tree.Graph();

                    UpdateLayoutDelegate uld = new UpdateLayoutDelegate(CallUpdateLayout);
                    Dispatcher.Invoke(uld, System.Windows.Threading.DispatcherPriority.Background, new object[] { this });
                }
                else
                {
                    //find the object as a string
                    WatchValue = InPortData[0].Object.ToString();
                    UpdateLayoutDelegate uld = new UpdateLayoutDelegate(CallUpdateLayout);
                    Dispatcher.Invoke(uld, System.Windows.Threading.DispatcherPriority.Background, new object[] { this });
                }
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

    #region delegates
    public delegate void dynElementUpdatedHandler(object sender, EventArgs e);
    public delegate void dynElementDestroyedHandler(object sender, EventArgs e);
    public delegate void dynElementReadyToBuildHandler(object sender, EventArgs e);
    public delegate void dynElementReadyToDestroyHandler(object sender, EventArgs e);
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
