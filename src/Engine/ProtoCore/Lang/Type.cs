using System;
using System.Collections.Generic;
using System.Diagnostics;
using ProtoCore.BuildData;
using ProtoCore.DSASM;
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
        /// Comment Jun: Initialize a type to the default values
        /// </summary>
        public void Initialize()
        {
            Name = string.Empty;
            UID = Constants.kInvalidIndex;
            rank = DSASM.Constants.kArbitraryRank;
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

            string rankText = string.Empty;
            if (IsIndexable)
            {
                if (rank == DSASM.Constants.kArbitraryRank)
                    rankText = "[]..[]";
                else
                {
                    for (int i = 0; i < rank; i++)
                        rankText += "[]";
                }
            }

            return typename + rankText;
        }

        public bool Equals(Type type)
        {
            return this.Name == type.Name && this.UID == type.UID && this.rank == type.rank;
        }

    }

    public enum PrimitiveType
    {
        kInvalidType = -1,
        kTypeNull,
        kTypeArray,
        kTypeDouble,
        kTypeInt,
        kTypeDynamic,
        kTypeBool,
        kTypeChar,
        kTypeString,
        kTypeVar,
        kTypeVoid,
        kTypeHostEntityID,
        kTypePointer,
        kTypeFunctionPointer,
        kTypeReturn,
        kMaxPrimitives
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
            if (type == PrimitiveType.kInvalidType || type >= PrimitiveType.kMaxPrimitives)
            {
                return null;
            }

            if (null == primitiveTypeNames)
            {
                primitiveTypeNames = new Dictionary<PrimitiveType, string>();
                primitiveTypeNames[PrimitiveType.kTypeArray] = DSDefinitions.Keyword.Array;
                primitiveTypeNames[PrimitiveType.kTypeDouble] = DSDefinitions.Keyword.Double;
                primitiveTypeNames[PrimitiveType.kTypeInt] = DSDefinitions.Keyword.Int;
                primitiveTypeNames[PrimitiveType.kTypeBool] = DSDefinitions.Keyword.Bool;
                primitiveTypeNames[PrimitiveType.kTypeChar] = DSDefinitions.Keyword.Char;
                primitiveTypeNames[PrimitiveType.kTypeString] = DSDefinitions.Keyword.String;
                primitiveTypeNames[PrimitiveType.kTypeVar] = DSDefinitions.Keyword.Var;
                primitiveTypeNames[PrimitiveType.kTypeNull] = DSDefinitions.Keyword.Null;
                primitiveTypeNames[PrimitiveType.kTypeVoid] = DSDefinitions.Keyword.Void;
                primitiveTypeNames[PrimitiveType.kTypeArray] = DSDefinitions.Keyword.Array;
                primitiveTypeNames[PrimitiveType.kTypeHostEntityID] = "hostentityid";
                primitiveTypeNames[PrimitiveType.kTypePointer] = "pointer_reserved";
                primitiveTypeNames[PrimitiveType.kTypeFunctionPointer] = DSDefinitions.Keyword.FunctionPointer;
                primitiveTypeNames[PrimitiveType.kTypeReturn] = "return_reserved";
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
            addressTypeClassMap.Add(ProtoCore.DSASM.AddressType.Null, (int)PrimitiveType.kTypeNull);
            addressTypeClassMap.Add(ProtoCore.DSASM.AddressType.ArrayPointer, (int)PrimitiveType.kTypeArray);
            addressTypeClassMap.Add(ProtoCore.DSASM.AddressType.Double, (int)PrimitiveType.kTypeDouble);
            addressTypeClassMap.Add(ProtoCore.DSASM.AddressType.Char, (int)PrimitiveType.kTypeChar);
            addressTypeClassMap.Add(ProtoCore.DSASM.AddressType.String, (int)PrimitiveType.kTypeString);
            addressTypeClassMap.Add(ProtoCore.DSASM.AddressType.Int, (int)PrimitiveType.kTypeInt);
            addressTypeClassMap.Add(ProtoCore.DSASM.AddressType.Boolean, (int)PrimitiveType.kTypeBool);
            addressTypeClassMap.Add(ProtoCore.DSASM.AddressType.Pointer, (int)PrimitiveType.kTypePointer);
            addressTypeClassMap.Add(ProtoCore.DSASM.AddressType.FunctionPointer, (int)PrimitiveType.kTypeFunctionPointer);
            addressTypeClassMap.Add(ProtoCore.DSASM.AddressType.DefaultArg, (int)PrimitiveType.kTypeVar);
        }


        public void SetTypeSystem()
        {
            Validity.Assert(null == classTable);
            if (null != classTable)
            {
                return;
            }

            classTable = new DSASM.ClassTable();

            classTable.Reserve((int)PrimitiveType.kMaxPrimitives);

            ClassNode cnode;

            cnode = new ClassNode { name = DSDefinitions.Keyword.Array, size = 0, rank = 5, symbols = null, vtable = null, typeSystem = this };
            /*cnode.coerceTypes.Add((int)PrimitiveType.kTypeDouble, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceScore);
            cnode.coerceTypes.Add((int)PrimitiveType.kTypeInt, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceScore);
            cnode.coerceTypes.Add((int)PrimitiveType.kTypeBool, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceScore);
            cnode.coerceTypes.Add((int)PrimitiveType.kTypeChar, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceScore);
            cnode.coerceTypes.Add((int)PrimitiveType.kTypeString, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceScore);
            */
            cnode.classId = (int)PrimitiveType.kTypeArray;
            classTable.SetClassNodeAt(cnode, (int)PrimitiveType.kTypeArray);

            //
            //
            cnode = new ClassNode { name = DSDefinitions.Keyword.Double, size = 0, rank = 4, symbols = null, vtable = null, typeSystem = this };
            cnode.coerceTypes.Add((int)PrimitiveType.kTypeBool, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceScore);
            cnode.coerceTypes.Add((int)PrimitiveType.kTypeInt, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceDoubleToIntScore);
            cnode.classId = (int)PrimitiveType.kTypeDouble;
            classTable.SetClassNodeAt(cnode, (int)PrimitiveType.kTypeDouble);

            //
            //
            cnode = new ClassNode { name = DSDefinitions.Keyword.Int, size = 0, rank = 3, symbols = null, vtable = null, typeSystem = this };
            cnode.coerceTypes.Add((int)PrimitiveType.kTypeBool, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceScore);
            cnode.coerceTypes.Add((int)PrimitiveType.kTypeDouble, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceIntToDoubleScore);
            cnode.classId = (int)PrimitiveType.kTypeInt;
            classTable.SetClassNodeAt(cnode, (int)PrimitiveType.kTypeInt);

            //
            //
            cnode = new ClassNode { name = DSDefinitions.Keyword.Bool, size = 0, rank = 2, symbols = null, vtable = null, typeSystem = this };
            // if convert operator to method call, without the following statement
            // a = true + 1 will fail, because _add expects two integers 
            //cnode.coerceTypes.Add((int)PrimitiveType.kTypeInt, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceScore);
            cnode.classId = (int)PrimitiveType.kTypeBool;
            classTable.SetClassNodeAt(cnode, (int)PrimitiveType.kTypeBool);

            //
            //
            cnode = new ClassNode { name = DSDefinitions.Keyword.Char, size = 0, rank = 1, symbols = null, vtable = null, typeSystem = this };
            cnode.coerceTypes.Add((int)PrimitiveType.kTypeBool, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceScore);
            cnode.coerceTypes.Add((int)PrimitiveType.kTypeString, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceScore);

            cnode.classId = (int)PrimitiveType.kTypeChar;
            classTable.SetClassNodeAt(cnode, (int)PrimitiveType.kTypeChar);

            //
            //
            cnode = new ClassNode { name = DSDefinitions.Keyword.String, size = 0, rank = 0, symbols = null, vtable = null, typeSystem = this };
            cnode.coerceTypes.Add((int)PrimitiveType.kTypeBool, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceScore);
            cnode.classId = (int)PrimitiveType.kTypeString;
            classTable.SetClassNodeAt(cnode, (int)PrimitiveType.kTypeString);

            //
            //
            cnode = new ClassNode { name = DSDefinitions.Keyword.Var, size = 0, rank = 0, symbols = null, vtable = null, typeSystem = this };
            /*cnode.coerceTypes.Add((int)PrimitiveType.kTypeDouble, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceScore);
            cnode.coerceTypes.Add((int)PrimitiveType.kTypeInt, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceScore);
            cnode.coerceTypes.Add((int)PrimitiveType.kTypeBool, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceScore);
            cnode.coerceTypes.Add((int)PrimitiveType.kTypeChar, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceScore);
            cnode.coerceTypes.Add((int)PrimitiveType.kTypeString, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceScore);*/
            cnode.classId = (int)PrimitiveType.kTypeVar;
            classTable.SetClassNodeAt(cnode, (int)PrimitiveType.kTypeVar);

            //
            //
            cnode = new ClassNode { name = DSDefinitions.Keyword.Null, size = 0, rank = 0, symbols = null, vtable = null, typeSystem = this };
            cnode.coerceTypes.Add((int)PrimitiveType.kTypeDouble, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceScore);
            cnode.coerceTypes.Add((int)PrimitiveType.kTypeInt, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceScore);
            cnode.coerceTypes.Add((int)PrimitiveType.kTypeBool, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceScore);
            cnode.coerceTypes.Add((int)PrimitiveType.kTypeChar, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceScore);
            cnode.coerceTypes.Add((int)PrimitiveType.kTypeString, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceScore);
            cnode.classId = (int)PrimitiveType.kTypeNull;
            classTable.SetClassNodeAt(cnode, (int)PrimitiveType.kTypeNull);

            //
            //
            cnode = new ClassNode { name = DSDefinitions.Keyword.Void, size = 0, rank = 0, symbols = null, vtable = null, typeSystem = this };
            cnode.classId = (int)PrimitiveType.kTypeVoid;
            classTable.SetClassNodeAt(cnode, (int)PrimitiveType.kTypeVoid);

            //
            //
            cnode = new ClassNode { name = "hostentityid", size = 0, rank = 0, symbols = null, vtable = null, typeSystem = this };
            cnode.classId = (int)PrimitiveType.kTypeHostEntityID;
            classTable.SetClassNodeAt(cnode, (int)PrimitiveType.kTypeHostEntityID);
            //
            //
            cnode = new ClassNode { name = "pointer_reserved", size = 0, rank = 0, symbols = null, vtable = null, typeSystem = this };
            // if convert operator to method call, without the following statement, 
            // a = b.c + d.e will fail, b.c and d.e are resolved as pointer and _add method requires two integer
            cnode.coerceTypes.Add((int)PrimitiveType.kTypeInt, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceScore);
            cnode.classId = (int)PrimitiveType.kTypePointer;
            classTable.SetClassNodeAt(cnode, (int)PrimitiveType.kTypePointer);
            //
            //
            cnode = new ClassNode { name = DSDefinitions.Keyword.FunctionPointer, size = 0, rank = 0, symbols = null, vtable = null, typeSystem = this };
            cnode.coerceTypes.Add((int)PrimitiveType.kTypeInt, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceScore);
            cnode.classId = (int)PrimitiveType.kTypeFunctionPointer;
            classTable.SetClassNodeAt(cnode, (int)PrimitiveType.kTypeFunctionPointer);

            //
            //
            cnode = new ClassNode { name = "return_reserved", size = 0, rank = 0, symbols = null, vtable = null, typeSystem = this };
            cnode.classId = (int)PrimitiveType.kTypeReturn;
            classTable.SetClassNodeAt(cnode, (int)PrimitiveType.kTypeReturn);
        }

        public bool IsHigherRank(int t1, int t2)
        {
            // TODO Jun: Refactor this when we implement operator overloading
            Validity.Assert(null != classTable);
            Validity.Assert(null != classTable.ClassNodes);
            if (t1 == (int)PrimitiveType.kInvalidType || t1 >= classTable.ClassNodes.Count)
            {
                return true;
            }
            else if (t2 == (int)PrimitiveType.kInvalidType || t2 >= classTable.ClassNodes.Count)
            {
                return false;
            }
            return classTable.ClassNodes[t1].rank >= classTable.ClassNodes[t2].rank;
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

        public string GetType(Type type)
        {
            Validity.Assert(null != classTable);
            return classTable.GetTypeName(type.UID);
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
                    type = (int)PrimitiveType.kInvalidType;
                }
            }
            return type;
        }

        public static bool IsConvertibleTo(int fromType, int toType, Core core)
        {
            if (Constants.kInvalidIndex != fromType && Constants.kInvalidIndex != toType)
            {
                if (fromType == toType)
                {
                    return true;
                }

                return core.ClassTable.ClassNodes[fromType].ConvertibleTo(toType);
            }
            return false;
        }

        //@TODO: Factor this into the type system

        public static StackValue ClassCoerece(StackValue sv, Type targetType, Core core)
        {
            //@TODO: Add proper coersion testing here.

            if (targetType.UID == (int)PrimitiveType.kTypeBool)
                return StackValue.BuildBoolean(true);

            return sv;
        }

        public static StackValue Coerce(StackValue sv, int UID, int rank, Core core)
        {
            Type t = new Type();
            t.UID = UID;
            t.rank = rank;

            return Coerce(sv, t, core);
        }

        public static StackValue Coerce(StackValue sv, Type targetType, Core core)
        {
            //@TODO(Jun): FIX ME - abort coersion for default args
            if (sv.IsDefaultArgument)
                return sv;

            if (!(
                sv.metaData.type == targetType.UID ||
                (core.ClassTable.ClassNodes[sv.metaData.type].ConvertibleTo(targetType.UID))
                || sv.IsArray))
            {
                core.RuntimeStatus.LogWarning(RuntimeData.WarningID.kConversionNotPossible, ProtoCore.StringConstants.kConvertNonConvertibleTypes);
                return StackValue.Null;
            }

            //if it's an array
            if (sv.IsArray && !targetType.IsIndexable)
            {
                //This is an array rank reduction
                //this may only be performed in recursion and is illegal here
                string errorMessage = String.Format(ProtoCore.StringConstants.kConvertArrayToNonArray, core.TypeSystem.GetType(targetType.UID));
                core.RuntimeStatus.LogWarning(RuntimeData.WarningID.kConversionNotPossible, errorMessage);
                return StackValue.Null;
            }


            if (sv.IsArray &&
                targetType.IsIndexable)
            {
                Validity.Assert(sv.IsArray);

                //We're being asked to convert an array into an array
                //walk over the structure converting each othe elements

                var hpe = core.Heap.GetHeapElement(sv);
#if GC_REFERENCE_COUNTING
                var isTemporary = hpe.Active && hpe.Refcount == 0;
#else
                var isTemporary = false;
#endif

                if (targetType.UID == (int)PrimitiveType.kTypeVar && targetType.rank == DSASM.Constants.kArbitraryRank && isTemporary)
                {
                    return sv;
                }

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
                    if (ArrayUtils.GetMaxRankForArray(sv, core) == 1)
                    {
                        //Last unpacking
                        newTargetType.rank = 0;
                    }
                    else
                    {
                        newTargetType.rank = Constants.kArbitraryRank;
                    }
                }

                return ArrayUtils.CopyArray(sv, newTargetType, core);
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
                    StackValue coercedValue = Coerce(sv, newTargetType, core);
                    GCUtils.GCRetain(coercedValue, core);
                    StackValue newSv = core.Heap.AllocateArray(new StackValue[] { coercedValue }, null);
                    return newSv;
                }
                else
                {
                    Validity.Assert(targetType.rank > 1, "Target rank should be greater than one for this clause");

                    Type newTargetType = new Type();
                    newTargetType.UID = targetType.UID;
                    newTargetType.Name = targetType.Name;
                    newTargetType.rank = targetType.rank - 1;

                    //Upcast once
                    StackValue coercedValue = Coerce(sv, newTargetType, core);
                    GCUtils.GCRetain(coercedValue, core);
                    StackValue newSv = core.Heap.AllocateArray(new StackValue[] { coercedValue }, null);
                    return newSv;
                }
            }

            if (sv.IsPointer)
            {
                StackValue ret = ClassCoerece(sv, targetType, core);
                return ret;
            }

            //If it's anything other than array, just create a new copy
            switch (targetType.UID)
            {
                case (int)PrimitiveType.kInvalidType:
                    Validity.Assert(false, "Can't convert invalid type");
                    break;

                case (int)PrimitiveType.kTypeBool:
                    return sv.ToBoolean(core);

                case (int)PrimitiveType.kTypeChar:
                    {
                        StackValue newSV = sv.ShallowClone();
                        newSV.metaData = new MetaData { type = (int)PrimitiveType.kTypeChar };
                        return newSV;
                    }

                case (int)PrimitiveType.kTypeDouble:
                    return sv.ToDouble();

                case (int)PrimitiveType.kTypeFunctionPointer:
                    if (sv.metaData.type != (int)PrimitiveType.kTypeFunctionPointer)
                    {
                        core.RuntimeStatus.LogWarning(RuntimeData.WarningID.kTypeMismatch, ProtoCore.StringConstants.kFailToConverToFunction);
                        return StackValue.Null;
                    }
                    return sv;

                case (int)PrimitiveType.kTypeHostEntityID:
                    {
                        StackValue newSV = sv.ShallowClone();
                        newSV.metaData = new MetaData { type = (int)PrimitiveType.kTypeHostEntityID };
                        return newSV;
                    }

                case (int)PrimitiveType.kTypeInt:
                    {
                        if (sv.metaData.type == (int)PrimitiveType.kTypeDouble)
                        {
                            //TODO(lukechurch): Once the API is improved (MAGN-5174)
                            //Replace this with a log entry notification
                            //core.RuntimeStatus.LogWarning(RuntimeData.WarningID.kTypeConvertionCauseInfoLoss, ProtoCore.StringConstants.kConvertDoubleToInt);
                        }
                        return sv.ToInteger();
                    }

                case (int)PrimitiveType.kTypeNull:
                    {
                        if (sv.metaData.type != (int)PrimitiveType.kTypeNull)
                        {
                            core.RuntimeStatus.LogWarning(RuntimeData.WarningID.kTypeMismatch, ProtoCore.StringConstants.kFailToConverToNull);
                            return StackValue.Null;
                        }
                        return sv;
                    }

                case (int)PrimitiveType.kTypePointer:
                    {
                        if (sv.metaData.type != (int)PrimitiveType.kTypeNull)
                        {
                            core.RuntimeStatus.LogWarning(RuntimeData.WarningID.kTypeMismatch, ProtoCore.StringConstants.kFailToConverToPointer);
                            return StackValue.Null;
                        }
                        return sv;
                    }

                case (int)PrimitiveType.kTypeString:
                    {
                        StackValue newSV = sv.ShallowClone();
                        newSV.metaData = new MetaData { type = (int)PrimitiveType.kTypeString };
                        if (sv.metaData.type == (int)PrimitiveType.kTypeChar)
                        {
                            char ch = EncodingUtils.ConvertInt64ToCharacter(newSV.opdata);
                            newSV = StackValue.BuildString(ch.ToString(), core.Heap);
                        }
                        return newSV;
                    }

                case (int)PrimitiveType.kTypeVar:
                    {
                        return sv;
                    }

                case (int)PrimitiveType.kTypeArray:
                    {
                        return ArrayUtils.CopyArray(sv, targetType, core);
                    }

                default:
                    if (sv.IsNull)
                        return StackValue.Null;
                    else
                        throw new NotImplementedException("Requested coercion not implemented");
            }

            throw new NotImplementedException("Requested coercion not implemented");
        }
    }
}
