using Autodesk.DesignScript.Runtime;
using DSCoreNodesUI;
using Dynamo.Models;

namespace Dynamo.Nodes
{
    public enum TomDickHarry
    {
        Tom,
        Dick,
        Harry
    };

    /// <summary>
    /// This exmple shows how to extend EnumBase to show an Enum as a 
    /// list of values in a drop-down on a node.
    /// </summary>
    [NodeName("Tom Dick and Harry")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("Every tom dick and harry.")]
    [IsDesignScriptCompatible]
    [IsVisibleInDynamoLibrary(false)]
    public class TomDickHarryList : EnumAsString<TomDickHarry> {
        public TomDickHarryList(WorkspaceModel workspace) : base(workspace) { }
    }
}
