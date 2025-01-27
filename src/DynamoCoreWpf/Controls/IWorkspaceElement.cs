using Dynamo.Utilities;

namespace Dynamo.Controls
{
    internal interface IWorkspaceElement
    {
        bool IsVisibleInCanvas { get; set; }
        Rect2D Rect { get; }
    }
}
