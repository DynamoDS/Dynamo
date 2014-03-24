using System.Windows;
using Dynamo.Utilities;
using System.Windows.Controls;
using System.IO;
using System.Xml;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Diagnostics;
using System;
using System.Collections.Generic;

namespace Dynamo.UI.Prompts
{
    internal class TaskDialogParams
    {
        List<Tuple<int, string, bool>> buttons = null;

        #region Public Operational Methods

        internal TaskDialogParams(Uri imageUri, string dialogTitle,
            string summary, string description)
        {
            this.ImageUri = ImageUri;
            this.DialogTitle = dialogTitle;
            this.Summary = summary;
            this.Description = description;
        }

        internal void AddLeftAlignedButton(int id, string content)
        {
            if (buttons == null)
                buttons = new List<Tuple<int, string, bool>>();

            buttons.Add(new Tuple<int, string, bool>(id, content, true));
        }

        internal void AddRightAlignedButton(int id, string content)
        {
            if (buttons == null)
                buttons = new List<Tuple<int, string, bool>>();

            buttons.Add(new Tuple<int, string, bool>(id, content, false));
        }

        #endregion

        #region Public Class Properties

        internal int ClickedButtonId { get; set; }
        internal Uri ImageUri { get; private set; }
        internal string DialogTitle { get; private set; }
        internal string Summary { get; private set; }
        internal string Description { get; private set; }

        internal IEnumerable<Tuple<int, string, bool>> Buttons
        {
            get { return buttons; }
        }

        #endregion
    }

    public partial class GenericTaskDialog : Window
    {
        #region Public Operational Methods

        public GenericTaskDialog() // Xaml design needs this.
        {
            InitializeComponent();
        }

        internal GenericTaskDialog(TaskDialogParams taskDialogParams)
        {
            InitializeComponent();
        }

        #endregion

    }
}
