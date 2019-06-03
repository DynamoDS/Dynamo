using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.Namespace;
using ProtoCore.Properties;
using ProtoCore.Utils;

namespace ProtoCore.DSASM
{
    [System.Diagnostics.DebuggerDisplay("{Name}, classId = {ID}")]
    public class ClassNode
    {
        public string Name { get; set; }
        public SymbolTable Symbols { get; set; }
        public ProcedureTable ProcTable { get; set; }
        public int Size { get; set; }
        public int Rank { get; set; }
        public int ID { get; set; }
        public int Base { get; set; }
        public bool IsImportedClass { get; set; }
        public ClassAttributes ClassAttributes { get; set; }
        public bool IsStatic { get; set; }
        public bool IsInterface { get; set; }
        public List<int> Interfaces { get; set; }

        /// <summary>
        /// String description of where the classnode was loaded from 
        /// The implementation of the class is stored from here
        /// </summary>
        public string ExternLib { get; set; }

        public TypeSystem TypeSystem { get; set; }
        public List<AttributeEntry> Attributes { get; set; }
        // A map of allowed coercions and their respective scores
        public Dictionary<int, int> CoerceTypes { get; set; }

        // Is the classnode a dummy (placeholder) class
        public bool IsEmpty
        {
            get
            {
                return IsImportedClass && string.IsNullOrEmpty(ExternLib);
            }
        }
        
        private ProcedureNode disposeMethod;
        private bool hasCachedDisposeMethod;

        public ClassNode()
        {
            IsImportedClass = false;
            Name = null;
            Size = 0;
            hasCachedDisposeMethod = false;
            disposeMethod = null;
            Rank = ProtoCore.DSASM.Constants.kDefaultClassRank;
            Symbols = new SymbolTable("classscope", 0);
            ID = (int)PrimitiveType.InvalidType;

            // Jun TODO: how significant is runtime index for class procedures?
            int classRuntimProc = ProtoCore.DSASM.Constants.kInvalidIndex;
            ProcTable = new ProcedureTable(classRuntimProc);
            Base = Constants.kInvalidIndex;
            ExternLib = string.Empty;
            IsInterface = false;
            Interfaces = new List<int>();

            // Set default allowed coerce types
            CoerceTypes = new Dictionary<int, int>();
            CoerceTypes.Add((int)ProtoCore.PrimitiveType.Var, (int)ProtoCore.DSASM.ProcedureDistance.CoerceScore);
            CoerceTypes.Add((int)ProtoCore.PrimitiveType.Array, (int)ProtoCore.DSASM.ProcedureDistance.CoerceScore);
            CoerceTypes.Add((int)ProtoCore.PrimitiveType.Null, (int)ProtoCore.DSASM.ProcedureDistance.CoerceScore);
        }

        public ClassNode(ClassNode rhs)
        {
            IsImportedClass = rhs.IsImportedClass;
            Name = rhs.Name;
            Size = rhs.Size;
            hasCachedDisposeMethod = rhs.hasCachedDisposeMethod;
            disposeMethod = rhs.disposeMethod;
            Rank = rhs.Rank;
            Symbols = new SymbolTable("classscope", 0);
            if (rhs.Symbols != null)
            {
                Symbols = new SymbolTable(rhs.Symbols.ScopeName, rhs.Symbols.RuntimeIndex);
            }
            ID = rhs.ID;

            int classRuntimProc = ProtoCore.DSASM.Constants.kInvalidIndex;
            ProcTable = new ProcedureTable(classRuntimProc);
            if (rhs.ProcTable != null)
            {
                ProcTable = new ProcedureTable(rhs.ProcTable);
            }
            Base = rhs.Base; 
            ExternLib = rhs.ExternLib;
            TypeSystem = rhs.TypeSystem;
            CoerceTypes = new Dictionary<int, int>(rhs.CoerceTypes);
            IsInterface = rhs.IsInterface;
            Interfaces = new List<int>(rhs.Interfaces);
        }

        public bool ConvertibleTo(int type)
        {
            Validity.Assert(null != CoerceTypes);
            Validity.Assert((int)PrimitiveType.InvalidType != ID);

            if ((int)PrimitiveType.Null == ID || CoerceTypes.ContainsKey(type))
            { 
                return true;
            }

            //chars are convertible to string

            else if (ID == (int)PrimitiveType.Char && type==(int)PrimitiveType.String)
            {
                return true;
            }

            //user defined type to bool
            else if (ID >=(int)PrimitiveType.MaxPrimitive && type == (int)PrimitiveType.Bool)
            {
                return true;
            }
                
                //string to boolean

            else if (ID == (int)PrimitiveType.String && type == (int)PrimitiveType.Bool)
            {
                return true;
            }
            //char to boolean
            else if (ID == (int)PrimitiveType.Char && type == (int)PrimitiveType.Bool)
            {
                return true;
            }
            
            return false;
        }

        public int GetCoercionScore(int type)
        {
            Validity.Assert(null != CoerceTypes);
            int score = (int)ProtoCore.DSASM.ProcedureDistance.NotMatchScore;

            if (type == ID)
                return (int)ProtoCore.DSASM.ProcedureDistance.ExactMatchScore;

            if ((int)PrimitiveType.Null == ID)
            {
                score = (int)ProtoCore.DSASM.ProcedureDistance.CoerceScore;
            }
            else
            {
                if (CoerceTypes.ContainsKey(type))
                {
                    score = CoerceTypes[type];
                }
            }


            return score;
        }

        public bool IsMyBase(int type)
        {
            if ((int)PrimitiveType.InvalidType == type)
                return false;

            if (Base != Constants.kInvalidIndex)
            {
                if (type == Base)
                    return true;

                ClassNode baseClassNode = TypeSystem.classTable.ClassNodes[Base];
                if (baseClassNode.IsMyBase(type))
                    return true;
            }

            return false;
        }


        public int GetFirstVisibleSymbolNoAccessCheck(string name)
        {
            IEnumerable<SymbolNode> allSymbols = Symbols.GetNodeForName(name);
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
        public ProcedureNode GetMemberFunction(string procName, List<ProtoCore.Type> argTypeList, int classScope, out bool isAccessible, out int functionHostClassIndex, bool isStaticOrConstructor = false)
        {
            isAccessible = false;
            functionHostClassIndex = Constants.kInvalidIndex;

            if (ProcTable == null)
                return null;

            ProcedureNode procNode = null;

            int functionIndex = ProcTable.IndexOf(procName, argTypeList, isStaticOrConstructor);
            if (functionIndex != Constants.kInvalidIndex)
            {
                int myClassIndex = TypeSystem.classTable.IndexOf(Name);
                functionHostClassIndex = myClassIndex;
                procNode = ProcTable.Procedures[functionIndex];

                if (classScope == Constants.kInvalidIndex)
                {
                    isAccessible = (procNode.AccessModifier == CompilerDefinitions.AccessModifier.Public);
                }
                else if (classScope == myClassIndex) 
                {
                    isAccessible = true;
                }
                else if (TypeSystem.classTable.ClassNodes[classScope].IsMyBase(myClassIndex))
                {
                    isAccessible = (procNode.AccessModifier != CompilerDefinitions.AccessModifier.Private);
                }
                else
                {
                    isAccessible = (procNode.AccessModifier == CompilerDefinitions.AccessModifier.Public);
                }

                return procNode;
            }

            if (Base != Constants.kInvalidIndex)
            {
                procNode = TypeSystem.classTable.ClassNodes[Base].GetMemberFunction(procName, argTypeList, classScope, out isAccessible, out functionHostClassIndex, isStaticOrConstructor);
            }

            return procNode;
        }

        public ProcedureNode GetFirstMemberFunctionBy(string procName)
        {
            if (ProcTable == null)
            {
                return null;
            }

            ProcedureNode procNode = ProcTable.GetFunctionsByName(procName).FirstOrDefault();
            if (procNode != null)
            {
                return procNode;
            }

            if (Base != Constants.kInvalidIndex)
            {
                var baseClass = TypeSystem.classTable.ClassNodes[Base];
                procNode = baseClass.GetFirstMemberFunctionBy(procName);
            }
            return procNode;
        }

        public ProcedureNode GetFirstMemberFunctionBy(string procName, int argCount)
        {
            if (ProcTable == null)            {                return null;            }
            ProcedureNode procNode = ProcTable.GetFunctionsByNameAndArgumentNumber(procName, argCount).FirstOrDefault();
            if (procNode != null)
            {
                return procNode;
            }

            if (Base != Constants.kInvalidIndex)
            {
                var baseClass = TypeSystem.classTable.ClassNodes[Base];
                procNode = baseClass.GetFirstMemberFunctionBy(procName, argCount);
            }
            return procNode;
        }

        public ProcedureNode GetFirstConstructorBy(string procName, int argCount)
        {
            if (ProcTable == null)
            {
                return null;
            }

            return  ProcTable
                          .GetFunctionsByNameAndArgumentNumber(procName, argCount)
                          .FirstOrDefault(p => p.IsConstructor);
        }

        public ProcedureNode GetFirstStaticFunctionBy(string procName)
        {
            if (ProcTable == null)
            {
                return null;
            }

            ProcedureNode procNode = ProcTable.GetFunctionsByName(procName)
                                           .Where(p => p.IsStatic)
                                           .FirstOrDefault();
            if (procNode != null)
            {
                return procNode;
            }

            if (Base != Constants.kInvalidIndex)
            {
                var baseClass = TypeSystem.classTable.ClassNodes[Base];
                procNode = baseClass.GetFirstStaticFunctionBy(procName);
            }
            return procNode;
        }

        public ProcedureNode GetFirstStaticFunctionBy(string procName, int argCount)
        {
            if (ProcTable == null)
            {
                return null;
            }

            ProcedureNode procNode = ProcTable.GetFunctionsByNameAndArgumentNumber(procName, argCount)
                                           .Where(p => p.IsStatic)
                                           .FirstOrDefault();
            if (procNode != null)
            {
                return procNode;
            }

            if (Base != Constants.kInvalidIndex)
            {
                var baseClass = TypeSystem.classTable.ClassNodes[Base];
                procNode = baseClass.GetFirstStaticFunctionBy(procName, argCount);
            }
            return procNode;
        }

        public bool IsMemberVariable(SymbolNode symbol)
        {
            // Jun:
            // A fast version of the find member variable where we search the symboltable directly
            Validity.Assert(null != symbol);
            return ProtoCore.DSASM.Constants.kInvalidIndex != Symbols.IndexOf(symbol.name)

                // A symbol is a member if it doesnt belong to any function
                && ProtoCore.DSASM.Constants.kInvalidIndex == symbol.functionIndex;
        }

        public ProcedureNode GetDisposeMethod()
        {
            if (!hasCachedDisposeMethod)
            {
                hasCachedDisposeMethod = true;
                if (ProcTable == null)
                {
                    disposeMethod = null;
                }
                else
                {
                    foreach (ProcedureNode procNode in ProcTable.Procedures)
                    {
                        if (CoreUtils.IsDisposeMethod(procNode.Name) && procNode.ArgumentInfos.Count == 0)
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
        
        private List<ClassNode> classNodes = new List<ClassNode>();

        //Symbol table to manage symbols with namespace
        private Namespace.SymbolTable symbolTable = new Namespace.SymbolTable();
        
        private List<ClassNode> GetClassHierarchy(ClassNode node)
        {
            var cNodes = new List<ClassNode> {node};
            
            while (node.Base != Constants.kInvalidIndex)
            {
                node = ClassNodes[node.Base];
                cNodes.Add(node);
            }
            return cNodes;
        }

        private ClassNode GetCommonBaseClass(IEnumerable<Symbol> symbols)
        {
            var cNodes = ClassNodes.Where(node => symbols.Any(s => s.Id == node.ID));
            var baseLists = new List<List<ClassNode>>();
            foreach (var classNode in cNodes)
            {
                var baseNodes = GetClassHierarchy(classNode);
                baseLists.Add(baseNodes);
            }
            var arr = baseLists.ToArray();

            if (!arr.Any()) return null;

            var baseClass = ArrayUtils.GetCommonItems(arr);
            return baseClass.FirstOrDefault();
        }

        // Don't directly modify class table list.
        public ReadOnlyCollection<ClassNode> ClassNodes
        {
            get
            {
                return classNodes.AsReadOnly();
            }
        }

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
                ProtoCore.DSASM.ClassNode cnode = new ProtoCore.DSASM.ClassNode { Name = ProtoCore.DSDefinitions.Keyword.Invalid, Size = 0, Rank = 0, Symbols = null, ProcTable = null };
                classNodes.Add(cnode);
            }
        }

        public int Append(ClassNode node)
        {
            Namespace.Symbol symbol = symbolTable.AddSymbol(node.Name);
            if (null == symbol)
            {
                return ProtoCore.DSASM.Constants.kInvalidIndex;
            }

            classNodes.Add(node);
            node.ID = classNodes.Count - 1;
            symbol.Id = node.ID;
            return node.ID;
        }

        public void SetClassNodeAt(ClassNode node, int index)
        {
            classNodes[index] = node;
            Namespace.Symbol symbol = null;
            if (!symbolTable.TryGetExactSymbol(node.Name, out symbol))
                symbol = symbolTable.AddSymbol(node.Name);

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
        /// Returns Class Id for the given fully qualified class name.
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
        /// Returns all matching classes for the given name from this ClassTable.
        /// If the classes have a common base class, this simply returns the given name of the class.
        /// </summary>
        /// <param name="name">Partial name of the class for lookup</param>
        /// <returns>Array of fully qualified name of all matching symbols</returns>
        public string[] GetAllMatchingClasses(string name)
        {
            var symbols = symbolTable.TryGetSymbols(name, s => s.Matches(name));

            var classes = new List<string>();

            if (symbols.Length > 1)
            {
                var baseClass = GetCommonBaseClass(symbols);

                if (baseClass != null)
                {
                    classes.Add(name);
                    return classes.ToArray();
                }
            }
            classes.AddRange(symbols.Select(t => t.FullName));
            return classes.ToArray();
        }

        public string GetTypeName(int UID)
        {
            if (UID == (int)PrimitiveType.InvalidType ||
                UID > ClassNodes.Count)
            {
                return null;
            }
            else
            {
                return ClassNodes[UID].Name; 
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
                    var baseClass = GetCommonBaseClass(symbols);

                    if (baseClass != null)
                    {
                        continue;
                    }

                    string message = string.Format(Resources.kMultipleSymbolFound, name, "");
                    foreach (var symbol in symbols)
                    {
                        message += ", " + symbol.FullName;
                    }

                    status.LogWarning(BuildData.WarningID.MultipleSymbolFound, message, graphNode: graphNode);
                }
            }
        }
    }
}
