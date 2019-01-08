using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using System.Linq;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.Utils;
using ProtoCore.DSASM;

namespace ProtoFFI
{
    /// <summary>
    /// This class creates ClassDeclNode for a given type and caches all the
    /// imported types. This class also keeps the list of FFIFunctionPointer
    /// for the given type.
    /// </summary>
    public class CLRModuleType
    {
        #region CONSTRUCTORS

        /// <summary>
        /// Private constructor to create empty CLRModuleType.
        /// </summary>
        /// <param name="type">System.Type</param>
        private CLRModuleType(Type type)
        {
            CLRType = type;
            string classname = CLRObjectMarshaler.GetTypeName(type);
            ClassNode = CreateEmptyClassNode(classname);
            ClassNode.IsStatic = type.IsAbstract && type.IsSealed;
        }

        static CLRModuleType()
        {
            mDisposeMethod = typeof(CLRModuleType).GetMethod("Dispose", BindingFlags.Static | BindingFlags.NonPublic);
        }
        #endregion

        #region PUBLIC_METHODS_AND_PROPERTIES

        /// <summary>
        /// Returns CLRModuleType for given Type. If CLRModuleType instance for the
        /// given type is not found, it creates a new one. If CLRDLLModule is
        /// passed as null, it creates empty CLRModuleType.
        /// </summary>
        /// <param name="type">System.Type to be imported in DesignScript</param>
        /// <param name="module">CLRDLLModule which imports this type</param>
        /// <param name="alias">Alias name, if any. For now its not supported.</param>
        public static CLRModuleType GetInstance(Type type, CLRDLLModule module, string alias)
        {
            CLRModuleType mtype;
            if (!mTypes.TryGetValue(type, out mtype))
            {
                lock (mTypes)
                {
                    if (!mTypes.TryGetValue(type, out mtype))
                    {
                        mtype = new CLRModuleType(type);
                        //Now check that a type with same name is not imported.
                        Type otherType;
                        if (mTypeNames.TryGetValue(mtype.FullName, out otherType))
                            throw new InvalidOperationException(string.Format("Can't import {0}, {1} is already imported as {2}, namespace support needed.", type.FullName, type.Name, otherType.FullName));

                        mTypes.Add(type, mtype);
                        mTypeNames.Add(mtype.FullName, type);
                    }
                }
            }

            if (module != null && mtype.Module == null)
            {
                mtype.Module = module;
                if (type.IsEnum)
                    mtype.ClassNode = mtype.ParseEnumType(type, alias);
                else
                    mtype.ClassNode = mtype.ParseSystemType(type, alias);
            }

            return mtype;
        }

        /// <summary>
        /// Returns all the types, which was referenced by other types but were not
        /// imported explicitly. These are empty types and corresponding DS Type
        /// don't contain any methods, constructors, properties or fields
        /// </summary>
        /// <returns>List of CLRModuleType</returns>
        public static List<CLRModuleType> GetEmptyTypes()
        {
            return GetTypes(isEmpty);
        }


        /// <summary>
        /// Returns all the types for the given predicate.
        /// </summary>
        /// <param name="predicate">A delegate for defining criteria</param>
        /// <returns>List of CLRModuleType</returns>
        public static List<CLRModuleType> GetTypes(Func<CLRModuleType, bool> predicate)
        {
            List<CLRModuleType> types = new List<CLRModuleType>();
            foreach (var item in mTypes)
            {
                if (predicate(item.Value))
                    types.Add(item.Value);
            }

            return types;
        }

        /// <summary>
        /// Returns list of function pointers available on this type for a given
        /// function name
        /// </summary>
        /// <param name="name">Function name</param>
        /// <returns>List of FFIFunctionPointer</returns>
        public List<FFIFunctionPointer> GetFunctionPointers(string name)
        {
            List<FFIFunctionPointer> pointers = null;
            if (!mFunctionPointers.TryGetValue(name, out pointers))
            {
                pointers = new List<FFIFunctionPointer>();
                mFunctionPointers[name] = pointers;
            }
            return pointers;
        }

        /// <summary>
        /// Ensures that dispose method node is created for this empty type.
        /// </summary>
        /// <param name="module">Reference module</param>
        public void EnsureDisposeMethod(CLRDLLModule module)
        {
            foreach (var item in ClassNode.Procedures)
            {
                if (CoreUtils.IsDisposeMethod(item.Name))
                    return; //Dispose method is already present.
            }
            bool resetModule = false;
            if (Module == null)
            {
                Module = module;
                resetModule = true;
            }
            AssociativeNode node = ParseMethod(mDisposeMethod);
            FunctionDefinitionNode func = node as FunctionDefinitionNode;
            if (func != null)
            {
                func.Name = ProtoCore.DSDefinitions.Keyword.Dispose;
                func.IsStatic = false;
                func.IsAutoGenerated = true;
                ClassNode.Procedures.Add(func);
            }
            if (resetModule)
                Module = null;
        }

        public static MethodInfo DisposeMethod
        {
            get
            {
                return mDisposeMethod;
            }
        }

        /// <summary>
        /// Imported ClassDeclNode
        /// </summary>
        public ClassDeclNode ClassNode { get; private set; }

        /// <summary>
        /// DesignScript Class name, together with Namespace name
        /// </summary>
        public string FullName { get { return ClassNode.ClassName; } }

        /// <summary>
        /// CLRDLLModule from which this type was imported.
        /// </summary>
        public CLRDLLModule Module { get; private set; }

        /// <summary>
        /// System.Type that was imported
        /// </summary>
        public Type CLRType { get; private set; }
        
        
        /// <summary>
        /// Imported ProtoCore.Type
        /// </summary>
        public ProtoCore.Type ProtoCoreType
        {
            get
            {
                if (null == mProtoCoreType)
                    mProtoCoreType = CLRObjectMarshaler.GetUserDefinedType(CLRType);

                return mProtoCoreType.Value;
            }
        }

        public static bool TryGetImportedDSType(Type type, out ProtoCore.Type dsType)
        {
            return mTypeMaps.TryGetValue(type, out dsType);
        }

        public static void SetTypeAttributes(Type type, FFIClassAttributes attributes)
        {
            lock (mTypeAttributeMaps)
            {
                mTypeAttributeMaps[type] = attributes;
            }
        }

        public static bool TryGetTypeAttributes(Type type, out FFIClassAttributes attributes)
        {
            return mTypeAttributeMaps.TryGetValue(type, out attributes);
        }

        public static ProtoCore.Type GetProtoCoreType(Type type, CLRDLLModule module)
        {
            ProtoCore.Type protoCoreType;
            if (mTypeMaps.TryGetValue(type, out protoCoreType))
                return protoCoreType;

            if (type == typeof(object) || !CLRObjectMarshaler.IsMarshaledAsNativeType(type))
            {
                if (type.IsEnum)
                    protoCoreType = CLRModuleType.GetInstance(type, module, string.Empty).ProtoCoreType;
                else
                    protoCoreType = CLRModuleType.GetInstance(type, null, string.Empty).ProtoCoreType;
            }
            else
                protoCoreType = CLRObjectMarshaler.GetProtoCoreType(type);

            lock (mTypeMaps)
            {
                mTypeMaps[type] = protoCoreType;
            }
            return protoCoreType;
        }

        public static System.Type GetImportedType(string typename)
        {
            Type type = null;
            if (mTypeNames.TryGetValue(typename, out type))
                return type;

            return null;
        }
        #endregion

        #region PRIVATE_METHODS_AND_FIELDS

        private ProtoCore.Type? mProtoCoreType;

        readonly Dictionary<string, List<FFIFunctionPointer>> mFunctionPointers = new Dictionary<string, List<FFIFunctionPointer>>();

        private Dictionary<MethodInfo, Attribute[]> mGetterAttributes = new Dictionary<MethodInfo, Attribute[]>();

        private static readonly Dictionary<Type, CLRModuleType> mTypes = new Dictionary<Type, CLRModuleType>();

        private static readonly Dictionary<string, Type> mTypeNames = new Dictionary<string, Type>();

        private static readonly Dictionary<System.Type, ProtoCore.Type> mTypeMaps = new Dictionary<Type, ProtoCore.Type>();

        private static readonly Dictionary<System.Type, FFIClassAttributes> mTypeAttributeMaps = new Dictionary<System.Type, FFIClassAttributes>();

        private Type GetBaseType(Type type)
        {
            Type b = type.BaseType;
            if (null != b && SupressesImport(b))
                return GetBaseType(b);

            return b;
        }

        private ClassDeclNode ParseEnumType(Type type, string alias)
        {
            //TODO: For now Enum can't be suppressed.
            Validity.Assert(type.IsEnum, "Non enum type is being imported as enum!!");

            string classname = alias;
            if (string.IsNullOrEmpty(classname))
                classname = CLRObjectMarshaler.GetTypeName(type);

            ProtoCore.AST.AssociativeAST.ClassDeclNode classnode = CreateEmptyClassNode(classname);
            classnode.ExternLibName = Module.Name;
            classnode.Name = type.Name;
            classnode.IsStatic = type.IsAbstract && type.IsSealed;

            FieldInfo[] fields = type.GetFields();
            foreach (var f in fields)
            {
                if (f.FieldType != type)
                    continue;

                VarDeclNode variable = ParseFieldDeclaration(f);
                if (null == variable)
                    continue;
                variable.IsStatic = true;
                classnode.Variables.Add(variable);
                FunctionDefinitionNode func = ParseFieldAccessor(f);
                if (null != func)
                {
                    func.IsStatic = true;
                    RegisterFunctionPointer(func.Name, f, null, func.ReturnType);
                    classnode.Procedures.Add(func);
                }
            }

            //Get all the attributes on this type and set it to the classnode.
            FFIClassAttributes cattrs = new FFIClassAttributes(type);
            classnode.ClassAttributes = cattrs;
            SetTypeAttributes(type, cattrs);

            return classnode;
        }

        private ClassDeclNode ParseSystemType(Type type, string alias)
        {
            Validity.Assert(!SupressesImport(type), "Supressed type is being imported!!");

            string classname = alias;
            if (classname == null | classname == string.Empty)
                classname = CLRObjectMarshaler.GetTypeName(type);

            ProtoCore.AST.AssociativeAST.ClassDeclNode classnode = CreateEmptyClassNode(classname);
            classnode.ExternLibName = Module.Name;
            classnode.Name = type.Name;

            Type baseType = GetBaseType(type);
            if (baseType != null && !CLRObjectMarshaler.IsMarshaledAsNativeType(baseType))
            {
                string baseTypeName = CLRObjectMarshaler.GetTypeName(baseType);

                classnode.BaseClass = baseTypeName;
                //Make sure that base class is imported properly.
                CLRModuleType.GetInstance(baseType, Module, string.Empty);
            }

            classnode.IsInterface = type.IsInterface;

            foreach (var interf in type.GetInterfaces())
            {
                if (!CLRObjectMarshaler.IsMarshaledAsNativeType(interf))
                {
                    string interfName = CLRObjectMarshaler.GetTypeName(interf);

                    classnode.Interfaces.Add(interfName);
                    CLRModuleType.GetInstance(interf, Module, string.Empty);
                }
            }

            // There is no static class in runtime. static class is simply 
            // marked as sealed and abstract. 
            classnode.IsStatic = type.IsAbstract && type.IsSealed;
            
            // If all methods are static, it doesn't make sense to expose
            // constructor. 
            if (!classnode.IsStatic)
            {
                ConstructorInfo[] ctors = type.GetConstructors();
                foreach (var c in ctors)
                {
                    if (c.IsPublic && !c.IsGenericMethod && !SupressesImport(c))
                    {
                        ConstructorDefinitionNode node = ParseConstructor(c, type);
                        classnode.Procedures.Add(node);

                        List<ProtoCore.Type> argTypes = GetArgumentTypes(node);
                        RegisterFunctionPointer(node.Name, c, argTypes, node.ReturnType);
                    }
                }
            }

            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static;
            bool isDerivedClass = !string.IsNullOrEmpty(classnode.BaseClass);
            if (isDerivedClass) //has base class
                flags |= BindingFlags.DeclaredOnly; //for derived class, parse only class declared methods.

            PropertyInfo[] properties = type.GetProperties(flags);
            foreach (var p in properties)
            {
                AssociativeNode node = ParseProperty(p);
                if (null != node)
                    classnode.Variables.Add(node);
            }

            bool isDisposable = typeof(IDisposable).IsAssignableFrom(type);
            MethodInfo[] methods = type.GetMethods(flags);
            bool hasDisposeMethod = false;

            foreach (var m in methods)
            {
                if (SupressesImport(m, mGetterAttributes))
                    continue;

                if (classnode.IsStatic && m.GetBaseDefinition().DeclaringType == baseType && baseType == typeof(object))
                    continue;

                // Mono issue: m == m.GetBaseDefinition() for methods from Object class returns True instead of False
                if (m.DeclaringType == typeof(object) && m == m.GetBaseDefinition())
                    continue;

                //Don't include overriden methods or generic methods
                if (m.IsPublic && !m.IsGenericMethod && m == m.GetBaseDefinition())
                {
                    AssociativeNode node = ParseAndRegisterFunctionPointer(isDisposable, ref hasDisposeMethod, m);
                    classnode.Procedures.Add(node);
                }
                else if (!hasDisposeMethod && isDisposable && baseType == typeof(Object) && isDisposeMethod(m))
                {
                    AssociativeNode node = ParseAndRegisterFunctionPointer(isDisposable, ref hasDisposeMethod, m);
                    classnode.Procedures.Add(node);
                }
            }
            if (!hasDisposeMethod && !isDisposable)
            {
                AssociativeNode node = ParseAndRegisterFunctionPointer(true, ref hasDisposeMethod, mDisposeMethod);
                classnode.Procedures.Add(node);
            }

            FieldInfo[] fields = type.GetFields();
            foreach (var f in fields)
            {
                if (SupressesImport(f))
                    continue;

                //Supress if defined in super-type
                if (isDerivedClass)
                {
                    FieldInfo[] supertypeFields = baseType.GetFields();

                    if (supertypeFields.Any(superF => superF.Name == f.Name))
                    {
                        continue;
                    }
                }


                VarDeclNode variable = ParseFieldDeclaration(f);
                if (null == variable)
                    continue;
                classnode.Variables.Add(variable);
                FunctionDefinitionNode func = ParseFieldAccessor(f);
                if (null != func)
                    RegisterFunctionPointer(func.Name, f, null, func.ReturnType);
            }

            FFIClassAttributes cattrs = new FFIClassAttributes(type);
            classnode.ClassAttributes = cattrs;
            SetTypeAttributes(type, cattrs);

            return classnode;
        }

        private AssociativeNode ParseAndRegisterFunctionPointer(bool isDisposable, ref bool hasDisposeMethod, MethodInfo m)
        {
            AssociativeNode node = ParseMethod(m);
            List<ProtoCore.Type> argTypes = GetArgumentTypes(node);

            FunctionDefinitionNode func = node as FunctionDefinitionNode;
            if (func != null)
            {
                //Rename the Dispose method to _Dispose, as required by DS.
                if (isDisposable && isDisposeMethod(m))
                {
                    hasDisposeMethod = true;
                    func.Name = ProtoCore.DSDefinitions.Keyword.Dispose;
                    func.IsStatic = false;
                    func.IsAutoGenerated = true;
                }

                RegisterFunctionPointer(func.Name, m, argTypes, func.ReturnType);
            }
            else if (node is ConstructorDefinitionNode)
            {
                ConstructorDefinitionNode constr = node as ConstructorDefinitionNode;
                RegisterFunctionPointer(constr.Name, m, argTypes, constr.ReturnType);
            }
            return node;
        }

        static readonly MethodInfo mDisposeMethod;
        private static void Dispose()
        {
            //Do nothing.
        }

        private static bool isEmpty(CLRModuleType type)
        {
            return null == type.Module;
        }

        private ClassDeclNode CreateEmptyClassNode(string classname)
        {
            ProtoCore.AST.AssociativeAST.ClassDeclNode classnode = new ProtoCore.AST.AssociativeAST.ClassDeclNode();
            classnode.ClassName = classname;
            classnode.Name = null;
            classnode.IsImportedClass = true;

            classnode.IsExternLib = true;
            classnode.ExternLibName = null; //Dummy class.

            return classnode;
        }

        private bool isPropertyAccessor(MethodInfo m)
        {
            //The property accessor methods are compiler generated, hence is marked as special name.
            if (null == m || !m.IsSpecialName)
                return false;

            string name = m.Name;
            int nParams = 0;
            if (name.StartsWith("get_"))
                name.Remove(0, 4);
            else if (name.StartsWith("set_"))
            {
                name.Remove(0, 4);
                nParams = 1;
            }
            else
                return false;

            ParameterInfo[] indexParams = m.GetParameters();
            return (null == indexParams || indexParams.Length == nParams);
        }

        private bool isOverloadedOperator(MethodInfo m)
        {
            if (null == m || !m.IsSpecialName)
                return false;

            return m.Name.StartsWith("op_");
        }

        private bool isDisposeMethod(MethodInfo m)
        {
            ParameterInfo[] ps = m.GetParameters();
            if ((ps == null || ps.Length == 0) && m.Name == "Dispose")
                return true;
            return false;
        }

        private PropertyInfo GetProperty(ref Type type, string name)
        {
            PropertyInfo info = type.GetProperty(name);
            if (null != info)
                return info;
            if (type.BaseType != null)
                type = type.BaseType;
            else
                return null;

            return GetProperty(ref type, name);
        }

        private ProtoCore.AST.AssociativeAST.AssociativeNode ParseProperty(PropertyInfo p)
        {
            MethodInfo m = p.GetAccessors(false)[0];
            var attribs = p.GetCustomAttributes(false).Cast<Attribute>().ToArray();
            mGetterAttributes.Add(m, attribs);

            if (null == p || SupressesImport(p))
                return null;

            //Index properties are not parsed as property at this moment.
            ParameterInfo[] indexParams = p.GetIndexParameters();
            if (null != indexParams && indexParams.Length > 0)
                return null;

            //If this method hides the base class accessor method by signature
            if (m.IsHideBySig)
            {
                //Check if it has a base class
                Type baseType = p.DeclaringType.BaseType;
                PropertyInfo baseProp = (baseType != null) ? GetProperty(ref baseType, p.Name) : null;
                //If this property is also declared in base class, then no need to add this is derived class.
                if (null != baseProp && baseProp.DeclaringType != p.DeclaringType)
                {
                    //base class also has this method.
                    return null;
                }
            }

            ProtoCore.AST.AssociativeAST.VarDeclNode varDeclNode = ParseArgumentDeclaration(p.Name, p.PropertyType);
            if (null != varDeclNode)
                varDeclNode.IsStatic = m.IsStatic;
            return varDeclNode;
        }

        public static bool SupressesImport(MemberInfo member)
        {
            return SupressesImport(member, null);
        }

        private static bool SupressesImport(MemberInfo member, Dictionary<MethodInfo, Attribute[]> getterAttributes)
        {
            if (null == member)
                return true;

            object[] atts = member.GetCustomAttributes(false);

            var method = member as MethodInfo;
            if (method != null)
            {
                // Skip importing methods that have out and ref parameters
                // as these are not supported currently: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-5460
                ParameterInfo[] parameters = method.GetParameters();
                foreach (var parameter in parameters)
                {
                    if (parameter.ParameterType.IsByRef == true)
                        return true;
                }

                // If method is a getter accessor belonging to a property
                // retrieve its attributes from its corresponding property
                if(getterAttributes != null)
                {
                    Attribute[] propAtts = null;
                    if(getterAttributes.TryGetValue(method, out propAtts))
                        atts = propAtts;
                }
            }

            foreach (Attribute item in atts)
            {
                if (item.SupressImportIntoVM())
                    return true;
            }

            return false;
        }

        private bool AllowsRankReduction(MethodInfo method)
        {
            object[] atts = method.GetCustomAttributes(false);
            foreach (var item in atts)
            {
                if (item is AllowRankReductionAttribute)
                    return true;
            }

            return false;
        }

        private ProtoCore.AST.AssociativeAST.VarDeclNode ParseFieldDeclaration(FieldInfo f)
        {
            if (null == f || SupressesImport(f))
                return null;

            ProtoCore.AST.AssociativeAST.VarDeclNode varDeclNode = ParseArgumentDeclaration(f.Name, f.FieldType);
            //TODO: temporary limitation, can't have variable name matching with class name.
            if (null != CLRModuleType.GetImportedType(f.Name))
                return null;
            if (null != varDeclNode)
                varDeclNode.IsStatic = f.IsStatic;
            return varDeclNode;
        }

        private FunctionDefinitionNode ParseFieldAccessor(FieldInfo f)
        {
            if (null == f || SupressesImport(f))
                return null;

            var func = new FunctionDefinitionNode
            {
                Name = string.Format("{0}{1}", Constants.kGetterPrefix, f.Name),
                Signature = new ArgumentSignatureNode(),
                ReturnType = CLRModuleType.GetProtoCoreType(f.FieldType, Module),
                FunctionBody = null,
                Access = ProtoCore.CompilerDefinitions.AccessModifier.Public,
                IsExternLib = true,
                ExternLibName = Module.Name,
                IsStatic = f.IsStatic,
                MethodAttributes = new FFIMethodAttributes(f),
            };
            
            return func;
        }

        private ProtoCore.AST.AssociativeAST.AssociativeNode ParseMethod(MethodInfo method)
        {
            ProtoCore.Type retype = CLRModuleType.GetProtoCoreType(method.ReturnType, Module);
            bool propaccessor = isPropertyAccessor(method);
            bool isOperator = isOverloadedOperator(method);

            FFIMethodAttributes mattrs = new FFIMethodAttributes(method, mGetterAttributes);
            if (method.IsStatic &&
                method.DeclaringType == method.ReturnType &&
                !propaccessor &&
                !isOperator)
            {
                //case for named constructor. Must return a pointer type
                if (!Object.Equals(method.ReturnType, CLRType))
                    throw new InvalidOperationException("Unexpected type for constructor {0D28FC00-F8F4-4049-AD1F-BBC34A68073F}");

                retype = ProtoCoreType;
                ConstructorDefinitionNode node = ParsedNamedConstructor(method, method.Name, retype);
                node.MethodAttributes = mattrs;
                return node;
            }

            string prefix = isOperator ? Constants.kInternalNamePrefix : string.Empty;
            var func = new ProtoCore.AST.AssociativeAST.FunctionDefinitionNode();

            if (isOperator)
            {
                func.Name = string.Format("{0}{1}", prefix, GetDSOperatorName(method.Name));
            }
            else
            {
                func.Name = string.Format("{0}{1}", prefix, method.Name);
            }
            func.Signature = ParseArgumentSignature(method);

            if ((retype.IsIndexable && mattrs.AllowRankReduction)
                || (typeof(object).Equals(method.ReturnType)))
            {
                retype.rank = Constants.kArbitraryRank;
            }
            func.ReturnType = retype;
            func.FunctionBody = null;
            func.Access = ProtoCore.CompilerDefinitions.AccessModifier.Public;
            func.IsExternLib = true;
            func.ExternLibName = Module.Name;
            func.IsStatic = method.IsStatic;
            func.MethodAttributes = mattrs;

            return func;
        }

        /// <summary>
        /// Convert C# overloaded opeator name to DS operator name
        /// </summary>
        /// <param name="methodName"></param>
        /// <returns></returns>
        private string GetDSOperatorName(string methodName)
        {
            switch (methodName)
            {
                case "op_Addition":
                    return Operator.add.ToString();
                case "op_Subtraction":
                    return Operator.sub.ToString();
                case "op_Multiply":
                    return Operator.mul.ToString();
                case "op_Division":
                    return Operator.div.ToString();
                case "op_Modulus":
                    return Operator.mod.ToString();
                case "op_LogicalAnd":
                    return Operator.and.ToString();
                case "op_LogicalOr":
                    return Operator.or.ToString();
                case "op_Equality":
                    return Operator.eq.ToString();
                case "op_GreaterThan":
                    return Operator.gt.ToString();
                case "op_LessThan":
                    return Operator.lt.ToString();
                case "op_Inequality":
                    return Operator.nq.ToString();
                case "op_GreaterThanOrEqual":
                    return Operator.ge.ToString();
                case "op_LessThanOrEqual":
                    return Operator.le.ToString();
                default:
                    return methodName;
            }
        }

        private void RegisterFunctionPointer(string functionName, MemberInfo method, List<ProtoCore.Type> argTypes, ProtoCore.Type retype)
        {
            List<FFIFunctionPointer> pointers = GetFunctionPointers(functionName);
            FFIFunctionPointer f = null;
            if (CoreUtils.IsDisposeMethod(functionName))
                f = new DisposeFunctionPointer(Module, method, retype);
            else if (CoreUtils.IsGetter(functionName))
                f = new GetterFunctionPointer(Module, functionName, method, retype);
            else
                f = new CLRFFIFunctionPointer(Module, functionName, method, argTypes, retype);

            if (!pointers.Contains(f))
                pointers.Add(f);
        }

        private ProtoCore.AST.AssociativeAST.ConstructorDefinitionNode ParseConstructor(ConstructorInfo c, System.Type type)
        {
            //Constructors should always return user defined type object, hence it should be pointer type.
            ProtoCore.Type selfType = ProtoCoreType;

            ProtoCore.AST.AssociativeAST.ConstructorDefinitionNode constr = ParsedNamedConstructor(c, type.Name, selfType);
            return constr;
        }

        private ProtoCore.AST.AssociativeAST.ConstructorDefinitionNode ParsedNamedConstructor(MethodBase method, string constructorName, ProtoCore.Type returnType)
        {
            ProtoCore.AST.AssociativeAST.ConstructorDefinitionNode constr = new ProtoCore.AST.AssociativeAST.ConstructorDefinitionNode();
            constr.Name = constructorName;
            constr.Signature = ParseArgumentSignature(method);
            constr.ReturnType = returnType;
            constr.FunctionBody = null;
            constr.Access = ProtoCore.CompilerDefinitions.AccessModifier.Public;
            constr.IsExternLib = true;
            constr.ExternLibName = Module.Name;

            return constr;
        }

        private ArgumentSignatureNode ParseArgumentSignature(MethodBase method)
        {
            ArgumentSignatureNode argumentSignature = new ArgumentSignatureNode();
            ParameterInfo[] parameters = method.GetParameters();
            foreach (var parameter in parameters)
            {
                var paramNode = ParseArgumentDeclaration(parameter.Name, parameter.ParameterType);

                var ffiAttribute = new FFIParamAttributes(parameter);
                paramNode.ExternalAttributes = ffiAttribute;
                if (ffiAttribute.IsArbitraryDimensionArray)
                {
                    var argType = paramNode.ArgumentType;
                    argType.rank = ProtoCore.DSASM.Constants.kArbitraryRank;
                    paramNode.ArgumentType = argType;
                }

                if (parameter.IsOptional)
                {
                    var lhs = paramNode.NameNode;

                    var defaultValue = parameter.DefaultValue;
                    if (defaultValue != null)
                    {
                        var rhs = AstFactory.BuildPrimitiveNodeFromObject(defaultValue);
                        paramNode.NameNode = AstFactory.BuildBinaryExpression(lhs, rhs, ProtoCore.DSASM.Operator.assign);
                    }
                }
                argumentSignature.AddArgument(paramNode);
            }

            argumentSignature.IsVarArg = parameters.Any()
                && parameters.Last().GetCustomAttributes(typeof(ParamArrayAttribute), false).Any();

            return argumentSignature;
        }

        private ProtoCore.AST.AssociativeAST.VarDeclNode ParseArgumentDeclaration(string parameterName, Type parameterType)
        {
            ProtoCore.AST.AssociativeAST.VarDeclNode varDeclNode = new ProtoCore.AST.AssociativeAST.VarDeclNode();
            varDeclNode.Access = ProtoCore.CompilerDefinitions.AccessModifier.Public;

            ProtoCore.AST.AssociativeAST.IdentifierNode identifierNode = 
                new ProtoCore.AST.AssociativeAST.IdentifierNode
                {
                    Value = parameterName,
                    Name = parameterName,
                    datatype = ProtoCore.TypeSystem.BuildPrimitiveTypeObject(ProtoCore.PrimitiveType.Var, 0)
                };
            //Lets emit native DS type object
            ProtoCore.Type argtype = CLRModuleType.GetProtoCoreType(parameterType, Module);

            varDeclNode.NameNode = identifierNode;
            varDeclNode.ArgumentType = argtype;
            return varDeclNode;
        }

        private List<ProtoCore.Type> GetArgumentTypes(AssociativeNode node)
        {
            ArgumentSignatureNode sigNode = null;
            if (node is FunctionDefinitionNode)
            {
                sigNode = (node as FunctionDefinitionNode).Signature;
            }
            else if (node is ConstructorDefinitionNode)
            {
                sigNode = (node as ConstructorDefinitionNode).Signature;
            }

            if (sigNode != null && sigNode.Arguments != null)
            {
                return sigNode.Arguments.Select(arg => arg.ArgumentType).ToList();
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region INTERNAL_METHODS

        /// <summary>
        /// This method is for testing, to ensure cache is cleared before every test.
        /// </summary>
        public static void ClearTypes()
        {
            lock (mTypes)
            {
                mTypes.Clear();
                mTypeNames.Clear();
                mTypeMaps.Clear();
                mTypeAttributeMaps.Clear();
            }
        }

        #endregion
    }

    /// <summary>
    /// Implements DLLModule for CLR types and FFI. This class supports .NET
    /// module import to DesignScript and provides FFIFunctionPointer &
    /// FFIObjectMarshler.
    /// </summary>
    public class CLRDLLModule : DLLModule
    {
        readonly Dictionary<string, CLRModuleType> mTypes = new Dictionary<string, CLRModuleType>();
        public string Name
        {
            get;
            private set;
        }

        public Module Module
        {
            get;
            private set;
        }

        public Assembly Assembly { get; private set; }

        public CLRDLLModule(string name, Assembly assembly)
        {
            Name = name;
            Assembly = assembly;
        }

        public CLRDLLModule(string name, Module module)
        {
            Name = name;
            Module = module;
        }

        //this is incomplete todo: implement
        public override List<FFIFunctionPointer> GetFunctionPointers(string className, string name)
        {
            CLRModuleType type = null;
            if (mTypes.TryGetValue(className, out type))
                return type.GetFunctionPointers(name);

            if (CoreUtils.IsDisposeMethod(name))
            {
                List<FFIFunctionPointer> pointers = new List<FFIFunctionPointer>();
                pointers.Add(new DisposeFunctionPointer(this, CLRModuleType.DisposeMethod, CLRModuleType.GetProtoCoreType(CLRModuleType.DisposeMethod.ReturnType, this)));
                return pointers;
            }

            throw new KeyNotFoundException(string.Format("Function definition for {0}.{1}, not found", className, name));
        }

        private bool ClassFilter(Type type, object criteria)
        {
            if (type.FullName == (string)criteria)
                return true;

            return type.Name == (string)criteria;
        }

        public override FFIFunctionPointer GetFunctionPointer(string className, string name, List<ProtoCore.Type> argTypes, ProtoCore.Type returnType)
        {
            List<FFIFunctionPointer> pointers = GetFunctionPointers(className, name);
            if (null == pointers)
                return null;

            foreach (var ptr in pointers)
            {
                CLRFFIFunctionPointer clrPtr = ptr as CLRFFIFunctionPointer;
                if (clrPtr.Contains(name, argTypes, returnType))
                    return clrPtr;
            }

            return null;
        }

        public override CodeBlockNode ImportCodeBlock(string typeName, string alias, CodeBlockNode refNode)
        {
            Type[] types = GetTypes(typeName);
            Type exttype = typeof(IExtensionApplication);

            foreach (var type in types)
            {
                //For now there is no support for generic type.
                if (!type.IsGenericType && type.IsPublic && !exttype.IsAssignableFrom(type) && !CLRModuleType.SupressesImport(type))
                {

                    CLRModuleType.GetInstance(type, this, alias);
                    Type[] nestedTypes = type.GetNestedTypes();
                    if (null != nestedTypes && nestedTypes.Length > 0)
                    {
                        foreach (var item in nestedTypes)
                        {
                            CLRModuleType.GetInstance(item, this, string.Empty);
                        }
                    }
                }
            }

            CodeBlockNode node = new CodeBlockNode();
            //Get all the types available on this module.
            //TODO: need to optimize for performance.
            List<CLRModuleType> moduleTypes = CLRModuleType.GetTypes((CLRModuleType mtype) => { return mtype.Module == this; });
            foreach (var item in moduleTypes)
            {
                node.Body.Add(item.ClassNode);
                mTypes[item.FullName] = item; //update Type dictionary.
            }

            //Also add all the available empty class nodes.
            List<CLRModuleType> emptyTypes = CLRModuleType.GetEmptyTypes();
            foreach (var item in emptyTypes)
            {
                item.EnsureDisposeMethod(this);
                node.Body.Add(item.ClassNode);
            }

            string ffidump = Environment.GetEnvironmentVariable("FFIDUMP");
            if (string.Compare(ffidump, "1") == 0)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                foreach (var item in node.Body)
                {
                    sb.Append(item.ToString());
                    sb.AppendLine();
                }
                using (System.IO.FileStream fs = new System.IO.FileStream(string.Format("{0}.ds", this.Name), System.IO.FileMode.Create))
                {
                    byte[] bytes = System.Text.Encoding.ASCII.GetBytes(sb.ToString());
                    fs.Write(bytes, 0, bytes.Length);
                }
            }

            return node;
        }

        public static System.Type GetImplemetationType(Assembly assembly, Type interfaceType, Type assemblyAttribute, bool searchFromAllExportedType)
        {
            if (null == assembly)
                return null;

            object[] attributes = assembly.GetCustomAttributes(assemblyAttribute, true);
            if (null != attributes && attributes.Length > 0 && assemblyAttribute.IsAssignableFrom(attributes[0].GetType()))
            {
                PropertyInfo prop = assemblyAttribute.GetProperty("Type");
                Type t = prop.GetValue(attributes[0], null) as Type;
                if (null != t && interfaceType.IsAssignableFrom(t))
                    return t;
            }

            if (!searchFromAllExportedType)
                return null;

            //Couldn't get interfaceType via assemblyAttribute, 
            //iterate  through all exported types and checkif there is a 
            //interfaceType implementation.
            //
            Type[] types = assembly.GetExportedTypes();
            foreach (var item in types)
            {
                if (!item.IsAbstract && !item.IsInterface && interfaceType.IsAssignableFrom(item))
                    return item;
            }

            return null;
        }

        public override Type GetExtensionAppType()
        {
            Type extensionAppType = typeof(IExtensionApplication);
            Type assemblyAttribute = typeof(Autodesk.DesignScript.Runtime.ExtensionApplicationAttribute);
            return GetImplemetationType((null != Module) ? Module.Assembly : Assembly,
                extensionAppType, assemblyAttribute, false);
        }

        private static void SetOption(Type configType, string setting, object value)
        {
            try
            {
                PropertyInfo prop = configType.GetProperty(setting);
                if (null == prop)
                    return;

                MethodInfo m = prop.GetSetMethod(true);
                if (null != m)
                    m.Invoke(null, new object[] { value });
            }
            catch (System.Exception)
            {
            }
        }

        private Type GetConfigurationType()
        {
            Type[] types = GetTypes(string.Empty);
            foreach (var item in types)
            {
                if ("Configuration" == CLRObjectMarshaler.GetCategory(item))
                    return item;
            }
            return null;
        }

        private Type[] GetTypes(string typeName)
        {
            Type[] types = null;
            if (typeName == null || typeName == string.Empty)
            {
                if (Module == null)
                    types = Assembly.GetExportedTypes();
                else
                    types = Module.GetTypes();
            }
            else
            {
                if (Module == null)
                {
                    types = new Type[1];
                    types[0] = Assembly.GetType(typeName);
                }
                else
                {
                    TypeFilter myFilter = ClassFilter;
                    types = Module.FindTypes(myFilter, typeName);
                }
            }
            return types;
        }

        public override FFIObjectMarshaler GetMarshaler(ProtoCore.RuntimeCore runtimeCore)
        {
            return CLRObjectMarshaler.GetInstance(runtimeCore);
        }
    }

    /// <summary>
    /// Helper class to load CLR modules.
    /// </summary>
    public class CSModuleHelper : ModuleHelper
    {
        readonly Dictionary<String, CLRDLLModule> mModules = new Dictionary<string, CLRDLLModule>(StringComparer.CurrentCultureIgnoreCase);

        /// <summary>
        /// Returns a CLRDLLModule after loading the given assembly.
        /// </summary>
        /// <param name="name">Name of assembly.</param>
        /// <returns>CLRDLLModule for given assembly/module name.</returns>
        public override DLLModule getModule(String name)
        {
            CLRDLLModule module = null;
            if (!mModules.TryGetValue(name, out module))
            {
                //see if it is a c# dll or native dll and create correct appropriate module and then query the module for function pointers.
                string filename = System.IO.Path.GetFileName(name);

                try
                {
                    Assembly theAssembly = FFIExecutionManager.Instance.LoadAssembly(name);
                    Module testDll = theAssembly.GetModule(filename);
                    if (testDll == null)
                        module = new CLRDLLModule(filename, theAssembly);
                    else
                        module = new CLRDLLModule(filename, testDll);
                    lock (mModules)
                    {
                        mModules.Add(name, module);
                    }
                }
                catch (BadImageFormatException exception)
                {
                    //This probably wasn't a .NET dll
                    System.Diagnostics.Debug.WriteLine(exception.Message);
                    System.Diagnostics.Debug.WriteLine(exception.StackTrace);
                    throw new System.Exception(string.Format("Dynamo can only import .NET DLLs. Failed to load library: {0}.", name));
                }

                catch (System.Exception exception)
                {
                    // If the exception is having HRESULT of 0x80131515, then perhaps we need to instruct the user to "unblock" the downloaded DLL. Please seee the following link for details:
                    // http://blogs.msdn.com/b/brada/archive/2009/12/11/visual-studio-project-sample-loading-error-assembly-could-not-be-loaded-and-will-be-ignored-could-not-load-file-or-assembly-or-one-of-its-dependencies-operation-is-not-supported-exception-from-hresult-0x80131515.aspx
                    // 
                    System.Diagnostics.Debug.WriteLine(exception.Message);
                    System.Diagnostics.Debug.WriteLine(exception.StackTrace);
                    throw new System.Exception(string.Format("Fail to load library: {0}.", name));
                }
            }

            return module;
        }

        public override FFIObjectMarshaler GetMarshaler(ProtoCore.RuntimeCore runtimeCore)
        {
            return CLRObjectMarshaler.GetInstance(runtimeCore);
        }
    }

    /// <summary>
    ///     It keeps FFI class's attributes.
    /// </summary>
    public class FFIClassAttributes : ClassAttributes
    {
        private Attribute[] attributes;
        /// <summary>
        /// FFI class attributes.
        /// </summary>
        public IEnumerable<Attribute> Attributes
        {
            get { return attributes; }
        }

        public FFIClassAttributes(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            // Hide all interfaces from library and search
            if (type.IsInterface) HiddenInLibrary = true;

            attributes = type.GetCustomAttributes(false).Cast<Attribute>().ToArray();
            foreach (var attr in attributes)
            {
                if (attr.HiddenInDynamoLibrary())
                {
                    HiddenInLibrary = true;
                }
                else if (attr is ObsoleteAttribute)
                {
                    HiddenInLibrary = true;
                    ObsoleteMessage = (attr as ObsoleteAttribute).Message;
                    if (string.IsNullOrEmpty(ObsoleteMessage))
                        ObsoleteMessage = "Obsolete";
                }
                else if (attr is PreferredShortNameAttribute)
                {
                    PreferredShortName = (attr as PreferredShortNameAttribute).PreferredShortName;
                }
            }
        }
    }

    /// <summary>
    /// It keeps FFI method's attributes.
    /// </summary>
    public class FFIMethodAttributes : ProtoCore.AST.AssociativeAST.MethodAttributes
    {
        private Attribute[] attributes;
        /// <summary>
        /// FFI method attributes.
        /// </summary>
        public IEnumerable<Attribute> Attributes
        {
            get { return attributes; }
        }
        public bool AllowRankReduction { get; protected set; }
        public bool RequireTracing { get; protected set; }

        //Set the MethodAttributes for Enum fields.
        public FFIMethodAttributes(FieldInfo f)
        {
            var atts = f.GetCustomAttributes(false).Cast<Attribute>();

            var parentAtts = f.DeclaringType.GetCustomAttributes(false).Cast<Attribute>();
            var isObsolete = false;
            var hidden = false;
            var message = "";
            foreach(var attr in parentAtts)
            {
                if(attr is ObsoleteAttribute)
                {
                    isObsolete = true;
                    message = (attr as ObsoleteAttribute).Message;
                    if (string.IsNullOrEmpty(message))
                        message = "Obsolete";
                }

                if (attr is IsVisibleInDynamoLibraryAttribute)
                {
                    hidden = !((IsVisibleInDynamoLibraryAttribute)attr).Visible;
                }
            }

            foreach (var attr in atts)
            {
                //Set the obsolete message for enum fields.
                if (attr is ObsoleteAttribute)
                {
                    HiddenInLibrary = true;
                    ObsoleteMessage = (attr as ObsoleteAttribute).Message;
                    if (string.IsNullOrEmpty(ObsoleteMessage))
                        ObsoleteMessage = "Obsolete";
                }
                else if(attr is IsVisibleInDynamoLibraryAttribute)
                {
                    HiddenInLibrary = !((IsVisibleInDynamoLibraryAttribute)attr).Visible;
                }
            }
            if (isObsolete || hidden)
            {
                HiddenInLibrary = true;
                if (isObsolete) ObsoleteMessage = message;
            }
        }

        public FFIMethodAttributes(MethodInfo method, Dictionary<MethodInfo, Attribute[]> getterAttributes)
        {
            if (method == null)
                throw new ArgumentNullException("method");

            FFIClassAttributes baseAttributes = null;
            Type type = method.DeclaringType;
            if (!CLRModuleType.TryGetTypeAttributes(type, out baseAttributes))
            {
                baseAttributes = new FFIClassAttributes(type);
                CLRModuleType.SetTypeAttributes(type, baseAttributes);
            }

            if (null != baseAttributes)
            {
                HiddenInLibrary = baseAttributes.HiddenInLibrary;
            }

            Attribute[] atts = null;
            if (getterAttributes.TryGetValue(method, out atts))
            {
                attributes = atts;
            }
            else
            {   
                attributes = method.GetCustomAttributes(false).Cast<Attribute>().ToArray();
            }
            
            foreach (var attr in attributes)
            {
                if (attr is AllowRankReductionAttribute)
                {
                    AllowRankReduction = true;
                }
                else if (attr is RuntimeRequirementAttribute)
                {
                    RequireTracing = (attr as RuntimeRequirementAttribute).RequireTracing;
                }
                else if (attr is MultiReturnAttribute)
                {
                    var multiReturnAttr = (attr as MultiReturnAttribute);
                    returnKeys = multiReturnAttr.ReturnKeys.ToList();
                }
                else if(attr.HiddenInDynamoLibrary())
                {
                    HiddenInLibrary = true;
                }
                else if (attr is IsVisibleInDynamoLibraryAttribute)
                {
                    HiddenInLibrary = false;
                }
                else if (attr is IsObsoleteAttribute)
                {
                    HiddenInLibrary = true;
                    ObsoleteMessage = (attr as IsObsoleteAttribute).Message;
                    if (string.IsNullOrEmpty(ObsoleteMessage))
                        ObsoleteMessage = "Obsolete";
                }
                else if (attr is ObsoleteAttribute)
                {
                    HiddenInLibrary = true;
                    ObsoleteMessage = (attr as ObsoleteAttribute).Message;
                    if (string.IsNullOrEmpty(ObsoleteMessage))
                        ObsoleteMessage = "Obsolete";
                }
                else if (attr is CanUpdatePeriodicallyAttribute)
                {
                    CanUpdatePeriodically = (attr as CanUpdatePeriodicallyAttribute).CanUpdatePeriodically;
                }
                else if (attr is IsLacingDisabledAttribute)
                {
                    IsLacingDisabled = true; 
                }
                else if (attr is AllowArrayPromotionAttribute)
                {
                    AllowArrayPromotion = (attr as AllowArrayPromotionAttribute).IsAllowed;
                }
            }
        }

    }

    /// <summary>
    /// A parameter's attributes.
    /// </summary>
    public class FFIParamAttributes: ProtoCore.AST.AssociativeAST.ExternalAttributes
    {
        public bool IsArbitraryDimensionArray
        {
            get
            {
                object isArbitraryRank;
                if (TryGetAttribute("ArbitraryDimensionArrayImportAttribute", out isArbitraryRank))
                    return (bool)isArbitraryRank;
                else
                    return false;
            }
        }

        public string DefaultArgumentExpression 
        { 
            get
            {
                object defaultExpression = null;
                if (TryGetAttribute("DefaultArgumentAttribute", out defaultExpression))
                    return defaultExpression as string;
                else
                    return null;
            }
        }

        public FFIParamAttributes(ParameterInfo parameter)
        {
            var attributes = parameter.GetCustomAttributes(false);
            foreach (var attr in attributes)
            {
                if (attr is DefaultArgumentAttribute)
                {
                    string defaultExpression = (attr as DefaultArgumentAttribute).ArgumentExpression;
                    AddAttribute("DefaultArgumentAttribute", defaultExpression);
                }
                else if (attr is ArbitraryDimensionArrayImportAttribute)
                {
                    AddAttribute("ArbitraryDimensionArrayImportAttribute", true);
                }
            }
        }
    }

    static class AttributeUtils
    {
        /// <summary>
        /// Checks if the given attribute is of type SupressImportIntoVMAttribute
        /// </summary>
        /// <param name="attribute">Given attribute</param>
        /// <returns>True if the given attribute is of type SupressImportIntoVMAttribute</returns>
        public static bool SupressImportIntoVM(this Attribute attribute)
        {
            //TODO@Dynamo 3.0 
            //Following code is to fix attribute resolution issue due to
            //presence of DynamoServices.dll in Dynamo Studio folder. The DLL
            //can be removed in 3.0, once we have removed the dlls we can restore
            //following code.
            //return attribute is SupressImportIntoVMAttribute;

            return null != attribute && attribute.GetType().Name == typeof(SupressImportIntoVMAttribute).Name;
        }

        /// <summary>
        /// Checks if the given attribute is of type IsVisibleInDynamoLibraryAttribute
        /// and has Visible property set to false.
        /// </summary>
        /// <param name="attribute">Given attribute</param>
        /// <returns>True if the given attribute is of 
        /// IsVisibleInDynamoLibraryAttribute type and has Visible property false.</returns>
        public static bool HiddenInDynamoLibrary(this Attribute attribute)
        {
            var visibleInLibraryAttr = attribute as IsVisibleInDynamoLibraryAttribute;
            if (visibleInLibraryAttr != null)
            {
                return visibleInLibraryAttr.Visible == false;
            }

            var propInfo = attribute.GetType().GetProperty("Visible");
            return propInfo != null && (bool)propInfo.GetValue(attribute) == false;
        }
    }
}
