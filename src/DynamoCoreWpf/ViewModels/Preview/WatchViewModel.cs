using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using Dynamo.Wpf.Properties;
using Dynamo.UI.Commands;
using Dynamo.Utilities;
using Microsoft.Practices.Prism.ViewModel;
using Dynamo.Configuration;
using CoreNodeModels;

namespace Dynamo.ViewModels
{
    public class WatchViewModel : NotificationObject
    {
        // Formats double value into string. E.g. 1054.32179 => "1054.32179"
        // For more info: https://msdn.microsoft.com/en-us/library/kfsatb94(v=vs.110).aspx
        private const string numberFormat = "g";

        #region Events

        public event Action Clicked;

        internal void Click()
        {
            if (Clicked != null)
                Clicked();
        }

        #endregion

        #region Properties/Fields
        public const string EMPTY_LIST = "Empty List";
        public const string LIST = "List";
        public const string EMPTY_DICTIONARY = "Empty Dictionary";
        public const string DICTIONARY = "Dictionary";

        internal Watch WatchNode { get; set; }

        private ObservableCollection<WatchViewModel> children = new ObservableCollection<WatchViewModel>();
        private string label;
        private string link;
        private bool showRawData;
        private string path = "";
        private bool isOneRowContent;
        private readonly Action<string> tagGeometry;
        private bool isCollection;
        private string valueType;

        // Instance variable for the number of items in the list 
        private int numberOfItems;

        // Instance variable for the max depth of items in the list
        private int maxListLevel;

        // Instance variable for the list of levels 
        private IEnumerable<int> levels;

        public DelegateCommand FindNodeForPathCommand { get; set; }

        /// <summary>
        /// A collection of child WatchItems.
        /// </summary>
        public ObservableCollection<WatchViewModel> Children
        {
            get { return children; }
            set
            {
                children = value;
                RaisePropertyChanged("Children");
            }
        }

        /// <summary>
        /// The string label visible in the watch.
        /// </summary>
        public string NodeLabel
        {
            get { return label; }
            set
            {
                label = value;
                RaisePropertyChanged("NodeLabel");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Link
        {
            get { return link; }
            set
            {
                link = value;
                RaisePropertyChanged("Link");
            }
        }

        /// <summary>
        /// Returns the last index of the Path, 
        /// surrounded with square brackets.
        /// </summary>
        public string ViewPath
        {
            get
            {
                var splits = Path.Split(':');
                if (splits.Count() == 1)
                    return string.Empty;
                return splits.Any() ? string.Format(NodeLabel == LIST ? "{0}" : " {0} ", splits.Last()) : string.Empty;
            }
        }

        /// <summary>
        /// A path describing the location of the data.
        /// Path takes the form var_xxxx...:0:1:2, where
        /// var_xxx is the AST identifier for the node, followed
        /// by : delimited indices representing the array index
        /// of the data.
        /// </summary>
        public string Path
        {
            get { return path; }
            set
            {
                path = value;
                RaisePropertyChanged("Path");
            }
        }

        /// <summary>
        /// A flag used to determine whether the item
        /// should be process to draw 'raw' data or data
        /// treated in some context. An example is the drawing
        /// of watch items with or without units.
        /// </summary>
        public bool ShowRawData
        {
            get { return showRawData; }
            set
            {
                showRawData = value;
                RaisePropertyChanged("ShowRawData");
            }
        }

        public bool IsNodeExpanded { get; set; }

        /// <summary>
        /// If Content is 1 string, e.g. "Empty", "null", "Function", margin should be more to the left side.
        /// For this purpose used this value. When it's true, margin in DataTrigger is set to -15,5,5,5; otherwise
        /// it's set to 5,5,5,5
        /// </summary>
        public bool IsOneRowContent
        {
            get { return isOneRowContent; }
            set
            {
                isOneRowContent = value;
                RaisePropertyChanged("IsOneRowContent");
            }
        }

        /// <summary>
        /// Number of items in the overall list if node output is a list
        /// </summary>
        public int NumberOfItems
        {
            get { return numberOfItems; }
            set
            {
                numberOfItems = value;
                RaisePropertyChanged("NumberOfItems");
            }
        }


        /// <summary>
        /// Indicates if the items are lists
        /// </summary>
        public bool IsCollection
        {
            get { return isCollection; }
            set
            {
                isCollection = value;
                RaisePropertyChanged("IsCollection");
            }
        }

        /// <summary>
        /// Returns a list of listlevel items
        /// </summary>
        public IEnumerable<int> Levels
        {
            get { return levels; }
            set
            {
                levels = value;
                RaisePropertyChanged("Levels");
            }
        }

        /// <summary>
        /// Indicates if the item is the top level item
        /// </summary>
        public bool IsTopLevel { get; set; }

        /// <summary>
        /// The type of the output value,
        /// used to display value type labels on previews
        /// </summary>
        public string ValueType
        {
            get { return valueType; }
            set { valueType = value; RaisePropertyChanged(nameof(ValueType)); }
        }

        #endregion

        public WatchViewModel(Action<string> tagGeometry) : this(null, null, tagGeometry, true) { }

        /// <summary>
        /// This is added to set the Type identifier for the watch data. 
        /// It calls the base constructor internally.
        /// </summary> 
        public WatchViewModel(object obj, string path, Action<string> tagGeometry, bool expanded = false) : this(GetStringFromObject(obj), path, tagGeometry, expanded)
        {
            ValueType = GetDisplayType(obj);
        }

        public WatchViewModel(string label, string path, Action<string> tagGeometry, bool expanded = false)
        {
            FindNodeForPathCommand = new DelegateCommand(FindNodeForPath, CanFindNodeForPath);
            Path = path;
            NodeLabel = label;
            IsNodeExpanded = expanded;
            this.tagGeometry = tagGeometry;
            NumberOfItems = 0;
            maxListLevel = 0;
            IsCollection = label == WatchViewModel.LIST || label == WatchViewModel.DICTIONARY;
        }

        private static string GetStringFromObject(object obj)
        {
            TypeCode type = Type.GetTypeCode(obj.GetType());
            switch (type)
            {
                case TypeCode.Boolean:
                    return ObjectToLabelString(obj);
                case TypeCode.Double:
                    return ((double)obj).ToString(numberFormat, CultureInfo.InvariantCulture);
                //TODO: uncomment this once https://jira.autodesk.com/browse/DYN-5101 is complete
                //return ((double)obj).ToString(ProtoCore.Mirror.MirrorData.PrecisionFormat, CultureInfo.InvariantCulture);
                case TypeCode.Int32:
                    return ((int)obj).ToString(CultureInfo.InvariantCulture);
                case TypeCode.Int64:
                    return ((long)obj).ToString(CultureInfo.InvariantCulture);
                case TypeCode.DateTime:
                    return ((DateTime)obj).ToString(PreferenceSettings.DefaultDateFormat, CultureInfo.InvariantCulture);
                case TypeCode.Object:
                    return ObjectToLabelString(obj);
                default:
                    if (double.TryParse(obj.ToString(), out double d))
                    {
                        return Convert.ToDouble(obj).ToString(ProtoCore.Mirror.MirrorData.PrecisionFormat, CultureInfo.InvariantCulture);
                    }
                    return (string)obj;
            };
        }

        private static string ObjectToLabelString(object obj)
        {
            if (obj == null)
                return Resources.NullString;

            else if (obj is bool)
                return obj.ToString().ToLower();

            else if (obj is double)
                return ((double)obj).ToString(CultureInfo.InvariantCulture);

            return obj.ToString();
        }

        private string GetDisplayType(object obj)
        {
            TypeCode typeCode = Type.GetTypeCode(obj.GetType());
            // returning a customized user friendly string instead of just returning the name of the type 
            switch (typeCode)
            {
                case TypeCode.Boolean:
                    return nameof(TypeCode.Boolean);
                case TypeCode.Double:
                    return nameof(TypeCode.Double);
                case TypeCode.Int64:
                    return nameof(TypeCode.Int64);
                case TypeCode.Int32:
                    return nameof(TypeCode.Int32);
                case TypeCode.Object:
                    return nameof(TypeCode.Object);
                case TypeCode.String:
                    return nameof(TypeCode.String);
                case TypeCode.Empty:
                    return String.Empty;
                default:
                    return String.Empty;
            }
        }
        private bool CanFindNodeForPath(object obj)
        {
            return !string.IsNullOrEmpty(obj.ToString());
        }

        private void FindNodeForPath(object obj)
        {
            if (tagGeometry != null)
            {
                tagGeometry(obj.ToString());
            }
            //visualizationManager.TagRenderPackageForPath(obj.ToString());
        }

        /// <summary>
        /// Method to account for the total number of items and the depth of a list in a list (in the WatchTree)
        /// </summary>
        /// 
        public void CountNumberOfItems()
        {
            var listLevelAndItemCount = GetMaximumDepthAndItemNumber(this);
            maxListLevel = listLevelAndItemCount.Item1;
            NumberOfItems = listLevelAndItemCount.Item2;
            IsCollection = maxListLevel > 1 || (Children.Count > 0 && (Children[0].NodeLabel == LIST || Children[0].NodeLabel == DICTIONARY));
        }

        private Tuple<int, int> GetMaximumDepthAndItemNumber(WatchViewModel wvm)
        {
            if (wvm.Children.Count == 0)
            {
                if (wvm.NodeLabel == WatchViewModel.EMPTY_LIST)
                    return new Tuple<int, int>(0, 0);
                else
                    return new Tuple<int, int>(1, 1);
            }

            // If its a top level WatchViewModel, call function on child
            if (wvm.Path == null) 
            {
                return GetMaximumDepthAndItemNumber(wvm.Children[0]);
            }

            // if it's a list, recurse
            if (wvm.NodeLabel == LIST)
            {
                var depthAndNumbers = wvm.Children.Select(GetMaximumDepthAndItemNumber);
                var maxDepth = depthAndNumbers.Select(t => t.Item1).DefaultIfEmpty(1).Max() + 1;
                var itemNumber = depthAndNumbers.Select(t => t.Item2).Sum();
                return new Tuple<int, int>(maxDepth, itemNumber);
            }

            return new Tuple<int, int>(1,1);
        }

        /// <summary>
        /// Set the list levels of each list 
        /// </summary>
        public void CountLevels()
        {
            Levels = maxListLevel > 0 ? Enumerable.Range(1, maxListLevel).Reverse().Select(x => x).ToList() : Enumerable.Empty<int>();
        }

        /// <summary>
        /// Get the NodeLabel for this WatchViewModel and any children.
        /// Each level of children will be indented by 2 spaces.
        /// </summary>
        /// <param name="depth">The number of levels of indentation.</param>
        /// <param name="includeKey">If true the list or dictionary key will be included in the string.</param>
        /// <returns></returns>
        public string GetNodeLabelTree(int depth = 0, bool includeKey = false)
        {
            string indent = new string(' ', depth * 2);
            var str = new StringBuilder();
            if (depth != 0) str.AppendLine();
            str.Append(indent);
            if (Children.Count == 0)
            {
                if (includeKey) str.Append($"{ViewPath.Trim()}: ");
                str.Append(NodeLabel);
            }
            else if (NodeLabel == WatchViewModel.DICTIONARY)
            {
                IEnumerable<string> labels = Children.Select(x => x.GetNodeLabelTree(depth + 1, true));
                str.Append("{");
                str.AppendLine(string.Join(",", labels));
                str.Append($"{indent}}}");
            }
            else
            {
                IEnumerable<string> labels = Children.Select(x => x.GetNodeLabelTree(depth + 1, false));
                str.Append("[");
                str.AppendLine(string.Join(",", labels));
                str.Append($"{indent}]");
            }
            return str.ToString();
        }
    }
}
