using System;
using System.Xml.Serialization;
using Dynamo.Core;

namespace Dynamo.Configuration
{
    /// <summary>
    /// A property that appears in the GraphMetadataViewExtension for all graphs, and
    /// is set by the user via the Preferences window. 
    /// </summary>
    public class RequiredProperty : NotificationObject
    {
        private string key;
        private string globalValue;
        private string graphValue;
        private bool isValueEditable;

        /// <summary>
        /// The ID of this RequiredProperty - should have a counterpart ExtensionRequiredProperty with the same ID
        /// </summary>
        public string UniqueId { get; set; }

        /// <summary>
        /// The name of this RequiredProperty
        /// </summary>
        public string Key
        {
            get => key;
            set
            {
                key = value;
                RaisePropertyChanged(nameof(Key));
            }
        }

        /// <summary>
        /// Users can assign a global value for a RequiredProperty via the Preferences window
        /// If set, this value overrides any value with the same key that are set in the graph
        /// and the value will be read-only in the ViewExtension
        /// </summary>
        public string GlobalValue
        {
            get => globalValue;
            set
            {
                globalValue = value;
                RaisePropertyChanged(nameof(GlobalValue));
                if (ValueIsGlobal)
                {
                    graphValue = value;
                    RaisePropertyChanged(nameof(GraphValue));
                }
            }
        }

        /// <summary>
        /// The value set by the user via the GraphMetadataViewExtension. If a GlobalValue has been set
        /// for the same RequiredProperty, this will be displayed in the ViewExtension instead of the GraphValue.
        /// If a GraphValue is set, this will be saved back to the .dyn/JSON file but will not be saved to the
        /// XML/DynamoSettings file, hence the XmlIgnore attribute.
        /// </summary>
        [XmlIgnore]
        public string GraphValue
        {
            get => graphValue;
            set
            {
                graphValue = value;
                RaisePropertyChanged(nameof(GraphValue));
            }
        }

        /// <summary>
        /// Determined whether this value can be set by users, or is locked for all graphs
        /// </summary>
        public bool ValueIsGlobal
        {
            get => isValueEditable;
            set
            {
                isValueEditable = value;
                RaisePropertyChanged(nameof(ValueIsGlobal));
            }
        }
        
        /// <summary>
        /// Public constructor for RequiredProperty which sets the UniqueId.
        /// </summary>
        public RequiredProperty()
        {
            UniqueId = Guid.NewGuid().ToString();
        }
    }
}
