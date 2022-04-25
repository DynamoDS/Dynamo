using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Dynamo.Search.SearchElements;
using DynamoUnits;
using UnitsUI;

namespace TestUINodes
{
    /// <summary>
    /// Class created in order to test protected methods in MeasurementInputBase parent
    /// </summary>
    public class MeasurementInputBaseConcrete : MeasurementInputBase
    {
        public MeasurementInputBaseConcrete() : base()
        {
            Measure = Length.FromDouble(0.0, LengthUnit.FractionalFoot);

            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("double", "double")));
            RegisterAllPorts();
        }

        public void SerializeCore(XmlElement nodeElement, SaveContext context) =>
            base.SerializeCore(nodeElement, context);
        public bool UpdateValueCore(UpdateValueParams updateValueParams) =>
            base.UpdateValueCore(updateValueParams);
    }
}
