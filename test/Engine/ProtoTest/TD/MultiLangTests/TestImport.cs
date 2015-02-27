using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTestFx.TD;
namespace ProtoTest.TD.MultiLangTests
{
    class TestImport : ProtoTestBase
    {
        string importPath = "..\\..\\..\\test\\Engine\\ProtoTest\\ImportFiles\\";

        [Test]
        //This served as a sample test case include functionality
        [Category("SmokeTest")]
        public void T001_BasicImport_CurrentDirectory()
        {
            string code = @"
import (""basicImport.ds"");
a = {1.1,2.2};
b = 2;
c = Scale(a,b);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", importPath);
            object[] expectedC = { 2.2, 4.4 };
            thisTest.Verify("c", expectedC);
        }

        [Test]

        public void T002_BasicImport_AbsoluteDirectory()
        {
            string code = @"
import (""basicImport.ds"");
a = {1.1,2.2};
b = 2;
c = Scale(a,b);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", importPath);
            object[] expectedC = { 2.2, 4.4 };
            thisTest.Verify("c", expectedC);
        }

        [Test]
        [Category("SmokeTest")]
        public void T003_BasicImport_ParentPath()
        {
            /*
            object[] expectedC = { 2.2, 4.4 };
            thisTest.Verify("c", expectedC);
            */
        }

        [Test]
        [Category("SmokeTest")]
        public void T004_BasicImport_CurrentDirectoryWithDotAndSlash()
        {
            string code = @"
import (""\basicImport.ds"");
a = {1.1,2.2};
b = 2;
c = Scale(a,b);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", importPath);
            object[] expectedC = { 2.2, 4.4 };
            thisTest.Verify("c", expectedC);
        }

        [Test]
        [Category("SmokeTest")]
        public void T005_BasicImport_RelativePath()
        {
            string code = @"
import (""\ExtraFolderToTestRelativePath\basicImport.ds"");
a = {1.1,2.2};
b = 2;
c = Scale(a,b);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", importPath);
            object[] expectedC = { 2.2, 4.4 };
            thisTest.Verify("c", expectedC);
        }

        
        [Test]
        [Category("SmokeTest")]
        public void T007_BasicImport_TestClassConstructorAndProperties()
        {
            string code = @"
import (""basicImport.ds"");
x = 10.1;
y = 20.2;
z = 30.3;
myPoint = Point.ByCoordinates(10.1, 20.2, 30.3);
myPointX = myPoint.X;
myPointY = myPoint.Y;
myPointZ = myPoint.Z;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", importPath);
            object myPointX = 10.1;
            object myPointY = 20.2;
            object myPointZ = 30.3;
            thisTest.Verify("myPointX", myPointX);
            thisTest.Verify("myPointY", myPointY);
            thisTest.Verify("myPointZ", myPointZ);
        }

        [Test]
        [Category("SmokeTest")]
        public void T008_BasicImport_TestClassConstructorAndProperties_UserDefinedClass()
        {
            string code = @"
import (""basicImport.ds"");
x1 = 10.1;
y1 = 20.2;
z1 = 30.3;
x2 = 110.1;
y2 = 120.2;
z2 = 130.3;
myPoint1 = Point.ByCoordinates(x1, y1, z1);
myPoint2 = Point.ByCoordinates(x2, y2, z2);
myLine = Line.ByStartPointEndPoint(myPoint1, myPoint2);
startPt = myLine.StartPoint;
endPt = myLine.EndPoint;
startPtX = startPt.X;
startPtY = startPt.Y;
startPtZ = startPt.Z;
endPtX = endPt.X;
endPtY = endPt.Y;
endPtZ = endPt.Z;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", importPath);
            object startPtX = 10.1;
            object startPtY = 20.2;
            object startPtZ = 30.3;
            object endPtX = 110.1;
            object endPtY = 120.2;
            object endPtZ = 130.3;
            thisTest.Verify("startPtX", startPtX);
            thisTest.Verify("startPtY", startPtY);
            thisTest.Verify("startPtZ", startPtZ);
            thisTest.Verify("endPtX", endPtX);
            thisTest.Verify("endPtY", endPtY);
            thisTest.Verify("endPtZ", endPtZ);
        }

        [Test]
        [Category("SmokeTest")]
        public void T009_BasicImport_TestClassInstanceMethod()
        {
            string code = @"
import (""basicImport.ds"");
x = 10.1;
y = 20.2;
z = 30.3;
myPoint = Point.ByCoordinates(10.1, 20.2, 30.3);
midValue = myPoint.MidValue();";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", importPath);
            object[] midValue = { 5.05, 10.1, 15.15 };
            thisTest.Verify("midValue", midValue);
        }

        [Test]
        [Category("SmokeTest")]
        public void T010_BaseImportWithVariableClassInstance_top()
        {
            string code = @"
import (""BaseImportWithVariableClassInstance.ds"");
c = a + b;
myPointX = myPoint.X;
arr = Scale(midValue, 4.0);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", importPath);
            object a = 5;
            object b = 10;
            object c = 15;
            object myPointX = 10.1;
            object[] arr = { 20.2, 40.4, 60.6 };
            thisTest.Verify("a", a);
            thisTest.Verify("b", b);
            thisTest.Verify("c", c);
            thisTest.Verify("myPointX", myPointX);
            thisTest.Verify("arr", arr);
        }

        [Test]
        [Category("SmokeTest")]
        public void T012_BaseImportImperative()
        {
            string code = @"
import (""BaseImportImperative.ds"");
a = 1;
b = a;
c;
[Associative]
{
	c = 3 * b;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", importPath);

            thisTest.Verify("a", 1);
            thisTest.Verify("b", 1);
            thisTest.Verify("c", 3);
        }

        [Test]
        [Category("SmokeTest")]
        public void T013_BaseImportImperative_Bottom()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string code = @"
a =10;
b = 2 * a; 
import (""BaseImportImperative.ds"");";
                ExecutionMirror mirror = thisTest.RunScriptSource(code, "", importPath);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T014_BasicImport_BeforeImperative()
        {
            string code = @"
import (""basicImport.ds"");
arr;
[Imperative]
{
	myPoint = Point.ByCoordinates(10.1, 20.2, 30.3);
	midValue = myPoint.MidValue();
	
	arr =  Scale(midValue, 4.0);
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", importPath);
            object[] arr = { 20.2, 40.4, 60.6 };
            thisTest.Verify("arr", arr);
        }

        [Test]
        [Category("SmokeTest")]
        public void T015_BasicImport_Middle()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string code = @"
a =10;
b = 2 * a; 
import (""BasicImport.ds"");
x = 10.1;
y = 20.2;
z = 30.3;
myPoint = Point.ByCoordinates(10.1, 20.2, 30.3);
midValue = myPoint.MidValue();
";
                ExecutionMirror mirror = thisTest.RunScriptSource(code, "", importPath);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T016_BaseImportAssociative()
        {
            string code = @"
import (""BaseImportAssociative.ds"");
a = 10;
b = 20;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", importPath);

            thisTest.Verify("a", 10);
            thisTest.Verify("b", 20);

        }

        [Test]
        [Category("SmokeTest")]
        public void T017_BaseImportWithVariableClassInstance_Associativity()
        {
            string code = @"
import (""BaseImportWithVariableClassInstance.ds"");
c = a + b;
a = 10;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", importPath);

            thisTest.Verify("a", 10);
            thisTest.Verify("b", 20);
            thisTest.Verify("c", 30);
        }

        [Test]
        [Category("SmokeTest")]
        public void T018_MultipleImport()
        {
            string code = @"
import (""basicImport1.ds"");
import (""basicImport2.ds"");
myPoint = Point.ByCoordinates(10.1, 20.2, 30.3);
z = myPoint.Z;
midValue = myPoint.MidValue();
arr = Scale(midValue, 4.0);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", importPath);
            object z = 30.3;
            object[] arr = { 20.2, 40.4, 60.6 };
            thisTest.Verify("z", z);
            thisTest.Verify("arr", arr);
        }

        [Test]
        [Category("SmokeTest")]
        public void T019_MultipleImport_ClashFunctionClassRedifinition()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string code = @"
import (""basicImport.ds"");
import (""basicImport2.ds"");
myPoint = Point.ByCoordinates(10.1, 20.2, 30.3);
z = myPoint.Z;
midValue = myPoint.MidValue();
arr = Scale(midValue, 4.0);";
                ExecutionMirror mirror = thisTest.RunScriptSource(code, "", importPath);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T020_MultipleImport_WithSameFunctionName()
        {
            string code = @"
import (""basicImport1.ds"");
import (""basicImport3.ds"");
arr = { 1.0, 2.0, 3.0 };
a1 = Scale( arr, 4.0 );
b = a * 2;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", importPath);
            thisTest.Verify("b", 6, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T022_Defect_1457740()
        {
            string code = @"
import (""basicImport1.ds"");
import (""basicImport3.ds"");
arr1 = { 1, 3, 5 };
temp = Scale( arr1, a );
a = a;
b = 2 * a;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", importPath);
            thisTest.Verify("b", 6, 0);
        }
    }
}
