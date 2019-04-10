using System;

namespace DynamoServices
{
    public interface IProfilingData
    {
        TimeSpan LastTotalExecutionTime { get; }
    }
}
