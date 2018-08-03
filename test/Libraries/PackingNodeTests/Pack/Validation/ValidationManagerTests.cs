using Dynamo.Graph.Nodes;
using NUnit.Framework;
using PackingNodeModels;
using PackingNodeModels.Pack;
using PackingNodeModels.Pack.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.Tests
{
    public class ValidationManagerTests
    {
        private Pack GetNode(TypeDefinition type = null)
        {
            var pack = new Pack();

            if (type == null)
            {
                type = new TypeDefinition();
                type.Name = "Something";
                type.Properties = new Dictionary<string, PropertyType> { { "prop1", new PropertyType { Type = "String" } } };
                pack.InPorts.Add(new Graph.Nodes.PortModel(PortType.Input, pack, new PortData("prop1", "")));
                pack.InPorts[1].Connectors.Add(new Graph.Connectors.ConnectorModel(pack.InPorts[1], pack.InPorts[1], Guid.NewGuid()));
            }


            var prop = pack.GetType().GetProperty("TypeDefinition");
            prop.SetValue(pack, type);

            return pack;
        }

        public class HandleValidation: ValidationManagerTests
        {
            [Test]
            public void WhenInvalidDataIsGiven_AddsAWarningToNode()
            {
                var pack = GetNode();
                var validationManager = new ValidationManager(pack);

                var data = new Dictionary<int, object> { { 1, 10 } }; //Should be a string value.

                validationManager.HandleValidation(data);

                Assert.That(pack.State, Is.EqualTo(ElementState.PersistentWarning));
                Assert.That(validationManager.Warnings.Count, Is.EqualTo(1));
            }

            [Test]
            public void WhenInvalidDataIsDisconnected_ShouldClearWarnings()
            {
                var pack = GetNode();
                var validationManager = new ValidationManager(pack);

                var data = new Dictionary<int, object> { { 1, 10 } }; //Should be a string value.

                validationManager.HandleValidation(data);

                try
                {
                    pack.InPorts[1].Connectors.Clear();
                }
                catch { }

                var data2 = new Dictionary<int, object> { { 1, null } };

                validationManager.HandleValidation(data2);

                Assert.That(pack.State, Is.Not.EqualTo(ElementState.PersistentWarning));
                Assert.That(validationManager.Warnings.Count, Is.EqualTo(0));
            }

            [Test]
            public void WhenValidDataIsConnected_ShouldNotAddWarnings()
            {
                var pack = GetNode();
                var validationManager = new ValidationManager(pack);

                var data = new Dictionary<int, object> { { 1, "String" } };

                validationManager.HandleValidation(data);

                Assert.That(pack.State, Is.Not.EqualTo(ElementState.PersistentWarning));
                Assert.That(validationManager.Warnings.Count, Is.EqualTo(0));
            }

            [Test]
            public void WhenValidDataIsConnectedOnPreviouslyInvalidValue_ShouldClearWarnings()
            {
                var pack = GetNode();
                var validationManager = new ValidationManager(pack);

                var data = new Dictionary<int, object> { { 1, 10 } }; //Should be a string value

                validationManager.HandleValidation(data);

                Assert.That(pack.State, Is.EqualTo(ElementState.PersistentWarning));
                Assert.That(validationManager.Warnings.Count, Is.EqualTo(1));

                var data2 = new Dictionary<int, object> { { 1, "String" } };

                validationManager.HandleValidation(data2);

                Assert.That(pack.State, Is.Not.EqualTo(ElementState.PersistentWarning));
                Assert.That(validationManager.Warnings.Count, Is.EqualTo(0));
            }

            [Test]
            public void WhenInValidDataIsConnectedOnPreviouslyInvalidValue_ShouldKeepWarnings()
            {
                var pack = GetNode();
                var validationManager = new ValidationManager(pack);

                var data = new Dictionary<int, object> { { 1, 10 } }; //Should be a string value

                validationManager.HandleValidation(data);

                Assert.That(pack.State, Is.EqualTo(ElementState.PersistentWarning));
                Assert.That(validationManager.Warnings.Count, Is.EqualTo(1));

                var data2 = new Dictionary<int, object> { { 1, null } };

                validationManager.HandleValidation(data2);

                Assert.That(pack.State, Is.EqualTo(ElementState.PersistentWarning));
                Assert.That(validationManager.Warnings.Count, Is.EqualTo(1));
            }
        }
    }
}
