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

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Xml;
using Autodesk.Revit.DB;

using Dynamo.Utilities;
using Dynamo.Connectors;
using Dynamo.Revit;
using Dynamo.Revit.SyncedNodeExtensions; //Gives the RegisterEval... methods

using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using Value = Dynamo.FScheme.Value;
using Dynamo.FSchemeInterop;

namespace Dynamo.Nodes
{
    [IsInteractive(true)]
    public abstract class dynElementSelection: dynNodeWithOneOutput
    {
        TextBox tb;
        System.Windows.Controls.Button selectButton;

        protected string _selectButtonContent;
        public string SelectButtonContent 
        {
            get { return _selectButtonContent; }
            set
            {
                _selectButtonContent = value;
                RaisePropertyChanged("SelectButtonContent");
            }
        }

        /// <summary>
        /// Determines what the text should read on the node when the selection has been changed.
        /// Is ignored in the case where nothing is selected.
        /// </summary>
        //protected abstract string SelectionText { get; }
        protected string _selectionText;

        public abstract string SelectionText { get; set; }

        protected dynElementSelection(PortData outPortData)
        {
            OutPortData.Add(outPortData);
            RegisterAllPorts();
        }

        public override void SetupCustomUIElements(Controls.dynNodeView NodeUI)
        {
            //add a button to the inputGrid on the dynElement
            selectButton = new System.Windows.Controls.Button();
            selectButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            selectButton.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            selectButton.Click += new System.Windows.RoutedEventHandler(selectButton_Click);
            //selectButton.Content = "Select Instance";
            selectButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            selectButton.VerticalAlignment = System.Windows.VerticalAlignment.Center;

            tb = new TextBox();
            //tb.Text = "Nothing Selected";
            SelectionText = "Nothing Selected";
            SelectButtonContent = "Select Instance";

            tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            SolidColorBrush backgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
            tb.Background = backgroundBrush;
            tb.BorderThickness = new Thickness(0);
            tb.IsReadOnly = true;
            tb.IsReadOnlyCaretVisible = false;

            //NodeUI.SetRowAmount(2);
            NodeUI.inputGrid.RowDefinitions.Add(new RowDefinition());
            NodeUI.inputGrid.RowDefinitions.Add(new RowDefinition());

            NodeUI.inputGrid.Children.Add(tb);
            NodeUI.inputGrid.Children.Add(selectButton);

            System.Windows.Controls.Grid.SetRow(selectButton, 0);
            System.Windows.Controls.Grid.SetRow(tb, 1);

            tb.DataContext = this;
            selectButton.DataContext = this;

            var selectTextBinding = new System.Windows.Data.Binding("SelectionText")
            {
                Mode = BindingMode.TwoWay,
            };
            tb.SetBinding(TextBox.TextProperty, selectTextBinding);

            var buttonTextBinding = new System.Windows.Data.Binding("SelectButtonContent")
            {
                Mode = BindingMode.TwoWay,
            };
            selectButton.SetBinding(Button.ContentProperty, buttonTextBinding);
        }

        private void selectButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.selectButton.IsEnabled = false;
            IdlePromise.ExecuteOnIdle(new Action(
                delegate
                {
                    this.OnSelectClick();
                    this.selectButton.IsEnabled = true;
                }
            ));
        }

        /// <summary>
        /// Callback for when the "Select" button has been clicked.
        /// </summary>
        /// 
        /// 
        protected abstract void OnSelectClick();

        private Element selected;
        /// <summary>
        /// The Element which is selected. Setting this property will automatically register the Element
        /// for proper updating, and will update this node's IsDirty value.
        /// </summary>
        public virtual Element SelectedElement
        {
            get { return selected; }
            set
            {
                var dirty = false;
                if (this.selected != null)
                {
                    if (value != null && value.Id.Equals(this.selected.Id))
                        return;

                    dirty = true;
                    this.UnregisterEvalOnModified(this.selected.Id);
                }
                else
                {
                    dirty = value != null;
                }

                this.selected = value;
                if (value != null)
                {
                    this.RegisterEvalOnModified(
                       value.Id,
                       delAction: delegate { this.selected = null; this.SelectedElement = null; }
                    );

                    //this.tb.Text = this.SelectionText;
                    //this.selectButton.Content = "Change";
                    SelectButtonContent = "Change";
                }
                else
                {
                    //this.tb.Text = "Nothing Selected.";
                    //this.selectButton.Content = "Select";
                    SelectionText = "Nothing Selected";
                    SelectButtonContent = "Select";
                }

                if (dirty)
                    this.RequiresRecalc = true;
            }
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            if (this.SelectedElement == null)
                throw new Exception("Nothing selected.");

            return Value.NewContainer(this.SelectedElement);
        }

        public override void SaveElement(XmlDocument xmlDoc, XmlElement dynEl)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            if (this.SelectedElement != null)
            {
                XmlElement outEl = xmlDoc.CreateElement("instance");
                outEl.SetAttribute("id", this.SelectedElement.Id.ToString());
                dynEl.AppendChild(outEl);
            }
        }

        public override void LoadElement(XmlNode elNode)
        {
            foreach (XmlNode subNode in elNode.ChildNodes)
            {
                if (subNode.Name.Equals("instance"))
                {
                    Element saved = null;
                    var id = new ElementId(Convert.ToInt32(subNode.Attributes[0].Value));
                    try
                    {
                        saved = dynRevitSettings.Doc.Document.GetElement(id) as FamilyInstance;
                    }
                    catch
                    {
                        dynSettings.Controller.DynamoViewModel.Log("Unable to find element with ID: " + id.IntegerValue);
                    }
                    this.SelectedElement = saved;
                }
            }
        }
    }

    [IsInteractive(true)]
    public abstract class dynMultipleElementSelection: dynNodeWithOneOutput
    {
        TextBox tb;
        System.Windows.Controls.Button selectButton;

        protected string _selectButtonContent;
        public string SelectButtonContent
        {
            get { return _selectButtonContent; }
            set
            {
                _selectButtonContent = value;
                RaisePropertyChanged("SelectButtonContent");
            }
        }

        /// <summary>
        /// Determines what the text should read on the node when the selection has been changed.
        /// Is ignored in the case where nothing is selected.
        /// </summary>
        //protected abstract string SelectionText { get; }
        protected string _selectionText;
        public abstract string SelectionText { get; set; }

        protected dynMultipleElementSelection(PortData outData)
        {
            OutPortData.Add(outData);
            RegisterAllPorts();
        }

        public override void SetupCustomUIElements(Controls.dynNodeView NodeUI)
        {

            //add a button to the inputGrid on the dynElement
            selectButton = new System.Windows.Controls.Button();
            selectButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            selectButton.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            selectButton.Click += new System.Windows.RoutedEventHandler(selectButton_Click);
            //selectButton.Content = "Select Instances";
            selectButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            selectButton.VerticalAlignment = System.Windows.VerticalAlignment.Center;

            tb = new TextBox();
            //tb.Text = "Nothing Selected";
            SelectionText = "Nothing Selected";
            SelectButtonContent = "Select Instances";

            tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            SolidColorBrush backgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
            tb.Background = backgroundBrush;
            tb.BorderThickness = new Thickness(0);
            tb.IsReadOnly = true;
            tb.IsReadOnlyCaretVisible = false;

            NodeUI.inputGrid.RowDefinitions.Add(new RowDefinition());
            NodeUI.inputGrid.RowDefinitions.Add(new RowDefinition());

            NodeUI.inputGrid.Children.Add(tb);
            NodeUI.inputGrid.Children.Add(selectButton);

            System.Windows.Controls.Grid.SetRow(selectButton, 0);
            System.Windows.Controls.Grid.SetRow(tb, 1);

            tb.DataContext = this;
            selectButton.DataContext = this;

            var selectTextBinding = new System.Windows.Data.Binding("SelectionText")
            {
                Mode = BindingMode.TwoWay,
            };
            tb.SetBinding(TextBox.TextProperty, selectTextBinding);

            var buttonTextBinding = new System.Windows.Data.Binding("SelectButtonContent")
            {
                Mode = BindingMode.TwoWay,
            };
            selectButton.SetBinding(Button.ContentProperty, buttonTextBinding);
        }

        private void selectButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.selectButton.IsEnabled = false;
            IdlePromise.ExecuteOnIdle(new Action(
                delegate
                {
                    this.OnSelectClick();
                    this.selectButton.IsEnabled = true;
                }
            ));
        }

        /// <summary>
        /// Callback for when the "Select" button has been clicked.
        /// </summary>
        /// 
        /// 
        protected abstract void OnSelectClick();

        private IList<Element> selected;
        /// <summary>
        /// The Element which is selected. Setting this property will automatically register the Element
        /// for proper updating, and will update this node's IsDirty value.
        /// </summary>
        public virtual IList<Element> SelectedElements
        {
            get { return selected; }
            set
            {
                var dirty = false;
                if (this.selected != null)
                {
                    foreach (Element selectedElement in this.selected)
                    {
                        foreach (Element previousElement in value)
                        {

                            if (previousElement != null && previousElement.Id.Equals(selectedElement.Id))
                                return;

                            dirty = true;
                            this.UnregisterEvalOnModified(selectedElement.Id);
                        }
                    }
                }
                else
                {
                    dirty = value != null;
                }

                this.selected = value;
                if (value != null)
                {
                    foreach (Element previousElement in value)
                    {
                        this.RegisterEvalOnModified(
                           previousElement.Id,
                           delAction: delegate { this.selected = null; this.SelectedElements = null; }
                        );
                    }

                    //this.tb.Text = this.SelectionText;
                    //this.selectButton.Content = "Change";
                    SelectButtonContent = "Change";
                }
                else
                {
                    //this.tb.Text = "Nothing Selected.";
                    //this.selectButton.Content = "Select";
                    SelectButtonContent = "Select";
                }

                if (dirty)
                    this.RequiresRecalc = true;
            }
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            if (this.SelectedElements == null)
                throw new Exception("Nothing selected.");

            FSharpList<Value> result = FSharpList<Value>.Empty;

            var els = new List<Value>();
            foreach (Element el in this.SelectedElements)
            {
                els.Add(Value.NewContainer(el));
            }

            return Value.NewList(Utils.SequenceToFSharpList(els));
        }

        public override void SaveElement(XmlDocument xmlDoc, XmlElement dynEl)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            if (this.SelectedElements != null)
            {
                foreach (Element selectedElement in this.SelectedElements)
                {
                    XmlElement outEl = xmlDoc.CreateElement("instance");
                    outEl.SetAttribute("id", selectedElement.Id.ToString());
                    dynEl.AppendChild(outEl);
                }
            }
        }

        public override void LoadElement(XmlNode elNode)
        {
            foreach (XmlNode subNode in elNode.ChildNodes)
            {
                if (subNode.Name.Equals("instance"))
                {
                    Element saved = null;
                    var id = new ElementId(Convert.ToInt32(subNode.Attributes[0].Value));
                    try
                    {
                        saved = dynRevitSettings.Doc.Document.GetElement(id) as FamilyInstance;
                    }
                    catch
                    {
                        dynSettings.Controller.DynamoViewModel.Log("Unable to find element with ID: " + id.IntegerValue);
                    }
                    this.SelectedElements.Add(saved);
                }
            }
        }
    }

    [NodeName("Select Family Instance")]
    [NodeCategory(BuiltinNodeCategories.CORE_SELECTION)]
    [NodeDescription("Select a family instance from the document.")]
    public class dynFamilyInstanceCreatorSelection : dynElementSelection
    {
        public dynFamilyInstanceCreatorSelection()
            : base(new PortData("fi", "Family instances created by this operation.", typeof(Value.Container)))
        { }

        protected override void OnSelectClick()
        {
            this.SelectedElement = dynRevitSettings.SelectionHelper.RequestFamilyInstanceSelection(
               dynRevitSettings.Doc, "Select Massing Family Instance"
            );
            RaisePropertyChanged("SelectionText");
        }

        public override string SelectionText
        {
            get
            {
                return _selectionText = this.SelectedElement == null ?
                    "Nothing Selected" :
                    this.SelectedElement.Name;
            }
            set
            {
                _selectionText = value;
                RaisePropertyChanged("SelectionText");
            }
        }
    }

    [NodeName("Select Divided Surface")]
    [NodeCategory(BuiltinNodeCategories.CORE_SELECTION)]
    [NodeDescription("Select a divided surface from the document.")]
    public class dynDividedSurfaceBySelection : dynElementSelection
    {
        Value data;

        public dynDividedSurfaceBySelection()
            : base(new PortData("srf", "The divided surface family instance(s)", typeof(Value.Container)))
        { }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var result = new List<List<FamilyInstance>>();

            //"Get an interface to the divided surfaces on this element."
            //TODO: do we want to select a face instead and try to get
            //the divided surface that way?
            DividedSurfaceData dsd = this.SelectedElement.GetDividedSurfaceData();

            if (dsd != null)
            {
                foreach (Reference r in dsd.GetReferencesWithDividedSurfaces())
                {
                    DividedSurface ds = dsd.GetDividedSurfaceForReference(r);

                    GridNode gn = new GridNode();

                    int u = 0;
                    while (u < ds.NumberOfUGridlines)
                    {

                        var lst = new List<FamilyInstance>();

                        gn.UIndex = u;

                        int v = 0;
                        while (v < ds.NumberOfVGridlines)
                        {
                            gn.VIndex = v;

                            //"Reports whether a grid node is a "seed node," a node that is associated with one or more tiles."
                            if (ds.IsSeedNode(gn))
                            {
                                FamilyInstance fi
                                  = ds.GetTileFamilyInstance(gn, 0);

                                //put the family instance into the tree
                                lst.Add(fi);
                            }
                            v = v + 1;
                        }

                        //don't add list if it's empty
                        if(lst.Count() > 0)
                            result.Add(lst);

                        u = u + 1;
                    }
                }

                this.data = Value.NewList(
                   Utils.SequenceToFSharpList(
                      result.Select(
                         row => Value.NewList(
                            Utils.SequenceToFSharpList(
                               row.Select(Value.NewContainer)
                            )
                         )
                      )
                   )
                );
            }

            return data;
        }

        public override string SelectionText
        {
            get
            {
                return _selectionText = this.SelectedElement == null ?
                    "Nothing Selected" :
                    "Element ID: " + this.SelectedElement.Id;
            }
            set
            {
                _selectionText = value;
                RaisePropertyChanged("SelectionText");
            }
        }

        /*
        public override Element SelectedElement
        {
            get
            {
                return base.SelectedElement;
            }
            set
            {
                base.SelectedElement = value;

                var result = new List<List<FamilyInstance>>();

                DividedSurfaceData dsd = this.SelectedElement.GetDividedSurfaceData();

                if (dsd != null)
                {
                    foreach (Reference r in dsd.GetReferencesWithDividedSurfaces())
                    {
                        DividedSurface ds = dsd.GetDividedSurfaceForReference(r);

                        GridNode gn = new GridNode();

                        int u = 0;
                        while (u < ds.NumberOfUGridlines)
                        {

                            var lst = new List<FamilyInstance>();

                            gn.UIndex = u;

                            int v = 0;
                            while (v < ds.NumberOfVGridlines)
                            {
                                gn.VIndex = v;

                                if (ds.IsSeedNode(gn))
                                {
                                    FamilyInstance fi
                                      = ds.GetTileFamilyInstance(gn, 0);

                                    //put the family instance into the tree
                                    lst.Add(fi);
                                }
                                v = v + 1;
                            }

                            result.Add(lst);

                            u = u + 1;
                        }
                    }

                    this.data = Value.NewList(
                       Utils.SequenceToFSharpList(
                          result.Select(
                             row => Value.NewList(
                                Utils.SequenceToFSharpList(
                                   row.Select(Value.NewContainer)
                                )
                             )
                          )
                       )
                    );
                }
            }
        }
        */

        protected override void OnSelectClick()
        {
            this.SelectedElement = null;
            this.SelectedElement = dynRevitSettings.SelectionHelper.RequestFormSelection(
               dynRevitSettings.Doc, "Select a form element."
            );
            RaisePropertyChanged("SelectionText");
        }
    }

    [NodeName("Select Face")]
    [NodeCategory(BuiltinNodeCategories.CORE_SELECTION)]
    [NodeDescription("Select a face from the document.")]
    public class dynFormElementBySelection : dynElementSelection, IDrawable
    {
        Reference f;

        public dynFormElementBySelection()
            : base(new PortData("face", "The face", typeof(Value.Container)))
        { }

        protected override void OnSelectClick()
        {
            var doc = dynRevitSettings.Doc;

            f = dynRevitSettings.SelectionHelper.RequestFaceReferenceSelection(
               doc, "Select a face."
            );
            this.SelectedElement = doc.Document.GetElement(f);
            RaisePropertyChanged("SelectionText");
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            return Value.NewContainer(f);
        }

        public override string SelectionText
        {
            get
            {
                return _selectionText = this.SelectedElement == null ?
                    "Nothing Selected" :
                    "Face ID: " + this.SelectedElement.Id;
            }
            set
            {
                _selectionText = value;
                RaisePropertyChanged("SelectionText");
            }
        }

        public RenderDescription Draw()
        {
            RenderDescription rd = new RenderDescription();

            Face face = (Face)dynRevitSettings.Doc.Document.GetElement(f).GetGeometryObjectFromReference(f);

            dynRevitTransactionNode.DrawFace(rd, face);

            return rd;
        }

    }

    [NodeName("Select Curve")]
    [NodeCategory(BuiltinNodeCategories.CORE_SELECTION)]
    [NodeDescription("Select a curve from the document.")] //or set of curves in the future
    public class dynCurvesBySelection : dynElementSelection
    {
        public dynCurvesBySelection()
            : base(new PortData("curve", "The curve", typeof(Value.Container)))
        { }

        protected override void OnSelectClick()
        {
            this.SelectedElement = dynRevitSettings.SelectionHelper.RequestCurveElementSelection(
               dynRevitSettings.Doc, "Select a curve."
            );
            RaisePropertyChanged("SelectionText");
        }

        public override string SelectionText
        {
            get
            {
                return _selectionText = this.SelectedElement == null ?
                    "Nothing Selected" :
                    "Curve ID: " + this.SelectedElement.Id;
            }
            set
            {
                _selectionText = value;   
                RaisePropertyChanged("SelectionText");
            }
        }
    }

    [NodeName("Select Curves")]
    [NodeCategory(BuiltinNodeCategories.CORE_SELECTION)]
    [NodeDescription("Select a set of curves from the document.")]
    public class dynMultipleCurvesBySelection : dynMultipleElementSelection
    {
        public dynMultipleCurvesBySelection()
            : base(new PortData("curves", "The curves", typeof(Value.Container)))
        { }

        protected override void OnSelectClick()
        {
            if (this.SelectedElements != null)
            {
                SelectedElements.Clear();
                SelectedElements = null;
            }

            SelectedElements = dynRevitSettings.SelectionHelper.RequestMultipleCurveElementsSelection(
               dynRevitSettings.Doc, "Select a set of curves."
            );

            RaisePropertyChanged("SelectionText");
        }

        string formatSelectionText(IList<Element> elements)
        {
            string selectionText = "";
            foreach (Element e in elements)
            {
                selectionText = selectionText + " " + e.Id.ToString();
            }
            return selectionText;
        }

        public override string SelectionText
        {
            get
            {
                return _selectionText = (this.SelectedElements != null && this.SelectedElements.Count > 0) ?
                      "Curve IDs:" + formatSelectionText(this.SelectedElements) :
                      "Nothing Selected";
            }
            set
            {
                _selectionText = value;
                RaisePropertyChanged("SelectionText");
            }
        }
    }

    [NodeName("Select Point")]
    [NodeCategory(BuiltinNodeCategories.CORE_SELECTION)]
    [NodeDescription("Select a reference point from the document.")]
    public class dynPointBySelection : dynElementSelection
    {
        public dynPointBySelection() :
            base(new PortData("pt", "The point", typeof(Value.Container)))
        { }

        protected override void OnSelectClick()
        {
            this.SelectedElement = dynRevitSettings.SelectionHelper.RequestReferencePointSelection(
               dynRevitSettings.Doc, "Select a reference point."
            );

            RaisePropertyChanged("SelectionText");
        }

        public override string SelectionText
        {
            get
            {
                return _selectionText = this.SelectedElement == null ?
                    "Nothing Selected" :
                    this.SelectedElement.Name + " (" + this.SelectedElement.Id + ")";
            }
            set
            {
                _selectionText = value;
                RaisePropertyChanged("SelectionText");
            }
        }
    }

    [NodeName("Select Level")]
    [NodeCategory(BuiltinNodeCategories.CORE_SELECTION)]
    [NodeDescription("Select a level from the document.")]
    public class dynLevelBySelection : dynElementSelection
    {
        public dynLevelBySelection() :
            base(new PortData("lvl", "The selected level", typeof(Value.Container)))
        { }

        protected override void OnSelectClick()
        {
            this.SelectedElement = dynRevitSettings.SelectionHelper.RequestLevelSelection(
               dynRevitSettings.Doc, "Select a level."
            );
            RaisePropertyChanged("SelectionText");
        }

        public override string SelectionText
        {
            get
            {
                return _selectionText = this.SelectedElement == null ?
                    "Nothing Selected" :
                    this.SelectedElement.Name + " (" + this.SelectedElement.Id + ")";
            }
            set
            {
                _selectionText = value;
                RaisePropertyChanged("SelectionText");
            }
        }
    }
}


