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
    public abstract class dynElementSelectionBase: dynNodeWithOneOutput
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

        protected dynElementSelectionBase(PortData outPortData)
        {
            OutPortData.Add(outPortData);
            RegisterAllPorts();
        }

        public override void SetupCustomUIElements(Controls.dynNodeView nodeUI)
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
            if (this.SelectedElement == null || SelectionText.Count() < 1 || SelectButtonContent.Count() < 1 )
            {
                SelectionText = "Nothing Selected";
                SelectButtonContent = "Select Instance";
            }

            tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            SolidColorBrush backgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
            tb.Background = backgroundBrush;
            tb.BorderThickness = new Thickness(0);
            tb.IsReadOnly = true;
            tb.IsReadOnlyCaretVisible = false;

            //NodeUI.SetRowAmount(2);
            nodeUI.inputGrid.RowDefinitions.Add(new RowDefinition());
            nodeUI.inputGrid.RowDefinitions.Add(new RowDefinition());

            nodeUI.inputGrid.Children.Add(tb);
            nodeUI.inputGrid.Children.Add(selectButton);

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
            {
                //if (dynSettings.Controller.Testing)
                //{
                //    //if we're in test mode
                //    //try to pass out an object of the right type
                //    if (this is dynCurvesBySelection)
                //    {
                //        FilteredElementCollector fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
                //        fec.OfClass(typeof(CurveElement));

                //        if (fec.ToElements().Any())
                //        {
                //            //attempt to find elements that have not yet been selected
                //            //this is important for things like lofts where you cannot have
                //            //an element be the input for two parts of the form
                //            var curveBySelectionNodes = dynSettings.Controller.DynamoModel.Nodes
                //                .Where(x => (x is dynCurvesBySelection) && (x as dynCurvesBySelection).SelectedElement != null);
                //            var previouslySelectedElements = curveBySelectionNodes
                //                .Select(x => (x as dynCurvesBySelection).SelectedElement);
                //            foreach (Element e in fec.ToElements())
                //            {
                //                if (e is ModelCurve)
                //                {
                //                    if (!previouslySelectedElements.Contains(e))
                //                    {
                //                        this.SelectedElement = e;
                //                        break;
                //                    }
                //                }
                //            }
                //        }
                //        else
                //            throw new Exception("Suitable curve could not be found for testing.");
                //    }
                //}
                //else
                    throw new Exception("Nothing selected.");
            }

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
                        saved = dynRevitSettings.Doc.Document.GetElement(id) as Element; // FamilyInstance;
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
    public abstract class dynMultipleElementSelectionBase: dynNodeWithOneOutput
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

        protected dynMultipleElementSelectionBase(PortData outData)
        {
            OutPortData.Add(outData);
            RegisterAllPorts();
        }

        public override void SetupCustomUIElements(Controls.dynNodeView nodeUI)
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

            if (this.SelectedElements == null || this.SelectedElements.Count() < 1 ||
                SelectionText.Count() < 1 || SelectButtonContent.Count() < 1)
            {
                SelectionText = "Nothing Selected";
                SelectButtonContent = "Select Instances";
            }

            tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            SolidColorBrush backgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
            tb.Background = backgroundBrush;
            tb.BorderThickness = new Thickness(0);
            tb.IsReadOnly = true;
            tb.IsReadOnlyCaretVisible = false;

            nodeUI.inputGrid.RowDefinitions.Add(new RowDefinition());
            nodeUI.inputGrid.RowDefinitions.Add(new RowDefinition());

            nodeUI.inputGrid.Children.Add(tb);
            nodeUI.inputGrid.Children.Add(selectButton);

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
                        saved = dynRevitSettings.Doc.Document.GetElement(id) as Element;
                    }
                    catch
                    {
                        dynSettings.Controller.DynamoViewModel.Log("Unable to find element with ID: " + id.IntegerValue);
                    }
                    if (this.SelectedElements == null)
                        this.SelectedElements = new List<Element>();
                    this.SelectedElements.Add(saved);
                }
            }
        }
    }

    [NodeName("Select Family Instance")]
    [NodeCategory(BuiltinNodeCategories.CORE_SELECTION)]
    [NodeDescription("Select a family instance from the document.")]
    public class dynFamilyInstanceCreatorSelection : dynElementSelectionBase
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
    public class dynDividedSurfaceBySelection : dynElementSelectionBase
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
    public class dynFormElementBySelection : dynElementSelectionBase, IDrawable
    {
        Reference f;

        public RenderDescription RenderDescription { get; set; }

        public dynFormElementBySelection()
            : base(new PortData("face", "The face", typeof(Value.Container)))
        {}

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
            Face face = (Face)dynRevitSettings.Doc.Document.GetElement(f).GetGeometryObjectFromReference(f);
            return Value.NewContainer(face);
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

        public void Draw()
        {
            if (this.RenderDescription == null)
                this.RenderDescription = new RenderDescription();
            else
                this.RenderDescription.ClearAll();

            Face face = (Face)dynRevitSettings.Doc.Document.GetElement(f).GetGeometryObjectFromReference(f);

            dynRevitTransactionNode.DrawFace(this.RenderDescription, face);

        }
        
        public override void SaveElement(XmlDocument xmlDoc, XmlElement dynEl)
        {

            dynEl.SetAttribute("faceRef", this.f.ConvertToStableRepresentation( dynRevitSettings.Doc.Document));
        }

        public override void LoadElement(XmlNode elNode)
        {
            try
            {
                this.f = Reference.ParseFromStableRepresentation(dynRevitSettings.Doc.Document, elNode.Attributes["faceRef"].Value.ToString());
                if (f != null)
                   this.SelectedElement = dynRevitSettings.Doc.Document.GetElement(f.ElementId);
            }
            catch { }
        }
    }

    [NodeName("Select Edge")]
    [NodeCategory(BuiltinNodeCategories.CORE_SELECTION)]
    [NodeDescription("Select an edge from the document.")]
    public class dynEdgeOnElementBySelection : dynElementSelectionBase, IDrawable
    {
        Reference f;
        public RenderDescription RenderDescription { get; set; }

        public dynEdgeOnElementBySelection()
            : base(new PortData("edge", "The edge", typeof(Value.Container)))
        {}

        protected override void OnSelectClick()
        {
            var doc = dynRevitSettings.Doc;

            f = dynRevitSettings.SelectionHelper.RequestEdgeReferenceSelection(
               doc, "Select an edge."
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
                    "Element of Edge  ID: " + this.SelectedElement.Id;
            }
            set
            {
                _selectionText = value;
                RaisePropertyChanged("SelectionText");
            }
        }

        public void Draw()
        {
            if (this.RenderDescription == null)
                this.RenderDescription = new RenderDescription();
            else
                this.RenderDescription.ClearAll();

            Edge edge = (Edge)dynRevitSettings.Doc.Document.GetElement(f).GetGeometryObjectFromReference(f);

            dynRevitTransactionNode.DrawGeometryElement(this.RenderDescription, edge);

        }

        public override void SaveElement(XmlDocument xmlDoc, XmlElement dynEl)
        {

            dynEl.SetAttribute("edgeRef", this.f.ConvertToStableRepresentation(dynRevitSettings.Doc.Document));
        }

        public override void LoadElement(XmlNode elNode)
        {
            try
            {
                this.f = Reference.ParseFromStableRepresentation(dynRevitSettings.Doc.Document, elNode.Attributes["edgeRef"].Value.ToString());
                if (f != null)
                    this.SelectedElement = dynRevitSettings.Doc.Document.GetElement(f.ElementId);
            }
            catch { }
        }

    }

    [NodeName("Select Curve")]
    [NodeCategory(BuiltinNodeCategories.CORE_SELECTION)]
    [NodeDescription("Select a curve from the document.")] //or set of curves in the future
    public class dynCurvesBySelection : dynElementSelectionBase, IDrawable
    {
        public dynCurvesBySelection()
            : base(new PortData("curve", "The curve", typeof(Value.Container)))
        { 
        }

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
                        saved = dynRevitSettings.Doc.Document.GetElement(id) as Element; // FamilyInstance;
                    }
                    catch
                    {
                        dynSettings.Controller.DynamoViewModel.Log("Unable to find element with ID: " + id.IntegerValue);
                    }

                    //only set the selected element if the element
                    //returned is of the type required by this node
                    if(saved is CurveElement)
                        this.SelectedElement = saved;

                }
            }
        }

        #region IDrawable Interface
        public RenderDescription RenderDescription { get; set; }
        public void Draw()
        {
            if (this.RenderDescription == null)
                this.RenderDescription = new RenderDescription();
            else
                this.RenderDescription.ClearAll();

            CurveElement ce = this.SelectedElement as CurveElement;
            dynRevitTransactionNode.DrawCurve(this.RenderDescription, ce.GeometryCurve);

        }
        #endregion
    }

    [NodeName("Select Curves")]
    [NodeCategory(BuiltinNodeCategories.CORE_SELECTION)]
    [NodeDescription("Select a set of curves from the document.")]
    public class dynMultipleCurvesBySelection : dynMultipleElementSelectionBase
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
    public class dynPointBySelection : dynElementSelectionBase
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
    public class dynLevelBySelection : dynElementSelectionBase
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

    [NodeName("Select Model Element")]
    [NodeCategory(BuiltinNodeCategories.CORE_SELECTION)]
    [NodeDescription("Select a model element from the document.")]
    public class dynModelElementSelection : dynElementSelectionBase
    {
        public dynModelElementSelection()
            : base(new PortData("me", "Model element reference created by this operation.", typeof(Value.Container)))
        { }

        protected override void OnSelectClick()
        {
            this.SelectedElement = dynRevitSettings.SelectionHelper.RequestModelElementSelection(
               dynRevitSettings.Doc, "Select Model Element"
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

    [NodeName("Select XYZ on element")]
    [NodeCategory(BuiltinNodeCategories.CORE_SELECTION)]
    [NodeDescription("Select a XYZ location on model face or edge of the element.")]
    public class dynXYZBySelection : dynElementSelectionBase, IDrawable
    {
        Reference refXYZ;
        double param0;
        double param1;
        bool init;

        public RenderDescription RenderDescription { get; set; }

        public dynXYZBySelection() :
            base(new PortData("XYZ", "The XYZ location on element", typeof(Value.Container)))
        {
        }

        protected override void OnSelectClick()
        {
            this.refXYZ = dynRevitSettings.SelectionHelper.RequestReferenceXYZSelection(
               dynRevitSettings.Doc, "Select a XYZ location on face or edge of the element."
            );
            if (this.refXYZ != null)
               this.SelectedElement = dynRevitSettings.Doc.Document.GetElement(refXYZ.ElementId);
           this.init = false;

            RaisePropertyChanged("SelectionText");
        }

       public override Value Evaluate(FSharpList<Value> args)
       {
           if (refXYZ.ElementReferenceType != ElementReferenceType.REFERENCE_TYPE_SURFACE &&
                 refXYZ.ElementReferenceType != ElementReferenceType.REFERENCE_TYPE_LINEAR)
               throw new Exception("Could not use face or edge which is not part of the model");

            GeometryObject thisObject = this.SelectedElement.GetGeometryObjectFromReference(refXYZ);
            Autodesk.Revit.DB.Transform thisTrf = null;

       
           {
               GeometryObject geomObj = this.SelectedElement.get_Geometry(new Autodesk.Revit.DB.Options());
               GeometryElement geomElement = geomObj as GeometryElement;

               // ugly code to detect if transform for geometry object is needed or not
               // filed request to provide this info via API, but meanwhile ...
               foreach (GeometryObject geob in geomElement)
               {
                   if (!(geob is GeometryInstance))
                       continue;
                  
                   GeometryInstance ginsta = geob as GeometryInstance;
                   GeometryElement gSymbolElement = ginsta.GetSymbolGeometry();
                   List <GeometryElement> geometryElements = new List <GeometryElement>();
                  geometryElements.Add(gSymbolElement);
                  bool found = false;
                  for (; geometryElements.Count > 0 && !found;)
                  {
                       GeometryElement thisGeometryElement = geometryElements[0];
                       geometryElements.Remove(thisGeometryElement);

                     foreach (GeometryObject geobSym in thisGeometryElement)
                     {
                           if (geobSym is GeometryElement)
                           {
                               geometryElements.Add((GeometryElement)geobSym);
                               continue;
                           }
                           if ((thisObject is Curve) && (geobSym is Curve) && (thisObject == geobSym))
                           {
                               found = true;
                               break;
                           }

                           if (thisObject is Curve)
                               continue;

                           if ((thisObject is Face) && (geobSym is Face) && (thisObject == geobSym))
                           {
                               found = true;
                               break;
                           }

                           if ((thisObject is Edge) && (geobSym is Face))
                           {
                               Edge edge = (Edge)thisObject;
                               //use GetFace after r2013 support is dropped
                               if (geobSym == edge.get_Face(0) || geobSym == edge.get_Face(1))
                               {
                                   found = true;
                                   break;
                               }
                           }
                           if (!(geobSym is Solid))
                               continue;

                           FaceArray solidFaces = ((Solid)geobSym).Faces;
                           int numFaces = solidFaces.Size;
                           for (int index = 0; index < numFaces && !found; index++)
                           {
                               Face faceAt = solidFaces.get_Item(index);
                               if ((thisObject is Face) && (thisObject == faceAt))
                               {
                                   found = true;
                                   break;
                               }
                               if (thisObject is Edge)
                               {
                                   Edge edge = (Edge)thisObject;
                                   //use GetFace after r2013 support is dropped
                                   if (faceAt == edge.get_Face(0) || faceAt == edge.get_Face(1))
                                   {
                                       found = true;
                                       break;
                                   }
                               }
                           }
                       }
                   }

                   if (found)
                   {
                       thisTrf = ginsta.Transform;
                       break;
                   }
               }
               if (thisObject == null)
                   throw new Exception("could not resolve reference for XYZ on Element");
            }
            XYZ thisXYZ = null;
            
            if (refXYZ.ElementReferenceType == ElementReferenceType.REFERENCE_TYPE_SURFACE && thisObject is Face)
            {
                Face face = thisObject as Face;
                if (!init)
                {
                   param0 = refXYZ.UVPoint[0];
                   param1 = refXYZ.UVPoint[1];
                   init = true;
                }
                UV uv = new UV(param0, param1);
                thisXYZ = face.Evaluate(uv);
                if (thisTrf != null)
                    thisXYZ = thisTrf.OfPoint(thisXYZ);
            }
            else if (refXYZ.ElementReferenceType == ElementReferenceType.REFERENCE_TYPE_LINEAR )
            {
                Curve curve = null;
                if (thisObject is Edge)
                {
                    Edge edge = (Edge)this.SelectedElement.GetGeometryObjectFromReference(refXYZ);
                    curve = edge.AsCurve();
                }
                else
                    curve = (Curve)this.SelectedElement.GetGeometryObjectFromReference(refXYZ);
                if (curve != null)
                {
                    if (init)
                        thisXYZ = curve.Evaluate(param0, true);
                    else
                    {
                        XYZ curPoint = refXYZ.GlobalPoint;
                        if (thisTrf != null)
                        {
                            Autodesk.Revit.DB.Transform inverseTrf = thisTrf.Inverse;
                            curPoint = inverseTrf.OfPoint(refXYZ.GlobalPoint);
                        }
                        IntersectionResult thisResult = curve.Project(curPoint);
                        param0 = curve.ComputeNormalizedParameter(thisResult.Parameter);
                        init = true;
                    }
                    thisXYZ = curve.Evaluate(param0, true);
                    param1 = -1.0;
                }
                else
                    throw new Exception("could not evaluate point on face or edge of the element");
                if (thisTrf != null)
                    thisXYZ = thisTrf.OfPoint(thisXYZ);
            }
            else 
                throw new Exception ("could not evaluate point on face or edge of the element");
           
            return Value.NewContainer(thisXYZ);
        }

        public override string SelectionText
        {
            get
            {
                return _selectionText = this.refXYZ == null ?
                    "Nothing Selected" :
                    "Point on element" + " (" + refXYZ.ElementId + ")";
            }
            set
            {
                _selectionText = value;
                RaisePropertyChanged("SelectionText");
            }
        }

        public void Draw()
        {
            if (this.RenderDescription == null)
                this.RenderDescription = new RenderDescription();
            else
                this.RenderDescription.ClearAll();

            XYZ thisXYZ = refXYZ.GlobalPoint;

            dynRevitTransactionNode.DrawXYZ(this.RenderDescription, thisXYZ);
        }
        public override void SaveElement(XmlDocument xmlDoc, XmlElement dynEl)
        {

            dynEl.SetAttribute("refXYZ", this.refXYZ.ConvertToStableRepresentation(dynRevitSettings.Doc.Document));
            dynEl.SetAttribute("refXYZparam0", this.param0.ToString());
            dynEl.SetAttribute("refXYZparam1", this.param1.ToString());
        }

        public override void LoadElement(XmlNode elNode)
        {
            try
            {
                this.refXYZ = Reference.ParseFromStableRepresentation(dynRevitSettings.Doc.Document, elNode.Attributes["refXYZ"].Value.ToString());
                if (refXYZ != null)
                {
                    this.SelectedElement = dynRevitSettings.Doc.Document.GetElement(refXYZ.ElementId);
                }
                param0 = Convert.ToDouble(elNode.Attributes["refXYZparam0"].Value);
                param1 = Convert.ToDouble(elNode.Attributes["refXYZparam1"].Value);
                init = true;
            }
            catch { }
        }
    }
}


