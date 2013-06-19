//Copyright 2013 Ian Keough

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
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Xml;
using System.Windows.Data;

using Autodesk.Revit.DB;

using Dynamo.Connectors;
using Dynamo.FSchemeInterop;
using Dynamo.Utilities;

using Microsoft.FSharp.Collections;

using Value = Dynamo.FScheme.Value;
using Dynamo.Revit;

namespace Dynamo.Nodes
{
    /// <summary>
    /// A class used to store a name and associated item for a drop down menu
    /// </summary>
    public class DynamoDropDownItem
    {
        public string Name { get; set; }
        public object Item { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public DynamoDropDownItem(string name, object item)
        {
            Name = name;
            Item = item;
        }
    }
    /// <summary>
    /// Base class for all nodes using a drop down
    /// </summary>
    public abstract class dynDropDrownBase : dynNodeWithOneOutput
    {
        private ObservableCollection<DynamoDropDownItem> items = new ObservableCollection<DynamoDropDownItem>();
        public ObservableCollection<DynamoDropDownItem> Items
        {
            get { return items; }
            set 
            { 
                items = value;
                RaisePropertyChanged("Items");
            }
        }

        private int selectedIndex = 0;
        public int SelectedIndex
        {
            get { return selectedIndex; }
            set
            {
                //do not allow selected index to
                //go out of range of the items collection
                if (value > Items.Count - 1)
                {
                    selectedIndex = -1;
                }
                else
                    selectedIndex = value;
                RaisePropertyChanged("SelectedIndex");
            }
        }

        public override void SetupCustomUIElements(Controls.dynNodeView nodeUI)
        {
            base.SetupCustomUIElements(nodeUI);

            //add a drop down list to the window
            ComboBox combo = new ComboBox();
            combo.Width = 300;
            combo.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            combo.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            nodeUI.inputGrid.Children.Add(combo);
            System.Windows.Controls.Grid.SetColumn(combo, 0);
            System.Windows.Controls.Grid.SetRow(combo, 0);

            combo.DropDownOpened += new EventHandler(combo_DropDownOpened);
            combo.SelectionChanged += delegate
            {
                if (combo.SelectedIndex != -1)
                    this.RequiresRecalc = true;
            };

            combo.DataContext = this;
            //bind this combo box to the selected item hash
            
            var bindingVal = new System.Windows.Data.Binding("Items")
            {
                Mode = BindingMode.TwoWay,
                Source = this
            };
            combo.SetBinding(ComboBox.ItemsSourceProperty, bindingVal);

            //bind the selected index to the 
            var indexBinding = new System.Windows.Data.Binding("SelectedIndex")
            {
                Mode = BindingMode.TwoWay,
                Source = this
            };
            combo.SetBinding(ComboBox.SelectedIndexProperty, indexBinding);
        }

        public override void SaveNode(XmlDocument xmlDoc, XmlElement dynEl, SaveContext context)
        {
            dynEl.SetAttribute("index", SelectedIndex.ToString());
        }

        public override void LoadNode(XmlNode elNode)
        {
            try
            {
                SelectedIndex = Convert.ToInt32(elNode.Attributes["index"].Value);
            }
            catch { }
        }

        public virtual void PopulateItems()
        {
            //override in child classes
        }

        /// <summary>
        /// When the dropdown is opened, the node's implementation of PopulateItemsHash is called
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void combo_DropDownOpened(object sender, EventArgs e)
        {
            PopulateItems();
        }
    }

    [NodeName("Select Fam")]
    [NodeCategory(BuiltinNodeCategories.CORE_SELECTION)]
    [NodeDescription("Select a Family Type from a drop down list.")]
    [IsInteractive(true)]
    public class dynFamilyTypeSelector : dynDropDrownBase
    {
        public dynFamilyTypeSelector()
        {
            OutPortData.Add(new PortData("", "Family type", typeof(Value.Container)));
            RegisterAllPorts();

            PopulateItems();
        }

        public override void PopulateItems()
        {
            Items.Clear();

            //load all the currently loaded types into the combo list
            FilteredElementCollector fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            fec.OfClass(typeof(Family));
            foreach (Family f in fec.ToElements())
            {
                foreach (FamilySymbol fs in f.Symbols)
                {
                    Items.Add(new DynamoDropDownItem(f.Name + ":" + fs.Name, fs)); 
                }
            }
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            if(SelectedIndex < 0)
                throw new Exception("Nothing selected!");

            return Value.NewContainer(Items[SelectedIndex].Item);
        }

    }

    [NodeName("Select Family Instance Parameter")]
    [NodeCategory(BuiltinNodeCategories.CORE_SELECTION)]
    [NodeDescription("Given a Family Instance or Symbol, allows the user to select a parameter as a string.")]
    [NodeSearchTags("fam")]
    [IsInteractive(true)]
    public class dynFamilyInstanceParameterSelector : dynDropDrownBase
    {
        ElementId storedId = null;
        private Element element;

        public dynFamilyInstanceParameterSelector()
        {
            InPortData.Add(new PortData("f", "Family Symbol or Instance", typeof(Value.Container)));
            OutPortData.Add(new PortData("", "Parameter Name", typeof(Value.String)));

            RegisterAllPorts();
        }

        private static string getStorageTypeString(StorageType st)
        {
            switch (st)
            {
                case StorageType.Integer:
                    return "int";
                case StorageType.Double:
                    return "dbl";
                case StorageType.String:
                    return "str";
                case StorageType.ElementId:
                default:
                    return "id";
            }
        }

        public override void PopulateItems() //(IEnumerable set, bool readOnly)
        {
            var doc = dynRevitSettings.Doc.Document;

            this.Items.Clear();

            if (element is FamilySymbol)
            {
                var paramDict = new Dictionary<string, dynamic>();

                var fs = element as FamilySymbol;

                foreach (dynamic p in fs.Parameters)
                {
                    if (p.IsReadOnly || p.StorageType == StorageType.None)
                        continue;
                    Items.Add(
                            new DynamoDropDownItem(p.Definition.Name + " (" + getStorageTypeString(p.StorageType) + ")", p));
                }

                var fd = doc.EditFamily(fs.Family);
                var ps = fd.FamilyManager.Parameters;

                foreach (dynamic p in ps)
                {
                    if (p.IsReadOnly || p.StorageType == StorageType.None)
                        continue;
                    Items.Add(
                            new DynamoDropDownItem(p.Definition.Name + " (" + getStorageTypeString(p.StorageType) + ")", p));
                }

            }
            else if (element is FamilyInstance)
            {
                var fi = element as FamilyInstance;

                foreach (dynamic p in fi.Parameters)
                {
                    if (p.IsReadOnly || p.StorageType == StorageType.None)
                        continue;
                    Items.Add(
                            new DynamoDropDownItem(p.Definition.Name + " (" + getStorageTypeString(p.StorageType) + ")", p));
                }
            }
            else
            {
                this.storedId = null;
            }

            this.Items = this.Items.OrderBy(x => x.Name).ToObservableCollection<DynamoDropDownItem>();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            element = (Element)((Value.Container)args[0]).Item;

            if (element.GetType() != typeof(FamilyInstance))
            {
                throw new Exception("The input is not a Family Instance.");
            }

            if (!element.Id.Equals(this.storedId))
            {
                this.storedId = element.Id;

                PopulateItems();
            }

            if(SelectedIndex == -1)
                throw new Exception("Please select a parameter.");

            return Value.NewContainer(((Parameter)Items[SelectedIndex].Item).Definition);
        }

        public override void SaveNode(XmlDocument xmlDoc, XmlElement dynEl, SaveContext context)
        {
            if (this.storedId != null)
            {
                XmlElement outEl = xmlDoc.CreateElement("familyid");
                outEl.SetAttribute("value", this.storedId.IntegerValue.ToString());
                dynEl.AppendChild(outEl);

                XmlElement param = xmlDoc.CreateElement("index");
                param.SetAttribute("value", SelectedIndex.ToString());
                dynEl.AppendChild(param);
            }

        }

        public override void LoadNode(XmlNode elNode)
        {
            var doc = dynRevitSettings.Doc.Document;

            int index = -1;

            foreach (XmlNode subNode in elNode.ChildNodes)
            {
                if (subNode.Name.Equals("familyid"))
                {
                    int id;
                    try
                    {
                        id = Convert.ToInt32(subNode.Attributes[0].Value);
                    }
                    catch
                    {
                        continue;
                    }
                    this.storedId = new ElementId(id);

                    element = doc.GetElement(this.storedId);

                }
                else if (subNode.Name.Equals("index"))
                {
                    try
                    {
                        index = Convert.ToInt32(subNode.Attributes[0].Value);
                    }
                    catch
                    {
                    }
                }
            }

            if (element != null)
            {
                PopulateItems();
                SelectedIndex = index;
            }  
        }
    }

    #region Disabled ParameterMapper

    //[NodeName("Instance Parameter Mapper")]
    //[NodeCategory(BuiltinNodeCategories.REVIT)]
    //[NodeDescription("Maps the parameters of a Family Type.")]
    //[RequiresTransaction(true)]
    //public class dynInstanceParameterMapper : dynElement
    //{
    //   Hashtable parameterMap = new Hashtable();

    //   public Hashtable ParameterMap
    //   {
    //      get { return parameterMap; }
    //   }

    //   public dynInstanceParameterMapper()
    //   {

    //      this.topControl.Width = 300;

    //      InPortData.Add(new PortData(null, "fi", "The family instance(s) to map.", typeof(dynElement)));
    //      OutPortData.Add(new PortData(null, "", "A map of parameter values on the instance.", typeof(dynInstanceParameterMapper)));
    //      OutPortData[0].Object = parameterMap;

    //      //add a button to the inputGrid on the dynElement
    //      System.Windows.Controls.Button paramMapButt = new System.Windows.Controls.Button();
    //      this.inputGrid.Children.Add(paramMapButt);
    //      paramMapButt.Margin = new System.Windows.Thickness(0, 0, 0, 0);
    //      paramMapButt.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
    //      paramMapButt.VerticalAlignment = System.Windows.VerticalAlignment.Center;
    //      paramMapButt.Click += new System.Windows.RoutedEventHandler(paramMapButt_Click);
    //      paramMapButt.Content = "Map";
    //      paramMapButt.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
    //      paramMapButt.VerticalAlignment = System.Windows.VerticalAlignment.Center;

    //      NodeUI.RegisterInputsAndOutput();

    //   }

    //   void paramMapButt_Click(object sender, System.Windows.RoutedEventArgs e)
    //   {
    //      //read from the state objects
    //      if (CheckInputs())
    //      {
    //         CleanupOldPorts();

    //         DataTree treeIn = InPortData[0].Object as DataTree;
    //         if (treeIn != null)
    //         {
    //            //find the first family instance in the tree
    //            //and map it
    //            FamilyInstance fi = treeIn.Trunk.FindFirst() as FamilyInstance;
    //            if (fi != null)
    //            {
    //               MapPorts(fi);
    //            }

    //         }
    //      }
    //   }

    //   private void MapPorts(FamilyInstance fi)
    //   {
    //      parameterMap.Clear();

    //      foreach (Parameter p in fi.Parameters)
    //      {
    //         if (!p.IsReadOnly)  //don't want it if it is read only
    //         {
    //            if (p.StorageType == StorageType.Double) //MDJ 11-23-11 - ian just had doubles but we need to add Ints, should convert to a switch case if we need more
    //            {
    //               string paramName = p.Definition.Name;

    //               PortData pd = new PortData(null,
    //                   p.Definition.Name.Substring(0, Math.Min(p.Definition.Name.Length, 3)),
    //                   paramName,
    //                   typeof(dynDouble));
    //               InPortData.Add(pd);
    //               parameterMap.Add(paramName, pd.Object);
    //            }
    //            else if (p.StorageType == StorageType.Integer)
    //            {
    //               string paramName = p.Definition.Name;

    //               PortData pd = new PortData(null,
    //                   p.Definition.Name.Substring(0, Math.Min(p.Definition.Name.Length, 3)),
    //                   paramName,
    //                   typeof(double));
    //               InPortData.Add(pd);
    //               parameterMap.Add(paramName, pd.Object);
    //            }
    //            else if (p.StorageType == StorageType.String)
    //            {
    //               string paramName = p.Definition.Name;

    //               PortData pd = new PortData(null,
    //                   p.Definition.Name.Substring(0, Math.Min(p.Definition.Name.Length, 3)),
    //                   paramName,
    //                   typeof(dynString));
    //               InPortData.Add(pd);
    //               parameterMap.Add(paramName, pd.Object);
    //            }
    //         }
    //      }

    //      //add back new ports
    //      for (int i = 1; i < InPortData.Count; i++)
    //      {
    //         dynElement el = InPortData[i].Object as dynElement;

    //         RowDefinition rd = new RowDefinition();
    //         gridLeft.RowDefinitions.Add(rd);

    //         //add a port for each input
    //         //distribute the ports along the 
    //         //edges of the icon
    //         AddPort(el, PortType.INPUT, InPortData[i].NickName, i);
    //      }

    //      //resize this thing
    //      base.ResizeElementForInputs();

    //      base.SetToolTips();
    //   }

    //   private void CleanupOldPorts()
    //   {

    //      //clear all the inputs but the first one
    //      //which is the family instance
    //      //first kill all the connectors
    //      for (int i = 1; i < InPortData.Count; i++)
    //      {
    //         dynPort p = InPorts[i];

    //         //must remove the connectors iteratively
    //         //do not use a foreach here!
    //         while (p.Connectors.Count > 0)
    //         {
    //            dynConnector c = p.Connectors[p.Connectors.Count - 1] as dynConnector;
    //            c.NotifyConnectedPortsOfDeletion();
    //         }
    //      }

    //      //then remove all the ports
    //      while (InPorts.Count > 1)
    //      {
    //         InPorts.RemoveAt(InPorts.Count - 1);
    //         InPortData.RemoveAt(InPortData.Count - 1);
    //      }

    //      while (gridLeft.RowDefinitions.Count > 1)
    //      {
    //         //remove the port from the children list
    //         gridLeft.Children.RemoveAt(gridLeft.RowDefinitions.Count - 1);
    //         gridLeft.RowDefinitions.RemoveAt(gridLeft.RowDefinitions.Count - 1);

    //      }

    //   }

    //   //sets the values on the family instances
    //   public override void Draw()
    //   {
    //      DataTree familyInstTree = InPortData[0].Object as DataTree;

    //      //skip the first port data because it's the family instances
    //      for (int i = 1; i < InPortData.Count; i++)
    //      {
    //         PortData pd = InPortData[i];
    //         //parameter value keys are the tooltip - the name 
    //         //of the parameter
    //         //set the objects on the parameter map
    //         parameterMap[pd.ToolTipString] = pd.Object;

    //         //start by assuming that you've got matching data trees
    //         DataTree doubleTree = pd.Object as DataTree;

    //         if (familyInstTree != null)
    //         {
    //            if (doubleTree != null)
    //            {
    //               //get the parameter represented by the port data
    //               Process(familyInstTree.Trunk, doubleTree.Trunk, pd.ToolTipString);
    //            }
    //            else
    //            {
    //               //if the incoming object is not null
    //               //then let's try to use it.
    //               if (pd.Object != null)
    //               {
    //                  //we've got a single value incoming
    //                  ProcessSingleValue(familyInstTree.Trunk, pd.Object, pd.ToolTipString);
    //               }
    //            }
    //         }

    //      }

    //   }

    //   public void Process(DataTreeBranch familyBranch, DataTreeBranch doubleBranch, string paramName)
    //   {
    //      int leafCount = 0;
    //      foreach (object o in familyBranch.Leaves)
    //      {
    //         FamilyInstance fi = o as FamilyInstance;
    //         if (fi != null)
    //         {
    //            Parameter p = fi.get_Parameter(paramName);
    //            if (p != null)
    //            {
    //               if (p.StorageType == StorageType.Double) //MDJ 11-23-11 - ian just had doubles but we need to add Ints, should convert to a switch case if we need more
    //               {
    //                  p.Set(Convert.ToDouble(doubleBranch.Leaves[leafCount]));
    //               }
    //               else if (p.StorageType == StorageType.Integer)
    //               {
    //                  p.Set(Convert.ToInt32(doubleBranch.Leaves[leafCount]));
    //               }
    //               else if (p.StorageType == StorageType.String)
    //               {
    //                  p.Set(Convert.ToString(doubleBranch.Leaves[leafCount]));
    //               }
    //            }
    //         }
    //         leafCount++;
    //      }

    //      int subBranchCount = 0;
    //      foreach (DataTreeBranch nextBranch in familyBranch.Branches)
    //      {
    //         //don't do this if the double tree doesn't
    //         //have a member in the same location
    //         if (doubleBranch.Branches.Count - 1 >= subBranchCount)
    //         {
    //            Process(nextBranch, doubleBranch.Branches[subBranchCount], paramName);
    //         }
    //         subBranchCount++;
    //      }
    //   }

    //   public void ProcessSingleValue(DataTreeBranch familyBranch, object value, string paramName)
    //   {
    //      int leafCount = 0;
    //      foreach (object o in familyBranch.Leaves)
    //      {
    //         FamilyInstance fi = o as FamilyInstance;
    //         if (fi != null)
    //         {
    //            Parameter p = fi.get_Parameter(paramName);
    //            if (p != null)
    //            {
    //               if (p.StorageType == StorageType.Double) //MDJ 11-23-11 - ian just had doubles but we need to add Ints, should convert to a switch case if we need more
    //               {
    //                  p.Set(Convert.ToDouble(value));
    //               }
    //               else if (p.StorageType == StorageType.Integer)
    //               {
    //                  p.Set(Convert.ToInt32(value));
    //               }
    //               else if (p.StorageType == StorageType.String)
    //               {
    //                  p.Set(Convert.ToString(value));
    //               }
    //            }
    //         }
    //         leafCount++;
    //      }

    //      foreach (DataTreeBranch nextBranch in familyBranch.Branches)
    //      {
    //         ProcessSingleValue(nextBranch, value, paramName);
    //      }
    //   }

    //   public override void Update()
    //   {
    //      OnDynElementReadyToBuild(EventArgs.Empty);
    //   }

    //   public override void Destroy()
    //   {
    //      base.Destroy();
    //   }
    //}

    //[NodeName("Family Instance Parameter Evaluation")]
    //[NodeCategory(BuiltinNodeCategories.REVIT)]
    //[NodeDescription("Modifies parameters on family instances.")]
    //[RequiresTransaction(true)]
    //public class dynFamilyInstanceParameterEvaluation : dynElement
    //{

    //   public dynFamilyInstanceParameterEvaluation()
    //   {

    //      InPortData.Add(new PortData(null, "fi", "Family instances.", typeof(dynElement)));
    //      InPortData.Add(new PortData(null, "map", "Parameter map.", typeof(dynInstanceParameterMapper)));

    //      NodeUI.RegisterInputsAndOutput();

    //   }

    //   public override void Draw()
    //   {
    //      if (CheckInputs())
    //      {
    //         DataTree treeIn = InPortData[0].Object as DataTree;
    //         if (treeIn != null)
    //         {
    //            Hashtable parameterMap = InPortData[1].Object as Hashtable;
    //            if (parameterMap != null)
    //               ProcessState(treeIn.Trunk, parameterMap);
    //         }
    //      }

    //      base.Draw();
    //   }

    //   private void ProcessState(DataTreeBranch bIn, Hashtable parameterMap)
    //   {
    //      foreach (object o in bIn.Leaves)
    //      {
    //         FamilyInstance fi = o as FamilyInstance;
    //         if (fi != null)
    //         {
    //            foreach (DictionaryEntry de in parameterMap)
    //            {
    //               if (de.Value != null)
    //               {
    //                  //find the parameter on the family instance
    //                  Parameter p = fi.get_Parameter(de.Key.ToString());
    //                  if (p != null)
    //                  {
    //                     if (p.StorageType == StorageType.Double)
    //                     {
    //                        p.Set((double)de.Value);
    //                     }
    //                     else if (p.StorageType == StorageType.Integer)
    //                     {
    //                        p.Set((int)de.Value);
    //                     }
    //                     else if (p.StorageType == StorageType.String)
    //                     {
    //                        p.Set((string)de.Value);
    //                     }
    //                  }
    //               }
    //            }
    //         }
    //      }

    //      foreach (DataTreeBranch nextBranch in bIn.Branches)
    //      {
    //         ProcessState(nextBranch, parameterMap);
    //      }
    //   }

    //   public override void Update()
    //   {
    //      OnDynElementReadyToBuild(EventArgs.Empty);
    //   }

    //   public override void Destroy()
    //   {
    //      base.Destroy();
    //   }
    //}

    #endregion

    [NodeName("Create Family Instance")]
    [NodeCategory(BuiltinNodeCategories.REVIT_FAMILYCREATION)]
    [NodeDescription("Creates family instances at a given XYZ location.")]
    public class dynFamilyInstanceCreatorXYZ : dynRevitTransactionNodeWithOneOutput
    {
        public dynFamilyInstanceCreatorXYZ()
        {
            InPortData.Add(new PortData("xyz", "xyz", typeof(Value.Container)));
            InPortData.Add(new PortData("type", "The Family Symbol to use for instantiation.", typeof(Value.Container)));
            OutPortData.Add(new PortData("fi", "Family instances created by this operation.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        private Value makeFamilyInstance(object location, FamilySymbol fs, int count)
        {
            XYZ pos = location is ReferencePoint
               ? (location as ReferencePoint).Position
               : (XYZ)location;

            FamilyInstance fi;

            if (this.Elements.Count > count)
            {
                Element e;
                if (dynUtils.TryGetElement(this.Elements[count],typeof(FamilyInstance), out e))
                {
                    fi = this.UIDocument.Document.GetElement(this.Elements[count]) as FamilyInstance;
                    fi.Symbol = fs;
                    LocationPoint lp = fi.Location as LocationPoint;
                    lp.Point = pos;
                }
                else
                {
                    fi = this.UIDocument.Document.IsFamilyDocument
                       ? this.UIDocument.Document.FamilyCreate.NewFamilyInstance(
                          pos, fs, Autodesk.Revit.DB.Structure.StructuralType.NonStructural
                       )
                       : this.UIDocument.Document.Create.NewFamilyInstance(
                          pos, fs, Autodesk.Revit.DB.Structure.StructuralType.NonStructural
                       );

                    this.Elements[count] = fi.Id;
                }
            }
            else
            {
                fi = this.UIDocument.Document.IsFamilyDocument
                   ? this.UIDocument.Document.FamilyCreate.NewFamilyInstance(
                      pos, fs, Autodesk.Revit.DB.Structure.StructuralType.NonStructural
                   )
                   : this.UIDocument.Document.Create.NewFamilyInstance(
                      pos, fs, Autodesk.Revit.DB.Structure.StructuralType.NonStructural
                   );

                this.Elements.Add(fi.Id);
            }

            return Value.NewContainer(fi);
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            FamilySymbol fs = (FamilySymbol)((Value.Container)args[1]).Item;
            var input = args[0];

            if (input.IsList)
            {
                var locList = (input as Value.List).Item;

                int count = 0;

                var result = Value.NewList(
                   Utils.SequenceToFSharpList(
                      locList.Select(
                         x =>
                            this.makeFamilyInstance(
                               ((Value.Container)x).Item,
                               fs,
                               count++
                            )
                      )
                   )
                );

                foreach (var e in this.Elements.Skip(count))
                {
                    this.DeleteElement(e);
                }

                return result;
            }
            else
            {
                var result = this.makeFamilyInstance(
                   ((Value.Container)input).Item,
                   fs,
                   0
                );

                foreach (var e in this.Elements.Skip(1))
                {
                    this.DeleteElement(e);
                }

                return result;
            }
        }
    }

    [NodeName("Create Family Instance By Level")]
    [NodeCategory(BuiltinNodeCategories.REVIT_FAMILYCREATION)]
    [NodeDescription("Creates family instances in the given level.")]
    public class dynFamilyInstanceCreatorLevel : dynRevitTransactionNodeWithOneOutput
    {
        public dynFamilyInstanceCreatorLevel()
        {
            InPortData.Add(new PortData("xyz", "xyz", typeof(Value.Container)));
            InPortData.Add(new PortData("typ", "The Family Symbol to use for instantiation.", typeof(Value.Container)));
            InPortData.Add(new PortData("lev", "The Level to use for instantiation.", typeof(Value.Container)));

            OutPortData.Add(new PortData("fi", "Family instances created by this operation.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        private Value makeFamilyInstance(object location, FamilySymbol fs, int count, Level level)
        {
            XYZ pos = location is ReferencePoint
               ? (location as ReferencePoint).Position
               : (XYZ)location;

            FamilyInstance fi;

            if (this.Elements.Count > count)
            {
                Element e;
                if (dynUtils.TryGetElement(this.Elements[count],typeof(FamilyInstance), out e))
                {
                    fi = this.UIDocument.Document.GetElement(this.Elements[count]) as FamilyInstance;
                    fi.Symbol = fs;
                    LocationPoint lp = fi.Location as LocationPoint;
                    lp.Point = pos;
                    //fi.Level = level;

                }
                else
                {
                    fi = this.UIDocument.Document.IsFamilyDocument
                       ? this.UIDocument.Document.FamilyCreate.NewFamilyInstance(
                       pos, fs, level, Autodesk.Revit.DB.Structure.StructuralType.NonStructural

                       )
                       : this.UIDocument.Document.Create.NewFamilyInstance(
                          pos, fs, level, Autodesk.Revit.DB.Structure.StructuralType.NonStructural
                       );

                    this.Elements[count] = fi.Id;
                }
            }
            else
            {
                fi = this.UIDocument.Document.IsFamilyDocument
                   ? this.UIDocument.Document.FamilyCreate.NewFamilyInstance(
                      pos, fs, level, Autodesk.Revit.DB.Structure.StructuralType.NonStructural
                   )
                   : this.UIDocument.Document.Create.NewFamilyInstance(
                      pos, fs, level, Autodesk.Revit.DB.Structure.StructuralType.NonStructural
                   );

                this.Elements.Add(fi.Id);
            }

            return Value.NewContainer(fi);
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            FamilySymbol fs = (FamilySymbol)((Value.Container)args[1]).Item;
            var input = args[0];
            Level level = (Level)((Value.Container)args[2]).Item;

            if (input.IsList)
            {
                var locList = (input as Value.List).Item;

                int count = 0;

                var result = Value.NewList(
                   Utils.SequenceToFSharpList(
                      locList.Select(
                         x =>
                            this.makeFamilyInstance(
                               ((Value.Container)x).Item,
                               fs,
                               count++,
                               level
                            )
                      )
                   )
                );

                foreach (var e in this.Elements.Skip(count))
                {
                    this.DeleteElement(e);
                }

                return result;
            }
            else
            {
                var result = this.makeFamilyInstance(
                   ((Value.Container)input).Item,
                   fs,
                   0,
                   level
                );

                foreach (var e in this.Elements.Skip(1))
                {
                    this.DeleteElement(e);
                }

                return result;
            }
        }
    }

    [NodeName("Curves from Fam")]
    [NodeCategory(BuiltinNodeCategories.REVIT_FAMILYCREATION)]
    [NodeDescription("Extracts curves from family instances.")]
    public class dynCurvesFromFamilyInstance : dynRevitTransactionNodeWithOneOutput
    {
        public dynCurvesFromFamilyInstance()
        {
            InPortData.Add(new PortData("fi", "family instance", typeof(Value.Container)));

            OutPortData.Add(new PortData("curves", "Curves extracted by this operation.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        private Value GetCurvesFromFamily(Autodesk.Revit.DB.FamilyInstance fi, int count,
                                   Autodesk.Revit.DB.Options options)
        {
            FamilySymbol fs = fi.Symbol;
            //Autodesk.Revit.DB.GeometryElement geomElem = fs.get_Geometry(options);
            Autodesk.Revit.DB.GeometryElement geomElem = fi.get_Geometry(options); // our particular case of a loaded mass family with no joins has no geom in the instance

            //fi.GetOriginalGeometry(options);
            //fi.GetTransform()

            Autodesk.Revit.DB.CurveArray curves = new CurveArray();
            Autodesk.Revit.DB.ReferenceArray curveRefs = new ReferenceArray();


            //Find all curves and insert them into curve array
            AddCurves(fi, geomElem, count, ref curves);

            //curves.Append(GetCurve(fi, options)); //test 

            //extract references for downstream use
            foreach (Curve c in curves)
            {
                curveRefs.Append(c.Reference);
            }

            //convert curvearray into list using Stephens MakeEnumerable
            Value result = Value.NewList(Utils.SequenceToFSharpList(
                            dynUtils.MakeEnumerable(curves).Select(Value.NewContainer)
                        ));


            return result;

        }

        /// <summary>
        /// Retrieve the first curve found for 
        /// the given element. In case the element is a 
        /// family instance, it may have its own non-empty
        /// solid, in which case we use that. Otherwise we 
        /// search the symbol geometry. If we use the 
        /// symbol geometry, we have to keep track of the 
        /// instance transform to map it to the actual
        /// instance project location.
        /// </summary>
        Curve GetCurve(Element e, Options opt)
        {
            GeometryElement geo = e.get_Geometry(opt);

            Curve curve = null;
            GeometryInstance inst = null;
            Transform t = Transform.Identity;

            // Some columns have no solids, and we have to 
            // retrieve the geometry from the symbol; 
            // others do have solids on the instance itself 
            // and no contents in the instance geometry 
            // (e.g. in rst_basic_sample_project.rvt).

            foreach (GeometryObject obj in geo)
            {
                curve = obj as Curve;

                if (null != curve)
                {
                    break;
                }

                inst = obj as GeometryInstance;
            }

            if (null == curve && null != inst)
            {
                geo = inst.GetSymbolGeometry();
                t = inst.Transform;

                foreach (GeometryObject obj in geo)
                {
                    curve = obj as Curve;

                    if (null != curve)
                    {
                        break;
                    }
                }
            }
            return curve;
        }



        private Value AddCurves(FamilyInstance fi, GeometryElement geomElem, int count, ref CurveArray curves)
        {
            foreach (GeometryObject geomObj in geomElem)
            {
                Curve curve = geomObj as Curve;
                if (null != curve)
                {
                    curves.Append(curve);
                    continue;
                }

                //If this GeometryObject is Instance, call AddCurve
                GeometryInstance geomInst = geomObj as GeometryInstance;
                if (null != geomInst)
                {
                    //curve live in family symbol in this case, need to apply the correct transform to get them in to 
                    //the project coordinate system lining up with the instance
                    // http://wikihelp.autodesk.com/Revit/enu/2012/Help/API_Dev_Guide/0074-Revit_Ge74/0108-Geometry108/0110-Geometry110/GeometryInstances

                    //Autodesk.Revit.DB.GeometryElement transformedGeomElem // curves transformed into project coords
                    //  = geomInst.GetInstanceGeometry(geomInst.Transform);
                    //AddCurves(fi, transformedGeomElem, count, ref curves);

                    GeometryElement transformedGeomElem // curves transformed into project coords
                        = geomInst.GetInstanceGeometry(geomInst.Transform.Inverse);
                    AddCurves(fi, transformedGeomElem, count, ref curves);

                    //Autodesk.Revit.DB.GeometryElement symbolTransformedGeomElem // curves in symbol coords
                    //    = geomInst.GetSymbolGeometry(geomInst.Transform);
                    //AddCurves(fi, symbolTransformedGeomElem, count, ref curves);
                }
            }
            return Value.NewContainer(curves);
        }
        
        public override Value Evaluate(FSharpList<Value> args)
        {
            var input = args[0];

            //create some geometry options so that we compute references
            Autodesk.Revit.DB.Options opts = new Options();
            opts.ComputeReferences = true;
            opts.DetailLevel = ViewDetailLevel.Medium;
            opts.IncludeNonVisibleObjects = false;


            if (input.IsList)
            {
                var familyList = (input as Value.List).Item;
                int count = 0;

                var result = Value.NewList(
                  Utils.SequenceToFSharpList(
                     familyList.Select(
                        x =>
                           this.GetCurvesFromFamily(
                              (FamilyInstance)((Value.Container)x).Item,
                              count++,
                              opts
                           )
                     )
                  )
               );

                foreach (var e in this.Elements.Skip(count))
                {
                    this.DeleteElement(e);
                }

                return result;
            }
            else // single instance passed in
            {
                int count = 0;
                var result = this.GetCurvesFromFamily(
                   (FamilyInstance)((Value.Container)input).Item,
                   count,
                   opts
                );

                foreach (var e in this.Elements.Skip(1)) // cleanup in case of going from list to single instance.
                {
                    this.DeleteElement(e);
                }

                return result;
            }
        }
    }

    //TODO: In Destroy(), have code that resets Elements back to their default.
    [NodeName("Set Family Instance Parameter")]
    [NodeCategory(BuiltinNodeCategories.REVIT_PARAMETERS)]
    [NodeDescription("Modifies a parameter on a family instance.")]
    public class dynFamilyInstanceParameterSetter : dynRevitTransactionNodeWithOneOutput
    {
        public dynFamilyInstanceParameterSetter()
        {
            InPortData.Add(new PortData("fi", "Family instance.", typeof(Value.Container)));
            InPortData.Add(new PortData("param", "Parameter to modify (string).", typeof(Value.String)));
            InPortData.Add(new PortData("value", "Value to set the parameter to.", typeof(object)));
            OutPortData.Add(new PortData("fi", "Modified family instance.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        private static Value setParam(FamilyInstance fi, string paramName, Value valueExpr)
        {
            var p = fi.get_Parameter(paramName);
            if (p != null)
            {
                return _setParam(fi, p, valueExpr);
            }
            throw new Exception("Parameter \"" + paramName + "\" was not found!");
        }

        private static Value setParam(FamilyInstance fi, Definition paramDef, Value valueExpr)
        {
            var p = fi.get_Parameter(paramDef);
            if (p != null)
            {
                return _setParam(fi, p, valueExpr);
            }
            throw new Exception("Parameter \"" + paramDef.Name + "\" was not found!");
        }

        private static Value _setParam(FamilyInstance ft, Parameter p, Value valueExpr)
        {
            if (p.StorageType == StorageType.Double)
            {
                p.Set(((Value.Number)valueExpr).Item);
            }
            else if (p.StorageType == StorageType.Integer)
            {
                p.Set((int)((Value.Number)valueExpr).Item);
            }
            else if (p.StorageType == StorageType.String)
            {
                p.Set(((Value.String)valueExpr).Item);
            }
            else if (valueExpr.IsNumber)
            {
                p.Set(new ElementId((int)(valueExpr as Value.Number).Item));
            }
            else
            {
                p.Set((ElementId)((Value.Container)valueExpr).Item);
            }
            return Value.NewContainer(ft);
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var valueExpr = args[2];

            var param = args[1];
            if (param.IsString)
            {
                var paramName = ((Value.String)param).Item;

                var input = args[0];
                if (input.IsList)
                {
                    var fiList = (input as Value.List).Item;
                    return Value.NewList(
                       Utils.SequenceToFSharpList(
                          fiList.Select(
                             x =>
                                setParam(
                                   (FamilyInstance)((Value.Container)x).Item,
                                   paramName,
                                   valueExpr
                                )
                          )
                       )
                    );
                }
                else
                {
                    var fs = (FamilyInstance)((Value.Container)input).Item;

                    return setParam(fs, paramName, valueExpr);
                }
            }
            else
            {
                var paramDef = (Definition)((Value.Container)param).Item;

                var input = args[0];
                if (input.IsList)
                {
                    var fiList = (input as Value.List).Item;
                    return Value.NewList(
                       Utils.SequenceToFSharpList(
                          fiList.Select(
                             x =>
                                setParam(
                                   (FamilyInstance)((Value.Container)x).Item,
                                   paramDef,
                                   valueExpr
                                )
                          )
                       )
                    );
                }
                else
                {
                    var fs = (FamilyInstance)((Value.Container)input).Item;

                    return setParam(fs, paramDef, valueExpr);
                }
            }
        }
    }

    [NodeName("Get Family Instance Parameter")]
    [NodeCategory(BuiltinNodeCategories.REVIT_PARAMETERS)]
    [NodeDescription("Fetches the value of a parameter of a Family Instance.")]
    public class dynFamilyInstanceParameterGetter : dynRevitTransactionNodeWithOneOutput
    {
        public dynFamilyInstanceParameterGetter()
        {
            InPortData.Add(new PortData("fi", "Family instance.", typeof(Value.Container)));
            InPortData.Add(new PortData("param", "Parameter to fetch.", typeof(Value.String)));

            OutPortData.Add(new PortData("val", "Parameter value.", typeof(object)));

            RegisterAllPorts();
        }

        private static Value getParam(FamilyInstance fi, string paramName)
        {
            var p = fi.get_Parameter(paramName);
            if (p != null)
            {
                return _getParam(fi, p);
            }
            throw new Exception("Parameter \"" + paramName + "\" was not found!");
        }

        private static Value getParam(FamilyInstance fi, Definition paramDef)
        {
            var p = fi.get_Parameter(paramDef);
            if (p != null)
            {
                return _getParam(fi, p);
            }
            throw new Exception("Parameter \"" + paramDef.Name + "\" was not found!");
        }

        private static Value _getParam(FamilyInstance fi, Parameter p)
        {
            if (p.StorageType == StorageType.Double)
            {
                return Value.NewNumber(p.AsDouble());
            }
            else if (p.StorageType == StorageType.Integer)
            {
                return Value.NewNumber(p.AsInteger());
            }
            else if (p.StorageType == StorageType.String)
            {
                return Value.NewString(p.AsString());
            }
            else
            {
                return Value.NewContainer(p.AsElementId());
            }
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var param = args[1];
            if (param.IsString)
            {
                var paramName = ((Value.String)param).Item;

                var input = args[0];
                if (input.IsList)
                {
                    var fiList = (input as Value.List).Item;
                    return Value.NewList(
                       Utils.SequenceToFSharpList(
                          fiList.Select(
                             x =>
                                getParam(
                                   (FamilyInstance)((Value.Container)x).Item,
                                   paramName
                                )
                          )
                       )
                    );
                }
                else
                {
                    var fi = (FamilyInstance)((Value.Container)input).Item;

                    return getParam(fi, paramName);
                }
            }
            else
            {
                var paramDef = (Definition)((Value.Container)param).Item;

                var input = args[0];
                if (input.IsList)
                {
                    var fiList = (input as Value.List).Item;
                    return Value.NewList(
                       Utils.SequenceToFSharpList(
                          fiList.Select(
                             x =>
                                getParam(
                                   (FamilyInstance)((Value.Container)x).Item,
                                   paramDef
                                )
                          )
                       )
                    );
                }
                else
                {
                    var fi = (FamilyInstance)((Value.Container)input).Item;

                    return getParam(fi, paramDef);
                }
            }
        }
    }

    [NodeName("Set Family Type Parameter")]
    [NodeCategory(BuiltinNodeCategories.REVIT_PARAMETERS)]
    [NodeDescription("Modifies a parameter on a family type.")]
    public class dynFamilyTypeParameterSetter : dynRevitTransactionNodeWithOneOutput
    {
        public dynFamilyTypeParameterSetter()
        {
            InPortData.Add(new PortData("ft", "Family type.", typeof(Value.Container)));
            InPortData.Add(new PortData("param", "Parameter to modify.", typeof(Value.String)));
            InPortData.Add(new PortData("value", "Value to set the parameter to.", typeof(object)));
            OutPortData.Add(new PortData("ft", "Modified family type.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        private static Value setParam(FamilySymbol fi, string paramName, Value valueExpr)
        {
            var p = fi.get_Parameter(paramName);
            if (p != null)
            {
                return _setParam(fi, p, valueExpr);
            }
            throw new Exception("Parameter \"" + paramName + "\" was not found!");
        }

        private static Value setParam(FamilySymbol fi, Definition paramDef, Value valueExpr)
        {
            var p = fi.get_Parameter(paramDef);
            if (p != null)
            {
                return _setParam(fi, p, valueExpr);
            }
            throw new Exception("Parameter \"" + paramDef.Name + "\" was not found!");
        }

        private static Value _setParam(FamilySymbol ft, Parameter p, Value valueExpr)
        {
            if (p.StorageType == StorageType.Double)
            {
                p.Set(((Value.Number)valueExpr).Item);
            }
            else if (p.StorageType == StorageType.Integer)
            {
                p.Set((int)((Value.Number)valueExpr).Item);
            }
            else if (p.StorageType == StorageType.String)
            {
                p.Set(((Value.String)valueExpr).Item);
            }
            else if (valueExpr.IsNumber)
            {
                p.Set(new ElementId((int)(valueExpr as Value.Number).Item));
            }
            else
            {
                p.Set((ElementId)((Value.Container)valueExpr).Item);
            }
            return Value.NewContainer(ft);
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var valueExpr = args[2];

            var param = args[1];
            if (param.IsString)
            {
                var paramName = ((Value.String)param).Item;

                var input = args[0];
                if (input.IsList)
                {
                    var fiList = (input as Value.List).Item;
                    return Value.NewList(
                       Utils.SequenceToFSharpList(
                          fiList.Select(
                             x =>
                                setParam(
                                   (FamilySymbol)((Value.Container)x).Item,
                                   paramName,
                                   valueExpr
                                )
                          )
                       )
                    );
                }
                else
                {
                    var fs = (FamilySymbol)((Value.Container)input).Item;

                    return setParam(fs, paramName, valueExpr);
                }
            }
            else
            {
                var paramDef = (Definition)((Value.Container)param).Item;

                var input = args[0];
                if (input.IsList)
                {
                    var fiList = (input as Value.List).Item;
                    return Value.NewList(
                       Utils.SequenceToFSharpList(
                          fiList.Select(
                             x =>
                                setParam(
                                   (FamilySymbol)((Value.Container)x).Item,
                                   paramDef,
                                   valueExpr
                                )
                          )
                       )
                    );
                }
                else
                {
                    var fs = (FamilySymbol)((Value.Container)input).Item;

                    return setParam(fs, paramDef, valueExpr);
                }
            }
        }
    }

    [NodeName("Get Family Type Parameter")]
    [NodeCategory(BuiltinNodeCategories.REVIT_PARAMETERS)]
    [NodeDescription("Fetches the value of a parameter of a Family Type.")]
    public class dynFamilyTypeParameterGetter : dynRevitTransactionNodeWithOneOutput
    {
        public dynFamilyTypeParameterGetter()
        {
            InPortData.Add(new PortData("ft", "Family type.", typeof(Value.Container)));
            InPortData.Add(new PortData("param", "Parameter to fetch (string).", typeof(Value.String)));

            OutPortData.Add(new PortData("val", "Parameter value.", typeof(object)));

            RegisterAllPorts();
        }

        private static Value getParam(FamilySymbol fi, string paramName)
        {
            var p = fi.get_Parameter(paramName);
            if (p != null)
            {
                return _getParam(fi, p);
            }
            throw new Exception("Parameter \"" + paramName + "\" was not found!");
        }

        private static Value getParam(FamilySymbol fi, Definition paramDef)
        {
            var p = fi.get_Parameter(paramDef);
            if (p != null)
            {
                return _getParam(fi, p);
            }
            throw new Exception("Parameter \"" + paramDef.Name + "\" was not found!");
        }

        private static Value _getParam(FamilySymbol fi, Parameter p)
        {
            if (p.StorageType == StorageType.Double)
            {
                return Value.NewNumber(p.AsDouble());
            }
            else if (p.StorageType == StorageType.Integer)
            {
                return Value.NewNumber(p.AsInteger());
            }
            else if (p.StorageType == StorageType.String)
            {
                return Value.NewString(p.AsString());
            }
            else
            {
                return Value.NewContainer(p.AsElementId());
            }
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var param = args[1];
            if (param.IsString)
            {
                var paramName = ((Value.String)param).Item;

                var input = args[0];
                if (input.IsList)
                {
                    var fiList = (input as Value.List).Item;
                    return Value.NewList(
                       Utils.SequenceToFSharpList(
                          fiList.Select(
                             x =>
                                getParam(
                                   (FamilySymbol)((Value.Container)x).Item,
                                   paramName
                                )
                          )
                       )
                    );
                }
                else
                {
                    var fi = (FamilySymbol)((Value.Container)input).Item;

                    return getParam(fi, paramName);
                }
            }
            else
            {
                var paramDef = (Definition)((Value.Container)param).Item;

                var input = args[0];
                if (input.IsList)
                {
                    var fiList = (input as Value.List).Item;
                    return Value.NewList(
                       Utils.SequenceToFSharpList(
                          fiList.Select(
                             x =>
                                getParam(
                                   (FamilySymbol)((Value.Container)x).Item,
                                   paramDef
                                )
                          )
                       )
                    );
                }
                else
                {
                    var fi = (FamilySymbol)((Value.Container)input).Item;

                    return getParam(fi, paramDef);
                }
            }
        }
    }
}


