using System;
using NUnit.Framework;
namespace ProtoTest.Associative
{
    class InheritaceTest : ProtoTestBase
    {
        [Test]
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
    
    //================================
    // Simulate physical object 2
    //================================
    
    // Construct a particle that inherits from a rigid body
    kLifetime = 0.25;
    p = Particle.Particle(kParticleID, kGravityCoeff, kLifetime);
    lt = p.lifetime;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("lt",0.25);
        }

        [Test]
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
