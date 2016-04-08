using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTest.TD;
using ProtoTestFx.TD;
namespace ProtoTest.Associative
{
    class InheritaceTest : ProtoTestBase
    {
        [Test]
        [Ignore][Category("DSDefinedClass_Ignored_DoNotTestInheritance")]
        public void InheritanceTest01()
        {
            String code =
@"
	class RigidBody
	{
        id : var;
        velocity : var;
        constructor RigidBody(objID : int, vel : double)
        {
            id = objID;
            velocity = vel;
        }
        def GetVelocity : double(drag : int)
        {
            return = velocity * drag;
        }
    }
    class Particle extends RigidBody
	{
        lifetime : var;
        constructor Particle(objID : int, vel : double, life : double)
        {
            id = objID;
            velocity = vel;
            lifetime = life;
        }
    }
 
 
    // TODO Jun: Fix defect, allow statements (or maybe just preprocs?) before a class decl
    // Define some constants
    kRigidBodyID    = 0; 
    kParticleID     = 1;
    kGravityCoeff   = 9.8;
       
    //================================
    // Simulate physical object 1
    //================================
    // Construct a base rigid body
    rb = RigidBody.RigidBody(kRigidBodyID, kGravityCoeff);
    rbVelocity = rb.GetVelocity(2);
    
    //================================
    // Simulate physical object 2
    //================================
    
    // Construct a particle that inherits from a rigid body
    kLifetime = 0.25;
    p = Particle.Particle(kParticleID, kGravityCoeff, kLifetime);
    lt = p.lifetime;
    particleVelocity = rb.GetVelocity(4);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("rbVelocity",19.6);
            thisTest.Verify("lt",0.25);
            thisTest.Verify("particleVelocity",39.2);
        }

        [Test]
        [Ignore][Category("DSDefinedClass_Ignored_DoNotTestInheritance")]
        public void InheritanceTest02()
        {
            String code =
@"
class A
{
    id : var;
    constructor A(pId : int)
    {
        loc = 20;
        id = pId;
    }
}
class B extends A
{
    constructor B(pId : int)
    {
        id = pId;
    }
}
a = B.B(1);
i = a.id;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("i",1);
        }

        [Test]
        [Ignore][Category("DSDefinedClass_Ignored_DoNotTestInheritance")]
        public void InheritanceTest03()
        {
            String code =
@"
class Geometry
{
    id: var;
}
class Point extends Geometry
{
    _x : var;
    constructor ByCoordinates(xx : double)
    {
        _x = xx;
    }
}
    
class Circle extends Geometry
{
    _cp : var;
    constructor ByCenterPointRadius()
    {
        _cp = Point.ByCoordinates(100.0);
    }
}
x;
[Associative]
{
    c = Circle.ByCenterPointRadius();
    cp = c._cp;
    x = cp._x;
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x",100);
        }

        [Test]
        [Ignore][Category("DSDefinedClass_Ignored_DoNotTestInheritance")]
        public void InheritanceTest04()
        {
            String code =
@"
class Geometry
{
    id : var;
}
class Curve extends Geometry
{}
class Point extends Geometry
{
    _x : var;
    _y : var;
    _z : var;
    
    constructor ByCoordinates(xx : double, yy : double, zz : double)
    {
        _x = xx;
        _y = yy;
        _z = zz;
    }
}
    
class Circle extends Curve
{
    _cp : var;
    constructor ByCenterPointRadius()
    {
        _cp = Point.ByCoordinates(10.0,20.0,30.0);
    }
    
    def get_CenterPoint : Point ()
    {        
        return = _cp;
    }
}
x;
[Associative]
{
    c = Circle.ByCenterPointRadius();
    pt = c.get_CenterPoint();
    x = pt._x;
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x",10);
        }


        [Test]
        [Ignore][Category("DSDefinedClass_Ignored_DoNotTestInheritance")]
        public void InheritanceTest05()
        {
            String code =
@"
class Base
{
    id : var;
}
class Derived extends Base
{
    idFoo : var;
}

def foo : Base ()
{
    a = Derived.Derived();
    a.idFoo = 2;
    return = a;
}

x = foo();

xo = x.idFoo;

xn = x != null;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("xn", true);
            thisTest.Verify("xo",2);
        }
    }
}
