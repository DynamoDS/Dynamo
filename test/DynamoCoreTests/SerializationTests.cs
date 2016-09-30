using NUnit.Framework;
using System.IO;
using Newtonsoft.Json;
using System;
using System.Linq;
using CoreNodeModels.Input;
using Dynamo.Graph.Nodes.ZeroTouch;
using System.Collections.Generic;
using Dynamo.Graph.Nodes;
using Dynamo.Serialization;
using Dynamo.Models;

namespace Dynamo.Tests
{
    [TestFixture]
    class SerializationTests : DynamoModelTestBase
    {
        [Test]
        public void SerializationTest()
        {
            var openPath = Path.Combine(TestDirectory, @"core\serialization\serialization.dyn");
            OpenModel(openPath);

            var model = CurrentDynamoModel;
            var originalGuid = model.CurrentWorkspace.Guid;

            var settings = new JsonSerializerSettings
            {
                Error = (sender, args) =>
                {
                    args.ErrorContext.Handled = true;
                    Console.WriteLine(args.ErrorContext.Error);
                },
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                Converters = new List<JsonConverter>{
                    new FunctionDescriptorConverter(CurrentDynamoModel.LibraryServices),
                    new CodeBlockNodeConverter(CurrentDynamoModel.LibraryServices),
                    new ConnectorConverter(),
                    new AnnotationConverter(),
                    new WorkspaceConverter(model.EngineController, model.Scheduler, model.NodeFactory, DynamoModel.IsTestMode, model.DebugSettings.VerboseLogging)
                },
                ReferenceResolverProvider = () => { return new IdReferenceResolver(); }
            };

            var json = JsonConvert.SerializeObject(CurrentDynamoModel.CurrentWorkspace, settings);
            
            Console.WriteLine(json);

            Assert.IsNotNullOrEmpty(json);

            CurrentDynamoModel.Load(json);

            var ws = CurrentDynamoModel.CurrentWorkspace;
            Assert.NotNull(ws);

            Assert.AreEqual(originalGuid, ws.Guid);

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

            Assert.True(ws.Nodes.All(n => n.InPorts.All(p => p.Owner == n)));
            Assert.True(ws.Nodes.All(n => n.OutPorts.All(p => p.Owner == n)));
            Assert.True(ws.Nodes.All(n => n.InPorts.All(p => p.PortType == PortType.Input)));
            Assert.True(ws.Nodes.All(n => n.OutPorts.All(p => p.PortType == PortType.Output)));

            // Set the ws as the current home workspace
            // and try to run it.
            RunCurrentModel();
            Assert.AreEqual(funcNode.CachedValue.Data, 8.0);
        }
    }
}
