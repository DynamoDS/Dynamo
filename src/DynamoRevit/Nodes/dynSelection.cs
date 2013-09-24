﻿//Copyright © Autodesk, Inc. 2012. All rights reserved.
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
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Linq;
using System.Xml;
using Autodesk.Revit.DB;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Utilities;
using Dynamo.Revit;
using Dynamo.Revit.SyncedNodeExtensions; //Gives the RegisterEval... methods
using Microsoft.FSharp.Collections;
using Value = Dynamo.FScheme.Value;
using Dynamo.FSchemeInterop;
using TextBox = System.Windows.Controls.TextBox;

namespace Dynamo.Nodes
{
    [IsInteractive(true)]
    public abstract class SelectionBase : NodeWithOneOutput
    {
        private bool _canSelect = true;
        protected string _selectButtonContent;
        protected string _selectionMessage;
        private Element _selected;
        protected string _selectionText;

        public bool CanSelect
        {
            get { return _canSelect; }
            set { 
                _canSelect = value;
                RaisePropertyChanged("CanSelect");
            }
        }
        
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
        /// The Element which is selected. Setting this property will automatically register the Element
        /// for proper updating, and will update this node's IsDirty value.
        /// </summary>
        public virtual Element SelectedElement
        {
            get { return _selected; }
            set
            {
                bool dirty;
                if (_selected != null)
                {
                    if (value != null && value.Id.Equals(_selected.Id))
                        return;

                    dirty = true;
                    this.UnregisterEvalOnModified(_selected.Id);
                }
                else
                    dirty = value != null;

                _selected = value;
                if (value != null)
                {
                    this.RegisterEvalOnModified(
                        value.Id,
                        delAction: delegate
                        {
                            _selected = null;
                            SelectedElement = null;
                        }
                        );

                    SelectButtonContent = "Change";
                }
                else
                {
                    SelectionText = "Nothing Selected";
                    SelectButtonContent = "Select";
                }

                if (dirty)
                    RequiresRecalc = true;
            }
        }
        
        /// <summary>
        /// Determines what the text should read on the node when the selection has been changed.
        /// Is ignored in the case where nothing is selected.
        /// </summary>
        public abstract string SelectionText { get; set; }

        protected SelectionBase(PortData outPortData)
        {
            OutPortData.Add(outPortData);
            RegisterAllPorts();
        }

        public override void SetupCustomUIElements(object ui)
        {
            var nodeUI = ui as dynNodeView;

            //add a button to the inputGrid on the dynElement
            var selectButton = new dynNodeButton
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center
            };
            selectButton.Click += selectButton_Click;

            var tb = new TextBox
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0)),
                BorderThickness = new Thickness(0),
                IsReadOnly = true,
                IsReadOnlyCaretVisible = false
            };

            //tb.Text = "Nothing Selected";
            if (SelectedElement == null || !SelectionText.Any() || !SelectButtonContent.Any())
            {
                SelectionText = "Nothing Selected";
                SelectButtonContent = "Select Instance";
            }

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
            selectButton.SetBinding(ContentControl.ContentProperty, buttonTextBinding);

            var buttonEnabledBinding = new System.Windows.Data.Binding("CanSelect")
            {
                Mode = BindingMode.TwoWay,
            };
            selectButton.SetBinding(Button.IsEnabledProperty, buttonEnabledBinding);
        }

        private void selectButton_Click(object sender, RoutedEventArgs e)
        {
            CanSelect = false;
            IdlePromise.ExecuteOnIdle(
                delegate
                {
                    OnSelectClick();
                    CanSelect = true;
                });
        }

        public virtual void OnSelectClick()
        {
            //this method calls a different selection action in the derived classes.
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            if (SelectedElement == null)
            {
                throw new Exception("Nothing selected.");
            }

            return Value.NewContainer(SelectedElement);
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            if (SelectedElement != null)
            {
                XmlElement outEl = xmlDoc.CreateElement("instance");
                outEl.SetAttribute("id", SelectedElement.UniqueId);
                nodeElement.AppendChild(outEl);
            }
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            foreach (XmlNode subNode in nodeElement.ChildNodes)
            {
                if (subNode.Name.Equals("instance"))
                {
                    Element saved = null;
                    var id = subNode.Attributes[0].Value;
                    try
                    {
                        saved = dynRevitSettings.Doc.Document.GetElement(id); // FamilyInstance;
                    }
                    catch
                    {
                        DynamoLogger.Instance.Log(
                            "Unable to find element with ID: " + id);
                    }
                    SelectedElement = saved;
                }
            }
        }

        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context); //Base implementation must be called
            if (context == SaveContext.Undo)
            {
                XmlDocument xmlDoc = element.OwnerDocument;
                XmlElement outEl = xmlDoc.CreateElement("instance");
                outEl.SetAttribute("id", SelectedElement.UniqueId);
                element.AppendChild(outEl);
            }
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context); //Base implementation must be called
            if (context == SaveContext.Undo)
            {
                XmlElement subNode = element.SelectSingleNode("instance") as XmlElement;
                if (subNode == null)
                    return;
                Element saved = null;
                var id = subNode.Attributes[0].Value;
                try
                {
                    saved = dynRevitSettings.Doc.Document.GetElement(id); // FamilyInstance;
                }
                catch
                {
                    DynamoLogger.Instance.Log(
                        "Unable to find element with ID: " + id);
                }
                SelectedElement = saved;
            }
        }

        #endregion
    }

    public abstract class ElementSelectionBase : SelectionBase
    {
        protected Func<string, Element> _selectionAction;

        protected ElementSelectionBase(PortData outPortData) : base(outPortData){}

        /// <summary>
        /// Callback for when the "Select" button has been clicked.
        /// </summary>
        public override void OnSelectClick()
        {
            try
            {
                SelectedElement = _selectionAction(_selectionMessage);
                RaisePropertyChanged("SelectionText");
                RequiresRecalc = true;
            }
            catch (OperationCanceledException)
            {
                CanSelect = true;
            }
            catch (Exception e)
            {
                DynamoLogger.Instance.Log(e);
            }
        }
    }

    [IsInteractive(true)]
    public abstract class ReferenceSelectionBase : SelectionBase
    {
        protected Func<string, Reference> _selectionAction;
        protected Reference _reference;

        protected ReferenceSelectionBase(PortData outPortData):base(outPortData){}

        /// <summary>
        /// Callback for when the "Select" button has been clicked.
        /// </summary>
        public override void OnSelectClick()
        {
            try
            {
                _reference = _selectionAction(_selectionMessage);
                if (_reference != null)
                    SelectedElement = dynRevitSettings.Doc.Document.GetElement(_reference.ElementId);
                RaisePropertyChanged("SelectionText");
                RequiresRecalc = true;
            }
            catch (OperationCanceledException)
            {
                CanSelect = true;
            }
            catch (Exception e)
            {
                DynamoLogger.Instance.Log(e);
            }
        }
    }

    [IsInteractive(true)]
    public abstract class MultipleElementSelectionBase : NodeWithOneOutput
    {
        private TextBlock _tb;
        private Button _selectButton;

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

        protected MultipleElementSelectionBase(PortData outData)
        {
            OutPortData.Add(outData);
            RegisterAllPorts();
        }

        public override void SetupCustomUIElements(object ui)
        {
            var nodeUI = ui as dynNodeView;

            //add a button to the inputGrid on the dynElement
            _selectButton = new dynNodeButton
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center
            };
            _selectButton.Click += selectButton_Click;

            _tb = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0)),
                TextWrapping = TextWrapping.Wrap,
                TextTrimming = TextTrimming.WordEllipsis,
                MaxWidth = 200,
                MaxHeight = 100
            };

            if (SelectedElements == null || !SelectedElements.Any() || !SelectionText.Any() || !SelectButtonContent.Any())
            {
                SelectionText = "Nothing Selected";
                SelectButtonContent = "Select Instances";
            }


            nodeUI.inputGrid.RowDefinitions.Add(new RowDefinition());
            nodeUI.inputGrid.RowDefinitions.Add(new RowDefinition());

            nodeUI.inputGrid.Children.Add(_tb);
            nodeUI.inputGrid.Children.Add(_selectButton);

            System.Windows.Controls.Grid.SetRow(_selectButton, 0);
            System.Windows.Controls.Grid.SetRow(_tb, 1);

            _tb.DataContext = this;
            _selectButton.DataContext = this;

            var selectTextBinding = new System.Windows.Data.Binding("SelectionText")
            {
                Mode = BindingMode.TwoWay,
            };
            _tb.SetBinding(TextBlock.TextProperty, selectTextBinding);

            var buttonTextBinding = new System.Windows.Data.Binding("SelectButtonContent")
            {
                Mode = BindingMode.TwoWay,
            };
            _selectButton.SetBinding(ContentControl.ContentProperty, buttonTextBinding);
        }

        private void selectButton_Click(object sender, RoutedEventArgs e)
        {
            _selectButton.IsEnabled = false;
            IdlePromise.ExecuteOnIdle(
                delegate
                {
                    OnSelectClick();
                    _selectButton.IsEnabled = true;
                });
        }

        /// <summary>
        /// Callback for when the "Select" button has been clicked.
        /// </summary>
        /// 
        /// 
        protected abstract void OnSelectClick();

        private IList<Element> _selected;

        /// <summary>
        /// The Element which is selected. Setting this property will automatically register the Element
        /// for proper updating, and will update this node's IsDirty value.
        /// </summary>
        public virtual IList<Element> SelectedElements
        {
            get { return _selected; }
            set
            {
                var dirty = false;
                if (_selected != null)
                {
                    foreach (Element selectedElement in _selected)
                    {
                        foreach (Element previousElement in value)
                        {
                            if (previousElement != null
                                && previousElement.Id.Equals(selectedElement.Id))
                                return;

                            dirty = true;
                            this.UnregisterEvalOnModified(selectedElement.Id);
                        }
                    }
                }
                else
                    dirty = value != null;

                _selected = value;
                if (value != null)
                {
                    foreach (Element previousElement in value)
                    {
                        this.RegisterEvalOnModified(
                            previousElement.Id,
                            delAction: delegate
                            {
                                _selected = null;
                                SelectedElements = null;
                            });
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
                    RequiresRecalc = true;
            }
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            if (SelectedElements == null)
                throw new Exception("Nothing selected.");

            var els = SelectedElements.Select(Value.NewContainer).ToList();

            return Value.NewList(Utils.SequenceToFSharpList(els));
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            if (SelectedElements != null)
            {
                foreach (Element selectedElement in SelectedElements)
                {
                    XmlElement outEl = xmlDoc.CreateElement("instance");
                    outEl.SetAttribute("id", selectedElement.UniqueId);
                    nodeElement.AppendChild(outEl);
                }
            }
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            foreach (XmlNode subNode in nodeElement.ChildNodes)
            {
                if (subNode.Name.Equals("instance"))
                {
                    Element saved = null;
                    var id = subNode.Attributes[0].Value;
                    try
                    {
                        saved = dynRevitSettings.Doc.Document.GetElement(id);
                    }
                    catch
                    {
                        DynamoLogger.Instance.Log(
                            "Unable to find element with ID: " + id);
                    }
                    if (SelectedElements == null)
                        SelectedElements = new List<Element>();
                    SelectedElements.Add(saved);
                }
            }
        }

        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context); //Base implementation must be called
            if (context == SaveContext.Undo)
            {
                XmlDocument xmlDoc = element.OwnerDocument;
                if (SelectedElements != null)
                {
                    foreach (Element selectedElement in SelectedElements)
                    {
                        XmlElement outEl = xmlDoc.CreateElement("instance");
                        outEl.SetAttribute("id", selectedElement.UniqueId);
                        element.AppendChild(outEl);
                    }
                }
            }
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context); //Base implementation must be called
            if (context == SaveContext.Undo)
            {
                XmlNodeList instanceNodes = element.SelectNodes("instance");
                foreach (XmlNode subNode in instanceNodes)
                {
                    Element saved = null;
                    var id = subNode.Attributes[0].Value;
                    try
                    {
                        saved = dynRevitSettings.Doc.Document.GetElement(id);
                    }
                    catch
                    {
                        DynamoLogger.Instance.Log(
                            "Unable to find element with ID: " + id);
                    }
                    if (SelectedElements == null)
                        SelectedElements = new List<Element>();
                    SelectedElements.Add(saved);
                }
            }
        }

        #endregion
    }

    [NodeName("Select Family Instance")]
    [NodeCategory(BuiltinNodeCategories.CORE_SELECTION)]
    [NodeDescription("Select a family instance from the document.")]
    public class FamilyInstanceCreatorSelection : ElementSelectionBase
    {
        public FamilyInstanceCreatorSelection()
            : base(
                new PortData(
                    "fi", "Family instances created by this operation.", typeof (Value.Container)))
        {
            _selectionMessage = "Select Family Instance";
            _selectionAction = dynRevitSettings.SelectionHelper.RequestFamilyInstanceSelection;
        }

        //protected override void OnSelectClick()
        //{
        //    try
        //    {
        //        SelectedElement = dynRevitSettings.SelectionHelper.RequestFamilyInstanceSelection("Select Massing Family Instance");
        //        RaisePropertyChanged("SelectionText");
        //        RequiresRecalc = true;
        //    }
        //    catch (OperationCanceledException cancelEx)
        //    {
        //        CanSelect = true;
        //    }
        //    catch (Exception e)
        //    {
        //        DynamoLogger.Instance.Log(e);
        //    }
        //}

        public override string SelectionText
        {
            get
            {
                return _selectionText = SelectedElement == null
                                            ? "Nothing Selected"
                                            : SelectedElement.Name;
            }
            set
            {
                _selectionText = value;
                RaisePropertyChanged("SelectionText");
            }
        }
    }

    [NodeName("Select Divided Surface Families")]
    [NodeCategory(BuiltinNodeCategories.CORE_SELECTION)]
    [NodeDescription("Select a all families on a divided surface by picking the underlying form.")]
    [NodeSearchTags("Curtain Panel", "Divided", "surface", "component", "family")]
    public class DividedSurfaceBySelection : ElementSelectionBase
    {
        private Value _data;

        public DividedSurfaceBySelection()
            : base(
                new PortData(
                    "fi", "The divided surface family instances", typeof (Value.Container)))
        {
            _selectionMessage = "Select a divided surface.";
            _selectionAction = dynRevitSettings.SelectionHelper.RequestFormSelection;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var result = new List<List<FamilyInstance>>();

            //"Get an interface to the divided surfaces on this element."
            //TODO: do we want to select a face instead and try to get
            //the divided surface that way?
            DividedSurfaceData dsd = SelectedElement.GetDividedSurfaceData();

            if(dsd == null)
                throw new Exception("The selected form has no divided surface data.");

            foreach (Reference r in dsd.GetReferencesWithDividedSurfaces())
            {
                Autodesk.Revit.DB.DividedSurface ds = dsd.GetDividedSurfaceForReference(r);

                var gn = new GridNode();

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
                    if (lst.Any())
                        result.Add(lst);

                    u = u + 1;
                }
            }

            _data = Value.NewList(
                Utils.SequenceToFSharpList(
                    result.Select(
                        row => Value.NewList(
                            Utils.SequenceToFSharpList(
                                row.Select(Value.NewContainer))))));

            return _data;
        }

        public override string SelectionText
        {
            get
            {
                return _selectionText = SelectedElement == null
                                            ? "Nothing Selected"
                                            : "Element ID: " + SelectedElement.Id;
            }
            set
            {
                _selectionText = value;
                RaisePropertyChanged("SelectionText");
            }
        }

        //protected override void OnSelectClick()
        //{
        //    SelectedElement = null;

        //    try
        //    {
        //        SelectedElement = dynRevitSettings.SelectionHelper.RequestFormSelection(
        //            dynRevitSettings.Doc, "Select a form element.");
        //        RaisePropertyChanged("SelectionText");
        //        RequiresRecalc = true;
        //    }
        //    catch (OperationCanceledException)
        //    {
        //        CanSelect = true;
        //    }
        //    catch (Exception e)
        //    {
        //        DynamoLogger.Instance.Log(e);
        //    }
            
        //}
    }

    [NodeName("Select Face")]
    [NodeCategory(BuiltinNodeCategories.CORE_SELECTION)]
    [NodeDescription("Select a face from the document.")]
    public class FormElementBySelection : ReferenceSelectionBase, IDrawable
    {
        public RenderDescription RenderDescription { get; set; }

        public FormElementBySelection()
            : base(new PortData("face", "The face", typeof (Value.Container)))
        {
            _selectionMessage = "Select a face.";
            _selectionAction = dynRevitSettings.SelectionHelper.RequestFaceReferenceSelection;
        }

        //protected override void OnSelectClick()
        //{
        //    var doc = dynRevitSettings.Doc;

        //    try
        //    {
        //        _f = dynRevitSettings.SelectionHelper.RequestFaceReferenceSelection(
        //            doc, "Select a face.");
        //        SelectedElement = doc.Document.GetElement(_f);
        //        RaisePropertyChanged("SelectionText");
        //        RequiresRecalc = true;
        //    }
        //    catch (OperationCanceledException cancelEx)
        //    {
        //        CanSelect = true;
        //    }
        //    catch (Exception e)
        //    {
        //        DynamoLogger.Instance.Log(e);
        //    }
        //}

        public override Value Evaluate(FSharpList<Value> args)
        {
            var opts = new Options { ComputeReferences = true };

            var face =
                (Autodesk.Revit.DB.Face)dynRevitSettings.Doc.Document.GetElement(_reference).GetGeometryObjectFromReference(_reference);

            //TODO: Is there a better way to get a face that has a reference?
            foreach (GeometryObject geob in dynRevitSettings.Doc.Document.GetElement(_reference).get_Geometry(opts))
            {
                if (FindFaceInGeometryObject(geob, ref face))
                    break;
            }

            return Value.NewContainer(face);
        }

        private static bool FindFaceInGeometryObject(GeometryObject geob, ref Face face)
        {
            var instance = geob as GeometryInstance;
            if (instance != null)
            {
                foreach (var geob_inner in instance.GetInstanceGeometry())
                {
                    FindFaceInGeometryObject(geob_inner, ref face);
                }
            }
            else
            {
                var solid = geob as Solid;
                if (solid != null)
                {
                    foreach (Face f in solid.Faces)
                    {
                        if (f == face)
                        {
                            face = f;
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public override string SelectionText
        {
            get
            {
                return _selectionText = SelectedElement == null
                                            ? "Nothing Selected"
                                            : "Face ID: " + SelectedElement.Id;
            }
            set
            {
                _selectionText = value;
                RaisePropertyChanged("SelectionText");
            }
        }

        public void Draw()
        {
            if (RenderDescription == null)
                RenderDescription = new RenderDescription();
            else
                RenderDescription.ClearAll();

            var face =
                (Autodesk.Revit.DB.Face)dynRevitSettings.Doc.Document.GetElement(_reference).GetGeometryObjectFromReference(_reference);

            RevitTransactionNode.DrawFace(RenderDescription, face);
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            if(_reference != null)
                nodeElement.SetAttribute(
                    "faceRef", _reference.ConvertToStableRepresentation(dynRevitSettings.Doc.Document));
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            try
            {
                _reference = Reference.ParseFromStableRepresentation(
                    dynRevitSettings.Doc.Document, nodeElement.Attributes["faceRef"].Value);
                if (_reference != null)
                    SelectedElement = dynRevitSettings.Doc.Document.GetElement(_reference.ElementId);
            }
            catch { }
        }
    }

    [NodeName("Select Edge")]
    [NodeCategory(BuiltinNodeCategories.CORE_SELECTION)]
    [NodeDescription("Select an edge from the document.")]
    public class EdgeOnElementBySelection : ReferenceSelectionBase, IDrawable
    {
        public RenderDescription RenderDescription { get; set; }

        public EdgeOnElementBySelection()
            : base(new PortData("edge", "The edge", typeof (Value.Container)))
        {
            _selectionMessage = "Select an edge.";
            _selectionAction = dynRevitSettings.SelectionHelper.RequestEdgeReferenceSelection;
        }

        //protected override void OnSelectClick()
        //{
        //    try
        //    {
        //        var doc = dynRevitSettings.Doc;

        //        _f = dynRevitSettings.SelectionHelper.RequestEdgeReferenceSelection(
        //            doc, "Select an edge.");
        //        SelectedElement = doc.Document.GetElement(_f);
        //        RaisePropertyChanged("SelectionText");
        //        RequiresRecalc = true;
        //    }
        //    catch (OperationCanceledException cancelEx)
        //    {
        //        CanSelect = true;
        //    }
        //    catch (Exception e)
        //    {
        //        dynRevitSettings.Controller.DynamoViewModel.Log(e);
        //    } 
        //}

        public override Value Evaluate(FSharpList<Value> args)
        {
            return Value.NewContainer(_reference);
        }

        public override string SelectionText
        {
            get
            {
                return _selectionText = SelectedElement == null
                                            ? "Nothing Selected"
                                            : "Element of Edge  ID: " + SelectedElement.Id;
            }
            set
            {
                _selectionText = value;
                RaisePropertyChanged("SelectionText");
            }
        }

        public void Draw()
        {
            if (RenderDescription == null)
                RenderDescription = new RenderDescription();
            else
                RenderDescription.ClearAll();

            var edge =
                (Edge)dynRevitSettings.Doc.Document.GetElement(_reference).GetGeometryObjectFromReference(_reference);

            RevitTransactionNode.DrawGeometryElement(RenderDescription, edge);
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            if(_reference != null)
                nodeElement.SetAttribute(
                    "edgeRef", _reference.ConvertToStableRepresentation(dynRevitSettings.Doc.Document));
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            try
            {
                _reference = Reference.ParseFromStableRepresentation(
                    dynRevitSettings.Doc.Document, nodeElement.Attributes["edgeRef"].Value);
                if (_reference != null)
                    SelectedElement = dynRevitSettings.Doc.Document.GetElement(_reference.ElementId);
            }
            catch { }
        }
    }

    [NodeName("Select Curve")]
    [NodeCategory(BuiltinNodeCategories.CORE_SELECTION)]
    [NodeDescription("Select a curve from the document.")] //or set of curves in the future
    public class CurvesBySelection : ElementSelectionBase, IDrawable
    {
        public CurvesBySelection()
            : base(new PortData("curve", "The curve", typeof (Value.Container)))
        {
            _selectionMessage = "Select a curve.";
            _selectionAction = dynRevitSettings.SelectionHelper.RequestCurveElementSelection;
        }

        //protected override void OnSelectClick()
        //{
        //    try
        //    {
        //        SelectedElement = dynRevitSettings.SelectionHelper.RequestCurveElementSelection(
        //            dynRevitSettings.Doc, "Select a curve.");
        //        RaisePropertyChanged("SelectionText");
        //        RequiresRecalc = true;
        //    }
        //    catch (OperationCanceledException cancelEx)
        //    {
        //        CanSelect = true;
        //    }
        //    catch (Exception e)
        //    {
        //        DynamoLogger.Instance.Log(e);
        //    }
        //}

        public override string SelectionText
        {
            get
            {
                return _selectionText = SelectedElement == null
                                            ? "Nothing Selected"
                                            : "Curve ID: " + SelectedElement.Id;
            }
            set
            {
                _selectionText = value;
                RaisePropertyChanged("SelectionText");
            }
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            foreach (XmlNode subNode in nodeElement.ChildNodes)
            {
                if (subNode.Name.Equals("instance"))
                {
                    Element saved = null;
                    var id = subNode.Attributes[0].Value;
                    try
                    {
                        saved = dynRevitSettings.Doc.Document.GetElement(id);
                            // FamilyInstance;
                    }
                    catch
                    {
                        DynamoLogger.Instance.Log(
                            "Unable to find element with ID: " + id);
                    }

                    //only set the selected element if the element
                    //returned is of the type required by this node
                    if (saved is CurveElement)
                        SelectedElement = saved;
                }
            }
        }

        #region IDrawable Interface

        public RenderDescription RenderDescription { get; set; }

        public void Draw()
        {
            if (RenderDescription == null)
                RenderDescription = new RenderDescription();
            else
                RenderDescription.ClearAll();

            CurveElement ce = SelectedElement as CurveElement;

            if (ce == null)
                return;

            RevitTransactionNode.DrawCurve(RenderDescription, ce.GeometryCurve);
        }

        #endregion
    }

    [NodeName("Select Elements")]
    [NodeCategory(BuiltinNodeCategories.CORE_SELECTION)]
    [NodeDescription("Box select a collection of Revit Elements from the document.")]
    public class MultipleCurvesBySelection : MultipleElementSelectionBase
    {
        public MultipleCurvesBySelection()
            : base(new PortData("Elements", "The Elements", typeof(Value.Container))) { }

        protected override void OnSelectClick()
        {
            if (SelectedElements != null)
            {
                SelectedElements.Clear();
                SelectedElements = null;
            }

            SelectedElements = dynRevitSettings.SelectionHelper
                                               .RequestMultipleCurveElementsSelection("Select a set of Revit Elements.");

            RaisePropertyChanged("SelectionText");
        }

        private static string formatSelectionText(IEnumerable<Element> elements)
        {
            return elements.Any() 
                ? System.String.Join(" ", elements.Select(x => x.Id.ToString())) 
                : "Nothing Selected";
        }

        public override string SelectionText
        {
            get
            {
                return _selectionText = (SelectedElements != null && SelectedElements.Count > 0)
                                            ? "Element IDs:" + formatSelectionText(SelectedElements.Where(x => x != null && x.Id != null))
                                            : "Nothing Selected";
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
    public class PointBySelection : ElementSelectionBase
    {
        public PointBySelection() :
            base(new PortData("pt", "The point", typeof (Value.Container)))
        {
            _selectionMessage = "Select a reference point.";
            _selectionAction = dynRevitSettings.SelectionHelper.RequestReferencePointSelection;
        }

        //protected override void OnSelectClick()
        //{
        //    try
        //    {
        //        SelectedElement = dynRevitSettings.SelectionHelper.RequestReferencePointSelection(
        //            dynRevitSettings.Doc, "Select a reference point.");
        //        RaisePropertyChanged("SelectionText");
        //        RequiresRecalc = true;
        //    }
        //    catch (OperationCanceledException cancelEx)
        //    {
        //        CanSelect = true;
        //    }
        //    catch (Exception e)
        //    {
        //        DynamoLogger.Instance.Log(e);
        //    }
            
        //}

        public override string SelectionText
        {
            get
            {
                return _selectionText = SelectedElement == null
                                            ? "Nothing Selected"
                                            : SelectedElement.Name + " ("
                                              + SelectedElement.Id + ")";
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
    public class LevelBySelection : ElementSelectionBase
    {
        public LevelBySelection() :
            base(new PortData("lvl", "The selected level", typeof (Value.Container)))
        {
            _selectionMessage = "Select a level.";
            _selectionAction = dynRevitSettings.SelectionHelper.RequestLevelSelection;
        }

        //protected override void OnSelectClick()
        //{
        //    try
        //    {
        //        SelectedElement = dynRevitSettings.SelectionHelper.RequestLevelSelection(
        //            dynRevitSettings.Doc, "Select a level.");
        //        RaisePropertyChanged("SelectionText");
        //        RequiresRecalc = true;
        //    }
        //    catch (OperationCanceledException cancelEx)
        //    {
        //        CanSelect = true;
        //    }
        //    catch (Exception e)
        //    {
        //        dynRevitSettings.Controller.DynamoViewModel.Log(e);
        //    }
            
        //}

        public override string SelectionText
        {
            get
            {
                return _selectionText = SelectedElement == null
                                            ? "Nothing Selected"
                                            : SelectedElement.Name + " ("
                                              + SelectedElement.Id + ")";
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
    public class ModelElementSelection : ElementSelectionBase
    {
        public ModelElementSelection()
            : base(
                new PortData(
                    "me", "Model element reference created by this operation.",
                    typeof (Value.Container)))
        {
            _selectionMessage = "Select Model Element";
            _selectionAction = dynRevitSettings.SelectionHelper.RequestModelElementSelection;
        }

        //protected override void OnSelectClick()
        //{
        //    SelectedElement = dynRevitSettings.SelectionHelper.RequestModelElementSelection(
        //        dynRevitSettings.Doc, "Select Model Element");
        //    RaisePropertyChanged("SelectionText");
        //}

        public override string SelectionText
        {
            get
            {
                return _selectionText = SelectedElement == null
                                            ? "Nothing Selected"
                                            : SelectedElement.Name;
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
    public class XyzBySelection : ReferenceSelectionBase, IDrawable
    {
        private Reference old_refXyz;
        private double _param0;
        private double _param1;
        private bool _init;

        public RenderDescription RenderDescription { get; set; }

        public XyzBySelection() :
            base(new PortData("XYZ", "The XYZ location on element", typeof (Value.Container)))
        {
            _selectionMessage = "Select a XYZ location on face or edge of the element.";
            _selectionAction = dynRevitSettings.SelectionHelper.RequestReferenceXYZSelection;
            old_refXyz = null;
        }

        //protected override void OnSelectClick()
        //{
        //    _refXyz = dynRevitSettings.SelectionHelper.RequestReferenceXYZSelection(
        //        dynRevitSettings.Doc, "Select a XYZ location on face or edge of the element."
        //        );
        //    if (_refXyz != null)
        //        SelectedElement = dynRevitSettings.Doc.Document.GetElement(_refXyz.ElementId);
        //    _init = false;

        //    RaisePropertyChanged("SelectionText");
        //}

        public override Value Evaluate(FSharpList<Value> args)
        {
            if (_reference.ElementReferenceType != ElementReferenceType.REFERENCE_TYPE_SURFACE &&
                _reference.ElementReferenceType != ElementReferenceType.REFERENCE_TYPE_LINEAR )
            {
                ElementId refElementId = _reference.ElementId;
                Element refElement = dynRevitSettings.Doc.Document.GetElement(refElementId);
                if (refElement is ReferencePoint)
                {
                    ReferencePoint rp = refElement as ReferencePoint;
                    XYZ rpXYZ = rp.Position;
                    return Value.NewContainer(rpXYZ);
                }
                GeometryObject thisObjectPoint = SelectedElement.GetGeometryObjectFromReference(_reference);
                if (!(thisObjectPoint is Autodesk.Revit.DB.Point))
                    throw new Exception("Could not use face or edge which is not part of the model");
                var thisPoint = thisObjectPoint as Autodesk.Revit.DB.Point;
                XYZ pointXYZ = thisPoint.Coord;
                return Value.NewContainer(pointXYZ);
            }

            GeometryObject thisObject = SelectedElement.GetGeometryObjectFromReference(_reference);
            Autodesk.Revit.DB.Transform thisTrf = null;
            if (_init && (old_refXyz == null || !_reference.Equals(old_refXyz)))
                _init = false;

            {
                GeometryObject geomObj =
                    SelectedElement.get_Geometry(new Options());
                var geomElement = geomObj as GeometryElement;

                // ugly code to detect if transform for geometry object is needed or not
                // filed request to provide this info via API, but meanwhile ...
                foreach (GeometryObject geob in geomElement)
                {
                    if (!(geob is GeometryInstance))
                        continue;

                    var ginsta = geob as GeometryInstance;
                    GeometryElement gSymbolElement = ginsta.GetSymbolGeometry();
                    var geometryElements = new List<GeometryElement> { gSymbolElement };
                    bool found = false;
                    for (; geometryElements.Count > 0 && !found;)
                    {
                        GeometryElement thisGeometryElement = geometryElements[0];
                        geometryElements.Remove(thisGeometryElement);

                        foreach (GeometryObject geobSym in thisGeometryElement)
                        {
                            if (geobSym is GeometryElement)
                            {
                                geometryElements.Add(geobSym as GeometryElement);
                                continue;
                            }
                            if ((thisObject is Curve) && (geobSym is Curve)
                                && (thisObject == geobSym))
                            {
                                found = true;
                                break;
                            }

                            if (thisObject is Curve)
                                continue;

                            if ((thisObject is Autodesk.Revit.DB.Face) && (geobSym is Autodesk.Revit.DB.Face) && (thisObject == geobSym))
                            {
                                found = true;
                                break;
                            }

                            if ((thisObject is Edge) && (geobSym is Autodesk.Revit.DB.Face))
                            {
                                var edge = thisObject as Edge;
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
                                Autodesk.Revit.DB.Face faceAt = solidFaces.get_Item(index);
                                if ((thisObject is Autodesk.Revit.DB.Face) && (thisObject == faceAt))
                                {
                                    found = true;
                                    break;
                                }
                                if (thisObject is Edge)
                                {
                                    var edge = thisObject as Edge;
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

            XYZ thisXYZ;

            if (_reference.ElementReferenceType == ElementReferenceType.REFERENCE_TYPE_SURFACE
                && thisObject is Autodesk.Revit.DB.Face)
            {
                var face = thisObject as Autodesk.Revit.DB.Face;
                if (!_init)
                {
                    _param0 = _reference.UVPoint[0];
                    _param1 = _reference.UVPoint[1];
                    _init = true;
                }
                var uv = new UV(_param0, _param1);
                thisXYZ = face.Evaluate(uv);
                if (thisTrf != null)
                    thisXYZ = thisTrf.OfPoint(thisXYZ);
            }
            else if (_reference.ElementReferenceType == ElementReferenceType.REFERENCE_TYPE_LINEAR)
            {
                Curve curve;
                if (thisObject is Edge)
                {
                    var edge = (Edge)SelectedElement.GetGeometryObjectFromReference(_reference);
                    curve = edge.AsCurve();
                }
                else
                    curve = (Curve)SelectedElement.GetGeometryObjectFromReference(_reference);
                if (curve != null)
                {
                    if (_init)
                        thisXYZ = curve.Evaluate(_param0, true);
                    else
                    {
                        XYZ curPoint = _reference.GlobalPoint;
                        if (thisTrf != null)
                        {
                            Autodesk.Revit.DB.Transform inverseTrf = thisTrf.Inverse;
                            curPoint = inverseTrf.OfPoint(_reference.GlobalPoint);
                        }
                        IntersectionResult thisResult = curve.Project(curPoint);
                        _param0 = curve.ComputeNormalizedParameter(thisResult.Parameter);
                        _init = true;
                    }
                    thisXYZ = curve.Evaluate(_param0, true);
                    _param1 = -1.0;
                }
                else
                    throw new Exception("could not evaluate point on face or edge of the element");
                if (thisTrf != null)
                    thisXYZ = thisTrf.OfPoint(thisXYZ);
            }
            else
                throw new Exception("could not evaluate point on face or edge of the element");

            old_refXyz = _reference;
            return Value.NewContainer(thisXYZ);
        }

        public override string SelectionText
        {
            get
            {
                return _selectionText = _reference == null
                                            ? "Nothing Selected"
                                            : "Point on element" + " (" + _reference.ElementId + ")";
            }
            set
            {
                _selectionText = value;
                RaisePropertyChanged("SelectionText");
            }
        }

        public void Draw()
        {
            if (RenderDescription == null)
                RenderDescription = new RenderDescription();
            else
                RenderDescription.ClearAll();

            XYZ thisXYZ = _reference.GlobalPoint;

            RevitTransactionNode.DrawXYZ(RenderDescription, thisXYZ);
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            if(_reference != null)
                nodeElement.SetAttribute(
                    "refXYZ", _reference.ConvertToStableRepresentation(dynRevitSettings.Doc.Document));
                nodeElement.SetAttribute("refXYZparam0", _param0.ToString(CultureInfo.InvariantCulture));
                nodeElement.SetAttribute("refXYZparam1", _param1.ToString(CultureInfo.InvariantCulture));
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            try
            {
                _reference = Reference.ParseFromStableRepresentation(
                    dynRevitSettings.Doc.Document, nodeElement.Attributes["refXYZ"].Value);
                if (_reference != null)
                    SelectedElement = dynRevitSettings.Doc.Document.GetElement(
                        _reference.ElementId);
                _param0 = Convert.ToDouble(nodeElement.Attributes["refXYZparam0"].Value);
                _param1 = Convert.ToDouble(nodeElement.Attributes["refXYZparam1"].Value);
                old_refXyz = _reference;
                _init = true;
            }
            catch { }
        }
    }
}