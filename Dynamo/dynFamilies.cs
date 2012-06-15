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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using Autodesk.Revit.DB;
using Dynamo.Connectors;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using Expression = Dynamo.FScheme.Expression;
using TextBox = System.Windows.Controls.TextBox;
using System.Collections.Generic;

namespace Dynamo.Elements
{
   [ElementName("Family Type Selector")]
   [ElementCategory(BuiltinElementCategories.REVIT)]
   [ElementDescription("An element which allows you to select a Family Type from a drop down list.")]
   [RequiresTransaction(true)]
   public class dynFamilyTypeSelector : dynElement
   {
      System.Windows.Controls.ComboBox combo;
      Dictionary<string, FamilySymbol> comboHash = new Dictionary<string, FamilySymbol>();

      public dynFamilyTypeSelector()
      {
         //widen the control
         this.topControl.Width = 300;

         //add a drop down list to the window
         combo = new System.Windows.Controls.ComboBox();
         combo.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
         combo.VerticalAlignment = System.Windows.VerticalAlignment.Center;
         this.inputGrid.Children.Add(combo);
         System.Windows.Controls.Grid.SetColumn(combo, 0);
         System.Windows.Controls.Grid.SetRow(combo, 0);

         combo.DropDownOpened += new EventHandler(combo_DropDownOpened);
         combo.SelectionChanged += delegate { this.IsDirty = true; };

         PopulateComboBox();

         OutPortData = new PortData("", "Family type", typeof(dynFamilyTypeSelector));
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
         FilteredElementCollector fec = new FilteredElementCollector(dynElementSettings.SharedInstance.Doc.Document);
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
   }

   #region Disabled ParameterMapper

   //[ElementName("Instance Parameter Mapper")]
   //[ElementCategory(BuiltinElementCategories.REVIT)]
   //[ElementDescription("An element which maps the parameters of a Family Type.")]
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
   //[ElementDescription("An element which allows you to modify parameters on family instances.")]
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
   [ElementDescription("An element which allows you to create family instances.")]
   [RequiresTransaction(true)]
   public class dynFamilyInstanceCreatorXYZ : dynElement
   {
      public dynFamilyInstanceCreatorXYZ()
      {
         InPortData.Add(new PortData("xyz", "xyz", typeof(object)));
         InPortData.Add(new PortData("typ", "The Family Symbol to use for instantiation.", typeof(FamilySymbol)));

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
            fi = this.UIDocument.Document.get_Element(this.Elements[count]) as FamilyInstance;
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
               FSchemeInterop.Utils.convertSequence(
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

            int delCount = 0;
            foreach (var e in this.Elements.Skip(count))
            {
               this.UIDocument.Document.Delete(e);
               delCount++;
            }
            if (delCount > 0)
               this.Elements.RemoveRange(count, delCount);

            return result;
         }
         else
         {
            var result = this.makeFamilyInstance(
               ((Expression.Container)input).Item,
               fs,
               0
            );

            int count = 0;
            foreach (var e in this.Elements.Skip(1))
            {
               this.UIDocument.Document.Delete(e);
               count++;
            }
            if (count > 0)
               this.Elements.RemoveRange(1, count);

            return result;
         }
      }
   }

   //TODO: In Destroy(), have code that resets Elements back to their default.
   [ElementName("Set Instance Parameter")]
   [ElementCategory(BuiltinElementCategories.REVIT)]
   [ElementDescription("An element which allows you to modify parameters on family instances.")]
   [RequiresTransaction(true)]
   public class dynFamilyInstanceParameterSetter : dynElement
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
            return Expression.NewContainer(fi);
         }
         throw new Exception("Parameter \"" + paramName + "\" was not found!");
      }

      public override Expression Evaluate(FSharpList<Expression> args)
      {
         var paramName = ((Expression.String)args[1]).Item;
         var valueExpr = args[2];

         var input = args[0];
         if (input.IsList)
         {
            var fiList = (input as Expression.List).Item;
            return Expression.NewList(
               FSchemeInterop.Utils.convertSequence(
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
            var fi = (FamilyInstance)((Expression.Container)input).Item;

            return setParam(fi, paramName, valueExpr);
         }
      }
   }
}

