using Dynamo.Scheduler;

namespace Dynamo.Core.Threading
{
    /// <summary>
    /// An entity that creates new schedulers. Typically invoked on the
    /// creation of a new home workspace.
    /// </summary>
    public interface ISchedulerFactory
    {
        DynamoScheduler Build();
    }
}