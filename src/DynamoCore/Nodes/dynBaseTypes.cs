using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using Dynamo.FSchemeInterop;
using Dynamo.FSchemeInterop.Node;
using Dynamo.Models;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using RestSharp.Contrib;
using Value = Dynamo.FScheme.Value;
using System.Globalization;
using ProtoCore.AST.AssociativeAST;
using Dynamo.DSEngine;

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

    #region FScheme Builtin Interop

    public abstract class BuiltinFunction : NodeWithOneOutput
    {
        public Func<FSharpList<Value>, Value> Func { get; protected internal set; }

        internal BuiltinFunction(FSharpFunc<FSharpList<Value>, Value> builtIn) 
            : this(builtIn.Invoke)
        { }

        internal BuiltinFunction(Func<FSharpList<Value>, Value> builtIn)
        {
            Func = builtIn;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
           return Func(args);
        }
    }

    #endregion

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

        private int _lastEvaledAmt;
        public override bool RequiresRecalc
        {
            get
            {
                return _lastEvaledAmt != InPortData.Count || base.RequiresRecalc;
            }
            set
            {
                base.RequiresRecalc = value;
            }
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

        protected override void OnEvaluate()
        {
            base.OnEvaluate();

            _lastEvaledAmt = InPortData.Count;
        }
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

        private int _lastEvaledAmt;
        public override bool RequiresRecalc
        {
            get
            {
                return _lastEvaledAmt != InPortData.Count || base.RequiresRecalc;
            }
            set
            {
                base.RequiresRecalc = value;
            }
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

        protected override void OnEvaluate()
        {
            base.OnEvaluate();

            _lastEvaledAmt = InPortData.Count;
        }
    }

    [NodeName("Identity")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT )]
    [NodeDescription("Identity function")]
    public class Identity : NodeWithOneOutput
    {
        public Identity()
        {
            InPortData.Add(new PortData("x", "in", typeof(object)));
            OutPortData.Add(new PortData("x", "out", typeof(object)));
            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            return args[0];
        }
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
    }

    #endregion

    #region Lists

    [NodeName("Reverse")]
    [NodeDescription("Reverses a list")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_MODIFY)]
    [NodeSearchTags("reverse", "end", "last")]
    public class Reverse : BuiltinFunction
    {
        public Reverse()
            : base(FScheme.Rev)
        {
            InPortData.Add(new PortData("list", "List to sort", typeof(Value.List)));
            OutPortData.Add(new PortData("rev", "Reversed list", typeof(Value.List)));

            RegisterAllPorts();
        }
    }

    [NodeName("List")]
    [NodeDescription("Makes a new list out of the given inputs")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_CREATE)]
    [NodeSearchTags("list")]
    public class NewList : VariableInput
    {
        public NewList()
        {
            InPortData.Add(new PortData("index0", "Item Index #0", typeof(object)));
            OutPortData.Add(new PortData("list", "A list", typeof(Value.List)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Disabled;
        }

        protected override string GetInputRootName()
        {
            return "index";
        }

        protected override string GetTooltipRootName()
        {
            return "Item Index #";
        }

        protected internal override void RemoveInput()
        {
            if (InPortData.Count > 1)
                base.RemoveInput();
        }

        protected override InputNode Compile(IEnumerable<string> portNames)
        {
            if (SaveResult)
                return base.Compile(portNames);
            else
                return new FunctionNode("list", portNames);
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            return ((Value.Function)Controller.FSchemeEnvironment.LookupSymbol("list"))
                .Item.Invoke(args);
        }
    }

    [NodeName("Sort with Comparator")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_MODIFY)]
    [NodeDescription("Returns a sorted list, using the given comparator function.")]
    public class SortWith : BuiltinFunction
    {
        public SortWith()
            : base(FScheme.SortWith)
        {
            InPortData.Add(new PortData("c(x, y)", "Comparator", typeof(object)));
            InPortData.Add(new PortData("list", "List to sort", typeof(Value.List)));
            OutPortData.Add(new PortData("sorted", "Sorted list", typeof(Value.List)));

            RegisterAllPorts();
        }
    }

    [NodeName("Sort by Key")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_MODIFY)]
    [NodeDescription("Returns a sorted list, using the given key mapper. The key mapper must return either all numbers or all strings.")]
    public class SortBy : BuiltinFunction
    {
        public SortBy()
            : base(FScheme.SortBy)
        {
            InPortData.Add(new PortData("f(x)", "Key Mapper", typeof(object), Value.NewFunction(Utils.ConvertToFSchemeFunc(FScheme.Identity))));
            InPortData.Add(new PortData("list", "List to sort", typeof(Value.List)));
            OutPortData.Add(new PortData("sorted", "Sorted list", typeof(Value.List)));

            RegisterAllPorts();
        }
    }

    [NodeName("Sort")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_MODIFY)]
    [NodeDescription("Returns a sorted list of numbers or strings.")]
    [NodeSearchTags("sort", "ascending", "order")]
    public class Sort : BuiltinFunction
    {
        public Sort()
            : base(FScheme.Sort)
        {
            InPortData.Add(new PortData("list", "List of numbers or strings to sort", typeof(Value.List)));
            OutPortData.Add(new PortData("sorted", "Sorted list", typeof(Value.List)));

            RegisterAllPorts();
        }
    }

    [NodeName("List Minimum")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_QUERY)]
    [NodeDescription("Returns the minimum value of a list, using the given key mapper. For all elements in the list, the key mapper must return either all numbers or all strings.")]
    public class ListMin : BuiltinFunction
    {
        public ListMin() 
            : base(FScheme.Min)
        {
            InPortData.Add(new PortData("f(x)", "Key Mapper", typeof(Value.Function), Value.NewFunction(Utils.ConvertToFSchemeFunc(FScheme.Identity))));
            InPortData.Add(new PortData("list", "List to get the minimum value of.", typeof(Value.List)));
            OutPortData.Add(new PortData("min", "Minimum value.", typeof(object)));

            RegisterAllPorts();
        }
    }

    [NodeName("List Maximum")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_QUERY)]
    [NodeDescription("Returns the maximum value of a list, using the given key mapper. For all elements in the list, the key mapper must return either all numbers or all strings.")]
    public class ListMax : BuiltinFunction
    {
        public ListMax()
            : base(FScheme.Max)
        {
            InPortData.Add(new PortData("f(x)", "Key Mapper", typeof(Value.Function), Value.NewFunction(Utils.ConvertToFSchemeFunc(FScheme.Identity))));
            InPortData.Add(new PortData("list", "List to get the maximum value of.", typeof(Value.List)));
            OutPortData.Add(new PortData("max", "Maximum value.", typeof(object)));

            RegisterAllPorts();
        }
    }

    [NodeName("Reduce")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_EVALUATE)]
    [NodeDescription("Reduces a list into a new value by combining each element with an accumulated result.")]
    [NodeSearchTags("fold")]
    public class Fold : BuiltinFunction
    {
        public Fold()
            : base(FScheme.FoldL)
        {
            InPortData.Add(new PortData("f(x, a)", "Reductor Function: first argument is an arbitrary item in the list being reduced, second is the current accumulated value, result is the new accumulated value.", typeof(object)));
            InPortData.Add(new PortData("a", "Starting accumulated value, to be passed into the first call to the Reductor function.", typeof(object)));
            InPortData.Add(new PortData("list", "List to reduce.", typeof(Value.List)));
            OutPortData.Add(new PortData("", "Result", typeof(object)));

            RegisterAllPorts();
        }
    }

    [NodeName("Filter")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_EVALUATE)]
    [NodeDescription("Filters a sequence by a given predicate \"p\" such that for an arbitrary element \"x\" p(x) = True.")]
    public class Filter : BuiltinFunction
    {
        public Filter()
            : base(FScheme.Filter)
        {
            InPortData.Add(new PortData("p(x)", "Predicate", typeof(object)));
            InPortData.Add(new PortData("seq", "Sequence to filter", typeof(Value.List)));
            OutPortData.Add(new PortData("filtered", "Sequence containing all elements \"x\" where p(x) = True", typeof(Value.List)));

            RegisterAllPorts();
        }
    }

    [NodeName("Filter Out")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_EVALUATE)]
    [NodeDescription("Filters a sequence by a given predicate \"p\" such that for an arbitrary element \"x\" p(x) = False.")]
    public class FilterOut : NodeWithOneOutput
    {
        public FilterOut()
        {
            InPortData.Add(new PortData("p(x)", "Predicate", typeof(Value.Function)));
            InPortData.Add(new PortData("seq", "Sequence to filter", typeof(Value.List)));
            OutPortData.Add(new PortData("filtered", "Sequence containing all elements \"x\" where p(x) = False", typeof(Value.List)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var p = ((Value.Function)args[0]).Item;
            var seq = ((Value.List)args[1]).Item;

            return Value.NewList(Utils.SequenceToFSharpList(
                seq.Where(x => !FScheme.ValueToBool(p.Invoke(Utils.MakeFSharpList(x))))));
        }
    }

    [NodeName("Number Range")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_CREATE)]
    [NodeDescription("Creates a sequence of numbers in the specified range.")]
    [NodeSearchTags("number", "range", "sequence", "series")]
    [AlsoKnownAs("Dynamo.Nodes.dynBuildSeq", "Dynamo.Nodes.BuildSeq")]
    public class NumberRange : BuiltinFunction
    {
        public NumberRange()
            : base(FScheme.BuildSeq)
        {
            InPortData.Add(new PortData("start", "Number to start the sequence at", typeof(Value.Number)));
            InPortData.Add(new PortData("end", "Number to end the sequence at", typeof(Value.Number)));
            InPortData.Add(new PortData("step", "Space between numbers", typeof(Value.Number)));
            OutPortData.Add(new PortData("seq", "New sequence", typeof(Value.List)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
        }
    }

    [NodeName("Number Sequence")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_CREATE)]
    [NodeDescription("Creates a sequence of numbers.")]
    [NodeSearchTags("number", "range", "sequence", "series")]
    public class NumberSeq : NodeWithOneOutput
    {
        public NumberSeq()
        {
            InPortData.Add(new PortData("start", "Number to start the sequence at", typeof(Value.Number)));
            InPortData.Add(new PortData("amount", "Amount of numbers in the sequence", typeof(Value.Number)));
            InPortData.Add(new PortData("step", "Space between numbers", typeof(Value.Number)));
            OutPortData.Add(new PortData("seq", "New sequence", typeof(Value.List)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var start = ((Value.Number)args[0]).Item;
            var amount = (int)((Value.Number)args[1]).Item;
            var step = ((Value.Number)args[2]).Item;

            return Value.NewList(Utils.SequenceToFSharpList(MakeSequence(start, amount, step)));
        }

        private IEnumerable<Value> MakeSequence(double start, int amount, double step)
        {
            for (int i = 0; i < amount; i++)
            {
                yield return Value.NewNumber(start);
                start += step;
            }
        }
    }

    [NodeName("Combine")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_EVALUATE)]
    [NodeDescription("Applies a combinator to each element in two sequences")]
    [NodeSearchTags("zip")]
    public class Combine : VariableInput
    {
        public Combine()
        {
            InPortData.Add(new PortData("comb", "Combinator", typeof(object)));
            InPortData.Add(new PortData("list1", "List #1", typeof(Value.List)));
            InPortData.Add(new PortData("list2", "List #2", typeof(Value.List)));
            OutPortData.Add(new PortData("combined", "Combined lists", typeof(Value.List)));

            RegisterAllPorts();
            ArgumentLacing = LacingStrategy.Disabled;
        }

        protected override string GetInputRootName()
        {
            return "list";
        }

        protected override string GetTooltipRootName()
        {
            return "List #";
        }

        protected internal override void RemoveInput()
        {
            //don't allow us to remove
            //the second list
            if (InPortData.Count == 3)
                return;

            if (InPortData.Count == 3)
                InPortData[1] = new PortData("lists", "List of lists to combine", typeof(object));
            if (InPortData.Count > 3)
                base.RemoveInput();
        }

        protected internal override void AddInput()
        {
            if (InPortData.Count == 2)
                InPortData[1] = new PortData("list1", "First list", typeof(object));
            base.AddInput();
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            nodeElement.SetAttribute("inputs", (InPortData.Count - 1).ToString());
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            var inputAttr = nodeElement.Attributes["inputs"];
            int inputs = inputAttr == null ? 2 : Convert.ToInt32(inputAttr.Value);
            if (inputs == 1)
                RemoveInput();
            else
            {
                for (; inputs > 2; inputs--)
                {
                    InPortData.Add(new PortData(GetInputRootName() + GetInputNameIndex(), "", typeof(object)));
                }

                RegisterAllPorts();
            }
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            return FScheme.Map(args);
        }
    }

    public class LacerBase : VariableInput
    {
        private readonly Func<FSharpList<Value>, Value> _func;

        public LacerBase(Func<FSharpList<Value>, Value> func)
        {
            _func = func;

            InPortData.Add(new PortData("comb", "Combinator", typeof(object)));
            InPortData.Add(new PortData("list1", "List #1", typeof(Value.List)));
            InPortData.Add(new PortData("list2", "List #2", typeof(Value.List)));
            OutPortData.Add(new PortData("combined", "Combined lists", typeof(Value.List)));

            RegisterAllPorts();
        }

        protected override string GetInputRootName()
        {
            return "list";
        }

        protected override string GetTooltipRootName()
        {
            return "List #";
        }

        protected internal override void RemoveInput()
        {
            if (InPortData.Count == 3)
                InPortData[1] = new PortData("lists", "List of lists to combine", typeof(object));
            if (InPortData.Count > 2)
                base.RemoveInput();
        }

        protected internal override void AddInput()
        {
            if (InPortData.Count == 2)
                InPortData[1] = new PortData("list1", "First list", typeof(object));
            base.AddInput();
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            nodeElement.SetAttribute("inputs", (InPortData.Count - 1).ToString());
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            var inputAttr = nodeElement.Attributes["inputs"];
            int inputs = inputAttr == null ? 2 : Convert.ToInt32(inputAttr.Value);
            if (inputs == 1)
                RemoveInput();
            else
            {
                for (; inputs > 2; inputs--)
                {
                    InPortData.Add(new PortData(GetInputRootName() + GetInputNameIndex(), "", typeof(object)));
                }

                RegisterAllPorts();
            }
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            return _func(args);
        }
    }

    [NodeName("Cartesian Product")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_EVALUATE)]
    [NodeDescription("Applies a combinator to each pair in the cartesian product of two sequences")]
    [NodeSearchTags("cross")]
    public class CartProd : LacerBase
    {
        public CartProd() : base(FScheme.CartProd) { }
    }

    [NodeName("Lace Shortest")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_EVALUATE)]
    [NodeDescription("Applies a combinator to each pair resulting from a shortest lacing of the input lists. All lists are truncated to the length of the shortest input.")]
    public class LaceShortest : LacerBase
    {
        public LaceShortest() : base(FScheme.LaceShortest) { }
    }

    [NodeName("Lace Longest")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_EVALUATE)]
    [NodeDescription("Applies a combinator to each pair resulting from a longest lacing of the input lists. All lists have their last element repeated to match the length of the longest input.")]
    public class LaceLongest : LacerBase
    {
        public LaceLongest() : base(FScheme.LaceLongest) { }
    }

    [NodeName("Map")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_EVALUATE)]
    [NodeDescription("Applies a function over all elements of a list, generating a new list from the results.")]
    public class Map : BuiltinFunction
    {
        public Map()
            : base(FScheme.Map)
        {
            InPortData.Add(new PortData("f(x)", "The procedure used to map elements", typeof(object)));
            InPortData.Add(new PortData("seq", "The sequence to map over.", typeof(Value.List)));
            OutPortData.Add(new PortData("mapped", "Mapped sequence", typeof(Value.List)));

            RegisterAllPorts();
        }
    }

    [NodeName("For Each")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_EVALUATE)]
    [NodeDescription("Performs a computation on each element of a list. Does not accumulate results.")]
    public class ForEach : BuiltinFunction
    {
        public ForEach()
            : base(FScheme.ForEach)
        {
            InPortData.Add(new PortData("f(x)", "The computation to perform on each element", typeof(object)));
            InPortData.Add(new PortData("seq", "The list to of elements.", typeof(Value.List)));
            OutPortData.Add(new PortData("", "", typeof(Value.Dummy)));

            RegisterAllPorts();
        }
    }

    [NodeName("True For All")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_QUERY)]
    [NodeDescription("Tests to see if all elements in a sequence satisfy the given predicate.")]
    public class AndMap : BuiltinFunction
    {
        public AndMap()
            : base(FScheme.AndMap)
        {
            InPortData.Add(new PortData("p(x)", "The predicate used to test elements", typeof(object)));
            InPortData.Add(new PortData("seq", "The sequence to test.", typeof(Value.List)));
            OutPortData.Add(new PortData("all?", "Whether or not all elements satisfy the given predicate.", typeof(Value.List)));

            RegisterAllPorts();
        }
    }

    [NodeName("True For Any")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_QUERY)]
    [NodeDescription("Tests to see if any elements in a sequence satisfy the given predicate.")]
    public class OrMap : BuiltinFunction
    {
        public OrMap()
            : base(FScheme.OrMap)
        {
            InPortData.Add(new PortData("p(x)", "The predicate used to test elements", typeof(object)));
            InPortData.Add(new PortData("seq", "The sequence to test.", typeof(Value.List)));
            OutPortData.Add(new PortData("any?", "Whether or not any elements satisfy the given predicate.", typeof(Value.List)));

            RegisterAllPorts();
        }
    }

    [NodeName("Split List")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_MODIFY)]
    [NodeDescription("Deconstructs a list pair.")]
    public class DeCons : NodeModel
    {
        public DeCons()
        {
            InPortData.Add(new PortData("list", "A non-empty list", typeof(Value.List)));
            OutPortData.Add(new PortData("first", "Head of the list", typeof(object)));
            OutPortData.Add(new PortData("rest", "Tail of the list", typeof(Value.List)));

            RegisterAllPorts();
        }

        public override void Evaluate(FSharpList<Value> args, Dictionary<PortData, Value> outPuts)
        {
            var list = (Value.List)args[0];

            outPuts[OutPortData[0]] = list.Item.Head;
            outPuts[OutPortData[1]] = Value.NewList(list.Item.Tail);
        }
    }

    [NodeName("Add to List")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_MODIFY)]
    [NodeDescription("Adds an element to the beginning of a list.")]
    [NodeSearchTags("cons", "pair")]
    public class List : BuiltinFunction
    {
        public List()
            : base(FScheme.Cons)
        {
            InPortData.Add(new PortData("item", "The Head of the new list", typeof(object)));
            InPortData.Add(new PortData("list", "The Tail of the new list", typeof(object)));
            OutPortData.Add(new PortData("list", "New list", typeof(Value.List)));

            RegisterAllPorts();
        }
    }

    [NodeName("Take from List")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_MODIFY)]
    [NodeDescription("Takes elements from a list")]
    public class TakeList : BuiltinFunction
    {
        public TakeList()
            : base(FScheme.Take)
        {
            InPortData.Add(new PortData("amt", "Amount of elements to extract", typeof(object)));
            InPortData.Add(new PortData("list", "The list to extract elements from", typeof(Value.List)));
            OutPortData.Add(new PortData("elements", "List of extraced elements", typeof(Value.List)));

            RegisterAllPorts();
        }
    }

    [NodeName("Drop from List")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_MODIFY)]
    [NodeDescription("Drops elements from a list")]
    public class DropList : BuiltinFunction
    {
        public DropList()
            : base(FScheme.Drop)
        {
            InPortData.Add(new PortData("amt", "Amount of elements to remote", typeof(object)));
            InPortData.Add(new PortData("list", "The list to drop elements from", typeof(Value.List)));
            OutPortData.Add(new PortData("elements", "List of remaining elements", typeof(Value.List)));

            RegisterAllPorts();
        }
    }

    [NodeName("Shift List Indices")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_MODIFY)]
    [NodeDescription("Shifts the indices of a list by a given amount.")]
    public class ShiftList : NodeWithOneOutput
    {
        public ShiftList()
        {
            InPortData.Add(
                new PortData("amt", "Amount to shift the list indices by.", typeof(Value.Number)));
            InPortData.Add(new PortData("list", "List to shift indices of.", typeof(Value.List)));
            OutPortData.Add(new PortData("list", "Shifted list", typeof(Value.List)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var amt = (int)((Value.Number)args[0]).Item;
            var list = ((Value.List)args[1]).Item;

            if (amt == 0)
                return Value.NewList(list);

            if (amt < 0)
            {
                return Value.NewList(
                    Utils.SequenceToFSharpList(
                        list.Skip(-amt).Concat(list.Take(-amt))));
            }

            var len = list.Length;
            return Value.NewList(
                Utils.SequenceToFSharpList(
                    list.Skip(len - amt).Concat(list.Take(len - amt))));
        }
    }

    [NodeName("Get from List")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_QUERY)]
    [NodeDescription("Gets an element from a list at a specified index.")]
    public class GetFromList : NodeWithOneOutput
    {
        public GetFromList()
        {
            InPortData.Add(new PortData("index", "Index of the element to extract", typeof(object)));
            InPortData.Add(new PortData("list", "The list to extract the element from", typeof(Value.List)));
            OutPortData.Add(new PortData("element", "Extracted element", typeof(object)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Disabled;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var indices = args[0];
            var lst = ((Value.List)args[1]).Item;

            if (indices.IsNumber)
            {
                var idx = (int)(indices as Value.Number).Item;
                return ListModule.Get(lst, idx);
            }
            else if (indices.IsList)
            {
                var idxs = (indices as Value.List).Item.Select(x => (int)((Value.Number)x).Item);
                return
                    Value.NewList(
                        Utils.SequenceToFSharpList(idxs.Select(i => ListModule.Get(lst, i))));
            }
            else
            {
                throw new Exception("\"index\" argument not a number or a list of numbers.");
            }
        }
    }

    [NodeName("Slice List")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_MODIFY)]
    [NodeDescription("Get a sublist from a given list.")]
    public class SliceList : NodeWithOneOutput
    {
        public SliceList()
        {
            InPortData.Add(new PortData("start", "Inclusive start index", typeof(object)));
            InPortData.Add(new PortData("count", "Number of elements to obtain from list", typeof(object)));
            InPortData.Add(new PortData("list", "The list to extract the elements from", typeof(Value.List)));
            OutPortData.Add(new PortData("element", "Extracted elements", typeof(object)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var start = (int)((Value.Number)args[0]).Item;
            var count = (int)((Value.Number)args[1]).Item;
            var lst = ((Value.List)args[2]).Item;

            return Value.NewList(Utils.SequenceToFSharpList(lst.Skip(start).Take(count)));
        }
    }

    [NodeName("Remove From List")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_MODIFY)]
    [NodeDescription("Removes an element from a list at a specified index.")]
    public class RemoveFromList : NodeWithOneOutput
    {
        public RemoveFromList()
        {
            InPortData.Add(new PortData("index", "Index of the element to remove", typeof(object)));
            InPortData.Add(new PortData("list", "The list to remove the element from", typeof(Value.List)));
            OutPortData.Add(new PortData("list", "List with element removed", typeof(object)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Disabled;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var indices = args[0];
            var lst = ((Value.List)args[1]).Item;

            if (indices.IsNumber)
            {
                var idx = (int)(indices as Value.Number).Item;
                return Value.NewList(Utils.SequenceToFSharpList(lst.Where((_, i) => i != idx)));
            }
            else if (indices.IsList)
            {
                var idxs =
                    new HashSet<int>(
                        (indices as Value.List).Item.Select(x => (int)((Value.Number)x).Item));
                return
                    Value.NewList(
                        Utils.SequenceToFSharpList(lst.Where((_, i) => !idxs.Contains(i))));
            }
            else
            {
                throw new Exception("\"index\" argument not a number or a list of numbers.");
            }
        }
    }

    [NodeName("Drop Every Nth")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_MODIFY)]
    [NodeDescription("Removes every nth element from a list.")]
    public class RemoveEveryNth : NodeWithOneOutput
    {
        public RemoveEveryNth()
        {
            InPortData.Add(new PortData("n", "All indices that are a multiple of this number will be removed.", typeof(object)));
            InPortData.Add(new PortData("list", "The list to remove elements from.", typeof(Value.List)));
            InPortData.Add(new PortData("offset", "Skip this amount before removing every Nth.", typeof(Value.Number), Value.NewNumber(0)));
            OutPortData.Add(new PortData("list", "List with elements removed.", typeof(object)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var n = (int)((Value.Number)args[0]).Item;
            var lst = ((Value.List)args[1]).Item;
            var offset = (int)((Value.Number)args[2]).Item;

            return Value.NewList(Utils.SequenceToFSharpList(lst.Skip(offset).Where((_, i) => (i + 1) % n != 0)));
        }
    }

    [NodeName("Take Every Nth")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_MODIFY)]
    [NodeDescription("Extracts every nth element from a list.")]
    public class TakeEveryNth : NodeWithOneOutput
    {
        public TakeEveryNth()
        {
            InPortData.Add(new PortData("n", "All indices that are a multiple of this number will be extracted.", typeof(object)));
            InPortData.Add(new PortData("list", "The list to extract elements from.", typeof(Value.List)));
            InPortData.Add(new PortData("offset", "Skip this amount before taking every Nth.", typeof(Value.Number), Value.NewNumber(0)));
            OutPortData.Add(new PortData("list", "Extracted elements.", typeof(object)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var n = (int)((Value.Number)args[0]).Item;
            var lst = ((Value.List)args[1]).Item;
            var offset = (int)((Value.Number)args[2]).Item;

            return Value.NewList(Utils.SequenceToFSharpList(lst.Skip(offset).Where((_, i) => (i + 1) % n == 0)));
        }
    }

    [NodeName("Empty List")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_CREATE)]
    [NodeDescription("An empty list")]
    [IsInteractive(false)]
    public class Empty : NodeModel
    {
        public Empty()
        {
            OutPortData.Add(new PortData("empty", "An empty list", typeof(Value.List)));

            RegisterAllPorts();
        }

        public override bool RequiresRecalc
        {
            get
            {
                return false;
            }
            set { }
        }

        protected internal override INode Build(Dictionary<NodeModel, Dictionary<int, INode>> preBuilt, int outPort)
        {
            Dictionary<int, INode> result;
            if (!preBuilt.TryGetValue(this, out result))
            {
                result = new Dictionary<int, INode>();
                result[outPort] = new SymbolNode("empty");
                preBuilt[this] = result;
            }
            return result[outPort];
        }
    }

    [NodeName("Is Empty List?")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_QUERY)]
    [NodeDescription("Checks to see if the given list is empty.")]
    public class IsEmpty : BuiltinFunction
    {
        public IsEmpty()
            : base(FScheme.IsEmpty)
        {
            InPortData.Add(new PortData("list", "A list", typeof(Value.List)));
            OutPortData.Add(new PortData("empty?", "Is the given list empty?", typeof(bool)));

            RegisterAllPorts();
        }
    }

    [NodeName("List Length")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_QUERY)]
    [NodeDescription("Gets the length of a list")]
    [NodeSearchTags("count")]
    public class Length : BuiltinFunction
    {
        public Length()
            : base(FScheme.Len)
        {
            InPortData.Add(new PortData("list", "A list", typeof(Value.List)));
            OutPortData.Add(new PortData("length", "Length of the list", typeof(object)));

            RegisterAllPorts();
        }
    }

    [NodeName("Join Lists")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_MODIFY)]
    [NodeDescription("Concatenates two lists.")]
    [NodeSearchTags("append", "extend", "concat")]
    public class Append : BuiltinFunction
    {
        public Append()
            : base(FScheme.Append)
        {
            InPortData.Add(new PortData("listA", "First list", typeof(Value.List)));
            InPortData.Add(new PortData("listB", "Second list", typeof(Value.List)));
            OutPortData.Add(new PortData("A+B", "A appended onto B", typeof(Value.List)));

            RegisterAllPorts();
        }
    }

    [NodeName("First of List")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_QUERY)]
    [NodeDescription("Gets the Head of a list")]
    [NodeSearchTags("car")]
    public class First : BuiltinFunction
    {
        public First()
            : base(FScheme.Car)
        {
            InPortData.Add(new PortData("list", "A list", typeof(Value.List)));
            OutPortData.Add(new PortData("first", "First element in the list", typeof(object)));

            RegisterAllPorts();
        }
    }

    [NodeName("Rest of List")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_QUERY)]
    [NodeDescription("Gets the Tail of a list (list with the first element removed).")]
    [NodeSearchTags("cdr")]
    public class Rest : BuiltinFunction
    {
        public Rest()
            : base(FScheme.Cdr)
        {
            InPortData.Add(new PortData("list", "A list", typeof(Value.List)));
            OutPortData.Add(new PortData("rest", "Tail of the list.", typeof(Value.List)));

            RegisterAllPorts();
        }
    }

    [NodeName("Partition List")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_MODIFY)]
    [NodeDescription("Create a lists of lists with each sub-list containing n elements.")]
    public class Slice : NodeWithOneOutput
    {
        public Slice()
        {
            InPortData.Add(new PortData("list", "A list", typeof(Value.List)));
            InPortData.Add(new PortData("n", "The length of each new sub list.", typeof(Value.List)));
            OutPortData.Add(new PortData("list", "A list of lists of length n.", typeof(Value.List)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            if(!args[0].IsList)
                throw new Exception("A list is required to slice.");
            if(args.Length != 2)
                throw new Exception("A number is required to specify the sublist length.");

            FSharpList<Value> lst = ((Value.List)args[0]).Item;
            var n = (int)Math.Round(((Value.Number)args[1]).Item);

            //if we have less elements in ther 
            //incoming list than the slice size,
            //just return the list
            if (lst.Count() < n)
            {
                return Value.NewList(lst);
            }

            var finalList = new List<Value>();
            var currList = new List<Value>();
            int count = 0;

            foreach (Value v in lst)
            {
                count++;

                currList.Add(v);

                if (count == n)
                {
                    finalList.Add(Value.NewList(Utils.SequenceToFSharpList(currList)));
                    currList = new List<Value>();
                    count = 0;
                }
            }

            if (currList.Any())
            {
                finalList.Add(Value.NewList(Utils.SequenceToFSharpList(currList)));
            }

            return Value.NewList(Utils.SequenceToFSharpList(finalList));
        }
    }

    [NodeName("Diagonal Right List")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_MODIFY)]
    [NodeDescription("Create a diagonal lists of lists from top left to lower right.")]
    public class DiagonalRightList : NodeWithOneOutput
    {
        public DiagonalRightList()
        {
            InPortData.Add(new PortData("list", "A list", typeof(Value.List)));
            InPortData.Add(new PortData("n", "The width of the new sub lists.", typeof(Value.List)));
            OutPortData.Add(new PortData("list", "A list of lists representing diagonals.", typeof(Value.List)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            if (!args[0].IsList)
                throw new Exception("A list is required to create diagonals.");

            FSharpList<Value> lst = ((Value.List)args[0]).Item;
            var n = (int)Math.Round(((Value.Number)args[1]).Item);

            //if we have less elements in the
            //incoming list than the slice size,
            //just return the list
            if (lst.Count() < n)
            {
                return Value.NewList(lst);
            }

            var finalList = new List<Value>();
            var currList = new List<Value>();

            //int count = 0;

            var startIndices = new List<int>();
            
            //get indices along 'side' of array
            for (int i = n; i < lst.Count(); i += n)
            {
                startIndices.Add(i);
            }

            startIndices.Reverse();

            //get indices along 'top' of array
            for (int i = 0; i < n; i++)
            {
                startIndices.Add(i);
            }

            foreach(int start in startIndices)
            {
                int index = start;

                while (index < lst.Count())
                {
                    var currentRow = (int)Math.Ceiling((index + 1)/(double)n);
                    currList.Add(lst.ElementAt(index));
                    index += n + 1;

                    //ensure we are skipping a row to get the next index
                    var nextRow = (int) Math.Ceiling((index + 1)/(double)n);
                    if (nextRow > currentRow + 1 || nextRow == currentRow)
                        break;
                }
                finalList.Add(Value.NewList(Utils.SequenceToFSharpList(currList)));
                currList = new List<Value>();
            }

            if (currList.Any())
            {
                finalList.Add(Value.NewList(Utils.SequenceToFSharpList(currList)));
            }

            return Value.NewList(Utils.SequenceToFSharpList(finalList));

        }
    }

    [NodeName("Diagonal Left List")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_MODIFY)]
    [NodeDescription("Create a diagonal lists of lists from top right to lower left.")]
    public class DiagonalLeftList : NodeWithOneOutput
    {
        public DiagonalLeftList()
        {
            InPortData.Add(new PortData("list", "A list", typeof(Value.List)));
            InPortData.Add(new PortData("n", "The width of the new sublists.", typeof(Value.List)));
            OutPortData.Add(new PortData("list", "A list of lists representing diagonals.", typeof(Value.List)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            if (!args[0].IsList)
                throw new Exception("A list is required to create diagonals.");

            FSharpList<Value> lst = ((Value.List)args[0]).Item;
            var n = (int)Math.Round(((Value.Number)args[1]).Item);

            //if we have less elements in the
            //incoming list than the slice size,
            //just return the list
            if (lst.Count() < n)
            {
                return Value.NewList(lst);
            }

            var finalList = new List<Value>();
            var currList = new List<Value>();

            //int count = 0;

            var startIndices = new List<int>();

            //get indices along 'top' of array
            for (int i = 0; i < n; i++)
            {
                startIndices.Add(i);
            }

            //get indices along 'side' of array
            for (int i = n-1 + n; i < lst.Count(); i += n)
            {
                startIndices.Add(i);
            }

            foreach (int start in startIndices)
            {
                int index = start;

                while (index < lst.Count())
                {
                    var currentRow = (int)Math.Ceiling((index + 1) / (double)n);
                    currList.Add(lst.ElementAt(index));
                    index += n - 1;

                    //ensure we are skipping a row to get the next index
                    var nextRow = (int)Math.Ceiling((index + 1) / (double)n);
                    if (nextRow > currentRow + 1 || nextRow == currentRow)
                        break;
                }
                finalList.Add(Value.NewList(Utils.SequenceToFSharpList(currList)));
                currList = new List<Value>();
            }

            if (currList.Any())
            {
                finalList.Add(Value.NewList(Utils.SequenceToFSharpList(currList)));
            }

            return Value.NewList(Utils.SequenceToFSharpList(finalList));

        }
    }

    [NodeName("Transpose Lists")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_MODIFY)]
    [NodeDescription("Swaps rows and columns in a list of lists.")]
    public class Transpose : BuiltinFunction
    {
        public Transpose() 
            : base(FScheme.Transpose)
        {
            InPortData.Add(new PortData("lists", "The list of lists to transpose.", typeof(Value.List)));
            OutPortData.Add(new PortData("", "Transposed list of lists.", typeof(Value.List)));

            RegisterAllPorts();
        }
    }

    [NodeName("Build Sublists")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_CREATE)]
    [NodeDescription("Build sublists from a list using a list-building syntax.")]
    public partial class Sublists : BasicInteractive<string>
    {
        public Sublists()
        {
            InPortData.Add(new PortData("list", "The list from which to create sublists.", typeof(Value.List)));
            InPortData.Add(new PortData("offset", "The offset to apply to the sub-list. Ex. \"0..3\" with an offset of 1 will yield {0,1,2,3}{1,2,3,4}{2,3,4,5}...", typeof(Value.List)));

            OutPortData.RemoveAt(0); //remove the existing blank output
            OutPortData.Add(new PortData("list", "The sublists.", typeof(Value.List)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
            Value = "";
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            base.LoadNode(nodeElement);
            processTextForNewInputs();
        }

        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context); //Base implementation must be called
            if (context == SaveContext.Undo)
            {
                XmlElementHelper helper = new XmlElementHelper(element);
                helper.SetAttribute("value", Value);
            }
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context); //Base implementation must be called
            processTextForNewInputs();
            if (context == SaveContext.Undo)
            {
                XmlElementHelper helper = new XmlElementHelper(element);
                Value = helper.ReadString("value");
            }
        }

        #endregion

        private void processTextForNewInputs()
        {
            var parameters = new List<string>();

            try
            {
                _parsed = DoubleInput.ParseValue(Value, new[] { ',' }, parameters, TokenConvert);

                if (InPortData.Count > 2)
                    InPortData.RemoveRange(2, InPortData.Count - 2);

                foreach (string parameter in parameters)
                {
                    InPortData.Add(new PortData(parameter, "variable", typeof(Value.Number)));
                }

                RegisterInputPorts();
            }
            catch (Exception e)
            {
                Error(e.Message);
            }
        }

        private double TokenConvert(double value)
        {
            return value;
        }

        internal static readonly Regex IdentifierPattern = new Regex(@"(?<id>[a-zA-Z_][^ ]*)|\[(?<id>\w(?:[^}\\]|(?:\\}))*)\]");
        internal static readonly string[] RangeSeparatorTokens = { "..", ":", };
        private List<DoubleInput.IDoubleSequence> _parsed;

        private static List<Tuple<int, int, int>> processText(string text, int maxVal, Func<string, int> idFoundCallback)
        {
            text = text.Replace(" ", "");

            string[] chunks = text.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            if (!chunks.Any())
                throw new Exception("Sub-list expression could not be parsed.");

            var ranges = new List<Tuple<int, int, int>>();

            foreach (string chunk in chunks)
            {
                string[] valueRange = chunk.Split(RangeSeparatorTokens, StringSplitOptions.RemoveEmptyEntries);

                int start = 0;
                int step = 1;

                if (!int.TryParse(valueRange[0], out start))
                {
                    var match = IdentifierPattern.Match(valueRange[0]);
                    if (match.Success)
                    {
                        start = idFoundCallback(match.Groups["id"].Value);
                    }
                    else
                    {
                        throw new Exception("Range start could not be parsed.");
                    }
                }

                int end = start;

                if (valueRange.Length > 1)
                {
                    if (!int.TryParse(valueRange[1], out end))
                    {
                        var match = IdentifierPattern.Match(valueRange[1]);
                        if (match.Success)
                        {
                            end = idFoundCallback(match.Groups["id"].Value);
                        }
                        else
                        {
                            throw new Exception("Range " + (valueRange.Length > 2 ? "step" : "end") + "could not be parsed.");
                        }
                    }
                }

                if (valueRange.Length > 2)
                {
                    if (!int.TryParse(valueRange[2], out end))
                    {
                        var match = IdentifierPattern.Match(valueRange[2]);
                        if (match.Success)
                        {
                            step = idFoundCallback(match.Groups["id"].Value);
                        }
                        else
                        {
                            throw new Exception("Range end could not be parsed.");
                        }
                    }
                }

                if (start < 0 || end < 0 || step <= 0)
                    throw new Exception("Range values must be greater than zero.");

                //if any values are greater than the length of the list - fail
                if (start >= maxVal || end >= maxVal)
                    throw new Exception("The start or end of a range is greater than the number of available elements in the list.");

                ranges.Add(Tuple.Create(start, end, step));
            }

            return ranges;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var list = ((Value.List)args[0]).Item;
            var len = list.Length;
            var offset = Convert.ToInt32(((Value.Number)args[1]).Item);

            if (offset <= 0)
                throw new Exception("\"" + InPortData[1].NickName + "\" argument must be greater than zero.");

            //sublist creation semantics are as follows:
            //EX. 1..2,5..8
            //This expression says give me elements 1-2 then jump 3 and give me elements 5-8
            //For a list 1,2,3,4,5,6,7,8,9,10, this will give us
            //1,2,5,8,2,3,6,9

            var paramLookup = args.Skip(2)
                                  .Select(
                                      (x, i) => new { Name = InPortData[i+2].NickName, Argument = x })
                                  .ToDictionary(x => x.Name, x => ((Value.Number)x.Argument).Item);

            var ranges = _parsed
                .Select(x => x.GetValue(paramLookup).Select(Convert.ToInt32).ToList())
                .ToList();

            //move through the list, creating sublists
            var finalList = new List<Value>();

            for (int j = 0; j < len; j+=offset)
            {
                var currList = new List<Value>();

                var query = ranges.Where(r => r[0] + j <= len - 1 && r.Last() + j <= len - 1);
                foreach (var range in query)
                {
                    currList.AddRange(range.Select(i => list.ElementAt(j+i)));
                }

                if (currList.Any())
                    finalList.Add(FScheme.Value.NewList(Utils.SequenceToFSharpList(currList)));
            }

            return FScheme.Value.NewList(Utils.SequenceToFSharpList(finalList));
        }

        protected override string DeserializeValue(string val)
        {
            return val;
        }
    }

    [NodeName("Repeat")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_CREATE)]
    [NodeDescription("Construct a list of a given item repeated a given number of times.")]
    public class Repeat : NodeWithOneOutput
    {
        public Repeat()
        {
            InPortData.Add(new PortData("thing", "The thing to repeat. This can be a single object or a list.", typeof(Value)));
            InPortData.Add(new PortData("length", "The number of times to repeat.", typeof(Value.Number)));
            OutPortData.Add(new PortData("list", "The list.", typeof(Value.List)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            int n = Convert.ToInt16(((Value.Number) args[1]).Item);

            if(n<0)
                throw new Exception("Can't make a repeated list of a negative amount.");

            return Value.NewList(Utils.SequenceToFSharpList(Enumerable.Repeat(args[0], n).ToList()));
        }
    }

    [NodeName("Flatten Completely")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_MODIFY)]
    [NodeDescription("Flatten nested lists into one list.")]
    public class FlattenList : NodeWithOneOutput
    {
        public FlattenList()
        {
            InPortData.Add(new PortData("list", "The list of lists to flatten.", typeof(Value.List)));
            OutPortData.Add(new PortData("list", "The flattened list.", typeof(Value.List)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Disabled;
        }

        internal static IEnumerable<Value> Flatten(IEnumerable<Value> list, ref int amt)
        {
            while (amt != 0)
            {
                bool keepFlattening = false;

                list = list.SelectMany<Value, Value>(
                    x =>
                    {
                        if (x is Value.List)
                        {
                            keepFlattening = true;
                            return (x as Value.List).Item;
                        }
                        return new[] { x };
                    }).ToList();

                if (keepFlattening)
                    amt--;
                else
                    break;
            }
            return list;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            if (!args[0].IsList)
                throw new Exception("A list is required to flatten.");

            IEnumerable<Value> list = ((Value.List)args[0]).Item;

            int amt = -1;
            return Value.NewList(Utils.SequenceToFSharpList(Flatten(list, ref amt)));
        }
    }

    [NodeName("Flatten")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_MODIFY)]
    [NodeDescription("Flatten nested lists into one list.")]
    public class FlattenListAmt : NodeWithOneOutput
    {
        public FlattenListAmt()
        {
            InPortData.Add(new PortData("list", "The list of lists to flatten.", typeof(Value.List)));
            InPortData.Add(new PortData("amt", "Amount of nesting to remove.", typeof(Value.Number)));

            OutPortData.Add(new PortData("list", "The flattened list.", typeof(Value.List)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Disabled;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            if (!args[0].IsList)
                throw new Exception("A list is required to flatten.");

            IEnumerable<Value> list = ((Value.List)args[0]).Item;

            var oldAmt = Convert.ToInt32(((Value.Number)args[1]).Item);

            if (oldAmt < 0)
                throw new Exception("Cannot flatten a list by a negative amount.");

            var amt = oldAmt;
            var result = FlattenList.Flatten(list, ref amt);

            if (amt > 0)
                throw new Exception("List not nested enough to flatten by given amount. Nesting Amt = " + (oldAmt - amt) + ", Given Amt = " + oldAmt);

            return Value.NewList(Utils.SequenceToFSharpList(result));
        }
    }

    #endregion

    #region Boolean

    public abstract class Comparison : BuiltinFunction
    {
        protected Comparison(FSharpFunc<FSharpList<Value>, Value> op, string name)
            : base(op)
        {
            InPortData.Add(new PortData("x", "operand", typeof(Value.Number)));
            InPortData.Add(new PortData("y", "operand", typeof(Value.Number)));
            OutPortData.Add(new PortData("x" + name + "y", "comp", typeof(Value.Number)));

            RegisterAllPorts();
        }
    }

    [NodeName("Less Than")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_COMPARISON)]
    [NodeDescription("Compares two numbers.")]
    [NodeSearchTags("less", "than", "<")]
    public class LessThan : Comparison
    {
        public LessThan() : base(FScheme.LT, "<") { }
    }

    [NodeName("Less Than Or Equal")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_COMPARISON)]
    [NodeDescription("Compares two numbers.")]
    [NodeSearchTags("<=")]
    public class LessThanEquals : Comparison
    {
        public LessThanEquals() : base(FScheme.LTE, "≤") { }
    }

    [NodeName("Greater Than")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_COMPARISON)]
    [NodeDescription("Compares two numbers.")]
    [NodeSearchTags(">")]
    public class GreaterThan : Comparison
    {
        public GreaterThan() : base(FScheme.GT, ">") { }
    }

    [NodeName("Greater Than Or Equal")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_COMPARISON)]
    [NodeDescription("Compares two numbers.")]
    [NodeSearchTags(">=", "Greater Than Or Equal")]
    public class GreaterThanEquals : Comparison
    {
        public GreaterThanEquals() : base(FScheme.GTE, "≥") { }
    }

    [NodeName("Equal")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_COMPARISON)]
    [NodeDescription("Compares two numbers.")]
    [NodeSearchTags("=")]
    public class Equal : Comparison
    {
        public Equal() : base(FSharpFunc<FSharpList<Value>, Value>.FromConverter(FScheme.EQ), "=") { }
    }

    [NodeName("And")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_CONDITIONAL)]
    [NodeDescription("Boolean AND: Returns true only if both of the inputs are true. If either is false, returns false.")]
    public class And : NodeModel
    {
        public And()
        {
            InPortData.Add(new PortData("a", "operand", typeof(Value.Number)));
            InPortData.Add(new PortData("b", "operand", typeof(Value.Number)));
            OutPortData.Add(new PortData("a∧b", "result", typeof(Value.Number)));
            RegisterAllPorts();
        }

        protected internal override INode Build(Dictionary<NodeModel, Dictionary<int, INode>> preBuilt, int outPort)
        {
            Dictionary<int, INode> result;
            if (preBuilt.TryGetValue(this, out result)) 
                return result[outPort];

            if (Enumerable.Range(0, InPortData.Count).All(HasInput))
            {
                var ifNode = new ConditionalNode();
                ifNode.ConnectInput("test", Inputs[0].Item2.Build(preBuilt, Inputs[0].Item1));
                ifNode.ConnectInput("true", Inputs[1].Item2.Build(preBuilt, Inputs[1].Item1));
                ifNode.ConnectInput("false", new NumberNode(0));
                result = new Dictionary<int, INode>();
                result[outPort] = ifNode;
            }
            else
            {
                var ifNode = new ConditionalNode();
                ifNode.ConnectInput("test", new SymbolNode(InPortData[0].NickName));
                ifNode.ConnectInput("true", new SymbolNode(InPortData[1].NickName));
                ifNode.ConnectInput("false", new NumberNode(0));

                var node = new AnonymousFunctionNode(
                    InPortData.Select(x => x.NickName),
                    ifNode);

                //For each index in InPortData
                //for (int i = 0; i < InPortData.Count; i++)
                foreach (var data in Enumerable.Range(0, InPortData.Count).Where(HasInput))
                {
                    //Compile input and connect it
                    node.ConnectInput(
                        InPortData[data].NickName,
                        Inputs[data].Item2.Build(preBuilt, Inputs[data].Item1));
                }

                RequiresRecalc = false;
                OnEvaluate(); //TODO: insert call into actual ast using a begin

                result = new Dictionary<int, INode>();
                result[outPort] = node;
            }
            preBuilt[this] = result;
            return result[outPort];
        }
    }

    [NodeName("Or")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_CONDITIONAL)]
    [NodeDescription("Boolean OR: Returns true if either of the inputs are true. If neither are true, returns false.")]
    public class Or : NodeModel
    {
        public Or()
        {
            InPortData.Add(new PortData("a", "operand", typeof(bool)));
            InPortData.Add(new PortData("b", "operand", typeof(bool)));
            OutPortData.Add(new PortData("a∨b", "result", typeof(bool)));
            RegisterAllPorts();
        }

        protected internal override INode Build(Dictionary<NodeModel, Dictionary<int, INode>> preBuilt, int outPort)
        {
            Dictionary<int, INode> result;
            if (preBuilt.TryGetValue(this, out result)) 
                return result[outPort];

            if (Enumerable.Range(0, InPortData.Count).All(HasInput))
            {
                var ifNode = new ConditionalNode();
                ifNode.ConnectInput("test", Inputs[0].Item2.Build(preBuilt, Inputs[0].Item1));
                ifNode.ConnectInput("true", new NumberNode(1));
                ifNode.ConnectInput("false", Inputs[1].Item2.Build(preBuilt, Inputs[1].Item1));

                result = new Dictionary<int, INode>();
                result[outPort] = ifNode;
            }
            else
            {
                var ifNode = new ConditionalNode();
                ifNode.ConnectInput("test", new SymbolNode(InPortData[0].NickName));
                ifNode.ConnectInput("true", new NumberNode(1));
                ifNode.ConnectInput("false", new SymbolNode(InPortData[1].NickName));

                var node = new AnonymousFunctionNode(
                    InPortData.Select(x => x.NickName),
                    ifNode);

                //For each index in InPortData
                //for (int i = 0; i < InPortData.Count; i++)
                foreach (var data in Enumerable.Range(0, InPortData.Count).Where(HasInput))
                {
                    //Compile input and connect it
                    node.ConnectInput(
                        InPortData[data].NickName,
                        Inputs[data].Item2.Build(preBuilt, Inputs[data].Item1));
                }

                RequiresRecalc = false;
                OnEvaluate(); //TODO: insert call into actual ast using a begin

                result = new Dictionary<int, INode>();
                result[outPort] = node;
            }
            preBuilt[this] = result;
            return result[outPort];
        }
    }

    [NodeName("Xor")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_CONDITIONAL)]
    [NodeDescription("Boolean XOR: Returns true if one input is true and the other is false. If both inputs are the same, returns false.")]
    public class Xor : BuiltinFunction
    {
        public Xor()
            : base(FScheme.Xor)
        {
            InPortData.Add(new PortData("a", "operand", typeof(bool)));
            InPortData.Add(new PortData("b", "operand", typeof(bool)));
            OutPortData.Add(new PortData("a⊻b", "result", typeof(bool)));
            RegisterAllPorts();
        }
    }

    [NodeName("Not")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_CONDITIONAL)]
    [NodeDescription("Boolean NOT: Inverts a boolean value. (True -> False, False -> True)")]
    [NodeSearchTags("invert")]
    public class Not : BuiltinFunction
    {
        public Not()
            : base(FScheme.Not)
        {
            InPortData.Add(new PortData("a", "operand", typeof(bool)));
            OutPortData.Add(new PortData("!a", "result", typeof(bool)));
            RegisterAllPorts();
        }
    }

    #endregion

    #region Math

    public abstract class MathBase : NodeWithOneOutput
    {
        protected MathBase()
        {
            ArgumentLacing = LacingStrategy.Longest;
        }
    }

    [NodeName("Add")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_MATH_ARITHMETIC)]
    [NodeDescription("Adds two numbers.")]
    [NodeSearchTags("plus", "sum", "+")]
    public class Addition : MathBase
    {
        public Addition()
        {
            InPortData.Add(new PortData("x", "operand", typeof(Value.Number)));
            InPortData.Add(new PortData("y", "operand", typeof(Value.Number)));
            OutPortData.Add(new PortData("x+y", "sum", typeof(Value.Number)));
            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var x = ((Value.Number)args[0]).Item;
            var y = ((Value.Number)args[1]).Item;

            return Value.NewNumber(x + y);
        }
    }

    [NodeName("Subtract")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_MATH_ARITHMETIC)]
    [NodeDescription("Subtracts two numbers.")]
    [NodeSearchTags("minus", "difference", "-")]
    public class Subtraction : MathBase
    {
        public Subtraction()
        {
            InPortData.Add(new PortData("x", "operand", typeof(Value.Number)));
            InPortData.Add(new PortData("y", "operand", typeof(Value.Number)));
            OutPortData.Add(new PortData("x-y", "difference", typeof(Value.Number)));
            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var x = ((Value.Number)args[0]).Item;
            var y = ((Value.Number)args[1]).Item;

            return Value.NewNumber(x - y);
        }
    }

    [NodeName("Multiply")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_MATH_ARITHMETIC)]
    [NodeDescription("Multiplies two numbers.")]
    [NodeSearchTags("times", "product", "*")]
    public class Multiplication : MathBase
    {
        public Multiplication()
        {
            InPortData.Add(new PortData("x", "operand", typeof(Value.Number)));
            InPortData.Add(new PortData("y", "operand", typeof(Value.Number)));
            OutPortData.Add(new PortData("x∙y", "product", typeof(Value.Number)));
            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var x = ((Value.Number)args[0]).Item;
            var y = ((Value.Number)args[1]).Item;

            return Value.NewNumber(x * y);
        }
    }

    [NodeName("Divide")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_MATH_ARITHMETIC)]
    [NodeDescription("Divides two numbers.")]
    [NodeSearchTags("division", "quotient", "/")]
    public class Division : MathBase
    {
        public Division()
        {
            InPortData.Add(new PortData("x", "operand", typeof(Value.Number)));
            InPortData.Add(new PortData("y", "operand", typeof(Value.Number)));
            OutPortData.Add(new PortData("x÷y", "result", typeof(Value.Number)));
            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var x = ((Value.Number)args[0]).Item;
            var y = ((Value.Number)args[1]).Item;

            return Value.NewNumber(x / y);
        }
    }

    [NodeName("Modulo")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_MATH_ARITHMETIC)]
    [NodeDescription("Remainder of division of two numbers.")]
    [NodeSearchTags("%", "remainder")]
    public class Modulo : MathBase
    {
        public Modulo()
        {
            InPortData.Add(new PortData("x", "operand", typeof(Value.Number)));
            InPortData.Add(new PortData("y", "operand", typeof(Value.Number)));
            OutPortData.Add(new PortData("x%y", "result", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var x = ((Value.Number)args[0]).Item;
            var y = ((Value.Number)args[1]).Item;

            return Value.NewNumber(x % y);
        }
    }

    [NodeName("Power")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_MATH_ARITHMETIC)]
    [NodeDescription("Raises a number to the power of another.")]
    [NodeSearchTags("pow", "exponentiation", "^")]
    public class Pow : MathBase
    {
        public Pow()
        {
            InPortData.Add(new PortData("x", "operand", typeof(Value.Number)));
            InPortData.Add(new PortData("y", "operand", typeof(Value.Number)));
            OutPortData.Add(new PortData("x^y", "result", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var x = ((Value.Number)args[0]).Item;
            var y = ((Value.Number)args[1]).Item;

            return Value.NewNumber(Math.Pow(x,y));
        }
    }

    [NodeName("Round")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_MATH_ROUNDING)]
    [NodeDescription("Rounds a number to the nearest integer value.")]
    public class Round : MathBase
    {
        public Round()
        {
            InPortData.Add(new PortData("dbl", "A number", typeof(Value.Number)));
            OutPortData.Add(new PortData("int", "Rounded number", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            return Value.NewNumber(
               Math.Round(((Value.Number)args[0]).Item)
            );
        }
    }

    [NodeName("Floor")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_MATH_ROUNDING)]
    [NodeDescription("Rounds a number to the nearest smaller integer.")]
    [NodeSearchTags("round")]
    public class Floor : MathBase
    {
        public Floor()
        {
            InPortData.Add(new PortData("dbl", "A number", typeof(Value.Number)));
            OutPortData.Add(new PortData("int", "Number rounded down", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            return Value.NewNumber(
               Math.Floor(((Value.Number)args[0]).Item)
            );
        }
    }

    [NodeName("Ceiling")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_MATH_ROUNDING)]
    [NodeDescription("Rounds a number to the nearest larger integer value.")]
    [NodeSearchTags("round")]
    public class Ceiling : MathBase
    {
        public Ceiling()
        {
            InPortData.Add(new PortData("dbl", "A number", typeof(Value.Number)));
            OutPortData.Add(new PortData("int", "Number rounded up", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            return Value.NewNumber(
               Math.Ceiling(((Value.Number)args[0]).Item)
            );
        }
    }

    [NodeName("Random Number By Seed")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_MATH_RANDOM)]
    [NodeDescription("Generates a uniform random number in the range [0.0, 1.0).")]
    public class RandomSeed : NodeWithOneOutput
    {
        public RandomSeed()
        {
            InPortData.Add(new PortData("num", "A number to function as a seed", typeof(Value.Number)));
            OutPortData.Add(new PortData("rand", "Random number between 0.0 and 1.0.", typeof(Value.Number)));
            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
        }

        private static System.Random random = new System.Random();
        public override Value Evaluate(FSharpList<Value> args)
        {
            random = new System.Random((int) ( (Value.Number) args[0] ).Item );
            return Value.NewNumber(random.NextDouble());
        }
    }

    [NodeName("Random Number")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_MATH_RANDOM)]
    [NodeDescription("Generates a uniform random number in the range [0.0, 1.0).")]
    public class Random : NodeWithOneOutput
    {
        public Random()
        {
            OutPortData.Add(new PortData("rand", "Random number between 0.0 and 1.0.", typeof(Value.Number)));
            RegisterAllPorts();
        }

        private readonly System.Random _random = new System.Random();

        public override bool RequiresRecalc
        {
            get
            {
                return true;
            }
            set { }
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            return Value.NewNumber(_random.NextDouble());
        }
    }

    [NodeName("Random Number List")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_MATH_RANDOM)]
    [NodeDescription("Generates a list of uniform random numbers in the range [0.0, 1.0).")]
    public class RandomList : NodeWithOneOutput
    {
        public RandomList()
        {
            InPortData.Add(new PortData("amt", "Number of random numbers to be generated.", typeof(Value.Number)));
            OutPortData.Add(new PortData("rand", "Random number between 0.0 and 1.0.", typeof(Value.List)));
            RegisterAllPorts();
        }

        private readonly System.Random _random = new System.Random();

        public override bool RequiresRecalc
        {
            get
            {
                return true;
            }
            set { }
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var n = (int)((Value.Number)args[0]).Item;

            var result = FSharpList<Value>.Empty;

            while (n-- > 0)
                result = FSharpList<Value>.Cons(Value.NewNumber(_random.NextDouble()), result);

            return Value.NewList(result);
        }
    }

    [NodeName("e")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_MATH_CONSTANTS)]
    [NodeDescription("e (base of natural logarithm) constant")]
    [NodeSearchTags("statistics", "natural", "logarithm")]
    [IsInteractive(false)]
    public class EConstant : NodeModel
    {
        public EConstant()
        {
            OutPortData.Add(new PortData("2.71828...", "e", typeof(Value.Number)));
            RegisterAllPorts();

            OldValue = Value.NewNumber(Math.E);
        }

        public override bool RequiresRecalc
        {
            get
            {
                return false;
            }
            set { }
        }

        protected internal override INode Build(Dictionary<NodeModel, Dictionary<int, INode>> preBuilt, int outPort)
        {
            Dictionary<int, INode> result;
            if (!preBuilt.TryGetValue(this, out result))
            {
                result = new Dictionary<int, INode>();
                result[outPort] = new NumberNode(Math.E);
                preBuilt[this] = result;
            }
            return result[outPort];
        }
    }

    [NodeName("Pi")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_MATH_CONSTANTS)]
    [NodeDescription("Pi constant")]
    [NodeSearchTags("trigonometry", "circle", "π")]
    [IsInteractive(false)]
    public class Pi : NodeModel
    {
        public Pi()
        {
            OutPortData.Add(new PortData("3.14159...", "pi", typeof(Value.Number)));
            RegisterAllPorts();

            OldValue = Value.NewNumber(Math.PI);
        }

        public override bool RequiresRecalc
        {
            get
            {
                return false;
            }
            set { }
        }

        protected internal override INode Build(Dictionary<NodeModel, Dictionary<int, INode>> preBuilt, int outPort)
        {
            Dictionary<int, INode> result;
            if (!preBuilt.TryGetValue(this, out result))
            {
                result = new Dictionary<int, INode>();
                result[outPort] = new NumberNode(3.14159265358979);
                preBuilt[this] = result;
            }
            return result[outPort];
        }
    }

    [NodeName("2*Pi")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_MATH_CONSTANTS)]
    [NodeDescription("Pi constant")]
    [NodeSearchTags("trigonometry", "circle", "π")]
    [IsInteractive(false)]
    [AlsoKnownAs("Dynamo.Nodes.2Pi", "Dynamo.Nodes.dyn2Pi")]
    public class PiTimes2 : NodeModel
    {
        public PiTimes2()
        {
            OutPortData.Add(new PortData("3.14159...*2", "2*pi", typeof(Value.Number)));
            RegisterAllPorts();

            OldValue = Value.NewNumber(Math.PI*2);
        }

        public override bool RequiresRecalc
        {
            get
            {
                return false;
            }
            set { }
        }

        protected internal override INode Build(Dictionary<NodeModel, Dictionary<int, INode>> preBuilt, int outPort)
        {
            Dictionary<int, INode> result;
            if (!preBuilt.TryGetValue(this, out result))
            {
                result = new Dictionary<int, INode>();
                result[outPort] = new NumberNode(3.14159265358979 * 2);
                preBuilt[this] = result;
            }
            return result[outPort];
        }
    }

    [NodeName("Sine")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_MATH_TRIGONOMETRY)]
    [NodeDescription("Computes the sine of the given angle.")]
    public class Sin : MathBase
    {
        public Sin()
        {
            InPortData.Add(new PortData("θ", "Angle in radians", typeof(Value.Number)));
            OutPortData.Add(new PortData("sin(θ)", "Sine value of the given angle", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var input = args[0];

            if (input.IsList)
            {
                return Value.NewList(
                   FSchemeInterop.Utils.SequenceToFSharpList(
                      ((Value.List)input).Item.Select(
                         x =>
                            Value.NewNumber(Math.Sin(((Value.Number)x).Item))
                      )
                   )
                );
            }
            else
            {
                double theta = ((Value.Number)input).Item;
                return Value.NewNumber(Math.Sin(theta));
            }
        }
    }

    [NodeName("Cosine")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_MATH_TRIGONOMETRY)]
    [NodeDescription("Computes the cosine of the given angle.")]
    public class Cos : MathBase
    {
        public Cos()
        {
            InPortData.Add(new PortData("θ", "Angle in radians", typeof(Value.Number)));
            OutPortData.Add(new PortData("cos(θ)", "Cosine value of the given angle", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var input = args[0];

            if (input.IsList)
            {
                return Value.NewList(
                   FSchemeInterop.Utils.SequenceToFSharpList(
                      ((Value.List)input).Item.Select(
                         x =>
                            Value.NewNumber(Math.Cos(((Value.Number)x).Item))
                      )
                   )
                );
            }
            else
            {
                double theta = ((Value.Number)input).Item;
                return Value.NewNumber(Math.Cos(theta));
            }
        }
    }

    [NodeName("Tangent")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_MATH_TRIGONOMETRY)]
    [NodeDescription("Computes the tangent of the given angle.")]
    public class Tan : MathBase
    {
        public Tan()
        {
            InPortData.Add(new PortData("θ", "Angle in radians", typeof(Value.Number)));
            OutPortData.Add(new PortData("tan(θ)", "Tangent value of the given angle", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var input = args[0];

            if (input.IsList)
            {
                return Value.NewList(
                   FSchemeInterop.Utils.SequenceToFSharpList(
                      ((Value.List)input).Item.Select(
                         x =>
                            Value.NewNumber(Math.Tan(((Value.Number)x).Item))
                      )
                   )
                );
            }
            else
            {
                double theta = ((Value.Number)input).Item;
                return Value.NewNumber(Math.Tan(theta));
            }
        }
    }

    [NodeName("Inverse Sine")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_MATH_TRIGONOMETRY)]
    [NodeDescription("Computes the inverse sine of the given angle.")]
    [NodeSearchTags("asin", "arcsine")]
    public class Asin : MathBase
    {
        public Asin()
        {
            InPortData.Add(new PortData("θ", "Angle in radians", typeof(Value.Number)));
            OutPortData.Add(new PortData("asin(θ)", "Arcsine value of the given angle", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var input = args[0];

            double theta = ((Value.Number)input).Item;
            return Value.NewNumber(Math.Asin(theta));
        }
    }

    [NodeName("Inverse Cosine")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_MATH_TRIGONOMETRY)]
    [NodeDescription("Computes the inverse cosine of the given angle.")]
    [NodeSearchTags("acos", "arccos")]
    public class Acos : MathBase
    {
        public Acos()
        {
            InPortData.Add(new PortData("θ", "Angle in radians", typeof(Value.Number)));
            OutPortData.Add(new PortData("acos(θ)", "Arccosine value of the given angle", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var input = args[0];

            double theta = ((Value.Number)input).Item;
            return Value.NewNumber(Math.Acos(theta));
        }
    }

    [NodeName("Inverse Tangent")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_MATH_TRIGONOMETRY)]
    [NodeDescription("Computes the secant of the given angle.")]
    [NodeSearchTags("atan", "arctangent")]
    public class Atan : MathBase
    {
        public Atan()
        {
            InPortData.Add(new PortData("θ", "Angle in radians", typeof(Value.Number)));
            OutPortData.Add(new PortData("atan(θ)", "Arctangent value of the given angle", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var input = args[0];

            double theta = ((Value.Number)input).Item;
            return Value.NewNumber(Math.Atan(theta));
        }
    }

    [NodeName("Average")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_QUERY)]
    [NodeDescription("Averages a list of numbers.")]
    [NodeSearchTags("avg")]
    public class Average : MathBase
    {
        public Average()
        {
            InPortData.Add(new PortData("numbers", "The list of numbers to average.", typeof(Value.List)));
            OutPortData.Add(new PortData("avg", "average", typeof(Value.Number)));
            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            if (!args[0].IsList)
                throw new Exception("A list of numbers is required to average.");

            var vals = ((Value.List)args[0]).Item.Select(
               x => ((Value.Number)x).Item
            );

            var average = vals.Average();

            return Value.NewNumber(average);
        }
    }

    [NodeName("Smooth")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_QUERY)]
    [NodeDescription("Smooths a list of numbers using a running average.")]
    [NodeSearchTags("running average", "moving average", "sma")]
    public class Smooth : MathBase
    {
        Queue<Value.Number> values = new Queue<Value.Number>();
        int maxNumValues = 10;

        public Smooth()
        {
            InPortData.Add(new PortData("val", "The current value.", typeof(Value.Container)));
            OutPortData.Add(new PortData("avg", "uses a simple moving average to smooth out values that fluctuate over time", typeof(Value.Number)));
            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            if (!args[0].IsNumber)
                throw new Exception("A number is required to smooth.");

            if (values.Count() < maxNumValues)
                values.Enqueue((Value.Number)args[0]); // add current values to queue until it fills up
            else
                values.Dequeue();//throw out the first value once we are up to the full queue amount

            var average = values.Average(num => num.Item);

            return Value.NewNumber(average);
        }
    }

    #endregion

    #region Control Flow

    //TODO: Setup proper IsDirty smart execution management
    [NodeName("Perform All")]
    [NodeCategory(BuiltinNodeCategories.CORE_EVALUATE)]
    [NodeDescription("Evaluates all inputs in order, and returns result of the last input.")]
    [NodeSearchTags("begin")]
    public class Begin : VariableInput
    {
        public Begin()
        {
            InPortData.Add(new PortData("expr1", "Expression #1", typeof(object)));
            InPortData.Add(new PortData("expr2", "Expression #2", typeof(object)));
            OutPortData.Add(new PortData("last", "Result of final expression", typeof(object)));

            RegisterAllPorts();
        }

        protected internal override void RemoveInput()
        {
            if (InPortData.Count > 2)
                base.RemoveInput();
        }

        protected override string GetInputRootName()
        {
            return "expr";
        }

        protected override string GetTooltipRootName()
        {
            return "Expression #";
        }

        protected override int GetInputNameIndex()
        {
            return InPortData.Count + 1;
        }

        private INode nestedBegins(Stack<Tuple<int, NodeModel>> inputs, Dictionary<NodeModel, Dictionary<int, INode>> preBuilt)
        {
            var popped = inputs.Pop();
            var firstVal = popped.Item2.Build(preBuilt, popped.Item1);

            if (inputs.Any())
            {
                var newBegin = new BeginNode(new List<string>() { "expr1", "expr2" });
                newBegin.ConnectInput("expr1", nestedBegins(inputs, preBuilt));
                newBegin.ConnectInput("expr2", firstVal);
                return newBegin;
            }
            else
                return firstVal;
        }

        protected internal override INode Build(Dictionary<NodeModel, Dictionary<int, INode>> preBuilt, int outPort)
        {
            if (!Enumerable.Range(0, InPortData.Count).All(HasInput))
            {
                Error("All inputs must be connected.");
                throw new Exception("Begin Node requires all inputs to be connected.");
            }
            
            Dictionary<int, INode> result;
            if (!preBuilt.TryGetValue(this, out result))
            {
                result = new Dictionary<int, INode>(); 
                result[outPort] = 
                    nestedBegins(
                        new Stack<Tuple<int, NodeModel>>(
                            Enumerable.Range(0, InPortData.Count).Select(x => Inputs[x])),
                    preBuilt);
                preBuilt[this] = result;
            }
            return result[outPort];
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            throw new NotImplementedException();
        }
    }

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
    }

    //TODO: Setup proper IsDirty smart execution management
    [NodeName("If")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_CONDITIONAL)]
    [NodeDescription("Conditional statement")]
    public class Conditional : NodeModel
    {
        public Conditional()
        {
            InPortData.Add(new PortData("test", "Test block", typeof(bool)));
            InPortData.Add(new PortData("true", "True block", typeof(object), Value.NewDummy("Empty true")));
            InPortData.Add(new PortData("false", "False block", typeof(object), Value.NewDummy("Empty false")));
            OutPortData.Add(new PortData("result", "Result", typeof(object)));

            RegisterAllPorts();

            foreach (var port in InPorts.Skip(1))
            {
                port.DefaultValueEnabled = false;
            }
        }

        protected internal override INode Build(Dictionary<NodeModel, Dictionary<int, INode>> preBuilt, int outPort)
        {
            if (!HasInput(0))
            {
                Error("Test input must be connected.");
                throw new Exception("If Node requires test input to be connected.");
            }
            return base.Build(preBuilt, outPort);
        }

        protected override InputNode Compile(IEnumerable<string> portNames)
        {
            return new ConditionalNode(portNames);
        }
    }
    
    [NodeName("Debug Breakpoint")]
    [NodeCategory(BuiltinNodeCategories.CORE_EVALUATE)]
    [NodeDescription("Halts execution until user clicks button.")]
    public partial class Breakpoint : NodeWithOneOutput
    {
        public Breakpoint()
        {
            InPortData.Add(new PortData("", "Object to inspect", typeof(object)));
            OutPortData.Add(new PortData("", "Object inspected", typeof(object)));
            RegisterAllPorts();
        }

        private bool Enabled { get; set; }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var result = args[0];

            DynamoLogger.Instance.Log(FScheme.print(result));

            if (Controller.DynamoViewModel.RunInDebug)
            {
                Enabled = true;
                Select();
                Controller.DynamoViewModel.ShowElement(this);

                while (Enabled)
                {
                    Thread.Sleep(1);
                }
            }

            return result;
        }
    }

    #endregion

    #region Interactive Primitive Types

    #region Base Classes

    [IsInteractive(true)]
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

        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context); //Base implementation must be called
            if (context == SaveContext.Undo)
            {
                XmlElementHelper helper = new XmlElementHelper(element);
                helper.SetAttribute("doubleValue", Value);
            }
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context); //Base implementation must be called
            if (context == SaveContext.Undo)
            {
                XmlElementHelper helper = new XmlElementHelper(element);
                Value = helper.ReadDouble("doubleValue");
            }
        }

        #endregion

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

        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context); //Base implementation must be called
            if (context == SaveContext.Undo)
            {
                XmlElementHelper helper = new XmlElementHelper(element);
                helper.SetAttribute("integerValue", Value);
            }
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context); //Base implementation must be called
            if (context == SaveContext.Undo)
            {
                XmlElementHelper helper = new XmlElementHelper(element);
                Value = helper.ReadInteger("integerValue");
            }
        }

        #endregion
    }

    public abstract class Bool : BasicInteractive<bool>
    {
        public override Value Evaluate(FSharpList<Value> args)
        {
            return FScheme.Value.NewNumber(Value ? 1 : 0);
        }

        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context); //Base implementation must be called
            if (context == SaveContext.Undo)
            {
                XmlElementHelper helper = new XmlElementHelper(element);
                helper.SetAttribute("boolValue", Value);
            }
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context); //Base implementation must be called
            if (context == SaveContext.Undo)
            {
                XmlElementHelper helper = new XmlElementHelper(element);
                Value = helper.ReadBoolean("boolValue");
            }
        }

        #endregion
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

    #endregion

    public delegate double ConversionDelegate(double value);

    [NodeName("Number")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("Creates a number.")]
    [IsDesignScriptCompatible]
    public partial class DoubleInput : NodeWithOneOutput
    {
        public DoubleInput()
        {
            OutPortData.Add(new PortData("", "", typeof(Value.Number)));

            RegisterAllPorts();

            _convertToken = Convert;
        }

        public virtual double Convert(double value)
        {
            return value;
        }

        private List<IDoubleSequence> _parsed;
        private string _value;
        protected ConversionDelegate _convertToken;

        public string Value
        {
            get { return _value; }
            set
            {
                if (_value != null && _value.Equals(value)) 
                    return;

                _value = value;

                var idList = new List<string>();

                try
                {
                    _parsed = ParseValue(value, new[] { '\n' }, idList, _convertToken);

                    InPortData.Clear();

                    foreach (var id in idList)
                    {
                        InPortData.Add(new PortData(id, "variable", typeof (Value.Number)));
                    }

                    RegisterInputPorts();

                    ArgumentLacing = InPortData.Any() ? LacingStrategy.Longest : LacingStrategy.Disabled;
                }
                catch (Exception e)
                {
                    Error(e.Message);
                }

                RequiresRecalc = value != null;
                RaisePropertyChanged("Value");
            }
        }

        public override bool IsConvertible
        {
            get { return true; }
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            XmlElement outEl = xmlDoc.CreateElement(typeof(double).FullName);
            outEl.SetAttribute("value", Value);
            nodeElement.AppendChild(outEl);
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            foreach (XmlNode subNode in nodeElement.ChildNodes.Cast<XmlNode>().Where(subNode => subNode.Name.Equals(typeof(double).FullName)))
            {
                Value = subNode.Attributes[0].Value;
            }
        }

        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context); //Base implementation must be called

            if (context == SaveContext.Undo)
            {
                XmlElementHelper helper = new XmlElementHelper(element);
                helper.SetAttribute("doubleInputValue", Value);
            }
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context); //Base implementation must be called

            if (context == SaveContext.Undo)
            {
                XmlElementHelper helper = new XmlElementHelper(element);
                this.Value = helper.ReadString("doubleInputValue");
            }
        }

        #endregion

        public static List<IDoubleSequence> ParseValue(string text, char[] seps, List<string> identifiers, ConversionDelegate convertToken)
        {
            var idSet = new HashSet<string>(identifiers);
            return text.Replace(" ", "").Split(seps, StringSplitOptions.RemoveEmptyEntries).Select(
                delegate(string x)
                {
                    var rangeIdentifiers = x.Split(
                        Sublists.RangeSeparatorTokens,
                        StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();

                    if (rangeIdentifiers.Length > 3)
                        throw new Exception("Bad range syntax: not of format \"start..end[..(increment|#count)]\"");

                    if (rangeIdentifiers.Length == 0)
                        throw new Exception("No identifiers found.");

                    IDoubleInputToken startToken = ParseToken(rangeIdentifiers[0], idSet, identifiers);

                    if (rangeIdentifiers.Length > 1)
                    {
                        if (rangeIdentifiers[1].StartsWith("#"))
                        {
                            var countToken = rangeIdentifiers[1].Substring(1);
                            IDoubleInputToken endToken = ParseToken(countToken, idSet, identifiers);

                            if (rangeIdentifiers.Length > 2)
                            {
                                if (rangeIdentifiers[2].StartsWith("#") || rangeIdentifiers[2].StartsWith("~"))
                                    throw new Exception("Cannot use range or approx. identifier on increment field when one has already been used to specify a count.");
                                return new Sequence(startToken, ParseToken(rangeIdentifiers[2], idSet, identifiers), endToken, convertToken);
                            }

                            return new Sequence(startToken, new DoubleToken(1), endToken, convertToken) as IDoubleSequence;
                        }
                        else
                        {
                            IDoubleInputToken endToken = ParseToken(rangeIdentifiers[1], idSet, identifiers);

                            if (rangeIdentifiers.Length > 2)
                            {
                                if (rangeIdentifiers[2].StartsWith("#"))
                                {
                                    var count = rangeIdentifiers[2].Substring(1);
                                    IDoubleInputToken countToken = ParseToken(count, idSet, identifiers);

                                    return new CountRange(startToken, countToken, endToken, convertToken);
                                }

                                if (rangeIdentifiers[2].StartsWith("~"))
                                {
                                    var approx = rangeIdentifiers[2].Substring(1);
                                    IDoubleInputToken approxToken = ParseToken(approx, idSet, identifiers);

                                    return new ApproxRange(startToken, approxToken, endToken, convertToken);
                                }

                                return new Range(startToken, ParseToken(rangeIdentifiers[2], idSet, identifiers), endToken, convertToken);
                            }
                            return new Range(startToken, new DoubleToken(1), endToken, convertToken) as IDoubleSequence;
                        }

                    }

                    return new OneNumber(startToken, convertToken) as IDoubleSequence;
                }).ToList();
        }

        private static IDoubleInputToken ParseToken(string id, HashSet<string> identifiers, List<string> list)
        {
            double dbl;
            if (double.TryParse(id, NumberStyles.Any, CultureInfo.CurrentCulture, out dbl))
                return new DoubleToken(dbl);

            var match = Sublists.IdentifierPattern.Match(id);
            if (match.Success)
            {
                var tokenId = match.Groups["id"].Value;
                if (!identifiers.Contains(tokenId))
                {
                    identifiers.Add(tokenId);
                    list.Add(tokenId);
                }
                return new IdentifierToken(tokenId);
            }

            throw new Exception("Bad identifier syntax: \"" + id + "\"");
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var paramDict = InPortData.Select(x => x.NickName)
                .Zip(args, Tuple.Create)
                .ToDictionary(x => x.Item1, x => ((Value.Number)x.Item2).Item);

            return _parsed.Count == 1
                ? _parsed[0].GetFSchemeValue(paramDict)
                : FScheme.Value.NewList(Utils.SequenceToFSharpList(_parsed.Select(x => x.GetFSchemeValue(paramDict))));
        }

        internal override IEnumerable<AssociativeNode> BuildAst(List<AssociativeNode> inputAstNodes)
        {
            var paramDict = InPortData.Select(x => x.NickName)
                   .Zip(inputAstNodes, Tuple.Create)
                   .ToDictionary(x => x.Item1, x => x.Item2);

            AssociativeNode rhs;

            if (null == _parsed)
            {
                rhs = AstFactory.BuildNullNode();
            }
            else
            {
                List<AssociativeNode> newInputs = _parsed.Count == 1
                    ? new List<AssociativeNode> { _parsed[0].GetAstNode(paramDict) }
                    : _parsed.Select(x => x.GetAstNode(paramDict)).ToList();

                rhs = newInputs.Count == 1 
                        ? newInputs[0] 
                        : AstFactory.BuildExprList(newInputs);
            }

            var assignment = AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), rhs);

            return new[] { assignment };
        }

        public interface IDoubleSequence
        {
            Value GetFSchemeValue(Dictionary<string, double> idLookup);
            IEnumerable<double> GetValue(Dictionary<string, double> idLookup);
            AssociativeNode GetAstNode(Dictionary<string, AssociativeNode> idLookup);
        }

        private class OneNumber : IDoubleSequence
        {
            private readonly IDoubleInputToken _token;
            private readonly double? _result;
            private readonly ConversionDelegate _convert;
 
            public OneNumber(IDoubleInputToken t, ConversionDelegate convertToken)
            {
                _token = t;
                _convert = convertToken;

                if (_token is DoubleToken)
                    _result = _convert(GetValue(null).First());
            }

            public Value GetFSchemeValue(Dictionary<string, double> idLookup)
            {
                return FScheme.Value.NewNumber(GetValue(idLookup).First());
            }

            public IEnumerable<double> GetValue(Dictionary<string, double> idLookup)
            {
                yield return _result ?? _token.GetValue(idLookup);
            }

            public AssociativeNode GetAstNode(Dictionary<string, AssociativeNode> idLookup)
            {
                if (_result == null)
                {
                    return _token.GetAstNode(idLookup);
                }
                else
                {
                    return new DoubleNode { value = _result.GetValueOrDefault().ToString() };
                }
            }
        }

        private class Sequence : IDoubleSequence
        {
            private readonly IDoubleInputToken _start;
            private readonly IDoubleInputToken _step;
            private readonly IDoubleInputToken _count;
            private readonly ConversionDelegate _convert;

            private readonly IEnumerable<double> _result;

            public Sequence(IDoubleInputToken start, IDoubleInputToken step, IDoubleInputToken count, ConversionDelegate convertToken)
            {
                _start = start;
                _step = step;
                _count = count;
                _convert = convertToken;

                if (_start is DoubleToken && _step is DoubleToken && _count is DoubleToken)
                {
                    _result = GetValue(null);
                }
            }

            public Value GetFSchemeValue(Dictionary<string, double> idLookup)
            {
                return FScheme.Value.NewList(
                    Utils.SequenceToFSharpList(
                        GetValue(idLookup).Select(FScheme.Value.NewNumber)));
            }

            public IEnumerable<double> GetValue(Dictionary<string, double> idLookup)
            {
                if (_result == null)
                {
                    var step = _step.GetValue(idLookup);

                    if (step == 0)
                        throw new Exception("Can't have 0 step.");

                    var start = _start.GetValue(idLookup);
                    var count = (int)_count.GetValue(idLookup);

                    if (count < 0)
                    {
                        count *= -1;
                        start += step * (count - 1);
                        step *= -1;
                    }

                    return CreateSequence(start, step, count);
                }
                return _result;
            }

            private static IEnumerable<double> CreateSequence(double start, double step, int count)
            {
                for (var i = 0; i < count; i++)
                {
                    yield return start;
                    start += step;
                }
            }

            public AssociativeNode GetAstNode(Dictionary<string, AssociativeNode> idLookup)
            {
                var rangeExpr = new RangeExprNode
                {
                    FromNode = _start.GetAstNode(idLookup),
                    ToNode = _step.GetAstNode(idLookup),
                    StepNode = _step.GetAstNode(idLookup),
                    stepoperator = ProtoCore.DSASM.RangeStepOperator.stepsize
                };
                return rangeExpr;
            }
        }

        private class Range : IDoubleSequence
        {
            private readonly IDoubleInputToken _start;
            private readonly IDoubleInputToken _step;
            private readonly IDoubleInputToken _end;
            private readonly ConversionDelegate _convert;

            private readonly IEnumerable<double> _result;

            public Range(IDoubleInputToken start, IDoubleInputToken step, IDoubleInputToken end, ConversionDelegate convertToken)
            {
                _start = start;
                _step = step;
                _end = end;
                _convert = convertToken;

                if (_start is DoubleToken && _step is DoubleToken && _end is DoubleToken)
                {
                    _result = GetValue(null);
                }
            }

            public Value GetFSchemeValue(Dictionary<string, double> idLookup)
            {
                return FScheme.Value.NewList(
                    Utils.SequenceToFSharpList(
                        GetValue(idLookup).Select(FScheme.Value.NewNumber)));
            }

            public IEnumerable<double> GetValue(Dictionary<string, double> idLookup)
            {
                if (_result == null)
                {
                    var step = _convert(_step.GetValue(idLookup));

                    if (step == 0)
                        throw new Exception("Can't have 0 step.");

                    var start = _convert(_start.GetValue(idLookup));
                    var end = _convert(_end.GetValue(idLookup));

                    return Process(start, step, end);
                }
                return _result;
            }

            protected virtual IEnumerable<double> Process(double start, double step, double end)
            {
                if (step < 0)
                {
                    step *= -1;
                    var tmp = end;
                    end = start;
                    start = tmp;
                }

                var countingUp = start < end;

                return countingUp 
                    ? FScheme.Range(start, step, end) 
                    : FScheme.Range(end, step, start).Reverse();
            }

            protected virtual ProtoCore.DSASM.RangeStepOperator GetRangeExpressionOperator()
            {
                return ProtoCore.DSASM.RangeStepOperator.stepsize;
            }

            public AssociativeNode GetAstNode(Dictionary<string, AssociativeNode> idLookup)
            {
                var rangeExpr = new RangeExprNode
                {
                    FromNode = _start.GetAstNode(idLookup),
                    ToNode = _end.GetAstNode(idLookup),
                    StepNode = _step.GetAstNode(idLookup),
                    stepoperator = GetRangeExpressionOperator()
                };
                return rangeExpr;
            }
        }

        private class CountRange : Range
        {
            public CountRange(IDoubleInputToken startToken, IDoubleInputToken countToken, IDoubleInputToken endToken, ConversionDelegate convertToken)
                : base(startToken, countToken, endToken, convertToken)
            { }

            protected override IEnumerable<double> Process(double start, double count, double end)
            {
                var c = (int)count;

                var neg = c < 0;

                c = Math.Abs(c) - 1;

                if (neg)
                    c *= -1;

                return base.Process(start, Math.Abs(start - end) / c, end);
            }

            protected override ProtoCore.DSASM.RangeStepOperator GetRangeExpressionOperator()
            {
                return ProtoCore.DSASM.RangeStepOperator.num;
            }
        }

        private class ApproxRange : Range
        {
            public ApproxRange(IDoubleInputToken start, IDoubleInputToken step, IDoubleInputToken end, ConversionDelegate convertToken) 
                : base(start, step, end, convertToken)
            { }

            protected override IEnumerable<double> Process(double start, double approx, double end)
            {
                var neg = approx < 0;

                var a = Math.Abs(approx);

                var dist = end - start;
                var stepnum = 1;
                if (dist != 0)
                {
                    var ceil = (int)Math.Ceiling(dist/a);
                    var floor = (int)Math.Floor(dist/a);

                    if (ceil != 0 && floor != 0)
                    {
                        var ceilApprox = Math.Abs(dist/ceil - a);
                        var floorApprox = Math.Abs(dist/floor - a);
                        stepnum = ceilApprox < floorApprox ? ceil : floor;
                    }
                }

                if (neg)
                    stepnum *= -1;

                return base.Process(start, Math.Abs(dist) / stepnum, end);
            }

            protected override ProtoCore.DSASM.RangeStepOperator GetRangeExpressionOperator()
            {
                return ProtoCore.DSASM.RangeStepOperator.approxsize;
            }
        }

        interface IDoubleInputToken
        {
            double GetValue(Dictionary<string, double> idLookup);
            AssociativeNode GetAstNode(Dictionary<string, AssociativeNode> idLookup);
        }

        private struct IdentifierToken : IDoubleInputToken
        {
            private readonly string _id;

            public IdentifierToken(string id)
            {
                _id = id;
            }

            public double GetValue(Dictionary<string, double> idLookup)
            {
                return idLookup[_id];
            }

            public AssociativeNode GetAstNode(Dictionary<string, AssociativeNode> idLookup)
            {
                return idLookup[_id];
            }
        }

        private struct DoubleToken : IDoubleInputToken
        {
            private readonly double _d;

            public DoubleToken(double d)
            {
                _d = d;
            }

            public double GetValue(Dictionary<string, double> idLookup)
            {
                return _d;
            }

            public AssociativeNode GetAstNode(Dictionary<string, AssociativeNode> idLookup)
            {
                return AstFactory.BuildDoubleNode(_d);
            }
        }
    }

    [NodeName("Angle")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("Convert angle from degrees to radians")]
    [NodeSearchTags("trigonometry", "angle", "degree")]
    public class AngleInput : DoubleInput
    {
        public override double Convert(double value)
        {
            return value * Math.PI / 180.0;
        }
    }

    [NodeName("Number Slider")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("Change a number value with a slider.")]
    [IsDesignScriptCompatible]
    public partial class DoubleSliderInput : Double
    {

        private double max;
        private double min;

        public DoubleSliderInput()
        {
            RegisterAllPorts();
            
            Min = 0.0;
            Max = 100.0;
            Value = 50.0;
        }

        public override double Value
        {
            get
            {
                return base.Value;
            }
            set
            {
                base.Value = value;
                //RaisePropertyChanged("Value"); //already called in base --SJE

                Debug.WriteLine(string.Format("Min:{0},Max:{1},Value:{2}", Min.ToString(CultureInfo.InvariantCulture), Max.ToString(CultureInfo.InvariantCulture), Value.ToString(CultureInfo.InvariantCulture)));
            }
        }
        
        public double Max
        {
            get { return max; }
            set
            {
                max = value;

                if (max < Value)
                    Value = max;

                RaisePropertyChanged("Max");
            }
        }

        public double Min
        {
            get { return min; }
            set
            {
                min = value;

                if (min > Value)
                    Value = min;

                RaisePropertyChanged("Min");
            } 
        }

        protected override double DeserializeValue(string val)
        {
            try
            {
                return Convert.ToDouble(val, CultureInfo.InvariantCulture);
            }
            catch
            {
                return 0;
            }
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            XmlElement outEl = xmlDoc.CreateElement(typeof(double).FullName);
            outEl.SetAttribute("value", Value.ToString(CultureInfo.InvariantCulture));
            outEl.SetAttribute("min", Min.ToString(CultureInfo.InvariantCulture));
            outEl.SetAttribute("max", Max.ToString(CultureInfo.InvariantCulture));
            nodeElement.AppendChild(outEl);
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            foreach (XmlNode subNode in nodeElement.ChildNodes)
            {
                if (subNode.Name.Equals(typeof(double).FullName))
                {
                    double value = Value;
                    double min = Min;
                    double max = Max;

                    foreach (XmlAttribute attr in subNode.Attributes)
                    {
                        if (attr.Name.Equals("value"))
                            value = DeserializeValue(attr.Value);
                        else if (attr.Name.Equals("min"))
                        {
                            min = Convert.ToDouble(attr.Value, CultureInfo.InvariantCulture);
                        }
                        else if (attr.Name.Equals("max"))
                        {
                            max = Convert.ToDouble(attr.Value, CultureInfo.InvariantCulture);
                        }
                    }

                    Min = min;
                    Max = max;
                    Value = value;
                }
            }
        }

        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context); //Base implementation must be called.
            if (context == SaveContext.Undo)
            {
                var helper = new XmlElementHelper(element);
                helper.SetAttribute("min", Min);
                helper.SetAttribute("max", Max);
            }
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context); //Base implementation must be called.
            if (context == SaveContext.Undo)
            {
                var helper = new XmlElementHelper(element);
                Min = helper.ReadDouble("min");
                Max = helper.ReadDouble("max");
            }
        }

        #endregion
    }

    [NodeName("Integer Slider")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("Change an integer value with a slider.")]
    public partial class IntegerSliderInput : Integer
    {
        private int max;
        private int min;

        public IntegerSliderInput()
        {
            RegisterAllPorts();

            Min = 0;
            Max = 100;
            Value = 50;
        }

        public override int Value
        {
            get
            {
                return base.Value;
            }
            set
            {
                base.Value = value;

                Debug.WriteLine(string.Format("Min:{0},Max:{1},Value:{2}", Min.ToString(CultureInfo.InvariantCulture), Max.ToString(CultureInfo.InvariantCulture), Value.ToString(CultureInfo.InvariantCulture)));
            }
        }

        public int Max
        {
            get { return max; }
            set
            {
                max = value;

                if (max < Value)
                    Value = max;

                RaisePropertyChanged("Max");
            }
        }

        public int Min
        {
            get { return min; }
            set
            {
                min = value;

                if (min > Value)
                    Value = min;

                RaisePropertyChanged("Min");
            }
        }

        protected override int DeserializeValue(string val)
        {
            try
            {
                return Convert.ToInt32(val, CultureInfo.InvariantCulture);
            }
            catch
            {
                return 0;
            }
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            XmlElement outEl = xmlDoc.CreateElement(typeof(int).FullName);
            outEl.SetAttribute("value", Value.ToString(CultureInfo.InvariantCulture));
            outEl.SetAttribute("min", Min.ToString(CultureInfo.InvariantCulture));
            outEl.SetAttribute("max", Max.ToString(CultureInfo.InvariantCulture));
            nodeElement.AppendChild(outEl);
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            foreach (XmlNode subNode in nodeElement.ChildNodes)
            {
                if (subNode.Name.Equals(typeof(int).FullName))
                {
                    int value = Value;
                    int min = Min;
                    int max = Max;

                    foreach (XmlAttribute attr in subNode.Attributes)
                    {
                        if (attr.Name.Equals("value"))
                            value = DeserializeValue(attr.Value);
                        else if (attr.Name.Equals("min"))
                        {
                            min = Convert.ToInt32(attr.Value, CultureInfo.InvariantCulture);
                        }
                        else if (attr.Name.Equals("max"))
                        {
                            max = Convert.ToInt32(attr.Value, CultureInfo.InvariantCulture);
                        }
                    }

                    Min = min;
                    Max = max;
                    Value = value;
                }
            }
        }

        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context); //Base implementation must be called.
            if (context == SaveContext.Undo)
            {
                var helper = new XmlElementHelper(element);
                helper.SetAttribute("min", Min);
                helper.SetAttribute("max", Max);
            }
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context); //Base implementation must be called.
            if (context == SaveContext.Undo)
            {
                var helper = new XmlElementHelper(element);
                Min = helper.ReadInteger("min");
                Max = helper.ReadInteger("max");
            }
        }

        #endregion
    }

    [NodeName("Boolean")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("Selection between a true and false.")]
    [NodeSearchTags("true", "truth", "false")]
    public partial class BoolSelector : Bool
    {
        public BoolSelector()
        {
            RegisterAllPorts();
        }

        protected override bool DeserializeValue(string val)
        {
            try
            {
                return val.ToLower().Equals("true");
            }
            catch
            {
                return false;
            }
        }

    }

    [NodeName("String")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("Creates a string.")]
    public partial class StringInput : String
    {
        //dynTextBox tb;

        public override string Value
        {
            get
            {
                return HttpUtility.HtmlDecode(base.Value);
            }
            set
            {
                base.Value = value;
            }
        }

        public StringInput()
        {
            RegisterAllPorts();
            Value = "";
        }

        protected override string DeserializeValue(string val)
        {
            return val;
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            XmlElement outEl = xmlDoc.CreateElement(typeof(string).FullName);
            outEl.SetAttribute("value", Value.ToString(CultureInfo.InvariantCulture));
            nodeElement.AppendChild(outEl);
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            foreach (XmlNode subNode in nodeElement.ChildNodes)
            {
                if (subNode.Name.Equals(typeof(string).FullName))
                {
                    foreach (XmlAttribute attr in subNode.Attributes)
                    {
                        if (attr.Name.Equals("value"))
                        {
                            Value = DeserializeValue(attr.Value);
                        }
                    }
                }
            }
        }

        [NodeMigration(from:"0.5.3.0")]
        public static void Migrate_0530_to_0600(XmlNode nodeElement)
        {
            var query = from XmlNode subNode in nodeElement.ChildNodes
                        where subNode.Name.Equals(typeof(string).FullName)
                        from XmlAttribute attr in subNode.Attributes
                        where attr.Name.Equals("value")
                        select attr;

            foreach (XmlAttribute attr in query)
                attr.Value = HttpUtility.HtmlEncode(HttpUtility.UrlDecode(attr.Value));
        }
    }

    [NodeName("Directory")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("Allows you to select a directory on the system to get its path.")]
    public partial class StringDirectory : StringFilename
    {
        protected override string DeserializeValue(string val)
        {
            if (Directory.Exists(val))
            {
                return val;
            }
            else
            {
                return "";
            }
        }

    }

    [NodeName("File Path")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("Allows you to select a file on the system to get its filename.")]
    public partial class StringFilename : BasicInteractive<string>
    {
        //TextBox tb;

        public StringFilename()
        {
            RegisterAllPorts();
            
            Value = "";
        }

        protected override string DeserializeValue(string val)
        {
            if (File.Exists(val))
            {
                return val;
            }
            else
            {
                return "";
            }
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            if (string.IsNullOrEmpty(Value))
                throw new Exception("No file selected.");

            return FScheme.Value.NewString(Value);
        }

        public override string PrintExpression()
        {
            return "\"" + base.PrintExpression() + "\"";
        }
    }

    #endregion

    #region Strings and Conversions

    [NodeName("Concat Strings")]
    [NodeDescription("Concatenates two or more strings")]
    [NodeCategory(BuiltinNodeCategories.CORE_STRINGS)]
    public class ConcatStrings : VariableInput
    {
        public ConcatStrings()
        {
            InPortData.Add(new PortData("s1", "String #1", typeof(Value.String)));
            InPortData.Add(new PortData("s2", "String #2", typeof(Value.String)));
            OutPortData.Add(new PortData("combined", "Combined lists", typeof(Value.String)));

            RegisterAllPorts();
        }

        protected override string GetInputRootName()
        {
            return "s";
        }

        protected override string GetTooltipRootName()
        {
            return "String #";
        }

        protected override int GetInputNameIndex()
        {
            return InPortData.Count + 1;
        }

        protected internal override void RemoveInput()
        {
            if (InPortData.Count > 2)
                base.RemoveInput();
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            foreach (var inport in InPortData.Skip(2))
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
                    InPortData.Add(new PortData(subNode.Attributes["name"].Value, "", typeof(object)));
                }
            }
            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            return Value.NewString(string.Concat(args.Cast<Value.String>().Select(x => x.Item)));
        }
    }

    [NodeName("String to Number")]
    [NodeDescription("Converts a string to a number")]
    [NodeCategory(BuiltinNodeCategories.CORE_STRINGS)]
    public class String2Num : BuiltinFunction
    {
        public String2Num()
            : base(FScheme.StringToNum)
        {
            InPortData.Add(new PortData("s", "A string", typeof(Value.String)));
            OutPortData.Add(new PortData("n", "A number", typeof(Value.Number)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
        }
    }

    [NodeName("Number to String")]
    [NodeDescription("Converts a number to a string")]
    [NodeCategory(BuiltinNodeCategories.CORE_STRINGS)]
    public class Num2String : BuiltinFunction
    {
        public Num2String()
            : base(FScheme.NumToString)
        {
            InPortData.Add(new PortData("n", "A number", typeof(Value.Number)));
            OutPortData.Add(new PortData("s", "A string", typeof(Value.String)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
        }
    }

    [NodeName("String Length")]
    [NodeDescription("Calculates the length of a string.")]
    [NodeCategory(BuiltinNodeCategories.CORE_STRINGS)]
    public class StringLen : NodeWithOneOutput
    {
        public StringLen()
        {
            InPortData.Add(new PortData("s", "A string", typeof(Value.String)));
            OutPortData.Add(new PortData("len(s)", "Length of given string", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            return Value.NewNumber(((Value.String)args[0]).Item.Length);
        }
    }

    [NodeName("To String")]
    [NodeDescription("Converts anything into it's string representation")]
    [NodeCategory(BuiltinNodeCategories.CORE_STRINGS)]
    public class ToString : NodeWithOneOutput
    {
        public ToString()
        {
            InPortData.Add(new PortData("input", "Anything", typeof(Value.Number))); // proxy for any type
            OutPortData.Add(new PortData("string", "The string representation of the input", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            return Value.NewString(NodeModel.PrintValue(args[0],0,10000,0, 25));
        }
    }

    [NodeName("Split String")]
    [NodeDescription("Splits given string around given delimiter into a list of sub strings.")]
    [NodeCategory(BuiltinNodeCategories.CORE_STRINGS)]
    public class SplitString : NodeWithOneOutput
    {
        public SplitString()
        {
            InPortData.Add(new PortData("str", "String to split", typeof(Value.String)));
            InPortData.Add(new PortData("del", "Delimiter", typeof(Value.String), Value.NewString("")));
            OutPortData.Add(new PortData("strs", "List of split strings", typeof(Value.List)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            string str = ((Value.String)args[0]).Item;
            string del = ((Value.String)args[1]).Item;

            return Value.NewList(
                Utils.SequenceToFSharpList(
                    del == ""
                        ? str.ToCharArray().Select(c => Value.NewString(c.ToString()))
                        : str.Split(new[] { del }, StringSplitOptions.None).Select(Value.NewString)));
        }
    }

    [NodeName("Join Strings")]
    [NodeDescription("Joins the given list of strings around the given delimiter.")]
    [NodeCategory(BuiltinNodeCategories.CORE_STRINGS)]
    public class JoinStrings : NodeWithOneOutput
    {
        public JoinStrings()
        {
            InPortData.Add(new PortData("strs", "List of strings to join.", typeof(Value.List)));
            InPortData.Add(new PortData("del", "Delimier", typeof(Value.String), Value.NewString("")));
            OutPortData.Add(new PortData("str", "Joined string", typeof(Value.String)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var strs = ((Value.List)args[0]).Item;
            var del = ((Value.String)args[1]).Item;

            return Value.NewString(
                string.Join(del, strs.Select(x => ((Value.String)x).Item))
            );
        }
    }

    [NodeName("String Case")]
    [NodeDescription("Converts a string to uppercase or lowercase")]
    [NodeCategory(BuiltinNodeCategories.CORE_STRINGS)]
    public class StringCase : NodeWithOneOutput
    {
        public StringCase()
        {
            InPortData.Add(new PortData("str", "String to convert", typeof(Value.String)));
            InPortData.Add(new PortData("upper?", "True = Uppercase, False = Lowercase", typeof(Value.Number)));
            OutPortData.Add(new PortData("s", "Converted string", typeof(Value.String)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            string s = ((Value.String)args[0]).Item;
            bool upper = ((Value.Number)args[1]).Item == 1.0;

            return Value.NewString(
                upper ? s.ToUpper() : s.ToLower()
            );
        }
    }

    [NodeName("Substring")]
    [NodeDescription("Gets a substring of a given string")]
    [NodeCategory(BuiltinNodeCategories.CORE_STRINGS)]
    public class Substring : NodeWithOneOutput
    {
        public Substring()
        {
            InPortData.Add(new PortData("str", "String to take substring from", typeof(Value.String)));
            InPortData.Add(new PortData("start", "Starting index of substring", typeof(Value.Number)));
            InPortData.Add(new PortData("length", "Length of substring", typeof(Value.Number)));
            OutPortData.Add(new PortData("sub", "Substring", typeof(Value.String)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            string s = ((Value.String)args[0]).Item;
            double start = ((Value.Number)args[1]).Item;
            double length = ((Value.Number)args[2]).Item;

            return Value.NewString(s.Substring((int)start, (int)length));
        }
    }

    #endregion

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
        private ObservableCollection<DynamoDropDownItem> items = new ObservableCollection<DynamoDropDownItem>();
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

        void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //sort the collection when changed
            //rehook the collection changed event
            var sortedItems = from item in Items
                              orderby item.Name
                              select item;
            Items = sortedItems.ToObservableCollection();
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

        public virtual void PopulateItems()
        {
            //override in child classes
        }

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
