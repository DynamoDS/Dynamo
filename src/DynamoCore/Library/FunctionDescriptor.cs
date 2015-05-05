using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dynamo.Interfaces;
using Dynamo.Library;

using ProtoCore.DSASM;
using ProtoCore.Utils;
using ProtoCore;

namespace Dynamo.DSEngine
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
    }

    public class FunctionDescriptorParams
    {
        public FunctionDescriptorParams()
        {
            IsVisibleInLibrary = true;
            Parameters = new List<TypedParameter>();
            ReturnKeys = new List<string>();
            ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar);
        }

        public string Assembly { get; set; }
        public string ClassName { get; set; }
        public string FunctionName { get; set; }
        public string Summary { get; set; }
        public string ObsoleteMsg { get; set; }
        public IEnumerable<TypedParameter> Parameters { get; set; }
        public ProtoCore.Type ReturnType { get; set; }
        public FunctionType FunctionType { get; set; }
        public bool IsVisibleInLibrary { get; set; }
        public bool CanUpdatePeriodically { get; set; }
        public IEnumerable<string> ReturnKeys { get; set; }
        public IPathManager PathManager { get; set; }
        public bool IsVarArg { get; set; }
        public bool IsBuiltIn { get; set; }
    }

    /// <summary>
    ///     Describe a DesignScript function in a imported library
    /// </summary>
    public class FunctionDescriptor : IFunctionDescriptor
    {
        /// <summary>
        ///     A comment describing the Function
        /// </summary>
        private string summary;

        private readonly IPathManager pathManager;

        public FunctionDescriptor(FunctionDescriptorParams funcDescParams)
        {
            summary = funcDescParams.Summary;
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
            if(type == FunctionType.InstanceMethod || type == FunctionType.InstanceProperty)
                inputParameters.Add(Tuple.Create(UnqualifedClassName.ToLower(), UnqualifedClassName));

            if (Parameters.Any())
            {
                inputParameters.AddRange(Parameters.Select(
                    par => Tuple.Create(par.Name, par.DisplayTypeName)));
            }

            InputParameters = inputParameters;
            
            //Not sure why returnType for constructors are var[]..[], use UnqualifiedClassName
            ReturnType = (type == FunctionType.Constructor) ?
                UnqualifedClassName :
                funcDescParams.ReturnType.ToShortString();

            Type = type;
            ReturnKeys = funcDescParams.ReturnKeys;
            IsVarArg = funcDescParams.IsVarArg;
            IsVisibleInLibrary = funcDescParams.IsVisibleInLibrary;
            ObsoleteMessage = funcDescParams.ObsoleteMsg;
            CanUpdatePeriodically = funcDescParams.CanUpdatePeriodically;
            IsBuiltIn = funcDescParams.IsBuiltIn;
        }

        public bool IsOverloaded { get; set; }

        /// <summary>
        ///     Full path to the assembly the defined this function
        /// </summary>
        public string Assembly { get; private set; }

        /// <summary>
        ///     Class name of this function. If the functino is global function,
        ///     return String.Empty.
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
        public string ReturnType { get; private set; }

        /// <summary>
        ///     If the function returns a dictionary, ReturnKeys is the key collection
        ///     used in returned dictionary.
        /// </summary>
        public IEnumerable<string> ReturnKeys { get; private set; }

        /// <summary>
        ///     Does the function accept a variable number of arguments?
        /// </summary>
        public bool IsVarArg { get; private set; }

        public bool IsBuiltIn { get; private set; }

        public string ObsoleteMessage { get; protected set; }
        public bool IsObsolete { get { return !string.IsNullOrEmpty(ObsoleteMessage); } }

        /// <summary>
        ///     Function type.
        /// </summary>
        public FunctionType Type { get; private set; }

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

        /// <summary>
        ///     Inputs for Node
        /// </summary>
        public IEnumerable<Tuple<string, string>> InputParameters
        {
            get;
            private set;
        }
        /// <summary>
        ///     The category of this function.
        /// </summary>
        public string Category
        {
            get
            {
                var categoryBuf = new StringBuilder();
                categoryBuf.Append(GetRootCategory());
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
                return categoryBuf.ToString();
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

                if (!string.IsNullOrEmpty(ReturnType))
                    descBuf.Append(": " + ReturnType);

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
        /// This attribute sets whether the function enables periodic update of the workspace.
        /// </summary>
        public bool CanUpdatePeriodically { get; private set; }

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

        public IPathManager PathManager { get { return pathManager; } }

        public override bool Equals(object obj)
        {
            if (null == obj || GetType() != obj.GetType())
                return false;

            return MangledName.Equals(obj as FunctionDescriptor);
        }

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
                    : LibraryServices.Categories.BuiltIns;
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
    }
}
