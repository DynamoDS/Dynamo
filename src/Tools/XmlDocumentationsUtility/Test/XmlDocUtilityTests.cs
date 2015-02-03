using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XmlDocumentationsUtility;

namespace XmlDocumentationsUtility.Test
{
    [TestFixture]
    class XmlDocUtilityTests
    {
        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public void GetMemberElement()
        {
            Assert.IsTrue(XmlDocumentationsUtility.GetMemberElement("M:Autodesk.DesignScript.Geometry.Surface.Join(Autodesk.DesignScript.Geometry.Surface)")
                == "Autodesk.DesignScript.Geometry.Surface.Join");

            Assert.IsTrue(XmlDocumentationsUtility.GetMemberElement("P:Autodesk.DesignScript.Geometry.Surface.Area") == "Autodesk.DesignScript.Geometry.Surface.Area");
        }

        [Test]
        public void GetTypeAndMemberName()
        {
            Tuple<string,string> typeAndMemberName = XmlDocumentationsUtility.GetTypeAndMemberName("Autodesk.DesignScript.Geometry.Surface.Area");
            string typeName = typeAndMemberName.Item1;
            string memberName = typeAndMemberName.Item2;
            Assert.IsTrue(typeName == "Autodesk.DesignScript.Geometry.Surface");
            Assert.IsTrue(memberName == "Area");

            typeAndMemberName = XmlDocumentationsUtility.GetTypeAndMemberName("Autodesk.DesignScript.Geometry.NurbsSurface.ToString");
            typeName = typeAndMemberName.Item1;
            memberName = typeAndMemberName.Item2;
            Assert.IsTrue(typeName == "Autodesk.DesignScript.Geometry.NurbsSurface");
            Assert.IsTrue(memberName == "ToString");
        }

        [Test]
        public void ParseMemberElement()
        {
            XmlDocumentationsUtility.MemberData memberData;

            memberData = XmlDocumentationsUtility.ParseMemberElement("M:Analysis.DataTypes.SurfaceAnalysisData.BySurfacePointsAndResults"+
                "(Autodesk.DesignScript.Geometry.Surface,System.Collections.Generic.IEnumerable{Autodesk.DesignScript.Geometry.UV},"+
            "System.Collections.Generic.IList{System.String},System.Collections.Generic.IList{System.Collections.Generic.IList{System.Double}})");

            Assert.IsTrue(memberData.type == XmlDocumentationsUtility.Type.Method);
            Assert.IsTrue(memberData.TypeName == "Analysis.DataTypes.SurfaceAnalysisData");
            Assert.IsTrue(memberData.MemberName == "BySurfacePointsAndResults");
        }
    }
}
