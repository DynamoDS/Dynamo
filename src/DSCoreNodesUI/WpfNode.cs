using Dynamo.Controls;

namespace DSCoreNodesUI
{
    /// <summary>
    /// A node that has a custom WPF interface.
    /// </summary>
    interface IWpfNode
    {
        void SetupCustomUIElements(dynNodeView view);
    }
}
