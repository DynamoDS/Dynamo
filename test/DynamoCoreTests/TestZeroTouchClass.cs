using System;

using Dynamo.Engine;
using Dynamo.Library;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Search.SearchElements;
using NUnit.Framework;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace ProtoTest.FFITests
{
    [TestFixture]
    class TestZeroTouchClass
    {
        [Test]
        public void DescriptionTest()
        {
            var assembly = System.Reflection.Assembly.UnsafeLoadFrom("FFITarget.dll");
            var testClass = assembly.GetType("FFITarget.DummyZeroTouchClass");

            MethodInfo methodWithDesc = testClass.GetMethod("FunctionWithDescription");
            MethodInfo methodWithoutDesc = testClass.GetMethod("FunctionWithoutDescription");

            NodeDescriptionAttribute atr = new NodeDescriptionAttribute("");
            IEnumerable<TypedParameter> arguments;
            FunctionDescriptor fucDescriptor;

            // 1 case. Method with description.
            var attributes = methodWithDesc.GetCustomAttributes(typeof(NodeDescriptionAttribute), false);
            Assert.IsNotNull(attributes);
            Assert.Greater(attributes.Length, 0);
            atr = attributes[0] as NodeDescriptionAttribute;
            arguments = methodWithDesc.GetParameters().Select(
                arg =>
                {
                    var type = new ProtoCore.Type();
                    type.Name = arg.ParameterType.ToString();
                    return new TypedParameter(arg.Name, type);
                });

            fucDescriptor = new FunctionDescriptor(new FunctionDescriptorParams
            {
                FunctionName = methodWithDesc.Name,
                Summary = atr.ElementDescription,
                Parameters = arguments
            });

            NodeModel node = new DSFunction(fucDescriptor);
            Assert.AreEqual(atr.ElementDescription + "\n\n" + fucDescriptor.Signature, node.Description);

            // 2 case. Method without description.
            atr = new NodeDescriptionAttribute("");
            attributes = methodWithoutDesc.GetCustomAttributes(typeof(NodeDescriptionAttribute), false);
            Assert.IsNotNull(attributes);
            Assert.AreEqual(attributes.Length, 0);
            arguments = methodWithoutDesc.GetParameters().Select(
                arg =>
                {
                    var type = new ProtoCore.Type();
                    type.Name = arg.ParameterType.ToString();
                    return new TypedParameter(arg.Name, type);
                });

            fucDescriptor = new FunctionDescriptor(new FunctionDescriptorParams
            {
                FunctionName = methodWithDesc.Name,
                Summary = atr.ElementDescription,
                Parameters = arguments
            });

            node = new DSFunction(fucDescriptor);
            Assert.AreEqual(fucDescriptor.Signature, node.Description);
        }

        [Test]
        public void InPortNamesAttributeTest()
        {
            var assembly = Assembly.UnsafeLoadFrom("FFITarget.dll");
            var testClass = assembly.GetType("FFITarget.DummyZeroTouchClass1");

            var attributes = testClass.GetCustomAttributes(typeof(InPortNamesAttribute), false);
            Assert.IsNotNull(attributes);
            Assert.Greater(attributes.Length, 0);
            Assert.IsTrue(attributes[0] is InPortNamesAttribute);

            var parameters = (attributes[0] as InPortNamesAttribute).PortNames;
            var expected = new[] { "par1", "par2" };

            Assert.AreEqual(expected, parameters);
        }

        [Test]
        public void InPortTypesAttributeTest()
        {
            var assembly = Assembly.UnsafeLoadFrom("FFITarget.dll");
            var testClass = assembly.GetType("FFITarget.DummyZeroTouchClass1");

            var attributes = testClass.GetCustomAttributes(typeof(InPortTypesAttribute), false);
            Assert.IsNotNull(attributes);
            Assert.Greater(attributes.Length, 0);
            Assert.IsTrue(attributes[0] is InPortTypesAttribute);

            var parameters = (attributes[0] as InPortTypesAttribute).PortTypes;
            var expected = new[] { "int", "double" };

            Assert.AreEqual(expected, parameters);
        }

        [Test]
        public void InPortDescriptionsAttributeTest()
        {
            var assembly = Assembly.UnsafeLoadFrom("FFITarget.dll");
            var testClass = assembly.GetType("FFITarget.DummyZeroTouchClass1");

            var attributes = testClass.GetCustomAttributes(typeof(InPortDescriptionsAttribute), false);
            Assert.IsNotNull(attributes);
            Assert.Greater(attributes.Length, 0);
            Assert.IsTrue(attributes[0] is InPortDescriptionsAttribute);

            var parameters = (attributes[0] as InPortDescriptionsAttribute).PortDescriptions;
            var expected = new[] { "some description" };

            Assert.AreEqual(expected, parameters);
        }

        [Test]
        public void OutPortNamesAttributeTest()
        {
            var assembly = Assembly.UnsafeLoadFrom("FFITarget.dll");
            var testClass = assembly.GetType("FFITarget.DummyZeroTouchClass1");

            var attributes = testClass.GetCustomAttributes(typeof(OutPortNamesAttribute), false);
            Assert.IsNotNull(attributes);
            Assert.Greater(attributes.Length, 0);
            Assert.IsTrue(attributes[0] is OutPortNamesAttribute);

            var parameters = (attributes[0] as OutPortNamesAttribute).PortNames;
            var expected = new[] { "out1", "out2", "out3" };

            Assert.AreEqual(expected, parameters);
        }

        [Test]
        public void OutPortTypesAttributeTest()
        {
            var assembly = Assembly.UnsafeLoadFrom("FFITarget.dll");
            var testClass = assembly.GetType("FFITarget.DummyZeroTouchClass1");

            var attributes = testClass.GetCustomAttributes(typeof(OutPortTypesAttribute), false);
            Assert.IsNotNull(attributes);
            Assert.Greater(attributes.Length, 0);
            Assert.IsTrue(attributes[0] is OutPortTypesAttribute);

            var parameters = (attributes[0] as OutPortTypesAttribute).PortTypes;
            var expected = new[] { "1", "2", "3" };

            Assert.AreEqual(expected, parameters);
        }

        [Test]
        public void OutPortDescriptionsAttributeTest()
        {
            var assembly = Assembly.UnsafeLoadFrom("FFITarget.dll");
            var testClass = assembly.GetType("FFITarget.DummyZeroTouchClass1");

            var attributes = testClass.GetCustomAttributes(typeof(OutPortDescriptionsAttribute), false);
            Assert.IsNotNull(attributes);
            Assert.Greater(attributes.Length, 0);
            Assert.IsTrue(attributes[0] is OutPortDescriptionsAttribute);

            var parameters = (attributes[0] as OutPortDescriptionsAttribute).PortDescriptions;
            var expected = new[] { "desc" };

            Assert.AreEqual(expected, parameters);
        }
    }
}
