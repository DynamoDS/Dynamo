using Dynamo.Engine;
using NUnit.Framework;


namespace Dynamo.Tests
{
    [TestFixture]
    internal class ExperimentalNodesTests
    {
        [Test]
        public void FunctionDescriptorIsMarkedExperimentalByExperimentalPrefsSection()
        {
            var x = new FunctionDescriptor(new FunctionDescriptorParams
            {
                Assembly = "ProtoGeometry.dll",
                ClassName = "Autodesk.DesignScript.Geometry.PanelSurface",
                FunctionName = "somefunc",
                Parameters = [],
                ReturnType = ProtoCore.TypeSystem.BuildPrimitiveTypeObject(ProtoCore.PrimitiveType.Void),
                FunctionType = FunctionType.InstanceMethod,
                IsVisibleInLibrary = true,
                ReturnKeys = [],
                PathManager = null,
                IsVarArg = false,
                ObsoleteMsg = null,
                CanUpdatePeriodically = false,
                IsBuiltIn = false,
                IsPackageMember = false,
                IsLacingDisabled = false,
                //even though this is set to false, the function should be marked as experimental
                //because the assembly/classname combination is marked as experimental in the experimental prefs section
                IsExperimental = false,
            });
            Assert.That(x.IsExperimental, Is.True);
        }
    }
}
