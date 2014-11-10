using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.UI.WebControls;
using System.Xml;

using Dynamo.DSEngine;
using Dynamo.Library;
using Dynamo.Models;

using Autodesk.DesignScript.Runtime;

using Dynamo.Utilities;

using ProtoCore.AST.AssociativeAST;

namespace Dynamo.Nodes
{
    /// <summary>
    /// DesignScript function node. All functions from DesignScript share the
    /// same function node but internally have different procedure.
    /// </summary>
    [NodeName("Function Node"), NodeDescription("DesignScript Builtin Functions"),
     IsInteractive(false), IsVisibleInDynamoLibrary(false), NodeSearchable(false), IsMetaNode]
    public class DSFunction : DSFunctionBase
    {
        public DSFunction() 
            : this(null)
        { }

        public DSFunction(FunctionDescriptor descriptor)
            : base(new ZeroTouchNodeController(descriptor))
        { }
    }
}
