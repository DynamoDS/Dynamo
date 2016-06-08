using Dynamo.Session;
namespace Dynamo.Events
{
    public delegate void ExecutionStateHandler(IExecutionSession session);

    /// <summary>
    /// Communication bridge between Dynamo and client libraries to notify
    /// about changes to execution state
    /// </summary>
    public static class ExecutionEvents
    {
        /// <summary>
        /// The graph is about to evaluate
        /// </summary>
        public static event ExecutionStateHandler GraphPreExecution;
        
        /// <summary>
        /// The graph is done evaluating
        /// </summary>
        public static event ExecutionStateHandler GraphPostExecution;

        /// <summary>
        /// Returns active session for the execution, when Graph is executing. 
        /// This property is set to null if graph is not executing.
        /// </summary>
        public static IExecutionSession ActiveSession { get; private set; }

        /// <summary>
        /// Notify observers that the graph is about to evaluate
        /// </summary>
        internal static void OnGraphPreExecution(IExecutionSession session)
        {
            ActiveSession = session;
            if (GraphPreExecution != null)
                GraphPreExecution(session);
        }

        /// <summary>
        /// Notify observers that the graph has evaluated
        /// </summary>
        internal static void OnGraphPostExecution(IExecutionSession session)
        {
            ActiveSession = null;
            if (GraphPostExecution != null)
                GraphPostExecution(session);
        }
    }
}
