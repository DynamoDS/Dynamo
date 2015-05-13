using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.Properties;
using ProtoCore.Utils;
using System.Linq;

namespace ProtoCore.DSASM
{
    [System.Diagnostics.DebuggerDisplay("{name}, classId = {classId}")]
    public class ClassNode
    {
        public string name { get; set; }
        public SymbolTable symbols { get; set; }
        public List<AST.AssociativeAST.AssociativeNode> defaultArgExprList { get; set; } 
        public ProcedureTable vtable { get; set; }
        public int size { get; set; }
        public int rank { get; set; }
        public int classId { get; set; }
        public List<int> baseList { get; set; }
        public bool IsImportedClass { get; set; }
        public ClassAttributes ClassAttributes { get; set; }

        /// <summary>
        /// String description of where the classnode was loaded from 
        /// The implementation of the class is stored from here
        /// </summary>
        public string ExternLib { get; set; }

        public TypeSystem typeSystem { get; set; }
        public List<AttributeEntry> Attributes { get; set; }
        // A map of allowed coercions and their respective scores
        public Dictionary<int, int> coerceTypes { get; set; }

        private ProcedureNode disposeMethod;
        private bool hasCachedDisposeMethod;

        public ClassNode()
        {
            IsImportedClass = false;
            name = null;
            size = 0;
            hasCachedDisposeMethod = false;
            disposeMethod = null;
            rank = ProtoCore.DSASM.Constants.kDefaultClassRank;
            symbols = new SymbolTable("classscope", 0);
            defaultArgExprList = new List<AST.AssociativeAST.AssociativeNode>();
            classId = (int)PrimitiveType.kInvalidType;

            // Jun TODO: how significant is runtime index for class procedures?
            int classRuntimProc = ProtoCore.DSASM.Constants.kInvalidIndex;
            vtable = new ProcedureTable(classRuntimProc);
            baseList = new List<int>();
            ExternLib = string.Empty;

            // Set default allowed coerce types
            coerceTypes = new Dictionary<int, int>();
            coerceTypes.Add((int)ProtoCore.PrimitiveType.kTypeVar, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceScore);
            coerceTypes.Add((int)ProtoCore.PrimitiveType.kTypeArray, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceScore);
            coerceTypes.Add((int)ProtoCore.PrimitiveType.kTypeNull, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceScore);
        }

        public ClassNode(ClassNode rhs)
        {
            IsImportedClass = rhs.IsImportedClass;
            name = rhs.name;
            size = rhs.size;
            hasCachedDisposeMethod = rhs.hasCachedDisposeMethod;
            disposeMethod = rhs.disposeMethod;
            rank = rhs.rank;
            symbols = new SymbolTable("classscope", 0);
            if (rhs.symbols != null)
            {
                symbols = new SymbolTable(rhs.symbols.ScopeName, rhs.symbols.RuntimeIndex);
            }
            defaultArgExprList = new List<AST.AssociativeAST.AssociativeNode>();
            classId = rhs.classId;

            int classRuntimProc = ProtoCore.DSASM.Constants.kInvalidIndex;
            vtable = new ProcedureTable(classRuntimProc);
            if (rhs.vtable != null)
            {
                vtable = new ProcedureTable(rhs.vtable);
            }
            baseList = new List<int>(rhs.baseList);
            ExternLib = rhs.ExternLib;
            typeSystem = rhs.typeSystem;
            coerceTypes = new Dictionary<int, int>(rhs.coerceTypes);
        }

        public bool ConvertibleTo(int type)
        {
            Validity.Assert(null != coerceTypes);
            Validity.Assert((int)PrimitiveType.kInvalidType != classId);

            if ((int)PrimitiveType.kTypeNull == classId || coerceTypes.ContainsKey(type))
            { 
                return true;
            }

            //chars are convertible to string

            else if (classId == (int)PrimitiveType.kTypeChar && type==(int)PrimitiveType.kTypeString)
            {
                return true;
            }

            //user defined type to bool
            else if (classId >=(int)PrimitiveType.kMaxPrimitives && type == (int)PrimitiveType.kTypeBool)
            {
                return true;
            }
                
                //string to boolean

            else if (classId == (int)PrimitiveType.kTypeString && type == (int)PrimitiveType.kTypeBool)
            {
                return true;
            }
            //char to boolean
            else if (classId == (int)PrimitiveType.kTypeChar && type == (int)PrimitiveType.kTypeBool)
            {
                return true;
            }
            
            return false;
        }

        public int GetCoercionScore(int type)
        {
            Validity.Assert(null != coerceTypes);
            int score = (int)ProtoCore.DSASM.ProcedureDistance.kNotMatchScore;

            if (type == classId)
                return (int)ProtoCore.DSASM.ProcedureDistance.kExactMatchScore;

            if ((int)PrimitiveType.kTypeNull == classId)
            {
                score = (int)ProtoCore.DSASM.ProcedureDistance.kCoerceScore;
            }
            else
            {
                if (coerceTypes.ContainsKey(type))
                {
                    score = coerceTypes[type];
                }
            }


            return score;
        }

        public bool IsMyBase(int type)
        {
            if ((int)PrimitiveType.kInvalidType == type)
                return false;

            foreach (int baseIndex in baseList)
            {
                Validity.Assert(baseIndex != (int)PrimitiveType.kInvalidType);
                if (type == baseIndex)
                    return true;

                ClassNode baseClassNode = typeSystem.classTable.ClassNodes[baseIndex];
                if (baseClassNode.IsMyBase(type))
                    return true;
            }

            return false;
        }


        public int GetFirstVisibleSymbolNoAccessCheck(string name)
        {
            IEnumerable<SymbolNode> allSymbols = symbols.GetNodeForName(name);
            if (allSymbols == null)
            {
                return Constants.kInvalidIndex;
            }

            // Try for member variables. 
            foreach (var memvar in allSymbols)
            {
                if (memvar.functionIndex == Constants.kGlobalScope)
                {
                    return memvar.symbolTableIndex;
                }
            }
            return Constants.kInvalidIndex;
        }

        // 1. In some class's scope, classScope != kInvalidIndex;
        //    1.1 In the same class scope, return member function directly
        //    1.2 In the derive class scope, return member fucntion != kPrivate
        //    1.3 Return member function whose access == kPublic
        //
        // 2. In global scope, classScope == kInvalidIndex;
        public ProtoCore.DSASM.ProcedureNode GetMemberFunction(string procName, List<ProtoCore.Type> argTypeList, int classScope, out bool isAccessible, out int functionHostClassIndex, bool isStaticOrConstructor = false)
        {
            isAccessible = false;
            functionHostClassIndex = ProtoCore.DSASM.Constants.kInvalidIndex;

            if (vtable == null)
                return null;

            ProtoCore.DSASM.ProcedureNode procNode = null;

            int functionIndex = vtable.IndexOf(procName, argTypeList, isStaticOrConstructor);
            if (functionIndex != ProtoCore.DSASM.Constants.kInvalidIndex)
            {
                int myClassIndex = typeSystem.classTable.IndexOf(name);
                functionHostClassIndex = myClassIndex;
                procNode = vtable.procList[functionIndex];

                if (classScope == ProtoCore.DSASM.Constants.kInvalidIndex)
                {
                    isAccessible = (procNode.access == CompilerDefinitions.AccessModifier.kPublic);
                }
                else if (classScope == myClassIndex) 
                {
                    isAccessible = true;
                }
                else if (typeSystem.classTable.ClassNodes[classScope].IsMyBase(myClassIndex))
                {
                    isAccessible = (procNode.access != CompilerDefinitions.AccessModifier.kPrivate);
                }
                else
                {
                    isAccessible = (procNode.access == CompilerDefinitions.AccessModifier.kPublic);
                }

                return procNode;
            }

            foreach (int baseClassIndex in baseList)
            {
                procNode = typeSystem.classTable.ClassNodes[baseClassIndex].GetMemberFunction(procName, argTypeList, classScope, out isAccessible, out functionHostClassIndex, isStaticOrConstructor);
                if (procNode != null && isAccessible)
                    break;
            }

            return procNode;
        }

        public ProcedureNode GetFirstMemberFunctionBy(string procName)
        {
            if (vtable == null)
            {
                return null;
            }

            ProcedureNode procNode = vtable.GetFunctionsBy(procName).FirstOrDefault();
            if (procNode != null)
            {
                return procNode;
            }

            foreach (int baseClassIndex in baseList)
            {
                var baseClass = typeSystem.classTable.ClassNodes[baseClassIndex];
                procNode = baseClass.GetFirstMemberFunctionBy(procName);
                if (null != procNode)
                {
                    break;
                }
            }
            return procNode;
        }

        public ProcedureNode GetFirstMemberFunctionBy(string procName, int argCount)
        {
            if (vtable == null)            {                return null;            }
            ProcedureNode procNode = vtable.GetFunctionsBy(procName, argCount).FirstOrDefault();
            if (procNode != null)
            {
                return procNode;
            }

            foreach (int baseClassIndex in baseList)
            {
                var baseClass = typeSystem.classTable.ClassNodes[baseClassIndex];
                procNode = baseClass.GetFirstMemberFunctionBy(procName, argCount);
                if (null != procNode)
                {
                    break;
                }
            }
            return procNode;
        }

        public ProcedureNode GetFirstConstructorBy(string procName, int argCount)
        {
            if (vtable == null)
            {
                return null;
            }

            return  vtable.GetFunctionsBy(procName, argCount)
                          .Where(p => p.isConstructor)
                          .FirstOrDefault();
        }

        public ProcedureNode GetFirstStaticFunctionBy(string procName)
        {
            if (vtable == null)
            {
                return null;
            }

            ProcedureNode procNode = vtable.GetFunctionsBy(procName)
                                           .Where(p => p.isStatic)
                                           .FirstOrDefault();
            if (procNode != null)
            {
                return procNode;
            }

            foreach (int baseClassIndex in baseList)
            {
                var baseClass = typeSystem.classTable.ClassNodes[baseClassIndex];
                procNode = baseClass.GetFirstStaticFunctionBy(procName);
                if (null != procNode)
                {
                    break;
                }
            }
            return procNode;
        }

        public ProcedureNode GetFirstStaticFunctionBy(string procName, int argCount)
        {
            if (vtable == null)
            {
                return null;
            }

            ProcedureNode procNode = vtable.GetFunctionsBy(procName, argCount)
                                           .Where(p => p.isStatic)
                                           .FirstOrDefault();
            if (procNode != null)
            {
                return procNode;
            }

            foreach (int baseClassIndex in baseList)
            {
                var baseClass = typeSystem.classTable.ClassNodes[baseClassIndex];
                procNode = baseClass.GetFirstStaticFunctionBy(procName, argCount);
                if (null != procNode)
                {
                    break;
                }
            }
            return procNode;
        }

        private ProcedureNode GetProcNode(string variableName)
        {
            if (vtable == null)
            {
                return null;
            }

            Validity.Assert(null != variableName && variableName.Length > 0);
            string getterName = ProtoCore.DSASM.Constants.kGetterPrefix + variableName;
            int index = vtable.IndexOfFirst(getterName);
            if (ProtoCore.DSASM.Constants.kInvalidIndex == index)
            {
                return null;
            }
            return vtable.procList[index];
        }

        public bool IsStaticMemberVariable(string variableName)
        {
            Validity.Assert(null != variableName && variableName.Length > 0);
            ProcedureNode proc = GetProcNode(variableName);
            Validity.Assert(null != proc);
            return proc.isStatic;
        }

        public bool IsMemberVariable(string variableName)
        {
            // Jun:
            // To find a member variable, get its getter name and check the vtable
            if (vtable == null)
            {
                return false;
            }

            Validity.Assert(null != variableName && variableName.Length > 0);
            string getterName = ProtoCore.DSASM.Constants.kGetterPrefix + variableName;
            return ProtoCore.DSASM.Constants.kInvalidIndex != vtable.IndexOfFirst(getterName) ? true : false;
        }

        public bool IsMemberVariable(SymbolNode symbol)
        {
            // Jun:
            // A fast version of the find member variable where we search the symboltable directly
            Validity.Assert(null != symbol);
            return ProtoCore.DSASM.Constants.kInvalidIndex != symbols.IndexOf(symbol.name)

                // A symbol is a member if it doesnt belong to any function
                && ProtoCore.DSASM.Constants.kInvalidIndex == symbol.functionIndex;
        }

        public ProcedureNode GetDisposeMethod()
        {
            if (!hasCachedDisposeMethod)
            {
                hasCachedDisposeMethod = true;
                if (vtable == null)
                {
                    disposeMethod = null;
                }
                else
                {
                    foreach (ProcedureNode procNode in vtable.procList)
                    {
                        if (CoreUtils.IsDisposeMethod(procNode.name) && procNode.argInfoList.Count == 0)
                        {
                            disposeMethod = procNode;
                            break;
                        }
                    }
                }
            }
            return disposeMethod;
        }
    }



    public class ClassTable
    {
        // Don't directly modify class table list.
        public ReadOnlyCollection<ClassNode> ClassNodes 
        {
            get
            {
                return classNodes.AsReadOnly();
            }
        }

        private List<ClassNode> classNodes = new List<ClassNode>();

        //Symbol table to manage symbols with namespace
        private Namespace.SymbolTable symbolTable = new Namespace.SymbolTable();

        public ClassTable()
        {
        }

        public ClassTable(ClassTable rhs)
        {
            classNodes = new List<ClassNode>();
            for (int n = 0; n < rhs.classNodes.Count; ++n)
            {
                classNodes.Add(new ClassNode(rhs.classNodes[n]));
            }

            symbolTable = new Namespace.SymbolTable(rhs.symbolTable);
        }

        public void Reserve(int size)
        {
            for (int n = 0; n < size; ++n)
            {
                ProtoCore.DSASM.ClassNode cnode = new ProtoCore.DSASM.ClassNode { name = ProtoCore.DSDefinitions.Keyword.Invalid, size = 0, rank = 0, symbols = null, vtable = null };
                classNodes.Add(cnode);
            }
        }

        public int Append(ClassNode node)
        {
            Namespace.Symbol symbol = symbolTable.AddSymbol(node.name);
            if (null == symbol)
            {
                return ProtoCore.DSASM.Constants.kInvalidIndex;
            }

            classNodes.Add(node);
            node.classId = classNodes.Count - 1;
            symbol.Id = node.classId;
            return node.classId;
        }

        public void SetClassNodeAt(ClassNode node, int index)
        {
            classNodes[index] = node;
            Namespace.Symbol symbol = null;
            if (!symbolTable.TryGetExactSymbol(node.name, out symbol))
                symbol = symbolTable.AddSymbol(node.name);

            symbol.Id = index;
        }

        /// <summary>
        /// Find a matching class for given partial class name.
        /// </summary>
        /// <param name="partialName">Partial class name for lookup.</param>
        /// <returns>Class Id if found, else ProtoCore.DSASM.Constants.kInvalidIndex</returns>
        public int IndexOf(string partialName)
        {
            Validity.Assert(null != partialName);

            Namespace.Symbol symbol = null;
            if (symbolTable.TryGetUniqueSymbol(partialName, out symbol))
                return symbol.Id;
            
            return ProtoCore.DSASM.Constants.kInvalidIndex;
        }

        /// <summary>
        /// Gets Class Id for the given fully qualified class name.
        /// </summary>
        /// <param name="fullname">Fully qualified class name</param>
        /// <returns>Class Id if found, else ProtoCore.DSASM.Constants.kInvalidIndex</returns>
        public int GetClassId(string fullname)
        {
            Validity.Assert(null != fullname);

            Namespace.Symbol symbol = null;
            if (symbolTable.TryGetExactSymbol(fullname, out symbol))
                return symbol.Id;

            return ProtoCore.DSASM.Constants.kInvalidIndex;
        }

        /// <summary>
        /// Tries to get the fully qualified name for the given name from this
        /// ClassTable.
        /// </summary>
        /// <param name="name">Partial name of the class for lookup</param>
        /// <param name="fullName">Fully qualified class name</param>
        /// <returns>True if the given name results a unique matching symbol in 
        /// this ClassTable.</returns>
        public bool TryGetFullyQualifiedName(string name, out string fullName)
        {
            Validity.Assert(null != name);
            Namespace.Symbol symbol = null;
            fullName = string.Empty;
            if (symbolTable.TryGetUniqueSymbol(name, out symbol))
                fullName = symbol.FullName;

            return !string.IsNullOrEmpty(fullName);
        }

        /// <summary>
        /// Returns all matching classes for the given name from this ClassTable
        /// </summary>
        /// <param name="name">Partial name of the class for lookup</param>
        /// <returns>Array of fully qualified name of all matching symbols</returns>
        public string[] GetAllMatchingClasses(string name)
        {
            Namespace.Symbol[] symbols = symbolTable.TryGetSymbols(name, (Namespace.Symbol s) => s.Matches(name));
            int size = symbols.Length;
            string[] classes = new string[size];
            for (int i = 0; i < size; ++i)
                classes[i] = symbols[i].FullName;

            return classes;
        }

        public string GetTypeName(int UID)
        {
            if (UID == (int)PrimitiveType.kInvalidType ||
                UID > ClassNodes.Count)
            {
                return null;
            }
            else
            {
                return ClassNodes[UID].name; 
            }
        }

        /// <summary>
        /// Audits the class table for multiple symbol definition.
        /// </summary>
        /// <param name="status">BuildStatus to log the warnings if
        /// multiple symbol found.</param>
        /// /// <param name="guid">Guid of node to which warning corresponds</param>
        public void AuditMultipleDefinition(BuildStatus status, AssociativeGraph.GraphNode graphNode)
        {
            var names = symbolTable.GetAllSymbolNames();
            if (names.Count == symbolTable.GetSymbolCount())
                return;

            foreach (var name in names)
            {
                var symbols = symbolTable.GetAllSymbols(name);
                if (symbols.Count > 1)
                {
                    string message = string.Format(Resources.kMultipleSymbolFound, name, "");
                    foreach (var symbol in symbols)
                    {
                        message += ", " + symbol.FullName;
                    }

                    status.LogWarning(BuildData.WarningID.kMultipleSymbolFound, message, graphNode: graphNode);
                }
            }
        }
    }
}
