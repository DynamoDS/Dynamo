using Dynamo.Graph.Workspaces;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Dynamo.Tests
{
    /// <summary>
    /// Tests for the type declaration swap between the Json.NET "$type" property
    /// and the "ConcreteType" property used in the saved graph format.
    /// </summary>
    [TestFixture]
    class SerializationExtensionsTests
    {
        private const string StringInputType = "CoreNodeModels.Input.StringInput, CoreNodeModels";

        private static string NodeJson(string typeProperty, string inputValue, string description)
        {
            var node = new JObject
            {
                { typeProperty, StringInputType },
                { "NodeType", "StringInputNode" },
                { "InputValue", inputValue },
                { "Description", description }
            };

            return new JObject { { "Nodes", new JArray(node) } }.ToString();
        }

        [Test]
        [Category("UnitTests")]
        public void WhenOpeningGraphThenConcreteTypePropertyBecomesTypeDeclaration()
        {
            var json = NodeJson("ConcreteType", "a string", "Creates a string");

            var result = SerializationExtensions.ReplaceTypeDeclarations(json, true);

            var node = (JObject)JObject.Parse(result)["Nodes"][0];
            Assert.AreEqual(StringInputType, node.Value<string>("$type"));
            Assert.IsNull(node.Property("ConcreteType"));
        }

        [Test]
        [Category("UnitTests")]
        public void WhenSavingGraphThenTypeDeclarationBecomesConcreteTypeProperty()
        {
            var json = NodeJson("$type", "a string", "Creates a string");

            var result = SerializationExtensions.ReplaceTypeDeclarations(json);

            var node = (JObject)JObject.Parse(result)["Nodes"][0];
            Assert.AreEqual(StringInputType, node.Value<string>("ConcreteType"));
            Assert.IsNull(node.Property("$type"));
        }

        [Test]
        [Category("UnitTests")]
        [TestCase("{\"ConcreteType\":\"A, B\",\"InputValue\":\"ConcreteType\"}", "{\"$type\":\"A, B\",\"InputValue\":\"ConcreteType\"}")]
        [TestCase("{\"ConcreteType\" : \"A, B\", \"InputValue\" : \"ConcreteType\"}", "{\"$type\" : \"A, B\", \"InputValue\" : \"ConcreteType\"}")]
        [TestCase("{\"ConcreteType\"\r\n:\"A, B\",\"Values\":[\"ConcreteType\",\"x\"]}", "{\"$type\"\r\n:\"A, B\",\"Values\":[\"ConcreteType\",\"x\"]}")]
        public void WhenJsonFormattingVariesThenOnlyPropertyNamesAreSwapped(string saved, string opened)
        {
            Assert.AreEqual(opened, SerializationExtensions.ReplaceTypeDeclarations(saved, true));
            Assert.AreEqual(saved, SerializationExtensions.ReplaceTypeDeclarations(opened));
        }

        [Test]
        [Category("UnitTests")]
        public void WhenOpeningGraphThenConcreteTypeInStringValuesIsPreserved()
        {
            var inputValue = "ConcreteType";
            var description = "Saved with value of \"ConcreteType\", not ConcreteType: or $type";
            var json = NodeJson("ConcreteType", inputValue, description);

            var result = SerializationExtensions.ReplaceTypeDeclarations(json, true);

            var node = (JObject)JObject.Parse(result)["Nodes"][0];
            Assert.AreEqual(StringInputType, node.Value<string>("$type"));
            Assert.AreEqual(inputValue, node.Value<string>("InputValue"));
            Assert.AreEqual(description, node.Value<string>("Description"));
        }

        [Test]
        [Category("UnitTests")]
        public void WhenSavingGraphThenTypeDeclarationInStringValuesIsPreserved()
        {
            var inputValue = "$type";
            var description = "Saved with value of \"$type\", not $type: or ConcreteType";
            var json = NodeJson("$type", inputValue, description);

            var result = SerializationExtensions.ReplaceTypeDeclarations(json);

            var node = (JObject)JObject.Parse(result)["Nodes"][0];
            Assert.AreEqual(StringInputType, node.Value<string>("ConcreteType"));
            Assert.AreEqual(inputValue, node.Value<string>("InputValue"));
            Assert.AreEqual(description, node.Value<string>("Description"));
        }

        [Test]
        [Category("UnitTests")]
        public void WhenSavingThenOpeningGraphThenStringValuesRoundTrip()
        {
            var inputValue = "ConcreteType and $type";
            var description = "\"ConcreteType\" and \"$type\" and {\"$typeid\":\"a\"}";
            var json = NodeJson("$type", inputValue, description);

            var saved = SerializationExtensions.ReplaceTypeDeclarations(json);
            var opened = SerializationExtensions.ReplaceTypeDeclarations(saved, true);

            Assert.AreEqual(json, opened);
            var node = (JObject)JObject.Parse(opened)["Nodes"][0];
            Assert.AreEqual(StringInputType, node.Value<string>("$type"));
            Assert.AreEqual(inputValue, node.Value<string>("InputValue"));
            Assert.AreEqual(description, node.Value<string>("Description"));
        }

        [Test]
        [Category("UnitTests")]
        public void WhenOpeningGraphThenTypeIdOfJsonStoredInStringValueIsRestored()
        {
            // Graphs saved by earlier versions store the "$typeid" of cached geometry json as "ConcreteTypeid".
            var savedCache = "[{\"ConcreteTypeid\":\"dynamo.geometry:sab-1.0.0\",\"nested\":\"{\\\"ConcreteType\\\": 1}\"}]";
            var expectedCache = "[{\"$typeid\":\"dynamo.geometry:sab-1.0.0\",\"nested\":\"{\\\"$type\\\": 1}\"}]";
            var json = NodeJson("ConcreteType", savedCache, "Remember");

            var opened = SerializationExtensions.ReplaceTypeDeclarations(json, true);

            var node = (JObject)JObject.Parse(opened)["Nodes"][0];
            Assert.AreEqual(expectedCache, node.Value<string>("InputValue"));

            var saved = SerializationExtensions.ReplaceTypeDeclarations(opened);
            Assert.AreEqual(json, saved);
        }
    }
}
