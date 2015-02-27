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
        ProtoScript.Config.RunConfiguration runnerConfig;
        string testCasePath = "..\\..\\..\\test\\Engine\\ProtoTest\\ImportFiles\\";
        ProtoScript.Runners.DebugRunner fsr;

        public override void Setup()
        {
            base.Setup();
            runnerConfig = new ProtoScript.Config.RunConfiguration();
            runnerConfig.IsParrallel = false;
            fsr = new ProtoScript.Runners.DebugRunner(core);
        }

        public override void TearDown()
        {
            base.TearDown();
            fsr = null;
        }

        [Test]
        [Category("Debugger")]
        public void T001_SampleTest()
        {
            //string errorString = "1463735 - Sprint 20 : rev 2147 : breakpoint cannot be set on property ' setter' and 'getter' methods ";
            string src = string.Format("{0}{1}", testCasePath, "T001_SampleTest.ds");
            fsr.LoadAndPreStart(src, runnerConfig);
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

            fsr.PreStart(src, runnerConfig);
            DebugRunner.VMState vms = fsr.Step();   // myTest = Test.FirstApproach({ 1, 2 }); 
            ProtoCore.CodeModel.CodePoint cp = new ProtoCore.CodeModel.CodePoint
            {
                LineNo = 15,
                CharNo = 5
            };

            fsr.ToggleBreakpoint(cp);
            fsr.Run();  // line containing "this"            

            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core);
            ExecutionMirror mirror = watchRunner.Execute(@"this");
            Obj objExecVal = mirror.GetWatchValue();
            Assert.AreNotEqual(null, objExecVal);
            Assert.AreNotEqual(null, objExecVal.Payload, defectID);
            Assert.AreEqual(mirror.GetType(objExecVal), "Test");
            vms = fsr.StepOver();

            watchRunner = new ExpressionInterpreterRunner(core);
            mirror = watchRunner.Execute(@"this");
            objExecVal = mirror.GetWatchValue();
            Assert.AreNotEqual(null, objExecVal);
            Assert.AreEqual(-1, (Int64)objExecVal.Payload, defectID);
            Assert.AreEqual(mirror.GetType(objExecVal), "null");
        }

        [Test]
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
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void Defect_1467584_Crash_In_Debug_Mode_3()
        {
            string src = @"
import(""DSCoreNodes.dll"");
import(""ProtoGeometry.dll"");
class Mesh
{
    Edges : _Edge[];
    Vertices : _Vertex[];
}
class EdgeMesh extends Mesh
{
    public constructor ByVerticesEdgeIndices(pointGeometry : Point[], edgeIndices : int[][])
    {
        initialVertices = _Vertex.ByPointGeometry(pointGeometry);
        
        Edges = _Edge.ByVertices(initialVertices[edgeIndices[0]], initialVertices[edgeIndices[1]]);
        
        Vertices = initialVertices.AddEdges(Edges);
    }
    
    public constructor ByLabelledVerticesEdges(initialVertices : LabelledVertex[], initialEdges : LabelledEdge[])
    {
        Edges = initialEdges.AddVertices(initialVertices);
        
        Vertices = initialVertices.AddEdges(Edges);
    }
}
class FaceMesh extends Mesh
{
    Faces : _Face[];
    
    public constructor ByVerticesFaceIndices(pointGeometry : Point[], faceIndices : int[][])
    {
    }
}
class _Vertex
{
    PointGeometry : Point;
    Edges : _Edge[];
    NextVertices : _Vertex[];
    
    constructor ByCoordinates(x : double, y : double, z : double)
    {
        PointGeometry = Point.ByCoordinates(x, y, z).SetColor(Color.Red);
    }
    
    constructor ByPointGeometry(pointGeometry : Point)
    {
        PointGeometry = pointGeometry;
    }
        
    constructor ByPointGeometryEdges(pointGeometry : Point, edges : _Edge[], nextVertices : _Vertex[])
    {
        PointGeometry = pointGeometry;
        Edges = edges;
        NextVertices = nextVertices;
    }
    
    def AddEdges : _Vertex ( allEdges : _Edge[])
    {
        i = 0;
        
        temp = this;
        
        Print(""temp = "" + temp);
        
        localEdges = { };
        localNextVertices = { };
        
        [Imperative]
        {
            for(m in allEdges)
            {
                if (m.StartVertex == temp) 
                {
                    localEdges[i] = m;
                    localNextVertices[i] = m.EndVertex;
                    i = i + 1;
                }
                else if (m.EndVertex == temp)
                {
                    localEdges[i] = m;
                    localNextVertices[i] = m.StartVertex;
                    i = i + 1;
                }
            }
        }
        
        Print(""i = "" + i);
        Print(""localEdgesCount = "" + Count(localEdges));
        return = _Vertex.ByPointGeometryEdges(this.PointGeometry, localEdges, localNextVertices);
    }
}
class LabelledVertex extends _Vertex
{
    Label : int;
    
    constructor ByLabelledCoordinates(label : int, x : double, y : double, z : double)
    {
        PointGeometry = Point.ByCoordinates(x, y, z).SetColor(Color.Red);
        Label = label;
    }
}
class _Edge
{
    StartVertex : _Vertex;
    EndVertex : _Vertex;
    CurveGeometry : Line;
    
    constructor ByVertices(startVertex : _Vertex, endVertex : _Vertex)
    {
        StartVertex = startVertex;
        EndVertex = endVertex;
        
        CurveGeometry = Line.ByStartPointEndPoint(StartVertex.PointGeometry, EndVertex.PointGeometry);
    }
}
class LabelledEdge extends _Edge
{
    Label : int;
    StartVertexLabel : int;
    EndVertexLabel : int;
    
    constructor ByVertexLabels(label : int, startVertexLabel : int, endVertexLabel : int)
    {
        Label = label;
        StartVertexLabel = startVertexLabel;
        EndVertexLabel = endVertexLabel;
    }
        
    constructor ByVertexLabels(label : int, startVertexLabel : int, endVertexLabel : int, labelledVertices : LabelledVertex[])
    {
        Label = label;
        StartVertex = labelledVertices[IndexOf(labelledVertices.Label, startVertexLabel)];
        EndVertex = labelledVertices[IndexOf(labelledVertices.Label, endVertexLabel)];
        CurveGeometry = Line.ByStartPointEndPoint(StartVertex.PointGeometry, EndVertex.PointGeometry);
    }
    
    def AddVertices(labelledVertices : LabelledVertex[])
    {
        return = LabelledEdge.ByVertexLabels(this.Label, this.StartVertexLabel, this.EndVertexLabel, labelledVertices);
    }
}
class _Face
{
    Edges : _Edge[];
    Vertices : _Vertex[];
}
vertices = LabelledVertex.ByLabelledCoordinates({ 100, 200, 300, 400 }, { 1, 10, 10, 1 }, { 1, 1, 10, 10 }, 0);
edges = LabelledEdge.ByVertexLabels({ 1000, 2000, 3000, 4000, 5000 }, { 100, 200, 300, 400, 100 }, { 200, 300, 400, 100, 300 });
mesh = EdgeMesh.ByLabelledVerticesEdges(vertices, edges);
check;
[Imperative]
{
    for(i in (0..3))
    {
        [Associative]
        {
            check = Cone.ByStartPointEndPointRadius(mesh.Vertices[i].PointGeometry, mesh.Vertices[i].NextVertices.PointGeometry, 0.1, 0.2);
        }
        //Geometry.UpdateDisplay();
        // Utils.Sleep(2000);
    }
} 
";
            DebugTestFx.CompareDebugAndRunResults(src);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void Defect_1467584_Crash_In_Debug_Mode_4()
        {
            string src = @" 
import(""DSCoreNodes.dll"");
import(""ProtoGeometry.dll"");
class Mesh
{
    Edges : _Edge[];
    Vertices : _Vertex[];
}
class EdgeMesh extends Mesh
{
    public constructor ByVerticesEdgeIndices(pointGeometry : Point[], edgeIndices : int[][])
    {
        initialVertices = _Vertex.ByPointGeometry(pointGeometry);
        
        Edges = _Edge.ByVertices(initialVertices[edgeIndices[0]], initialVertices[edgeIndices[1]]);
        
        Vertices = initialVertices.AddEdges(Edges);
    }
    
    public constructor ByLabelledVerticesEdges(initialVertices : LabelledVertex[], initialEdges : LabelledEdge[])
    {
        Edges = initialEdges.AddVertices(initialVertices);
        
        Vertices = initialVertices.AddEdges(Edges);
    }
}
class FaceMesh extends Mesh
{
    Faces : _Face[];
    
    public constructor ByVerticesFaceIndices(pointGeometry : Point[], faceIndices : int[][])
    {
    }
}
class _Vertex
{
    PointGeometry : Point;
    Edges : _Edge[];
    NextVertices : _Vertex[];
    
    constructor ByCoordinates(x : double, y : double, z : double)
    {
        PointGeometry = Point.ByCoordinates(x, y, z).SetColor(Color.Red);
    }
    
    constructor ByPointGeometry(pointGeometry : Point)
    {
        PointGeometry = pointGeometry;
    }
        
    constructor ByPointGeometryEdges(pointGeometry : Point, edges : _Edge[], nextVertices : _Vertex[])
    {
        PointGeometry = pointGeometry;
        Edges = edges;
        NextVertices = nextVertices;
    }
    
    def AddEdges : _Vertex ( allEdges : _Edge[])
    {
        i = 0;
        
        temp = this;
        
        Print(""temp = "" + temp);
        
        localEdges = { };
        localNextVertices = { };
        
        [Imperative]
        {
            for(m in allEdges)
            {
                if (m.StartVertex == temp) 
                {
                    localEdges[i] = m;
                    localNextVertices[i] = m.EndVertex;
                    i = i + 1;
                }
                else if (m.EndVertex == temp)
                {
                    localEdges[i] = m;
                    localNextVertices[i] = m.StartVertex;
                    i = i + 1;
                }
            }
        }
        
        Print(""i = "" + i);
        Print(""localEdgesCount = "" + Count(localEdges));
        return = _Vertex.ByPointGeometryEdges(this.PointGeometry, localEdges, localNextVertices);
    }
}
class LabelledVertex extends _Vertex
{
    Label : int;
    
    constructor ByLabelledCoordinates(label : int, x : double, y : double, z : double)
    {
        PointGeometry = Point.ByCoordinates(x, y, z).SetColor(Color.Red);
        Label = label;
    }
}
class _Edge
{
    StartVertex : _Vertex;
    EndVertex : _Vertex;
    CurveGeometry : Line;
    
    constructor ByVertices(startVertex : _Vertex, endVertex : _Vertex)
    {
        StartVertex = startVertex;
        EndVertex = endVertex;
        
        CurveGeometry = Line.ByStartPointEndPoint(StartVertex.PointGeometry, EndVertex.PointGeometry);
    }
}
class LabelledEdge extends _Edge
{
    Label : int;
    StartVertexLabel : int;
    EndVertexLabel : int;
    
    constructor ByVertexLabels(label : int, startVertexLabel : int, endVertexLabel : int)
    {
        Label = label;
        StartVertexLabel = startVertexLabel;
        EndVertexLabel = endVertexLabel;
    }
        
    constructor ByVertexLabels(label : int, startVertexLabel : int, endVertexLabel : int, labelledVertices : LabelledVertex[])
    {
        Label = label;
        StartVertex = labelledVertices[IndexOf(labelledVertices.Label, startVertexLabel)];
        EndVertex = labelledVertices[IndexOf(labelledVertices.Label, endVertexLabel)];
        CurveGeometry = Line.ByStartPointEndPoint(StartVertex.PointGeometry, EndVertex.PointGeometry);
    }
    
    def AddVertices(labelledVertices : LabelledVertex[])
    {
        return = LabelledEdge.ByVertexLabels(this.Label, this.StartVertexLabel, this.EndVertexLabel, labelledVertices);
    }
}
class _Face
{
    Edges : _Edge[];
    Vertices : _Vertex[];
}
vertices = LabelledVertex.ByLabelledCoordinates({ 100, 200, 300, 400 }, { 1, 10, 10, 1 }, { 1, 1, 10, 10 }, 0);
edges = LabelledEdge.ByVertexLabels({ 1000, 2000, 3000, 4000, 5000 }, { 100, 200, 300, 400, 100 }, { 200, 300, 400, 100, 300 });
mesh = EdgeMesh.ByLabelledVerticesEdges(vertices, edges);
check;
[Imperative]
{
    for(i in 5..8)
    {
        [Associative]
        {
            check = Cone.ByStartPointEndPointRadius(mesh.Vertices[i].PointGeometry, mesh.Vertices[i].NextVertices.PointGeometry, 0.1, 0.2);
        }
        Geometry.UpdateDisplay();
        Utils.Sleep(2000);
    }
}
";
            DebugTestFx.CompareDebugAndRunResults(src);
        }


        [Test]
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
        [Category("WatchFx Tests")]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void T003_Defect_1467629_Debugging_InlineCondition_With_Multiple_Return_Types()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"import(""ProtoGeometry.dll"");
import(""DSCoreNodes.dll"");
class FixitySymbol // this class is implicitly extended from var 
{
    Origin : Point; // define the class properties.. 
    Size : double;
    IsFixed : bool;
    Symbol : var[]..[]; //defined 'by composition', by one or more Solids 
    constructor FromOriginSize(origin : Point, size : double, isFixed : bool) //example constructor 
    {
        Origin = origin; // by convention properties of the class (with uppercase names) 
        Size = size; // are populated from the corresponding arguments (with lowercase names) 
        IsFixed = isFixed;
        localWCS = CoordinateSystem.WCS;
        Symbol = isFixed ? // using an in-line conditional
            Cuboid.ByLengths(CoordinateSystem.ByOriginVectors(Origin, // if true 
            Vector.ByCoordinates(1,0,0), Vector.ByCoordinates(0,1,0)), Size, Size, Size) : { Sphere.ByCenterPointRadius(Origin, Size * 0.25),
            Cone.ByCenterLineRadius(Line.ByStartPointDirectionLength(Origin, Vector.ByCoordinates(0,0,1), -Size), Size * 0.01, Size * 0.5) };
    }
    def Move : FixitySymbol(x : double, y : double, z : double) // an instance method 
    {
        return = FixitySymbol.FromOriginSize(this.Origin.Translate(x, y, z), this.Size, this.IsFixed); // note: the use of 'this' key word to refer to the instance 
    }
    
}
origin1 = Point.ByCoordinates(5, 5, 0); // define some appropriate input arguments 
origin2 = Point.ByCoordinates(0..10..5, 10, 0); // including a collection of points 
firstFixitySymbol = FixitySymbol.FromOriginSize(origin1, 2, true); // initially constructed 
firstFixitySymbol = firstFixitySymbol.Move(0, -4, 0); // modified by the instance method";
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
