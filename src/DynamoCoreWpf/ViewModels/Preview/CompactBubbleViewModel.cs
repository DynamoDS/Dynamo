using System;
using System.Globalization;
using Dynamo.Configuration;
using Dynamo.Wpf.Properties;

namespace Dynamo.ViewModels
{
    /// <summary>
    /// Class containing data to display in compact preview bubble
    /// </summary>
    public class CompactBubbleViewModel : ViewModelBase
    {
        #region Properties
        private string nodeLabel;
        private string valueType;

        /// <summary>
        /// Represents type of node output
        /// </summary>
        public string NodeLabel
        {
            get { return nodeLabel; }
            set
            {
                nodeLabel = value;
                RaisePropertyChanged("NodeLabel");
            }
        }

        public string ValueType
        {
            get { return valueType; }
            set { valueType = value; RaisePropertyChanged(nameof(ValueType)); }
        }

        private int numberOfItems;
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
                RaisePropertyChanged("ShowNumberOfItems");
            }
        }

        /// <summary>
        /// Indicates if number of list items is shown
        /// </summary>
        public bool ShowNumberOfItems
        {
            get { return NumberOfItems > 0 && IsCollection; }
        }

        /// <summary>
        /// Indicates if number of list items is shown
        /// </summary>
        internal bool IsCollection { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates an instance of <cref name="CompactBubbleViewModel"/> class with empty data
        /// </summary>
        public CompactBubbleViewModel(bool isCollection)
        {
            IsCollection = isCollection;
        }

        /// <summary>
        /// Creates an instance of <cref name="CompactBubbleViewModel"/> class with specified data
        /// </summary>
        /// <param name="nodeLabel">Text representing type of node output</param>
        /// <param name="items">Number of items in the overall list if node output is a list</param>
        public CompactBubbleViewModel(string nodeLabel, int items)
        {
            NodeLabel = nodeLabel;
            NumberOfItems = items;
        }

        public void SetObjectType(object obj)
        {
            if (obj != null)
            {
                ValueType = GetDisplayType(obj);
            }
        }

        private string GetDisplayType(object obj)
        {
            TypeCode typeCode = Type.GetTypeCode(obj.GetType());

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
                    return string.Empty;
                default:
                    return string.Empty;
            }
        }
        #endregion
    }
}
