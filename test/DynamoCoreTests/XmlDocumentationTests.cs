using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using Dynamo.DSEngine;
using Dynamo.Library;

using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    class XmlDocumentationTests
    {
        #region Helpers

        private static XDocument SampleDocument = XDocument.Parse(
@"
<doc>          
    <members>
        <member name=""M:Autodesk.DesignScript.Geometry.Geometry.Translate(System.Double,System.Double,System.Double)"">
            <summary>
            Translates any given geometry by the given displacements in the x, y,
            and z directions defined in WCS respectively. 
            </summary>
            <param name=""xTranslation"">Displacement along X-axis.</param>
            <param name=""yTranslation"">Displacement along Y-axis.</param>
            <param name=""zTranslation"">Displacement along Z-axis.</param>
            <search>
            move,push
            </search>
            <returns>Transformed Geometry.</returns>
        </member>
    </members>
</doc>
");

        private static FunctionDescriptor GetTranslateMethod()
        {
            var parms = new List<TypedParameter>()
            {
                new TypedParameter("xTranslation", "double"),
                new TypedParameter("yTranslation", "double"),
                new TypedParameter("zTranslation", "double")
            };

            var funcDesc = new FunctionDescriptor(
                "ProtoGeometry.dll",
                "Autodesk.DesignScript.Geometry.Geometry",
                "Translate",
                parms,
                "Autodesk.DesignScript.Geometry.Geometry",
                FunctionType.InstanceMethod,
                false);

            parms.ForEach(x => x.Function = funcDesc);

            return funcDesc;
        }

        #endregion

        [Test]
        [Category("UnitTests")]
        public static void GetSearchTags_FunctionDescriptorXDocument_CanFindSearchTagsForInstanceMethod()
        {
            var method = GetTranslateMethod();

            var tags = method.GetSearchTags(SampleDocument);

            Assert.AreEqual(2, tags.Count());
            Assert.AreEqual("move", tags.First());
            Assert.AreEqual("push", tags.Last());
        }

        [Test]
        [Category("UnitTests")]
        public static void GetSummary_FunctionDescriptorXDocument_CanFindSummaryForBasicInstanceMethod()
        {
            var method = GetTranslateMethod();

            var summary = method.GetSummary(SampleDocument);

            Assert.IsTrue(summary.StartsWith("Translates any"));
            Assert.IsTrue(summary.EndsWith("defined in WCS respectively."));
        }

        [Test]
        [Category("UnitTests")]
        public static void GetDescription_TypedParameterXDocument_CanFindParameterDescriptionForBasicInstanceMethod()
        {
            var method = GetTranslateMethod();
            var paramX = method.Parameters.First();

            var descript = paramX.GetDescription(SampleDocument);

            Assert.AreEqual("Displacement along X-axis.", descript);
        }
    }
}
