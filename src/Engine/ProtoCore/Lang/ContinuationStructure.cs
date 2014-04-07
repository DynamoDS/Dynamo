using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoCore.DSASM;

namespace ProtoCore.Lang
{
    /// <summary>
    /// Continuation structure holds the data for a partial execution of a replicated callsite
    /// </summary>
    public class ContinuationStructure
    {
        public ContinuationStructure()
        {
            Done = false;
            NextDispatchArgs = new List<StackValue>();
            RunningResult = new List<StackValue>();
            IsFirstCall = true;
        }

        /// <summary>
        /// True iff execution is complete
        /// </summary>
        public bool Done { get; set; }

        /// <summary>
        /// This represents the result. If execution of the callsite is complete this is the retun value, otherwise it is a partial value
        /// </summary>
        public StackValue Result { get; set; }

        /// <summary>
        /// This is the resolved list of arguments that should be used to execute the function end point
        /// </summary>
        public List<StackValue> NextDispatchArgs { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<StackValue> RunningResult { get; set; }

        /// <summary>
        /// This flag indicates whether the current FEP call is a continuation of a replicating call
        /// </summary>
        public bool IsFirstCall { get; set; }

        /// <summary>
        /// These are cached at the first replicating call so that they can be used in GC when the replication is Done
        /// </summary>
        public List<StackValue> InitialDotCallDimensions { get; set; }
        public List<StackValue> InitialArguments { get; set; }

        /// <summary>
        /// The instruction point of the replicating CALLR instruction needs to be cached in order to return to the next instruction at end of replication
        /// </summary>
        public int InitialPC { get; set; }

        /// <summary>
        /// The depth of the replicating member function used in determining thisPtr
        /// </summary>
        public int InitialDepth { get; set; }
    }   
}
