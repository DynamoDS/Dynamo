using System;
using System.Linq;
using CoreNodeModels.Input;
using Dynamo.Graph.Connectors;
using Dynamo.Graph.Nodes.CustomNodes;
using NUnit.Framework;

namespace Dynamo.Tests.GraphTest.Connectors
{
    [TestFixture]
    class ConnectorModelTests : DynamoModelTestBase
    {
        /// <summary>
        /// This test method will execute the  private void Connect(PortModel p) method under several circunstances
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void ConnectorModelConnectTest()
        {
            //Arrange
            var outnode1 = new Output();
            outnode1.Symbol = "out1";

            var outnode2 = new Output();
            outnode2.Symbol = "out2";

            var numberNode = new DoubleInput();
            numberNode.Value = "5";

            //Act
            //It passes as parameters the same out port (start and end the same port) to the Connect(PortModel p) method then it won't create the connection to end.
            var connector1 = new ConnectorModel(numberNode.OutPorts.FirstOrDefault(), numberNode.OutPorts.FirstOrDefault(), Guid.NewGuid());

            //It passes the second parameter (End) the output port then the Connect(PortModel p) method will try to connect an End port and it will fail
            var connector2 = new ConnectorModel(outnode2.InPorts.FirstOrDefault(), numberNode.OutPorts.FirstOrDefault(), Guid.NewGuid());

            //The outnode2.InPorts has already nodes connected then when calling the Connect(PortModel p) method will remove the port connected
            var connector3 = new ConnectorModel(numberNode.OutPorts.FirstOrDefault(), outnode2.InPorts.FirstOrDefault(),Guid.NewGuid());

            //Assert
            //The End will be null due that the connection from Start to End was not created (out port was passed to both parameters)
            Assert.IsNull(connector1.End);

            //The End will be null due that the connection from Start to End was not created (in port was passed to both parameters)
            Assert.IsNull(connector2.End);

            //It will create the connector from Start to End (removing the previous connected InPort).
            Assert.IsNotNull(connector3.Start);
            Assert.IsNotNull(connector3.End);
        }
    }
}
