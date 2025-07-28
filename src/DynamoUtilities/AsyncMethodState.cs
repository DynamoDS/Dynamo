
namespace DynamoUtilities
{
    /// <summary>
    /// Simple representation of the states of an async method
    /// Used for identifying issues like Dispose called before async method is finished. 
    /// </summary>
    internal enum AsyncMethodState
    {
        NotStarted = 0,// Async method not called yet
        Started,// Async method called but not finished execution (usually set before any awaits)
        Done// Async method has finished execution (all awaits have finished)
    }
}
