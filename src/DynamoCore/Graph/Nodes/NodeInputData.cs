using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using Dynamo.Graph.Workspaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dynamo.Graph.Nodes
{
    // NodeInputTypes is now serialized twice as NodeInputData.Type and NodeInputData.Type2.
    // This enables us to add new values to the enum while allowing previous dynamo versions to successfully
    // load new files with those new types. This is the case because by default json.net ignores missing properties.
    // So Type2 is not deserialized at all in previous versions of Dynamo.
    // Type's setter limits the possible values to a subset of the enum to avoid clients setting this to a value that would break file
    // deserialization in previous dynamo versions.
    // TODO We should unify these properties (Type and Type2) when possible (Dynamo 3.x) 

    /// <summary>
    /// Possible graph input types. 
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum NodeInputTypes
    {
        [EnumMember(Value = "number")]
        numberInput,
        [EnumMember(Value = "boolean")]
        booleanInput,
        [EnumMember(Value = "string")]
        stringInput,
        [EnumMember(Value = "color")]
        colorInput,
        [EnumMember(Value = "date")]
        dateInput,
        [Obsolete()]
        [EnumMember(Value = "selection")]
        selectionInput,
        /// <summary>
        /// Should not be used when setting NodeInputData.Type, use selection instead, can be used with Type2.
        /// </summary>
        [EnumMember(Value = "hostSelection")]
        hostSelection,
        /// <summary>
        /// Should not be used when setting NodeInputData.Type, use selection instead, can be used with Type2.
        /// </summary>
        [EnumMember(Value = "dropdownSelection")]
        dropdownSelection
    };

    /// <summary>
    /// Represents a node which acts as a UI input for the graph
    /// - may also hold a value for that input
    /// </summary>
    public class NodeInputData
    {
        /// <summary>
        /// The id of the node.
        /// </summary>
        [JsonConverter(typeof(IdToGuidConverter))]
        public Guid Id { get; set; }
        /// <summary>
        /// Display name of the input node.
        /// </summary>
        public string Name { get; set; }
        private NodeInputTypes type;
        /// <summary>
        /// The type of input this node is.
        /// </summary>
        [Obsolete("Obsolete, this member has been replaced by Type2, which may contain new input types.")]
        public NodeInputTypes Type
        {
            get => type;
            set
            {
                // we don't allow setting Type to these enum values as they cause old versions of dynamo to fail to load files.
                // use NodeInputData.Type2 instead.
                if (value is NodeInputTypes.dropdownSelection || value is NodeInputTypes.hostSelection)
                {
                    type = NodeInputTypes.selectionInput;
                }
                else { type = value; }
            }
        }
        /// The type of input this node is.
        /// </summary>
        public NodeInputTypes Type2 { get; set; }
        /// <summary>
        /// The value of the input when the graph was saved.
        /// This should always be a string for all types.
        /// </summary>
        public string Value { get; set; }

        //optional properties, might be null
        /// <summary>
        /// If this input is a dropdownSelection type a list of choices a user can select.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Choices { get; set; }
        /// <summary>
        /// if this input is a Number, the max value of that number.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public double? MaximumValue { get; set; }
        /// <summary>
        /// if this input is a Number, the min value of that number.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public double? MinimumValue { get; set; }
        /// <summary>
        /// if this input is a Number, the step value of that number when it acts as a slider.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public double? StepValue { get; set; }
        /// <summary>
        /// if this input is a Number, the number type Double or Integer that the number returns.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string NumberType { get; set; }
        /// <summary>
        /// Description displayed to user of this input node.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        /// <summary>
        /// The index of the selected item.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int SelectedIndex { get; set; }

        private static Dictionary<Type, NodeInputTypes> dotNetTypeToNodeInputType = new Dictionary<Type, NodeInputTypes>
        {
            {typeof(String),NodeInputTypes.stringInput},
            { typeof(Boolean),NodeInputTypes.booleanInput},
            { typeof(DateTime),NodeInputTypes.dateInput},
            { typeof(double),NodeInputTypes.numberInput},
            { typeof(Int32),NodeInputTypes.numberInput},
            { typeof(Int64),NodeInputTypes.numberInput},
            {typeof(float),NodeInputTypes.numberInput},
        };

        [Obsolete("To be removed in Dynamo 3.x")]
        public static NodeInputTypes getNodeInputTypeFromType(Type type)
        {
            NodeInputTypes output;
            if (dotNetTypeToNodeInputType.TryGetValue(type, out output))
            {
                return output;
            }
            else
            {
                throw new ArgumentException("could not find an inputType for this type");
            }
        }

        public override bool Equals(object obj)
        {
            var converted = obj as NodeInputData;

            bool valNumberComparison;
            try
            {
                valNumberComparison = Math.Abs(Convert.ToDouble(this.Value, CultureInfo.InvariantCulture) - Convert.ToDouble(converted.Value, CultureInfo.InvariantCulture)) < .000001;
            }
            catch (Exception)
            {
                //this just stays false.
                valNumberComparison = false;
            }

            return obj is NodeInputData && this.Id == converted.Id &&
                this.Description == converted.Description &&
                this.Choices == converted.Choices &&
                this.MaximumValue == converted.MaximumValue &&
                this.MinimumValue == converted.MinimumValue &&
                //TODO don't check name for now as this requires a VIew.
                //and we only have model level tests.
                //this.Name == converted.Name &&
                this.NumberType == converted.NumberType &&
                this.StepValue == converted.StepValue &&
                this.Type == converted.Type &&
                this.Type2 == converted.Type2 &&
                //check if the value is the same or if the value is a number check is it similar
                ((this.Value == converted.Value) || valNumberComparison || this.Value.ToString() == converted.Value.ToString());
        }
    }

}
