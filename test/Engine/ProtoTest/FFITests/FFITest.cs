using System;
using System.Collections.Generic;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTestFx.TD;
namespace ProtoTest.TD.FFI
{
    class FFITest
    {
        public TestFrameWork thisTest = new TestFrameWork();
        string FFIPath = "..\\..\\..\\Scripts\\TD\\FFI\\";
        [SetUp]
        public void Setup()
        {
        }


        public void T001_FFI_MathLibrary_Sqrt_Trigonomatry()
        {

            string code = @"
class Math
{
                external (""ffi_library"") def dc_sqrt : double (val : double );
                external (""ffi_library"") def dc_sin  : double (val : double );
                external (""ffi_library"") def dc_cos  : double (val : double );
                external (""ffi_library"") def dc_tan  : double (val : double );
                external (""ffi_library"") def dc_asin : double (val : double );
                external (""ffi_library"") def dc_acos : double (val : double );
                external (""ffi_library"") def dc_atan : double (val : double );
                external (""ffi_library"") def dc_log  : double (val : double );
           
                constructor GetInstance()
                {}
                
                def Sqrt : double ( val : double )
                {
                                return = dc_sqrt(val);
                }
                def Sin : double ( val : double )
                {
                                return = dc_sin(val);
                }
                
                def Cos : double ( val : double )
                {
                                return = dc_cos(val);
                }
                
                def Tan : double ( val : double )
                {
                                return = dc_tan(val);
                }
                
                def ArcSin : double ( val : double )
                {
                                return = dc_asin(val);
                }
                
                def ArcCos : double ( val : double )
                {
                                return = dc_acos(val);
                }
                
                def ArcTan : double ( val : double )
                {
                                return = dc_atan(val);
                }
                
                def Log : double ( val : double )
                {
                                return = dc_log(val);
                }
}
[Associative]
{
                math = Math.GetInstance();
                
                sqrt_10 = math.Sqrt(10.0);
                log_100 = math.Log(100.0);
                
                angle = 30.0;
                sin_30 = math.Sin(angle);
                cos_30 = math.Cos(angle);
                tan_30 = math.Tan(angle);
                
                //            answer of each of these should be 30, 
                //            off course within tolerance of 1e-6
                asin_30 = math.ArcSin(sin_30);
                acos_30 = math.ArcCos(cos_30);
                atan_30 = math.ArcTan(tan_30);
                
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("sqrt_10", 3.162278, 1);
            thisTest.Verify("log_100", 4.605170, 1);
            thisTest.Verify("angle", 30.0, 1);
            thisTest.Verify("sin_30", 0.5, 1);
            thisTest.Verify("cos_30", 0.866025, 1);
            thisTest.Verify("tan_30", 0.577350, 1);
            thisTest.Verify("asin_30", 30.0, 1);
            thisTest.Verify("acos_30", 30.0, 1);
            thisTest.Verify("atan_30", 30.0, 1);
        }

        public void T002_FFI_Matrix_Simple()
        {
            string code = @"
class Matrix
{
    id : var;
    rows : var;
    cols : var;
    
    external (""ffi_library"") def dc_create_matrix : int (rows : int, cols : int);
    external (""ffi_library"") def dc_delete_matrix : int (id : int);
    external (""ffi_library"") def dc_set_matrix_element : double( ptr : int, row : int, col : int, value : double);
    external (""ffi_library"") def dc_get_matrix_element : double( ptr : int, row : int, col : int);
    
    constructor Create(row : int, col : int)
    {
        rows = row;
        cols = col;
        id = dc_create_matrix(rows, cols);
    }
    def SetAt : double ( row: int, col : int, value : double)
    {
        dummy = dc_set_matrix_element(id, row, col, value);
        return = dummy;
    }
    
    def GetAt : double ( row : int, col : int )
    {
        //return = dc_get_matrix_element(id, row, col);
        return = 1.0;
    }
    
        def Delete : bool ()
    {
        id = dc_delete_matrix(id);
        return = false;
    }
    
}
[Associative]
{
    mat1 = Matrix.Create(4,4);
    
    val = 1.0;
    dummy1 = mat1.SetAt(0,0,val);
    dummy2 = mat1.SetAt(1,1,val);
    dummy3 = mat1.SetAt(2,2,val);
    dummy4 = mat1.SetAt(3,3,val);
    
    val_00 = mat1.GetAt(0,0);
    val_11 = mat1.GetAt(1,1);
    val_22 = mat1.GetAt(2,2);
    val_33 = mat1.GetAt(3,3);
    
    mat2 = mat1.Delete();
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("dummy1", 1.0, 1);
            thisTest.Verify("dummy2", 1.0, 1);
            thisTest.Verify("dummy3", 1.0, 1);
            thisTest.Verify("dummy4", 1.0, 1);
            thisTest.Verify("val_00", 1.0, 1);
            thisTest.Verify("val_11", 1.0, 1);
            thisTest.Verify("val_22", 1.0, 1);
            thisTest.Verify("val_33", 1.0, 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T003_FFI_Tuple4_XYZH_Simple()
        {
            string code = @"
class Tuple4
{
    X : var;
    Y : var;
    Z : var;
    H : var;
    
    constructor XYZH(xValue : double, yValue : double, zValue : double, hValue : double)
    {
        X = xValue;
        Y = yValue;
        Z = zValue;
        H = hValue;        
    }
    
    constructor XYZ(xValue : double, yValue : double, zValue : double)
    {
        X = xValue;
        Y = yValue;
        Z = zValue;
        H = 1.0;        
    }
    constructor ByCoordinates3(coordinates : double[] )
    {
        X = coordinates[0];
        Y = coordinates[1];
        Z = coordinates[2];
        H = 1.0;        
    }
    constructor ByCoordinates4(coordinates : double[] )
    {
        X = coordinates[0];
        Y = coordinates[1];
        Z = coordinates[2];
        H = coordinates[3];    
    }
    
    def get_X : double () 
    {
        return = X;
    }
    
    def get_Y : double () 
    {
        return = Y;
    }
    
    def get_Z : double () 
    {
        return = Z;
    }
    
    def get_H : double () 
    {
        return = H;
    }
    
    public def Multiply : double(other : Tuple4)
    {
        return = X * other.X + Y * other.Y + Z * other.Z + H * other.H;
    }
    
    public def Coordinates3 : double[] ()
    { 
        return = {X, Y, Z };
    }
    
    public def Coordinates4 :double[] () 
    { 
        return = {X, Y, Z, H };
    }
    
    
}
tuple1 = Tuple4.XYZH (-10.0, -20.0, -30.0, -40);
resultX = tuple1.X;
resultY = tuple1.Y;
resultZ = tuple1.Z;
resultH = tuple1.H;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("resultX", -10.0);
            thisTest.Verify("resultY", -20.0);
            thisTest.Verify("resultZ", -30.0);
            thisTest.Verify("resultH", -40.0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T004_FFI_Tuple4_XYZ_Simple_WithGetMethods()
        {
            string code = @"
class Tuple4
{
    X : var;
    Y : var;
    Z : var;
    H : var;
    
    constructor XYZH(xValue : double, yValue : double, zValue : double, hValue : double)
    {
        X = xValue;
        Y = yValue;
        Z = zValue;
        H = hValue;        
    }
    
    constructor XYZ(xValue : double, yValue : double, zValue : double)
    {
        X = xValue;
        Y = yValue;
        Z = zValue;
        H = 1.0;        
    }
    constructor ByCoordinates3(coordinates : double[] )
    {
        X = coordinates[0];
        Y = coordinates[1];
        Z = coordinates[2];
        H = 1.0;        
    }
    constructor ByCoordinates4(coordinates : double[] )
    {
        X = coordinates[0];
        Y = coordinates[1];
        Z = coordinates[2];
        H = coordinates[3];    
    }
    
    def get_X : double () 
    {
        return = X;
    }
    
    def get_Y : double () 
    {
        return = Y;
    }
    
    def get_Z : double () 
    {
        return = Z;
    }
    
    def get_H : double () 
    {
        return = H;
    }
    
    public def Multiply : double(other : Tuple4)
    {
        return = X * other.X + Y * other.Y + Z * other.Z + H * other.H;
    }
    
    public def Coordinates3 : double[] ()
    { 
        return = {X, Y, Z };
    }
    
    public def Coordinates4 :double[] () 
    { 
        return = {X, Y, Z, H };
    }
    
    
}
tuple1 = Tuple4.XYZ (-10.0, -20.0, -30.0);
resultX = tuple1.get_X();
resultY = tuple1.get_Y();
resultZ = tuple1.get_Z();
resultH = tuple1.get_H();
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("resultX", -10.0);
            thisTest.Verify("resultY", -20.0);
            thisTest.Verify("resultZ", -30.0);
            thisTest.Verify("resultH", 1.0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T005_FFI_Tuple4_ByCoordinate3_Simple()
        {
            string code = @"
class Tuple4
{
    X : var;
    Y : var;
    Z : var;
    H : var;
    
    constructor XYZH(xValue : double, yValue : double, zValue : double, hValue : double)
    {
        X = xValue;
        Y = yValue;
        Z = zValue;
        H = hValue;        
    }
    
    constructor XYZ(xValue : double, yValue : double, zValue : double)
    {
        X = xValue;
        Y = yValue;
        Z = zValue;
        H = 1.0;        
    }
    constructor ByCoordinates3(coordinates : double[] )
    {
        X = coordinates[0];
        Y = coordinates[1];
        Z = coordinates[2];
        H = 1.0;        
    }
    constructor ByCoordinates4(coordinates : double[] )
    {
        X = coordinates[0];
        Y = coordinates[1];
        Z = coordinates[2];
        H = coordinates[3];    
    }
    
    def get_X : double () 
    {
        return = X;
    }
    
    def get_Y : double () 
    {
        return = Y;
    }
    
    def get_Z : double () 
    {
        return = Z;
    }
    
    def get_H : double () 
    {
        return = H;
    }
    
    public def Multiply : double(other : Tuple4)
    {
        return = X * other.X + Y * other.Y + Z * other.Z + H * other.H;
    }
    
    public def Coordinates3 : double[] ()
    { 
        return = {X, Y, Z };
    }
    
    public def Coordinates4 :double[] () 
    { 
        return = {X, Y, Z, H };
    }
    
    
}
cor1 = {10.0, 11.0, 12.0, 13.0};
tuple1 = Tuple4.ByCoordinates3 (cor1);
result3 = tuple1.Coordinates3();
result4 = tuple1.Coordinates4();
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object[] expectedA = { 10.0, 11.0, 12.0 };
            object[] expectedB = { 10.0, 11.0, 12.0, 1.0 };
            thisTest.Verify("result3", expectedA);
            thisTest.Verify("result4", expectedB);
        }

        [Test]
        [Category("SmokeTest")]
        public void T006_FFI_Tuple4_ByCoordinate4_Simple()
        {
            string code = @"
class Tuple4
{
    X : var;
    Y : var;
    Z : var;
    H : var;
    
    constructor XYZH(xValue : double, yValue : double, zValue : double, hValue : double)
    {
        X = xValue;
        Y = yValue;
        Z = zValue;
        H = hValue;        
    }
    
    constructor XYZ(xValue : double, yValue : double, zValue : double)
    {
        X = xValue;
        Y = yValue;
        Z = zValue;
        H = 1.0;        
    }
    constructor ByCoordinates3(coordinates : double[] )
    {
        X = coordinates[0];
        Y = coordinates[1];
        Z = coordinates[2];
        H = 1.0;        
    }
    constructor ByCoordinates4(coordinates : double[] )
    {
        X = coordinates[0];
        Y = coordinates[1];
        Z = coordinates[2];
        H = coordinates[3];    
    }
    
    def get_X : double () 
    {
        return = X;
    }
    
    def get_Y : double () 
    {
        return = Y;
    }
    
    def get_Z : double () 
    {
        return = Z;
    }
    
    def get_H : double () 
    {
        return = H;
    }
    
    public def Multiply : double(other : Tuple4)
    {
        return = X * other.X + Y * other.Y + Z * other.Z + H * other.H;
    }
    
    public def Coordinates3 : double[] ()
    { 
        return = {X, Y, Z };
    }
    
    public def Coordinates4 :double[] () 
    { 
        return = {X, Y, Z, H };
    }
    
    
}
cor1 = {10.0, 11.0, 12.0, 13.0};
tuple1 = Tuple4.ByCoordinates4 (cor1);
result3 = tuple1.Coordinates3();
result4 = tuple1.Coordinates4();
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object[] expectedA = { 10.0, 11.0, 12.0 };
            object[] expectedB = { 10.0, 11.0, 12.0, 13.0 };
            thisTest.Verify("result3", expectedA);
            thisTest.Verify("result4", expectedB);
        }

        [Test]
        [Category("SmokeTest")]
        public void T007_FFI_Tuple4_Multiply_Simple()
        {
            string code = @"
class Tuple4
{
    X : var;
    Y : var;
    Z : var;
    H : var;
    
    constructor XYZH(xValue : double, yValue : double, zValue : double, hValue : double)
    {
        X = xValue;
        Y = yValue;
        Z = zValue;
        H = hValue;        
    }
    
    constructor XYZ(xValue : double, yValue : double, zValue : double)
    {
        X = xValue;
        Y = yValue;
        Z = zValue;
        H = 1.0;        
    }
    constructor ByCoordinates3(coordinates : double[] )
    {
        X = coordinates[0];
        Y = coordinates[1];
        Z = coordinates[2];
        H = 1.0;        
    }
    constructor ByCoordinates4(coordinates : double[] )
    {
        X = coordinates[0];
        Y = coordinates[1];
        Z = coordinates[2];
        H = coordinates[3];    
    }
    
    def get_X : double () 
    {
        return = X;
    }
    
    def get_Y : double () 
    {
        return = Y;
    }
    
    def get_Z : double () 
    {
        return = Z;
    }
    
    def get_H : double () 
    {
        return = H;
    }
    
    public def Multiply : double(other : Tuple4)
    {
        return = X * other.X + Y * other.Y + Z * other.Z + H * other.H;
    }
    
    public def Coordinates3 : double[] ()
    { 
        return = {X, Y, Z };
    }
    
    public def Coordinates4 :double[] () 
    { 
        return = {X, Y, Z, H };
    }
    
    
}
cor1 = {10.0, 10.0, 10.0, 10.0};
cor2 = {10.0, 10.0, 10.0, 10.0};
tuple1 = Tuple4.ByCoordinates4 (cor1);
tuple2 = Tuple4.ByCoordinates4 (cor2);
result1 = tuple1.Coordinates4();
result2 = tuple2.Coordinates4();
multiply = tuple1.Multiply(tuple2);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("multiply", 400.0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T008_FFI_Transform_ByDate_Simple()
        {

            string code = @"
class Tuple4
{
    X : var;
    Y : var;
    Z : var;
    H : var;
    
    constructor XYZH(xValue : double, yValue : double, zValue : double, hValue : double)
    {
        X = xValue;
        Y = yValue;
        Z = zValue;
        H = hValue;        
    }
    
    constructor XYZ(xValue : double, yValue : double, zValue : double)
    {
        X = xValue;
        Y = yValue;
        Z = zValue;
        H = 1.0;        
    }
    constructor ByCoordinates3(coordinates : double[] )
    {
        X = coordinates[0];
        Y = coordinates[1];
        Z = coordinates[2];
        H = 1.0;        
    }
    constructor ByCoordinates4(coordinates : double[] )
    {
        X = coordinates[0];
        Y = coordinates[1];
        Z = coordinates[2];
        H = coordinates[3];    
    }
    
    def get_X : double () 
    {
        return = X;
    }
    
    def get_Y : double () 
    {
        return = Y;
    }
    
    def get_Z : double () 
    {
        return = Z;
    }
    
    def get_H : double () 
    {
        return = H;
    }
    
    public def Multiply : double (other : Tuple4)
    {
        return = X * other.X + Y * other.Y + Z * other.Z + H * other.H;
    }
    
    public def Coordinates3 : double[] ()
    { 
        return = {X, Y, Z };
    }
    
    public def Coordinates4 : double[] () 
    { 
        return = {X, Y, Z, H };
    }
}
class Vector
{
    X : var;
    Y : var;
    Z : var;
    
    public constructor ByCoordinates(xx : double, yy : double, zz : double)
    {
        X = xx;
        Y = yy;
        Z = zz;
    }
}
class Transform
{
    public C0 : Tuple4; 
    public C1 : Tuple4; 
    public C2 : Tuple4; 
    public C3 : Tuple4;     
    
    public constructor ByTuples(C0Value : Tuple4, C1Value : Tuple4, C2Value : Tuple4, C3Value : Tuple4)
    {
        C0 = C0Value;
        C1 = C1Value;
        C2 = C2Value;
        C3 = C3Value;
    }
    public constructor ByData(data : double[][])
    {
        C0 = Tuple4.ByCoordinates4(data[0]);
        C1 = Tuple4.ByCoordinates4(data[1]);
        C2 = Tuple4.ByCoordinates4(data[2]);
        C3 = Tuple4.ByCoordinates4(data[3]);
    }
    
    public def ApplyTransform : Tuple4 (t : Tuple4)
    {
        tx = Tuple4.XYZH(C0.X, C1.X, C2.X, C3.X);
        RX = tx.Multiply(t);
        ty = Tuple4.XYZH(C0.Y, C1.Y, C2.Y, C3.Y);
        RY = ty.Multiply(t);
        tz = Tuple4.XYZH(C0.Z, C1.Z, C2.Z, C3.Z);
        RZ = tz.Multiply(t);
        th = Tuple4.XYZH(C0.H, C1.H, C2.H, C3.H);
        RH = th.Multiply(t);
        
        return = Tuple4.XYZH(RX, RY, RZ, RH);
    }
    
    public def TransformVector : Vector (p: Vector)
    {    
        tpa = Tuple4.XYZH(p.X, p.Y, p.Z, 0.0);
        tpcv = ApplyTransform(tpa);
        return = Vector.ByCoordinates(tpcv.X, tpcv.Y, tpcv.Z);    
    }
}
data = {    {1.0,0.0,0.0,0.0},
            {0.0,1.0,0.0,0.0},
            {0.0,0.0,1.0,0.0},
            {0.0,0.0,0.0,1.0}
        };
        
xform = Transform.ByData(data);
c0 = xform.C0;
c1 = xform.C1;
c2 = xform.C2;
c3 = xform.C3;
c0_X = c0.X;
c0_Y = c0.Y;
c0_Z = c0.Z;
c0_H = c0.H;
c1_X = c1.X;
c1_Y = c1.Y;
c1_Z = c1.Z;
c1_H = c1.H;
c2_X = c2.X;
c2_Y = c2.Y;
c2_Z = c2.Z;
c2_H = c2.H;
c3_X = c3.X;
c3_Y = c3.Y;
c3_Z = c3.Z;
c3_H = c3.H;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c0_X", 1.0);
            thisTest.Verify("c0_Y", 0.0);
            thisTest.Verify("c0_Z", 0.0);
            thisTest.Verify("c0_H", 0.0);
            thisTest.Verify("c1_X", 0.0);
            thisTest.Verify("c1_Y", 1.0);
            thisTest.Verify("c1_Z", 0.0);
            thisTest.Verify("c1_H", 0.0);
            thisTest.Verify("c2_X", 0.0);
            thisTest.Verify("c2_Y", 0.0);
            thisTest.Verify("c2_Z", 1.0);
            thisTest.Verify("c2_H", 0.0);
            thisTest.Verify("c3_X", 0.0);
            thisTest.Verify("c3_Y", 0.0);
            thisTest.Verify("c3_Z", 0.0);
            thisTest.Verify("c3_H", 1.0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T009_FFI_Transform_ByTuples_Simple()
        {
            string code = @"
class Tuple4
{
    X : var;
    Y : var;
    Z : var;
    H : var;
    
    constructor XYZH(xValue : double, yValue : double, zValue : double, hValue : double)
    {
        X = xValue;
        Y = yValue;
        Z = zValue;
        H = hValue;        
    }
    
    constructor XYZ(xValue : double, yValue : double, zValue : double)
    {
        X = xValue;
        Y = yValue;
        Z = zValue;
        H = 1.0;        
    }
    constructor ByCoordinates3(coordinates : double[] )
    {
        X = coordinates[0];
        Y = coordinates[1];
        Z = coordinates[2];
        H = 1.0;        
    }
    constructor ByCoordinates4(coordinates : double[] )
    {
        X = coordinates[0];
        Y = coordinates[1];
        Z = coordinates[2];
        H = coordinates[3];    
    }
    
    def get_X : double () 
    {
        return = X;
    }
    
    def get_Y : double () 
    {
        return = Y;
    }
    
    def get_Z : double () 
    {
        return = Z;
    }
    
    def get_H : double () 
    {
        return = H;
    }
    
    public def Multiply : double (other : Tuple4)
    {
        return = X * other.X + Y * other.Y + Z * other.Z + H * other.H;
    }
    
    public def Coordinates3 : double[] ()
    { 
        return = {X, Y, Z };
    }
    
    public def Coordinates4 : double[] () 
    { 
        return = {X, Y, Z, H };
    }
}
class Vector
{
    X : var;
    Y : var;
    Z : var;
    
    public constructor ByCoordinates(xx : double, yy : double, zz : double)
    {
        X = xx;
        Y = yy;
        Z = zz;
    }
}
class Transform
{
    public C0 : Tuple4; 
    public C1 : Tuple4; 
    public C2 : Tuple4; 
    public C3 : Tuple4;     
    
    public constructor ByTuples(C0Value : Tuple4, C1Value : Tuple4, C2Value : Tuple4, C3Value : Tuple4)
    {
        C0 = C0Value;
        C1 = C1Value;
        C2 = C2Value;
        C3 = C3Value;
    }
    public constructor ByData(data : double[][])
    {
        C0 = Tuple4.ByCoordinates4(data[0]);
        C1 = Tuple4.ByCoordinates4(data[1]);
        C2 = Tuple4.ByCoordinates4(data[2]);
        C3 = Tuple4.ByCoordinates4(data[3]);
    }
    
    public def ApplyTransform : Tuple4 (t : Tuple4)
    {
        tx = Tuple4.XYZH(C0.X, C1.X, C2.X, C3.X);
        RX = tx.Multiply(t);
        ty = Tuple4.XYZH(C0.Y, C1.Y, C2.Y, C3.Y);
        RY = ty.Multiply(t);
        tz = Tuple4.XYZH(C0.Z, C1.Z, C2.Z, C3.Z);
        RZ = tz.Multiply(t);
        th = Tuple4.XYZH(C0.H, C1.H, C2.H, C3.H);
        RH = th.Multiply(t);
        
        return = Tuple4.XYZH(RX, RY, RZ, RH);
    }
    
    public def TransformVector : Vector (p: Vector)
    {    
        tpa = Tuple4.XYZH(p.X, p.Y, p.Z, 0.0);
        tpcv = ApplyTransform(tpa);
        return = Vector.ByCoordinates(tpcv.X, tpcv.Y, tpcv.Z);    
    }
}
data = {    {1.0,0.0,0.0,0.0},
            {0.0,1.0,0.0,0.0},
            {0.0,0.0,1.0,0.0},
            {0.0,0.0,0.0,1.0}
        };
        
t0 = Tuple4.ByCoordinates4(data[0]);
t1 = Tuple4.ByCoordinates4(data[1]);
t2 = Tuple4.ByCoordinates4(data[2]);
t3 = Tuple4.ByCoordinates4(data[3]);
xform = Transform.ByTuples(t0, t1, t2, t3);
c0 = xform.C0;
c1 = xform.C1;
c2 = xform.C2;
c3 = xform.C3;
c0_X = c0.X;
c0_Y = c0.Y;
c0_Z = c0.Z;
c0_H = c0.H;
c1_X = c1.X;
c1_Y = c1.Y;
c1_Z = c1.Z;
c1_H = c1.H;
c2_X = c2.X;
c2_Y = c2.Y;
c2_Z = c2.Z;
c2_H = c2.H;
c3_X = c3.X;
c3_Y = c3.Y;
c3_Z = c3.Z;
c3_H = c3.H;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c0_X", 1.0);
            thisTest.Verify("c0_Y", 0.0);
            thisTest.Verify("c0_Z", 0.0);
            thisTest.Verify("c0_H", 0.0);
            thisTest.Verify("c1_X", 0.0);
            thisTest.Verify("c1_Y", 1.0);
            thisTest.Verify("c1_Z", 0.0);
            thisTest.Verify("c1_H", 0.0);
            thisTest.Verify("c2_X", 0.0);
            thisTest.Verify("c2_Y", 0.0);
            thisTest.Verify("c2_Z", 1.0);
            thisTest.Verify("c2_H", 0.0);
            thisTest.Verify("c3_X", 0.0);
            thisTest.Verify("c3_Y", 0.0);
            thisTest.Verify("c3_Z", 0.0);
            thisTest.Verify("c3_H", 1.0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T010_FFI_Transform_ApplyTransform()
        {
            string code = @"
class Tuple4
{
    X : var;
    Y : var;
    Z : var;
    H : var;
    
    constructor XYZH(xValue : double, yValue : double, zValue : double, hValue : double)
    {
        X = xValue;
        Y = yValue;
        Z = zValue;
        H = hValue;        
    }
    
    constructor XYZ(xValue : double, yValue : double, zValue : double)
    {
        X = xValue;
        Y = yValue;
        Z = zValue;
        H = 1.0;        
    }
    constructor ByCoordinates3(coordinates : double[] )
    {
        X = coordinates[0];
        Y = coordinates[1];
        Z = coordinates[2];
        H = 1.0;        
    }
    constructor ByCoordinates4(coordinates : double[] )
    {
        X = coordinates[0];
        Y = coordinates[1];
        Z = coordinates[2];
        H = coordinates[3];    
    }
    
    def get_X : double () 
    {
        return = X;
    }
    
    def get_Y : double () 
    {
        return = Y;
    }
    
    def get_Z : double () 
    {
        return = Z;
    }
    
    def get_H : double () 
    {
        return = H;
    }
    
    public def Multiply : double (other : Tuple4)
    {
        return = X * other.X + Y * other.Y + Z * other.Z + H * other.H;
    }
    
    public def Coordinates3 : double[] ()
    { 
        return = {X, Y, Z };
    }
    
    public def Coordinates4 : double[] () 
    { 
        return = {X, Y, Z, H };
    }
}
/*
t1 = Tuple4.XYZH(0,0,0,0);
t2 = Tuple4.XYZ(0,0,0);
t3 = Tuple4.ByCoordinates3({0.0,0,0});
t4 = Tuple4.ByCoordinates4({0.0,0,0,0});
mult = t1.Multiply(t2);
c3 = t3.Coordinates3();
c4 = t3.Coordinates4();
*/
class Vector
{
    X : var;
    Y : var;
    Z : var;
    
    public constructor ByCoordinates(xx : double, yy : double, zz : double)
    {
        X = xx;
        Y = yy;
        Z = zz;
    }
}
class Transform
{
    public C0 : Tuple4; 
    public C1 : Tuple4; 
    public C2 : Tuple4; 
    public C3 : Tuple4;     
    
    public constructor ByTuples(C0Value : Tuple4, C1Value : Tuple4, C2Value : Tuple4, C3Value : Tuple4)
    {
        C0 = C0Value;
        C1 = C1Value;
        C2 = C2Value;
        C3 = C3Value;
    }
    public constructor ByData(data : double[][])
    {
        C0 = Tuple4.ByCoordinates4(data[0]);
        C1 = Tuple4.ByCoordinates4(data[1]);
        C2 = Tuple4.ByCoordinates4(data[2]);
        C3 = Tuple4.ByCoordinates4(data[3]);
    }
    
    public def ApplyTransform : Tuple4 (t : Tuple4)
    {
        tx = Tuple4.XYZH(C0.X, C1.X, C2.X, C3.X);
        RX = tx.Multiply(t);
        ty = Tuple4.XYZH(C0.Y, C1.Y, C2.Y, C3.Y);
        RY = ty.Multiply(t);
        tz = Tuple4.XYZH(C0.Z, C1.Z, C2.Z, C3.Z);
        RZ = tz.Multiply(t);
        th = Tuple4.XYZH(C0.H, C1.H, C2.H, C3.H);
        RH = th.Multiply(t);
        
        return = Tuple4.XYZH(RX, RY, RZ, RH);
    }
	
    public def NativeMultiply : Transform(other : Transform)
    {              
        tc0 = ApplyTransform(other.C0);
        tc1 = ApplyTransform(other.C1);
        tc2 = ApplyTransform(other.C2);
        tc3 = ApplyTransform(other.C3);
        return = Transform.ByTuples(tc0, tc1, tc2, tc3);
    }
    
    public def NativePreMultiply : Transform (other : Transform)
    {     
        //  as we don't have this now let's do it longer way!        
        //return = other.NativeMultiply(this);
        //
        tc0 = other.ApplyTransform(C0);
        tc1 = other.ApplyTransform(C1);
        tc2 = other.ApplyTransform(C2);
        tc3 = other.ApplyTransform(C3);
        return = Transform.ByTuples(tc0, tc1, tc2, tc3);
    }
    
}
data = {    {2.0,0.0,0.0,0.0},
            {0.0,2.0,0.0,0.0},
            {0.0,0.0,2.0,0.0},
            {0.0,0.0,0.0,2.0}
        };
        
  
        
xform = Transform.ByData(data);
tuple = Tuple4.XYZH(0.1,2,4,1);
result = xform.ApplyTransform(tuple);
x = result.X;
y = result.Y;
z = result.Z;
h = result.H;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 0.2);
            thisTest.Verify("y", 4.0);
            thisTest.Verify("z", 8.0);
            thisTest.Verify("h", 2.0);

        }

        [Test]
        [Category("SmokeTest")]
        public void T011_FFI_Transform_NativeMultiply()
        {
            string code = @"
class Tuple4
{
    X : var;
    Y : var;
    Z : var;
    H : var;
    
    constructor XYZH(xValue : double, yValue : double, zValue : double, hValue : double)
    {
        X = xValue;
        Y = yValue;
        Z = zValue;
        H = hValue;        
    }
    
    constructor XYZ(xValue : double, yValue : double, zValue : double)
    {
        X = xValue;
        Y = yValue;
        Z = zValue;
        H = 1.0;        
    }
    constructor ByCoordinates3(coordinates : double[] )
    {
        X = coordinates[0];
        Y = coordinates[1];
        Z = coordinates[2];
        H = 1.0;        
    }
    constructor ByCoordinates4(coordinates : double[] )
    {
        X = coordinates[0];
        Y = coordinates[1];
        Z = coordinates[2];
        H = coordinates[3];    
    }
    
    def get_X : double () 
    {
        return = X;
    }
    
    def get_Y : double () 
    {
        return = Y;
    }
    
    def get_Z : double () 
    {
        return = Z;
    }
    
    def get_H : double () 
    {
        return = H;
    }
    
    public def Multiply : double (other : Tuple4)
    {
        return = X * other.X + Y * other.Y + Z * other.Z + H * other.H;
    }
    
    public def Coordinates3 : double[] ()
    { 
        return = {X, Y, Z };
    }
    
    public def Coordinates4 : double[] () 
    { 
        return = {X, Y, Z, H };
    }
}
class Vector
{
    X : var;
    Y : var;
    Z : var;
    
    public constructor ByCoordinates(xx : double, yy : double, zz : double)
    {
        X = xx;
        Y = yy;
        Z = zz;
    }
}
class Transform
{
    public C0 : Tuple4; 
    public C1 : Tuple4; 
    public C2 : Tuple4; 
    public C3 : Tuple4;     
    
    public constructor ByTuples(C0Value : Tuple4, C1Value : Tuple4, C2Value : Tuple4, C3Value : Tuple4)
    {
        C0 = C0Value;
        C1 = C1Value;
        C2 = C2Value;
        C3 = C3Value;
    }
    public constructor ByData(data : double[][])
    {
        C0 = Tuple4.ByCoordinates4(data[0]);
        C1 = Tuple4.ByCoordinates4(data[1]);
        C2 = Tuple4.ByCoordinates4(data[2]);
        C3 = Tuple4.ByCoordinates4(data[3]);
    }
    
    public def ApplyTransform : Tuple4 (t : Tuple4)
    {
        tx = Tuple4.XYZH(C0.X, C1.X, C2.X, C3.X);
        RX = tx.Multiply(t);
        ty = Tuple4.XYZH(C0.Y, C1.Y, C2.Y, C3.Y);
        RY = ty.Multiply(t);
        tz = Tuple4.XYZH(C0.Z, C1.Z, C2.Z, C3.Z);
        RZ = tz.Multiply(t);
        th = Tuple4.XYZH(C0.H, C1.H, C2.H, C3.H);
        RH = th.Multiply(t);
        
        return = Tuple4.XYZH(RX, RY, RZ, RH);
    }
	
    public def NativeMultiply : Transform(other : Transform)
    {              
        tc0 = ApplyTransform(other.C0);
        tc1 = ApplyTransform(other.C1);
        tc2 = ApplyTransform(other.C2);
        tc3 = ApplyTransform(other.C3);
        return = Transform.ByTuples(tc0, tc1, tc2, tc3);
    }
    
    public def NativePreMultiply : Transform (other : Transform)
    {     
        //  as we don't have this now let's do it longer way!        
        //return = other.NativeMultiply(this);
        //
        tc0 = other.ApplyTransform(C0);
        tc1 = other.ApplyTransform(C1);
        tc2 = other.ApplyTransform(C2);
        tc3 = other.ApplyTransform(C3);
        return = Transform.ByTuples(tc0, tc1, tc2, tc3);
    }
    
}
data1 = {    {2.0,0.0,0.0,0.0},
            {0.0,2.0,0.0,0.0},
            {0.0,0.0,2.0,0.0},
            {0.0,0.0,0.0,2.0}
        };
data2 = {    {0.0,3.0,0.0,0.0},
            {0.0,2.0,0.0,0.0},
            {1.0,0.0,2.0,0.0},
            {0.5,0.0,0.0,2.0}
        };        
  
        
xform1 = Transform.ByData(data1);
xform2 = Transform.ByData(data2);
result = xform1.NativeMultiply(xform2);
r0 = result.C0;
r1 = result.C1;
r2 = result.C2;
r3 = result.C3;
r0X = r0.X;
r0Y = r0.Y;
r0Z = r0.Z;
r0H = r0.H;
r1X = r1.X;
r1Y = r1.Y;
r1Z = r1.Z;
r1H = r1.H;
r2X = r2.X;
r2Y = r2.Y;
r2Z = r2.Z;
r2H = r2.H;
r3X = r3.X;
r3Y = r3.Y;
r3Z = r3.Z;
r3H = r3.H;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r0X", 0.0);
            thisTest.Verify("r0Y", 6.0);
            thisTest.Verify("r0Z", 0.0);
            thisTest.Verify("r0H", 0.0);
            thisTest.Verify("r1X", 0.0);
            thisTest.Verify("r1Y", 4.0);
            thisTest.Verify("r1Z", 0.0);
            thisTest.Verify("r1H", 0.0);
            thisTest.Verify("r2X", 2.0);
            thisTest.Verify("r2Y", 0.0);
            thisTest.Verify("r2Z", 4.0);
            thisTest.Verify("r2H", 0.0);
            thisTest.Verify("r3X", 1.0);
            thisTest.Verify("r3Y", 0.0);
            thisTest.Verify("r3Z", 0.0);
            thisTest.Verify("r3H", 4.0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T012_FFI_Transform_NativePreMultiply()
        {
            string code = @"
class Tuple4
{
    X : var;
    Y : var;
    Z : var;
    H : var;
    
    constructor XYZH(xValue : double, yValue : double, zValue : double, hValue : double)
    {
        X = xValue;
        Y = yValue;
        Z = zValue;
        H = hValue;        
    }
    
    constructor XYZ(xValue : double, yValue : double, zValue : double)
    {
        X = xValue;
        Y = yValue;
        Z = zValue;
        H = 1.0;        
    }
    constructor ByCoordinates3(coordinates : double[] )
    {
        X = coordinates[0];
        Y = coordinates[1];
        Z = coordinates[2];
        H = 1.0;        
    }
    constructor ByCoordinates4(coordinates : double[] )
    {
        X = coordinates[0];
        Y = coordinates[1];
        Z = coordinates[2];
        H = coordinates[3];    
    }
    
    def get_X : double () 
    {
        return = X;
    }
    
    def get_Y : double () 
    {
        return = Y;
    }
    
    def get_Z : double () 
    {
        return = Z;
    }
    
    def get_H : double () 
    {
        return = H;
    }
    
    public def Multiply : double (other : Tuple4)
    {
        return = X * other.X + Y * other.Y + Z * other.Z + H * other.H;
    }
    
    public def Coordinates3 : double[] ()
    { 
        return = {X, Y, Z };
    }
    
    public def Coordinates4 : double[] () 
    { 
        return = {X, Y, Z, H };
    }
}
class Vector
{
    X : var;
    Y : var;
    Z : var;
    
    public constructor ByCoordinates(xx : double, yy : double, zz : double)
    {
        X = xx;
        Y = yy;
        Z = zz;
    }
}
class Transform
{
    public C0 : Tuple4; 
    public C1 : Tuple4; 
    public C2 : Tuple4; 
    public C3 : Tuple4;     
    
    public constructor ByTuples(C0Value : Tuple4, C1Value : Tuple4, C2Value : Tuple4, C3Value : Tuple4)
    {
        C0 = C0Value;
        C1 = C1Value;
        C2 = C2Value;
        C3 = C3Value;
    }
    public constructor ByData(data : double[][])
    {
        C0 = Tuple4.ByCoordinates4(data[0]);
        C1 = Tuple4.ByCoordinates4(data[1]);
        C2 = Tuple4.ByCoordinates4(data[2]);
        C3 = Tuple4.ByCoordinates4(data[3]);
    }
    
    public def ApplyTransform : Tuple4 (t : Tuple4)
    {
        tx = Tuple4.XYZH(C0.X, C1.X, C2.X, C3.X);
        RX = tx.Multiply(t);
        ty = Tuple4.XYZH(C0.Y, C1.Y, C2.Y, C3.Y);
        RY = ty.Multiply(t);
        tz = Tuple4.XYZH(C0.Z, C1.Z, C2.Z, C3.Z);
        RZ = tz.Multiply(t);
        th = Tuple4.XYZH(C0.H, C1.H, C2.H, C3.H);
        RH = th.Multiply(t);
        
        return = Tuple4.XYZH(RX, RY, RZ, RH);
    }
	
    public def NativeMultiply : Transform(other : Transform)
    {              
        tc0 = ApplyTransform(other.C0);
        tc1 = ApplyTransform(other.C1);
        tc2 = ApplyTransform(other.C2);
        tc3 = ApplyTransform(other.C3);
        return = Transform.ByTuples(tc0, tc1, tc2, tc3);
    }
    
    public def NativePreMultiply : Transform (other : Transform)
    {     
        //  as we don't have this now let's do it longer way!        
        //return = other.NativeMultiply(this);
        //
        tc0 = other.ApplyTransform(C0);
        tc1 = other.ApplyTransform(C1);
        tc2 = other.ApplyTransform(C2);
        tc3 = other.ApplyTransform(C3);
        return = Transform.ByTuples(tc0, tc1, tc2, tc3);
    }
    
}
data1 = {    {2.0,0.0,0.0,0.0},
            {0.0,2.0,0.0,0.0},
            {0.0,0.0,2.0,0.0},
            {0.0,0.0,3.0,3.0}
        };
data2 = {    {0.0,3.0,0.0,0.0},
            {0.0,2.0,0.0,0.0},
            {1.0,0.0,2.0,0.0},
            {0.5,0.0,0.0,2.0}
        };        
  
        
xform1 = Transform.ByData(data1);
xform2 = Transform.ByData(data2);
result = xform1.NativePreMultiply(xform2);
r0 = result.C0;
r1 = result.C1;
r2 = result.C2;
r3 = result.C3;
r0X = r0.X;
r0Y = r0.Y;
r0Z = r0.Z;
r0H = r0.H;
r1X = r1.X;
r1Y = r1.Y;
r1Z = r1.Z;
r1H = r1.H;
r2X = r2.X;
r2Y = r2.Y;
r2Z = r2.Z;
r2H = r2.H;
r3X = r3.X;
r3Y = r3.Y;
r3Z = r3.Z;
r3H = r3.H;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r0X", 0.0);
            thisTest.Verify("r0Y", 6.0);
            thisTest.Verify("r0Z", 0.0);
            thisTest.Verify("r0H", 0.0);
            thisTest.Verify("r1X", 0.0);
            thisTest.Verify("r1Y", 4.0);
            thisTest.Verify("r1Z", 0.0);
            thisTest.Verify("r1H", 0.0);
            thisTest.Verify("r2X", 2.0);
            thisTest.Verify("r2Y", 0.0);
            thisTest.Verify("r2Z", 4.0);
            thisTest.Verify("r2H", 0.0);
            thisTest.Verify("r3X", 4.5);
            thisTest.Verify("r3Y", 0.0);
            thisTest.Verify("r3Z", 6.0);
            thisTest.Verify("r3H", 6.0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T013_FFI_Transform_TransformVector()
        {
            string code = @"
class Tuple4
{
    X : var;
    Y : var;
    Z : var;
    H : var;
    
    constructor XYZH(xValue : double, yValue : double, zValue : double, hValue : double)
    {
        X = xValue;
        Y = yValue;
        Z = zValue;
        H = hValue;        
    }
    
    constructor XYZ(xValue : double, yValue : double, zValue : double)
    {
        X = xValue;
        Y = yValue;
        Z = zValue;
        H = 1.0;        
    }
    constructor ByCoordinates3(coordinates : double[] )
    {
        X = coordinates[0];
        Y = coordinates[1];
        Z = coordinates[2];
        H = 1.0;        
    }
    constructor ByCoordinates4(coordinates : double[] )
    {
        X = coordinates[0];
        Y = coordinates[1];
        Z = coordinates[2];
        H = coordinates[3];    
    }
    
    def get_X : double () 
    {
        return = X;
    }
    
    def get_Y : double () 
    {
        return = Y;
    }
    
    def get_Z : double () 
    {
        return = Z;
    }
    
    def get_H : double () 
    {
        return = H;
    }
    
    public def Multiply : double (other : Tuple4)
    {
        return = X * other.X + Y * other.Y + Z * other.Z + H * other.H;
    }
    
    public def Coordinates3 : double[] ()
    { 
        return = {X, Y, Z };
    }
    
    public def Coordinates4 : double[] () 
    { 
        return = {X, Y, Z, H };
    }
}
/*
t1 = Tuple4.XYZH(0,0,0,0);
t2 = Tuple4.XYZ(0,0,0);
t3 = Tuple4.ByCoordinates3({0.0,0,0});
t4 = Tuple4.ByCoordinates4({0.0,0,0,0});
mult = t1.Multiply(t2);
c3 = t3.Coordinates3();
c4 = t3.Coordinates4();
*/
class Vector
{
    X : var;
    Y : var;
    Z : var;
    
    public constructor ByCoordinates(xx : double, yy : double, zz : double)
    {
        X = xx;
        Y = yy;
        Z = zz;
    }
}
class Transform
{
    public C0 : Tuple4; 
    public C1 : Tuple4; 
    public C2 : Tuple4; 
    public C3 : Tuple4;     
    
    public constructor ByTuples(C0Value : Tuple4, C1Value : Tuple4, C2Value : Tuple4, C3Value : Tuple4)
    {
        C0 = C0Value;
        C1 = C1Value;
        C2 = C2Value;
        C3 = C3Value;
    }
    public constructor ByData(data : double[][])
    {
        C0 = Tuple4.ByCoordinates4(data[0]);
        C1 = Tuple4.ByCoordinates4(data[1]);
        C2 = Tuple4.ByCoordinates4(data[2]);
        C3 = Tuple4.ByCoordinates4(data[3]);
    }
    
    public def ApplyTransform : Tuple4 (t : Tuple4)
    {
        tx = Tuple4.XYZH(C0.X, C1.X, C2.X, C3.X);
        RX = tx.Multiply(t);
        ty = Tuple4.XYZH(C0.Y, C1.Y, C2.Y, C3.Y);
        RY = ty.Multiply(t);
        tz = Tuple4.XYZH(C0.Z, C1.Z, C2.Z, C3.Z);
        RZ = tz.Multiply(t);
        th = Tuple4.XYZH(C0.H, C1.H, C2.H, C3.H);
        RH = th.Multiply(t);
        
        return = Tuple4.XYZH(RX, RY, RZ, RH);
    }
    
    public def TransformVector : Vector (p: Vector)
    {    
        tpa = Tuple4.XYZH(p.X, p.Y, p.Z, 0.0);
        tpcv = ApplyTransform(tpa);
        return = Vector.ByCoordinates(tpcv.X, tpcv.Y, tpcv.Z);    
    }
    
    public def NativeMultiply : Transform(other : Transform)
    {              
        tc0 = ApplyTransform(other.C0);
        tc1 = ApplyTransform(other.C1);
        tc2 = ApplyTransform(other.C2);
        tc3 = ApplyTransform(other.C3);
        return = Transform.ByTuples(tc0, tc1, tc2, tc3);
    }
    
    public def NativePreMultiply : Transform (other : Transform)
    {     
        //  as we don't have this now let's do it longer way!        
        //return = other.NativeMultiply(this);
        //
        tc0 = other.ApplyTransform(C0);
        tc1 = other.ApplyTransform(C1);
        tc2 = other.ApplyTransform(C2);
        tc3 = other.ApplyTransform(C3);
        return = Transform.ByTuples(tc0, tc1, tc2, tc3);
    }
    
}
data = {    {1.0,0.0,0.0,0.0},
            {0.0,2.0,0.0,0.0},
            {0.0,0.0,3.0,0.0},
            {0.0,0.0,0.0,4.0}
        };
   
xform = Transform.ByData(data);
testVector = Vector.ByCoordinates (10, 20, 30);
resultVector = xform.TransformVector (testVector);
x = testVector.X;
y = testVector.Y;
z = testVector.Z;
resultx = resultVector.X;
resulty = resultVector.Y;
resultz = resultVector.Z;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("resultx", 10.0);
            thisTest.Verify("resulty", 40.0);
            thisTest.Verify("resultz", 90.0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T014_FFI_Transform_TransformPoint()
        {
            string code = @"
class Geometry
{
    private hostEntityID : var;
}
class Point extends Geometry
{
   
    public X                        : var; //double = GlobalCoordinates[0];
    public Y                        : var; //double = GlobalCoordinates[1];
    public Z                        : var; //double = GlobalCoordinates[2];
    private def init : bool ()
    {
       
        X                      =  0.0;
        Y                      =  0.0;
        Z                      =  0.0;
   
        return = true;
    }
    
   
	
	public constructor ByCoordinates(xTranslation : double, yTranslation : double, zTranslation : double)
    {
        neglect = init();
        X = xTranslation;
        Y = yTranslation;
        Z = zTranslation;
    }
}
class Tuple4
{
    X : var;
    Y : var;
    Z : var;
    H : var;
    
    constructor XYZH(xValue : double, yValue : double, zValue : double, hValue : double)
    {
        X = xValue;
        Y = yValue;
        Z = zValue;
        H = hValue;        
    }
    
    constructor XYZ(xValue : double, yValue : double, zValue : double)
    {
        X = xValue;
        Y = yValue;
        Z = zValue;
        H = 1.0;        
    }
    constructor ByCoordinates3(coordinates : double[] )
    {
        X = coordinates[0];
        Y = coordinates[1];
        Z = coordinates[2];
        H = 1.0;        
    }
    constructor ByCoordinates4(coordinates : double[] )
    {
        X = coordinates[0];
        Y = coordinates[1];
        Z = coordinates[2];
        H = coordinates[3];    
    }
    
    def get_X : double () 
    {
        return = X;
    }
    
    def get_Y : double () 
    {
        return = Y;
    }
    
    def get_Z : double () 
    {
        return = Z;
    }
    
    def get_H : double () 
    {
        return = H;
    }
    
    public def Multiply : double (other : Tuple4)
    {
        return = X * other.X + Y * other.Y + Z * other.Z + H * other.H;
    }
    
    public def Coordinates3 : double[] ()
    { 
        return = {X, Y, Z };
    }
    
    public def Coordinates4 : double[] () 
    { 
        return = {X, Y, Z, H };
    }
}
class Vector
{
    X : var;
    Y : var;
    Z : var;
    
    public constructor ByCoordinates(xx : double, yy : double, zz : double)
    {
        X = xx;
        Y = yy;
        Z = zz;
    }
}
class Transform
{
    public C0 : Tuple4; 
    public C1 : Tuple4; 
    public C2 : Tuple4; 
    public C3 : Tuple4;     
    
    public constructor ByTuples(C0Value : Tuple4, C1Value : Tuple4, C2Value : Tuple4, C3Value : Tuple4)
    {
        C0 = C0Value;
        C1 = C1Value;
        C2 = C2Value;
        C3 = C3Value;
    }
    public constructor ByData(data : double[][])
    {
        C0 = Tuple4.ByCoordinates4(data[0]);
        C1 = Tuple4.ByCoordinates4(data[1]);
        C2 = Tuple4.ByCoordinates4(data[2]);
        C3 = Tuple4.ByCoordinates4(data[3]);
    }
    
    public def ApplyTransform : Tuple4 (t : Tuple4)
    {
        tx = Tuple4.XYZH(C0.X, C1.X, C2.X, C3.X);
        RX = tx.Multiply(t);
        ty = Tuple4.XYZH(C0.Y, C1.Y, C2.Y, C3.Y);
        RY = ty.Multiply(t);
        tz = Tuple4.XYZH(C0.Z, C1.Z, C2.Z, C3.Z);
        RZ = tz.Multiply(t);
        th = Tuple4.XYZH(C0.H, C1.H, C2.H, C3.H);
        RH = th.Multiply(t);
        
        return = Tuple4.XYZH(RX, RY, RZ, RH);
    }
    public def TransformPoint : Point (p: Point)
    {
        tpa = Tuple4.XYZH(p.X, p.Y, p.Z, 1.0);
        tpcv = ApplyTransform(tpa);
        return = Point.ByCoordinates(tpcv.X, tpcv.Y, tpcv.Z);	
    }
    public def NativeMultiply : Transform(other : Transform)
    {              
        tc0 = ApplyTransform(other.C0);
        tc1 = ApplyTransform(other.C1);
        tc2 = ApplyTransform(other.C2);
        tc3 = ApplyTransform(other.C3);
        return = Transform.ByTuples(tc0, tc1, tc2, tc3);
    }
    
    public def NativePreMultiply : Transform (other : Transform)
    {     
        //  as we don't have this now let's do it longer way!        
        //return = other.NativeMultiply(this);
        //
        tc0 = other.ApplyTransform(C0);
        tc1 = other.ApplyTransform(C1);
        tc2 = other.ApplyTransform(C2);
        tc3 = other.ApplyTransform(C3);
        return = Transform.ByTuples(tc0, tc1, tc2, tc3);
    }
    
  
}
data = {    {1.0,0.0,0.0,0.0},
            {0.0,2.0,0.0,0.0},
            {0.0,0.0,3.0,0.0},
            {0.0,0.0,0.0,4.0}
        };
        
xform = Transform.ByData(data);
testPoint = Point.ByCoordinates(10,20,30);
x = testPoint.X;
y = testPoint.Y;
z = testPoint.Z;
resultPoint = xform.TransformPoint(testPoint);
resultx = resultPoint.X;
resulty = resultPoint.Y;
resultz = resultPoint.Z;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("resultx", 10.0);
            thisTest.Verify("resulty", 40.0);
            thisTest.Verify("resultz", 90.0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T015_FFI_Transform_Identity()
        {
            string code = @"
class Geometry
{
    private hostEntityID : var;
}
class Point extends Geometry
{
   
    public X                        : var; //double = GlobalCoordinates[0];
    public Y                        : var; //double = GlobalCoordinates[1];
    public Z                        : var; //double = GlobalCoordinates[2];
    private def init : bool ()
    {
       
        X                      =  0.0;
        Y                      =  0.0;
        Z                      =  0.0;
   
        return = true;
    }
    
   
	
	public constructor ByCoordinates(xTranslation : double, yTranslation : double, zTranslation : double)
    {
        neglect = init();
        X = xTranslation;
        Y = yTranslation;
        Z = zTranslation;
    }
	
   
   
   
}
class Tuple4
{
    X : var;
    Y : var;
    Z : var;
    H : var;
    
    constructor XYZH(xValue : double, yValue : double, zValue : double, hValue : double)
    {
        X = xValue;
        Y = yValue;
        Z = zValue;
        H = hValue;        
    }
    
    constructor XYZ(xValue : double, yValue : double, zValue : double)
    {
        X = xValue;
        Y = yValue;
        Z = zValue;
        H = 1.0;        
    }
    constructor ByCoordinates3(coordinates : double[] )
    {
        X = coordinates[0];
        Y = coordinates[1];
        Z = coordinates[2];
        H = 1.0;        
    }
    constructor ByCoordinates4(coordinates : double[] )
    {
        X = coordinates[0];
        Y = coordinates[1];
        Z = coordinates[2];
        H = coordinates[3];    
    }
    
    def get_X : double () 
    {
        return = X;
    }
    
    def get_Y : double () 
    {
        return = Y;
    }
    
    def get_Z : double () 
    {
        return = Z;
    }
    
    def get_H : double () 
    {
        return = H;
    }
    
    public def Multiply : double (other : Tuple4)
    {
        return = X * other.X + Y * other.Y + Z * other.Z + H * other.H;
    }
    
    public def Coordinates3 : double[] ()
    { 
        return = {X, Y, Z };
    }
    
    public def Coordinates4 : double[] () 
    { 
        return = {X, Y, Z, H };
    }
}
class Vector
{
    X : var;
    Y : var;
    Z : var;
    
    public constructor ByCoordinates(xx : double, yy : double, zz : double)
    {
        X = xx;
        Y = yy;
        Z = zz;
    }
}
class Transform
{
    public C0 : Tuple4; 
    public C1 : Tuple4; 
    public C2 : Tuple4; 
    public C3 : Tuple4;     
    
    public constructor ByTuples(C0Value : Tuple4, C1Value : Tuple4, C2Value : Tuple4, C3Value : Tuple4)
    {
        C0 = C0Value;
        C1 = C1Value;
        C2 = C2Value;
        C3 = C3Value;
    }
    public constructor ByData(data : double[][])
    {
        C0 = Tuple4.ByCoordinates4(data[0]);
        C1 = Tuple4.ByCoordinates4(data[1]);
        C2 = Tuple4.ByCoordinates4(data[2]);
        C3 = Tuple4.ByCoordinates4(data[3]);
    }
    
   
    public constructor Identity()
	
	{  C0 = Tuple4.XYZH(1.0,0.0,0.0,0.0);
       C1 = Tuple4.XYZH(0.0,1.0,0.0,0.0);
       C2 = Tuple4.XYZH(0.0,0.0,1.0,0.0);
       C3 = Tuple4.XYZH(0.0,0.0,0.0,1.0);
     
     }
	
}
resultTransform = Transform.Identity();
r0 = resultTransform.C0;
r1 = resultTransform.C1;
r2 = resultTransform.C2;
r3 = resultTransform.C3;
r0X = r0.X;
r0Y = r0.Y;
r0Z = r0.Z;
r0H = r0.H;
r1X = r1.X;
r1Y = r1.Y;
r1Z = r1.Z;
r1H = r1.H;
r2X = r2.X;
r2Y = r2.Y;
r2Z = r2.Z;
r2H = r2.H;
r3X = r3.X;
r3Y = r3.Y;
r3Z = r3.Z;
r3H = r3.H;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r0X", 1.0);
            thisTest.Verify("r0Y", 0.0);
            thisTest.Verify("r0Z", 0.0);
            thisTest.Verify("r0H", 0.0);
            thisTest.Verify("r1X", 0.0);
            thisTest.Verify("r1Y", 1.0);
            thisTest.Verify("r1Z", 0.0);
            thisTest.Verify("r1H", 0.0);
            thisTest.Verify("r2X", 0.0);
            thisTest.Verify("r2Y", 0.0);
            thisTest.Verify("r2Z", 1.0);
            thisTest.Verify("r2H", 0.0);
            thisTest.Verify("r3X", 0.0);
            thisTest.Verify("r3Y", 0.0);
            thisTest.Verify("r3Z", 0.0);
            thisTest.Verify("r3H", 1.0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T016_FFI_Transform_GetTuples()
        {
            string code = @"
class Geometry
{
    private hostEntityID : var;
}
class Point extends Geometry
{
   
    public X                        : var; //double = GlobalCoordinates[0];
    public Y                        : var; //double = GlobalCoordinates[1];
    public Z                        : var; //double = GlobalCoordinates[2];
    private def init : bool ()
    {
       
        X                      =  0.0;
        Y                      =  0.0;
        Z                      =  0.0;
   
        return = true;
    }
    
   
	
	public constructor ByCoordinates(xTranslation : double, yTranslation : double, zTranslation : double)
    {
        neglect = init();
        X = xTranslation;
        Y = yTranslation;
        Z = zTranslation;
    }
	
   
   
   
}
class Tuple4
{
    X : var;
    Y : var;
    Z : var;
    H : var;
    
    constructor XYZH(xValue : double, yValue : double, zValue : double, hValue : double)
    {
        X = xValue;
        Y = yValue;
        Z = zValue;
        H = hValue;        
    }
    
    constructor XYZ(xValue : double, yValue : double, zValue : double)
    {
        X = xValue;
        Y = yValue;
        Z = zValue;
        H = 1.0;        
    }
    constructor ByCoordinates3(coordinates : double[] )
    {
        X = coordinates[0];
        Y = coordinates[1];
        Z = coordinates[2];
        H = 1.0;        
    }
    constructor ByCoordinates4(coordinates : double[] )
    {
        X = coordinates[0];
        Y = coordinates[1];
        Z = coordinates[2];
        H = coordinates[3];    
    }
    
    def get_X : double () 
    {
        return = X;
    }
    
    def get_Y : double () 
    {
        return = Y;
    }
    
    def get_Z : double () 
    {
        return = Z;
    }
    
    def get_H : double () 
    {
        return = H;
    }
    
    public def Multiply : double (other : Tuple4)
    {
        return = X * other.X + Y * other.Y + Z * other.Z + H * other.H;
    }
    
    public def Coordinates3 : double[] ()
    { 
        return = {X, Y, Z };
    }
    
    public def Coordinates4 : double[] () 
    { 
        return = {X, Y, Z, H };
    }
}
class Vector
{
    X : var;
    Y : var;
    Z : var;
    
    public constructor ByCoordinates(xx : double, yy : double, zz : double)
    {
        X = xx;
        Y = yy;
        Z = zz;
    }
}
class Transform
{
    public C0 : Tuple4; 
    public C1 : Tuple4; 
    public C2 : Tuple4; 
    public C3 : Tuple4;     
    
    public constructor ByTuples(C0Value : Tuple4, C1Value : Tuple4, C2Value : Tuple4, C3Value : Tuple4)
    {
        C0 = C0Value;
        C1 = C1Value;
        C2 = C2Value;
        C3 = C3Value;
    }
    public constructor ByData(data : double[][])
    {
        C0 = Tuple4.ByCoordinates4(data[0]);
        C1 = Tuple4.ByCoordinates4(data[1]);
        C2 = Tuple4.ByCoordinates4(data[2]);
        C3 = Tuple4.ByCoordinates4(data[3]);
    }
    
   	
    public def GetTuples : Tuple4[]()
    {
        tupleDataC0 = C0;
        tupleDataC1 = C1;
        tupleDataC2 = C2;
        tupleDataC3 = C3;
        return = { tupleDataC0, tupleDataC1, tupleDataC2, tupleDataC3 };
    }
    public constructor Identity()
	
	{  C0 = Tuple4.XYZH(1.0,0.0,0.0,0.0);
       C1 = Tuple4.XYZH(0.0,1.0,0.0,0.0);
       C2 = Tuple4.XYZH(0.0,0.0,1.0,0.0);
       C3 = Tuple4.XYZH(0.0,0.0,0.0,1.0);
     
     }
	
}
resultTransform = Transform.Identity();
resultTuples = resultTransform.GetTuples();
r0 = resultTuples[0];
r1 = resultTuples[1];
r2 = resultTuples[2];
r3 = resultTuples[3];
r0X = r0.X;
r0Y = r0.Y;
r0Z = r0.Z;
r0H = r0.H;
r1X = r1.X;
r1Y = r1.Y;
r1Z = r1.Z;
r1H = r1.H;
r2X = r2.X;
r2Y = r2.Y;
r2Z = r2.Z;
r2H = r2.H;
r3X = r3.X;
r3Y = r3.Y;
r3Z = r3.Z;
r3H = r3.H;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r0X", 1.0);
            thisTest.Verify("r0Y", 0.0);
            thisTest.Verify("r0Z", 0.0);
            thisTest.Verify("r0H", 0.0);
            thisTest.Verify("r1X", 0.0);
            thisTest.Verify("r1Y", 1.0);
            thisTest.Verify("r1Z", 0.0);
            thisTest.Verify("r1H", 0.0);
            thisTest.Verify("r2X", 0.0);
            thisTest.Verify("r2Y", 0.0);
            thisTest.Verify("r2Z", 1.0);
            thisTest.Verify("r2H", 0.0);
            thisTest.Verify("r3X", 0.0);
            thisTest.Verify("r3Y", 0.0);
            thisTest.Verify("r3Z", 0.0);
            thisTest.Verify("r3H", 1.0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T017_FFI_Transform_GetData()
        {
            object[] result0 = { 1.0, 0.0, 0.0, 0.0 };
            object[] result1 = { 0.0, 1.0, 0.0, 0.0 };
            object[] result2 = { 0.0, 0.0, 1.0, 0.0 };
            object[] result3 = { 0.0, 0.0, 0.0, 1.0 };
            string code = @"
class Geometry
{
    private hostEntityID : var;
}
class Point extends Geometry
{
   
    public X                        : var; //double = GlobalCoordinates[0];
    public Y                        : var; //double = GlobalCoordinates[1];
    public Z                        : var; //double = GlobalCoordinates[2];
    private def init : bool ()
    {
       
        X                      =  0.0;
        Y                      =  0.0;
        Z                      =  0.0;
   
        return = true;
    }
    
   
	
	public constructor ByCoordinates(xTranslation : double, yTranslation : double, zTranslation : double)
    {
        neglect = init();
        X = xTranslation;
        Y = yTranslation;
        Z = zTranslation;
    }
	
   
   
   
}
class Tuple4
{
    X : var;
    Y : var;
    Z : var;
    H : var;
    
    constructor XYZH(xValue : double, yValue : double, zValue : double, hValue : double)
    {
        X = xValue;
        Y = yValue;
        Z = zValue;
        H = hValue;        
    }
    
    constructor XYZ(xValue : double, yValue : double, zValue : double)
    {
        X = xValue;
        Y = yValue;
        Z = zValue;
        H = 1.0;        
    }
    constructor ByCoordinates3(coordinates : double[] )
    {
        X = coordinates[0];
        Y = coordinates[1];
        Z = coordinates[2];
        H = 1.0;        
    }
    constructor ByCoordinates4(coordinates : double[] )
    {
        X = coordinates[0];
        Y = coordinates[1];
        Z = coordinates[2];
        H = coordinates[3];    
    }
    
    def get_X : double () 
    {
        return = X;
    }
    
    def get_Y : double () 
    {
        return = Y;
    }
    
    def get_Z : double () 
    {
        return = Z;
    }
    
    def get_H : double () 
    {
        return = H;
    }
    
    public def Multiply : double (other : Tuple4)
    {
        return = X * other.X + Y * other.Y + Z * other.Z + H * other.H;
    }
    
    public def Coordinates3 : double[] ()
    { 
        return = {X, Y, Z };
    }
    
    public def Coordinates4 : double[] () 
    { 
        return = {X, Y, Z, H };
    }
}
class Vector
{
    X : var;
    Y : var;
    Z : var;
    
    public constructor ByCoordinates(xx : double, yy : double, zz : double)
    {
        X = xx;
        Y = yy;
        Z = zz;
    }
}
class Transform
{
    public C0 : Tuple4; 
    public C1 : Tuple4; 
    public C2 : Tuple4; 
    public C3 : Tuple4;     
    
    public constructor ByTuples(C0Value : Tuple4, C1Value : Tuple4, C2Value : Tuple4, C3Value : Tuple4)
    {
        C0 = C0Value;
        C1 = C1Value;
        C2 = C2Value;
        C3 = C3Value;
    }
    public constructor ByData(data : double[][])
    {
        C0 = Tuple4.ByCoordinates4(data[0]);
        C1 = Tuple4.ByCoordinates4(data[1]);
        C2 = Tuple4.ByCoordinates4(data[2]);
        C3 = Tuple4.ByCoordinates4(data[3]);
    }
    
   	
    public def GetData()
	{
		t0 = C0;
		t1 = C1;
		t2 = C2;
		t3 = C3;
		temp = 	{	{t0.X, t0.Y, t0.Z, t0.H},
					{t1.X, t1.Y, t1.Z, t1.H},
					{t2.X, t2.Y, t2.Z, t2.H},
					{t3.X, t3.Y, t3.Z, t3.H}
				};
		return = temp;
	}
    public constructor Identity()
	
	{  C0 = Tuple4.XYZH(1.0,0.0,0.0,0.0);
       C1 = Tuple4.XYZH(0.0,1.0,0.0,0.0);
       C2 = Tuple4.XYZH(0.0,0.0,1.0,0.0);
       C3 = Tuple4.XYZH(0.0,0.0,0.0,1.0);
     
     }
	
}
resultTransform = Transform.Identity();
resultData = resultTransform.GetData();
result0 = resultData[0];
result1 = resultData[1];
result2 = resultData[2];
result3 = resultData[3];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("result0", result0);
            thisTest.Verify("result1", result1);
            thisTest.Verify("result2", result2);
            thisTest.Verify("result3", result3);

        }

        public void Z001_FFI_Regress_1455587()
        {
            string code = @"
external (""ffi_library"") def sum_all : double (arr : double[], numElems : int);
arr = {1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0};
sum_1_to_10 = sum_all(arr, 10);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("sum_1_to_10", 55.0);
        }

        public void T018_FFI_Math_Trigonometric_Hyperbolic()
        {
            const double deg_to_rad = Math.PI / 180;
            string code = @"
class Math
{
				external (""ffi_library"") def dc_sec:double(deg:double);
				external (""ffi_library"") def dc_csc:double(deg:double);
				external (""ffi_library"") def dc_cot:double(deg:double);
				external (""ffi_library"") def dc_asec:double(deg:double);
				external (""ffi_library"") def dc_acsc:double(deg:double);
				external (""ffi_library"") def dc_acot:double(deg:double);	
				external (""ffi_library"") def dc_cosh : double (val : double);
				external (""ffi_library"") def dc_sinh : double (val : double);
				external (""ffi_library"") def dc_tanh : double (val : double);			
				external (""ffi_library"") def dc_csch : double (val : double);
				external (""ffi_library"") def dc_sech : double (val : double);
				external (""ffi_library"") def dc_coth : double (val : double);					
           
                constructor GetInstance()
                {}              
                			
                def Sec : double ( val : double )
                {
                                return = dc_sec(val);
                }
                
                def Csc : double ( val : double )
                {
                                return = dc_csc(val);
                }
                
                def Cot : double ( val : double )
                {
                                return = dc_cot(val);
                }
				
                def ArcSec : double ( val : double )
                {
                                return = dc_asec(val);
                }
                
                def ArcCsc : double ( val : double )
                {
                                return = dc_acsc(val);
                }
                
                def ArcCot : double ( val : double )
                {
                                return = dc_acot(val);
                }
				
				def Sinh : double ( val : double )
                {
                                return = dc_sinh(val);
                }
                
                def Cosh : double ( val : double )
                {
                                return = dc_cosh(val);
                }
                
                def Tanh : double ( val : double )
                {
                                return = dc_tanh(val);
                }
				
				def Sech : double ( val : double )
                {
                                return = dc_sech(val);
                }
                
                def Csch : double ( val : double )
                {
                                return = dc_csch(val);
                }
                
                def Coth : double ( val : double )
                {
                                return = dc_coth(val);
                }
}
[Associative]
{
                math = Math.GetInstance();
				//trigonometric
                angle = 30.0;
                sec_30 = math.Sec(angle);
                csc_30 = math.Csc(angle);
                cot_30 = math.Cot(angle);
                asec_30 = math.ArcSec(sec_30);
                acsc_30 = math.ArcCsc(csc_30);
                acot_30 = math.ArcCot(cot_30);
				//hyperbolic
				sinh_1 = math.Sinh(1.0);
				cosh_1 = math.Cosh(1.0);
				tanh_1 = math.Tanh(1.0);				
				sech_1 = math.Sech(1.0);
				csch_1 = math.Csch(1.0);
				coth_1 = math.Coth(1.0);     
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("angle", 30.0, 1);
            thisTest.Verify("sec_30", 1 / Math.Sin(deg_to_rad * 30), 1);
            thisTest.Verify("csc_30", 1 / Math.Cos(deg_to_rad * 30), 1);
            thisTest.Verify("cot_30", 1 / Math.Tan(deg_to_rad * 30), 1);
            thisTest.Verify("asec_30", 30.0, 1);
            thisTest.Verify("acsc_30", 30.0, 1);
            thisTest.Verify("acot_30", 30.0, 1);
            thisTest.Verify("sinh_1", Math.Sinh(1), 1);
            thisTest.Verify("cosh_1", Math.Cosh(1), 1);
            thisTest.Verify("tanh_1", Math.Tanh(1), 1);
            thisTest.Verify("sech_1", 1 / Math.Sinh(1), 1);
            thisTest.Verify("csch_1", 1 / Math.Cosh(1), 1);
            thisTest.Verify("coth_1", 1 / Math.Tanh(1), 1);
        }

        public void T019_FFI_Math_Others()
        {
            double val = 8.8234893405;
            string code = @"
class Math
{
                external (""ffi_library"") def dc_ceil : double (val : double);
				external (""ffi_library"") def dc_floor : double (val : double);
				external (""ffi_library"") def dc_abs : double (val : double);
				external (""ffi_library"") def dc_pow : double (val1 : double, val2 : double);
				external (""ffi_library"") def dc_exp : double (val : double);
				external (""ffi_library"") def dc_factorial : int (val : int); //overflow problem. int is too small
				external (""ffi_library"") def dc_sum_range_step_size : double (val1 : double, val2 : double, step: double);
				external (""ffi_library"") def dc_sum_range_step_num : double (val1 : double, val2 : double, stepNum: int);
				external (""ffi_library"") def dc_sign : int (val : double);
				external (""ffi_library"") def dc_log10 : double (val : double);
				external (""ffi_library"") def dc_logbase: double (val : double, logbase : double);
				external (""ffi_library"") def dc_max : double (val1 : double, val2 : double);
				external (""ffi_library"") def dc_min : double (val1 : double, val2 : double);
				external (""ffi_library"") def dc_round : double (val : double, decimals: int);
				
                constructor GetInstance()
                {}
                
                def Ceil : double ( val : double )
                {
                                return = dc_ceil(val);
                }
                def Floor : double ( val : double )
                {
                                return = dc_floor(val);
                }
				
				def Abs : double ( val : double )
                {
                                return = dc_abs(val);
                }
                
                def Pow : double ( val1 : double, val2 : double )
                {
                                return = dc_pow(val1, val2);
                }
                
                def Exp : double ( val : double )
                {
                                return = dc_exp(val);
                }
				
				def Factorial : int ( val : int )
                {
                                return = dc_factorial(val);
                }
				
				def SumRangeStepSize : double (val1 : double, val2 : double, step: double)
                {
                                return = dc_sum_range_step_size(val1, val2, step);
                }
				
				def SumRangeStepNum : double (val1 : double, val2 : double, stepNum: int)
                {
                                return = dc_sum_range_step_num(val1, val2, stepNum);
                }
				
				def Sign : int ( val : double )
                {
                                return = dc_sign(val);
                }
                def Log10 : double ( val : double )
                {
                                return = dc_log10(val);
                }
				
				def LogBase : double (val : double, logbase : double)
                {
                                return = dc_logbase(val, logbase);
                }
                
                def Max : double ( val1 : double, val2 : double )
                {
                                return = dc_max(val1, val2);
                }
				
				def Min : double ( val1 : double, val2 : double )
                {
                                return = dc_min(val1, val2);
                }
                
                def Round : double (val : double, decimals: int)
                {
                                return = dc_round(val, decimals);
                }
}
[Associative]
{
                math = Math.GetInstance();
				ceil_5d5 = math.Ceil(5.5);
				floor_5d5 = math.Floor(5.5);
				abs_5 = math.Abs(5.0);
				abs_neg5 = math.Abs(-5.0);
				pow_3p4 = math.Pow(3.0, 4.0);	
				exp_3 = math.Exp(3.0);
				fact_10 = math.Factorial(10);
				sum_1t100s11 = math.SumRangeStepSize(1.0,100.0,11.0);
				sum_1t100sn11 = math.SumRangeStepNum(1.0,100.0,11);
				val = 8.8234893405;
				sign_pos = math.Sign(val);
				//sign_neg = math.Sign(-val); //wrong answer, sign_neg = 4294967295, should be -1
				sign_zero = math.Sign(0.0);
				log10_val = math.Log10(val);
				logbase_val_5 = math.LogBase(val, 5.0);
				max_10 = math.Max(val, 10.0);
				min_Neg2 = math.Min(val, -2.0);
				round_val_5 = math.Round(val, 5);
				round_negval_5 = math.Round(-val, 5); 
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("ceil_5d5", 6.0, 1);
            thisTest.Verify("floor_5d5", 5.0, 1);
            thisTest.Verify("abs_5", 5.0, 1);
            thisTest.Verify("abs_neg5", 5.0, 1);
            thisTest.Verify("pow_3p4", 81.0, 1);
            thisTest.Verify("exp_3", Math.Exp(3), 1);
            thisTest.Verify("fact_10", 3628800, 1);
            thisTest.Verify("sum_1t100s11", 505.0, 1);
            thisTest.Verify("sum_1t100sn11", 606.0, 1);
            thisTest.Verify("val", val, 1);
            thisTest.Verify("sign_pos", 1, 1);
            thisTest.Verify("sign_zero", 0, 1);
            //thisTest.Verify("sign_neg", -1, 1);
            thisTest.Verify("log10_val", Math.Log10(val), 1);
            thisTest.Verify("logbase_val_5", Math.Log(val, 5), 1);
            thisTest.Verify("max_10", 10.0, 1);
            thisTest.Verify("min_Neg2", -2.0, 1);
            thisTest.Verify("round_val_5", 8.82349, 1);
            thisTest.Verify("round_negval_5", -8.82349, 1);
        }


        [Test]
        //public void T020_Sample_Test_1458422_Regress()

        public void T020_Sample_Test()
        {
            string code = @"
import (""FFITarget.dll"");
	vec =  ClassFunctionality.ClassFunctionality(3,4,0); 
	o = vec.Int
	
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.Fail("1467174 - sprint24 : rev 3150 : warning:Function 'get_X' not Found");
            thisTest.Verify("vec_X", 3.0, 0);
            thisTest.Verify("vec_Y", 4.0, 0);
            thisTest.Verify("vec_Z", 0.0, 0);
        }

        [Test]
        public void T021_Vector_ByCoordinates_1458422_Regress()
        {
            //Assert.Fail("1463747 - Sprint 20 : rev 2147 : FFI issue : exception is thrown when same geometry is assigned to same variable more than once "); 

            string code = @"
import (Vector from ""ProtoGeometry.dll"");
	vec =  Vector.ByCoordinates(3.0,4.0,0.0,true); 
	vec_X = vec.get_X();
	vec_Y = vec.get_Y();
	vec_Z = vec.get_Z();
	vec_Normalised=vec.Normalize();
	vec2 =  Vector.ByCoordinates(3.0,4.0,0.0,false);
	
	vec2 =  Vector.ByCoordinates(3.0,4.0,0.0,false);
	vec2_X = vec2.get_X();
	vec2_Y = vec2.get_Y();
	vec2_Z = vec2.get_Z();
	vec_len = vec2.GetLength();
	vec1 =  Vector.ByCoordinates(3.0,4.0,0.0,null); 
	vec4 =  Vector.ByCoordinateArrayN({3.0,4.0,0.0});
	vec4_coord={vec4.get_X(),vec4.get_Y(),vec4.get_Z()};
	vec5 =  Vector.ByCoordinateArrayN({3.0,4.0,0.0},true); 
	vec5_coord={vec5.get_X(),vec5.get_Y(),vec5.get_Z()};
	
	is_same = vec.Equals(vec);// same vec
	vec2=  Vector.ByCoordinates(1.0,2.0,0.0);
	is_same2 = vec.Equals(vec2);// different vec
	
	
	vec3 =  Vector.ByCoordinates(1.0,0.0,0.0,true); 
	is_parallel1 = vec.IsParallel(vec); //same vec
	vec4=  Vector.ByCoordinates(3.0,0.0,0.0);	
	is_parallel2 = vec3.IsParallel(vec4);//parallel
	vec5 =  Vector.ByCoordinates(3.0,4.0,5.0); //non parallel
	is_parallel3 = vec.IsParallel(vec5);
	vec6 =  Vector.ByCoordinates(0.0,1.0,0.0);
	vec7 =  Vector.ByCoordinates(1.0,0.0,0.0);
	is_perp1 = vec6.IsPerpendicular(vec7);//same vec
	is_perp2 = vec6.IsPerpendicular(vec5);//diff vec
	dotProduct=vec2.Dot(vec2);
	vec8 =  Vector.ByCoordinates(1.0,0.0,0.0,false);
	vec9 =  Vector.ByCoordinates(0.0,1.0,0.0,false);
	crossProduct=vec8.Cross(vec9);
	cross_X=crossProduct.get_X();
	cross_Y=crossProduct.get_Y();
	cross_Z=crossProduct.get_Z();
	newVec=vec5.Scale(2.0);//single
	newVec_X=newVec.get_X();
	newVec_Y=newVec.get_Y();
	newVec_Z=newVec.get_Z();
	coord_Vec=    vec.ComputeGlobalCoords(1,2,3);
	
	";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.Fail("1467174 - sprint24 : rev 3150 : warning:Function 'get_X' not Found");
            thisTest.Verify("vec_X", 0.6, 0);
            thisTest.Verify("vec_Y", 0.8, 0);
            thisTest.Verify("vec_Z", 0.0, 0);
            thisTest.Verify("vec_Normalised", true, 0);
            thisTest.Verify("vec2_X", 1.0, 0); //vec2 is initialized as {3.0, 4.0, 0.0} but reset as {1.0, 2.0, 0.0}
            thisTest.Verify("vec2_Y", 2.0, 0);
            thisTest.Verify("vec2_Z", 0.0, 0);
            thisTest.Verify("vec_len", 2.23606797749979, 0);
            List<Object> vec4 = new List<Object> { 3.0, 0.0, 0.0 };
            List<Object> vec5 = new List<Object> { 3.0, 4.0, 5.0 };
            Assert.IsTrue(mirror.CompareArrays("vec4_coord", vec4, typeof(System.Double))); //updated to vec4=  Vector.ByCoordinates(3.0,0.0,0.0);
            Assert.IsTrue(mirror.CompareArrays("vec5_coord", vec5, typeof(System.Double))); //updated to vec5 =  Vector.ByCoordinates(3.0,4.0,5.0);
            thisTest.Verify("is_same", true, 0);
            thisTest.Verify("is_same2", false, 0);
            thisTest.Verify("is_parallel1", true, 0);
            thisTest.Verify("is_parallel2", true, 0);
            thisTest.Verify("is_parallel3", false, 0);
            thisTest.Verify("is_perp1", true, 0);
            thisTest.Verify("is_perp2", false, 0);
            thisTest.Verify("is_perp2", false, 0);
            thisTest.Verify("dotProduct", 5.0, 0);
            thisTest.Verify("cross_X", 0.0, 0);
            thisTest.Verify("cross_Y", 0.0, 0);
            thisTest.Verify("cross_Z", 1.0, 0);
            thisTest.Verify("newVec_X", 6.0, 0);
            thisTest.Verify("newVec_Y", 8.0, 0);
            thisTest.Verify("newVec_Z", 10.0, 0);
            thisTest.Verify("vec1", null, 0);
            thisTest.Verify("coord_Vec", null, 0); //ComputeGlobalCoords is not defined on Vector class.
        }

        [Test]
        [Category("SmokeTest")]
        public void T022_Array_Marshal()
        {
            string code = @"
import (Dummy from ""ProtoTest.dll"");
dummy = Dummy.Dummy();
arr = {0.0,1.0,2.0,3.0,4.0,5.0,6.0,7.0,8.0,9.0,10.0};
sum_1_10 = dummy.SumAll(arr);
twice_arr = dummy.Twice(arr);
	";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("sum_1_10", 55.0, 0);
            object[] Expectedresult = { 0.0, 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0 };
            object[] Expectedresult2 = { 0.0, 2.0, 4.0, 6.0, 8.0, 10.0, 12.0, 14.0, 16.0, 18.0, 20.0 };
            thisTest.Verify("twice_arr", Expectedresult2, 0);
        }
    

        [Test]
        public void T023_MethodOverload()
        {
            string code = @"
import(""FFITarget.dll"");
cf1 = ClassFunctionality.ClassFunctionality(1);
cf2 = ClassFunctionality.ClassFunctionality(2);
i = 3;

o1 = cf1.OverloadedAdd(cf2);
o2 = cf1.OverloadedAdd(i);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("o1", 3);
            thisTest.Verify("o2", 4);


        }
    
    }
}
