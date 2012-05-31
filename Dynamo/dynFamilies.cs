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
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Dynamo.Connectors;
using Dynamo.Utilities;
using TextBox = System.Windows.Controls.TextBox;

using Expression = Dynamo.FScheme.Expression;
using Microsoft.FSharp.Collections;

using System.Linq;

namespace Dynamo.Elements
{
   [ElementName("Family Type Selector")]
   [ElementCategory(BuiltinElementCategories.REVIT)]
   [ElementDescription("An element which allows you to select a Family Type from a drop down list.")]
   [RequiresTransaction(true)]
   public class dynFamilyTypeSelector : dynElement
   {
      System.Windows.Controls.ComboBox combo;
      Hashtable comboHash;

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
         comboHash = new Hashtable();

         PopulateComboBox();

         OutPortData = new PortData(null, "", "Family type", typeof(dynFamilyTypeSelector));
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
               comboHash.Add(comboText, fs);
            }
         }
      }

      public override Expression Evaluate(FSharpList<Expression> args)
      {
         ComboBoxItem cbi = combo.SelectedItem as ComboBoxItem;

         if (cbi != null)
         {
            var f = (FamilySymbol)comboHash[cbi.Content];
            return Expression.NewContainer(f);
         }

         throw new Exception("Nothing selected!");
      }
   }

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

   // MDJ added 11-14-11 
   // created new class dynFamilyInstanceCreatorXYZ by copying dynFamilyInstanceCreator, would be nice if there was a way to pass in either XYZ or RefPoints to previous class.
   // Hack: blind copy paste to change this to take XYZ instead
   // goal is to make family instances placeable in the project env
   // right now refpoints cannot be placed so the previous family instance creator fail
   //
   //

   [ElementName("Family Instance Creator")]
   [ElementCategory(BuiltinElementCategories.REVIT)]
   [ElementDescription("An element which allows you to create family instances.")]
   [RequiresTransaction(true)]
   public class dynFamilyInstanceCreatorXYZ : dynElement
   {
      public dynFamilyInstanceCreatorXYZ()
      {
         InPortData.Add(new PortData(null, "xyz", "xyz", typeof(object)));
         InPortData.Add(new PortData(null, "typ", "The Family Symbol to use for instantiation.", typeof(FamilySymbol)));

         OutPortData = new PortData(null, "fi", "Family instances created by this operation.", typeof(FamilyInstance));

         base.RegisterInputsAndOutputs();
      }

      private Expression makeFamilyInstance(object location, FamilySymbol fs, int count)
      {
         XYZ pos = location is ReferencePoint
            ? ((ReferencePoint)location).Position
            : (XYZ)location;

         FamilyInstance fi;

         if (this.Elements.Count > count)
         {
            fi = (FamilyInstance)this.Elements[count];
            LocationPoint lp = (LocationPoint)fi.Location;
            lp.Point = pos;
            //ElementTransformUtils.MoveElement(
            //   fi.Document, fi.Id, pos
            //);
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

            this.Elements.Add(fi);
         }

         return Expression.NewContainer(fi);
      }

      public override Expression Evaluate(FSharpList<Expression> args)
      {
         FamilySymbol fs = (FamilySymbol)((Expression.Container)args[1]).Item;
         var input = args[0];
         
         if (input.IsList)
         {
            var locList = ((Expression.List)input).Item;

            int count = 0;

            return Expression.NewList(
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
         }
         else
         {
            return this.makeFamilyInstance(
               ((Expression.Container)input).Item,
               fs,
               0
            );
         }
      }
   }

   // MDJ added 11-21-11 
   // created new class dynFamilyInstanceCreatorBySelection by copying dynFamilyInstanceCreator, 
   //
   //

   [ElementName("Family Instance Selection")]
   [ElementCategory(BuiltinElementCategories.REVIT)]
   [ElementDescription("An element which allows you to select a family instance from the document and reference it in Dynamo.")]
   [RequiresTransaction(false)]
   public class dynFamilyInstanceCreatorSelection : dynElement
   {
      TextBox tb;
      System.Windows.Controls.Button familyInstanceButt;

      public dynFamilyInstanceCreatorSelection()
      {
         OutPortData = new PortData(null, "fi", "Family instances created by this operation.", typeof(FamilyInstance));

         //add a button to the inputGrid on the dynElement
         familyInstanceButt = new System.Windows.Controls.Button();
         familyInstanceButt.Margin = new System.Windows.Thickness(0, 0, 0, 0);
         familyInstanceButt.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
         familyInstanceButt.VerticalAlignment = System.Windows.VerticalAlignment.Center;
         familyInstanceButt.Click += new System.Windows.RoutedEventHandler(familyInstanceButt_Click);
         familyInstanceButt.Content = "Select Instance";
         familyInstanceButt.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
         familyInstanceButt.VerticalAlignment = System.Windows.VerticalAlignment.Center;

         tb = new TextBox();
         tb.Text = "Nothing Selected";
         tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
         tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
         SolidColorBrush backgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
         tb.Background = backgroundBrush;
         tb.BorderThickness = new Thickness(0);
         tb.IsReadOnly = true;
         tb.IsReadOnlyCaretVisible = false;

         this.inputGrid.RowDefinitions.Add(new RowDefinition());
         this.inputGrid.RowDefinitions.Add(new RowDefinition());

         this.inputGrid.Children.Add(tb);
         this.inputGrid.Children.Add(familyInstanceButt);

         System.Windows.Controls.Grid.SetRow(familyInstanceButt, 0);
         System.Windows.Controls.Grid.SetRow(tb, 1);

         base.RegisterInputsAndOutputs();

         this.topControl.Height = 60;
         UpdateLayoutDelegate uld = new UpdateLayoutDelegate(CallUpdateLayout);
         Dispatcher.Invoke(uld, System.Windows.Threading.DispatcherPriority.Background, new object[] { this });

      }

      //public event PropertyChangedEventHandler PropertyChanged;

      //private void NotifyPropertyChanged(String info)
      //{
      //   if (PropertyChanged != null)
      //   {
      //      PropertyChanged(this, new PropertyChangedEventArgs(info));
      //   }
      //}

      public FamilyInstance pickedFamilyInstance;

      public FamilyInstance PickedFamilyInstance
      {
         get { return pickedFamilyInstance; }
         set
         {
            pickedFamilyInstance = value;
            NotifyPropertyChanged("PickedFamilyInstance");
         }
      }

      private ElementId familyInstanceID;

      private ElementId FamilyInstanceID
      {
         get { return familyInstanceID; }
         set
         {
            familyInstanceID = value;
            NotifyPropertyChanged("FamilyInstanceID");
         }
      }
      void familyInstanceButt_Click(object sender, System.Windows.RoutedEventArgs e)
      {
         PickedFamilyInstance = Dynamo.Utilities.SelectionHelper.RequestFamilyInstanceSelection(dynElementSettings.SharedInstance.Doc, "Selec Massing Family Instance", dynElementSettings.SharedInstance);

         if (PickedFamilyInstance != null)
         {
            FamilyInstanceID = PickedFamilyInstance.Id;
            familyInstanceButt.Content = "Change Instance";
            this.tb.Text = PickedFamilyInstance.Name;
         }
         else
         {
            familyInstanceButt.Content = "Select Instance";
            this.tb.Text = "Nothing Selected";
         }
      }

      public override FScheme.Expression Evaluate(Microsoft.FSharp.Collections.FSharpList<FScheme.Expression> args)
      {
         if (PickedFamilyInstance.Id.IntegerValue == FamilyInstanceID.IntegerValue) // sanity check
            return FScheme.Expression.NewContainer(PickedFamilyInstance);
         else
            throw new Exception("SANITY CHECK FAILED");
      }

      public override void SaveElement(XmlDocument xmlDoc, XmlElement dynEl)
      {
         //Debug.WriteLine(pd.Object.GetType().ToString());
         if (this.PickedFamilyInstance != null)
         {
            XmlElement outEl = xmlDoc.CreateElement("instance");
            outEl.SetAttribute("id", this.PickedFamilyInstance.Id.ToString());
            dynEl.AppendChild(outEl);
         }
      }

      public override void LoadElement(XmlNode elNode)
      {
         foreach (XmlNode subNode in elNode.ChildNodes)
         {
            if (subNode.Name.Equals("instance"))
            {
               try
               {
                  this.PickedFamilyInstance = dynElementSettings.SharedInstance.Doc.Document.get_Element(
                     new ElementId(Convert.ToInt32(subNode.Attributes[0].Value))
                  ) as FamilyInstance;
                  if (this.PickedFamilyInstance != null)
                  {
                     FamilyInstanceID = PickedFamilyInstance.Id;
                     this.tb.Text = this.PickedFamilyInstance.Name;
                     this.familyInstanceButt.Content = "Change Instance";
                  }
               }
               catch { }
            }
         }
      }
   }


   //[ElementName("Family Instance Creator")]
   //[ElementCategory(BuiltinElementCategories.REVIT)]
   //[ElementDescription("An element which allows you to create family instance from a point.")]
   //[RequiresTransaction(true)]
   //public class dynFamilyInstanceCreator : dynElement
   //{
   //   public dynFamilyInstanceCreator()
   //   {
   //      InPortData.Add(new PortData(null, "pt", "Reference point.", typeof(object)));
   //      InPortData.Add(new PortData(null, "typ", "The Family Type to use for instantiation.", typeof(object)));

   //      OutPortData = new PortData(null, "fi", "Family instance created by this operation.", typeof(object)));

   //      base.RegisterInputsAndOutputs();
   //   }

   //   public override FScheme.Expression Evaluate(Microsoft.FSharp.Collections.FSharpList<FScheme.Expression> args)
   //   {
   //      var pt = (ReferencePoint)((FScheme.Expression.Container)args[0]).Item;
   //      var fs = (FamilySymbol)((FScheme.Expression.Container)args[1]).Item;

   //      return FScheme.Expression.NewContainer(
   //         dynElementSettings.SharedInstance.Doc.Document.FamilyCreate.NewFamilyInstance(pt.Position, fs, StructuralType.NonStructural)
   //      );
   //   }

   //   public override void Draw()
   //   {
   //      if (CheckInputs())
   //      {
   //         DataTree treeIn = InPortData[0].Object as DataTree;
   //         if (treeIn != null)
   //         {
   //            Process(treeIn.Trunk, this.Tree.Trunk);
   //         }
   //      }

   //      base.Draw();
   //   }

   //   public void Process(DataTreeBranch bIn, DataTreeBranch currentBranch)
   //   {

   //      foreach (object o in bIn.Leaves)
   //      {
   //         ReferencePoint rp = o as ReferencePoint;

   //         if (rp != null)
   //         {
   //            //get the location of the point
   //            XYZ pos = rp.Position;
   //            FamilySymbol fs = InPortData[1].Object as FamilySymbol;
   //            FamilyInstance fi = dynElementSettings.SharedInstance.Doc.Document.FamilyCreate.NewFamilyInstance(pos, fs, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);

   //            Elements.Append(fi);
   //            currentBranch.Leaves.Add(fi);

   //         }
   //      }

   //      foreach (DataTreeBranch b1 in bIn.Branches)
   //      {
   //         DataTreeBranch newBranch = new DataTreeBranch();
   //         this.Tree.Trunk.Branches.Add(newBranch);
   //         Process(b1, newBranch);
   //      }
   //   }

   //   public void ProcessState(DataTreeBranch bIn, Hashtable parameterMap)
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
   //                     if (de.Value != null)
   //                        p.Set((double)de.Value);
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

   [ElementName("Set Instance Parameter")]
   [ElementCategory(BuiltinElementCategories.REVIT)]
   [ElementDescription("An element which allows you to modify parameters on family instances.")]
   [RequiresTransaction(true)]
   public class dynFamilyInstanceParameterSetter : dynElement
   {
      public dynFamilyInstanceParameterSetter()
      {
         InPortData.Add(new PortData(null, "fi", "Family instance.", typeof(object)));
         InPortData.Add(new PortData(null, "param", "Parameter to modify (string).", typeof(object)));
         InPortData.Add(new PortData(null, "value", "Value to set the parameter to.", typeof(object)));

         OutPortData = new PortData(null, "fi", "Modified family instance.", typeof(object));

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

      public override Expression Evaluate(FSharpList<FScheme.Expression> args)
      {
         var paramName = ((Expression.String)args[1]).Item;
         var valueExpr = args[2];

         var input = args[0];
         if (input.IsList)
         {
            var fiList = ((Expression.List)input).Item;
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

