using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using ProtoCore.Lang;
using ProtoScript.Runners;
using ProtoCore.DSASM.Mirror;
using ProtoTestFx.TD;
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
        [Ignore][Category("DSDefinedClass_Ignored_DebuggerVersion")]
        [Category("Debugger")]
        public void T001_SampleTest()
        {
            //string errorString = "1463735 - Sprint 20 : rev 2147 : breakpoint cannot be set on property ' setter' and 'getter' methods ";
            string src = string.Format("{0}{1}", testCasePath, "T001_SampleTest.ds");
            fsr.LoadAndPreStart(src);
            ProtoCore.CodeModel.CodePoint cp = new ProtoCore.CodeModel.CodePoint
            {
                CharNo = 8,
                LineNo = 17,
                SourceLocation = new ProtoCore.CodeModel.CodeFile
                {
                    FilePath = Path.GetFullPath(src)
                }
            };
            fsr.ToggleBreakpoint(cp);
            // First step should land on line "p = Point.Point();".
            ProtoScript.Runners.DebugRunner.VMState vms = fsr.StepOver();
            Assert.AreEqual(14, vms.ExecutionCursor.StartInclusive.LineNo);
            // These are not used for now, so I'm commenting them out.
            // Object[] exp = { 1, 2, 3, new object[] { 4, 5 }, 6.0, 7, new object[] { 8.0, 9 } };
            // Object[] exp2 = new Object[] { exp, 10 };
            Obj stackValue = null;
            try
            {
                stackValue = vms.mirror.GetDebugValue("y");
            }
            catch (ProtoCore.DSASM.Mirror.UninitializedVariableException exception)
            {
                // Variable "y" isn't valid as of now.
                Assert.AreEqual("y", exception.Name);
            }
            vms = fsr.Run(); // Run to breakpoint on property setter "p.x = 20;".
            Assert.AreEqual(17, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(17, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(14, vms.ExecutionCursor.EndExclusive.CharNo);
            stackValue = vms.mirror.GetDebugValue("y");
            Assert.AreEqual("10", stackValue.Payload.ToString());
        }

        [Test]
        [Ignore][Category("DSDefinedClass_Ignored_DebuggerVersion")]
        [Category("Failure")]
        public void Defect_1467570_Crash_In_Debug_Mode()
        {
            string src = @" 
class Test 
{   
    IntArray : int[]; 
    
    constructor FirstApproach(intArray : int[]) 
    { 
        IntArray = intArray; 
    } 
    
    def Transform(adjust : int) 
    { 
        return = Test.FirstApproach(this.IntArray + adjust); 
    } 
        
} 
myTest = Test.FirstApproach({ 1, 2 }); 
myNeTwst = myTest.Transform(1); 
";
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3989
            string defectID = "MAGN-3989 Inspection of 'this' pointer has issues in expression interpreter";

            fsr.PreStart(src);
            DebugRunner.VMState vms = fsr.Step();   // myTest = Test.FirstApproach({ 1, 2 }); 
            ProtoCore.CodeModel.CodePoint cp = new ProtoCore.CodeModel.CodePoint
            {
                LineNo = 15,
                CharNo = 5
            };

            fsr.ToggleBreakpoint(cp);
            fsr.Run();  // line containing "this"            

            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"this");
            Obj objExecVal = mirror.GetWatchValue();
            Assert.AreNotEqual(null, objExecVal);
            Assert.AreNotEqual(null, objExecVal.Payload, defectID);
            Assert.AreEqual(mirror.GetType(objExecVal), "Test");
            vms = fsr.StepOver();

            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"this");
            objExecVal = mirror.GetWatchValue();
            Assert.AreNotEqual(null, objExecVal);
            Assert.AreEqual(-1, (Int64)objExecVal.Payload, defectID);
            Assert.AreEqual(mirror.GetType(objExecVal), "null");
        }

        [Test]
        [Ignore][Category("DSDefinedClass_Ignored_DebuggerVersion")]
        public void Defect_1467584_Crash_In_Debug_Mode()
        {
            string src = @" 
class LabelledEdge extends _Edge
{
        constructor ByVertexLabels(a : int)
        {
                
        }
        
        def AddVertices()
    {
       return = LabelledEdge.ByVertexLabels(5);
                
    }
}
class _Edge
{
}
edges = LabelledEdge.ByVertexLabels({ 1000, 2000, 3000, 4000, 5000 });
x = edges.AddVertices(); 
";
            DebugTestFx.CompareDebugAndRunResults(src);

        }

        [Test]
        [Ignore][Category("DSDefinedClass_Ignored_DebuggerVersion")]
        public void Defect_1467584_Crash_In_Debug_Mode_2()
        {
            string src = @" 
class LabelledEdge extends _Edge
{
        constructor ByVertexLabels()
        {
                
        }
        
        def AddVertices()
    {
       return = LabelledEdge.ByVertexLabels();
                
    }
}
class _Edge
{
}
edges = { LabelledEdge.ByVertexLabels(), LabelledEdge.ByVertexLabels()};
x = edges.AddVertices(); 
";
            DebugTestFx.CompareDebugAndRunResults(src);
        }

        [Test]
        [Ignore][Category("DSDefinedClass_Ignored_DebuggerVersion")]
        public void Defect_1467584_Crash_In_Debug_Mode_5()
        {
            string src = @" 
class LabelledEdge extends _Edge
{
        constructor ByVertexLabels()
        {
                
        }
        
        def AddVertices(x:int)
        {
           return = LabelledEdge.ByVertexLabels(x);
                
        }
}
class _Edge
{
}
edges = { LabelledEdge.ByVertexLabels(), LabelledEdge.ByVertexLabels()};
x = edges.AddVertices(0..1); 
";
            DebugTestFx.CompareDebugAndRunResults(src);
        }

        [Test]
        [Ignore][Category("DSDefinedClass_Ignored_DebuggerVersion")]
        public void Defect_1467584_Crash_In_Debug_Mode_6()
        {
            string src = @" 
class LabelledEdge extends _Edge
{
        constructor ByVertexLabels()
        {
                
        }
        
        static def AddVertices(x:int)
        {
           return = LabelledEdge.ByVertexLabels(x);
                
        }
}
class _Edge
{
}
edges = { LabelledEdge.ByVertexLabels(), LabelledEdge.ByVertexLabels()};
x = edges.AddVertices(0..1); 
";
            DebugTestFx.CompareDebugAndRunResults(src);
        }

        [Test]
        [Ignore][Category("DSDefinedClass_Ignored_DebuggerVersion")]
        public void Defect_1467584_Crash_In_Debug_Mode_7()
        {
            string src = @" 
class LabelledEdge extends Edge
{
    y;
    constructor ByVertexLabels(a : int) : base.add(a)
    {
        y = a;
    }
        
    def AddVertices(x : int)
    {
       return = Edge.add(x);
                
    }
}
class Edge
{
    a1 : int;
    constructor add(xx)
    {
        a1 = xx;
    }
}
edges = LabelledEdge.ByVertexLabels({ 1000, 2000, 3000, 4000, 5000 });
x = edges.AddVertices(0..2);
";
            DebugTestFx.CompareDebugAndRunResults(src);
        }

        [Test]
        [Ignore][Category("DSDefinedClass_Ignored_DebuggerVersion")]
        public void Defect_IDE_884_UsingBangInsideImperative_1()
        {
            string src = @" 
class test
{
    static def foo()
    {
        included = true;  
        return = [Imperative]
        {
            a = !included;
            return = a;                
        }
        return = true;
    }
    
}
test1 = test.foo();
";
            DebugTestFx.CompareDebugAndRunResults(src);
        }

        [Test]
        [Ignore][Category("DSDefinedClass_Ignored_DebuggerVersion")]
        public void Defect_IDE_884_UsingBangInsideImperative_2()
        {
            string src = @" 
class test
{
    def foo()
    {
        a; 
        [Imperative]
        {
            included = true;
            a = !included;
        }
        return = a;
    }
}
p = test.test();
b = p.foo();
";
            DebugTestFx.CompareDebugAndRunResults(src);
        }

        [Test]
        [Ignore][Category("DSDefinedClass_Ignored_DebuggerVersion")]
        public void Defect_IDE_884_UsingBangInsideImperative_3()
        {
            string src = @" 
class test
{
    def foo()
    {
        a; 
        [Imperative]
        {
            included = true;
            a = !included;
        }
        return = a;
    }
}
p = test.test();
b = p.foo();
";
            DebugTestFx.CompareDebugAndRunResults(src);
        }

        [Test]
        [Ignore][Category("DSDefinedClass_Ignored_DebuggerVersion")]
        [Category("WatchFx Tests")]
        public void T002_Defect_1467629_Debugging_InlineCondition_With_Multiple_Return_Types()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"class B
{
    b = 0;
    constructor B(xx)
    {
        b = xx;
    }
}
class A  
{
    x : var[]..[];
    constructor A(a : int) //example constructor 
    {
        x = a == 1 ? B.B(1) : { B.B(0), B.B(2) }; // using an in-line conditional
            
    }   
}
c = 2;
aa = A.A(c);
c = 1;";
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
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
