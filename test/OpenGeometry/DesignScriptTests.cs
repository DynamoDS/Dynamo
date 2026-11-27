using System;
using System.IO;
using Autodesk.DesignScript.Geometry;
using NUnit.Framework;
using ProtoFFI;
using ProtoScript.Runners;

namespace OpenGeometry.Tests
{
    [TestFixture, Category("UnitTests")]
    public sealed class DesignScriptTests
    {
        [Test]
        public void WhenDesignScriptImportsProtoGeometryThenBothVolumeBranchesEvaluate()
        {
            const string script = """
                import("ProtoGeometry.dll");
                box = Cuboid.ByLengths(10, 10, 10);
                tool = Cylinder.ByPointsRadius(Point.ByCoordinates(0, 0, -6), Point.ByCoordinates(0, 0, 6), 2);
                cut = box.Difference(tool);
                cutVolume = cut.Volume;
                boxVolume = box.Volume;
                """;
            var core = new ProtoCore.Core(new ProtoCore.Options { ExecutionMode = ProtoCore.ExecutionMode.Serial });
            core.Options.IncludeDirectories.Add(Path.GetDirectoryName(typeof(Solid).Assembly.Location));
            core.Compilers.Add(ProtoCore.Language.Associative, new ProtoAssociative.Compiler(core));
            core.Compilers.Add(ProtoCore.Language.Imperative, new ProtoImperative.Compiler(core));
            DLLFFIHandler.Register(FFILanguage.CSharp, new CSModuleHelper());
            var runtime = new ProtoScriptRunner().Execute(script, core);

            try
            {
                Assert.That(core.BuildStatus.ErrorCount, Is.Zero);
                Assert.That(runtime.RuntimeStatus.WarningCount, Is.Zero);
                var cut = runtime.Mirror.GetValue("cutVolume");
                var box = runtime.Mirror.GetValue("boxVolume");
                Assert.Multiple(() =>
                {
                    Assert.That(cut.IsDouble, Is.True);
                    Assert.That(box.IsDouble, Is.True);
                    Assert.That(cut.DoubleValue, Is.EqualTo(1_000 - 40 * Math.PI).Within(0.001));
                    Assert.That(box.DoubleValue, Is.EqualTo(1_000).Within(0.001));
                });
            }
            finally
            {
                runtime.Cleanup();
            }
        }
    }
}
