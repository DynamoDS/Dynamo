using Dynamo.DSEngine.CodeCompletion;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ProtoCore.Mirror;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace Dynamo.UI
{
    /// <summary>
    /// This class represents the tooltip for completion of function signatures
    /// </summary>
    class CodeBlockMethodInsightWindow : OverloadInsightWindow
    {
        private Caret caret;
        private CodeBlockInsightItem oldSelectedItem;

        sealed class CodeBlockMethodOverloadProvider : IOverloadProvider
        {
            private readonly CodeBlockMethodInsightWindow insightWindow;

            public CodeBlockMethodOverloadProvider(CodeBlockMethodInsightWindow insightWindow)
            {
                this.insightWindow = insightWindow;
                insightWindow.items.CollectionChanged += OnInsightWindowItemsChanged;
            }

            void OnInsightWindowItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                OnPropertyChanged("Count");
                OnPropertyChanged("CurrentHeader");
                OnPropertyChanged("CurrentContent");
                OnPropertyChanged("CurrentIndexText");
                insightWindow.OnSelectedItemChanged(EventArgs.Empty);
            }

            public event PropertyChangedEventHandler PropertyChanged;

            private int selectedIndex;
            /// <summary>
            /// Index of selected overload using the "down arrow" key
            /// </summary>

            public int SelectedIndex
            {
                get
                {
                    return selectedIndex;
                }
                set
                {
                    if (selectedIndex != value)
                    {
                        selectedIndex = value;
                        OnPropertyChanged("SelectedIndex");
                        OnPropertyChanged("CurrentHeader");
                        OnPropertyChanged("CurrentContent");
                        OnPropertyChanged("CurrentIndexText");
                    }
                }
            }

            /// <summary>
            /// Count of the number of overloads for a function
            /// </summary>
            public int Count
            {
                get { return insightWindow.Items.Count; }
            }

            /// <summary>
            /// Text display in tooltip indicating which index of overload is selected
            /// out of the total number of overloads
            /// </summary>

            public string CurrentIndexText
            {
                get { return (selectedIndex + 1).ToString() + " of " + this.Count.ToString(); }
            }

            /// <summary>
            /// Header information comprising signature of overload selected
            /// </summary>
            public object CurrentHeader
            {
                get
                {
                    CodeBlockInsightItem item = insightWindow.SelectedItem;
                    return item != null ? item.Header : null;
                }
            }

            public object CurrentContent
            {
                get
                {
                    CodeBlockInsightItem item = insightWindow.SelectedItem;
                    return item != null ? item.Content : null;
                }
            }

            internal void OnPropertyChanged(string propertyName)
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }
        }

        public CodeBlockMethodInsightWindow(TextArea textArea)
            : base(textArea)
        {
            this.Provider = new CodeBlockMethodOverloadProvider(this);
            this.Provider.PropertyChanged += delegate(object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == "SelectedIndex")
                    OnSelectedItemChanged(EventArgs.Empty);
            };
            AttachEvents();
        }

        readonly ObservableCollection<CodeBlockInsightItem> items = new ObservableCollection<CodeBlockInsightItem>();
        /// <summary>
        /// List of overload items for a given method
        /// </summary>
        public IList<CodeBlockInsightItem> Items
        {

            get { return items; }
        }

        /// <summary>
        /// Selected overload item
        /// </summary>
        public CodeBlockInsightItem SelectedItem
        {

            get
            {
                int index = this.Provider.SelectedIndex;
                if (index < 0 || index >= items.Count)
                    return null;
                else
                    return items[index];
            }
            set
            {
                this.Provider.SelectedIndex = items.IndexOf(value);
                OnSelectedItemChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// TODO: Implement this for highlighting of parameters in 
        /// function signature tooltip as you type
        /// </summary>
        public event EventHandler CaretPositionChanged;


        protected override void DetachEvents()
        {
            if (caret != null)
                caret.PositionChanged -= caret_PositionChanged;
            base.DetachEvents();
        }

        protected virtual void OnSelectedItemChanged(EventArgs e)
        {
            if (oldSelectedItem != null)
                oldSelectedItem.PropertyChanged -= SelectedItemPropertyChanged;
            oldSelectedItem = SelectedItem;
            if (oldSelectedItem != null)
                oldSelectedItem.PropertyChanged += SelectedItemPropertyChanged;
        }

        private void AttachEvents()
        {
            caret = this.TextArea.Caret;

            if (caret != null)
                caret.PositionChanged += caret_PositionChanged;
        }

        private void caret_PositionChanged(object sender, EventArgs e)
        {
            if (CaretPositionChanged != null)
            {
                CaretPositionChanged(this, e);
            }
        }

        private void SelectedItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var provider = Provider as CodeBlockMethodOverloadProvider;
            if (provider == null) return;
            switch (e.PropertyName)
            {
                case "Header":
                    provider.OnPropertyChanged("CurrentHeader");
                    break;
                case "Content":
                    provider.OnPropertyChanged("CurrentContent");
                    break;
            }
        }
    }

    /// <summary>
    /// This class represents the individual item in the list of 
    /// function overload items in the function signature insight window
    /// </summary>
    sealed class CodeBlockInsightItem : INotifyPropertyChanged
    {
        public readonly string Signature;

        public readonly string MethodName;

        public event PropertyChangedEventHandler PropertyChanged;

        public CodeBlockInsightItem(CompletionData completionData)
        {
            this.Signature = completionData.Stub;
            this.MethodName = completionData.Text;
        }

        string header;
        public object Header
        {
            get
            {
                if (header == null)
                {
                    header = Signature;
                    OnPropertyChanged("Header");
                }
                return header;
            }
        }

        /// <summary>
        /// TODO: Implement this
        /// </summary>
        string content = string.Empty;
        public object Content
        {
            get
            {
                return content;
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
