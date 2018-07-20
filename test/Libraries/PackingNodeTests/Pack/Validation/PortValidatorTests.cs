using Dynamo.Graph.Nodes;
using NUnit.Framework;
using PackingNodeModels;
using PackingNodeModels.Pack;
using PackingNodeModels.Pack.Validation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.Tests
{
    public class PortValidatorTests
    {
        public class ValidateMethod
        {
            [Test]
            public void WhenValueIsNull_ShouldReturnErrorMessage()
            {
                var property = new KeyValuePair<string, PropertyType>("prop1", new PropertyType { Type = "String" });
                var result = PortValidator.Validate(property, null, null);

                Assert.That(result, Is.EqualTo("Input prop1 expected type String but received null."));
            }

            public class WhenPropertyIsCollection
            {
                [Test]
                public void WhenValueIsNotAnArrayList_ShouldReturnErrorMessage()
                {
                    var property = new KeyValuePair<string, PropertyType>("prop1", new PropertyType { Type = "String", IsCollection = true });
                    var result = PortValidator.Validate(property, "value", null);

                    Assert.That(result, Is.EqualTo("Input prop1 expected an array of type String but received a single value."));
                }

                [Test]
                public void WhenValueIsCollection_ValidData_ShouldReturnNull()
                {
                    var property = new KeyValuePair<string, PropertyType>("prop1", new PropertyType { Type = "String", IsCollection = true });
                    var result = PortValidator.Validate(property, new ArrayList { "value1", "value2", "value3" }, null);

                    Assert.Null(result);
                }

                [Test]
                public void WhenValueIsACollectionOfSingleValuesAndCollections_ShouldReturnErrorMessage()
                {
                    var property = new KeyValuePair<string, PropertyType>("prop1", new PropertyType { Type = "String", IsCollection = true });
                    var result = PortValidator.Validate(property, new ArrayList { new ArrayList { "value1", "value2" }, "value3" }, null);

                    Assert.That(result, Is.EqualTo("Input prop1 expected an array of type String but received a mixed combination of single values and arrays."));
                }

                [Test]
                public void WhenValueIsCollectionOfCollection_ValidData_ShouldReturnNull()
                {
                    var property = new KeyValuePair<string, PropertyType>("prop1", new PropertyType { Type = "String", IsCollection = true });
                    var result = PortValidator.Validate(property, new ArrayList { new ArrayList { "value1", "value2" }, new ArrayList { "value3" } }, null);

                    Assert.Null(result);
                }

                [Test]
                public void WhenValueIsCollectionOfCollection_OneInvalidValue_ShouldReturnErrorMessage()
                {
                    var property = new KeyValuePair<string, PropertyType>("prop1", new PropertyType { Type = "String", IsCollection = true });
                    var result = PortValidator.Validate(property, new ArrayList { new ArrayList { "value1", "value2" }, new ArrayList { 1 } }, null);

                    Assert.NotNull(result);
                }

                [Test]
                public void WhenValueIsCollectionOfCollection_NestedArray_ShouldReturnErrorMessage()
                {
                    var property = new KeyValuePair<string, PropertyType>("prop1", new PropertyType { Type = "String", IsCollection = true });
                    var result = PortValidator.Validate(property, new ArrayList { new ArrayList { "value1", "value2" }, new ArrayList { new ArrayList { "value3" } } }, null);

                    Assert.That(result, Is.EqualTo("Input prop1 expected an array of type String but received a nested array."));
                }
            }

            public class WhenPropertyIsNotCollection
            {
                [Test]
                public void WhenValueIsArrayList_NestedArrays_ShouldReturnError() //Lacing with subvalues being collections
                {
                    var property = new KeyValuePair<string, PropertyType>("prop1", new PropertyType { Type = "String", IsCollection = false });
                    var result = PortValidator.Validate(property, new ArrayList { new ArrayList { "value1", "value2" } }, null);

                    Assert.That(result, Is.EqualTo("Input prop1 expected a single value of type String but received a list of values."));
                }

                [Test]
                public void WhenValueIsArrayList_ValidData_ShouldReturnNull()
                {
                    var property = new KeyValuePair<string, PropertyType>("prop1", new PropertyType { Type = "String", IsCollection = false });
                    var result = PortValidator.Validate(property, new ArrayList { "value1", "value2" }, null);

                    Assert.Null(result);
                }

                [Test]
                public void WhenValueIsSingle_Invalid_ShouldReturnError()
                {
                    var property = new KeyValuePair<string, PropertyType>("prop1", new PropertyType { Type = "String", IsCollection = false });
                    var result = PortValidator.Validate(property, 1, null);

                    Assert.NotNull(result);
                }

                [Test]
                public void WhenValueIsSingle_Valid_ShouldReturnError()
                {
                    var property = new KeyValuePair<string, PropertyType>("prop1", new PropertyType { Type = "String", IsCollection = false });
                    var result = PortValidator.Validate(property, "string", null);

                    Assert.Null(result);
                }
            }
        }
        public class ValidateTypeMatch
        {
            [Test]
            public void WhenTypeIsKnown_TypeMatch_ShouldReturnNull()
            {
                var property = new KeyValuePair<string, PropertyType>("prop1", new PropertyType { Type = "String" });
                var result = PortValidator.ValidateTypeMatch(property, "string", null);

                Assert.Null(result);
            }

            [Test]
            public void WhenTypeIsKnown_TypeDoesNotMatch_ShouldReturnErrorMessage()
            {
                var property = new KeyValuePair<string, PropertyType>("prop1", new PropertyType { Type = "String" });
                var result = PortValidator.ValidateTypeMatch(property, 10, null);

                Assert.That(result, Is.EqualTo("Input prop1 expected type String but received System.Int32."));
            }

            [Test]
            public void WhenTypeIsUnknown_HookedToNonPackNode_ShouldReturnErrorMessage()
            {
                var property = new KeyValuePair<string, PropertyType>("prop1", new PropertyType { Type = "Something" });
                var portModel = new PortModel(PortType.Input, null, new PortData("prop1",""));
                var ownerPortModel = new PortModel(PortType.Output, null, new PortData("out", ""));
                portModel.Connectors.Add(new Graph.Connectors.ConnectorModel(ownerPortModel, portModel, Guid.NewGuid()));
                var result = PortValidator.ValidateTypeMatch(property, 10, portModel);

                Assert.That(result, Is.EqualTo("Input prop1 expected type Something but received System.Int32."));
            }

            [Test]
            public void WhenTypeIsUnknown_HookedToPackNodeButWrongMatch_ShouldReturnErrorMessage()
            {
                var previousPack = new Pack();
                previousPack.GetType().GetProperty("TypeDefinition").SetValue(previousPack, new TypeDefinition { Name = "NotSomething" });

                var property = new KeyValuePair<string, PropertyType>("prop1", new PropertyType { Type = "Something" });
                var portModel = new PortModel(PortType.Input, null, new PortData("prop1", ""));
                var ownerPortModel = new PortModel(PortType.Output, previousPack, new PortData("out", ""));
                portModel.Connectors.Add(new Graph.Connectors.ConnectorModel(ownerPortModel, portModel, Guid.NewGuid()));
                var result = PortValidator.ValidateTypeMatch(property, 10, portModel);

                Assert.That(result, Is.EqualTo("Input prop1 expected type Something but received NotSomething."));
            }

            [Test]
            public void WhenTypeIsUnknown_HookedToPackNodeWithTypeMatch_ShouldReturnNull()
            {
                var previousPack = new Pack();
                previousPack.GetType().GetProperty("TypeDefinition").SetValue(previousPack, new TypeDefinition { Name = "Something" });

                var property = new KeyValuePair<string, PropertyType>("prop1", new PropertyType { Type = "Something" });
                var portModel = new PortModel(PortType.Input, null, new PortData("prop1", ""));
                var ownerPortModel = new PortModel(PortType.Output, previousPack, new PortData("out", ""));
                portModel.Connectors.Add(new Graph.Connectors.ConnectorModel(ownerPortModel, portModel, Guid.NewGuid()));
                var result = PortValidator.ValidateTypeMatch(property, 10, portModel);

                Assert.Null(result);
            }
        }
    }
}
