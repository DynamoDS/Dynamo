using System;

namespace DynamoServices
{
    public class ProfilingData : IProfilingData
    {
        public TimeSpan LastTotalExecutionTime { get; internal set; }

        internal void OutputToConsole()
        {
            Console.WriteLine("This is a test.");
        }
    }
}
