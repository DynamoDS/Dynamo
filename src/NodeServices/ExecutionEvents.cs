namespace DynamoServices
{
    public delegate void ExecutionStateHandler();

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
        /// Notify observers that the graph is about to evaluate
        /// </summary>
        public static void OnGraphPreExecution()
        {
            if (GraphPreExecution != null)
                GraphPreExecution();
        }

        /// <summary>
        /// Notify observers that the graph has evaluated
        /// </summary>
        public static void OnGraphPostExecution()
        {
            if (GraphPostExecution != null)
                GraphPostExecution();
        }
    }
}
