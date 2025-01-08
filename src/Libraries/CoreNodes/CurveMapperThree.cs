using Dynamo.Graph.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSCore
{
    [IsDesignScriptCompatible]
    [NodeName("CurveMapperThree")]
    [NodeCategory("Math.Graph")]
    [NodeDescription("CurveMapperNodeDescription")]
    [NodeSearchTags("CurveMapperSearchTags")]
    public class CurveMapperThree : NodeModel
    {        
        public CurveMapperThree()
        {
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("x-MinLimit",
                "a")));
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("x-MaxLimit",
                "b")));
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("y-MinLimit",
                "c")));
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("y-MaxLimit",
                "d")));
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("count",
                "e")));
            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("numbers",
                "f")));

            RegisterAllPorts();
        }
    }
}
