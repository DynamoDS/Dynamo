using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using Dynamo.Graph.Workspaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dynamo.Graph.Nodes
{
    /// <summary>
    /// Possible graph input types. 
    /// </summary>
    [Obsolete("please use InputTypes instead, will be removed in 3.x. Will not be updated with new types." +
        "Serializing hostSelection or dropdownSelection will produce broken dynamo files. ")]
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
        [EnumMember(Value = "selection")]
        selectionInput,
        [EnumMember(Value = "hostSelection")]
        hostSelection,
        [EnumMember(Value = "dropdownSelection")]
        dropdownSelection
    };
    /// <summary>
    /// Possible graph input types. This Enum replaces NodeInputTypes. 
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum InputTypes
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
        [Obsolete("Use hostSelection or dropdownSelection instead")]
        [EnumMember(Value = "selection")]
        selectionInput,
        [EnumMember(Value = "hostSelection")]
        hostSelection,
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
        /// <summary>
        /// The type of input this node is.
        /// </summary>
        [Obsolete("Obsolete, this member has been replaced by Type2, which may contain new input types.")]
        public NodeInputTypes Type { get; set; }
        /// The type of input this node is.
        /// </summary>
        public InputTypes Type2 { get; set; }
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
        private static Dictionary<Type, InputTypes> dotNetTypeToInputType = new Dictionary<Type, InputTypes>
        {
            {typeof(String),InputTypes.stringInput},
            { typeof(Boolean),InputTypes.booleanInput},
            { typeof(DateTime),InputTypes.dateInput},
            { typeof(double),InputTypes.numberInput},
            { typeof(Int32),InputTypes.numberInput},
            { typeof(Int64),InputTypes.numberInput},
            {typeof(float),InputTypes.numberInput},
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
        internal static InputTypes GetInputTypeFromType(Type type)
        {
            InputTypes output;
            if (dotNetTypeToInputType.TryGetValue(type, out output))
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

            var valNumberComparison = false;
            try
            {
                valNumberComparison = Math.Abs(Convert.ToDouble(this.Value, CultureInfo.InvariantCulture) - Convert.ToDouble(converted.Value, CultureInfo.InvariantCulture)) < .000001;
            }
            catch (Exception e)
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
