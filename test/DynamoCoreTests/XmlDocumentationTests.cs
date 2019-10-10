using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Dynamo.Engine;
using Dynamo.Extensions;
using Dynamo.Library;
using NUnit.Framework;
using ProtoCore;

namespace Dynamo.Tests
{
    [TestFixture]
    class XmlDocumentationTests
    {
        #region Helpers

        private static XmlReader SampleDocument = XmlReader.Create(new StringReader(
@"
<doc>          
    <members>
        <member name=""M:MyNamespace.MyClass.MyMethod(System.Double,System.Double,System.Double)"">
            <summary>
            My method bla bla bla bla summary.
            </summary>
            <param name=""a"">Double a.</param>
            <param name=""b"">Double b.</param>
            <param name=""c"">Double c.</param>
            <search>
            move,push
            </search>
            <weights>0.4,0.2</weights>
            <returns name=""foo"">Transformed Geometry.</returns>
        </member>
        <member name=""M:MyNamespace.MyClass.#ctor"">
            <summary>
            Constructor summary.
            </summary>
        </member>
    </members>
</doc>
"));

        private FunctionDescriptor GetMyMethod()
        {
            var parms = new List<TypedParameter>()
            {
                new TypedParameter("a", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Double, 0)),
                new TypedParameter("b", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Double, 0)),
                new TypedParameter("c", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Double, 0))
            };

            var funcDesc = new FunctionDescriptor(new FunctionDescriptorParams
            {
                Assembly = "MyAssembly.dll",
                ClassName = "MyNamespace.MyClass",
                FunctionName = "MyMethod",
                Parameters = parms,
                ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var),
                FunctionType = FunctionType.InstanceMethod
            });

            parms.ForEach(x => x.UpdateFunctionDescriptor(funcDesc));

            return funcDesc;
        }

        private FunctionDescriptor GetConstructorMethod()
        {
            var funcDesc = new FunctionDescriptor(new FunctionDescriptorParams
            {
                Assembly = "MyAssembly.dll",
                ClassName = "MyNamespace.MyClass",
                FunctionName = "MyClass",
                ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var),
                FunctionType = FunctionType.Constructor
            });

            return funcDesc;
        }

        #endregion

        [Test]
        [Category("UnitTests")]
        public void GetSearchTags_FunctionDescriptorXDocument_CanFindSearchTagsForInstanceMethod()
        {
            var method = GetMyMethod();

            var tags = method.GetSearchTags(SampleDocument);

            Assert.AreEqual(2, tags.Count());
            Assert.AreEqual("move", tags.First());
            Assert.AreEqual("push", tags.Last());
        }

        [Test]
        [Category("UnitTests")]
        public void GetSummary_FunctionDescriptorXDocument_CanFindSummaryForBasicInstanceMethod()
        {
            var method = GetMyMethod();

            var summary = method.GetSummary(SampleDocument);

            Assert.IsTrue(summary.StartsWith("My method bla"));
            Assert.IsTrue(summary.EndsWith("summary."));
        }

        [Test]
        [Category("UnitTests")]
        public void GetDescription_TypedParameterXDocument_CanFindParameterDescriptionForBasicInstanceMethod()
        {
            var method = GetMyMethod();
            var paramX = method.Parameters.First();

            var descript = paramX.GetDescription(SampleDocument);

            Assert.AreEqual("Double a.", descript);

            paramX = method.Parameters.ElementAt(1);
            descript = paramX.GetDescription(SampleDocument);
            Assert.AreEqual("Double b.", descript);

            paramX = method.Parameters.ElementAt(2);
            descript = paramX.GetDescription(SampleDocument);
            Assert.AreEqual("Double c.", descript);
        }

        [Test]
        [Category("UnitTests")]
        public void GetSummary_FromConstructor()
        {
            var method = GetConstructorMethod();

            var summary = method.GetSummary(SampleDocument);

            Assert.AreEqual("Constructor summary.", summary);
        }

        [Test]
        [Category("UnitTests")]
        public void GetReturnKeys_FromMyMethod()
        {
            var method = GetMyMethod();

            var returns = method.GetReturns(SampleDocument);

            Assert.AreEqual(1, returns.Count());
            Assert.AreEqual("foo", returns.ElementAt(0).Item1);
            Assert.AreEqual("Transformed Geometry.", returns.ElementAt(0).Item2);
        }

        [Test]
        [Category("UnitTests")]
        public void GetTagsAndWeights_FromMethod()
        {
            var method = GetMyMethod();

            var tags = method.GetSearchTags(SampleDocument);
            var weights = method.GetSearchTagWeights(SampleDocument);

            Assert.AreEqual(2, tags.Count());
            Assert.AreEqual("move", tags.First());
            Assert.AreEqual("push", tags.Last());

            Assert.AreEqual(2, weights.Count());
            Assert.AreEqual(0.4, weights.First());
            Assert.AreEqual(0.2, weights.Last());
        }

        [Test]
        [Category("UnitTests")]
        public void EnumDescriptionTest()
        {
            Assert.AreEqual(Dynamo.Properties.Resources.HeaderCreate, LibraryHeaders.TestCreate.GetDescription());
            Assert.AreEqual(Dynamo.Properties.Resources.HeaderAction, LibraryHeaders.TestAction.GetDescription());
            Assert.AreEqual(LibraryHeaders.TestQuery.ToString(), LibraryHeaders.TestQuery.GetDescription());
        }

    }

    enum LibraryHeaders
    {
        [EnumDescription("HeaderCreate", typeof(Dynamo.Properties.Resources))] TestCreate,
        [EnumDescription("HeaderAction", typeof(Dynamo.Properties.Resources))] TestAction,
        TestQuery,
    }
}
