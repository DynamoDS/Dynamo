using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using ProtoCore;
using ProtoCore.Utils;
using ProtoCore.Lang.Replication;

namespace EmitMSIL
{
    public class Replication
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="className">fully qualified name parsed from function call AST</param>
        /// <param name="methodName">parsed from function call AST</param>
        /// <param name="args"></param>
        /// <param name="replicationAttrs"></param>
        /// <returns></returns>
        public static IList ReplicationLogic(string className, string methodName, IList args, string[][] replicationAttrs)
        {
            var modules = ProtoFFI.DLLFFIHandler.Modules.Values.OfType<ProtoFFI.CLRDLLModule>();
            var assemblies = modules.Select(m => m.Assembly ?? (m.Module?.Assembly)).Where(m => m != null);
            MethodInfo mi = null;
            foreach (var asm in assemblies)
            {
                var type = asm.GetType(className);
                if (type == null) continue;

                // There should be a way to get the exact method after matching parameter types for a node
                // using its function descriptor. AST isn't sufficient for parameter type info.
                mi = type.GetMethods(BindingFlags.Public | BindingFlags.Static).Where(
                    m => m.Name == methodName && m.GetParameters().Length == args.Count).FirstOrDefault();

                if(mi == null)
                {
                    mi = type.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(
                    m => m.Name == methodName && m.GetParameters().Length + 1 == args.Count).FirstOrDefault();
                }

                if (mi != null)
                    break;

                //if (method != null)
                //{
                //    argTypes = method.GetParameters().Select(p => p.ParameterType).ToList();
                //    return method.ReturnType;
                //}
            }
            if (mi == null)
            {
                throw new MissingMethodException("No matching method found in loaded assemblies.");
            }

            var reducedArgs = ReduceArgs(args);


            // Construct replicationGuides from replicationAttrs
            var replicationGuides = ConstructRepGuides(replicationAttrs);

            var partialReplicationGuides = PerformRepGuideDemotion(reducedArgs, replicationGuides);

            //Replication Control is an ordered list of the elements that we have to replicate over
            //Ordering implies containment, so element 0 is the outer most forloop, element 1 is nested within it etc.
            //Take the explicit replication guides and build the replication structure
            //Turn the replication guides into a guide -> List args data structure
            var partialInstructions = Replicator.BuildPartialReplicationInstructions(partialReplicationGuides);

            // Testing invoking method without replication
            object result;
            if (mi.IsStatic)
            {
                //Validity.Assert(args.Count == mi.GetParameters().Length);
                result = mi.Invoke(null, reducedArgs.ToArray());
            }
            else
            {
                result = mi.Invoke(reducedArgs[0], reducedArgs.Skip(1).ToArray());
            }

            return new[] { result };

        }

        private static List<object> ReduceArgs(IList args)
        {
            var reducedArgs = new List<object>();
            foreach(var arg in args)
            {
                if (arg is IList argList && argList.Count == 1)
                {
                    reducedArgs.Add(argList[0]);
                }
                else
                {
                    reducedArgs.Add(arg);
                }
            }
            return reducedArgs;
        }

        private static List<List<ReplicationGuide>> ConstructRepGuides(string[][] replicationAttrs)
        {
            var repGuides = new List<List<ReplicationGuide>>();
            foreach(var argGuides in replicationAttrs)
            {
                var argRepGuides = new List<ReplicationGuide>();
                foreach(var guide in argGuides)
                {
                    bool longest = false;
                    int guideNum = 0;
                    if (!string.IsNullOrEmpty(guide))
                    {
                        int len = guide.Length - 1;
                        if (guide[len] == 'L')
                        {
                            longest = true;
                            guideNum = int.Parse(guide.Substring(0, len));
                        }
                        else
                            guideNum = int.Parse(guide);
                    }
                    argRepGuides.Add(new ReplicationGuide(guideNum, longest));
                }
                repGuides.Add(argRepGuides);
            }
            return repGuides;
        }

        /// <summary>
        /// If all the arguments that have rep guides are single values, then strip the rep guides
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="partialReplicationGuides"></param>
        /// <returns></returns>
        private static List<List<ReplicationGuide>> PerformRepGuideDemotion(IList arguments,
            List<List<ReplicationGuide>> providedReplicationGuides)
        {
            if (providedReplicationGuides.Count == 0)
                return providedReplicationGuides;

            //Check if rep guide demotion needed (each time there is a rep guide, the value is a single)
            for (int i = 0; i < arguments.Count; i++)
            {
                if (providedReplicationGuides[i].Count == 0)
                {
                    continue; //Ignore this case
                }

                //We have rep guides
                if (arguments[i] is IList)
                {
                    //Rep guides on array, use guides as provided
                    return providedReplicationGuides;
                }
            }

            //Everwhere where we have replication guides, we have single values
            //drop the guides
            return new List<List<ReplicationGuide>>();
        }

    }
}
