using NUnit.Framework;
using System.IO;
using Newtonsoft.Json;

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
                Error = (sender, args) =>
                {
                    args.ErrorContext.Handled = true;
                },
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Objects
            };
            //settings.Converters.Add(new WorkspaceConverter());
            var json = JsonConvert.SerializeObject(CurrentDynamoModel.CurrentWorkspace, settings);
            Assert.IsNotNullOrEmpty(json);
        }

    }
}
