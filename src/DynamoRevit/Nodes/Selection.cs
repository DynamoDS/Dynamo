using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Xml;
using Autodesk.Revit.DB;
using Dynamo.Controls;
using Dynamo.FSchemeInterop;
using Dynamo.Models;
using Dynamo.Revit;
using Dynamo.Revit.SyncedNodeExtensions; //Gives the RegisterEval... methods
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using RevitServices.Persistence;
using RevitServices.Threading;
using Value = Dynamo.FScheme.Value;
using TextBox = System.Windows.Controls.TextBox;
using RevThread = RevitServices.Threading;

namespace Dynamo.Nodes
{
    [IsInteractive(true)]
    public abstract class SelectionBase : NodeWithOneOutput
    {
        #region private members
        
        private bool _canSelect = true;
        protected string _selectButtonContent;
        protected string _selectionMessage;
        private Element _selected;
        protected string _selectionText;
        private bool _buttonEnabled;
        
        #endregion

        #region properties

        public bool CanSelect
        {
            get { return _canSelect; }
            set
            {
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
                        });

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

        #endregion

        #region constructors

        protected SelectionBase()
        {
            dynSettings.Controller.DynamoViewModel.PropertyChanged += DynamoViewModel_PropertyChanged;
        }

        protected SelectionBase(PortData outPortData) : this()
        {
            OutPortData.Add(outPortData);
            RegisterAllPorts();

            dynRevitSettings.Controller.RevitDocumentChanged += Controller_RevitDocumentChanged;
        }
        
        #endregion

        void DynamoViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "RunEnabled")
            {
                CanSelect = dynSettings.Controller.DynamoViewModel.RunEnabled;
            }
        }

        void Controller_RevitDocumentChanged(object sender, EventArgs e)
        {
            SelectedElement = null;
        }

        public void SetupCustomUIElements(dynNodeView nodeUI)
        {
            //add a button to the inputGrid on the dynElement
            var selectButton = new DynamoNodeButton
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
            RevThread.IdlePromise.ExecuteOnIdleAsync(
                delegate
                {
                    OnSelectClick();
                    CanSelect = true;
                });
        }

        /// <summary>
        /// Overriden in the derived classes
        /// </summary>
        public virtual void OnSelectClick()
        {
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
                        saved = DocumentManager.Instance.CurrentUIDocument.Document.GetElement(id); // FamilyInstance;
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

    }
}