﻿using Dynamo.Nodes;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using DynCmd = Dynamo.Models.DynamoModel;

namespace Dynamo.UI.Controls
{
    /// <summary>
    /// Interaction logic for CodeBlockEditor.xaml
    /// </summary>
    public partial class CodeBlockEditor : UserControl
    {
        private NodeViewModel nodeViewModel;
        private DynamoViewModel dynamoViewModel;

        public CodeBlockEditor()
        {
            InitializeComponent();
        }

        public CodeBlockEditor(NodeViewModel nodeViewModel)
        {
            InitializeComponent();

            this.nodeViewModel = nodeViewModel;
            this.dynamoViewModel = nodeViewModel.DynamoViewModel;
            this.DataContext = nodeViewModel.NodeModel;

            // Register text editing events
            this.InnerTextEditor.TextChanged += InnerTextEditor_TextChanged;
            this.InnerTextEditor.TextArea.LostFocus += TextArea_LostFocus;
            this.Loaded += (obj, args) => this.InnerTextEditor.TextArea.Focus();

            InitializeSyntaxHighlighter();
        }

        #region Generic Properties
        internal TextEditor InternalEditor
        {
            get { return this.InnerTextEditor; }
        }

        public string Code
        {
            get
            {
                // Since this property a one way binding from source (CodeBlockNodeModel) to 
                // target (this class), the getter should never be called
                throw new NotImplementedException();

            }
            set
            {
                this.InnerTextEditor.Text = value;
            }
        }
        #endregion

        internal bool Focus()
        {
            return InternalEditor.Focus();
        }

        #region Dependency Property
        public static readonly DependencyProperty CodeProperty = DependencyProperty.Register("Code", typeof(string),
            typeof(CodeBlockEditor), new PropertyMetadata((obj, args) =>
            {
                var target = (CodeBlockEditor)obj;
                target.Code = (string)args.NewValue;
            })
        );
        #endregion

        #region Generic Event Handlers
        /// <summary>
        /// Called when the CBN is committed and the underlying source data 
        /// needs to be updated with the text typed in the CBN
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TextArea_LostFocus(object sender, RoutedEventArgs e)
        {
            this.InnerTextEditor.TextArea.ClearSelection();

            this.nodeViewModel.DynamoViewModel.ExecuteCommand(
                new DynCmd.UpdateModelValueCommand(
                    this.nodeViewModel.NodeModel.GUID, "Code", this.InnerTextEditor.Text));
        }

        void InnerTextEditor_TextChanged(object sender, EventArgs e)
        {
            if (WatermarkLabel.Visibility == Visibility.Visible)
                WatermarkLabel.Visibility = System.Windows.Visibility.Collapsed;

        }
        #endregion

        #region Private Helper Methods
        private void InitializeSyntaxHighlighter()
        {
            var stream = GetType().Assembly.GetManifestResourceStream(
                "Dynamo.UI.Resources." + Configurations.HighlightingFile);

            this.InnerTextEditor.SyntaxHighlighting = HighlightingLoader.Load(
                new XmlTextReader(stream), HighlightingManager.Instance);

            // Highlighting Digits
            var rules = this.InnerTextEditor.SyntaxHighlighting.MainRuleSet.Rules;
            rules.Add(CodeBlockUtils.CreateDigitRule());
        }

        private void OnRequestReturnFocusToSearch()
        {
            dynamoViewModel.ReturnFocusToSearch();
        }

        private void HandleEscape()
        {
            var text = this.InnerTextEditor.Text;
            var cb = DataContext as CodeBlockNodeModel;

            if (cb == null || cb.Code != null && text.Equals(cb.Code))
                OnRequestReturnFocusToSearch();
            else
                this.InnerTextEditor.Text = (DataContext as CodeBlockNodeModel).Code;
        }
        #endregion

        #region Key Press Event Handlers
        /// <summary>
        /// To allow users to remove focus by pressing Shift Enter. Uses two bools (shift / enter)
        /// and sets them when pressed/released
        /// </summary>        
        protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e)
        {

            if (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                if (e.Key == Key.Enter || e.Key == Key.Return)
                {
                    OnRequestReturnFocusToSearch();
                }
            }
            else if (e.Key == Key.Escape)
            {
                HandleEscape();
            }
        }

        #endregion
    }

    
}
