using Dynamo.DSEngine;
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
    }
}
