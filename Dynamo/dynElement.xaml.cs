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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Reflection;
using System.Windows.Controls.Primitives;
using Dynamo.Connectors;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Grid = System.Windows.Controls.Grid;
using System.Diagnostics;
using Dynamo.Controls;
using Dynamo.Utilities;

namespace Dynamo.Elements
{
    /// <summary>
    /// Interaction logic for dynControl.xaml
    /// </summary
    public enum ElementState { DEAD, ACTIVE, SELECTED, ERROR };
    

    public partial class dynElement : UserControl, IDynamic
    {
        #region private members

        List<dynPort> inPorts;
        List<dynPort> outPorts;
        List<dynPort> statePorts;
        List<PortData> inPortData;
        List<PortData> outPortData;
        List<PortData> statePortData;
        string nickName = "dE";
        ElementArray elements;
        Guid guid;
        bool isSelected = false;
        ElementState state = ElementState.DEAD;
        ElementState prevState = ElementState.DEAD;
        DataTree dataTree;
        bool elementsHaveBeenDeleted = false;
        #endregion

        public delegate void SetToolTipDelegate(string message);
        public delegate void MarkConnectionStateDelegate(bool bad);
        public delegate void SetStateDelegate(dynElement el, ElementState state);
        public delegate void UpdateLayoutDelegate(FrameworkElement el);

        #region public members
        public string ToolTipText
        {
            get { return outPortData[0].ToolTipString; }
        }
        public List<PortData> InPortData
        {
            get { return inPortData; }
        }
        public List<PortData> OutPortData
        {
            get { return outPortData; }
        }
        public List<PortData> StatePortData
        {
            get { return statePortData; }
        }
        public List<dynPort> InPorts
        {
            get { return inPorts; }
            set
            {
                inPorts = value;
            }
        }
        public List<dynPort> OutPorts
        {
            get { return outPorts; }
            set { outPorts = value; }
        }
        public List<dynPort> StatePorts
        {
            get { return statePorts; }
            set { statePorts = value; }
        }
        public string NickName
        {
            get { return nickName; }
        }
        public Guid GUID
        {
            get { return guid; }
            set { guid = value; }
        }
        public ElementArray Elements
        {
            get { return elements; }
        }
        public ElementState State
        {
            get { return state; }
            set
            {
                if (value == ElementState.ACTIVE)
                {
                    elementRectangle.Fill = dynElementSettings.SharedInstance.ActiveBrush;
                }
                else if (value == ElementState.DEAD)
                {
                    elementRectangle.Fill = dynElementSettings.SharedInstance.DeadBrush;
                }
                else if (value == ElementState.ERROR)
                {
                    elementRectangle.Fill = dynElementSettings.SharedInstance.ErrorBrush;
                }
                else if (value == ElementState.SELECTED)
                {
                    elementRectangle.Fill = dynElementSettings.SharedInstance.SelectedBrush;
                }

                state = value;
            }

        }
        public DataTree Tree
        {
            get { return dataTree; }
            set { dataTree = value; }
        }
        public bool ElementsHaveBeenDeleted
        {
            get { return elementsHaveBeenDeleted; }
            set { elementsHaveBeenDeleted = value; }
        }
        #endregion

        #region events
        public event dynElementUpdatedHandler dynElementUpdated;
        public event dynElementDestroyedHandler dynElementDestroyed;
        public event dynElementReadyToBuildHandler dynElementReadyToBuild;
        public event dynElementReadyToDestroyHandler dynElementReadyToDestroy;
        #endregion

        #region constructors
        /// <summary>
        /// dynElement constructor for use by workbench in creating dynElements
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="nickName"></param>
        public dynElement(string nickName)
        {
            InitializeComponent();

            //inputs = new List<dynElement>();
            //outputs = new List<dynElement>();
            inPorts = new List<dynPort>();
            inPortData = new List<PortData>();
            outPortData = new List<PortData>();
            outPorts = new List<dynPort>();
            statePorts = new List<dynPort>();
            statePortData = new List<PortData>();
            elements = new ElementArray();

            dataTree = new DataTree();

            //this.settings = settings;
            this.nickName = nickName;
            this.nickNameBlock.Text = nickName;

            //this.workBench = settings.WorkBench;

            SetStateDelegate ssd = new SetStateDelegate(SetState);
            Dispatcher.Invoke(ssd, System.Windows.Threading.DispatcherPriority.Background, new object[] {this, ElementState.DEAD });
            //this.State = ElementState.DEAD;

            //set the z index to 2
            Canvas.SetZIndex(this, 1);

            //generate a guid for the component

            dynElementReadyToBuild += new dynElementReadyToBuildHandler(Build);
        }

        /// <summary>
        /// dynElement constructor for use when reconstructing dynElements from file
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="nickName"></param>
        /// <param name="guid"></param>
        public dynElement(dynElementSettings settings, string nickName, Guid guid)
        {
            InitializeComponent();

            inPorts = new List<dynPort>();
            inPortData = new List<PortData>();
            outPortData = new List<PortData>();
            outPorts = new List<dynPort>();

            dataTree = new DataTree();

            //this.settings = settings;
            this.nickName = nickName;
            this.nickNameBlock.Text = nickName;
            //this.workBench = settings.WorkBench;

            SetStateDelegate ssd = new SetStateDelegate(SetState);
            Dispatcher.Invoke(ssd, System.Windows.Threading.DispatcherPriority.Background, new object[] {this, ElementState.DEAD });
            //this.State = ElementState.DEAD;

            //set the z index to 2
            Canvas.SetZIndex(this, 1);

            this.guid = guid;

            dynElementReadyToBuild += new dynElementReadyToBuildHandler(Build);
        }

        public dynElement(dynElementSettings settings)
        {
            //this.settings = settings;
            this.guid = Guid.NewGuid();

            //empty constructor to create non-GUI types
            dynElementReadyToBuild += new dynElementReadyToBuildHandler(Build);
        }

        #endregion

        void SetState(dynElement el, ElementState state)
        {
            el.State = state;
        }

        protected virtual void OnDynElementUpdated(EventArgs e)
        {
            if (dynElementUpdated != null)
            {
                dynElementUpdated(this, e);
            }
        }
        
        protected virtual void OnDynElementDestroyed(EventArgs e)
        {
            if (dynElementDestroyed != null)
            {
                dynElementDestroyed(this, e);
            }
        }

        protected virtual void OnDynElementReadyToBuild(EventArgs e)
        {
            if (dynElementReadyToBuild != null)
            {
                dynElementReadyToBuild(this, e);
            }
        }

        protected virtual void OnDynElementReadyToDestroy(EventArgs e)
        {
            if (dynElementReadyToDestroy != null)
            {
                dynElementReadyToDestroy(this, e);
            }
        }
        
        public void RegisterInputsAndOutputs()
        {
            ResizeElementForInputs();
            SetupPortGrids();
            RegisterInputs();
            RegisterStatePorts();
            RegisterOutputs();
            SetToolTips();
            ValidateConnections();
        }

        /// <summary>
        /// Resize the control based on the number of inputs.
        /// </summary>
        public void ResizeElementForInputs()
        {
            //HACK
            //We don't want to remove these grids for the Instance Parameter mapper
            //because it will need them later
            if (statePortData.Count == 0 && this.GetType() != typeof(dynInstanceParameterMapper))
            {

                //size the height of the controller based on the 
                //whichever is larger the inport or the outport list
                this.topControl.Height = Math.Max(inPortData.Count, outPortData.Count) * 20 + 10; //spacing for inputs + title space + bottom space
                grid.Children.Remove(gridBottom);
                grid.Children.Remove(portNamesBottom);
                
                Thickness leftGridThick = new Thickness(gridLeft.Margin.Left, gridLeft.Margin.Top, gridLeft.Margin.Right, 5);
                gridLeft.Margin = leftGridThick;
                Thickness leftNamesThick = new Thickness(portNamesLeft.Margin.Left, portNamesLeft.Margin.Top, portNamesLeft.Margin.Right, 5);
                portNamesLeft.Margin = leftNamesThick;

                Thickness rightGridThick = new Thickness(gridRight.Margin.Left, gridRight.Margin.Top, gridRight.Margin.Right, 5);
                gridRight.Margin = rightGridThick;
                Thickness rightNamesThick = new Thickness(portNamesRight.Margin.Left, portNamesRight.Margin.Top, portNamesRight.Margin.Right, 5);
                portNamesRight.Margin = rightNamesThick;

                Thickness inputGridThick = new Thickness(inputGrid.Margin.Left, inputGrid.Margin.Top, inputGrid.Margin.Right, 5);
                inputGrid.Margin = inputGridThick;

                grid.UpdateLayout();

            }
            else
            {
                this.topControl.Height = Math.Max(inPortData.Count, outPortData.Count) * 20 +40+ 10; //spacing for inputs + title space + bottom space  
            }

            this.elementShine.Height = this.topControl.Height / 2;

            if (inputGrid.Children.Count == 0)
            {
                //decrease the width of the node because
                //there's nothing in the middle grid
                this.topControl.Width = 100;
            }
            else
                this.topControl.Width = Math.Max(200,StatePortData.Count * 20) + 10;
            
        }

        /// <summary>
        /// Sets up the control's grids based on numbers of input and output ports.
        /// </summary>
        public void SetupPortGrids()
        {
            //add one column for inputs
            ColumnDefinition cdIn = new ColumnDefinition();
            cdIn.Width = GridLength.Auto;
            gridLeft.ColumnDefinitions.Add(cdIn);

            ColumnDefinition namesIn = new ColumnDefinition();
            namesIn.Width = GridLength.Auto;
            portNamesLeft.ColumnDefinitions.Add(namesIn);

            //add one column for outputs
            ColumnDefinition cdOut = new ColumnDefinition();
            cdOut.Width = GridLength.Auto;
            gridLeft.ColumnDefinitions.Add(cdOut);

            ColumnDefinition namesOut = new ColumnDefinition();
            namesOut.Width = GridLength.Auto;
            portNamesRight.ColumnDefinitions.Add(namesOut);

            //add one row for state ports
            RowDefinition rdState = new RowDefinition();
            rdState.Height = GridLength.Auto;
            gridBottom.RowDefinitions.Add(rdState);

            RowDefinition namesState = new RowDefinition();
            namesState.Height = GridLength.Auto;
            portNamesBottom.RowDefinitions.Add(namesState);

            foreach (object input in InPortData)
            {
                RowDefinition rd = new RowDefinition();
                gridLeft.RowDefinitions.Add(rd);

                RowDefinition nameRd = new RowDefinition();
                portNamesLeft.RowDefinitions.Add(nameRd);
            }

            foreach (object output in OutPortData)
            {
                RowDefinition rd = new RowDefinition();
                gridRight.RowDefinitions.Add(rd);

                RowDefinition nameRd = new RowDefinition();
                portNamesRight.RowDefinitions.Add(nameRd);
            }

            foreach(object state in StatePortData)
            {
                ColumnDefinition cd = new ColumnDefinition();
                gridBottom.ColumnDefinitions.Add(cd);

                ColumnDefinition nameCd = new ColumnDefinition();
                portNamesBottom.ColumnDefinitions.Add(nameCd);
            }


        }
        
        /// <summary>
        /// Reads inputs list and adds ports for each input.
        /// </summary>
        public void RegisterInputs()
        {
            //read the inputs list and create a number of
            //input ports
            int count = 0;
            foreach (PortData pd in InPortData)
            {
                //add a port for each input
                //distribute the ports along the 
                //edges of the icon
                AddPort(pd.Object, PortType.INPUT, inPortData[count].NickName, count);
                count++;
            }
        }

        public void RegisterStatePorts()
        {
            //read the inputs list and create a number of
            //input ports
            int count = 0;
            foreach (PortData pd in StatePortData)
            {
                //add a port for each input
                //distribute the ports along the 
                //edges of the icon
                AddPort(pd.Object, PortType.STATE, statePortData[count].NickName, count);
                count++;
            }
        }

        /// <summary>
        /// Reads outputs list and adds ports for each output
        /// </summary>
        public void RegisterOutputs()
        {
            //read the outputs list and create a number of 
            //output ports
            int count = 0;
            foreach (PortData pd in OutPortData)
            {
                //add a port for each output
                //pass in the y center of the port
                AddPort(pd.Object, PortType.OUTPUT,outPortData[count].NickName, count);
                count++;

            }
        }

        public void SetToolTips()
        {
            //set all the tooltips
            int count = 0;
            foreach (dynPort p in inPorts)
            {
                //get the types from the types list
                p.toolTipText.Text = p.Owner.InPortData[count].ToolTipString;
                count++;
            }

            count = 0;
            foreach (dynPort p in outPorts)
            {
                p.toolTipText.Text = p.Owner.OutPortData[count].ToolTipString;
                count++;
            }

            count = 0;
            foreach (dynPort p in statePorts)
            {
                p.toolTipText.Text = p.Owner.StatePortData[count].ToolTipString;
                count++;
            }
        }

        /// <summary>
        /// Add a dynPort element to this control.
        /// </summary>
        /// <param name="isInput">Is the port an input?</param>
        /// <param name="index">The index of the port in the port list.</param>
        public void AddPort(object el, PortType portType, string name, int index)
        {
            dynPort p = new dynPort(index);

            //create a text block for the name of the port
            TextBlock tb = new TextBlock();
            
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.FontSize = 12;
            tb.FontWeight = FontWeights.Normal;
            tb.Foreground = new SolidColorBrush(Colors.Black);
            tb.Text = name;

            //set the z order to the back
            //Canvas.SetZIndex(this, 1);

            if (portType == PortType.INPUT)
            {
                tb.HorizontalAlignment = HorizontalAlignment.Left;

                p.PortType = PortType.INPUT;
                inPorts.Add(p);
                gridLeft.Children.Add(p);
                Grid.SetColumn(p, 0);
                Grid.SetRow(p, index);

                portNamesLeft.Children.Add(tb);
                Grid.SetColumn(tb, 0);
                Grid.SetRow(tb, index);
                
            }
            else if(portType == PortType.OUTPUT)
            {

                tb.HorizontalAlignment = HorizontalAlignment.Right;

                p.PortType = PortType.OUTPUT;
                outPorts.Add(p);
                gridRight.Children.Add(p);
                Grid.SetColumn(p, 0);
                Grid.SetRow(p, index);

                portNamesRight.Children.Add(tb);
                Grid.SetColumn(tb, 0);
                Grid.SetRow(tb, index);
                
            }
            else if (portType == PortType.STATE)
            {
                tb.HorizontalAlignment = HorizontalAlignment.Center;

                p.PortType = PortType.STATE;
                statePorts.Add(p);
                gridBottom.Children.Add(p);
                Grid.SetColumn(p, index);
                Grid.SetRow(p, 0);

                portNamesBottom.Children.Add(tb);
                Grid.SetColumn(tb, index);
                Grid.SetRow(tb, 0);
            }

            p.Owner = this;
            
            //register listeners on the port
            p.PortConnected += new PortConnectedHandler(p_PortConnected);
            p.PortDisconnected += new PortConnectedHandler(p_PortDisconnected);
        }

        /// <summary>
        /// When a port is connected, register a listener for the dynElementUpdated event
        /// and tell the object to build
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void p_PortConnected(object sender, EventArgs e)
        {
            ValidateConnections();
        }
        
        void p_PortDisconnected(object sender, EventArgs e)
        {
            Destroy();
            ValidateConnections();
        }

        /// <summary>
        /// Color the connection according to it's port connectivity
        /// if all ports are connected, color green, else color orange
        /// </summary>
        void ValidateConnections()
        {
            bool flag = false;

            foreach (dynPort p in inPorts)
            {
                if (p.Connectors.Count == 0)
                {
                    flag = true;
                }
            }

           
            SetStateDelegate ssd = new SetStateDelegate(SetState);
            
            if (flag)
            {
                Dispatcher.Invoke(ssd, System.Windows.Threading.DispatcherPriority.Background, new object[] {this, ElementState.DEAD });
            }
            else
            {
                Dispatcher.Invoke(ssd, System.Windows.Threading.DispatcherPriority.Background, new object[] {this, ElementState.ACTIVE });
            }
            

        }

        void MarkConnectionState(bool bad)
        {
            SetStateDelegate ssd = new SetStateDelegate(SetState);
            if (bad)
            {
                Dispatcher.Invoke(ssd, System.Windows.Threading.DispatcherPriority.Background, new object[] {this, ElementState.ERROR });
                //this.State = ElementState.ERROR;
            }
            else
            {
                Dispatcher.Invoke(ssd, System.Windows.Threading.DispatcherPriority.Background, new object[] {this, ElementState.ACTIVE });
                //this.State = ElementState.ACTIVE;
            }
        }

        /// <summary>
        /// The build method is called back from the child class.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Build(object sender, EventArgs e)
        {
            dynElement el = sender as dynElement;

            bool useTransaction = true;

            object[] attribs = el.GetType().GetCustomAttributes(typeof(RequiresTransactionAttribute), false);
            if (attribs.Length > 0)
            {
                if ((attribs[0] as RequiresTransactionAttribute).RequiresTransaction == false)
                {
                    useTransaction = false;
                }
            }

            #region using transaction
            if (useTransaction)
            {
                Transaction t = new Transaction(dynElementSettings.SharedInstance.Doc.Document, el.GetType().ToString() + " update.");
                TransactionStatus ts = t.Start();

                try
                {
                    FailureHandlingOptions failOpt = t.GetFailureHandlingOptions();
                    failOpt.SetFailuresPreprocessor(dynElementSettings.SharedInstance.WarningSwallower);
                    t.SetFailureHandlingOptions(failOpt);

                    /*if (!elementsHaveBeenDeleted)
                    {
                        //find the end node and delete in reverse
                        //this strips away all the downstream geometry in reverse build order
                        List<dynElement> downStream = new List<dynElement>();
                        el.FindDownstreamElements(ref downStream);
                        downStream.Reverse();
                        foreach (dynElement delEl in downStream)
                            delEl.Destroy();
                    }*/
                    el.Destroy();
                    el.Draw();

                    UpdateLayoutDelegate uld = new UpdateLayoutDelegate(CallUpdateLayout);
                    Dispatcher.Invoke(uld, System.Windows.Threading.DispatcherPriority.Background, new object[] { el }); 
                    //el.UpdateLayout();

                    elementsHaveBeenDeleted = false;

                    ts = t.Commit();

                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message + " : " + ex.StackTrace);
                    dynElementSettings.SharedInstance.Bench.Log(ex.Message);

                    SetToolTipDelegate sttd = new SetToolTipDelegate(SetTooltip);
                    Dispatcher.Invoke(sttd, System.Windows.Threading.DispatcherPriority.Background,
                        new object[] { ex.Message});

                    //this.ToolTip = ex.Message;

                    MarkConnectionStateDelegate mcsd = new MarkConnectionStateDelegate(MarkConnectionState);
                    Dispatcher.Invoke(mcsd, System.Windows.Threading.DispatcherPriority.Background,
                        new object[] { true });
                    //MarkConnectionState(true);

                    if (ts == TransactionStatus.Committed)
                    {
                        t.RollBack();
                    }

                    t.Dispose();

                    dynElementSettings.SharedInstance.Writer.WriteLine(ex.Message);
                    dynElementSettings.SharedInstance.Writer.WriteLine(ex.StackTrace);
                }

                try
                {
                    el.UpdateOutputs();
                }
                catch(Exception ex)
                {
           
                    //Debug.WriteLine("Outputs could not be updated.");
                    dynElementSettings.SharedInstance.Bench.Log("Outputs could not be updated.");
                    if (ts == TransactionStatus.Committed)
                    {
                        t.RollBack();
                    }

                    t.Dispose();

                    dynElementSettings.SharedInstance.Writer.WriteLine(ex.Message);
                    dynElementSettings.SharedInstance.Writer.WriteLine(ex.StackTrace);
                }
            }
            #endregion

            #region no transaction
            if (!useTransaction)
            {
                try
                {
                    /*
                    if (!elementsHaveBeenDeleted)
                    {
                        //find the end node and delete in reverse
                        //this strips away all the downstream geometry in reverse build order
                        List<dynElement> downStream = new List<dynElement>();
                        el.FindDownstreamElements(ref downStream);
                        downStream.Reverse();
                        foreach (dynElement delEl in downStream)
                            delEl.Destroy();
                    }*/
                    el.Destroy();

                    el.Draw();

                    UpdateLayoutDelegate uld = new UpdateLayoutDelegate(CallUpdateLayout);
                    Dispatcher.Invoke(uld, System.Windows.Threading.DispatcherPriority.Background, new object[] { el }); 
                    //el.UpdateLayout();
                    elementsHaveBeenDeleted = false;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message + " : " + ex.StackTrace);
                    dynElementSettings.SharedInstance.Bench.Log(ex.Message);

                    SetToolTipDelegate sttd = new SetToolTipDelegate(SetTooltip);
                    Dispatcher.Invoke(sttd, System.Windows.Threading.DispatcherPriority.Background,
                        new object[] { ex.Message });
                    //this.ToolTip = ex.Message;

                    MarkConnectionState(true);

                    dynElementSettings.SharedInstance.Writer.WriteLine(ex.Message);
                    dynElementSettings.SharedInstance.Writer.WriteLine(ex.StackTrace);

                }

                try
                {
                    el.UpdateOutputs();
                }
                catch(Exception ex)
                {

                    //Debug.WriteLine("Outputs could not be updated.");
                    dynElementSettings.SharedInstance.Bench.Log("Outputs could not be updated.");

                    dynElementSettings.SharedInstance.Writer.WriteLine(ex.Message);
                    dynElementSettings.SharedInstance.Writer.WriteLine(ex.StackTrace);
                }
            }
            #endregion

            //settings.Doc.Document.Regenerate();

        }

        public void CallUpdateLayout(FrameworkElement el)
        {
            el.UpdateLayout();
        }

        public void SetTooltip(string message)
        {
            this.ToolTip = message;
        }

        public virtual void Draw()
        {
        }

        /// <summary>
        /// Destroy all elements belonging to this dynElement
        /// </summary>
        public virtual void Destroy()
        {
            foreach (Element e in elements)
            {
                dynElementSettings.SharedInstance.Doc.Document.Delete(e);
            }

            //clear out the array to avoid object initialization errors
            elements.Clear();

            //clear the data tree
            dataTree.Clear();
        }

        public virtual void Update()
        {
        }

        public void UpdateOutputs()
        {
            foreach (dynPort p in outPorts)
            {
                foreach (dynConnector c in p.Connectors)
                {
                    c.SendMessage();
                }
            }
        }

        public void FindDownstreamElements(ref List<dynElement>downStream)
        {
            foreach (dynPort p in outPorts)
            {
                foreach (dynConnector c in p.Connectors)
                {
                    if (!downStream.Contains(c.End.Owner))
                    {
                        //don't add it if it's already there
                        downStream.Add(c.End.Owner);
                    }

                    //set a flag on the element to say 
                    //that it has already processed its downstream geometry
                    c.End.Owner.ElementsHaveBeenDeleted = true;
                    c.End.Owner.FindDownstreamElements(ref downStream);
                }
            }
        }

        public void ClearOutputs()
        {
            foreach (dynPort p in outPorts)
            {
                foreach (dynConnector c in p.Connectors)
                {
                    c.Kill();
                }
            }

            outPorts.Clear();
            outPortData.Clear();

        }

        public bool CheckInputs()
        {
            MarkConnectionStateDelegate mcsd = new MarkConnectionStateDelegate(MarkConnectionState);

            foreach(PortData pd in InPortData)
            {
                //if the port data's object is null
                //or the port data's object type can not be matched
                if (pd.Object == null ) //|| pd.Object.GetType() != pd.PortType)
                {
                    Dispatcher.Invoke(mcsd, System.Windows.Threading.DispatcherPriority.Background,
                        new object[] { true });
                    //MarkConnectionState(true);


                    SetToolTipDelegate sttd = new SetToolTipDelegate(SetTooltip);
                    Dispatcher.Invoke(sttd, System.Windows.Threading.DispatcherPriority.Background,
                        new object[] { "One or more connections is null." });
                    //this.ToolTip = "One or more connections is null.";

                    return false;
                }
            }

            
            Dispatcher.Invoke(mcsd, System.Windows.Threading.DispatcherPriority.Background,
                new object[] { false });
            //MarkConnectionState(false);

            return true;
        }

        public void ToggleSelectionState()
        {
            //set the selected mode
            if (isSelected)
            {
                isSelected = false;
            }
            else
            {
                isSelected = true;

                SetStateDelegate ssd = new SetStateDelegate(SetState);
                Dispatcher.Invoke(ssd, System.Windows.Threading.DispatcherPriority.Background, new object[] {this, ElementState.SELECTED });
                //state = ElementState.SELECTED;
            }
        }
        
        public void SetSelectionState(bool selected)
        {
            isSelected = selected;
        }

        private void topControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void elementRectangle_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (this.state != ElementState.SELECTED)
            {
                prevState = this.state;

                SetStateDelegate ssd = new SetStateDelegate(SetState);
                Dispatcher.Invoke(ssd, System.Windows.Threading.DispatcherPriority.Background, new object[] {this, ElementState.SELECTED });
                //this.State = ElementState.SELECTED;

                //set all other items to their previous state.
                foreach (dynElement el in dynElementSettings.SharedInstance.Bench.Elements)
                {
                    if (el.Equals(this)) continue;

                    Dispatcher.Invoke(ssd, System.Windows.Threading.DispatcherPriority.Background, new object[] {el, el.prevState});
                    //el.State = el.prevState ;
                }
            }
            else
            {
                this.State = prevState;
            }
        }

    }
}
