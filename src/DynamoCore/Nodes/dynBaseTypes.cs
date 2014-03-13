using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.UI;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using Dynamo.FSchemeInterop;
using Dynamo.FSchemeInterop.Node;
using Dynamo.Units;
using Dynamo.Models;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using RestSharp.Contrib;
using Value = Dynamo.FScheme.Value;
using System.Globalization;
using ProtoCore.AST.AssociativeAST;
using Dynamo.DSEngine;
using Utils = Dynamo.FSchemeInterop.Utils;

namespace Dynamo.Nodes
{
    /// <summary>
    /// Built-in Dynamo Categories. If you want your node to appear in one of the existing Dynamo
    /// categories, then use these constants. This ensures that if the names of the categories
    /// change down the road, your node will still be placed there.
    /// </summary>
    public static partial class BuiltinNodeCategories
    {
        public const string CORE = "Core";
        public const string CORE_INPUT = "Core.Input";
        public const string CORE_STRINGS = "Core.Strings";
        public const string CORE_LISTS_CREATE = "Core.Lists.Create";
        public const string CORE_LISTS_MODIFY = "Core.Lists.Modify";
        public const string CORE_LISTS_EVALUATE = "Core.Lists.Evaluate";
        public const string CORE_LISTS_QUERY = "Core.Lists.Query";
        public const string CORE_VIEW = "Core.View";
        public const string CORE_ANNOTATE = "Core.Annotate";
        public const string CORE_EVALUATE = "Core.Evaluate";
        public const string CORE_TIME = "Core.Time";
        public const string CORE_SCRIPTING = "Core.Scripting";
        public const string CORE_FUNCTIONS = "Core.Functions";

        public const string LOGIC = "Logic";
        public const string LOGIC_MATH_ARITHMETIC = "Logic.Math.Arithmetic";
        public const string LOGIC_MATH_ROUNDING = "Logic.Math.Rounding";
        public const string LOGIC_MATH_CONSTANTS = "Logic.Math.Constants";
        public const string LOGIC_MATH_TRIGONOMETRY = "Logic.Math.Trigonometry";
        public const string LOGIC_MATH_RANDOM = "Logic.Math.Random";
        public const string LOGIC_MATH_OPTIMIZE = "Logic.Math.Optimize";
        public const string LOGIC_EFFECT = "Logic.Effect";
        public const string LOGIC_COMPARISON = "Logic.Comparison";
        public const string LOGIC_CONDITIONAL = "Logic.Conditional";
        public const string LOGIC_LOOP = "Logic.Loop";


        public const string GEOMETRY = "Geometry";

        public const string GEOMETRY_CURVE_CREATE = "Geometry.Curve.Create";
        public const string GEOMETRY_CURVE_DIVIDE = "Geometry.Curve.Divide";
        public const string GEOMETRY_CURVE_PRIMITIVES = "Geometry.Curve.Primitives";
        public const string GEOMETRY_CURVE_QUERY = "Geometry.Curve.Query";
        public const string GEOMETRY_CURVE_FIT = "Geometry.Curve.Fit";

        public const string GEOMETRY_POINT_CREATE = "Geometry.Point.Create";
        public const string GEOMETRY_POINT_MODIFY = "Geometry.Point.Modify";
        public const string GEOMETRY_POINT_QUERY = "Geometry.Point.Query";
        public const string GEOMETRY_POINT_GRID = "Geometry.Point.Grid";
        public const string GEOMETRY_POINT_TESSELATE = "Geometry.Point.Tesselate";

        public const string GEOMETRY_SOLID_BOOLEAN = "Geometry.Solid.Boolean";
        public const string GEOMETRY_SOLID_CREATE = "Geometry.Solid.Create";
        public const string GEOMETRY_SOLID_MODIFY = "Geometry.Solid.Modify";
        public const string GEOMETRY_SOLID_PRIMITIVES = "Geometry.Solid.Primitives";
        public const string GEOMETRY_SOLID_QUERY = "Geometry.Solid.Extract";
        public const string GEOMETRY_SOLID_REPAIR = "Geometry.Solid.Repair";

        public const string GEOMETRY_SURFACE_CREATE = "Geometry.Surface.Create";
        public const string GEOMETRY_SURFACE_QUERY = "Geometry.Surface.Query";
        public const string GEOMETRY_SURFACE_UV = "Geometry.Surface.UV";
        public const string GEOMETRY_SURFACE_DIVIDE = "Geometry.Surface.Divide";

        public const string GEOMETRY_TRANSFORM_APPLY = "Geometry.Transform.Apply";
        public const string GEOMETRY_TRANSFORM_MODIFY = "Geometry.Transform.Modify";
        public const string GEOMETRY_TRANSFORM_CREATE = "Geometry.Transform.Create";

        public const string GEOMETRY_INTERSECT = "Geometry.Intersect";

        public const string GEOMETRY_EXPERIMENTAL_PRIMITIVES = "Geometry.Experimental.Primitives";
        public const string GEOMETRY_EXPERIMENTAL_SURFACE = "Geometry.Experimental.Surface";
        public const string GEOMETRY_EXPERIMENTAL_CURVE = "Geometry.Experimental.Curve";
        public const string GEOMETRY_EXPERIMENTAL_SOLID = "Geometry.Experimental.Solid";
        public const string GEOMETRY_EXPERIMENTAL_MODIFY = "Geometry.Experimental.Modify";
        public const string GEOMETRY_EXPERIMENTAL_VIEW = "Geometry.Experimental.View";

        public const string REVIT = "Revit";
        public const string REVIT_DOCUMENT = "Revit.Document";
        public const string REVIT_DATUMS = "Revit.Datums";
        public const string REVIT_FAMILIES = "Revit.Families";
        public const string REVIT_SELECTION = "Revit.Selection";
        public const string REVIT_VIEW = "Revit.View";
        public const string REVIT_REFERENCE = "Revit.Reference";
        public const string REVIT_PARAMETERS = "Revit.Parameters";
        public const string REVIT_BAKE = "Revit.Bake";
        public const string REVIT_API = "Revit.API";

        public const string ANALYZE = "Analyze";
        public const string ANALYZE_MEASURE = "Analyze.Measure";
        public const string ANALYZE_DISPLAY = "Analyze.Display";
        public const string ANALYZE_COLOR = "Analyze.Color";
        public const string ANALYZE_STRUCTURE = "Analyze.Structure";
        public const string ANALYZE_CLIMATE = "Analyze.Climate";
        public const string ANALYZE_ACOUSTIC = "Analyze.Acoustic";
        public const string ANALYZE_SOLAR = "Analyze.Solar";

        public const string IO = "Input/Output";
        public const string IO_FILE = "Input/Output.File";
        public const string IO_NETWORK = "Input/Output.Network";
        public const string IO_HARDWARE = "Input/Output.Hardware";
    }

    public static class Utilities
    {
        public static string Ellipsis(string value, int desiredLength)
        {
            if (desiredLength > value.Length)
            {
                return value;
            }
            else
            {
                return value.Remove(desiredLength - 1) + "...";
            }
        }

        /// <summary>
        /// <para>This method patches the fullyQualifiedName of a given type. It 
        /// updates the given name to its newer form (i.e. "Dynamo.Nodes.Xyz")
        /// if it matches the older form (e.g. "Dynamo.Elements.Xyz").</para>
        /// <para>The method also attempts to update "XYZ/UV" convention to 
        /// "Xyz/Uv" to comply with the new Dynamo naming convention.</para>
        /// </summary>
        /// <param name="fullyQualifiedName">A fully qualified name. An example
        /// of this would be "Dynamo.Elements.dynNode".</param>
        /// <returns>The processed fully qualified name. For an example, the 
        /// name "Dynamo.Elements.UV" will be returned as "Dynamo.Nodes.Uv".
        /// </returns>
        public static string PreprocessTypeName(string fullyQualifiedName)
        {
            if (string.IsNullOrEmpty(fullyQualifiedName))
                throw new ArgumentNullException("fullyQualifiedName");

            // older files will have nodes in the Dynamo.Elements namespace
            string oldPrefix = "Dynamo.Elements.";
            string newPrefix = "Dynamo.Nodes.";
            string className = string.Empty;

            // Attempt to extract the class name out of the fully qualified 
            // name, regardless of whether it is in the form of the older 
            // "Dynamo.Elements.XxxYyy" or the newer "Dynamo.Nodes.XxxYyy".
            // 
            if (fullyQualifiedName.StartsWith(oldPrefix))
                className = fullyQualifiedName.Substring(oldPrefix.Length);
            else if (fullyQualifiedName.StartsWith(newPrefix))
                className = fullyQualifiedName.Substring(newPrefix.Length);
            else
            {
                // We are only expected to process names of our built-in types,
                // and if we're given any of the system types, then we'll just
                // return them as-is without any patches.
                // 
                return fullyQualifiedName;
            }

            // Remove prefix of 'dyn' from older files.
            if (className.StartsWith("dyn"))
                className = className.Remove(0, 3);

            // Older files will have nodes that use "XYZ" and "UV" 
            // instead of "Xyz" and "Uv". Update these names.
            className = className.Replace("XYZ", "Xyz");
            className = className.Replace("UV", "Uv");
            return newPrefix + className; // Always new prefix from now on.
        }

        /// <summary>
        /// <para>Resolve either a built-in type or a system type, given its fully
        /// qualified name. This method performs the search with the following 
        /// order:</para>
        /// <para>1. Search among the built-in types registered with 
        /// DynamoController.BuiltInTypesByName dictionary</para>
        /// <para>2. Search among the available .NET runtime types</para>
        /// <para>3. Search among built-in types, taking their "also-known-as" 
        /// attributes into consideration when matching the type name</para>
        /// </summary>
        /// <param name="fullyQualifiedName"></param>
        /// <returns></returns>
        public static System.Type ResolveType(string fullyQualifiedName)
        {
            if (string.IsNullOrEmpty(fullyQualifiedName))
                throw new ArgumentNullException("fullyQualifiedName");

            TypeLoadData tData = null;
            var builtInTypes = dynSettings.Controller.BuiltInTypesByName;
            if (builtInTypes.TryGetValue(fullyQualifiedName, out tData))
                return tData.Type; // Found among built-in types, return it.

            //try and get a system type by this name
            Type type = Type.GetType(fullyQualifiedName);
            if (null != type)
                return type;

            // If we still can't find the type, try the also known as attributes.
            foreach (var builtInType in dynSettings.Controller.BuiltInTypesByName)
            {
                var attribs = builtInType.Value.Type.GetCustomAttributes(
                    typeof(AlsoKnownAsAttribute), false);

                if (attribs.Count() <= 0)
                    continue;

                AlsoKnownAsAttribute akaAttrib = attribs[0] as AlsoKnownAsAttribute;
                if (akaAttrib.Values.Contains(fullyQualifiedName))
                {
                    DynamoLogger.Instance.Log(string.Format(
                        "Found matching node for {0} also known as {1}",
                        builtInType.Key, fullyQualifiedName));

                    return builtInType.Value.Type; // Found a matching type.
                }
            }

            DynamoLogger.Instance.Log(string.Format(
                "Could not load node of type: {0}", fullyQualifiedName));

            DynamoLogger.Instance.Log("Loading will continue but nodes " +
                "might be missing from your workflow.");

            return null;
        }
    }

    public delegate double ConversionDelegate(double value);

    public abstract partial class VariableInput : NodeWithOneOutput
    {
        protected VariableInput()
        {
        }

        protected abstract string GetInputRootName();
        protected abstract string GetTooltipRootName();

        protected virtual int GetInputNameIndex()
        {
            return InPortData.Count;
        }

        protected internal virtual void RemoveInput()
        {
            var count = InPortData.Count;
            if (count > 0)
            {
                InPortData.RemoveAt(count - 1);
            }
        }

        protected internal virtual void AddInput()
        {
            var idx = GetInputNameIndex();
            InPortData.Add(new PortData(GetInputRootName() + idx, GetTooltipRootName() + idx, typeof(object)));
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            foreach (var inport in InPortData)
            {
                XmlElement input = xmlDoc.CreateElement("Input");

                input.SetAttribute("name", inport.NickName);

                nodeElement.AppendChild(input);
            }
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            int i = InPortData.Count;
            foreach (XmlNode subNode in nodeElement.ChildNodes)
            {
                if (i > 0)
                {
                    i--;
                    continue;
                }

                if (subNode.Name == "Input")
                {
                    InPortData.Add(new PortData(subNode.Attributes["name"].Value, "", typeof(object)));
                }
            }
            RegisterAllPorts();
        }

        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context); //Base implementation must be called
            XmlDocument xmlDoc = element.OwnerDocument;
            foreach (var inport in InPortData)
            {
                XmlElement input = xmlDoc.CreateElement("Input");
                input.SetAttribute("name", inport.NickName);
                element.AppendChild(input);
            }
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context); //Base implementation must be called

            if (context == SaveContext.Undo)
            {
                //Reads in the new number of ports required from the data stored in the Xml Element
                //during Serialize (nextLength). Changes the current In Port Data to match the
                //required size by adding or removing port data.
                int currLength = InPortData.Count;
                XmlNodeList inNodes = element.SelectNodes("Input");
                int nextLength = inNodes.Count;
                if (nextLength > currLength)
                {
                    for (; currLength < nextLength; currLength++)
                    {
                        XmlNode subNode = inNodes.Item(currLength);
                        string nickName = subNode.Attributes["name"].Value;
                        InPortData.Add(new PortData(nickName, "", typeof(object)));
                    }
                }
                else if (nextLength < currLength)
                    InPortData.RemoveRange(nextLength, currLength - nextLength);

                RegisterAllPorts();
            }
        }

        #endregion
    }

    public abstract partial class VariableInputAndOutput : NodeModel
    {
        protected VariableInputAndOutput()
        {
        }

        protected abstract string GetInputRootName();
        protected abstract string GetOutputRootName();
        protected abstract string GetTooltipRootName();

        protected virtual int GetInputNameIndex()
        {
            return InPortData.Count;
        }

        protected virtual void RemoveInput()
        {
            var count = InPortData.Count;
            if (count > 0)
            {
                InPortData.RemoveAt(count - 1);
                OutPortData.RemoveAt(count - 1);
            }
        }

        protected internal virtual void AddInput()
        {
            var idx = GetInputNameIndex();
            InPortData.Add(new PortData(GetInputRootName() + idx, GetTooltipRootName() + idx, typeof(object)));
            OutPortData.Add(new PortData(GetOutputRootName() + idx, GetTooltipRootName() + idx, typeof(object)));
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            foreach (var inport in InPortData)
            {
                XmlElement input = xmlDoc.CreateElement("Input");

                input.SetAttribute("name", inport.NickName);

                nodeElement.AppendChild(input);
            }

            foreach (var outport in OutPortData)
            {
                XmlElement output = xmlDoc.CreateElement("Output");

                output.SetAttribute("name", outport.NickName);

                nodeElement.AppendChild(output);
            }
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            int i = InPortData.Count;
            foreach (XmlNode subNode in nodeElement.ChildNodes)
            {
                if (i > 0)
                {
                    i--;
                    continue;
                }

                if (subNode.Name == "Input")
                {
                    InPortData.Add(new PortData(subNode.Attributes["name"].Value, "", typeof(object)));
                }
                else if (subNode.Name == "Output")
                {
                    OutPortData.Add(new PortData(subNode.Attributes["name"].Value, "", typeof(object)));
                }
            }
            RegisterAllPorts();
        }

        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context); //Base implementation must be called
            XmlDocument xmlDoc = element.OwnerDocument;
            foreach (var inport in InPortData)
            {
                XmlElement input = xmlDoc.CreateElement("Input");
                input.SetAttribute("name", inport.NickName);
                element.AppendChild(input);
            }
            foreach (var outport in OutPortData)
            {
                XmlElement output = xmlDoc.CreateElement("Output");
                output.SetAttribute("name", outport.NickName);
                element.AppendChild(output);
            }
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context); //Base implementation must be called

            if (context == SaveContext.Undo)
            {
                //Reads in the new number of ports required from the data stored in the Xml Element
                //during Serialize (nextLength). Changes the current In Port Data to match the
                //required size by adding or removing port data.

                // INPUTS
                int currLength = InPortData.Count;
                XmlNodeList inNodes = element.SelectNodes("Input");
                int nextLength = inNodes.Count;
                if (nextLength > currLength)
                {
                    for (; currLength < nextLength; currLength++)
                    {
                        XmlNode subNode = inNodes.Item(currLength);
                        string nickName = subNode.Attributes["name"].Value;
                        InPortData.Add(new PortData(nickName, "", typeof(object)));
                    }
                }
                else if (nextLength < currLength)
                    InPortData.RemoveRange(nextLength, currLength - nextLength);

                // OUTPUTS
                currLength = OutPortData.Count;
                XmlNodeList outNodes = element.SelectNodes("Output");
                nextLength = outNodes.Count;
                if (nextLength > currLength)
                {
                    for (; currLength < nextLength; currLength++)
                    {
                        XmlNode subNode = outNodes.Item(currLength);
                        string nickName = subNode.Attributes["name"].Value;
                        OutPortData.Add(new PortData(nickName, "", typeof(object)));
                    }
                }
                else if (nextLength < currLength)
                    OutPortData.RemoveRange(nextLength, currLength - nextLength);

                RegisterAllPorts();
            }
        }

        #endregion
    }

    #region Functions

    [NodeName("Compose Functions")]
    [NodeCategory(BuiltinNodeCategories.CORE_FUNCTIONS)]
    [NodeDescription("Composes two single parameter functions into one function.")]
    public class ComposeFunctions : NodeWithOneOutput
    { 
        public ComposeFunctions()
        {
            InPortData.Add(new PortData("f", "A Function", typeof(Value.Function)));
            InPortData.Add(new PortData("g", "A Function", typeof(Value.Function)));
            OutPortData.Add(new PortData("g ∘ f", "Composed function: g(f(x))", typeof(Value.Function)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var f = ((Value.Function)args[0]).Item;
            var g = ((Value.Function)args[1]).Item;

            return Value.NewFunction(Utils.ConvertToFSchemeFunc(x => g.Invoke(Utils.MakeFSharpList(f.Invoke(x)))));
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            XmlElement composeNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(composeNode, "",
                "Compose", "__Compose@_FunctionObject[]");
            migratedData.AppendNode(composeNode);
            string composeNodeId = MigrationManager.GetGuidFromXmlElement(composeNode);

            XmlElement createListNode = MigrationManager.CreateNode(data.Document,
                "DSCoreNodesUI.CreateList", "Create List");
            migratedData.AppendNode(createListNode);
            createListNode.SetAttribute("inputcount", "2");
            string createListNodeId = MigrationManager.GetGuidFromXmlElement(createListNode);

            //create and reconnect the connecters
            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.INPUT);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.INPUT);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            PortId newInPort0 = new PortId(composeNodeId, 0, PortType.INPUT);
            PortId newInPort1 = new PortId(createListNodeId, 0, PortType.INPUT);
            PortId newInPort2 = new PortId(createListNodeId, 1, PortType.INPUT);

            data.ReconnectToPort(connector0, newInPort1);
            data.ReconnectToPort(connector1, newInPort2);
            data.CreateConnector(createListNode, 0, composeNode, 0);

            return migratedData;
        }
    }

    #endregion

    #region Control Flow

    [NodeName("Apply Function to List")]
    [NodeCategory(BuiltinNodeCategories.CORE_EVALUATE)]
    [NodeDescription("Applies a function to a list of arguments.")]
    public class ApplyList : NodeWithOneOutput
    {
        public ApplyList()
        {
            InPortData.Add(new PortData("func", "Function", typeof(Value.Function)));
            InPortData.Add(new PortData("args", "List of arguments to apply function to.", typeof(Value.List)));

            OutPortData.Add(new PortData("result", "Result of function application.", typeof(object)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var f = ((Value.Function)args[0]).Item;
            var fArgs = ((Value.List)args[1]).Item;

            return f.Invoke(fArgs);
        }
    }

    //TODO: Setup proper IsDirty smart execution management
    [NodeName("Apply Function")]
    [NodeCategory(BuiltinNodeCategories.CORE_EVALUATE)]
    [NodeDescription("Applies a function to arguments.")]
    public class Apply1 : VariableInput
    {
        public Apply1()
        {
            InPortData.Add(new PortData("func", "Function", typeof(object)));
            OutPortData.Add(new PortData("result", "Result of function application.", typeof(object)));

            RegisterAllPorts();
        }

        protected override string GetInputRootName()
        {
            return "arg";
        }

        protected override string GetTooltipRootName()
        {
            return "Argument #";
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var f = ((Value.Function)args[0]).Item;
            var fArgs = args.Tail;

            return f.Invoke(fArgs);
        }

        protected internal override void RemoveInput()
        {
            if (InPortData.Count > 1)
                base.RemoveInput();
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            foreach (var inport in InPortData.Skip(1))
            {
                XmlElement input = xmlDoc.CreateElement("Input");

                input.SetAttribute("name", inport.NickName);

                nodeElement.AppendChild(input);
            }
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            foreach (XmlNode subNode in nodeElement.ChildNodes)
            {
                if (subNode.Name == "Input")
                {
                    var attr = subNode.Attributes["name"].Value;

                    if (!attr.Equals("func"))
                        InPortData.Add(new PortData(subNode.Attributes["name"].Value, "", typeof(object)));
                }
            }
            RegisterAllPorts();
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            XmlElement applyNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(applyNode, "",
                "Apply", "Apply@_FunctionObject,var[]..[]");
            migratedData.AppendNode(applyNode);
            string applyNodeId = MigrationManager.GetGuidFromXmlElement(applyNode);

            int numberOfArgs = oldNode.ChildNodes.Count;
            string numberOfArgsString = numberOfArgs.ToString();
            XmlElement createListNode = MigrationManager.CreateNode(data.Document,
                "DSCoreNodesUI.CreateList", "Create List");
            migratedData.AppendNode(createListNode);
            createListNode.SetAttribute("inputcount", numberOfArgsString);
            string createListNodeId = MigrationManager.GetGuidFromXmlElement(createListNode);

            //create and reconnect the connecters
            while (numberOfArgs > 0) 
            {
                PortId oldInPort = new PortId(oldNodeId, numberOfArgs, PortType.INPUT);
                XmlElement connector = data.FindFirstConnector(oldInPort);
                PortId newInPort = new PortId(createListNodeId, numberOfArgs - 1, PortType.INPUT);
                data.ReconnectToPort(connector, newInPort);
                numberOfArgs--;
            }
            data.CreateConnector(createListNode, 0, applyNode, 1);

            return migratedData;
        }
    }
    
    #endregion

    public abstract partial class BasicInteractive<T> : NodeWithOneOutput
    {
        private T _value;
        public virtual T Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (_value == null || !_value.Equals(value))
                {
                    _value = value;
                    //DynamoLogger.Instance.Log("Value changed to: " + _value);
                    RequiresRecalc = value != null;
                    RaisePropertyChanged("Value");
                }
            }
        }

        protected abstract T DeserializeValue(string val);
        protected abstract string SerializeValue(T val);

        protected BasicInteractive()
        {
            Type type = typeof(T);
            OutPortData.Add(new PortData("", type.Name, type));
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            XmlElement outEl = xmlDoc.CreateElement(typeof(T).FullName);
            outEl.SetAttribute("value", Value.ToString());
            nodeElement.AppendChild(outEl);
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            foreach (XmlNode subNode in nodeElement.ChildNodes)
            {
                if (subNode.Name.Equals(typeof(T).FullName))
                {
                    Value = DeserializeValue(subNode.Attributes[0].Value);
                }
            }
        }

        public override string PrintExpression()
        {
            return Value.ToString();
        }

        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context); //Base implementation must be called
            if (context == SaveContext.Undo)
            {
                var document = element.OwnerDocument;
                XmlElement childElement = document.CreateElement(typeof(T).FullName);
                childElement.SetAttribute("value", SerializeValue(this.Value));
                element.AppendChild(childElement);
            }
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context); //Base implementation must be called
            if (context == SaveContext.Undo)
            {
                foreach (XmlNode childNode in element.ChildNodes)
                {
                    if (childNode.Name.Equals(typeof(T).FullName) == false)
                        continue;

                    this.Value = DeserializeValue(childNode.Attributes["value"].Value);
                    break;
                }
            }
        }

        #endregion
    }

    public abstract class Double : BasicInteractive<double>
    {
        public override bool IsConvertible
        {
            get { return true; }
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            return FScheme.Value.NewNumber(Value);
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            XmlElement outEl = xmlDoc.CreateElement(typeof(double).FullName);
            outEl.SetAttribute("value", Value.ToString(CultureInfo.InvariantCulture));
            nodeElement.AppendChild(outEl);
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var rhs = AstFactory.BuildDoubleNode(Value);
            var assignment = AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), rhs);

            return new[] { assignment };
        }
    }

    public abstract class Integer : BasicInteractive<int>
    {
        public override Value Evaluate(FSharpList<Value> args)
        {
            return FScheme.Value.NewNumber(Value);
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            XmlElement outEl = xmlDoc.CreateElement(typeof(int).FullName);
            outEl.SetAttribute("value", Value.ToString(CultureInfo.InvariantCulture));
            nodeElement.AppendChild(outEl);
        }
    }

    public abstract class Bool : BasicInteractive<bool>
    {
        public override Value Evaluate(FSharpList<Value> args)
        {
            return FScheme.Value.NewNumber(Value ? 1 : 0);
        }
    }

    public abstract partial class String : BasicInteractive<string>
    {
        public override Value Evaluate(FSharpList<Value> args)
        {
            return FScheme.Value.NewString(Value);
        }

        public override string PrintExpression()
        {
            return "\"" + base.PrintExpression() + "\"";
        }

        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context); //Base implementation must be called
            if (context == SaveContext.Undo)
            {
                XmlElementHelper helper = new XmlElementHelper(element);
                helper.SetAttribute("stringValue", Value);
            }
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context); //Base implementation must be called
            if (context == SaveContext.Undo)
            {
                XmlElementHelper helper = new XmlElementHelper(element);
                Value = helper.ReadString("stringValue");
            }
        }

        #endregion
    }

    /// <summary>
    /// A class used to store a name and associated item for a drop down menu
    /// </summary>
    public class DynamoDropDownItem:IComparable
    {
        public string Name { get; set; }
        public object Item { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public DynamoDropDownItem(string name, object item)
        {
            Name = name;
            Item = item;
        }

        public int CompareTo(object obj)
        {
            var a = obj as DynamoDropDownItem;
            if (a == null)
                return 1;

            return this.Name.CompareTo(a);
        }

    }

    /// <summary>
    /// Base class for all nodes using a drop down
    /// </summary>
    public abstract partial class DropDrownBase : NodeWithOneOutput
    {
        protected ObservableCollection<DynamoDropDownItem> items = new ObservableCollection<DynamoDropDownItem>();
        public ObservableCollection<DynamoDropDownItem> Items
        {
            get { return items; }
            set
            {
                items = value;
                RaisePropertyChanged("Items");
            }
        }

        private int selectedIndex = 0;
        public int SelectedIndex
        {
            get { return selectedIndex; }
            set
            {
                //do not allow selected index to
                //go out of range of the items collection
                if (value > Items.Count - 1)
                {
                    selectedIndex = -1;
                }
                else
                    selectedIndex = value;
                RaisePropertyChanged("SelectedIndex");
            }
        }

        protected DropDrownBase()
        {
            Items.CollectionChanged += Items_CollectionChanged;
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            nodeElement.SetAttribute("index", SelectedIndex.ToString());
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            try
            {
                SelectedIndex = Convert.ToInt32(nodeElement.Attributes["index"].Value);
            }
            catch { }
        }

        public abstract void PopulateItems();

        /// <summary>
        /// When the dropdown is opened, the node's implementation of PopulateItemsHash is called
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void combo_DropDownOpened(object sender, EventArgs e)
        {
            PopulateItems();
        }

        /// <summary>
        /// Executed when the items collection has changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //SortItems();
        }

        /// <summary>
        /// The base behavior for the drop down node is to return the item at the selected index in the Items collection.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public override Value Evaluate(FSharpList<Value> args)
        {
            return Value.NewContainer(Items[SelectedIndex].Item);
        }
    }
}
