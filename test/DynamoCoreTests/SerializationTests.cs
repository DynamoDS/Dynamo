using NUnit.Framework;
using System.IO;
using Newtonsoft.Json;
using Dynamo.Graph.Workspaces;
using System;
using System.Linq;
using CoreNodeModels.Input;
using Dynamo.Graph.Nodes.ZeroTouch;

namespace Dynamo.Tests
{
    [TestFixture]
    class SerializationTests : DynamoModelTestBase
    {
        [Test]
        public void SerializationTest()
        {
            var openPath = Path.Combine(TestDirectory, @"core\input_nodes\NumberNodeAndNumberSlider.dyn");
            OpenModel(openPath);

            var settings = new JsonSerializerSettings
            {
                Error = (sender, args)=>
                {
                    args.ErrorContext.Handled = true;
                    Console.WriteLine(args.ErrorContext.Error);
                },
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Objects,
                Formatting = Formatting.Indented,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            };

            var json = JsonConvert.SerializeObject(CurrentDynamoModel.CurrentWorkspace, settings);
            Console.WriteLine(json);

            Assert.IsNotNullOrEmpty(json);

            var ws = JsonConvert.DeserializeObject<HomeWorkspaceModel>(json, settings);
            Assert.NotNull(ws);
            var doubleNode = ws.Nodes.First(n => n is DoubleInput);
            Assert.AreEqual(1, doubleNode.OutPorts.Count);
            Assert.AreEqual(0, doubleNode.InPorts.Count);

            var sliderNode = ws.Nodes.First(n => n is DoubleSlider);
            Assert.AreEqual(1, sliderNode.OutPorts.Count);
            Assert.AreEqual(0, sliderNode.InPorts.Count);

            var funcNode = ws.Nodes.First(n => n is DSFunction);
            Assert.AreEqual(1, funcNode.OutPorts.Count);
            Assert.AreEqual(2, funcNode.InPorts.Count);

            Assert.AreEqual(2,ws.Connectors.Count());
            
        }
    }
}
