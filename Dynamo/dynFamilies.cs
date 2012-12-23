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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Xml;

using Autodesk.Revit.DB;

using Dynamo.Connectors;
using Dynamo.FSchemeInterop;
using Dynamo.Utilities;

using Microsoft.FSharp.Collections;

using Expression = Dynamo.FScheme.Expression;

namespace Dynamo.Elements
{
    [ElementName("Family Type Selector")]
    [ElementCategory(BuiltinElementCategories.REVIT)]
    [ElementDescription("Select a Family Type from a drop down list.")]
    [RequiresTransaction(false)]
    [IsInteractive(true)]
    public class dynFamilyTypeSelector : dynNode
    {
        ComboBox combo;
        Dictionary<string, FamilySymbol> comboHash = new Dictionary<string, FamilySymbol>();

        public dynFamilyTypeSelector()
        {
            //widen the control
            this.topControl.Width = 300;

            //add a drop down list to the window
            combo = new ComboBox();
            combo.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            combo.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            this.inputGrid.Children.Add(combo);
            System.Windows.Controls.Grid.SetColumn(combo, 0);
            System.Windows.Controls.Grid.SetRow(combo, 0);

            combo.DropDownOpened += new EventHandler(combo_DropDownOpened);
            combo.SelectionChanged += delegate
            {
                if (combo.SelectedIndex != -1)
                    this.IsDirty = true;
            };

            PopulateComboBox();

            OutPortData = new PortData("", "Family type", typeof(FamilySymbol));
            base.RegisterInputsAndOutputs();
        }

        void combo_DropDownOpened(object sender, EventArgs e)
        {
            PopulateComboBox();
        }

        private void PopulateComboBox()
        {
            comboHash.Clear();
            combo.Items.Clear();

            //load all the currently loaded types into the combo list
            FilteredElementCollector fec = new FilteredElementCollector(this.UIDocument.Document);
            fec.OfClass(typeof(Family));
            foreach (Family f in fec.ToElements())
            {
                foreach (FamilySymbol fs in f.Symbols)
                {
                    ComboBoxItem cbi = new ComboBoxItem();
                    string comboText = f.Name + ":" + fs.Name;
                    cbi.Content = comboText;
                    combo.Items.Add(cbi);
                    comboHash[comboText] = fs;
                }
            }
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            ComboBoxItem cbi = combo.SelectedItem as ComboBoxItem;

            if (cbi != null)
            {
                var f = comboHash[cbi.Content as string];
                return Expression.NewContainer(f);
            }

            throw new Exception("Nothing selected!");
        }

        public override void SaveElement(XmlDocument xmlDoc, XmlElement dynEl)
        {
            dynEl.SetAttribute("index", this.combo.SelectedIndex.ToString());
        }

        public override void LoadElement(XmlNode elNode)
        {
            try
            {
                combo.SelectedIndex = Convert.ToInt32(elNode.Attributes["index"].Value);
            }
            catch { }
        }
    }

    [ElementName("Family Parameter Selector")]
    [ElementCategory(BuiltinElementCategories.REVIT)]
    [ElementDescription("Given a Family Instance or Symbol, allows the user to select a paramter as a string.")]
    [RequiresTransaction(false)]
    [IsInteractive(true)]
    public class dynFamilyInstanceParameterSelector : dynNode
    {
        ComboBox paramBox = new ComboBox();
        ElementId storedId = null;
        Definition value;
        List<Definition> values = new List<Definition>();

        public dynFamilyInstanceParameterSelector()
        {
            //widen the control
            this.topControl.Height = 175;

            //add a drop down list to the window
            paramBox.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            paramBox.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            this.inputGrid.Children.Add(paramBox);
            System.Windows.Controls.Grid.SetColumn(paramBox, 0);
            System.Windows.Controls.Grid.SetRow(paramBox, 0);

            paramBox.SelectionChanged += delegate
            {
                if (paramBox.SelectedIndex != -1)
                {
                    this.value = this.values[this.paramBox.SelectedIndex];
                    this.IsDirty = true;
                }
            };

            paramBox.IsEnabled = false;

            InPortData.Add(new PortData("f", "Family Symbol or Instance", typeof(Element)));
            OutPortData = new PortData("", "Parameter Name", typeof(string));
            base.RegisterInputsAndOutputs();
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

        private void PopulateComboBox(IEnumerable set, bool readOnly)
        {
            this.values.Clear();

            SortedList<string, dynamic> paramList = new SortedList<string, dynamic>();

            foreach (dynamic p in set)
            {
                if ((readOnly && p.IsReadOnly) || p.StorageType == StorageType.None)
                    continue;

                var val = p.Definition.Name + " (" + getStorageTypeString(p.StorageType) + ")";
                paramList.Add(val, p);
            }

            foreach (dynamic p in paramList.Values)
            {
                this.values.Add(p.Definition);
            }

            this.paramBox.Dispatcher.Invoke(new Action(
               delegate
               {
                   this.paramBox.IsEnabled = true;
                   this.paramBox.Items.Clear();
                   foreach (string val in paramList.Keys)
                   {
                       this.paramBox.Items.Add(val);
                   }
               }
            ));
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            var input = (Element)((Expression.Container)args[0]).Item;

            if (!input.Id.Equals(this.storedId))
            {
                this.storedId = input.Id;
                if (input is FamilySymbol)
                {
                    var paramDict = new Dictionary<Definition, dynamic>();

                    var fs = input as FamilySymbol;

                    foreach (dynamic p in fs.Parameters)
                        paramDict[p.Definition] = p;

                    var fd = this.UIDocument.Document.EditFamily(fs.Family);
                    var ps = fd.FamilyManager.Parameters;

                    foreach (dynamic p in ps)
                        paramDict[p.Definition] = p;

                    //this.PopulateComboBox(fs.Parameters, false);
                    this.PopulateComboBox(paramDict.Values, false);
                }
                else
                {
                    var fi = (FamilyInstance)input;
                    this.PopulateComboBox(fi.Parameters, true);
                }
            }

            return Expression.NewContainer(this.value);
        }

        public override void SaveElement(XmlDocument xmlDoc, XmlElement dynEl)
        {
            XmlElement outEl = xmlDoc.CreateElement("familyid");
            outEl.SetAttribute("value", this.storedId.IntegerValue.ToString());
            dynEl.AppendChild(outEl);

            XmlElement param = xmlDoc.CreateElement("index");
            param.SetAttribute("value", this.paramBox.SelectedIndex.ToString());
            dynEl.AppendChild(param);
        }

        public override void LoadElement(XmlNode elNode)
        {
            int selection = -1;
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
                    Element e = this.UIDocument.Document.GetElement(this.storedId);
                    if (e is FamilySymbol)
                    {
                        var paramDict = new Dictionary<string, dynamic>();

                        var fs = e as FamilySymbol;

                        foreach (dynamic p in fs.Parameters)
                            paramDict[p.Definition.Name] = p;

                        var fd = this.UIDocument.Document.EditFamily(fs.Family);
                        var ps = fd.FamilyManager.Parameters;

                        foreach (dynamic p in ps)
                            paramDict[p.Definition.Name] = p;

                        //this.PopulateComboBox(fs.Parameters, false);
                        this.PopulateComboBox(paramDict.Values, false);
                    }
                    else if (e is FamilyInstance)
                    {
                        this.PopulateComboBox((e as FamilyInstance).Parameters, true);
                    }
                    else
                    {
                        this.storedId = null;
                        continue;
                    }
                }
                else if (subNode.Name.Equals("index"))
                {
                    try
                    {
                        selection = Convert.ToInt32(subNode.Attributes[0].Value);
                    }
                    catch { }
                }
            }
            if (this.storedId != null)
                this.paramBox.SelectedIndex = selection;
        }
    }

    #region Disabled ParameterMapper

    //[ElementName("Instance Parameter Mapper")]
    //[ElementCategory(BuiltinElementCategories.REVIT)]
    //[ElementDescription("Maps the parameters of a Family Type.")]
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
    //      OutPortData = new PortData(null, "", "A map of parameter values on the instance.", typeof(dynInstanceParameterMapper)));
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

    //      base.RegisterInputsAndOutputs();

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
    //            c.Kill();
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

    //[ElementName("Family Instance Parameter Evaluation")]
    //[ElementCategory(BuiltinElementCategories.REVIT)]
    //[ElementDescription("Modifies parameters on family instances.")]
    //[RequiresTransaction(true)]
    //public class dynFamilyInstanceParameterEvaluation : dynElement
    //{

    //   public dynFamilyInstanceParameterEvaluation()
    //   {

    //      InPortData.Add(new PortData(null, "fi", "Family instances.", typeof(dynElement)));
    //      InPortData.Add(new PortData(null, "map", "Parameter map.", typeof(dynInstanceParameterMapper)));

    //      base.RegisterInputsAndOutputs();

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

    [ElementName("Family Instance Creator")]
    [ElementCategory(BuiltinElementCategories.REVIT)]
    [ElementDescription("Creates family instances at a given XYZ location.")]
    [RequiresTransaction(true)]
    public class dynFamilyInstanceCreatorXYZ : dynNode
    {
        public dynFamilyInstanceCreatorXYZ()
        {
            InPortData.Add(new PortData("xyz", "xyz", typeof(object)));
            InPortData.Add(new PortData("type", "The Family Symbol to use for instantiation.", typeof(FamilySymbol)));

            OutPortData = new PortData("fi", "Family instances created by this operation.", typeof(FamilyInstance));

            base.RegisterInputsAndOutputs();
        }

        private Expression makeFamilyInstance(object location, FamilySymbol fs, int count)
        {
            XYZ pos = location is ReferencePoint
               ? (location as ReferencePoint).Position
               : (XYZ)location;

            FamilyInstance fi;

            if (this.Elements.Count > count)
            {
                Element e;
                if (dynUtils.TryGetElement(this.Elements[count], out e))
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

            return Expression.NewContainer(fi);
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            FamilySymbol fs = (FamilySymbol)((Expression.Container)args[1]).Item;
            var input = args[0];

            if (input.IsList)
            {
                var locList = (input as Expression.List).Item;

                int count = 0;

                var result = Expression.NewList(
                   Utils.convertSequence(
                      locList.Select(
                         x =>
                            this.makeFamilyInstance(
                               ((Expression.Container)x).Item,
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
                   ((Expression.Container)input).Item,
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

    [ElementName("Family Instance By Level Creator")]
    [ElementCategory(BuiltinElementCategories.REVIT)]
    [ElementDescription("An element which allows you to create family instances.")]
    [RequiresTransaction(true)]
    public class dynFamilyInstanceCreatorLevel : dynNode
    {
        public dynFamilyInstanceCreatorLevel()
        {
            InPortData.Add(new PortData("xyz", "xyz", typeof(object)));
            InPortData.Add(new PortData("typ", "The Family Symbol to use for instantiation.", typeof(FamilySymbol)));
            InPortData.Add(new PortData("lev", "The Level to use for instantiation.", typeof(FamilySymbol)));

            OutPortData = new PortData("fi", "Family instances created by this operation.", typeof(FamilyInstance));

            base.RegisterInputsAndOutputs();
        }

        private Expression makeFamilyInstance(object location, FamilySymbol fs, int count, Level level)
        {
            XYZ pos = location is ReferencePoint
               ? (location as ReferencePoint).Position
               : (XYZ)location;

            FamilyInstance fi;

            if (this.Elements.Count > count)
            {
                Element e;
                if (dynUtils.TryGetElement(this.Elements[count], out e))
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

            return Expression.NewContainer(fi);
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            FamilySymbol fs = (FamilySymbol)((Expression.Container)args[1]).Item;
            var input = args[0];
            Level level = (Level)((Expression.Container)args[2]).Item;

            if (input.IsList)
            {
                var locList = (input as Expression.List).Item;

                int count = 0;

                var result = Expression.NewList(
                   Utils.convertSequence(
                      locList.Select(
                         x =>
                            this.makeFamilyInstance(
                               ((Expression.Container)x).Item,
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
                   ((Expression.Container)input).Item,
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

    [ElementName("Curves from Family")]
    [ElementCategory(BuiltinElementCategories.REVIT)]
    [ElementDescription("An element which allows you to extract curves from family instances.")]
    [RequiresTransaction(true)]
    public class dynCurvesFromFamilyInstance : dynNode
    {
        public dynCurvesFromFamilyInstance()
        {
            InPortData.Add(new PortData("fi", "family instance", typeof(object)));


            OutPortData = new PortData("curves", "Curves extracted by this operation.", typeof(Curve));

            base.RegisterInputsAndOutputs();
        }

        private Expression GetCurvesFromFamily(Autodesk.Revit.DB.FamilyInstance fi, int count,
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
            Expression result = Expression.NewList(Utils.convertSequence(
                            dynUtils.MakeEnumerable(curves).Select(Expression.NewContainer)
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



        private Expression AddCurves(FamilyInstance fi, GeometryElement geomElem, int count, ref CurveArray curves)
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
            return Expression.NewContainer(curves);
        }

        /*
        public void GetAndTransformCurve(Autodesk.Revit.ApplicationServices.Application app,
    Autodesk.Revit.DB.Element element, Options geoOptions)
        {
            // Get geometry element of the selected element
            Autodesk.Revit.DB.GeometryElement geoElement = element.get_Geometry(geoOptions);

            // Get geometry object
            foreach (GeometryObject geoObject in geoElement.Objects)
            {
                // Get the geometry instance which contains the geometry information
                Autodesk.Revit.DB.GeometryInstance instance =
                    geoObject as Autodesk.Revit.DB.GeometryInstance;
                if (null != instance)
                {
                    foreach (GeometryObject o in instance.SymbolGeometry.Objects)
                    {
                        // Get curve
                        Curve curve = o as Curve;
                        if (curve != null)
                        {
                            // transfrom the curve to make it in the instance's coordinate space
                            curve = curve.get_Transformed(instance.Transform);
                        }
                    }
                }
            }
        }*/


        public override Expression Evaluate(FSharpList<Expression> args)
        {
            var input = args[0];

            //create some geometry options so that we compute references
            Autodesk.Revit.DB.Options opts = new Options();
            opts.ComputeReferences = true;
            opts.DetailLevel = ViewDetailLevel.Medium;
            opts.IncludeNonVisibleObjects = false;


            if (input.IsList)
            {
                var familyList = (input as Expression.List).Item;
                int count = 0;

                var result = Expression.NewList(
                  Utils.convertSequence(
                     familyList.Select(
                        x =>
                           this.GetCurvesFromFamily(
                              (FamilyInstance)((Expression.Container)x).Item,
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
                   (FamilyInstance)((Expression.Container)input).Item,
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
    [ElementName("Set Instance Parameter")]
    [ElementCategory(BuiltinElementCategories.REVIT)]
    [ElementDescription("Modifies a parameter on a family instance.")]
    [RequiresTransaction(true)]
    public class dynFamilyInstanceParameterSetter : dynNode
    {
        public dynFamilyInstanceParameterSetter()
        {
            InPortData.Add(new PortData("fi", "Family instance.", typeof(object)));
            InPortData.Add(new PortData("param", "Parameter to modify (string).", typeof(object)));
            InPortData.Add(new PortData("value", "Value to set the parameter to.", typeof(object)));

            OutPortData = new PortData("fi", "Modified family instance.", typeof(object));

            base.RegisterInputsAndOutputs();
        }

        private static Expression setParam(FamilyInstance fi, string paramName, Expression valueExpr)
        {
            var p = fi.get_Parameter(paramName);
            if (p != null)
            {
                return _setParam(fi, p, valueExpr);
            }
            throw new Exception("Parameter \"" + paramName + "\" was not found!");
        }

        private static Expression setParam(FamilyInstance fi, Definition paramDef, Expression valueExpr)
        {
            var p = fi.get_Parameter(paramDef);
            if (p != null)
            {
                return _setParam(fi, p, valueExpr);
            }
            throw new Exception("Parameter \"" + paramDef.Name + "\" was not found!");
        }

        private static Expression _setParam(FamilyInstance ft, Parameter p, Expression valueExpr)
        {
            if (p.StorageType == StorageType.Double)
            {
                p.Set(((Expression.Number)valueExpr).Item);
            }
            else if (p.StorageType == StorageType.Integer)
            {
                p.Set((int)((Expression.Number)valueExpr).Item);
            }
            else if (p.StorageType == StorageType.String)
            {
                p.Set(((Expression.String)valueExpr).Item);
            }
            else if (valueExpr.IsNumber)
            {
                p.Set(new ElementId((int)(valueExpr as Expression.Number).Item));
            }
            else
            {
                p.Set((ElementId)((Expression.Container)valueExpr).Item);
            }
            return Expression.NewContainer(ft);
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            var valueExpr = args[2];

            var param = args[1];
            if (param.IsString)
            {
                var paramName = ((Expression.String)param).Item;

                var input = args[0];
                if (input.IsList)
                {
                    var fiList = (input as Expression.List).Item;
                    return Expression.NewList(
                       Utils.convertSequence(
                          fiList.Select(
                             x =>
                                setParam(
                                   (FamilyInstance)((Expression.Container)x).Item,
                                   paramName,
                                   valueExpr
                                )
                          )
                       )
                    );
                }
                else
                {
                    var fs = (FamilyInstance)((Expression.Container)input).Item;

                    return setParam(fs, paramName, valueExpr);
                }
            }
            else
            {
                var paramDef = (Definition)((Expression.Container)param).Item;

                var input = args[0];
                if (input.IsList)
                {
                    var fiList = (input as Expression.List).Item;
                    return Expression.NewList(
                       Utils.convertSequence(
                          fiList.Select(
                             x =>
                                setParam(
                                   (FamilyInstance)((Expression.Container)x).Item,
                                   paramDef,
                                   valueExpr
                                )
                          )
                       )
                    );
                }
                else
                {
                    var fs = (FamilyInstance)((Expression.Container)input).Item;

                    return setParam(fs, paramDef, valueExpr);
                }
            }
        }
    }

    [ElementName("Get Instance Parameter")]
    [ElementCategory(BuiltinElementCategories.REVIT)]
    [ElementDescription("Fetches the value of a parameter of a Family Instance.")]
    [RequiresTransaction(true)]
    public class dynFamilyInstanceParameterGetter : dynNode
    {
        public dynFamilyInstanceParameterGetter()
        {
            InPortData.Add(new PortData("fi", "Family instance.", typeof(FamilyInstance)));
            InPortData.Add(new PortData("param", "Parameter to fetch.", typeof(string)));

            OutPortData = new PortData("val", "Parameter value.", typeof(object));

            base.RegisterInputsAndOutputs();
        }

        private static Expression getParam(FamilyInstance fi, string paramName)
        {
            var p = fi.get_Parameter(paramName);
            if (p != null)
            {
                return _getParam(fi, p);
            }
            throw new Exception("Parameter \"" + paramName + "\" was not found!");
        }

        private static Expression getParam(FamilyInstance fi, Definition paramDef)
        {
            var p = fi.get_Parameter(paramDef);
            if (p != null)
            {
                return _getParam(fi, p);
            }
            throw new Exception("Parameter \"" + paramDef.Name + "\" was not found!");
        }

        private static Expression _getParam(FamilyInstance fi, Parameter p)
        {
            if (p.StorageType == StorageType.Double)
            {
                return Expression.NewNumber(p.AsDouble());
            }
            else if (p.StorageType == StorageType.Integer)
            {
                return Expression.NewNumber(p.AsInteger());
            }
            else if (p.StorageType == StorageType.String)
            {
                return Expression.NewString(p.AsString());
            }
            else
            {
                return Expression.NewContainer(p.AsElementId());
            }
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            var param = args[1];
            if (param.IsString)
            {
                var paramName = ((Expression.String)param).Item;

                var input = args[0];
                if (input.IsList)
                {
                    var fiList = (input as Expression.List).Item;
                    return Expression.NewList(
                       Utils.convertSequence(
                          fiList.Select(
                             x =>
                                getParam(
                                   (FamilyInstance)((Expression.Container)x).Item,
                                   paramName
                                )
                          )
                       )
                    );
                }
                else
                {
                    var fi = (FamilyInstance)((Expression.Container)input).Item;

                    return getParam(fi, paramName);
                }
            }
            else
            {
                var paramDef = (Definition)((Expression.Container)param).Item;

                var input = args[0];
                if (input.IsList)
                {
                    var fiList = (input as Expression.List).Item;
                    return Expression.NewList(
                       Utils.convertSequence(
                          fiList.Select(
                             x =>
                                getParam(
                                   (FamilyInstance)((Expression.Container)x).Item,
                                   paramDef
                                )
                          )
                       )
                    );
                }
                else
                {
                    var fi = (FamilyInstance)((Expression.Container)input).Item;

                    return getParam(fi, paramDef);
                }
            }
        }
    }


    [ElementName("Set Type Parameter")]
    [ElementCategory(BuiltinElementCategories.REVIT)]
    [ElementDescription("Modifies a parameter on a family type.")]
    [RequiresTransaction(true)]
    public class dynFamilyTypeParameterSetter : dynNode
    {
        public dynFamilyTypeParameterSetter()
        {
            InPortData.Add(new PortData("ft", "Family type.", typeof(object)));
            InPortData.Add(new PortData("param", "Parameter to modify.", typeof(object)));
            InPortData.Add(new PortData("value", "Value to set the parameter to.", typeof(object)));

            OutPortData = new PortData("ft", "Modified family type.", typeof(object));

            base.RegisterInputsAndOutputs();
        }

        private static Expression setParam(FamilySymbol fi, string paramName, Expression valueExpr)
        {
            var p = fi.get_Parameter(paramName);
            if (p != null)
            {
                return _setParam(fi, p, valueExpr);
            }
            throw new Exception("Parameter \"" + paramName + "\" was not found!");
        }

        private static Expression setParam(FamilySymbol fi, Definition paramDef, Expression valueExpr)
        {
            var p = fi.get_Parameter(paramDef);
            if (p != null)
            {
                return _setParam(fi, p, valueExpr);
            }
            throw new Exception("Parameter \"" + paramDef.Name + "\" was not found!");
        }

        private static Expression _setParam(FamilySymbol ft, Parameter p, Expression valueExpr)
        {
            if (p.StorageType == StorageType.Double)
            {
                p.Set(((Expression.Number)valueExpr).Item);
            }
            else if (p.StorageType == StorageType.Integer)
            {
                p.Set((int)((Expression.Number)valueExpr).Item);
            }
            else if (p.StorageType == StorageType.String)
            {
                p.Set(((Expression.String)valueExpr).Item);
            }
            else if (valueExpr.IsNumber)
            {
                p.Set(new ElementId((int)(valueExpr as Expression.Number).Item));
            }
            else
            {
                p.Set((ElementId)((Expression.Container)valueExpr).Item);
            }
            return Expression.NewContainer(ft);
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            var valueExpr = args[2];

            var param = args[1];
            if (param.IsString)
            {
                var paramName = ((Expression.String)param).Item;

                var input = args[0];
                if (input.IsList)
                {
                    var fiList = (input as Expression.List).Item;
                    return Expression.NewList(
                       Utils.convertSequence(
                          fiList.Select(
                             x =>
                                setParam(
                                   (FamilySymbol)((Expression.Container)x).Item,
                                   paramName,
                                   valueExpr
                                )
                          )
                       )
                    );
                }
                else
                {
                    var fs = (FamilySymbol)((Expression.Container)input).Item;

                    return setParam(fs, paramName, valueExpr);
                }
            }
            else
            {
                var paramDef = (Definition)((Expression.Container)param).Item;

                var input = args[0];
                if (input.IsList)
                {
                    var fiList = (input as Expression.List).Item;
                    return Expression.NewList(
                       Utils.convertSequence(
                          fiList.Select(
                             x =>
                                setParam(
                                   (FamilySymbol)((Expression.Container)x).Item,
                                   paramDef,
                                   valueExpr
                                )
                          )
                       )
                    );
                }
                else
                {
                    var fs = (FamilySymbol)((Expression.Container)input).Item;

                    return setParam(fs, paramDef, valueExpr);
                }
            }
        }
    }

    [ElementName("Get Type Parameter")]
    [ElementCategory(BuiltinElementCategories.REVIT)]
    [ElementDescription("Fetches the value of a parameter of a Family Type.")]
    [RequiresTransaction(true)]
    public class dynFamilyTypeParameterGetter : dynNode
    {
        public dynFamilyTypeParameterGetter()
        {
            InPortData.Add(new PortData("ft", "Family type.", typeof(FamilySymbol)));
            InPortData.Add(new PortData("param", "Parameter to fetch (string).", typeof(string)));

            OutPortData = new PortData("val", "Parameter value.", typeof(object));

            base.RegisterInputsAndOutputs();
        }

        private static Expression getParam(FamilySymbol fi, string paramName)
        {
            var p = fi.get_Parameter(paramName);
            if (p != null)
            {
                return _getParam(fi, p);
            }
            throw new Exception("Parameter \"" + paramName + "\" was not found!");
        }

        private static Expression getParam(FamilySymbol fi, Definition paramDef)
        {
            var p = fi.get_Parameter(paramDef);
            if (p != null)
            {
                return _getParam(fi, p);
            }
            throw new Exception("Parameter \"" + paramDef.Name + "\" was not found!");
        }

        private static Expression _getParam(FamilySymbol fi, Parameter p)
        {
            if (p.StorageType == StorageType.Double)
            {
                return Expression.NewNumber(p.AsDouble());
            }
            else if (p.StorageType == StorageType.Integer)
            {
                return Expression.NewNumber(p.AsInteger());
            }
            else if (p.StorageType == StorageType.String)
            {
                return Expression.NewString(p.AsString());
            }
            else
            {
                return Expression.NewContainer(p.AsElementId());
            }
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            var param = args[1];
            if (param.IsString)
            {
                var paramName = ((Expression.String)param).Item;

                var input = args[0];
                if (input.IsList)
                {
                    var fiList = (input as Expression.List).Item;
                    return Expression.NewList(
                       Utils.convertSequence(
                          fiList.Select(
                             x =>
                                getParam(
                                   (FamilySymbol)((Expression.Container)x).Item,
                                   paramName
                                )
                          )
                       )
                    );
                }
                else
                {
                    var fi = (FamilySymbol)((Expression.Container)input).Item;

                    return getParam(fi, paramName);
                }
            }
            else
            {
                var paramDef = (Definition)((Expression.Container)param).Item;

                var input = args[0];
                if (input.IsList)
                {
                    var fiList = (input as Expression.List).Item;
                    return Expression.NewList(
                       Utils.convertSequence(
                          fiList.Select(
                             x =>
                                getParam(
                                   (FamilySymbol)((Expression.Container)x).Item,
                                   paramDef
                                )
                          )
                       )
                    );
                }
                else
                {
                    var fi = (FamilySymbol)((Expression.Container)input).Item;

                    return getParam(fi, paramDef);
                }
            }
        }
    }
}


