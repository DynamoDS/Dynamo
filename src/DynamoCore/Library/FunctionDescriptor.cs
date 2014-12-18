using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Dynamo.Library;

using ProtoCore.DSASM;
using ProtoCore.Utils;

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

    /// <summary>
    ///     Describe a DesignScript function in a imported library
    /// </summary>
    public class FunctionDescriptor : IFunctionDescriptor
    {
        /// <summary>
        ///     A comment describing the Function
        /// </summary>
        private string summary;

        public FunctionDescriptor(string name, IEnumerable<TypedParameter> parameters, FunctionType type, bool isOverloaded)
            : this(null, null, name, parameters, null, type, isOverloaded)
        { }

        public FunctionDescriptor(
            string assembly, string className, string functionName, IEnumerable<TypedParameter> parameters,
            string returnType, FunctionType type, bool isOverloaded, bool isVisibleInLibrary = true,
            IEnumerable<string> returnKeys = null, bool isVarArg = false, string obsoleteMsg = "")
            : this(
                assembly,
                className,
                functionName,
                null,
                parameters,
                returnType,
                type,
                isOverloaded,
                isVisibleInLibrary,
                returnKeys,
                isVarArg,
                obsoleteMsg) { }

        public FunctionDescriptor(
            string assembly, string className, string functionName, string summary,
            IEnumerable<TypedParameter> parameters, string returnType, FunctionType type, bool isOverloaded,
            bool isVisibleInLibrary = true, IEnumerable<string> returnKeys = null, bool isVarArg = false, string obsoleteMsg = "")
        {
            this.summary = summary;
            IsOverloaded = isOverloaded;
            Assembly = assembly;
            ClassName = className;
            FunctionName = functionName;

            if (parameters == null)
                Parameters = new List<TypedParameter>();
            else
            {
                Parameters = parameters.Select(
                    x =>
                    {
                        x.Function = this;
                        return x;
                    });
            }

            ReturnType = returnType == null ? "var[]..[]" : returnType.Split('.').Last();
            Type = type;
            ReturnKeys = returnKeys ?? new List<string>();
            IsVarArg = isVarArg;
            IsVisibleInLibrary = isVisibleInLibrary;
            ObsoleteMessage = obsoleteMsg;
        }

        public bool IsOverloaded { get; private set; }

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
            get { return !String.IsNullOrEmpty(Summary) ? Summary + "\n\n" + Signature : Signature; }
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

            LibraryCustomization cust = LibraryCustomizationServices.GetForAssembly(Assembly);

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