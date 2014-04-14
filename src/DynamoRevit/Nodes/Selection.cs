using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Xml;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Utilities;
using TextBox = System.Windows.Controls.TextBox;
using RevThread = RevitServices.Threading;

namespace Dynamo.Nodes
{
    /*[IsInteractive(true)]
    public abstract class SelectionBase : NodeModel
    {
        #region private members
        
        private bool canSelect = true;
        private string selectButtonContent;
        private string selected;

        #endregion

        #region properties

        public bool CanSelect
        {
            get { return canSelect; }
            set
            {
                canSelect = value;
                RaisePropertyChanged("CanSelect");
            }
        }

        public string SelectButtonContent
        {
            get { return selectButtonContent; }
            set
            {
                selectButtonContent = value;
                RaisePropertyChanged("SelectButtonContent");
            }
        }
    
        /// <summary>
        /// The Element which is selected. Setting this property will automatically register the Element
        /// for proper updating, and will update this node's IsDirty value.
        /// </summary>
        public virtual string SelectedElement
        {
            get { return selected; }
            set
            {
                bool dirty;
                if (selected != null)
                {
                    if (value != null && value.Equals(selected))
                        return;

                    dirty = true;
                    this.UnregisterEvalOnModified(selected);
                }
                else
                    dirty = value != null;

                selected = value;
                if (value != null)
                {
                    this.RegisterEvalOnModified(
                        value,
                        delAction: delegate
                        {
                            selected = null;
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

        void DynamoViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
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
                Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)),
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

            Grid.SetRow(selectButton, 0);
            Grid.SetRow(tb, 1);

            tb.DataContext = this;
            selectButton.DataContext = this;

            var selectTextBinding = new Binding("SelectionText")
            {
                Mode = BindingMode.TwoWay,
            };
            tb.SetBinding(TextBox.TextProperty, selectTextBinding);

            var buttonTextBinding = new Binding("SelectButtonContent")
            {
                Mode = BindingMode.TwoWay,
            };
            selectButton.SetBinding(ContentControl.ContentProperty, buttonTextBinding);

            var buttonEnabledBinding = new Binding("CanSelect")
            {
                Mode = BindingMode.TwoWay,
            };
            selectButton.SetBinding(UIElement.IsEnabledProperty, buttonEnabledBinding);
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
                outEl.SetAttribute("id", SelectedElement);
                nodeElement.AppendChild(outEl);
            }
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            SelectedElement = (from XmlNode subNode in nodeElement.ChildNodes
                               where subNode.Name.Equals("instance")
                               let xmlAttributeCollection = subNode.Attributes
                               where xmlAttributeCollection != null
                               select xmlAttributeCollection[0].Value).Last();
        }
    }*/
}