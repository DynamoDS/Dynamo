using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTestFx.TD;
using ProtoCore.Mirror;
namespace ProtoTest.TD.FFI
{
    class FFITest : ProtoTestBase
    {
        [Test]
        [Category("SmokeTest")]
        public void T003_ClassTest()
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
        public void T004_Tuple4_XYZ_Simple_WithGetMethods()
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
        public void T005_Tuple4_ByCoordinate3_Simple()
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
        public void T006_Tuple4_ByCoordinate4_Simple()
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
        public void T007_Tuple4_Multiply_Simple()
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
        public void T008_Transform_ByDate_Simple()
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
        public void T009_Transform_ByTuples_Simple()
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
        public void T010_Transform_ApplyTransform()
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
        public void T011_Transform_NativeMultiply()
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
        public void T012_Transform_NativePreMultiply()
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
        public void T013_Transform_TransformVector()
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
        public void T014_Transform_TransformPoint()
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
        public void T015_Transform_Identity()
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
        public void T016_Transform_GetTuples()
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
        public void T017_Transform_GetData()
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


        [Test]
        public void T020_Sample_Test()
        {
            string code = @"
import (""FFITarget.dll"");
	vec =  ClassFunctionality.ClassFunctionality(3,4,0); 
	o = vec.IntVal;
	
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("o", 7, 0);
        }

        [Test]
        public void T021_Vector_ByCoordinates_1458422_Regress()
        {
            string code = @"
import (""FFITarget.dll"");
    cf = ClassFunctionality.ClassFunctionality(0,1,2);
    intVal = cf.IntVal;
    vc1 = cf.ClassProperty;
    vc2 = cf.ClassProperty;
    vc1Value = vc1.SomeValue;
    vc2Value = vc2.SomeValue;
    vcValueEquality = vc1Value == vc2Value;
 
	
	";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("vcValueEquality", true);

            thisTest.Verify("intVal", 3);
            thisTest.Verify("vc1Value", 3);
            thisTest.Verify("vc2Value", 3);

        }

        [Test]
        [Category("SmokeTest")]
        public void T022_Array_Marshal()
        {
            string code = @"
import (Dummy from ""FFITarget.dll"");
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

        [Test]
        public void T024_MethodOverload_static()
        {
            string code = @"
import(""FFITarget.dll"");
cf1 = ClassFunctionality.ClassFunctionality(1);
dp1 = DummyPoint.ByCoordinates(0,1,2);

o1 = OverloadTarget.StaticOverload(1);
o2 = OverloadTarget.StaticOverload(cf1);
o3 = OverloadTarget.StaticOverload(dp1);


";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("o1", 0);
            thisTest.Verify("o2", 1);
            thisTest.Verify("o3", 2);


        }

        [Test]
        public void T025_MethodOverload_DifferentPrimitiveType()
        {
            string code = @"
import(""FFITarget.dll"");

o1 = OverloadTarget.DifferentPrimitiveType(1);
o2 = OverloadTarget.DifferentPrimitiveType(true);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("o1", 2);
            thisTest.Verify("o2", 1);
        }

        [Test]
        public void T026_MethodOverload_DifferentIEnumerable()
        {
            string code = @"
import(""FFITarget.dll"");

a = DummyClassA.DummyClassA();
o1 = OverloadTarget.IEnumerableOfDifferentObjectType(a);
o2 = OverloadTarget.IEnumerableOfDifferentObjectType(a);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("o1", 3);
            thisTest.Verify("o2", 3);

        }

        [Test]
        public void MethodWithRefOutParams_NoLoad()
        {
            string code = @"
import(""FFITarget.dll"");
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            string ffiTargetClass = "ClassWithRefParams";

            // Assert that the class name is indeed a class
            ClassMirror type = null;
            Assert.DoesNotThrow(() => type = new ClassMirror(ffiTargetClass, thisTest.GetTestCore()));

            var members = type.GetMembers();

            var expected = new string[] { "ClassWithRefParams" };

            var actual = members.OrderBy(n => n.Name).Select(x => x.Name).ToArray();
            Assert.AreEqual(expected, actual);
        }

    }
}
