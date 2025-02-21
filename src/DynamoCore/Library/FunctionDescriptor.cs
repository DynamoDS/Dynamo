using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Dynamo.Configuration;
using Dynamo.Graph.Nodes;
using Dynamo.Interfaces;
using Dynamo.Library;
using ProtoCore;
using ProtoCore.DSASM;
using ProtoCore.Utils;

namespace Dynamo.Engine
{
    /// <summary>
    ///     Describes a function, whether imported or defined in a custom node.
    /// </summary>
    public interface IFunctionDescriptor
    {
        /// <summary>
        ///     Name to be displayed for the function.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        ///     An unique name to identify a function. It is used to create 
        ///     a corresponding node instance
        /// </summary>
        string MangledName { get; }

        /// <summary>
        ///     Return keys for multi-output functions.
        /// </summary>
        IEnumerable<string> ReturnKeys { get; }

        /// <summary>
        ///     Function parameters
        /// </summary>
        IEnumerable<TypedParameter> Parameters { get; }

        /// <summary>
        ///     Function name.
        /// </summary>
        string FunctionName { get; }

        /// <summary>
        ///     Return Type
        /// </summary>
        ProtoCore.Type ReturnType { get; }
    }

    /// <summary>
    ///     Contains parameters for function description.
    /// </summary>
    public class FunctionDescriptorParams
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionDescriptorParams"/> class.
        /// </summary>
        public FunctionDescriptorParams()
        {
            IsVisibleInLibrary = true;
            Parameters = new List<TypedParameter>();
            ReturnKeys = new List<string>();
            ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var);
        }

        /// <summary>
        ///     Returns full path to the assembly the defined this function
        /// </summary>
        public string Assembly { get; set; }

        /// <summary>
        ///     Returns class name of this function. If the functinon is global, return String.Empty.
        /// </summary>
        public string ClassName { get; set; }

        /// <summary>
        ///     Returns function name.
        /// </summary>
        public string FunctionName { get; set; }

        /// <summary>
        ///     Returns comment describing the function along with the signature
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        ///     Message specified if function is obsolete
        /// </summary>
        public string ObsoleteMsg { get; set; }

        /// <summary>
        /// Returns function parameters data
        /// </summary>
        public IEnumerable<TypedParameter> Parameters { get; set; }

        /// <summary>
        /// Describes the type of object to return by the function
        /// </summary>
        public ProtoCore.Type ReturnType { get; set; }

        /// <summary>
        /// Describes type of function
        /// </summary>
        public FunctionType FunctionType { get; set; }

        /// <summary>
        ///     This attribute sets, if this function is shown in library or not.
        /// </summary>
        public bool IsVisibleInLibrary { get; set; }

        /// <summary>
        /// This attribute sets whether the function enables periodic update of the workspace.
        /// </summary>
        public bool CanUpdatePeriodically { get; set; }

        /// <summary>
        ///     If the function returns a dictionary, ReturnKeys is the key collection
        /// used in returned dictionary.
        /// </summary>
        public IEnumerable<string> ReturnKeys { get; set; }

        /// <summary>
        ///     Returns instance of IPathManager
        /// </summary>
        public IPathManager PathManager { get; set; }

        /// <summary>
        ///     Does the function accept a variable number of arguments?
        /// </summary>
        public bool IsVarArg { get; set; }

        /// <summary>
        ///     Indicates if it is built-in function
        /// </summary>
        public bool IsBuiltIn { get; set; }

        /// <summary>
        ///     Indicates if the function is packaged element (either zero-touch DLLs or DYFs)
        /// </summary>
        public bool IsPackageMember { get; set; }

        /// <summary>
        ///     Indicates if the lacing strategy is disabled on the function
        /// </summary>
        public bool IsLacingDisabled { get; set; }
        //TODO - should this somehow contain more info - ExperimentalInfo{IsExperimental, ExperimentalMessage/url}?}
        /// <summary>
        /// Experimental/Unstable function
        /// </summary>
        internal bool IsExperimental { get; set; }
    }

    /// <summary>
    ///     Describe a DesignScript function in a imported library
    /// </summary>
    public class FunctionDescriptor : IFunctionDescriptor
    {
        /// <summary>
        /// A dictionary of loaded assemblies by assembly name to speed up Dynamo loading
        /// </summary>
        private static Dictionary<string, Assembly> assembliesByName;

        /// <summary>
        /// Ensure the assembly cache is kept around until all callers have finished using it
        /// </summary>
        private static int assemblyCachingRequests = 0;

        /// <summary>
        ///     A comment describing the Function
        /// </summary>
        private string summary;

        private readonly IPathManager pathManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionDescriptor"/> class.
        /// </summary>
        /// <param name="funcDescParams">Function descriptor parameters.</param>
        public FunctionDescriptor(FunctionDescriptorParams funcDescParams)
        {
            if (!String.IsNullOrEmpty(funcDescParams.Summary))
            {
                summary = funcDescParams.Summary;
            }

            pathManager = funcDescParams.PathManager;
            Assembly = funcDescParams.Assembly;
            ClassName = funcDescParams.ClassName;
            FunctionName = funcDescParams.FunctionName;

            Parameters = funcDescParams.Parameters.Select(
                x =>
                {
                    x.UpdateFunctionDescriptor(this);
                    return x;
                }).ToList();

            var type = funcDescParams.FunctionType;
            var inputParameters = new List<Tuple<string, string>>();
            //Add instance parameter as one of the inputs for instance method as well as properties.
            if (type == FunctionType.InstanceMethod || type == FunctionType.InstanceProperty)
                inputParameters.Add(Tuple.Create(UnqualifedClassName.ToLower(), UnqualifedClassName));

            if (Parameters.Any())
            {
                inputParameters.AddRange(Parameters.Select(
                    par => Tuple.Create(par.Name, par.DisplayTypeName)));
            }

            InputParameters = inputParameters;
            ReturnType = funcDescParams.ReturnType;
            Type = type;
            ReturnKeys = funcDescParams.ReturnKeys;
            IsVarArg = funcDescParams.IsVarArg;
            IsVisibleInLibrary = funcDescParams.IsVisibleInLibrary;
            ObsoleteMessage = funcDescParams.ObsoleteMsg;
            CanUpdatePeriodically = funcDescParams.CanUpdatePeriodically;
            IsBuiltIn = funcDescParams.IsBuiltIn;
            IsPackageMember = funcDescParams.IsPackageMember;
            IsLacingDisabled = funcDescParams.IsLacingDisabled;
            IsExperimental = funcDescParams.IsExperimental || CheckIfFunctionIsMarkedExperimentalByPrefs(this);
        }

        /// <summary>
        ///     Indicates if the function overloads
        /// </summary>
        public bool IsOverloaded { get; set; }

        /// <summary>
        ///     Full path to the assembly which defined this function
        /// </summary>
        public string Assembly { get; private set; }

        /// <summary>
        ///     Class name of this function. If the function is global, return String.Empty.
        /// </summary>
        public string ClassName { get; private set; }

        /// <summary>
        ///     Function name.
        /// </summary>
        public string FunctionName { get; private set; }

        /// <summary>
        ///     Function parameters.
        /// </summary>
        public IEnumerable<TypedParameter> Parameters { get; private set; }

        /// <summary>
        ///     Function return type.
        /// </summary>
        public ProtoCore.Type ReturnType { get; private set; }

        /// <summary>
        ///     If the function returns a dictionary, ReturnKeys is the key collection
        ///     used in returned dictionary.
        /// </summary>
        public IEnumerable<string> ReturnKeys { get; private set; }

        /// <summary>
        ///     Does the function accept a variable number of arguments?
        /// </summary>
        public bool IsVarArg { get; private set; }

        /// <summary>
        ///     Indicates if it is a built-in function
        /// </summary>
        public bool IsBuiltIn { get; private set; }

        /// <summary>
        ///     Indicates if the function is packaged element (either zero-touch DLLs or DYFs)
        /// </summary>
        public bool IsPackageMember { get; private set; }

        /// <summary>
        ///     Message specified if function is obsolete
        /// </summary>
        public string ObsoleteMessage { get; protected set; }

        /// <summary>
        /// Indicates if the function is obsolete
        /// </summary>
        public bool IsObsolete { get { return !string.IsNullOrEmpty(ObsoleteMessage); } }

        /// <summary>
        ///     Function type.
        /// </summary>
        public FunctionType Type { get; private set; }

        /// <summary>
        ///     Returns summary of the function from its documentation xml 
        /// using the corresponding FunctionDescriptor object.
        /// </summary>
        public string Summary
        {
            get { return summary ?? (summary = this.GetSummary()); }
        }

        /// <summary>
        ///     A comment describing the function along with the signature
        /// </summary>
        public string Description
        {
            get { return !String.IsNullOrEmpty(Summary) ? Summary : string.Empty; }
        }

        private IEnumerable<Tuple<string, string>> returns;

        /// <summary>
        ///     If the XML documentation for the function includes a returns field,
        ///     this parameter contains a collection of tuples of output names to
        ///     descriptions.
        /// 
        ///     Otherwise, this list will be empty.
        /// </summary>
        public IEnumerable<Tuple<string, string>> Returns { get { return returns ?? (returns = this.GetReturns()); } }

        /// <summary>
        ///     Inputs for Node
        /// </summary>
        public IEnumerable<Tuple<string, string>> InputParameters
        {
            get;
            private set;
        }

        private string category;

        /// <summary>
        ///     The category of this function.
        /// </summary>
        public string Category
        {
            get
            {
                if (category != null)
                {
                    return category;
                }

                var categoryBuf = new StringBuilder();
                categoryBuf.Append(GetRootCategory());

                //if this is not BuiltIn function search NodeCategoryAttribute for it
                if (ClassName != null)
                {
                    //get function assembly
                    var asmName = Path.GetFileNameWithoutExtension(Assembly);

                    //get class type of function
                    if (TryGetAssembly(asmName, out var asm) && asm.GetType(ClassName) is System.Type type)
                    {
                        //get NodeCategoryAttribute for this function if it was been defined
                        var nodeCat = type.GetMethods().Where(x => x.Name == FunctionName)
                            .Select(x => x.GetCustomAttribute(typeof(NodeCategoryAttribute)))
                            .Where(x => x != null)
                            .Cast<NodeCategoryAttribute>()
                            .Select(x => x.ElementCategory)
                            .FirstOrDefault();

                        //if attribute is found compose node category string with last part from attribute
                        if (!string.IsNullOrEmpty(nodeCat) && (
                            nodeCat == LibraryServices.Categories.Constructors
                            || nodeCat == LibraryServices.Categories.Properties
                            || nodeCat == LibraryServices.Categories.MemberFunctions))
                        {
                            categoryBuf.Append("." + UnqualifedClassName + "." + nodeCat);
                            category = categoryBuf.ToString();
                            return category;
                        }
                    }
                }

                switch (Type)
                {
                    case FunctionType.Constructor:
                        categoryBuf.Append(
                            "." + UnqualifedClassName + "." + LibraryServices.Categories.Constructors);
                        break;

                    case FunctionType.StaticMethod:
                    case FunctionType.InstanceMethod:
                        categoryBuf.Append(
                            "." + UnqualifedClassName + "." + LibraryServices.Categories.MemberFunctions);
                        break;

                    case FunctionType.StaticProperty:
                    case FunctionType.InstanceProperty:
                        categoryBuf.Append(
                            "." + UnqualifedClassName + "." + LibraryServices.Categories.Properties);
                        break;
                }

                category = categoryBuf.ToString();
                return category;
            }
        }

        /// <summary>
        ///     The string that is used to search for this function.
        /// </summary>
        public string QualifiedName
        {
            get
            {
                return FunctionType.GenericFunction == Type
                    ? UserFriendlyName
                    : ClassName + "." + UserFriendlyName;
            }
        }

        /// <summary>
        ///     A unique name to identify a function. It is necessary when a
        ///     function is overloaded.
        /// </summary>
        public string MangledName
        {
            get
            {
                return Parameters != null && Parameters.Any()
                    ? QualifiedName + "@" + string.Join(",", Parameters.Select(p => p.Type))
                    : QualifiedName;
            }
        }

        /// <summary>
        ///     The full signature of the function.
        /// </summary>
        public string Signature
        {
            get
            {
                var descBuf = new StringBuilder();
                descBuf.Append(DisplayName);

                if (Parameters != null && Parameters.Any())
                {
                    string signature = string.Join(", ", Parameters.Select(p => p.ToString()));
                    descBuf.Append(" (");
                    descBuf.Append(signature);
                    descBuf.Append(")");
                }
                else if (FunctionType.InstanceProperty != Type && FunctionType.StaticProperty != Type)
                    descBuf.Append(" ( )");

                var typeName = ReturnType.ToShortString();
                if (!string.IsNullOrEmpty(typeName))
                    descBuf.Append(": " + typeName);

                return descBuf.ToString();
            }
        }

        /// <summary>
        ///     Return a user friendly name. E.g., for operator '+' it will return
        ///     'Add'
        /// </summary>
        public string UserFriendlyName
        {
            get
            {
                if (FunctionName.StartsWith(Constants.kInternalNamePrefix))
                {
                    string name = FunctionName.Substring(Constants.kInternalNamePrefix.Length);

                    Operator op;
                    if (Enum.TryParse(name, out op))
                        name = Op.GetOpSymbol(op);

                    return name;
                }
                return FunctionName;
            }
        }

        /// <summary>
        ///     QualifiedName with leading namespaces removed.
        /// </summary>
        public string DisplayName
        {
            get
            {
                if (FunctionType.GenericFunction == Type)
                    return UserFriendlyName;

                int idx = ClassName.LastIndexOf('.');
                return idx < 0
                    ? QualifiedName
                    : string.Format("{0}.{1}", ClassName.Substring(idx + 1), UserFriendlyName);
            }
        }

        /// <summary>
        ///     This attribute sets, if this function is shown in library or not.
        /// </summary>
        public bool IsVisibleInLibrary { get; private set; }

        /// <summary>
        ///     This attribute sets whether the function enables periodic update of the workspace.
        /// </summary>
        public bool CanUpdatePeriodically { get; private set; }

        /// <summary>
        ///     Returns class name without namespace
        /// </summary>
        public string UnqualifedClassName
        {
            get
            {
                if (string.IsNullOrEmpty(ClassName))
                    return string.Empty;

                int idx = ClassName.LastIndexOf('.');
                return idx < 0 ? ClassName : ClassName.Substring(idx + 1);
            }
        }

        /// <summary>
        ///     Returns namespace where the function is specified
        /// </summary>
        public string Namespace
        {
            get
            {
                if (string.IsNullOrEmpty(ClassName))
                    return string.Empty;

                int idx = ClassName.LastIndexOf('.');
                return idx < 0 ? String.Empty : ClassName.Substring(0, idx);
            }
        }

        /// <summary>
        ///     Returns instance of IPathManager
        /// </summary>
        public IPathManager PathManager { get { return pathManager; } }

        /// <summary>
        ///     Returns if the lacing strategy is disabled
        /// </summary>
        public bool IsLacingDisabled { get; private set; }

        /// <summary>
        ///     Overrides equality check of two <see cref="FunctionDescriptor"/> objects
        /// </summary>
        /// <param name="obj"><see cref="FunctionDescriptor"/> object to compare 
        /// with the current one</param>
        /// <returns>Returns true if two <see cref="FunctionDescriptor"/> objects 
        /// are equals</returns>
        public override bool Equals(object obj)
        {
            if (null == obj || GetType() != obj.GetType())
                return false;

            return MangledName.Equals(obj as FunctionDescriptor);
        }

        /// <summary>
        ///     Overrides computing the hash code for the <see cref="FunctionDescriptor"/>
        /// </summary>
        /// <returns>The hash code for this <see cref="FunctionDescriptor"/></returns>
        public override int GetHashCode()
        {
            return MangledName.GetHashCode();
        }

        private string GetRootCategory()
        {
            if (string.IsNullOrEmpty(Assembly))
            {
                return CoreUtils.IsInternalMethod(FunctionName)
                    ? LibraryServices.Categories.Operators
                    : LibraryServices.Categories.BuiltIn;
            }

            LibraryCustomization cust = LibraryCustomizationServices.GetForAssembly(Assembly, pathManager);

            if (cust != null)
            {
                string f = cust.GetNamespaceCategory(Namespace);
                if (!String.IsNullOrEmpty(f))
                    return f;
            }

            string filename = Path.GetFileNameWithoutExtension(Assembly);

            return string.IsNullOrEmpty(Namespace) ? filename : filename + "." + Namespace;
        }
        private bool CheckIfFunctionIsMarkedExperimentalByPrefs(FunctionDescriptor fd)
        {
            if (PreferenceSettings.InitialExperimentalLib_Namespaces.
                Where(x => x.StartsWith(fd.Assembly + ":")).Select(x => x.Split(":").LastOrDefault()).Any(nsp => fd.QualifiedName.StartsWith(nsp)))
            {
                return true;
            }
            return false;
        }
        internal bool IsExperimental { get;}

        /// <summary>
        /// Try to get an <see cref="System.Reflection.Assembly"/> by name
        /// </summary>
        /// <param name="assemblyName">Name of the assembly to load</param>
        /// <param name="assembly">The assembly</param>
        /// <returns><see langword="true"/> if a matching assembly was found</returns>
        private static bool TryGetAssembly(string assemblyName, out Assembly assembly)
        {
            // Use the lookup dictionary if it exists, to avoid doing .GetName calls.
            if (assembliesByName != null)
            {
                return assembliesByName.TryGetValue(assemblyName, out assembly);
            }

            assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(x => x.GetName().Name == assemblyName);

            return assembly != null;
        }


        /// <summary>
        /// Load all assemblies by name in the current domain for faster <see cref="Category"/> lookup.
        /// </summary>
        /// <returns>An <see cref="IDisposable"/> that removes the assembly dictionary on <see cref="IDisposable.Dispose"/></returns>
        internal static IDisposable CacheAssemblyNamesForZeroTouchNodeSearch()
        {
            return Scheduler.Disposable.Create(() => {
                // If in a nested call, the assembliesByName cache should already exist,
                // and 'assemblyCachingRequests' should be higher than 0
                if (Interlocked.Increment(ref assemblyCachingRequests) == 1)
                {
                    assembliesByName = new();
                    foreach(var asm in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        // Only add the first occurence of an assembly name, to match the
                        // functionality of TryGetAssembly
                        assembliesByName.TryAdd(asm.GetName().Name, asm);
                    }
                }
            },
            () => {
                // If in a nested call, the count should be larger than 1, and the outer
                // caller should be responsible for deleting the cache
                if (Interlocked.Decrement(ref assemblyCachingRequests) == 0)
                {
                    assembliesByName = null;
                }
            });
        }
    }
}
