using System;
using System.Collections.Generic;
using FFITarget;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Mirror;
using ProtoScript.Runners;
using ProtoTest;
namespace ProtoFFITests
{
    class CSFFIDispose : ProtoTestBase 
    {
        private LiveRunner astLiveRunner = null;

        public override void Setup()
        {
            base.Setup();
            DisposeTracer.DisposeCount = 0;
            AbstractDerivedDisposeTracer2.DisposeCount = 0;
            astLiveRunner = new LiveRunner();
        }

        public override void TearDown()
        {
            base.TearDown();
            astLiveRunner.Dispose();
        }

        [Test]
        public void Dispose01_NoFunctionCall()
        {
            String code =
            @"              
import(""FFITarget.dll"");
cf1 = ClassFunctionality.ClassFunctionality(2);
[Associative]
{
    x = 2;
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectStillInScope("cf1", 0);
        }

        [Test]
        public void Dispose02_FunctionNonArray()
        {
            String code =
            @"              
import(""FFITarget.dll"");
def foo(p:ClassFunctionality)
{
    return = null;
}
pt1 = ClassFunctionality.ClassFunctionality(0);
[Associative]
{
    vec1 = ClassFunctionality.ClassFunctionality(1);
    vec2 = ClassFunctionality.ClassFunctionality(2);
    vec3 = ClassFunctionality.ClassFunctionality(0);
    vecarr = [ vec1, vec2, vec3 ];
    
    pt2 = ClassFunctionality.ClassFunctionality(1);
    pt3 = ClassFunctionality.ClassFunctionality(2);
    ptarr = [ pt1, pt2, pt3 ];
    test = foo(pt1);
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            thisTest.VerifyFFIObjectStillInScope("pt1", 0);
            thisTest.VerifyFFIObjectOutOfScope("pt2");
            thisTest.VerifyFFIObjectOutOfScope("pt3");
            
            
        }

        [Test]
        public void Dispose03_FunctionReplication()
        {
            String code =
            @"              
import(""FFITarget.dll"");
def foo(p:ClassFunctionality)
{
    return = null;
}
pt1 = ClassFunctionality.ClassFunctionality(0);
[Associative]
{
    vec1 = ClassFunctionality.ClassFunctionality(1);
    vec2 = ClassFunctionality.ClassFunctionality(2);
    vec3 = ClassFunctionality.ClassFunctionality(0);
    vecarr = [ vec1, vec2, vec3 ];
    
    pt2 = ClassFunctionality.ClassFunctionality(0);
    pt3 = ClassFunctionality.ClassFunctionality(1);
    ptarr = [ pt1, pt2, pt3 ];
    test = foo(ptarr);
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            thisTest.VerifyFFIObjectStillInScope("pt1", 0);
            thisTest.VerifyFFIObjectOutOfScope("pt2");
            thisTest.VerifyFFIObjectOutOfScope("pt3");
            
            
        }

        [Test]
        public void Dispose04_FunctionArray()
        {
            String code =
            @"              
import(""FFITarget.dll"");
def foo(p:ClassFunctionality[])
{
    return = null;
}
pt1 = ClassFunctionality.ClassFunctionality(0);
[Associative]
{
    vec1 = ClassFunctionality.ClassFunctionality(1);
    vec2 = ClassFunctionality.ClassFunctionality(1);
    vec3 = ClassFunctionality.ClassFunctionality(0);
    vecarr = [ vec1, vec2, vec3 ];
    
    pt2 = ClassFunctionality.ClassFunctionality(0);
    pt3 = ClassFunctionality.ClassFunctionality(0);
    ptarr = [ pt1, pt2, pt3 ];
    test = foo(ptarr);
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            thisTest.VerifyFFIObjectStillInScope("pt1", 0);
            thisTest.VerifyFFIObjectOutOfScope("pt2");
            thisTest.VerifyFFIObjectOutOfScope("pt3");
            
            
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Dispose05_StaticFunctionNonArray()
        {
            String code =
            @"              
import(""FFITarget.dll"");
pt1 = ClassFunctionality.ClassFunctionality(0);
[Associative]
{
    vec1 = ClassFunctionality.ClassFunctionality(1);
    vec2 = ClassFunctionality.ClassFunctionality(1);
    vec3 = ClassFunctionality.ClassFunctionality(0);
    vecarr = [ vec1, vec2, vec3 ];
    
    pt2 = ClassFunctionality.ClassFunctionality(0);
    pt3 = ClassFunctionality.ClassFunctionality(0);
    ptarr = [ pt1, pt2, pt3 ];
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            thisTest.VerifyFFIObjectStillInScope("pt1", 0);
            thisTest.VerifyFFIObjectOutOfScope("pt2");
            thisTest.VerifyFFIObjectOutOfScope("pt3");
            
            
            
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Dispose06_StaticFunctionReplication()
        {
            String code =
            @"              
import(""FFITarget.dll"");
pt1 = ClassFunctionality.ClassFunctionality(0, 0, 0);
[Associative]
{
    vec1 = ClassFunctionality(1, 1, 1);
    vec2 = ClassFunctionality.ClassFunctionality(1, 0, 0);
    vec3 = ClassFunctionality.ClassFunctionality(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    
    pt2 = ClassFunctionality.ClassFunctionality(0, 0, 0);
    pt3 = ClassFunctionality.ClassFunctionality(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            thisTest.VerifyFFIObjectStillInScope("pt1", 0);
            thisTest.VerifyFFIObjectOutOfScope("pt2");
            thisTest.VerifyFFIObjectOutOfScope("pt3");
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Dispose07_StaticFunctionNonArray()
        {
            String code =
            @"              
import(""FFITarget.dll"");
pt1 = DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    
    pt2 = DummyPoint.ByCoordinates(0, 0, 0);
    pt3 = DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            thisTest.VerifyFFIObjectStillInScope("pt1", 0);
            thisTest.VerifyFFIObjectOutOfScope("pt2");
            thisTest.VerifyFFIObjectOutOfScope("pt3");
            
            
            
            
            
            
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Dispose08_MemFunctionNonArray()
        {
            String code =
            @"              
import(""FFITarget.dll"");
pt1 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    
    
    
   
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    
    pt2 =DummyPoint.ByCoordinates(0, 0, 0);
    pt3 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
    
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            thisTest.VerifyFFIObjectStillInScope("pt1", 0);
            thisTest.VerifyFFIObjectOutOfScope("pt2");
            thisTest.VerifyFFIObjectOutOfScope("pt3");
            
            
            
            
            
            
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Dispose09_MemFunctionReplication()
        {
            String code =
            @"              
import(""FFITarget.dll"");
pt1 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    
    
    
   
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    
    pt2 =DummyPoint.ByCoordinates(0, 0, 0);
    pt3 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
    
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            thisTest.VerifyFFIObjectStillInScope("pt1", 0);
            thisTest.VerifyFFIObjectOutOfScope("pt2");
            thisTest.VerifyFFIObjectOutOfScope("pt3");
            
            
            
            
            
            
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Dispose10_MemFunctionArray()
        {
            String code =
            @"              
import(""FFITarget.dll"");
pt1 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    
    
    
   
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    
    pt2 =DummyPoint.ByCoordinates(0, 0, 0);
    pt3 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
    test = a1.bar(ptarr);
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            thisTest.VerifyFFIObjectStillInScope("pt1", 0);
            thisTest.VerifyFFIObjectOutOfScope("pt2");
            thisTest.VerifyFFIObjectOutOfScope("pt3");
            
            
            
            
            
            
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Dispose11_ReplicationNonArray()
        {
            String code =
            @"              
import(""FFITarget.dll"");
pt1 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    
    
    
   
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    
    pt2 =DummyPoint.ByCoordinates(0, 0, 0);
    pt3 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
    
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            thisTest.VerifyFFIObjectStillInScope("pt1", 0);
            thisTest.VerifyFFIObjectOutOfScope("pt2");
            thisTest.VerifyFFIObjectOutOfScope("pt3");
            
            
            
            
            
            
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Dispose12_ReplicationReplication()
        {
            String code =
            @"              
import(""FFITarget.dll"");
pt1 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    
    
    
   
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    
    pt2 =DummyPoint.ByCoordinates(0, 0, 0);
    pt3 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
    
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            thisTest.VerifyFFIObjectStillInScope("pt1", 0);
            thisTest.VerifyFFIObjectOutOfScope("pt2");
            thisTest.VerifyFFIObjectOutOfScope("pt3");
            
            
            
            
            
            
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Dispose13_ReplicationArray()
        {
            String code =
            @"              
import(""FFITarget.dll"");
pt1 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    
    
    
   
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    
    pt2 =DummyPoint.ByCoordinates(0, 0, 0);
    pt3 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
    
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            thisTest.VerifyFFIObjectStillInScope("pt1", 0);
            thisTest.VerifyFFIObjectOutOfScope("pt2");
            thisTest.VerifyFFIObjectOutOfScope("pt3");
            
            
            
            
            
            
        }

        [Test]
        public void Dispose14_GlobalFunctionTwoArguments()
        {
            String code =
            @"              
import(""FFITarget.dll"");
def foo(p:Point, v:Vector)
{
    return = null;
}
pt1 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    
    pt2 =DummyPoint.ByCoordinates(0, 0, 0);
    pt3 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
    test = foo(ptarr,vecarr);
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            thisTest.VerifyFFIObjectStillInScope("pt1", 0);
            thisTest.VerifyFFIObjectOutOfScope("pt2");
            thisTest.VerifyFFIObjectOutOfScope("pt3");
            
            
        }

        [Test]
        public void Dispose15_GlobalFunctionTwoArguments()
        {
            String code =
            @"              
import(""FFITarget.dll"");
def foo(p:Point, v:Vector)
{
    return = null;
}
pt1 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    
    pt2 =DummyPoint.ByCoordinates(0, 0, 0);
    pt3 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
    test = foo(ptarr,vec1);
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            thisTest.VerifyFFIObjectStillInScope("pt1", 0);
            thisTest.VerifyFFIObjectOutOfScope("pt2");
            thisTest.VerifyFFIObjectOutOfScope("pt3");
            
            
        }

        [Test]
        public void Dispose16_GlobalFunctionTwoArguments()
        {
            String code =
            @"              
import(""FFITarget.dll"");
def foo(p:Point, v:Vector)
{
    return = null;
}
pt1 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    
    pt2 =DummyPoint.ByCoordinates(0, 0, 0);
    pt3 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
    test = foo(ptarr,vecarr);
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            thisTest.VerifyFFIObjectStillInScope("pt1", 0);
            thisTest.VerifyFFIObjectOutOfScope("pt2");
            thisTest.VerifyFFIObjectOutOfScope("pt3");
            
            
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Dispose17_StaticFunctionTwoArguments()
        {
            String code =
            @"              
import(""FFITarget.dll"");
pt1 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    
    
    
   
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    
    pt2 =DummyPoint.ByCoordinates(0, 0, 0);
    pt3 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
    
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            thisTest.VerifyFFIObjectStillInScope("pt1", 0);
            thisTest.VerifyFFIObjectOutOfScope("pt2");
            thisTest.VerifyFFIObjectOutOfScope("pt3");
            
            
            
            
            
            
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Dispose18_StaticFunctionTwoArguments()
        {
            String code =
            @"              
import(""FFITarget.dll"");
pt1 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    
    
    
   
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    
    pt2 =DummyPoint.ByCoordinates(0, 0, 0);
    pt3 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
    
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            thisTest.VerifyFFIObjectStillInScope("pt1", 0);
            thisTest.VerifyFFIObjectOutOfScope("pt2");
            thisTest.VerifyFFIObjectOutOfScope("pt3");
            
            
            
            
            
            
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Dispose19_StaticFunctionTwoArguments()
        {
            String code =
            @"              
import(""FFITarget.dll"");
pt1 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    
    
    
   
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    
    pt2 =DummyPoint.ByCoordinates(0, 0, 0);
    pt3 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
    
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            thisTest.VerifyFFIObjectStillInScope("pt1", 0);
            thisTest.VerifyFFIObjectOutOfScope("pt2");
            thisTest.VerifyFFIObjectOutOfScope("pt3");
            
            
            
            
            
            
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Dispose20_MemberFunctionTwoArguments()
        {
            String code =
            @"              
import(""FFITarget.dll"");
pt1 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    
    
    
   
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    
    pt2 =DummyPoint.ByCoordinates(0, 0, 0);
    pt3 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            thisTest.VerifyFFIObjectStillInScope("pt1", 0);
            thisTest.VerifyFFIObjectOutOfScope("pt2");
            thisTest.VerifyFFIObjectOutOfScope("pt3");
            
            
            
            
            
            
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Dispose21_MemberFunctionTwoArguments()
        {
            String code =
            @"              
import(""FFITarget.dll"");
pt2 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    
    
    
   
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    pt1 =DummyPoint.ByCoordinates(0, 0, 0);
    pt3 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
    
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            thisTest.VerifyFFIObjectOutOfScope("pt1");
            thisTest.VerifyFFIObjectStillInScope("pt2", 0);
            thisTest.VerifyFFIObjectOutOfScope("pt3");
            
            
            
            
            
            
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Dispose22_MemberFunctionTwoArguments()
        {
            String code =
            @"              
import(""FFITarget.dll"");
pt2 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    
    
    
   
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    pt1 =DummyPoint.ByCoordinates(0, 0, 0);
    pt3 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
    
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            thisTest.VerifyFFIObjectOutOfScope("pt1");
            thisTest.VerifyFFIObjectStillInScope("pt2", 0);
            thisTest.VerifyFFIObjectOutOfScope("pt3");
            
            
            
            
            
            
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Dispose23_MemberFunctionTwoArguments()
        {
            String code =
            @"              
import(""FFITarget.dll"");
pt2 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    
    
    
   
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    pt1 =DummyPoint.ByCoordinates(0, 0, 0);
    pt3 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            thisTest.VerifyFFIObjectOutOfScope("pt1");
            thisTest.VerifyFFIObjectOutOfScope("pt3");
            
            
            
            
            
            
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Dispose24_MemberFunctionTwoArguments()
        {
            String code =
            @"              
import(""FFITarget.dll"");
pt2 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    
    
    
   
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    pt1 =DummyPoint.ByCoordinates(0, 0, 0);
    pt3 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            thisTest.VerifyFFIObjectOutOfScope("pt1");
            thisTest.VerifyFFIObjectStillInScope("pt2", 0);
            thisTest.VerifyFFIObjectOutOfScope("pt3");
            
            
            
            
            
            
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Dispose25_MemberFunctionTwoArguments()
        {
            String code =
            @"              
import(""FFITarget.dll"");
pt2 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    
    
    
   
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    pt1 =DummyPoint.ByCoordinates(0, 0, 0);
    pt3 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
    
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            thisTest.VerifyFFIObjectOutOfScope("pt1");
            thisTest.VerifyFFIObjectStillInScope("pt2", 0);
            thisTest.VerifyFFIObjectOutOfScope("pt3");
            
            
            
            
            
            
        }

        [Test]
        public void Dispose26_GlobalFunctionReturnArray()
        {
            String code =
            @"              
import(""FFITarget.dll"");
def foo :DummyPoint[] (p :DummyPoint[])
{
    return = p;
}
pt2 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    pt1 =DummyPoint.ByCoordinates(0, 0, 0);
    pt3 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
    test = foo(ptarr);
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            thisTest.VerifyFFIObjectOutOfScope("pt1");
            thisTest.VerifyFFIObjectStillInScope("pt2", 0);
            thisTest.VerifyFFIObjectOutOfScope("pt3");
            
            
        }

        [Test]
        public void Dispose27_GlobalFunctionReturnArray()
        {
            String code =
            @"              
import(""FFITarget.dll"");
def foo :DummyPoint[] (p :DummyPoint)
{
    return = p;
}
pt2 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    pt1 =DummyPoint.ByCoordinates(0, 0, 0);
    pt3 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
    test = foo(ptarr);
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            thisTest.VerifyFFIObjectOutOfScope("pt1");
            thisTest.VerifyFFIObjectStillInScope("pt2", 0);
            thisTest.VerifyFFIObjectOutOfScope("pt3");
            
            
        }

        [Test]
        public void Dispose28_GlobalFunctionReturnArray()
        {
            String code =
            @"              
import(""FFITarget.dll"");
def foo :DummyPoint (p :DummyPoint)
{
    return = p;
}
pt2 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    pt1 =DummyPoint.ByCoordinates(0, 0, 0);
    pt3 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
    test = foo(ptarr);
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            thisTest.VerifyFFIObjectOutOfScope("pt1");
            thisTest.VerifyFFIObjectStillInScope("pt2", 0);
            thisTest.VerifyFFIObjectOutOfScope("pt3");
            
            
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Dispose29_MemberFunctionReturnArray()
        {
            String code =
            @"              
import(""FFITarget.dll"");
pt2 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    
    
    
   
    pt1 =DummyPoint.ByCoordinates(0, 0, 0);
    pt3 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("pt1");
            thisTest.VerifyFFIObjectStillInScope("pt2", 0);
            thisTest.VerifyFFIObjectOutOfScope("pt3");
            
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            
            
            
            
            
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Dispose30_MemberFunctionReturnArray()
        {
            String code =
            @"              
import(""FFITarget.dll"");
pt2 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    
    
    
   
    pt1 =DummyPoint.ByCoordinates(0, 0, 0);
    pt3 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    arc1 = Arc.ByCenterPointRadiusAngle(pt1,5,0,180,vec1);
    arc2 = Arc.ByCenterPointRadiusAngle(pt2,5,0,180,vec2);
    arc3 = Arc.ByCenterPointRadiusAngle(pt3,5,0,180,vec3);
    arcarr = [ arc1, arc2, arc3 ];
    
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("pt1");
            thisTest.VerifyFFIObjectStillInScope("pt2", 0);
            thisTest.VerifyFFIObjectOutOfScope("pt3");
            
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            thisTest.VerifyFFIObjectOutOfScope("arc1");
            thisTest.VerifyFFIObjectOutOfScope("arc2");
            thisTest.VerifyFFIObjectOutOfScope("arc3");
            
            
            
            
            
            
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Dispose31_MemberFunctionReturnArray()
        {
            String code =
            @"              
import(""FFITarget.dll"");
pt2 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    
    
    
   
    pt1 =DummyPoint.ByCoordinates(0, 0, 0);
    pt3 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("pt1");
            thisTest.VerifyFFIObjectStillInScope("pt2", 0);
            thisTest.VerifyFFIObjectOutOfScope("pt3");
            
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            
            
            
            
            
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Dispose32_StaticFunctionReturnArray()
        {
            String code =
            @"              
import(""FFITarget.dll"");
pt2 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    
    
    
   
    pt1 =DummyPoint.ByCoordinates(0, 0, 0);
    pt3 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("pt1");
            thisTest.VerifyFFIObjectStillInScope("pt2", 0);
            thisTest.VerifyFFIObjectOutOfScope("pt3");
            
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            
            
            
            
            
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Dispose33_StaticFunctionReturnArray()
        {
            String code =
            @"              
import(""FFITarget.dll"");
pt2 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    
    
    
   
    pt1 =DummyPoint.ByCoordinates(0, 0, 0);
    pt3 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("pt1");
            thisTest.VerifyFFIObjectStillInScope("pt2", 0);
            thisTest.VerifyFFIObjectOutOfScope("pt3");
            
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            
            
            
            
            
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Dispose34_StaticFunctionReturnArray()
        {
            String code =
            @"              
import(""FFITarget.dll"");
pt2 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    
    
    
   
    pt1 =DummyPoint.ByCoordinates(0, 0, 0);
    pt3 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("pt1");
            thisTest.VerifyFFIObjectStillInScope("pt2", 0);
            thisTest.VerifyFFIObjectOutOfScope("pt3");
            
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            
            
            
            
            
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Dispose35_StaticFunctionReturnObject()
        {
            String code =
            @"              
import(""FFITarget.dll"");
pt2 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    
    
    
   
    pt1 =DummyPoint.ByCoordinates(0, 0, 0);
    pt3 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("pt1");
            thisTest.VerifyFFIObjectStillInScope("pt2", 0);
            thisTest.VerifyFFIObjectOutOfScope("pt3");
            
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            
            
            
            
            
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Dispose36_StaticFunctionReturnObject()
        {
            String code =
            @"              
import(""FFITarget.dll"");
pt2 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    
    
    
   
    pt1 =DummyPoint.ByCoordinates(0, 0, 0);
    pt3 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("pt1");
            thisTest.VerifyFFIObjectStillInScope("pt2", 0);
            thisTest.VerifyFFIObjectOutOfScope("pt3");
            
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            
            
            
            
            
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Dispose37_MemberFunctionReturnObject()
        {
            String code =
            @"              
import(""FFITarget.dll"");
pt2 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    
    
    
   
    pt1 =DummyPoint.ByCoordinates(0, 0, 0);
    pt3 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("pt1");
            thisTest.VerifyFFIObjectStillInScope("pt2", 0);
            thisTest.VerifyFFIObjectOutOfScope("pt3");
            
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            
            
            
            
            
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Dispose38_MemberFunctionReturnObject()
        {
            String code =
            @"              
import(""FFITarget.dll"");
pt2 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    
    
    
   
    pt1 =DummyPoint.ByCoordinates(0, 0, 0);
    pt3 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("pt1");
            thisTest.VerifyFFIObjectStillInScope("pt2", 0);
            thisTest.VerifyFFIObjectOutOfScope("pt3");
            
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            
            
            
            
            
        }

        [Test]
        public void Dispose39_GlobalFunctionReturnObject()
        {
            String code =
            @"              
import(""FFITarget.dll"");
def foo :DummyPoint (p :DummyPoint[])
    {
        return = p[0];
    }
pt2 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    
    
    
   
    pt1 =DummyPoint.ByCoordinates(0, 0, 0);
    pt3 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    test = foo(ptarr);
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("pt1");
            thisTest.VerifyFFIObjectStillInScope("pt2", 0);
            thisTest.VerifyFFIObjectOutOfScope("pt3");
            
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            
            
            
            
            
        }

        [Test]
        public void Dispose40_GlobalFunctionReturnObject()
        {
            String code =
            @"              
import(""FFITarget.dll"");
def foo :DummyPoint (p :DummyPoint)
    {
        return = p;
    }
pt2 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    
    
    
   
    pt1 =DummyPoint.ByCoordinates(0, 0, 0);
    pt3 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    test = foo(pt1);
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("pt1");
            thisTest.VerifyFFIObjectStillInScope("pt2", 0);
            thisTest.VerifyFFIObjectOutOfScope("pt3");
            
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            
            
            
            
            
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Dispose41_MemberFunctionReturnArray()
        {
            String code =
            @"              
import(""FFITarget.dll"");
pt3 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    
    
    
   
    pt1 =DummyPoint.ByCoordinates(0, 0, 0);
    pt2 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("pt1");
            thisTest.VerifyFFIObjectOutOfScope("pt2");
            thisTest.VerifyFFIObjectStillInScope("pt3", 0);
            
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            
            
            
            
            
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Dispose42_MemberFunctionReturnArray()
        {
            String code =
            @"              
import(""FFITarget.dll"");
pt3 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    
    
    
   
    pt1 =DummyPoint.ByCoordinates(0, 0, 0);
    pt2 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    arc1 = Arc.ByCenterPointRadiusAngle(pt1,5,0,180,vec1);
    arc2 = Arc.ByCenterPointRadiusAngle(pt2,5,0,180,vec2);
    arc3 = Arc.ByCenterPointRadiusAngle(pt3,5,0,180,vec3);
    arcarr = [ arc1, arc2, arc3 ];
    
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("pt1");
            thisTest.VerifyFFIObjectOutOfScope("pt2");
            thisTest.VerifyFFIObjectStillInScope("pt3", 0);
            
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            thisTest.VerifyFFIObjectOutOfScope("arc1");
            thisTest.VerifyFFIObjectOutOfScope("arc2");
            thisTest.VerifyFFIObjectOutOfScope("arc3");
            
            
            
            
            
            
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Dispose43_MemberFunctionReturnArray()
        {
            String code =
            @"              
import(""FFITarget.dll"");
pt3 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    
    
    
   
    pt1 =DummyPoint.ByCoordinates(0, 0, 0);
    pt2 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("pt1");
            thisTest.VerifyFFIObjectOutOfScope("pt2");
            thisTest.VerifyFFIObjectStillInScope("pt3", 0);
            
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            
            
            
            
            
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Dispose44_MemberFunctionReturnObject()
        {
            String code =
            @"              
import(""FFITarget.dll"");
pt3 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    
    
    
   
    pt1 =DummyPoint.ByCoordinates(0, 0, 0);
    pt2 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("pt1");
            thisTest.VerifyFFIObjectOutOfScope("pt2");
            thisTest.VerifyFFIObjectStillInScope("pt3", 0);
            
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            
            
            
            
            
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Dispose45_MemberFunctionReturnObject()
        {
            String code =
            @"              
import(""FFITarget.dll"");
pt3 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    
    
    
   
    pt1 =DummyPoint.ByCoordinates(0, 0, 0);
    pt2 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("pt1");
            thisTest.VerifyFFIObjectOutOfScope("pt2");
            thisTest.VerifyFFIObjectStillInScope("pt3", 0);
            
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            
            
            
            
            
        }

        [Test]
        public void Dispose46_GlobalFunctionReturnNewArray()
        {
            String code =
            @"              
import(""FFITarget.dll"");
def foo : DummyVector[] (p :DummyPoint[])
    {        
        vec = [Vector.ByCoordinates(1, 1, 1),Vector.ByCoordinates(1, 0, 0),Vector.ByCoordinates(0, 1, 0)]; 
        return = vec;
    }
pt3 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    pt1 =DummyPoint.ByCoordinates(0, 0, 0);
    pt2 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
    test = foo(ptarr);
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            thisTest.VerifyFFIObjectOutOfScope("pt1");
            thisTest.VerifyFFIObjectOutOfScope("pt2");
            thisTest.VerifyFFIObjectStillInScope("pt3", 0);
            
            
        }

        [Test]
        public void Dispose47_GlobalFunctionReturnNewArray()
        {
            String code =
            @"              
import(""FFITarget.dll"");
def foo : DummyVector[] (p :DummyPoint)
{        
    vec = DummyVector.ByCoordinates(1, 1, 1); 
    return = vec;
}
pt3 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    pt1 =DummyPoint.ByCoordinates(0, 0, 0);
    pt2 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
    test = foo(ptarr);
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            thisTest.VerifyFFIObjectOutOfScope("pt1");
            thisTest.VerifyFFIObjectOutOfScope("pt2");
            thisTest.VerifyFFIObjectStillInScope("pt3", 0);
            
            
        }

        [Test]
        public void Dispose48_GlobalFunctionReturnNewArray()
        {
            String code =
            @"              
import(""FFITarget.dll"");
def foo : DummyVector(p :DummyPoint)
{        
    vec = DummyVector.ByCoordinates(1, 1, 1); 
    return = vec;
}
pt3 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    pt1 =DummyPoint.ByCoordinates(0, 0, 0);
    pt2 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
    test = foo(ptarr);
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            thisTest.VerifyFFIObjectOutOfScope("pt1");
            thisTest.VerifyFFIObjectOutOfScope("pt2");
            thisTest.VerifyFFIObjectStillInScope("pt3", 0);
            
            
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Dispose49_MemberFunctionReturnNewArray()
        {
            String code =
            @"              
import(""FFITarget.dll"");
pt3 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    
    
    
   
    pt1 =DummyPoint.ByCoordinates(0, 0, 0);
    pt2 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("pt1");
            thisTest.VerifyFFIObjectOutOfScope("pt2");
            thisTest.VerifyFFIObjectStillInScope("pt3", 0);
            
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            
            
            
            
            
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Dispose50_MemberFunctionReturnNewArray()
        {
            String code =
            @"              
import(""FFITarget.dll"");
pt3 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    
    
    
   
    pt1 =DummyPoint.ByCoordinates(0, 0, 0);
    pt2 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    arc1 = Arc.ByCenterPointRadiusAngle(pt1,5,0,180,vec1);
    arc2 = Arc.ByCenterPointRadiusAngle(pt2,5,0,180,vec2);
    arc3 = Arc.ByCenterPointRadiusAngle(pt3,5,0,180,vec3);
    arcarr = [ arc1, arc2, arc3 ];
    
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("pt1");
            thisTest.VerifyFFIObjectOutOfScope("pt2");
            thisTest.VerifyFFIObjectStillInScope("pt3", 0);
            
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            thisTest.VerifyFFIObjectOutOfScope("arc1");
            thisTest.VerifyFFIObjectOutOfScope("arc2");
            thisTest.VerifyFFIObjectOutOfScope("arc3");
            
            
            
            
            
            
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Dispose51_MemberFunctionReturnNewArray()
        {
            String code =
            @"              
import(""FFITarget.dll"");
pt3 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    
    
    
   
    pt1 =DummyPoint.ByCoordinates(0, 0, 0);
    pt2 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("pt1");
            thisTest.VerifyFFIObjectOutOfScope("pt2");
            thisTest.VerifyFFIObjectStillInScope("pt3", 0);
            
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            
            
            
            
            
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Dispose52_StaticFunctionReturnNewArray()
        {
            String code =
            @"              
import(""FFITarget.dll"");
pt3 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    
    
    
   
    pt1 =DummyPoint.ByCoordinates(0, 0, 0);
    pt2 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("pt1");
            thisTest.VerifyFFIObjectOutOfScope("pt2");
            thisTest.VerifyFFIObjectStillInScope("pt3", 0);
            
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            
            
            
            
            
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Dispose53_StaticFunctionReturnNewArray()
        {
            String code =
            @"              
import(""FFITarget.dll"");
pt3 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    
    
    
   
    pt1 =DummyPoint.ByCoordinates(0, 0, 0);
    pt2 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("pt1");
            thisTest.VerifyFFIObjectOutOfScope("pt2");
            thisTest.VerifyFFIObjectStillInScope("pt3", 0);
            
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            
            
            
            
            
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Dispose54_StaticFunctionReturnNewArray()
        {
            String code =
            @"              
import(""FFITarget.dll"");
pt3 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    
    
    
   
    pt1 =DummyPoint.ByCoordinates(0, 0, 0);
    pt2 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("pt1");
            thisTest.VerifyFFIObjectOutOfScope("pt2");
            thisTest.VerifyFFIObjectStillInScope("pt3", 0);
            
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            
            
            
            
            
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Dispose55_StaticFunctionReturnNewObject()
        {
            String code =
            @"              
import(""FFITarget.dll"");
pt3 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    
    
    
   
    pt1 =DummyPoint.ByCoordinates(0, 0, 0);
    pt2 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("pt1");
            thisTest.VerifyFFIObjectOutOfScope("pt2");
            thisTest.VerifyFFIObjectStillInScope("pt3", 0);
            
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            
            
            
            
            
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Dispose56_StaticFunctionReturnNewObject()
        {
            String code =
            @"              
import(""FFITarget.dll"");
pt3 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    
    
    
   
    pt1 =DummyPoint.ByCoordinates(0, 0, 0);
    pt2 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("pt1");
            thisTest.VerifyFFIObjectOutOfScope("pt2");
            thisTest.VerifyFFIObjectStillInScope("pt3", 0);
            
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            
            
            
            
            
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Dispose57_MemberFunctionReturnNewObject()
        {
            String code =
            @"              
import(""FFITarget.dll"");
pt3 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    
    
    
   
    pt1 =DummyPoint.ByCoordinates(0, 0, 0);
    pt2 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("pt1");
            thisTest.VerifyFFIObjectOutOfScope("pt2");
            thisTest.VerifyFFIObjectStillInScope("pt3", 0);
            
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            
            
            
            
            
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Dispose58_MemberFunctionReturnNewObject()
        {
            String code =
            @"              
import(""FFITarget.dll"");
pt3 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    
    
    
   
    pt1 =DummyPoint.ByCoordinates(0, 0, 0);
    pt2 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("pt1");
            thisTest.VerifyFFIObjectOutOfScope("pt2");
            thisTest.VerifyFFIObjectStillInScope("pt3", 0);
            
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            
            
            
            
            
        }

        [Test]
        public void Dispose59_GlobalFunctionReturnNewObject()
        {
            String code =
            @"              
import(""FFITarget.dll"");
def foo : DummyVector (p :DummyPoint[])
    {        
        vec = [Vector.ByCoordinates(1, 1, 1),Vector.ByCoordinates(1, 0, 0),Vector.ByCoordinates(0, 1, 0)]; 
        return = vec[0];
    }
pt3 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    
    
    
   
    pt1 =DummyPoint.ByCoordinates(0, 0, 0);
    pt2 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    test = foo(ptarr);
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("pt1");
            thisTest.VerifyFFIObjectOutOfScope("pt2");
            thisTest.VerifyFFIObjectStillInScope("pt3", 0);
            
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            
            
            
            
            
        }

        [Test]
        public void Dispose60_GlobalFunctionReturnNewObject()
        {
            String code =
            @"              
import(""FFITarget.dll"");
def foo : DummyVector (p :DummyPoint)
    {
        vec = DummyVector.ByCoordinates(1,1,1);
        return = vec;
    }
pt3 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    
    
    
   
    pt1 =DummyPoint.ByCoordinates(0, 0, 0);
    pt2 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    test = foo(pt1);
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("pt1");
            thisTest.VerifyFFIObjectOutOfScope("pt2");
            thisTest.VerifyFFIObjectStillInScope("pt3", 0);
            
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            
            
            
            
            
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Dispose61_MemberFunctionReturnNewArray()
        {
            String code =
            @"              
import(""FFITarget.dll"");
pt3 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    
    
    
   
    pt1 =DummyPoint.ByCoordinates(0, 0, 0);
    pt2 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("pt1");
            thisTest.VerifyFFIObjectOutOfScope("pt2");
            thisTest.VerifyFFIObjectStillInScope("pt3", 0);
            
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            
            
            
            
            
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Dispose62_MemberFunctionReturnNewArray()
        {
            String code =
            @"              
import(""FFITarget.dll"");
pt3 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    
    
    
   
    pt1 =DummyPoint.ByCoordinates(0, 0, 0);
    pt2 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    arc1 = Arc.ByCenterPointRadiusAngle(pt1,5,0,180,vec1);
    arc2 = Arc.ByCenterPointRadiusAngle(pt2,5,0,180,vec2);
    arc3 = Arc.ByCenterPointRadiusAngle(pt3,5,0,180,vec3);
    arcarr = [ arc1, arc2, arc3 ];
    
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("pt1");
            thisTest.VerifyFFIObjectOutOfScope("pt2");
            thisTest.VerifyFFIObjectStillInScope("pt3", 0);
            
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            thisTest.VerifyFFIObjectOutOfScope("arc1");
            thisTest.VerifyFFIObjectOutOfScope("arc2");
            thisTest.VerifyFFIObjectOutOfScope("arc3");
            
            
            
            
            
            
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Dispose63_MemberFunctionReturnNewArray()
        {
            String code =
            @"              
import(""FFITarget.dll"");
pt3 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    
    
    
   
    pt1 =DummyPoint.ByCoordinates(0, 0, 0);
    pt2 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("pt1");
            thisTest.VerifyFFIObjectOutOfScope("pt2");
            thisTest.VerifyFFIObjectStillInScope("pt3", 0);
            
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            
            
            
            
            
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Dispose64_MemberFunctionReturnNewObject()
        {
            String code =
            @"              
import(""FFITarget.dll"");
pt3 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    
    
    
   
    pt1 =DummyPoint.ByCoordinates(0, 0, 0);
    pt2 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("pt1");
            thisTest.VerifyFFIObjectOutOfScope("pt2");
            thisTest.VerifyFFIObjectStillInScope("pt3", 0);
            
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            
            
            
            
            
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Dispose65_MemberFunctionReturnNewObject()
        {
            String code =
            @"              
import(""FFITarget.dll"");
pt3 =DummyPoint.ByCoordinates(0, 0, 0);
[Associative]
{
    
    
    
   
    pt1 =DummyPoint.ByCoordinates(0, 0, 0);
    pt2 =DummyPoint.ByCoordinates(0, 0, 0);
    ptarr = [ pt1, pt2, pt3 ];
    vec1 = DummyVector.ByCoordinates(1, 1, 1);
    vec2 = DummyVector.ByCoordinates(1, 0, 0);
    vec3 = DummyVector.ByCoordinates(0, 1, 0);
    vecarr = [ vec1, vec2, vec3 ];
    
}
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("pt1");
            thisTest.VerifyFFIObjectOutOfScope("pt2");
            thisTest.VerifyFFIObjectStillInScope("pt3", 0);
            
            thisTest.VerifyFFIObjectOutOfScope("vec1");
            thisTest.VerifyFFIObjectOutOfScope("vec2");
            thisTest.VerifyFFIObjectOutOfScope("vec3");
            
            
            
            
            
            
        }


        [Test]
        public void Dispose67_PointInScope()
        {
            String code =
            @"   
     
import (""FFITarget.dll"");
class myPoint
{
    p :DummyPoint;
	constructor create()
    {
        p =DummyPoint.ByCoordinates(0, 0, 0);
    }
}
pt = myPoint.create();
[Associative]
{
    a = pt.p;
}
c = pt.p;
carr = [c.X,c.Y,c.Z];
            ";
            thisTest.RunScriptSource(code);
            object[] a = new object[] { 0.0, 0.0, 0.0 };
            thisTest.Verify("carr", a);
        }


        [Test]
        public void Dispose_FFITarget()
        {
            String code =
            @"              
import(""FFITarget.dll"");
[Associative]
{
 x = DisposeTracer.DisposeTracer();
}
__GC();
s1 = DisposeTracer.DisposeCount;
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("x");
            
            thisTest.Verify("s1", 1);
        }

        [Test]
        public void Dispose_FFITarget_Inherited()
        {
            String code =
            @"              
import(""FFITarget.dll"");
[Associative]
{
 x = DerivedDisposeTracer.DerivedDisposeTracer();
}
__GC();
s1 = DerivedDisposeTracer.DisposeCount;
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("x");
            
            thisTest.Verify("s1", 1);
        }

        [Test]
        public void Dispose_FFITarget_Overridden()
        {
            String code =
            @"              
import(""FFITarget.dll"");
[Associative]
{
 x = AbstractDerivedDisposeTracer2.AbstractDerivedDisposeTracer2();
}
__GC();
s1 = AbstractDerivedDisposeTracer2.DisposeCount;
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyFFIObjectOutOfScope("x");
            
            thisTest.Verify("s1", 1);
        }


        [Test]
        [Category("Trace")]
        public void InheritedLiveRunnerDispose()
        {
            string setupCode =
            @"import(""FFITarget.dll""); 
x = AbstractDerivedDisposeTracer2.AbstractDerivedDisposeTracer2(); 
s1 = AbstractDerivedDisposeTracer2.DisposeCount;
";

            // Create 2 CBNs

            List<Subtree> added = new List<Subtree>();


            // Simulate a new new CBN
            Guid guid1 = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, setupCode));

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            AssertValue("s1", 0);


            // Simulate a new new CBN
            Guid guid2 = System.Guid.NewGuid();
            added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2,
                "x = null;__GC();" +
                "s2 = AbstractDerivedDisposeTracer2.DisposeCount; "));


            syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            AssertValue("s2", 1);
        }

        [Test]
        [Category("Trace")]
        public void IntermediatePatch()
        {
            string setupCode =
            @"import(""FFITarget.dll""); 
x = DerivedDisposeTracer.DerivedDisposeTracer(); 
";

            // Create 2 CBNs

            List<Subtree> added = new List<Subtree>();


            // Simulate a new new CBN
            Guid guid1 = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, setupCode));

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);


            // Simulate a new new CBN
            Guid guid2 = System.Guid.NewGuid();
            added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2,
                "x = null;" ));


            syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

        }

        [Test]
        public void CleanupAfterPropertyAccess()
        {
            String code =
            @"import(""FFITarget.dll""); 
x = AbstractDerivedDisposeTracer2.AbstractDerivedDisposeTracer2(1);
so = x.I;
s1 = AbstractDerivedDisposeTracer2.DisposeCount;
x = null;
__GC();
s1 = AbstractDerivedDisposeTracer2.DisposeCount;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("s1", 1);
        }

        [Test]
        [Category("Trace")]
        public void CleanupAfterPropertyAccessLR()
        {
            string setupCode =
            @"import(""FFITarget.dll""); 
x = AbstractDerivedDisposeTracer2.AbstractDerivedDisposeTracer2(1);
so = x.I;
s1 = AbstractDerivedDisposeTracer2.DisposeCount;
";

            // Create 2 CBNs

            List<Subtree> added = new List<Subtree>();


            // Simulate a new new CBN
            Guid guid1 = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, setupCode));

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            AssertValue("s1", 0);


            // Simulate a new new CBN
            Guid guid2 = System.Guid.NewGuid();
            added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2,"x = null;" ));
            syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);


            // Simulate a new new CBN
            Guid guid3 = System.Guid.NewGuid();
            added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid3, "__GC();s2 = AbstractDerivedDisposeTracer2.DisposeCount; "));
            syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            AssertValue("s2", 1);
        }


        //MOVE THIS TEST
        [Test]
        [Category("Trace")]
        public void InheritenceLiveRunner()
        {
            string setupCode =
            @"import(""FFITarget.dll"");
o = InheritenceDriver.Gen();

o.Y = 4;
oy = o.Y;
";

            // Create 2 CBNs

            List<Subtree> added = new List<Subtree>();


            // Simulate a new new CBN
            Guid guid1 = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, setupCode));

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            AssertValue("oy", 4);


        }

        [Test]
        public void DisposeDispoableObject()
        {
            string code=
@"
import(""FFITarget.dll"");
tracer = HiddenDisposeTracer();
count1 = tracer.DisposeCount;
[Associative]
{
    disposer1 = tracer.GetHiddenDisposer();
    disposer1 = null; 
    disposer2 = tracer.GetHiddenDisposer();
    disposer2 = null;
}
__GC();
count2 = tracer.DisposeCount;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("count1", 0);
            thisTest.Verify("count2", 2);
        }

        [Test]
        public void DisposeMultipleDispoableObject()
        {
            string code =
@"
import(""FFITarget.dll"");
tracer1 = HiddenDisposeTracer();
tracer2 = HiddenDisposeTracer();
count1 = tracer1.DisposeCount;
count2 = tracer2.DisposeCount;
[Associative]
{
    disposer1a = tracer1.GetHiddenDisposer();
    disposer1a = null;
    disposer1b = tracer1.GetHiddenDisposer();
    disposer1b = null;
    disposer2a = tracer2.GetHiddenDisposer();
    disposer2a = null;
    disposer2b = tracer2.GetHiddenDisposer();
    disposer2b = null;
    disposer2c = tracer2.GetHiddenDisposer();
    disposer2c = null;
}
__GC();
d1 = tracer1.DisposeCount;
d2 = tracer2.DisposeCount;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("count1", 0);
            thisTest.Verify("count2", 0);

            thisTest.Verify("d1", 2);
            thisTest.Verify("d2", 3);
        }
        [Test]
        public void DisposeEnumWrapper()
        {
            string code =
                @"
import(""FFITarget.dll"");
x = Days.Monday;
__GC();
x = Days.Monday;
__GC();
x = Days.Monday;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", Days.Monday);
        }

        private void AssertValue(string varname, object value)
        {
            var mirror = astLiveRunner.InspectNodeValue(varname);
            MirrorData data = mirror.GetData();
            object svValue = data.Data;
            if (value is double)
            {
                Assert.AreEqual((double)svValue, Convert.ToDouble(value));
            }
            else if (value is int)
            {
                Assert.AreEqual(Convert.ToInt64(svValue), Convert.ToInt64(value));
            }
            else if (value is bool)
            {
                Assert.AreEqual((bool)svValue, Convert.ToBoolean(value));
            }
        }
    }
}
