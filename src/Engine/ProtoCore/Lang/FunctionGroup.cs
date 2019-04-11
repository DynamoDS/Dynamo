using System.Collections.Generic;
using System.Linq;
using ProtoCore.DSASM;
using ProtoCore.Lang.Replication;
using ProtoCore.Properties;
using ProtoCore.Utils;

namespace ProtoCore
{
    /// <summary>
    /// A function group is a collection of overloads to the same method
    /// </summary>
	public class FunctionGroup
	{
        public List<FunctionEndPoint> FunctionEndPoints { get; private set; }

        //@TODO this resolution currently only operates over formal parameters, not the return type as well

		public FunctionGroup ()
		{
            FunctionEndPoints = new List<FunctionEndPoint>();
		}

        public FunctionGroup(List<FunctionEndPoint> rhs)
        {
            Validity.Assert(null != rhs);
            Validity.Assert(null == FunctionEndPoints);
            FunctionEndPoints = new List<FunctionEndPoint>(rhs);
        }

        public void CopyVisible(List<FunctionEndPoint> rhs)
        {
            Validity.Assert(null != rhs);
            Validity.Assert(null != FunctionEndPoints);

            foreach (FunctionEndPoint fep in rhs)
            {
                Validity.Assert(null != fep.procedureNode);
                if (fep.procedureNode.AccessModifier != CompilerDefinitions.AccessModifier.Private && !fep.procedureNode.IsConstructor)
                {
                    if (!FunctionEndPoints.Contains(fep))
                    {
                        FunctionEndPoints.Add(fep);
                    }
                }
            }
        }

        public void CopyPublic(List<FunctionEndPoint> rhs)
        {
            Validity.Assert(null != rhs);
            Validity.Assert(null != FunctionEndPoints);

            foreach (FunctionEndPoint fep in rhs)
            {
                Validity.Assert(null != fep.procedureNode);
                if (fep.procedureNode.AccessModifier == CompilerDefinitions.AccessModifier.Public)
                {
                    if (!FunctionEndPoints.Contains(fep))
                    {
                        FunctionEndPoints.Add(fep);
                    }
                }
            }
        }
        /// <summary>
        /// For a given list of formal parameters, get the function end points that resolve
        /// </summary>
        /// <param name="context"></param>
        /// <param name="formalParams"></param>
        /// <param name="replicationInstructions"></param>
        /// <param name="stackFrame"></param>
        /// <param name="core"></param>
        /// <param name="unresolvable">The number of argument sets that couldn't be resolved</param>
        /// <returns></returns>
        public bool CanGetExactMatchStatics(
            Runtime.Context context,
            List<List<StackValue>> reducedFormalParams,
            StackFrame stackFrame,
            RuntimeCore runtimeCore,
            out HashSet<FunctionEndPoint> lookup)
        {
            lookup = new HashSet<FunctionEndPoint>();
            foreach (List<StackValue> formalParamSet in reducedFormalParams)
            {
                List<FunctionEndPoint> feps = GetExactTypeMatches(context, formalParamSet, new List<ReplicationInstruction>(), stackFrame, runtimeCore);
                if (feps.Count == 0)
                {
                    return false;
                }

                foreach (FunctionEndPoint fep in feps)
                {
                    lookup.Add(fep);
                }
             }

            return true;
        }


        /// <summary>
        /// Returns a list of all the function end points that are type compliant, there maybe more than one due to pattern matches
        /// </summary>
        /// <returns></returns>
        public List<FunctionEndPoint> GetExactTypeMatches(ProtoCore.Runtime.Context context,
            List<StackValue> formalParams, List<ReplicationInstruction> replicationInstructions, StackFrame stackFrame, RuntimeCore runtimeCore)
        {
            List<FunctionEndPoint> ret = new List<FunctionEndPoint>();
            List<List<StackValue>> allReducedParamSVs = Replicator.ComputeAllReducedParams(formalParams, replicationInstructions, runtimeCore);
            
            //@TODO(Luke): Need to add type statistics checks to the below if it is an array to stop int[] matching char[]
            
            //Now test the reduced Params over all of the available end points
            StackValue thisptr = stackFrame.ThisPtr;
            bool isInstance = thisptr.IsPointer && thisptr.Pointer!= Constants.kInvalidIndex;
            bool isGlobal = thisptr.IsPointer && thisptr.Pointer == Constants.kInvalidIndex;
                                  
            foreach (FunctionEndPoint fep in FunctionEndPoints) 
            {
                var proc = fep.procedureNode;

                // Member functions are overloaded with thisptr as the first
                // parameter, so if member function replicates on the left hand
                // side, the type matching should only be applied to overloaded
                // member functions, otherwise should only be applied to original
                // member functions.
                if (isInstance && context.IsReplicating != proc.IsAutoGeneratedThisProc)
                {
                    continue;
                }
                else if (isGlobal && !proc.IsConstructor && !proc.IsStatic && proc.ClassID != Constants.kGlobalScope)
                {
                    continue;
                }

                if (allReducedParamSVs.All(ps => fep.DoesTypeDeepMatch(ps, runtimeCore)))
                {

                    ret.Add(fep);
                }
            }

            return ret; 
        }

        /// <summary>
        /// Returns a dictionary of the function end points that are type compatible
        /// with the costs of the associated conversions
        /// </summary>
        /// <param name="context"></param>
        /// <param name="formalParams"></param>
        /// <param name="replicationInstructions"></param>
        /// <returns></returns>
        public Dictionary<FunctionEndPoint, int> GetConversionDistances(Runtime.Context context,
            List<StackValue> formalParams, List<ReplicationInstruction> replicationInstructions, 
            ClassTable classTable, RuntimeCore runtimeCore, bool allowArrayPromotion = false)
        {
            Dictionary<FunctionEndPoint, int> ret = new Dictionary<FunctionEndPoint, int>();

            //@PERF: Consider parallelising this
            List<FunctionEndPoint> feps = FunctionEndPoints;
            List<StackValue> reducedParamSVs = Replicator.EstimateReducedParams(formalParams, replicationInstructions, runtimeCore);

            foreach (FunctionEndPoint fep in feps)
            {
                int distance = fep.GetConversionDistance(reducedParamSVs, classTable, allowArrayPromotion, runtimeCore);
                if (distance != 
                    (int)ProcedureDistance.InvalidDistance)
                    ret.Add(fep, distance);
            }

            return ret;
        }

        /// <summary>
        /// Returns a dictionary of the function end points that are type compatible
        /// with any branch of replicated parameters. 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="formalParams"></param>
        /// <param name="replicationInstructions"></param>
        /// <param name="classTable"></param>
        /// <param name="runtimeCore"></param>
        /// <returns></returns>
        public Dictionary<FunctionEndPoint, int> GetLooseConversionDistances(
            Runtime.Context context,
            List<StackValue> formalParams,
            List<ReplicationInstruction> replicationInstructions,
            ClassTable classTable,
            RuntimeCore runtimeCore)
        {
            Dictionary<FunctionEndPoint, int> ret = new Dictionary<FunctionEndPoint, int>();
            var reducedParams = Replicator.ComputeAllReducedParams(formalParams, replicationInstructions, runtimeCore);

            foreach (FunctionEndPoint fep in FunctionEndPoints)
            {
                foreach (var reducedParam in reducedParams)
                {
                    int distance = fep.GetConversionDistance(reducedParam, classTable, true, runtimeCore);
                    if (distance != (int)ProcedureDistance.InvalidDistance)
                    {
                        ret.Add(fep, distance);
                        break;
                    } 
                }
            }

            return ret;
        }

        public static bool CheckInvalidArrayCoersion(FunctionEndPoint fep, List<StackValue> reducedSVs, ClassTable classTable, RuntimeCore runtimeCore, bool allowArrayPromotion)
        {
            for (int i = 0; i < reducedSVs.Count; i++)
            {
                Type typ = fep.FormalParams[i];
                if (typ.UID == (int)ProtoCore.PrimitiveType.InvalidType)
                    return true;

                if (!typ.IsIndexable)
                    continue; //It wasn't an array param, skip

                //Compute the type of target param
                if (!allowArrayPromotion)
                    Validity.Assert(reducedSVs[i].IsArray, "This should be an array otherwise this shouldn't have passed previous tests");


                if (!allowArrayPromotion)
                {
                    if (typ.rank != ArrayUtils.GetMaxRankForArray(reducedSVs[i], runtimeCore) &&
                        typ.rank != DSASM.Constants.kArbitraryRank)
                        return true; //Invalid co-ercsion
                }
                else
                {
                    if (typ.rank < ArrayUtils.GetMaxRankForArray(reducedSVs[i], runtimeCore) &&
                        typ.rank != DSASM.Constants.kArbitraryRank)
                        return true; //Invalid co-ercsion
                    
                }


                Dictionary<ClassNode, int> arrayTypes = ArrayUtils.GetTypeStatisticsForArray(reducedSVs[i], runtimeCore);

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
                    ClassNode commonBaseType = ArrayUtils.GetGreatestCommonSubclassForArray(reducedSVs[i], runtimeCore);

                    if (commonBaseType == null)
                        throw new ProtoCore.Exceptions.ReplicationCaseNotCurrentlySupported(
                            string.Format(Resources.ArrayWithNotSupported, "{0C644179-14F5-4172-8EF8-A2F3739901B2}"));

                    cn = commonBaseType; //From now on perform tests on the commmon base type
                }

    

                ClassNode argTypeNode = classTable.ClassNodes[typ.UID];

                //cn now represents the class node of the argument
                //argTypeNode represents the class node of the argument



                bool isNotExactTypeMatch = cn != argTypeNode;
                bool argumentsNotNull = cn != runtimeCore.DSExecutable.classTable.ClassNodes[(int) PrimitiveType.Null];
                bool recievingTypeNotAVar = argTypeNode != runtimeCore.DSExecutable.classTable.ClassNodes[(int) PrimitiveType.Var];
                bool isNotConvertible = !cn.ConvertibleTo(typ.UID);
                
                //bool isCalleeVar = cn == core.classTable.list[(int) PrimitiveType.kTypeVar];
                

                //Is it an invalid conversion?
                if (isNotExactTypeMatch && argumentsNotNull && recievingTypeNotAVar && isNotConvertible)// && !isCalleeVar)
                {
                    return true; //It's an invalid coersion

                }


            }

            return false;


        }


        public Dictionary<FunctionEndPoint, int> GetCastDistances(ProtoCore.Runtime.Context context, List<StackValue> formalParams, List<ReplicationInstruction> replicationInstructions, ClassTable classTable, RuntimeCore runtimeCore)
        {
            Dictionary<FunctionEndPoint, int> ret = new Dictionary<FunctionEndPoint, int>();

            //@PERF: Consider parallelising this
            List<FunctionEndPoint> feps = FunctionEndPoints;
            List<StackValue> reducedParamSVs = Replicator.EstimateReducedParams(formalParams, replicationInstructions, runtimeCore);

            foreach (FunctionEndPoint fep in feps)
            {
                int dist = fep.ComputeCastDistance(reducedParamSVs, classTable, runtimeCore);
                ret.Add(fep, dist);
            }

            return ret;
        }


        public override string ToString()
        {
            if (FunctionEndPoints.Count == 0)
                return "FunctionGroup: Empty";
            else
            {

                return "FunctionGroup: " + FunctionEndPoints[0].procedureNode.Name;
            }
        }
    }


}
	
