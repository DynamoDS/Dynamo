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
        internal const string doubleType = "double";
        internal const string boolType = "bool";
        internal const string intType = "int";
        internal const string objectType = "object";
        internal const string stringType = "string";
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
                    return boolType;
                case TypeCode.Double:
                    return doubleType;
                case TypeCode.Int64:
                    return intType;
                case TypeCode.Int32:
                    return intType;
                case TypeCode.Object:
                    return objectType;
                case TypeCode.String:
                    return stringType;
                case TypeCode.Empty:
                    return String.Empty;
                default:
                    return String.Empty;
            }
        }
        #endregion
    }
}
