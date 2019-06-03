using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoCore.DSASM;
using ProtoCore.Exceptions;
using ProtoCore.Properties;
using ProtoCore.Utils;

namespace ProtoCore
{
    public struct Type
    {
        public string Name;
        public int UID;
        public int rank;

        public bool IsIndexable
        {
            get
            {
                return rank > 0 || rank == Constants.kArbitraryRank;
            }
        }

        /// <summary>
        /// Constructor of Type using Short string
        /// </summary>
        /// <param name="displayTypeName"> Serialized Short String</param>
        public Type(string TypeName, int TypeRank)
        {
            UID = Constants.kInvalidIndex;
            rank = TypeRank;
            Name = TypeName;
        }

        /// <summary>
        /// Comment Jun: Initialize a type to the default values
        /// </summary>
        public void Initialize()
        {
            Name = string.Empty;
            UID = Constants.kInvalidIndex;
            rank = DSASM.Constants.kArbitraryRank;
        }

        private string RankString
        {
            get
            {
                if (IsIndexable)
                {
                    return rank == Constants.kArbitraryRank ?
                        "[]..[]" : new StringBuilder().Insert(0, "[]", rank).ToString();
                }
                else
                {
                    return String.Empty;
                }
            }
        }

        public override string ToString()
        {
            string typename = Name;
            if (string.IsNullOrEmpty(typename))
            {
                typename = TypeSystem.GetPrimitTypeName((PrimitiveType)UID);
                if (string.IsNullOrEmpty(typename))
                    typename = DSDefinitions.Keyword.Var;
            }

            return typename + RankString;
        }

        /// <summary>
        /// To its string representation, but using unqualified class class name.
        /// </summary>
        /// <returns></returns>
        public string ToShortString()
        {

            if (!string.IsNullOrEmpty(Name) && Name.Contains("."))
            {
                return Name.Split('.').Last() + RankString; 
            }
            else
            {
                return ToString();
            }
        }

        public bool Equals(Type type)
        {
            return this.Name == type.Name && this.UID == type.UID && this.rank == type.rank;
        }

    }

    public enum PrimitiveType
    {
        InvalidType = -1,
        Null,
        Array,
        Double,
        Integer,
        Bool,
        Char,
        String,
        Var,
        Void,
        Pointer,
        FunctionPointer,
        Return,
        MaxPrimitive
    }

    public class TypeSystem
    {
        public ProtoCore.DSASM.ClassTable classTable { get; private set; }
        public Dictionary<ProtoCore.DSASM.AddressType, int> addressTypeClassMap { get; set; }
        private static Dictionary<PrimitiveType, string> primitiveTypeNames;

        public TypeSystem()
        {
            SetTypeSystem();
            BuildAddressTypeMap();
        }

        public static string GetPrimitTypeName(PrimitiveType type)
        {
            if (type == PrimitiveType.InvalidType || type >= PrimitiveType.MaxPrimitive)
            {
                return null;
            }

            if (null == primitiveTypeNames)
            {
                primitiveTypeNames = new Dictionary<PrimitiveType, string>();
                primitiveTypeNames[PrimitiveType.Array] = DSDefinitions.Keyword.Array;
                primitiveTypeNames[PrimitiveType.Double] = DSDefinitions.Keyword.Double;
                primitiveTypeNames[PrimitiveType.Integer] = DSDefinitions.Keyword.Int;
                primitiveTypeNames[PrimitiveType.Bool] = DSDefinitions.Keyword.Bool;
                primitiveTypeNames[PrimitiveType.Char] = DSDefinitions.Keyword.Char;
                primitiveTypeNames[PrimitiveType.String] = DSDefinitions.Keyword.String;
                primitiveTypeNames[PrimitiveType.Var] = DSDefinitions.Keyword.Var;
                primitiveTypeNames[PrimitiveType.Null] = DSDefinitions.Keyword.Null;
                primitiveTypeNames[PrimitiveType.Void] = DSDefinitions.Keyword.Void;
                primitiveTypeNames[PrimitiveType.Array] = DSDefinitions.Keyword.Array;
                primitiveTypeNames[PrimitiveType.Pointer] = DSDefinitions.Keyword.PointerReserved;
                primitiveTypeNames[PrimitiveType.FunctionPointer] = DSDefinitions.Keyword.FunctionPointer;
                primitiveTypeNames[PrimitiveType.Return] = DSDefinitions.Keyword.Return;
            }
            return primitiveTypeNames[type];
        }

        public void SetClassTable(ProtoCore.DSASM.ClassTable table)
        {
            Validity.Assert(null != table);
            Validity.Assert(0 == table.ClassNodes.Count);

            if (0 != table.ClassNodes.Count)
            {
                return;
            }

            for (int i = 0; i < classTable.ClassNodes.Count; ++i)
            {
                table.Append(classTable.ClassNodes[i]);
            }
            classTable = table;
        }

        public void BuildAddressTypeMap()
        {
            addressTypeClassMap = new Dictionary<DSASM.AddressType, int>();
            addressTypeClassMap.Add(ProtoCore.DSASM.AddressType.Null, (int)PrimitiveType.Null);
            addressTypeClassMap.Add(ProtoCore.DSASM.AddressType.ArrayPointer, (int)PrimitiveType.Array);
            addressTypeClassMap.Add(ProtoCore.DSASM.AddressType.Double, (int)PrimitiveType.Double);
            addressTypeClassMap.Add(ProtoCore.DSASM.AddressType.Char, (int)PrimitiveType.Char);
            addressTypeClassMap.Add(ProtoCore.DSASM.AddressType.String, (int)PrimitiveType.String);
            addressTypeClassMap.Add(ProtoCore.DSASM.AddressType.Int, (int)PrimitiveType.Integer);
            addressTypeClassMap.Add(ProtoCore.DSASM.AddressType.Boolean, (int)PrimitiveType.Bool);
            addressTypeClassMap.Add(ProtoCore.DSASM.AddressType.Pointer, (int)PrimitiveType.Pointer);
            addressTypeClassMap.Add(ProtoCore.DSASM.AddressType.FunctionPointer, (int)PrimitiveType.FunctionPointer);
            addressTypeClassMap.Add(ProtoCore.DSASM.AddressType.DefaultArg, (int)PrimitiveType.Var);
        }


        public void SetTypeSystem()
        {
            Validity.Assert(null == classTable);
            if (null != classTable)
            {
                return;
            }

            classTable = new DSASM.ClassTable();

            classTable.Reserve((int)PrimitiveType.MaxPrimitive);

            ClassNode cnode;

            cnode = new ClassNode { Name = DSDefinitions.Keyword.Array, Rank = 5, TypeSystem = this };
            cnode.ID = (int)PrimitiveType.Array;
            classTable.SetClassNodeAt(cnode, (int)PrimitiveType.Array);

            cnode = new ClassNode { Name = DSDefinitions.Keyword.Double, Rank = 4, TypeSystem = this };
            cnode.ClassAttributes = new AST.AssociativeAST.ClassAttributes("", "num");
            cnode.CoerceTypes.Add((int)PrimitiveType.Bool, (int)ProtoCore.DSASM.ProcedureDistance.CoerceScore);
            cnode.CoerceTypes.Add((int)PrimitiveType.Integer, (int)ProtoCore.DSASM.ProcedureDistance.CoerceDoubleToIntScore);
            cnode.ID = (int)PrimitiveType.Double;
            classTable.SetClassNodeAt(cnode, (int)PrimitiveType.Double);

            cnode = new ClassNode { Name = DSDefinitions.Keyword.Int, Rank = 3, TypeSystem = this };
            cnode.ClassAttributes = new AST.AssociativeAST.ClassAttributes("", "num");
            cnode.CoerceTypes.Add((int)PrimitiveType.Bool, (int)ProtoCore.DSASM.ProcedureDistance.CoerceScore);
            cnode.CoerceTypes.Add((int)PrimitiveType.Double, (int)ProtoCore.DSASM.ProcedureDistance.CoerceIntToDoubleScore);
            cnode.ID = (int)PrimitiveType.Integer;
            classTable.SetClassNodeAt(cnode, (int)PrimitiveType.Integer);

            cnode = new ClassNode { Name = DSDefinitions.Keyword.Bool, Rank = 2, TypeSystem = this };
            cnode.ID = (int)PrimitiveType.Bool;
            cnode.ClassAttributes = new AST.AssociativeAST.ClassAttributes("", "bool");
            classTable.SetClassNodeAt(cnode, (int)PrimitiveType.Bool);

            cnode = new ClassNode { Name = DSDefinitions.Keyword.Char, Rank = 1, TypeSystem = this };
            cnode.CoerceTypes.Add((int)PrimitiveType.Bool, (int)ProtoCore.DSASM.ProcedureDistance.CoerceScore);
            cnode.CoerceTypes.Add((int)PrimitiveType.String, (int)ProtoCore.DSASM.ProcedureDistance.CoerceScore);

            cnode.ID = (int)PrimitiveType.Char;
            classTable.SetClassNodeAt(cnode, (int)PrimitiveType.Char);

            cnode = new ClassNode { Name = DSDefinitions.Keyword.String, Rank = 0, TypeSystem = this };
            cnode.CoerceTypes.Add((int)PrimitiveType.Bool, (int)ProtoCore.DSASM.ProcedureDistance.CoerceScore);
            cnode.ID = (int)PrimitiveType.String;
            cnode.ClassAttributes = new AST.AssociativeAST.ClassAttributes("", "str");
            classTable.SetClassNodeAt(cnode, (int)PrimitiveType.String);

            cnode = new ClassNode { Name = DSDefinitions.Keyword.Var, Rank = 0, TypeSystem = this };
            cnode.ID = (int)PrimitiveType.Var;
            classTable.SetClassNodeAt(cnode, (int)PrimitiveType.Var);

            cnode = new ClassNode { Name = DSDefinitions.Keyword.Null, Rank = 0, TypeSystem = this };
            cnode.CoerceTypes.Add((int)PrimitiveType.Double, (int)ProtoCore.DSASM.ProcedureDistance.CoerceScore);
            cnode.CoerceTypes.Add((int)PrimitiveType.Integer, (int)ProtoCore.DSASM.ProcedureDistance.CoerceScore);
            cnode.CoerceTypes.Add((int)PrimitiveType.Bool, (int)ProtoCore.DSASM.ProcedureDistance.CoerceScore);
            cnode.CoerceTypes.Add((int)PrimitiveType.Char, (int)ProtoCore.DSASM.ProcedureDistance.CoerceScore);
            cnode.CoerceTypes.Add((int)PrimitiveType.String, (int)ProtoCore.DSASM.ProcedureDistance.CoerceScore);
            cnode.ID = (int)PrimitiveType.Null;
            classTable.SetClassNodeAt(cnode, (int)PrimitiveType.Null);

            cnode = new ClassNode { Name = DSDefinitions.Keyword.Void, Rank = 0, TypeSystem = this };
            cnode.ID = (int)PrimitiveType.Void;
            classTable.SetClassNodeAt(cnode, (int)PrimitiveType.Void);

            cnode = new ClassNode { Name = DSDefinitions.Keyword.PointerReserved, Rank = 0, TypeSystem = this };
            cnode.CoerceTypes.Add((int)PrimitiveType.Integer, (int)ProtoCore.DSASM.ProcedureDistance.CoerceScore);
            cnode.ID = (int)PrimitiveType.Pointer;
            classTable.SetClassNodeAt(cnode, (int)PrimitiveType.Pointer);

            cnode = new ClassNode { Name = DSDefinitions.Keyword.FunctionPointer, Rank = 0,TypeSystem = this };
            cnode.CoerceTypes.Add((int)PrimitiveType.Integer, (int)ProtoCore.DSASM.ProcedureDistance.CoerceScore);
            cnode.ID = (int)PrimitiveType.FunctionPointer;
            cnode.ClassAttributes = new AST.AssociativeAST.ClassAttributes("", "func");
            classTable.SetClassNodeAt(cnode, (int)PrimitiveType.FunctionPointer);

            cnode = new ClassNode { Name = DSDefinitions.Keyword.Return, Rank = 0, TypeSystem = this };
            cnode.ID = (int)PrimitiveType.Return;
            classTable.SetClassNodeAt(cnode, (int)PrimitiveType.Return);
        }

        public bool IsHigherRank(int t1, int t2)
        {
            // TODO Jun: Refactor this when we implement operator overloading
            Validity.Assert(null != classTable);
            Validity.Assert(null != classTable.ClassNodes);
            if (t1 == (int)PrimitiveType.InvalidType || t1 >= classTable.ClassNodes.Count)
            {
                return true;
            }
            else if (t2 == (int)PrimitiveType.InvalidType || t2 >= classTable.ClassNodes.Count)
            {
                return false;
            }
            return classTable.ClassNodes[t1].Rank >= classTable.ClassNodes[t2].Rank;
        }

        public static Type BuildPrimitiveTypeObject(PrimitiveType pType, int rank = Constants.kArbitraryRank)
        {
            Type type = new Type();
            type.Name = GetPrimitTypeName(pType);
            type.UID = (int)pType; ;
            type.rank = rank;
            return type;
        }

        //@TODO(Luke): Once the type system has been refactored, get rid of this
        public Type BuildTypeObject(int UID, int rank = Constants.kArbitraryRank)
        {
            Type type = new Type();
            type.Name = GetType(UID);
            type.UID = UID;
            type.rank = rank;
            return type;
        }

        public string GetType(int UID)
        {
            Validity.Assert(null != classTable);
            return classTable.GetTypeName(UID);
        }

        public int GetType(string ident)
        {
            Validity.Assert(null != classTable);
            return classTable.IndexOf(ident);
        }

        public int GetType(StackValue sv)
        {
            int type = (int)Constants.kInvalidIndex;
            if (sv.IsReferenceType)
            {
                type = sv.metaData.type;
            }
            else
            {
                if (!addressTypeClassMap.TryGetValue(sv.optype, out type))
                {
                    type = (int)PrimitiveType.InvalidType;
                }
            }
            return type;
        }

        //@TODO: Factor this into the type system

        public static StackValue ClassCoerece(StackValue sv, Type targetType, RuntimeCore runtimeCore)
        {
            //@TODO: Add proper coersion testing here.

            if (targetType.UID == (int)PrimitiveType.Bool)
                return StackValue.BuildBoolean(true);

            return sv;
        }

        public static StackValue Coerce(StackValue sv, int UID, int rank, RuntimeCore runtimeCore)
        {
            Type t = new Type();
            t.UID = UID;
            t.rank = rank;

            return Coerce(sv, t, runtimeCore);
        }

        public static StackValue Coerce(StackValue sv, Type targetType, RuntimeCore runtimeCore)
        {
            ProtoCore.Runtime.RuntimeMemory rmem = runtimeCore.RuntimeMemory;
            
            //@TODO(Jun): FIX ME - abort coersion for default args
            if (sv.IsDefaultArgument)
                return sv;

            if (!(
                sv.metaData.type == targetType.UID ||
                (runtimeCore.DSExecutable.classTable.ClassNodes[sv.metaData.type].ConvertibleTo(targetType.UID))
                || sv.IsArray))
            {
                runtimeCore.RuntimeStatus.LogWarning(Runtime.WarningID.ConversionNotPossible, Resources.kConvertNonConvertibleTypes);
                return StackValue.Null;
            }

            //if it's an array
            if (sv.IsArray && !targetType.IsIndexable)
            {
                //This is an array rank reduction
                //this may only be performed in recursion and is illegal here
                string errorMessage = String.Format(Resources.kConvertArrayToNonArray, runtimeCore.DSExecutable.TypeSystem.GetType(targetType.UID));
                runtimeCore.RuntimeStatus.LogWarning(Runtime.WarningID.ConversionNotPossible, errorMessage);
                return StackValue.Null;
            }


            if (sv.IsArray &&
                targetType.IsIndexable)
            {
                Validity.Assert(sv.IsArray);

                //We're being asked to convert an array into an array
                //walk over the structure converting each othe elements

                //Validity.Assert(targetType.rank != -1, "Arbitrary rank array conversion not yet implemented {2EAF557F-62DE-48F0-9BFA-F750BBCDF2CB}");

                //Decrease level of reductions by one
                Type newTargetType = new Type();
                newTargetType.UID = targetType.UID;
                if (targetType.rank != Constants.kArbitraryRank)
                {
                    newTargetType.rank = targetType.rank - 1;
                }
                else
                {
                    if (ArrayUtils.GetMaxRankForArray(sv, runtimeCore) == 1)
                    {
                        //Last unpacking
                        newTargetType.rank = 0;
                    }
                    else
                    {
                        newTargetType.rank = Constants.kArbitraryRank;
                    }
                }

                var array = runtimeCore.Heap.ToHeapObject<DSArray>(sv);
                return array.CopyArray(newTargetType, runtimeCore);
            }

            if (!sv.IsArray && !sv.IsNull &&
                targetType.IsIndexable &&
                targetType.rank != DSASM.Constants.kArbitraryRank)
            {
                //We're being asked to promote the value into an array
                if (targetType.rank == 1)
                {
                    Type newTargetType = new Type();
                    newTargetType.UID = targetType.UID;
                    newTargetType.Name = targetType.Name;
                    newTargetType.rank = 0;

                    //Upcast once
                    StackValue coercedValue = Coerce(sv, newTargetType, runtimeCore);
                    try
                    {
                        StackValue newSv = rmem.Heap.AllocateArray(new StackValue[] { coercedValue });
                        return newSv;
                    }
                    catch (RunOutOfMemoryException)
                    {
                        runtimeCore.RuntimeStatus.LogWarning(Runtime.WarningID.RunOutOfMemory, Resources.RunOutOfMemory);
                        return StackValue.Null;
                    }
                }
                else
                {
                    Validity.Assert(targetType.rank > 1, "Target rank should be greater than one for this clause");

                    Type newTargetType = new Type();
                    newTargetType.UID = targetType.UID;
                    newTargetType.Name = targetType.Name;
                    newTargetType.rank = targetType.rank - 1;

                    //Upcast once
                    StackValue coercedValue = Coerce(sv, newTargetType, runtimeCore);
                    try
                    {
                        StackValue newSv = rmem.Heap.AllocateArray(new StackValue[] { coercedValue });
                        return newSv;
                    }
                    catch (RunOutOfMemoryException)
                    {
                        runtimeCore.RuntimeStatus.LogWarning(Runtime.WarningID.RunOutOfMemory, Resources.RunOutOfMemory);
                        return StackValue.Null;
                    }
                }
            }

            if (sv.IsPointer)
            {
                StackValue ret = ClassCoerece(sv, targetType, runtimeCore);
                return ret;
            }

            //If it's anything other than array, just create a new copy
            switch (targetType.UID)
            {
                case (int)PrimitiveType.InvalidType:
                    runtimeCore.RuntimeStatus.LogWarning(Runtime.WarningID.InvalidType, Resources.kInvalidType);
                    return StackValue.Null;

                case (int)PrimitiveType.Bool:
                    return sv.ToBoolean(runtimeCore);

                case (int)PrimitiveType.Char:
                    {
                        StackValue newSV = sv.ShallowClone();
                        newSV.metaData = new MetaData { type = (int)PrimitiveType.Char };
                        return newSV;
                    }

                case (int)PrimitiveType.Double:
                    return sv.ToDouble();

                case (int)PrimitiveType.FunctionPointer:
                    if (sv.metaData.type != (int)PrimitiveType.FunctionPointer)
                    {
                        runtimeCore.RuntimeStatus.LogWarning(Runtime.WarningID.TypeMismatch, Resources.kFailToConverToFunction);
                        return StackValue.Null;
                    }
                    return sv;

                case (int)PrimitiveType.Integer:
                    {
                        if (sv.metaData.type == (int)PrimitiveType.Double)
                        {
                            //TODO(lukechurch): Once the API is improved (MAGN-5174)
                            //Replace this with a log entry notification
                            //core.RuntimeStatus.LogWarning(RuntimeData.WarningID.kTypeConvertionCauseInfoLoss, Resources.kConvertDoubleToInt);
                        }
                        return sv.ToInteger();
                    }

                case (int)PrimitiveType.Null:
                    {
                        if (sv.metaData.type != (int)PrimitiveType.Null)
                        {
                            runtimeCore.RuntimeStatus.LogWarning(Runtime.WarningID.TypeMismatch, Resources.kFailToConverToNull);
                            return StackValue.Null;
                        }
                        return sv;
                    }

                case (int)PrimitiveType.Pointer:
                    {
                        if (sv.metaData.type != (int)PrimitiveType.Null)
                        {
                            runtimeCore.RuntimeStatus.LogWarning(Runtime.WarningID.TypeMismatch, Resources.kFailToConverToPointer);
                            return StackValue.Null;
                        }
                        return sv;
                    }

                case (int)PrimitiveType.String:
                    {
                        StackValue newSV = sv.ShallowClone();
                        newSV.metaData = new MetaData { type = (int)PrimitiveType.String };
                        if (sv.metaData.type == (int)PrimitiveType.Char)
                        {
                            char ch = Convert.ToChar(newSV.CharValue);
                            newSV = StackValue.BuildString(ch.ToString(), rmem.Heap);
                        }
                        return newSV;
                    }

                case (int)PrimitiveType.Var:
                    {
                        return sv;
                    }

                case (int)PrimitiveType.Array:
                    {
                        var array = runtimeCore.Heap.ToHeapObject<DSArray>(sv);
                        return array.CopyArray(targetType, runtimeCore);
                    }

                default:
                    if (sv.IsNull)
                        return StackValue.Null;
                    else
                        throw new NotImplementedException("Requested coercion not implemented");
            }
        }
    }
}
