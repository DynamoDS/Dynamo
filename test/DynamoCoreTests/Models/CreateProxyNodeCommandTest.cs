using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Dynamo.Models;
using Dynamo.Utilities;
using NUnit.Framework;
using static Dynamo.Models.DynamoModel;

namespace Dynamo.Tests.ModelsTest
{
    [TestFixture]
    class CreateProxyNodeCommandTest : DynamoModelTestBase
    {
        /// <summary>
        /// This test method will execute the next methods from the CreateProxyNodeCommand class
        /// public CreateProxyNodeCommand(string nodeId, string customnodeFunctionId,
        ///double x, double y,bool defaultPosition, bool transformCoordinates, string name, int inputs, int outputs)
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void CreateProxyNodeCommand_Constructor()
        {
            //Arrange
            Guid newNodeGuid = Guid.NewGuid();

            //Act
            var cmdOne = new CreateProxyNodeCommand(
                    newNodeGuid.ToString(),
                    "CoreNodeModels.Input.DoubleSlider",
                    100,
                    150,
                    true,
                    true,
                    "TestCommand",
                    1,
                    1);

            //Assert
            Assert.IsNotNull(cmdOne);
            
        }

        /// <summary>
        /// This test method will execute the next methods from the CreateProxyNodeCommand class
        ///static CreateProxyNodeCommand DeserializeCore(XmlElement element)
        /// protected override void SerializeCore(XmlElement element)
        /// NickName
        /// Inputs
        /// Outputs
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void CreateProxyNodeCommand_SerializeDeserializeCore()
        {
            //Arrange
            XmlDocument xmlDocument = new XmlDocument();
            XmlElement elemProxyTest = xmlDocument.CreateElement("TestProxyCommand");

            XmlElement elemTestChild = xmlDocument.CreateElement("TestProxyCommandChild");
            elemProxyTest.AppendChild(elemTestChild);
           
            var helper = new XmlElementHelper(elemProxyTest);
            //DeserializeCore method is checking several attributes, then we need to set them up before calling DeserializeCore
            helper.SetAttribute("NodeName", "ProxyNodeTest");
            helper.SetAttribute("NickName", "ProxyNode");
            helper.SetAttribute("X", 100);
            helper.SetAttribute("Y", 150);
            helper.SetAttribute("DefaultPosition", true);
            helper.SetAttribute("TransformCoordinates", true);
            helper.SetAttribute("Inputs", 1);
            helper.SetAttribute("Outputs", 1);

            //Act
            var proxyNodeCommandNode = CreateProxyNodeCommand.DeserializeCore(elemProxyTest);
            var xmlSerializedCommand = proxyNodeCommandNode.Serialize(xmlDocument);

            //Assert
            Assert.IsNotNull(proxyNodeCommandNode);
            Assert.AreEqual(proxyNodeCommandNode.NickName,"ProxyNode");
            Assert.AreEqual(proxyNodeCommandNode.Inputs, 1);
            Assert.AreEqual(proxyNodeCommandNode.Outputs, 1);
        }
    }
}
