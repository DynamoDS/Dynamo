using System;
using System.Collections.Generic;
using System.Text;
using ProtoCore.DSASM;
using ProtoCore.Lang.Replication;
using ProtoCore.Properties;
using ProtoCore.Utils;

namespace ProtoCore
{
	public abstract class FunctionEndPoint
	{
	    public int ClassOwnerIndex {
            get;
            set;
        }

        public ProtoCore.DSASM.ProcedureNode  procedureNode {
            get;
            set;
        }
		
		public Type[] FormalParams  {
			get;
			set;
		}
		
		public bool FuncHasOverloads { //If the function has overloads and the type distance has changed, we'll have to do a re-resolution
			get;
			internal set;
		}
		
		public bool FuncHasPredicates { //If the function has predicates, we need to retest
			get;
			internal set;
		}

        // In which block it is defined?
        public int BlockScope { get; set; }


        /// <summary>
        /// Tests whether this end point matches the type of the passed arguments
        /// </summary>
        /// <param name="formalParameters">The proposed parameters </param>
        /// <returns></returns>
        public bool DoesTypeMatch(List<StackValue> formalParameters)
        {
            if (formalParameters.Count != FormalParams.Length)
                return false;

            for (int i = 0; i < FormalParams.Length; i++)
            {
                if (FormalParams[i].IsIndexable && formalParameters[i].IsArray)
                    continue;

                if (FormalParams[i].UID != formalParameters[i].metaData.type)
                    return false;
            }
            return true;
        }

        public bool DoesTypeDeepMatch(List<StackValue> formalParameters, RuntimeCore runtimeCore)
        {
            if (formalParameters.Count != FormalParams.Length)
                return false;

            for (int i = 0; i < FormalParams.Length; i++)
            {
                if (FormalParams[i].IsIndexable != formalParameters[i].IsArray)
                    return false;

                if (FormalParams[i].IsIndexable && formalParameters[i].IsArray)
                {
                    if (FormalParams[i].rank != ArrayUtils.GetMaxRankForArray(formalParameters[i], runtimeCore)
                        && FormalParams[i].rank != DSASM.Constants.kArbitraryRank)
                        return false;


                    Type typ = FormalParams[i];
                    Dictionary<ClassNode, int> arrayTypes = ArrayUtils.GetTypeStatisticsForArray(formalParameters[i], runtimeCore);

                    ClassNode cn = null;

                    if (arrayTypes.Count == 0)
                    {
                        //This was an empty array
                        Validity.Assert(cn == null, "If it was an empty array, there shouldn't be a type node");
                        cn = runtimeCore.DSExecutable.classTable.ClassNodes[(int)PrimitiveType.kTypeNull];
                    }
                    else if (arrayTypes.Count == 1)
                    {
                        //UGLY, get the key out of the array types, of which there is only one
                        foreach (ClassNode key in arrayTypes.Keys)
                            cn = key;
                    }
                    else if (arrayTypes.Count > 1)
                    {
                        ClassNode commonBaseType = ArrayUtils.GetGreatestCommonSubclassForArray(formalParameters[i], runtimeCore);

                        if (commonBaseType == null)
                            throw new ProtoCore.Exceptions.ReplicationCaseNotCurrentlySupported(
                                string.Format(Resources.ArrayWithNotSupported, "{0C644179-14F5-4172-8EF8-A2F3739901B2}"));

                        cn = commonBaseType; //From now on perform tests on the commmon base type
                    }


                    ClassNode argTypeNode = runtimeCore.DSExecutable.classTable.ClassNodes[typ.UID];

                    //cn now represents the class node of the argument
                    //argTypeNode represents the class node of the argument

                    //TODO(Jun)This is worrying test

                    //Disable var as exact match, otherwise resolution between double and var will fail
                    if (cn != argTypeNode && cn != runtimeCore.DSExecutable.classTable.ClassNodes[(int)PrimitiveType.kTypeNull]  && argTypeNode != runtimeCore.DSExecutable.classTable.ClassNodes[(int)PrimitiveType.kTypeVar] )
                        return false;

                    //if (coersionScore != (int)ProcedureDistance.kExactMatchScore)
                    //    return false;

                    continue;
                }

                if (FormalParams[i].UID != formalParameters[i].metaData.type)
                    return false;
            }
            return true;
        }


        internal int ComputeCastDistance(List<StackValue> args, ClassTable classTable, RuntimeCore runtimeCore)
        {
            //Compute the cost to migrate a class calls argument types to the coresponding base types
            //This cannot be used to determine whether a function can be called as it will ignore anything that doesn't
            //it should only be used to determine which class is closer

            if (args.Count != FormalParams.Length)
                return int.MaxValue;

            int distance = 0;

            if (0 == args.Count)
            {
                return distance;
            }
            else
            {
                // Check if all the types match the current function at 'n'
                for (int i = 0; i < args.Count; ++i)
                {
                    int rcvdType = args[i].metaData.type;

                    // If its a default argumnet, then it wasnt provided by the caller
                    // The rcvdType is the type of the argument signature
                    if (args[i].IsDefaultArgument)
                    {
                        rcvdType = FormalParams[i].UID;
                    }

                    int expectedType = FormalParams[i].UID;

                    int currentCost = 0;

                    if (FormalParams[i].IsIndexable != args[i].IsArray) //Replication code will take care of this
                    {
                        continue;
                    }
                    else if (FormalParams[i].IsIndexable && (FormalParams[i].IsIndexable == args[i].IsArray))
                    {
                        continue;

                    }
                    else if (expectedType == rcvdType && (FormalParams[i].IsIndexable == args[i].IsArray))
                    {
                        continue;

                    }
                    else if (rcvdType != ProtoCore.DSASM.Constants.kInvalidIndex &&
                        expectedType != ProtoCore.DSASM.Constants.kInvalidIndex)
                    {
                        currentCost += ClassUtils.GetUpcastCountTo(classTable.ClassNodes[rcvdType],
                                                                   classTable.ClassNodes[expectedType], runtimeCore);
                 
                    }
                    distance += currentCost;
                }
                return distance;
            }
        }




	    /// <summary>
        /// Compute the number of type transforms needed to turn the current type into the target type
        /// Note that this method returns int[] -> char[] as an exact match
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public int ComputeTypeDistance(List<StackValue> args, ProtoCore.DSASM.ClassTable classTable, RuntimeCore runtimeCore, bool allowArrayPromotion = false)
        {
            //Modified from proc Table, does not use quite the same arguments
                
            int distance = (int)ProcedureDistance.kMaxDistance;

            if (0 == args.Count && 0 == FormalParams.Length)
            {
                distance = (int)ProcedureDistance.kExactMatchDistance;
            }
            else
            {
                // Jun Comment:
                // Default args not provided by the caller would have been pushed by the call instruction as optype = DefaultArs
                if (FormalParams.Length == args.Count)
                {
                    // Check if all the types match the current function at 'n'
                    for (int i = 0; i < args.Count; ++i)
                    {
                        int rcvdType = args[i].metaData.type;

                        // If its a default argumnet, then it wasnt provided by the caller
                        // The rcvdType is the type of the argument signature
                        if (args[i].IsDefaultArgument)
                        {
                            rcvdType = FormalParams[i].UID; 
                        }

                        int expectedType = FormalParams[i].UID;
                        int currentScore = (int)ProcedureDistance.kNotMatchScore;

                        //Fuqiang: For now disable rank check
                        //if function is expecting array, but non-array or array of lower rank is passed, break.
                        //if (args[i].rank != -1 && args[i].UID != (int)PrimitiveType.kTypeVar && args[i].rank < argTypeList[i].rank)

                        //Only enable type check, and array and non-array check
                       /*  SUSPECTED REDUNDANT Luke,Jun
                        * if (args[i].rank != -1 && args[i].UID != (int)PrimitiveType.kTypeVar && !args[i].IsIndexable && FormalParams[i].IsIndexable)
                        {
                            distance = (int)ProcedureDistance.kMaxDistance;
                            break;
                        }
                        else */

                        //sv rank > param rank

                        if (allowArrayPromotion)
                        {
                            //stop array -> single
                            if (args[i].IsArray && !FormalParams[i].IsIndexable) //Replication code will take care of this
                            {
                                distance = (int)ProcedureDistance.kMaxDistance;
                                break;
                            }
                        }
                        else
                        {
                            //stop array -> single && single -> array
                            if (args[i].IsArray != FormalParams[i].IsIndexable)
                            //Replication code will take care of this
                            {
                                distance = (int)ProcedureDistance.kMaxDistance;
                                break;
                            }
                        }
                        
                        if (FormalParams[i].IsIndexable && (FormalParams[i].IsIndexable == args[i].IsArray))
                        {
                            //In case of conversion from double to int, add a conversion score.
                            //There are overloaded methods and the difference is the parameter type between int and double.
                            //Add this to make it call the correct one. - Randy
                            bool bContainsDouble = ArrayUtils.ContainsDoubleElement(args[i], runtimeCore);
                            if (FormalParams[i].UID == (int)PrimitiveType.kTypeInt && bContainsDouble)
                            {
                                currentScore = (int)ProcedureDistance.kCoerceDoubleToIntScore;
                            }
                            else if (FormalParams[i].UID == (int)PrimitiveType.kTypeDouble && !bContainsDouble)
                            {
                                currentScore = (int)ProcedureDistance.kCoerceIntToDoubleScore;
                            }
                            else
                            {
                                currentScore = (int)ProcedureDistance.kExactMatchScore;
                            }
                        }
                        else if (expectedType == rcvdType && (FormalParams[i].IsIndexable == args[i].IsArray))
                        {
                            currentScore = (int)ProcedureDistance.kExactMatchScore;
                        }
                        else if (rcvdType != ProtoCore.DSASM.Constants.kInvalidIndex)
                        {
                            currentScore = classTable.ClassNodes[rcvdType].GetCoercionScore(expectedType);
                            if (currentScore == (int)ProcedureDistance.kNotMatchScore)
                            {
                                distance = (int)ProcedureDistance.kMaxDistance;
                                break;
                            }
                        }
                        distance -= currentScore;
                    }
                }
            }
            return distance;
        }

        public int GetConversionDistance(List<StackValue> reducedParamSVs, ProtoCore.DSASM.ClassTable classTable, bool allowArrayPromotion, RuntimeCore runtimeCore)
        {

            int dist = ComputeTypeDistance(reducedParamSVs, classTable, runtimeCore, allowArrayPromotion);
            if (dist >= 0 && dist != (int)ProcedureDistance.kMaxDistance) //Is it possible to convert to this type?
            {
                if (!FunctionGroup.CheckInvalidArrayCoersion(this, reducedParamSVs, classTable, runtimeCore, allowArrayPromotion))
                    return dist;
            }

            return (int) ProcedureDistance.kInvalidDistance;
        }

        public abstract bool DoesPredicateMatch(ProtoCore.Runtime.Context c, List<StackValue> formalParameters, List<ReplicationInstruction> replicationInstructions);
        public abstract StackValue Execute(ProtoCore.Runtime.Context c, List<StackValue> formalParameters, ProtoCore.DSASM.StackFrame stackFrame, RuntimeCore runtimeCore);

        /// <summary>
        /// Convert the parameters passed to the types specified in this fep
        /// </summary>
        /// <param name="formalParameters"></param>
        /// <returns></returns>
        public List<StackValue> CoerceParameters(List<StackValue> formalParameters, RuntimeCore runtimeCore)
        {
            List<StackValue> fixedUpVersions = new List<StackValue>();
            for (int i = 0; i < formalParameters.Count; i++)
            {
                StackValue formalParam = formalParameters[i];
                Type targetType = FormalParams[i];

                StackValue coercedParam = TypeSystem.Coerce(formalParam, targetType, runtimeCore);
                fixedUpVersions.Add(coercedParam);
            }

            return fixedUpVersions;
        }


        public override string ToString()
        {
            string name = procedureNode.name;
            string returnType = procedureNode.returntype.ToString();

            System.Text.StringBuilder sb = new StringBuilder();

            sb.Append(name);
            sb.Append("(");

            if (FormalParams.Length > 0)
            {

                for (int i = 0; i < FormalParams.Length - 1; i++)
                {
                    sb.Append(FormalParams[i] + ",");
                }
                sb.Append(FormalParams[FormalParams.Length - 1]);
            }
            sb.Append(")");
            sb.Append("-> ");
            sb.Append(returnType);

            return sb.ToString();
        }
    
    }



}

