namespace Dynamo.Graph.Nodes
{
    /// <summary>
    /// An interface for 3 concrete classes relating to warnings/errors a node is showing.
    /// </summary>
    public interface INodeInformationalState
    {
        /// <summary>
        /// The Info/Warning/Error message being displayed to the user
        /// </summary>
        string Message { get; set; }
    }
}