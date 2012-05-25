//Copyright 2012 Ian Keough

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
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Autodesk.Revit.DB;
using Dynamo.Connectors;
using Dynamo.FSchemeInterop;
using Dynamo.FSchemeInterop.Node;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using Expression = Dynamo.FScheme.Expression;
using Grid = System.Windows.Controls.Grid;
using Dynamo.Controls;
using System.Windows.Data;


namespace Dynamo.Elements
{
   /// <summary>
   /// Interaction logic for dynControl.xaml
   /// </summary
   public enum ElementState { DEAD, ACTIVE, SELECTED, ERROR };
   public enum LacingType { SHORTEST, LONGEST, FULL };

   public partial class dynElement : UserControl, IDynamic, INotifyPropertyChanged
   {
      #region delegates
      public delegate void dynElementUpdatedHandler(object sender, EventArgs e);
      public delegate void dynElementDestroyedHandler(object sender, EventArgs e);
      public delegate void dynElementReadyToBuildHandler(object sender, EventArgs e);
      public delegate void dynElementReadyToDestroyHandler(object sender, EventArgs e);
      public delegate void dynElementSelectedHandler(object sender, EventArgs e);
      public delegate void dynElementDeselectedHandler(object sender, EventArgs e);
      #endregion

      public event PropertyChangedEventHandler PropertyChanged;

      protected void NotifyPropertyChanged(String info)
      {
         if (PropertyChanged != null)
         {
            PropertyChanged(this, new PropertyChangedEventArgs(info));
         }
      }

      #region private members
      List<dynPort> inPorts;
      dynPort outPort;
      List<dynPort> statePorts;
      List<PortData> inPortData;
      PortData outPortData;
      List<PortData> statePortData;
      Dictionary<dynPort, TextBlock> inPortTextBlocks;
      string nickName;
      Guid guid;
      //bool isSelected = false;
      ElementState state;
      DataTree dataTree;
      bool elementsHaveBeenDeleted = false;
      SetStateDelegate stateSetter;
      LacingType lacingType = LacingType.SHORTEST;
      #endregion

      public delegate void SetToolTipDelegate(string message);
      public delegate void MarkConnectionStateDelegate(bool bad);
      public delegate void UpdateLayoutDelegate(FrameworkElement el);
      public delegate void SetStateDelegate(dynElement el, ElementState state);

      #region public members
      private dynWorkspace _workspace;
      public dynWorkspace WorkSpace
      {
         get
         {
            return _workspace;
         }
         internal set
         {
            _workspace = value;
         }
      }

      public dynElement TopControl
      {
         get { return this.topControl; }
      }

      public string ToolTipText
      {
         get { return outPortData.ToolTipString; }
      }

      public List<PortData> InPortData
      {
         get { return inPortData; }
      }

      public PortData OutPortData
      {
         get { return outPortData; }
         set { outPortData = value; }
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

      public dynPort OutPort
      {
         get { return outPort; }
         set { outPort = value; }
      }

      public List<dynPort> StatePorts
      {
         get { return statePorts; }
         set { statePorts = value; }
      }

      public string NickName
      {
         get { return nickName; }
         set
         {
            nickName = value;
            NotifyPropertyChanged("NickName");
         }
      }

      public Guid GUID
      {
         get { return guid; }
         set { guid = value; }
      }

      private List<List<Element>> elements;
      public List<Element> Elements
      {
         get
         {
            while (this.elements.Count <= this.runCount)
               this.elements.Add(new List<Element>());
            return this.elements[this.runCount];
         }
         private set
         {
            this.elements[this.runCount] = value;
         }
      }

      public ElementState State
      {
         get { return state; }
         set
         {
            switch (value)
            {
               case ElementState.ACTIVE:
                  elementRectangle.Fill = dynElementSettings.SharedInstance.ActiveBrush;
                  break;
               case ElementState.DEAD:
                  elementRectangle.Fill = dynElementSettings.SharedInstance.DeadBrush;
                  break;
               case ElementState.ERROR:
                  elementRectangle.Fill = dynElementSettings.SharedInstance.ErrorBrush;
                  break;
               case ElementState.SELECTED:
               default:
                  elementRectangle.Fill = dynElementSettings.SharedInstance.SelectedBrush;
                  break;
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

      public LacingType LacingType
      {
         get { return lacingType; }
         set { lacingType = value; }
      }

      public Grid ContentGrid
      {
         get { return this.inputGrid; }
      }

      protected Autodesk.Revit.UI.UIDocument UIDocument
      {
         get { return dynElementSettings.SharedInstance.Doc; }
      }

      protected dynBench Bench
      {
         get { return dynElementSettings.SharedInstance.Bench; }
      }
      #endregion

      #region events
      //public event dynElementUpdatedHandler dynElementUpdated;
      public event dynElementDestroyedHandler dynElementDestroyed;
      public event dynElementReadyToBuildHandler dynElementReadyToBuild;
      public event dynElementReadyToDestroyHandler dynElementReadyToDestroy;
      public event dynElementSelectedHandler dynElementSelected;
      public event dynElementDeselectedHandler dynElementDeselected;
      #endregion

      #region constructors
      /// <summary>
      /// dynElement constructor for use by workbench in creating dynElements
      /// </summary>
      /// <param name="settings"></param>
      /// <param name="nickName"></param>
      public dynElement()
      {
         InitializeComponent();
         //System.Uri resourceLocater = new System.Uri("/DynamoElements;component/dynElement.xaml", UriKind.Relative);
         //System.Uri resourceLocater = new System.Uri("/dynElement.xaml", UriKind.Relative);
         //System.Windows.Application.LoadComponent(this, resourceLocater);

         //set the main grid's data context to 
         //this element
         nickNameBlock.DataContext = this;

         inPorts = new List<dynPort>();
         inPortData = new List<PortData>();
         statePorts = new List<dynPort>();
         statePortData = new List<PortData>();
         elements = new List<List<Element>>() { new List<Element>() };
         Elements = new List<Element>();
         dataTree = new DataTree();
         inPortTextBlocks = new Dictionary<dynPort, TextBlock>();

         stateSetter = new SetStateDelegate(SetState);
         Dispatcher.BeginInvoke(
            stateSetter,
            System.Windows.Threading.DispatcherPriority.Background,
            new object[] { this, ElementState.DEAD }
         );

         //Fetch the element name from the custom attribute.
         var nameArray = this.GetType().GetCustomAttributes(typeof(ElementNameAttribute), true);

         if (nameArray.Length > 0)
         {
            ElementNameAttribute elNameAttrib = nameArray[0] as ElementNameAttribute;
            if (elNameAttrib != null)
            {
               NickName = elNameAttrib.ElementName;
            }
         }
         else
            NickName = "";

         //set the z index to 2
         Canvas.SetZIndex(this, 1);

         //dynElementReadyToBuild += new dynElementReadyToBuildHandler(Build);
      }
      #endregion

      void WorkspaceModified()
      {
         this.IsDirty = true;
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

      protected virtual void OnDynElementSelected(EventArgs e)
      {
         if (dynElementSelected != null)
         {
            dynElementSelected(this, e);
         }
      }

      protected virtual void OnDynElementDeselected(EventArgs e)
      {
         if (dynElementDeselected != null)
         {
            dynElementDeselected(this, e);
         }
      }

      protected virtual string Save()
      {
         //override this in your derived classes to specify the save behavior
         return "";
      }

      public void RegisterInputsAndOutputs()
      {
         ResizeElementForInputs();
         SetupPortGrids();
         RegisterInputs();
         //RegisterStatePorts();
         RegisterOutputs();
         SetToolTips();
         ValidateConnections();
      }

      public void ReregisterInputs()
      {
         ResizeElementForInputs();
         SetupPortGrids();
         RegisterInputs();
         SetToolTips();
         ValidateConnections();
      }

      private Dictionary<UIElement, bool> enabledDict = new Dictionary<UIElement, bool>();

      public void DisableInteraction()
      {
         enabledDict.Clear();

         foreach (UIElement e in this.inputGrid.Children)
         {
            enabledDict[e] = e.IsEnabled;

            e.IsEnabled = false;
         }
      }

      public void EnableInteraction()
      {
         foreach (UIElement e in this.inputGrid.Children)
         {
            if (enabledDict.ContainsKey(e))
               e.IsEnabled = enabledDict[e];
         }
      }

      /// <summary>
      /// Resize the control based on the number of inputs.
      /// </summary>
      public void ResizeElementForInputs()
      {
         //HACK
         //We don't want to remove these grids for the Instance Parameter mapper
         //because it will need them later
         //if (this.GetType() != typeof(dynInstanceParameterMapper))
         //{
         //size the height of the controller based on the 
         //whichever is larger the inport or the outport list
         this.topControl.Height = Math.Max(inPortData.Count, 1) * 20 + 10; //spacing for inputs + title space + bottom space
         //grid.Children.Remove(gridBottom);
         //grid.Children.Remove(portNamesBottom);

         Thickness leftGridThick = new Thickness(gridLeft.Margin.Left, gridLeft.Margin.Top, gridLeft.Margin.Right, 5);
         gridLeft.Margin = leftGridThick;
         //Thickness leftNamesThick = new Thickness(portNamesLeft.Margin.Left, portNamesLeft.Margin.Top, portNamesLeft.Margin.Right, 5);
         //portNamesLeft.Margin = leftNamesThick;

         //Thickness rightGridThick = new Thickness(gridRight.Margin.Left, gridRight.Margin.Top, gridRight.Margin.Right, 5);
         //gridRight.Margin = rightGridThick;
         //Thickness rightNamesThick = new Thickness(portNamesRight.Margin.Left, portNamesRight.Margin.Top, portNamesRight.Margin.Right, 5);
         //portNamesRight.Margin = rightNamesThick;

         Thickness inputGridThick = new Thickness(inputGrid.Margin.Left, inputGrid.Margin.Top, inputGrid.Margin.Right, 5);
         inputGrid.Margin = inputGridThick;

         grid.UpdateLayout();
         //}
         //else
         //{
         //   this.topControl.Height = Math.Max(inPortData.Count, outPortData.Count) * 20 + 10; //spacing for inputs + title space 
         //}

         this.elementShine.Height = this.topControl.Height / 2;

         if (inputGrid.Children.Count == 0)
         {
            //decrease the width of the node because
            //there's nothing in the middle grid
            this.topControl.Width = 100;
         }
         else
            this.topControl.Width = Math.Max(200, StatePortData.Count * 20) + 10;
      }

      /// <summary>
      /// Sets up the control's grids based on numbers of input and output ports.
      /// </summary>
      public void SetupPortGrids()
      {
         //add one column for inputs
         //ColumnDefinition cdIn = new ColumnDefinition();
         //cdIn.Width = GridLength.Auto;
         //gridLeft.ColumnDefinitions.Add(cdIn);

         //ColumnDefinition namesIn = new ColumnDefinition();
         //namesIn.Width = GridLength.Auto;
         //portNamesLeft.ColumnDefinitions.Add(namesIn);

         ////add one column for outputs
         //ColumnDefinition cdOut = new ColumnDefinition();
         //cdOut.Width = GridLength.Auto;
         //gridLeft.ColumnDefinitions.Add(cdOut);

         //ColumnDefinition namesOut = new ColumnDefinition();
         //namesOut.Width = GridLength.Auto;
         //portNamesRight.ColumnDefinitions.Add(namesOut);

         ////add one row for state ports
         //RowDefinition rdState = new RowDefinition();
         //rdState.Height = GridLength.Auto;
         //gridBottom.RowDefinitions.Add(rdState);

         //RowDefinition namesState = new RowDefinition();
         //namesState.Height = GridLength.Auto;
         //portNamesBottom.RowDefinitions.Add(namesState);

         int count = 0;
         int numRows = gridLeft.RowDefinitions.Count;
         foreach (object input in InPortData)
         {
            if (count++ < numRows)
               continue;

            RowDefinition rd = new RowDefinition();
            gridLeft.RowDefinitions.Add(rd);

            //RowDefinition nameRd = new RowDefinition();
            //portNamesLeft.RowDefinitions.Add(nameRd);
         }

         if (count < numRows)
         {
            gridLeft.RowDefinitions.RemoveRange(count, numRows - count);
         }

         count = 0;
         numRows = gridRight.RowDefinitions.Count;
         if (numRows == 0)
         {
            gridRight.RowDefinitions.Add(new RowDefinition());
         }

         //foreach (object output in OutPortData)
         //{
         //   if (count++ < numRows)
         //      continue;

         //   RowDefinition rd = new RowDefinition();
         //   gridRight.RowDefinitions.Add(rd);

         //   //RowDefinition nameRd = new RowDefinition();
         //   //portNamesRight.RowDefinitions.Add(nameRd);
         //}

         if (numRows > 1)
         {
            gridRight.RowDefinitions.RemoveRange(count, numRows - 1);
         }

         //foreach(object state in StatePortData)
         //{
         //    ColumnDefinition cd = new ColumnDefinition();
         //    gridBottom.ColumnDefinitions.Add(cd);

         //    ColumnDefinition nameCd = new ColumnDefinition();
         //    portNamesBottom.ColumnDefinitions.Add(nameCd);
         //}
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

         if (this.inPorts.Count > count)
         {
            foreach (var inport in this.inPorts.Skip(count))
            {
               RemovePort(inport);
            }

            this.inPorts.RemoveRange(count, this.inPorts.Count - count);
         }
      }

      private void RemovePort(dynPort inport)
      {
         if (inport.PortType == PortType.INPUT)
         {
            int index = this.inPorts.FindIndex(x => x == inport);

            gridLeft.Children.Remove(inport);
            gridLeft.Children.Remove(this.inPortTextBlocks[inport]);

            while (inport.Connectors.Any())
            {
               inport.Connectors[0].Kill();
            }
         }

         //else if (portType == PortType.OUTPUT)
         //{

         //}
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
         //this.outPorts.Clear();

         //read the outputs list and create a number of 
         //output ports
         //int count = 0;
         //foreach (PortData pd in OutPortData)
         //{
         //   //add a port for each output
         //   //pass in the y center of the port
         //   AddPort(pd.Object, PortType.OUTPUT, outPortData[count].NickName, count);
         //   count++;
         //}

         //AddPort(OutPortData.Object, PortType.OUTPUT, OutPortData.NickName, 0);

         dynPort p = new dynPort(0);

         //create a text block for the name of the port
         TextBlock tb = new TextBlock();

         tb.VerticalAlignment = VerticalAlignment.Center;
         tb.FontSize = 12;
         tb.FontWeight = FontWeights.Normal;
         tb.Foreground = new SolidColorBrush(Colors.Black);
         tb.Text = OutPortData.NickName;

         tb.HorizontalAlignment = HorizontalAlignment.Right;

         p.PortType = PortType.OUTPUT;
         outPort = p;
         gridRight.Children.Add(p);
         Grid.SetColumn(p, 1);
         Grid.SetRow(p, 0);

         //portNamesRight.Children.Add(tb);
         gridRight.Children.Add(tb);
         Grid.SetColumn(tb, 0);
         Grid.SetRow(tb, 0);

         p.Owner = this;

         //register listeners on the port
         p.PortConnected += new PortConnectedHandler(p_PortConnected);
         p.PortDisconnected += new PortConnectedHandler(p_PortDisconnected);
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

         outPort.toolTipText.Text = outPort.Owner.OutPortData.ToolTipString;

         //count = 0;
         //foreach (dynPort p in statePorts)
         //{
         //   p.toolTipText.Text = p.Owner.StatePortData[count].ToolTipString;
         //   count++;
         //}
      }

      /// <summary>
      /// Add a dynPort element to this control.
      /// </summary>
      /// <param name="isInput">Is the port an input?</param>
      /// <param name="index">The index of the port in the port list.</param>
      public void AddPort(object el, PortType portType, string name, int index)
      {
         //set the z order to the back
         //Canvas.SetZIndex(p, 1);

         if (portType == PortType.INPUT)
         {
            if (inPorts.Count > index)
            {
               this.inPortTextBlocks[this.inPorts[index]].Text = name;
            }
            else
            {
               dynPort p = new dynPort(index);

               //create a text block for the name of the port
               TextBlock tb = new TextBlock();

               tb.VerticalAlignment = VerticalAlignment.Center;
               tb.FontSize = 12;
               tb.FontWeight = FontWeights.Normal;
               tb.Foreground = new SolidColorBrush(Colors.Black);
               tb.Text = name;

               tb.HorizontalAlignment = HorizontalAlignment.Left;

               p.PortType = PortType.INPUT;
               inPorts.Add(p);
               inPortTextBlocks[p] = tb;
               gridLeft.Children.Add(p);
               Grid.SetColumn(p, 0);
               Grid.SetRow(p, index);

               //portNamesLeft.Children.Add(tb);
               gridLeft.Children.Add(tb);
               Grid.SetColumn(tb, 1);
               Grid.SetRow(tb, index);

               p.Owner = this;

               //register listeners on the port
               p.PortConnected += new PortConnectedHandler(p_PortConnected);
               p.PortDisconnected += new PortConnectedHandler(p_PortDisconnected);
            }
         }
         //else if (portType == PortType.OUTPUT)
         //{
         //   dynPort p = new dynPort(index);

         //   //create a text block for the name of the port
         //   TextBlock tb = new TextBlock();

         //   tb.VerticalAlignment = VerticalAlignment.Center;
         //   tb.FontSize = 12;
         //   tb.FontWeight = FontWeights.Normal;
         //   tb.Foreground = new SolidColorBrush(Colors.Black);
         //   tb.Text = name;

         //   tb.HorizontalAlignment = HorizontalAlignment.Right;

         //   p.PortType = PortType.OUTPUT;
         //   outPort = p;
         //   gridRight.Children.Add(p);
         //   Grid.SetColumn(p, 1);
         //   Grid.SetRow(p, index);

         //   //portNamesRight.Children.Add(tb);
         //   gridRight.Children.Add(tb);
         //   Grid.SetColumn(tb, 0);
         //   Grid.SetRow(tb, index);

         //   p.Owner = this;

         //   //register listeners on the port
         //   p.PortConnected += new PortConnectedHandler(p_PortConnected);
         //   p.PortDisconnected += new PortConnectedHandler(p_PortDisconnected);
         //}
         //else if (portType == PortType.STATE)
         //{
         //    tb.HorizontalAlignment = HorizontalAlignment.Center;

         //    p.PortType = PortType.STATE;
         //    statePorts.Add(p);
         //    gridBottom.Children.Add(p);
         //    Grid.SetColumn(p, index);
         //    Grid.SetRow(p, 0);

         //    portNamesBottom.Children.Add(tb);
         //    Grid.SetColumn(tb, index);
         //    Grid.SetRow(tb, 0);
         //}


      }

      Dictionary<dynPort, dynElement> previousEvalPortMappings = new Dictionary<dynPort, dynElement>();

      void CheckPortsForRecalc()
      {
         this.IsDirty = this.InPorts.All(
            delegate(dynPort p)
            {
               dynElement oldIn;
               var cons = p.Connectors;
               return !this.previousEvalPortMappings.TryGetValue(p, out oldIn)
                  || (oldIn == null && !cons.Any())
                  || (cons.Any() && oldIn == p.Connectors[0].Start.Owner);
            }
         );
      }

      /// <summary>
      /// When a port is connected, register a listener for the dynElementUpdated event
      /// and tell the object to build
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void p_PortConnected(object sender, EventArgs e)
      {
         var port = (dynPort)sender;
         if (port.PortType == PortType.INPUT)
            CheckPortsForRecalc();

         ValidateConnections();
      }

      void p_PortDisconnected(object sender, EventArgs e)
      {
         var port = (dynPort)sender;
         if (port.PortType == PortType.INPUT)
            CheckPortsForRecalc();

         //Destroy();
         ValidateConnections();
      }

      /// <summary>
      /// Color the connection according to it's port connectivity
      /// if all ports are connected, color green, else color orange
      /// </summary>
      public void ValidateConnections()
      {
         bool flag = false;

         foreach (dynPort p in inPorts)
         {
            if (p.Connectors.Count == 0)
            {
               flag = true;
            }
         }

         if (flag)
         {
            Dispatcher.Invoke(stateSetter, System.Windows.Threading.DispatcherPriority.Background, new object[] { this, ElementState.DEAD });
         }
         else
         {
            Dispatcher.Invoke(stateSetter, System.Windows.Threading.DispatcherPriority.Background, new object[] { this, ElementState.ACTIVE });
         }
      }

      protected void MarkConnectionState(bool bad)
      {
         //don't change the color of the object
         //if it's selected
         if (state != ElementState.SELECTED)
         {
            if (bad)
            {
               Dispatcher.Invoke(stateSetter, System.Windows.Threading.DispatcherPriority.Background, new object[] { this, ElementState.ERROR });
            }
            else
            {
               Dispatcher.Invoke(stateSetter, System.Windows.Threading.DispatcherPriority.Background, new object[] { this, ElementState.ACTIVE });
            }
         }
      }

      #region SJEChanges

      public virtual void SaveElement(System.Xml.XmlDocument xmlDoc, System.Xml.XmlElement dynEl)
      {

      }

      private int runCount;

      internal void ResetRuns()
      {
         if (this.runCount > 0)
         {
            PruneRuns();
            this.runCount = 0;
         }
      }

      void PruneRuns()
      {
         for (int i = this.elements.Count - 1; i >= this.runCount; i--)
         {
            var elems = this.elements[i];
            foreach (var e in elems)
               dynElementSettings.SharedInstance.Doc.Document.Delete(e);
            elems.Clear();
         }

         if (this.elements.Count > this.runCount)
         {
            this.elements.RemoveRange(
               this.runCount,
               this.elements.Count - this.runCount
            );
         }
      }

      private bool _isDirty = true;
      ///<summary>
      ///Does this element need to be regenerated?
      ///This value only changes to dirty when the input connections have been changed.
      ///</summary>
      public virtual bool IsDirty
      {
         get
         {
            if (this._isDirty)
               return true;
            else
            {
               bool dirty = this.InPorts.Any(x => x.Connectors.Any(y => y.Start.Owner.IsDirty));
               this._isDirty = dirty;
               return dirty;
            }
            //return this._isDirty
            //   || this.InPorts.Any(x => x.Connectors.Any(y => y.Start.Owner.IsDirty));
         }
         set
         {
            this._isDirty = value;
            if (value && this._report)
               this.WorkSpace.Modified(); //TODO: Implement
         }
      }

      private bool _report = true;

      protected Expression oldValue;

      private bool _saveResult = false;
      public bool SaveResult
      {
         get
         {
            return this._saveResult
               && this.InPorts.All(x => x.Connectors.Any());
         }
         set
         {
            this._saveResult = value;
         }
      }


      public void MarkDirty()
      {
         if (this._isDirty)
            return;
         else
         {
            bool dirty = this.InPorts.Any(x => x.Connectors.Any(y => y.Start.Owner.IsDirty));
            this._isDirty = dirty;
            return;
         }
      }

      protected void Delete(ElementId e)
      {
         dynElementSettings.SharedInstance.DeletionSet.Add(e);
      }


      protected internal virtual INode Build()
      {
         return this.compile(this.InPortData.Select(x => x.NickName));
      }

      protected internal virtual ProcedureCallNode Compile(IEnumerable<string> portNames)
      {
         if (this.SaveResult)
         {
            return new ExternalMacroNode(
               new ExternMacro(this.evalIfDirty),
               portNames
            );
         }
         else
         {
            return new ExternalFunctionNode(
               new FScheme.ExternFunc(this.eval),
               portNames
            );
         }
      }


      protected virtual void OnEvaluate() { }
      protected internal virtual void OnSave()
      {
         //Save all of the connection states, so we can check if this is dirty
         foreach (dynPort p in this.InPorts)
         {
            this.previousEvalPortMappings[p] = p.Connectors.Any()
               ? p.Connectors[0].Start.Owner
               : null;
         }
      }

      private Expression evalIfDirty(FSharpList<Expression> args, ExecutionEnvironment environment)
      {
         if (this.IsDirty || this.oldValue == null)
         {
            this.IsDirty = false;
            this.oldValue = this.eval(
               Utils.convertSequence(
                  args.Select(
                     input => environment.Evaluate(input)
                  )
               )
            );
         }
         else
            this.runCount++;
         return this.oldValue;
      }


      private INode compile(IEnumerable<string> portNames)
      {
         var node = this.Compile(portNames);

         for (int i = 0; i < this.InPortData.Count; i++)
         {
            var port = this.InPorts[i];
            if (port.Connectors.Count > 0)
            {
               var data = this.InPortData[i];
               node.ConnectInput(
                  data.NickName,
                  port.Connectors[0].Start.Owner.Build()
               );
            }
         }

         return node;
      }


      protected Expression eval(FSharpList<Expression> args)
      {
         var bench = dynElementSettings.SharedInstance.Bench;

         if (bench.CancelRun)
            throw new Exception("Run Cancelled.");

         if (this.SaveResult)
         {
            //Store the port mappings for this evaluate. We will compare later to see if it is dirty;
            foreach (dynPort p in this.InPorts)
            {
               this.previousEvalPortMappings[p] = p.Connectors.Any()
                  ? p.Connectors[0].Start.Owner
                  : null;
            }
         }

         object[] attribs = this.GetType().GetCustomAttributes(typeof(RequiresTransactionAttribute), false);
         bool useTransaction = attribs.Length > 0 && ((RequiresTransactionAttribute)attribs[0]).RequiresTransaction;
         bool debug = bench.RunInDebug;

         Expression result = null;

         this.OnEvaluate();

         if (useTransaction)
         {
            #region using transaction

            if (!debug)
            {
               #region no debug

               try
               {
                  if (bench.TransMode == TransactionMode.Manual && !bench.IsTransactionActive())
                  {
                     throw new Exception("A Revit transaction is required in order evaluate this element.");
                  }

                  bench.InitTransaction();

                  //if (this.SaveResult)
                  //   this.Destroy();

                  result = this.Evaluate(args);

                  UpdateLayoutDelegate uld = new UpdateLayoutDelegate(CallUpdateLayout);
                  Dispatcher.Invoke(uld, System.Windows.Threading.DispatcherPriority.Background, new object[] { this });

                  elementsHaveBeenDeleted = false;
               }
               catch (Exception ex)
               {
                  this.Dispatcher.Invoke(new Action(
                     delegate
                     {
                        Debug.WriteLine(ex.Message + " : " + ex.StackTrace);
                        bench.Log(ex.Message);
                        bench.ShowElement(this);

                        dynElementSettings.SharedInstance.Writer.WriteLine(ex.Message);
                        dynElementSettings.SharedInstance.Writer.WriteLine(ex.StackTrace);
                     }
                  ));

                  SetToolTipDelegate sttd = new SetToolTipDelegate(SetTooltip);
                  Dispatcher.Invoke(sttd, System.Windows.Threading.DispatcherPriority.Background,
                      new object[] { ex.Message });

                  MarkConnectionStateDelegate mcsd = new MarkConnectionStateDelegate(MarkConnectionState);
                  Dispatcher.Invoke(mcsd, System.Windows.Threading.DispatcherPriority.Background,
                      new object[] { true });
               }

               #endregion
            }
            else
            {
               #region debug

               this.Dispatcher.Invoke(new Action(
                  () =>
                     bench.Log("Starting a debug transaction for element: " + this.NickName)
               ));

               result = IdlePromise<Expression>.ExecuteOnIdle(
                  delegate
                  {
                     //this.Dispatcher.Invoke(new Action(
                     //   () =>
                     //      dynElementSettings.SharedInstance.Bench.Log("Creating transaction...")
                     //));

                     bench.InitTransaction();

                     try
                     {
                        //this.Destroy();

                        //this.Dispatcher.Invoke(new Action(
                        //   () =>
                        //      dynElementSettings.SharedInstance.Bench.Log("Evaluating Element")
                        //));

                        //if (this.SaveResult)
                        //   this.Destroy();

                        var exp = this.Evaluate(args);

                        UpdateLayoutDelegate uld = new UpdateLayoutDelegate(CallUpdateLayout);
                        Dispatcher.Invoke(uld, System.Windows.Threading.DispatcherPriority.Background, new object[] { this });

                        elementsHaveBeenDeleted = false;

                        //this.Dispatcher.Invoke(new Action(
                        //   () =>
                        //      dynElementSettings.SharedInstance.Bench.Log("Committing transaction")
                        //));

                        bench.EndTransaction();

                        return exp;
                     }
                     catch (Exception ex)
                     {
                        this.Dispatcher.Invoke(new Action(
                           delegate
                           {
                              Debug.WriteLine(ex.Message + " : " + ex.StackTrace);
                              bench.Log(ex.Message);
                              bench.ShowElement(this);
                           }
                        ));

                        SetToolTipDelegate sttd = new SetToolTipDelegate(SetTooltip);
                        Dispatcher.Invoke(sttd, System.Windows.Threading.DispatcherPriority.Background,
                            new object[] { ex.Message });

                        MarkConnectionStateDelegate mcsd = new MarkConnectionStateDelegate(MarkConnectionState);
                        Dispatcher.Invoke(mcsd, System.Windows.Threading.DispatcherPriority.Background,
                            new object[] { true });

                        bench.CancelTransaction();

                        this.Dispatcher.Invoke(new Action(
                           delegate
                           {
                              dynElementSettings.SharedInstance.Writer.WriteLine(ex.Message);
                              dynElementSettings.SharedInstance.Writer.WriteLine(ex.StackTrace);
                           }
                        ));

                        return null;
                     }
                  }
               );

               //dynElementSettings.SharedInstance.Bench.Dispatcher.Invoke(new Action(
               //   () =>
               //      dynElementSettings.SharedInstance.Bench.Log("End Idle Call")
               //));

               #endregion
            }

            #endregion
         }
         else
         {
            #region no transaction

            try
            {
               //if (this.SaveResult)
               //   this.Destroy();

               result = this.Evaluate(args);

               UpdateLayoutDelegate uld = new UpdateLayoutDelegate(CallUpdateLayout);
               Dispatcher.Invoke(uld, System.Windows.Threading.DispatcherPriority.Background, new object[] { this });

               elementsHaveBeenDeleted = false;
            }
            catch (Exception ex)
            {
               this.Dispatcher.Invoke(new Action(
                  delegate
                  {
                     Debug.WriteLine(ex.Message + " : " + ex.StackTrace);
                     dynElementSettings.SharedInstance.Bench.Log(ex.Message);

                     dynElementSettings.SharedInstance.Writer.WriteLine(ex.Message);
                     dynElementSettings.SharedInstance.Writer.WriteLine(ex.StackTrace);

                     dynElementSettings.SharedInstance.Bench.ShowElement(this);

                     MarkConnectionState(true);
                  }
               ));

               SetToolTipDelegate sttd = new SetToolTipDelegate(SetTooltip);
               Dispatcher.Invoke(sttd, System.Windows.Threading.DispatcherPriority.Background,
                   new object[] { ex.Message });
            }

            //try
            //{
            //   this.UpdateOutputs();
            //}
            //catch (Exception ex)
            //{
            //   //Debug.WriteLine("Outputs could not be updated.");
            //   dynElementSettings.SharedInstance.Bench.Log("Outputs could not be updated.");

            //   dynElementSettings.SharedInstance.Writer.WriteLine(ex.Message);
            //   dynElementSettings.SharedInstance.Writer.WriteLine(ex.StackTrace);
            //}

            #endregion
         }

         //Increment the run counter
         this.runCount++;

         if (result != null)
            return result;
         else
            throw new Exception();
      }

      public virtual Expression Evaluate(FSharpList<Expression> args)
      {
         throw new NotImplementedException();
      }

      #endregion


      /// <summary>
      /// The build method is called back from the child class.
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      //public void Build(object sender, EventArgs e)
      //{
      //   dynElement el = sender as dynElement;

      //   bool useTransaction = true;

      //   object[] attribs = el.GetType().GetCustomAttributes(typeof(RequiresTransactionAttribute), false);
      //   if (attribs.Length > 0)
      //   {
      //      if ((attribs[0] as RequiresTransactionAttribute).RequiresTransaction == false)
      //      {
      //         useTransaction = false;
      //      }
      //   }

      //   bool debug = dynElementSettings.SharedInstance.Bench.RunInDebug;

      //   if (!debug)
      //   {
      //      #region no debug

      //      #region using transaction
      //      if (useTransaction)
      //      {
      //         Transaction t = new Transaction(dynElementSettings.SharedInstance.Doc.Document, el.GetType().ToString() + " update.");
      //         TransactionStatus ts = t.Start();

      //         try
      //         {
      //            FailureHandlingOptions failOpt = t.GetFailureHandlingOptions();
      //            failOpt.SetFailuresPreprocessor(dynElementSettings.SharedInstance.WarningSwallower);
      //            t.SetFailureHandlingOptions(failOpt);

      //            el.Destroy();
      //            el.Draw();

      //            UpdateLayoutDelegate uld = new UpdateLayoutDelegate(CallUpdateLayout);
      //            Dispatcher.Invoke(uld, System.Windows.Threading.DispatcherPriority.Background, new object[] { el });

      //            elementsHaveBeenDeleted = false;

      //            ts = t.Commit();

      //         }
      //         catch (Exception ex)
      //         {
      //            Debug.WriteLine(ex.Message + " : " + ex.StackTrace);
      //            dynElementSettings.SharedInstance.Bench.Log(ex.Message);

      //            SetToolTipDelegate sttd = new SetToolTipDelegate(SetTooltip);
      //            Dispatcher.Invoke(sttd, System.Windows.Threading.DispatcherPriority.Background,
      //                new object[] { ex.Message });

      //            MarkConnectionStateDelegate mcsd = new MarkConnectionStateDelegate(MarkConnectionState);
      //            Dispatcher.Invoke(mcsd, System.Windows.Threading.DispatcherPriority.Background,
      //                new object[] { true });

      //            if (ts == TransactionStatus.Committed)
      //            {
      //               t.RollBack();
      //            }

      //            t.Dispose();

      //            dynElementSettings.SharedInstance.Writer.WriteLine(ex.Message);
      //            dynElementSettings.SharedInstance.Writer.WriteLine(ex.StackTrace);
      //         }

      //         try
      //         {
      //            el.UpdateOutputs();
      //         }
      //         catch (Exception ex)
      //         {

      //            //Debug.WriteLine("Outputs could not be updated.");
      //            dynElementSettings.SharedInstance.Bench.Log("Outputs could not be updated.");
      //            if (ts == TransactionStatus.Committed)
      //            {
      //               t.RollBack();
      //            }

      //            t.Dispose();

      //            dynElementSettings.SharedInstance.Writer.WriteLine(ex.Message);
      //            dynElementSettings.SharedInstance.Writer.WriteLine(ex.StackTrace);
      //         }
      //      }
      //      #endregion

      //      #region no transaction
      //      if (!useTransaction)
      //      {
      //         try
      //         {

      //            el.Destroy();

      //            el.Draw();

      //            UpdateLayoutDelegate uld = new UpdateLayoutDelegate(CallUpdateLayout);
      //            Dispatcher.Invoke(uld, System.Windows.Threading.DispatcherPriority.Background, new object[] { el });

      //            elementsHaveBeenDeleted = false;
      //         }
      //         catch (Exception ex)
      //         {
      //            Debug.WriteLine(ex.Message + " : " + ex.StackTrace);
      //            dynElementSettings.SharedInstance.Bench.Log(ex.Message);

      //            SetToolTipDelegate sttd = new SetToolTipDelegate(SetTooltip);
      //            Dispatcher.Invoke(sttd, System.Windows.Threading.DispatcherPriority.Background,
      //                new object[] { ex.Message });

      //            MarkConnectionState(true);

      //            dynElementSettings.SharedInstance.Writer.WriteLine(ex.Message);
      //            dynElementSettings.SharedInstance.Writer.WriteLine(ex.StackTrace);

      //         }

      //         try
      //         {
      //            el.UpdateOutputs();
      //         }
      //         catch (Exception ex)
      //         {

      //            //Debug.WriteLine("Outputs could not be updated.");
      //            dynElementSettings.SharedInstance.Bench.Log("Outputs could not be updated.");

      //            dynElementSettings.SharedInstance.Writer.WriteLine(ex.Message);
      //            dynElementSettings.SharedInstance.Writer.WriteLine(ex.StackTrace);
      //         }
      //      }
      //      #endregion

      //      #endregion
      //   }
      //   else
      //   {
      //      #region debug

      //      #region using transaction
      //      if (useTransaction)
      //      {
      //         Transaction t = new Transaction(dynElementSettings.SharedInstance.Doc.Document, el.GetType().ToString() + " update.");
      //         TransactionStatus ts = t.Start();

      //         try
      //         {
      //            FailureHandlingOptions failOpt = t.GetFailureHandlingOptions();
      //            failOpt.SetFailuresPreprocessor(dynElementSettings.SharedInstance.WarningSwallower);
      //            t.SetFailureHandlingOptions(failOpt);

      //            el.Destroy();
      //            el.Draw();

      //            UpdateLayoutDelegate uld = new UpdateLayoutDelegate(CallUpdateLayout);
      //            Dispatcher.Invoke(uld, System.Windows.Threading.DispatcherPriority.Background, new object[] { el });

      //            elementsHaveBeenDeleted = false;

      //            ts = t.Commit();

      //         }
      //         catch (Exception ex)
      //         {
      //            Debug.WriteLine(ex.Message + " : " + ex.StackTrace);
      //            dynElementSettings.SharedInstance.Bench.Log(ex.Message);

      //            SetToolTipDelegate sttd = new SetToolTipDelegate(SetTooltip);
      //            Dispatcher.Invoke(sttd, System.Windows.Threading.DispatcherPriority.Background,
      //                new object[] { ex.Message });

      //            MarkConnectionStateDelegate mcsd = new MarkConnectionStateDelegate(MarkConnectionState);
      //            Dispatcher.Invoke(mcsd, System.Windows.Threading.DispatcherPriority.Background,
      //                new object[] { true });

      //            if (ts == TransactionStatus.Committed)
      //            {
      //               t.RollBack();
      //            }

      //            t.Dispose();

      //            dynElementSettings.SharedInstance.Writer.WriteLine(ex.Message);
      //            dynElementSettings.SharedInstance.Writer.WriteLine(ex.StackTrace);
      //         }

      //         try
      //         {
      //            el.UpdateOutputs();
      //         }
      //         catch (Exception ex)
      //         {

      //            //Debug.WriteLine("Outputs could not be updated.");
      //            dynElementSettings.SharedInstance.Bench.Log("Outputs could not be updated.");
      //            if (ts == TransactionStatus.Committed)
      //            {
      //               t.RollBack();
      //            }

      //            t.Dispose();

      //            dynElementSettings.SharedInstance.Writer.WriteLine(ex.Message);
      //            dynElementSettings.SharedInstance.Writer.WriteLine(ex.StackTrace);
      //         }
      //      }

      //      #endregion

      //      #region no transaction
      //      if (!useTransaction)
      //      {
      //         try
      //         {

      //            el.Destroy();

      //            el.Draw();

      //            UpdateLayoutDelegate uld = new UpdateLayoutDelegate(CallUpdateLayout);
      //            Dispatcher.Invoke(uld, System.Windows.Threading.DispatcherPriority.Background, new object[] { el });

      //            elementsHaveBeenDeleted = false;
      //         }
      //         catch (Exception ex)
      //         {
      //            Debug.WriteLine(ex.Message + " : " + ex.StackTrace);
      //            dynElementSettings.SharedInstance.Bench.Log(ex.Message);

      //            SetToolTipDelegate sttd = new SetToolTipDelegate(SetTooltip);
      //            Dispatcher.Invoke(sttd, System.Windows.Threading.DispatcherPriority.Background,
      //                new object[] { ex.Message });

      //            MarkConnectionState(true);

      //            dynElementSettings.SharedInstance.Writer.WriteLine(ex.Message);
      //            dynElementSettings.SharedInstance.Writer.WriteLine(ex.StackTrace);

      //         }

      //         try
      //         {
      //            el.UpdateOutputs();
      //         }
      //         catch (Exception ex)
      //         {

      //            //Debug.WriteLine("Outputs could not be updated.");
      //            dynElementSettings.SharedInstance.Bench.Log("Outputs could not be updated.");

      //            dynElementSettings.SharedInstance.Writer.WriteLine(ex.Message);
      //            dynElementSettings.SharedInstance.Writer.WriteLine(ex.StackTrace);
      //         }
      //      }
      //      #endregion

      //      #endregion
      //   }
      //}

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
         this.runCount = 0;
         foreach (var els in this.elements)
         {
            foreach (Element e in els)
            {
               dynElementSettings.SharedInstance.Doc.Document.Delete(e);
            }
            //els.Clear();
         }

         //clear out the array to avoid object initialization errors
         elements.Clear();

         //clear the data tree
         //dataTree.Clear();
      }

      public virtual void Update()
      {
      }

      public void UpdateOutputs()
      {
         //send the messages without updating
         foreach (dynConnector c in outPort.Connectors)
         {
            c.SendMessage();
         }

         //aggregate the unique output nodes
         //this avoids multiple updates of the same node
         //if a node has the same node connected to several of its outputs
         var uniqueNodes = new HashSet<dynElement>();
         foreach (dynConnector c in outPort.Connectors)
         {
            //if (!uniqueNodes.Contains(c.End.Owner))
            //{
            uniqueNodes.Add(c.End.Owner);
            //}
         }

         //update the unique nodes
         foreach (dynElement el in uniqueNodes)
         {
            el.Update();
         }
      }

      public void FindDownstreamElements(ref List<dynElement> downStream)
      {
         foreach (dynConnector c in outPort.Connectors)
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

      public void ClearOutputs()
      {
         foreach (dynConnector c in outPort.Connectors)
         {
            c.Kill();
         }

         outPort = null;
         outPortData = null;
      }

      public bool CheckInputs()
      {
         MarkConnectionStateDelegate mcsd = new MarkConnectionStateDelegate(MarkConnectionState);

         foreach (PortData pd in InPortData)
         {
            //if the port data's object is null
            //or the port data's object type can not be matched
            if (pd.Object == null) //|| pd.Object.GetType() != pd.PortType)
            {
               Dispatcher.Invoke(mcsd, System.Windows.Threading.DispatcherPriority.Background,
                   new object[] { true });

               SetToolTipDelegate sttd = new SetToolTipDelegate(SetTooltip);
               Dispatcher.Invoke(sttd, System.Windows.Threading.DispatcherPriority.Background,
                   new object[] { "One or more connections is null." });

               return false;
            }
         }


         Dispatcher.Invoke(mcsd, System.Windows.Threading.DispatcherPriority.Background,
             new object[] { false });

         return true;
      }

      public void Select()
      {
         Dispatcher.Invoke(stateSetter, System.Windows.Threading.DispatcherPriority.Background, new object[] { this, ElementState.SELECTED });
      }

      public void Deselect()
      {
         Dispatcher.Invoke(stateSetter, System.Windows.Threading.DispatcherPriority.Background, new object[] { this, ElementState.ACTIVE });
      }

      void SetState(dynElement el, ElementState state)
      {
         el.State = state;
      }

      private void topControl_Loaded(object sender, RoutedEventArgs e)
      {

      }

      //for information about routed events see:
      //http://msdn.microsoft.com/en-us/library/ms742806.aspx

      //tunneling event
      //from MSDN "...Tunneling routed events are often used or handled as part of the compositing for a 
      //control, such that events from composite parts can be deliberately suppressed or replaced by 
      //events that are specific to the complete control.
      //starts at parent and climbs down children to element
      private void OnPreviewKeyUp(object sender, KeyEventArgs e)
      {
         //e.Handled = true;
      }

      //bubbling event
      //from MSDN "...Bubbling routed events are generally used to report input or state changes 
      //from distinct controls or other UI elements."
      //starts at element and climbs up parents to root
      private void OnKeyUp(object sender, KeyEventArgs e)
      {
         //set handled to avoid triggering key press
         //events on the workbench
         //e.Handled = true;
      }

      private void longestList_cm_Click(object sender, RoutedEventArgs e)
      {
         lacingType = LacingType.LONGEST;
         //fullLace_cm.IsChecked = false;
         //shortestList_cm.IsChecked = false;
         //longestList_cm.IsChecked = true;
      }

      private void shortestList_cm_Click(object sender, RoutedEventArgs e)
      {
         lacingType = LacingType.SHORTEST;
         //fullLace_cm.IsChecked = false;
         //shortestList_cm.IsChecked = true;
         //longestList_cm.IsChecked = false;
      }

      private void fullLace_cm_Click(object sender, RoutedEventArgs e)
      {
         lacingType = LacingType.FULL;
         //fullLace_cm.IsChecked = true;
         //shortestList_cm.IsChecked = false;
         //longestList_cm.IsChecked = false;
      }

      private void MainContextMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
      {

      }

      public virtual void LoadElement(System.Xml.XmlNode elNode)
      {

      }

      private void deleteElem_cm_Click(object sender, RoutedEventArgs e)
      {
         //this.Destroy();
         this.DisableReporting();
         var bench = dynElementSettings.SharedInstance.Bench;

         //IdlePromise<bool>.ExecuteOnIdle(
         //   delegate
         //   {
         bench.InitTransaction();
         try
         {
            this.Destroy();
         }
         catch { }
         bench.EndTransaction();
         //   }
         //);

         bench.DeleteElement(this);
         this.WorkSpace.Modified();
      }

      internal void DisableReporting()
      {
         this._report = false;
      }

      internal void EnableReporting()
      {
         this._report = true;
      }
   }

   [ValueConversion(typeof(double), typeof(Thickness))]
   public class MarginConverter : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
         double height = (double)value;
         return new Thickness(0, -1 * height - 3, 0, 0);
      }

      public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
         return null;
      }
   }
}
