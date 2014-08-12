using Dynamo.Controls;

namespace Dynamo.UI
{
    /// <summary>
    /// A node that has a custom WPF interface.
    /// </summary>
    public interface IWpfNode
    {
        /// <summary>
        /// Provides additional initialization to the WPF UI for the node.
        /// </summary>
        /// <param name="view"></param>
        void SetupCustomUIElements(dynNodeView view);
    }
}
