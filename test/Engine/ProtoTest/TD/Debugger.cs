using NUnit.Framework;
using ProtoTestFx;
namespace ProtoTest.TD
{
    class Debugger : ProtoTestBase
    {
        string testCasePath = "..\\..\\..\\test\\Engine\\ProtoTest\\ImportFiles\\";
        ProtoScript.Runners.DebugRunner fsr;

        public override void Setup()
        {
            base.Setup();
            fsr = new ProtoScript.Runners.DebugRunner(core);
        }

        public override void TearDown()
        {
            base.TearDown();
            fsr = null;
        }

        [Test]
        public void T004_Defect_IDE_963_Crash_On_Debugging()
        {
            string defectID = "MAGN-4005 GC issue with globally declared objects used in update loop inside Associative language block";

            string src = @" 
import(""FFITarget.dll"");

a : DummyPoint = null;
b : DummyLine = null;
[Associative]
{
    a = DummyPoint.ByCoordinates(10, 0, 0);
    b = DummyLine.ByStartPointEndPoint(a, DummyPoint.ByCoordinates(10, 5, 0));
    a = DummyPoint.ByCoordinates(15, 0, 0);
}
c = b;
";
            DebugTestFx.CompareDebugAndRunResults(src, defectID);
        }

        [Test]
        public void T005_Defect_IDE_963_Crash_On_Debugging()
        {
            string src = @" 
import(""FFITarget.dll"");
a : DummyPoint = null;
b : DummyLine = null;
[Imperative]
{
    a = DummyPoint.ByCoordinates(10, 0, 0);
    b = DummyLine.ByStartPointEndPoint(a, DummyPoint.ByCoordinates(10, 5, 0));
    a = DummyPoint.ByCoordinates(15, 0, 0);
}
c = b;
";
            DebugTestFx.CompareDebugAndRunResults(src);
        }
    }
}
