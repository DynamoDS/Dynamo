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
		
        // In which block it is defined?
        public int BlockScope { get; set; }

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
                        cn = runtimeCore.DSExecutable.classTable.ClassNodes[(int)PrimitiveType.Null];
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
                    if (cn != argTypeNode && cn != runtimeCore.DSExecutable.classTable.ClassNodes[(int)PrimitiveType.Null]  && argTypeNode != runtimeCore.DSExecutable.classTable.ClassNodes[(int)PrimitiveType.Var] )
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
            {
                return int.MaxValue;
            }

            if (0 == args.Count)
            {
                return 0;
            }

            int distance = 0;
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

                if (FormalParams[i].IsIndexable != args[i].IsArray) //Replication code will take care of this
                {
                    continue;
                }
                else if (FormalParams[i].IsIndexable)  // both are arrays
                {
                    continue;
                }
                else if (expectedType == rcvdType)
                {
                    continue;
                }
                else if (rcvdType != Constants.kInvalidIndex && expectedType != Constants.kInvalidIndex)
                {
                    int currentCost = ClassUtils.GetUpcastCountTo(
                        classTable.ClassNodes[rcvdType],
                        classTable.ClassNodes[expectedType],
                        runtimeCore);
                    distance += currentCost;
                }
            }
            return distance;
        }

	    /// <summary>
        /// Compute the number of type transforms needed to turn the current type into the target type
        /// Note that this method returns int[] -> char[] as an exact match
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public int ComputeTypeDistance(List<StackValue> args, ProtoCore.DSASM.ClassTable classTable, RuntimeCore runtimeCore, bool allowArrayPromotion = false)
        {
            if (args.Count == 0 && FormalParams.Length == 0)
            {
                return (int)ProcedureDistance.ExactMatchDistance;
            }

            if (args.Count != FormalParams.Length)
            {
                return (int)ProcedureDistance.MaxDistance;
            }

            int distance = (int)ProcedureDistance.MaxDistance;
            // Jun Comment:
            // Default args not provided by the caller would have been pushed by the call instruction as optype = DefaultArs
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
                int currentScore = (int)ProcedureDistance.NotMatchScore;

                //sv rank > param rank
                if (allowArrayPromotion)
                {
                    //stop array -> single
                    if (args[i].IsArray && !FormalParams[i].IsIndexable) //Replication code will take care of this
                    {
                        distance = (int)ProcedureDistance.MaxDistance;
                        break;
                    }
                }
                else
                {
                    //stop array -> single && single -> array
                    if (args[i].IsArray != FormalParams[i].IsIndexable)
                    //Replication code will take care of this
                    {
                        distance = (int)ProcedureDistance.MaxDistance;
                        break;
                    }
                }

                if (FormalParams[i].IsIndexable && (FormalParams[i].IsIndexable == args[i].IsArray))
                {
                    //In case of conversion from double to int, add a conversion score.
                    //There are overloaded methods and the difference is the parameter type between int and double.
                    //Add this to make it call the correct one. - Randy
                    bool bContainsDouble = ArrayUtils.ContainsDoubleElement(args[i], runtimeCore);
                    if (FormalParams[i].UID == (int)PrimitiveType.Integer && bContainsDouble)
                    {
                        currentScore = (int)ProcedureDistance.CoerceDoubleToIntScore;
                    }
                    else if (FormalParams[i].UID == (int)PrimitiveType.Double && !bContainsDouble)
                    {
                        currentScore = (int)ProcedureDistance.CoerceIntToDoubleScore;
                    }
                    else
                    {
                        currentScore = (int)ProcedureDistance.ExactMatchScore;
                    }
                }
                else if (expectedType == rcvdType && (FormalParams[i].IsIndexable == args[i].IsArray))
                {
                    currentScore = (int)ProcedureDistance.ExactMatchScore;
                }
                else if (rcvdType != ProtoCore.DSASM.Constants.kInvalidIndex)
                {
                    currentScore = classTable.ClassNodes[rcvdType].GetCoercionScore(expectedType);
                    if (currentScore == (int)ProcedureDistance.NotMatchScore)
                    {
                        distance = (int)ProcedureDistance.MaxDistance;
                        break;
                    }
                }
                distance -= currentScore;
            }
            return distance;
        }

        public int GetConversionDistance(List<StackValue> reducedParamSVs, ProtoCore.DSASM.ClassTable classTable, bool allowArrayPromotion, RuntimeCore runtimeCore)
        {
            // If the replication strategy allows array promotion, first check for the case
            // where it could be disabled using the [AllowArrayPromotion(false)] attribute
            // and if so set it from the attribute.
            if (allowArrayPromotion)
            {
                var ma = procedureNode.MethodAttribute;
                if (ma != null)
                {
                    allowArrayPromotion = ma.AllowArrayPromotion;
                }
            }
            int dist = ComputeTypeDistance(reducedParamSVs, classTable, runtimeCore, allowArrayPromotion);
            if (dist >= 0 && dist != (int)ProcedureDistance.MaxDistance) //Is it possible to convert to this type?
            {
                if (!FunctionGroup.CheckInvalidArrayCoersion(this, reducedParamSVs, classTable, runtimeCore, allowArrayPromotion))
                    return dist;
            }

            return (int) ProcedureDistance.InvalidDistance;
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
            string name = procedureNode.Name;
            string returnType = procedureNode.ReturnType.ToString();

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

