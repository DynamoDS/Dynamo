using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Lang;
using ProtoTest.TD;
using ProtoTestFx.TD;
namespace ProtoTest.Associative
{
    class ReferenceCount : ProtoTestBase
    {
        [Test]
        public void TestReferenceCount_BaseCase01()
        {
            string code = @"class A{}[Associative]{    a = A.A();    as = {a};}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a", 0);
        }

        [Test]
        public void TestReferenceCount01_NoFunctionCall()
        {
            string code =
                @"            class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }    def foo(b:B)     {        return = null;    }    def bar(b:B[])    {        return = null;    }    static def ding(b:B)    {        return = null;    }    static def dong(b:B[])    {        return = null;    }    }class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}def foo(b:B){    return = null;}def bar(b:B[]){    return = null;}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();b1 = B.B();b2 = B.B();b3 = B.B();as = {a1, a2, a3};bs = {b1, b2, b3};a = A.A();b = B.B();}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("a", 0);
            thisTest.VerifyReferenceCount("b", 0);
        }

        [Test]
        public void TestReferenceCount01_NoFunctionCall_Dispose()
        {
            string code =
                @"            class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }    def foo(b:B)     {        return = null;    }    def bar(b:B[])    {        return = null;    }    static def ding(b:B)    {        return = null;    }    static def dong(b:B[])    {        return = null;    }    }class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}def foo(b:B){    return = null;}def bar(b:B[]){    return = null;}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();b1 = B.B();b2 = B.B();b3 = B.B();as = {a1, a2, a3};bs = {b1, b2, b3};a = A.A();b = B.B();}aDispose = A.count;bDispose = B.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
        }

        [Test]
        public void TestReferenceCount02_FunctionNonArray()
        {
            string code =
                @"            class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }    def foo(b:B)     {        return = null;    }    def bar(b:B[])    {        return = null;    }    static def ding(b:B)    {        return = null;    }    static def dong(b:B[])    {        return = null;    }    }class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}def foo(b:B){    return = null;}def bar(b:B[]){    return = null;}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();b1 = B.B();b2 = B.B();b3 = B.B();as = {a1, a2, a3};bs = {b1, b2, b3};a = A.A();b = B.B();r = foo(b);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("a", 0);
            thisTest.VerifyReferenceCount("b", 0);
        }

        [Test]
        public void TestReferenceCount02_FunctionNonArray_Dispose()
        {
            string code =
                @"            class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }    def foo(b:B)     {        return = null;    }    def bar(b:B[])    {        return = null;    }    static def ding(b:B)    {        return = null;    }    static def dong(b:B[])    {        return = null;    }    }class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}def foo(b:B){    return = null;}def bar(b:B[]){    return = null;}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();b1 = B.B();b2 = B.B();b3 = B.B();as = {a1, a2, a3};bs = {b1, b2, b3};a = A.A();b = B.B();r = foo(b);}aDispose = A.count;bDispose = B.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
        }

        [Test]
        public void TestReferenceCount03_FunctionReplication()
        {
            string code =
                @"            class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }    def foo(b:B)     {        return = null;    }    def bar(b:B[])    {        return = null;    }    static def ding(b:B)    {        return = null;    }    static def dong(b:B[])    {        return = null;    }    }class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}def foo(b:B){    return = null;}def bar(b:B[]){    return = null;}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();b1 = B.B();b2 = B.B();b3 = B.B();as = {a1, a2, a3};bs = {b1, b2, b3};a = A.A();b = B.B();r = foo(bs);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("a", 0);
            thisTest.VerifyReferenceCount("b", 0);
        }

        [Test]
        public void TestReferenceCount03_FunctionReplication_Dispose()
        {
            string code =
                @"            class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }    def foo(b:B)     {        return = null;    }    def bar(b:B[])    {        return = null;    }    static def ding(b:B)    {        return = null;    }    static def dong(b:B[])    {        return = null;    }    }class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}def foo(b:B){    return = null;}def bar(b:B[]){    return = null;}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();b1 = B.B();b2 = B.B();b3 = B.B();as = {a1, a2, a3};bs = {b1, b2, b3};a = A.A();b = B.B();r = foo(bs);}aDispose = A.count;bDispose = B.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
        }

        [Test]
        public void TestReferenceCount04_FunctionArray()
        {
            string code =
                @"            class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }    def foo(b:B)     {        return = null;    }    def bar(b:B[])    {        return = null;    }    static def ding(b:B)    {        return = null;    }    static def dong(b:B[])    {        return = null;    }    }class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}def foo(b:B){    return = null;}def bar(b:B[]){    return = null;}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();b1 = B.B();b2 = B.B();b3 = B.B();as = {a1, a2, a3};bs = {b1, b2, b3};a = A.A();b = B.B();r = bar(bs);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("a", 0);
            thisTest.VerifyReferenceCount("b", 0);
        }

        [Test]
        public void TestReferenceCount04_FunctionArray_Dispose()
        {
            string code =
                @"            class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }    def foo(b:B)     {        return = null;    }    def bar(b:B[])    {        return = null;    }    static def ding(b:B)    {        return = null;    }    static def dong(b:B[])    {        return = null;    }    }class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}def foo(b:B){    return = null;}def bar(b:B[]){    return = null;}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();b1 = B.B();b2 = B.B();b3 = B.B();as = {a1, a2, a3};bs = {b1, b2, b3};a = A.A();b = B.B();r = bar(bs);}aDispose = A.count;bDispose = B.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
        }

        [Test]
        public void TestReferenceCount05_StaticFunctionNonArray()
        {
            string code =
                @"            class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }    def foo(b:B)     {        return = null;    }    def bar(b:B[])    {        return = null;    }    static def ding(b:B)    {        return = null;    }    static def dong(b:B[])    {        return = null;    }    }class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}def foo(b:B){    return = null;}def bar(b:B[]){    return = null;}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();b1 = B.B();b2 = B.B();b3 = B.B();as = {a1, a2, a3};bs = {b1, b2, b3};a = A.A();b = B.B();r = A.ding(b);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("a", 0);
            thisTest.VerifyReferenceCount("b", 0);
        }

        [Test]
        public void TestReferenceCount05_StaticFunctionNonArray_Dispose()
        {
            string code =
                @"            class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }    def foo(b:B)     {        return = null;    }    def bar(b:B[])    {        return = null;    }    static def ding(b:B)    {        return = null;    }    static def dong(b:B[])    {        return = null;    }    }class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}def foo(b:B){    return = null;}def bar(b:B[]){    return = null;}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();b1 = B.B();b2 = B.B();b3 = B.B();as = {a1, a2, a3};bs = {b1, b2, b3};a = A.A();b = B.B();r = A.ding(b);}aDispose = A.count;bDispose = B.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
        }

        [Test]
        public void TestReferenceCount06_StaticFunctionReplication()
        {
            string code =
                @"            class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }    def foo(b:B)     {        return = null;    }    def bar(b:B[])    {        return = null;    }    static def ding(b:B)    {        return = null;    }    static def dong(b:B[])    {        return = null;    }    }class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}def foo(b:B){    return = null;}def bar(b:B[]){    return = null;}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();b1 = B.B();b2 = B.B();b3 = B.B();as = {a1, a2, a3};bs = {b1, b2, b3};a = A.A();b = B.B();r = A.ding(bs);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("a", 0);
            thisTest.VerifyReferenceCount("b", 0);
        }

        [Test]
        public void TestReferenceCount06_StaticFunctionReplication_Dispose()
        {
            string code =
                @"            class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }    def foo(b:B)     {        return = null;    }    def bar(b:B[])    {        return = null;    }    static def ding(b:B)    {        return = null;    }    static def dong(b:B[])    {        return = null;    }    }class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}def foo(b:B){    return = null;}def bar(b:B[]){    return = null;}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();b1 = B.B();b2 = B.B();b3 = B.B();as = {a1, a2, a3};bs = {b1, b2, b3};a = A.A();b = B.B();r = A.ding(bs);}aDispose = A.count;bDispose = B.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
        }

        [Test]
        public void TestReferenceCount07_StaticFunctionArray()
        {
            string code =
                @"            class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }    def foo(b:B)     {        return = null;    }    def bar(b:B[])    {        return = null;    }    static def ding(b:B)    {        return = null;    }    static def dong(b:B[])    {        return = null;    }    }class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}def foo(b:B){    return = null;}def bar(b:B[]){    return = null;}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();b1 = B.B();b2 = B.B();b3 = B.B();as = {a1, a2, a3};bs = {b1, b2, b3};a = A.A();b = B.B();r = A.dong(bs);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("a", 0);
            thisTest.VerifyReferenceCount("b", 0);
        }

        [Test]
        public void TestReferenceCount07_StaticFunctionArray_Dispose()
        {
            string code =
                @"            class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }    def foo(b:B)     {        return = null;    }    def bar(b:B[])    {        return = null;    }    static def ding(b:B)    {        return = null;    }    static def dong(b:B[])    {        return = null;    }    }class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}def foo(b:B){    return = null;}def bar(b:B[]){    return = null;}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();b1 = B.B();b2 = B.B();b3 = B.B();as = {a1, a2, a3};bs = {b1, b2, b3};a = A.A();b = B.B();r = A.dong(bs);}aDispose = A.count;bDispose = B.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
        }

        [Test]
        public void TestReferenceCount08_MemFunctionNonArray()
        {
            string code =
                @"            class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }    def foo(b:B)     {        return = null;    }    def bar(b:B[])    {        return = null;    }    static def ding(b:B)    {        return = null;    }    static def dong(b:B[])    {        return = null;    }    }class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}def foo(b:B){    return = null;}def bar(b:B[]){    return = null;}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();b1 = B.B();b2 = B.B();b3 = B.B();as = {a1, a2, a3};bs = {b1, b2, b3};a = A.A();b = B.B();r = a.foo(b);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("a", 0);
            thisTest.VerifyReferenceCount("b", 0);
        }

        [Test]
        public void TestReferenceCount08_MemFunctionNonArray_Dispose()
        {
            string code =
                @"            class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }    def foo(b:B)     {        return = null;    }    def bar(b:B[])    {        return = null;    }    static def ding(b:B)    {        return = null;    }    static def dong(b:B[])    {        return = null;    }    }class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}def foo(b:B){    return = null;}def bar(b:B[]){    return = null;}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();b1 = B.B();b2 = B.B();b3 = B.B();as = {a1, a2, a3};bs = {b1, b2, b3};a = A.A();b = B.B();r = a.foo(b);}aDispose = A.count;bDispose = B.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
        }

        [Test]
        public void TestReferenceCount09_MemFunctionReplication()
        {
            string code =
                @"            class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }    def foo(b:B)     {        return = null;    }    def bar(b:B[])    {        return = null;    }    static def ding(b:B)    {        return = null;    }    static def dong(b:B[])    {        return = null;    }    }class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}def foo(b:B){    return = null;}def bar(b:B[]){    return = null;}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();b1 = B.B();b2 = B.B();b3 = B.B();as = {a1, a2, a3};bs = {b1, b2, b3};a = A.A();b = B.B();r = a.foo(bs);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("a", 0);
            thisTest.VerifyReferenceCount("b", 0);
        }

        [Test]
        public void TestReferenceCount09_MemFunctionReplication_Dispose()
        {
            string code =
                @"            class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }    def foo(b:B)     {        return = null;    }    def bar(b:B[])    {        return = null;    }    static def ding(b:B)    {        return = null;    }    static def dong(b:B[])    {        return = null;    }    }class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}def foo(b:B){    return = null;}def bar(b:B[]){    return = null;}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();b1 = B.B();b2 = B.B();b3 = B.B();as = {a1, a2, a3};bs = {b1, b2, b3};a = A.A();b = B.B();r = a.foo(bs);}aDispose = A.count;bDispose = B.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
        }

        [Test]
        public void TestReferenceCount10_MemFunctionArray()
        {
            string code =
                @"            class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }    def foo(b:B)     {        return = null;    }    def bar(b:B[])    {        return = null;    }    static def ding(b:B)    {        return = null;    }    static def dong(b:B[])    {        return = null;    }    }class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}def foo(b:B){    return = null;}def bar(b:B[]){    return = null;}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();b1 = B.B();b2 = B.B();b3 = B.B();as = {a1, a2, a3};bs = {b1, b2, b3};a = A.A();b = B.B();r = a.bar(bs);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("a", 0);
            thisTest.VerifyReferenceCount("b", 0);
        }

        [Test]
        public void TestReferenceCount10_MemFunctionArray_Dispose()
        {
            string code =
                @"            class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }    def foo(b:B)     {        return = null;    }    def bar(b:B[])    {        return = null;    }    static def ding(b:B)    {        return = null;    }    static def dong(b:B[])    {        return = null;    }    }class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}def foo(b:B){    return = null;}def bar(b:B[]){    return = null;}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();b1 = B.B();b2 = B.B();b3 = B.B();as = {a1, a2, a3};bs = {b1, b2, b3};a = A.A();b = B.B();r = a.bar(bs);}aDispose = A.count;bDispose = B.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
        }

        [Test]
        public void TestReferenceCount11_ReplicationNonArray()
        {
            string code =
                @"     class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }    def foo(b:B)     {        return = null;    }    def bar(b:B[])    {        return = null;    }    static def ding(b:B)    {        return = null;    }    static def dong(b:B[])    {        return = null;    }    }class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}def foo(b:B){    return = null;}def bar(b:B[]){    return = null;}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();b1 = B.B();b2 = B.B();b3 = B.B();as = {a1, a2, a3};bs = {b1, b2, b3};a = A.A();b = B.B();r = as.foo(b);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("a", 0);
            thisTest.VerifyReferenceCount("b", 0);
        }

        [Test]
        public void TestReferenceCount11_ReplicationNonArray_Dispose()
        {
            string code =
                @"     class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }    def foo(b:B)     {        return = null;    }    def bar(b:B[])    {        return = null;    }    static def ding(b:B)    {        return = null;    }    static def dong(b:B[])    {        return = null;    }    }class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}def foo(b:B){    return = null;}def bar(b:B[]){    return = null;}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();b1 = B.B();b2 = B.B();b3 = B.B();as = {a1, a2, a3};bs = {b1, b2, b3};a = A.A();b = B.B();r = as.foo(b);}aDispose = A.count;bDispose = B.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
        }

        [Test]
        public void TestReferenceCount12_ReplicationReplication()
        {
            string code =
                @"     class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }    def foo(b:B)     {        return = null;    }    def bar(b:B[])    {        return = null;    }    static def ding(b:B)    {        return = null;    }    static def dong(b:B[])    {        return = null;    }    }class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}def foo(b:B){    return = null;}def bar(b:B[]){    return = null;}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();b1 = B.B();b2 = B.B();b3 = B.B();as = {a1, a2, a3};bs = {b1, b2, b3};a = A.A();b = B.B();r = as.foo(bs);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("a", 0);
            thisTest.VerifyReferenceCount("b", 0);
        }

        [Test]
        public void TestReferenceCount12_ReplicationReplication_Dispose()
        {
            string code =
                @"     class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }    def foo(b:B)     {        return = null;    }    def bar(b:B[])    {        return = null;    }    static def ding(b:B)    {        return = null;    }    static def dong(b:B[])    {        return = null;    }    }class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}def foo(b:B){    return = null;}def bar(b:B[]){    return = null;}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();b1 = B.B();b2 = B.B();b3 = B.B();as = {a1, a2, a3};bs = {b1, b2, b3};a = A.A();b = B.B();r = as.foo(bs);}aDispose = A.count;bDispose = B.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
        }

        [Test]
        public void TestReferenceCount13_ReplicationArray()
        {
            string code =
                @"     class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }    def foo(b:B)     {        return = null;    }    def bar(b:B[])    {        return = null;    }    static def ding(b:B)    {        return = null;    }    static def dong(b:B[])    {        return = null;    }    }class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}def foo(b:B){    return = null;}def bar(b:B[]){    return = null;}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();b1 = B.B();b2 = B.B();b3 = B.B();as = {a1, a2, a3};bs = {b1, b2, b3};a = A.A();b = B.B();r = as.bar(bs);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("a", 0);
            thisTest.VerifyReferenceCount("b", 0);
        }

        [Test]
        public void TestReferenceCount13_ReplicationArray_Dispose()
        {
            string code =
                @"     class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }    def foo(b:B)     {        return = null;    }    def bar(b:B[])    {        return = null;    }    static def ding(b:B)    {        return = null;    }    static def dong(b:B[])    {        return = null;    }    }class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}def foo(b:B){    return = null;}def bar(b:B[]){    return = null;}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();b1 = B.B();b2 = B.B();b3 = B.B();as = {a1, a2, a3};bs = {b1, b2, b3};a = A.A();b = B.B();r = as.bar(bs);}aDispose = A.count;bDispose = B.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
        }

        [Test]
        public void TestReferenceCount14_GlobalFunctionTwoArguments()
        {
            string code =
                @"     class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}def foo(a: A, b:B){    return = null;}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};r = foo(as, bs);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
        }

        [Test]
        public void TestReferenceCount14_GlobalFunctionTwoArguments_Dispose()
        {
            string code =
                @"     class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}def foo(a: A, b:B){    return = null;}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};r = foo(as, bs);}aDispose = A.count;bDispose = B.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
        }

        [Test]
        public void TestReferenceCount15_GlobalFunctionTwoArguments()
        {
            string code =
                @"     class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    } }class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}def foo(a: A, b:B){    return = null;}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b = B.B();r = foo(as, b);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("b", 0);
        }

        [Test]
        public void TestReferenceCount15_GlobalFunctionTwoArguments_Dispose()
        {
            string code =
                @"     class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    } }class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}def foo(a: A, b:B){    return = null;}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b = B.B();r = foo(as, b);}aDispose = A.count;bDispose = B.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
        }

        [Test]
        public void TestReferenceCount16_GlobalFunctionTwoArguments()
        {
            string code =
                @"   class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    } }class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}def foo(a: A, b:B[]){    return = null;}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};r = foo(as, bs);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
        }

        [Test]
        public void TestReferenceCount16_GlobalFunctionTwoArguments_Dispose()
        {
            string code =
                @"   class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    } }class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}def foo(a: A, b:B[]){    return = null;}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};r = foo(as, bs);}aDispose = A.count;bDispose = B.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
        }

        [Test]
        public void TestReferenceCount17_StaticFunctionTwoArguments()
        {
            string code =
                @"   class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     static def ding(a:A, b:B)    {         return = null;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};r = A.ding(as, bs);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
        }

        [Test]
        public void TestReferenceCount17_StaticFunctionTwoArguments_Dispose()
        {
            string code =
                @"   class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     static def ding(a:A, b:B)    {         return = null;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};r = A.ding(as, bs);}aDispose = A.count;bDispose = B.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
        }

        [Test]
        public void TestReferenceCount18_StaticFunctionTwoArguments()
        {
            string code =
                @"   class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     static def ding(a:A, b:B)    {         return = null;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b = B.B();r = A.ding(as, b);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("b", 0);
        }

        [Test]
        public void TestReferenceCount18_StaticFunctionTwoArguments_Dispose()
        {
            string code =
                @"   class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     static def ding(a:A, b:B)    {         return = null;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b = B.B();r = A.ding(as, b);}aDispose = A.count;bDispose = B.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
        }

        [Test]
        public void TestReferenceCount19_StaticFunctionTwoArguments()
        {
            string code =
                @"   class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     static def ding(a:A, b:B[])    {         return = null;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};r = A.ding(as, bs);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
        }

        [Test]
        public void TestReferenceCount19_StaticFunctionTwoArguments_Dispose()
        {
            string code =
                @"   class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     static def ding(a:A, b:B[])    {         return = null;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};r = A.ding(as, bs);}aDispose = A.count;bDispose = B.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
        }

        [Test]
        public void TestReferenceCount20_MemberFunctionTwoArguments()
        {
            string code =
                @"   class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     def foo(b:B,c:C)     {        return = null;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};a = A.A();r = a.foo(bs, cs);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("c1", 0);
            thisTest.VerifyReferenceCount("c2", 0);
            thisTest.VerifyReferenceCount("c3", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("cs", 0);
            thisTest.VerifyReferenceCount("a", 0);
        }

        [Test]
        public void TestReferenceCount20_MemberFunctionTwoArguments_Dispose()
        {
            string code =
                @"   class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     def foo(b:B,c:C)     {        return = null;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};a = A.A();r = a.foo(bs, cs);}aDispose = A.count;bDispose = B.count;cDispose = C.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }

        [Test]
        public void TestReferenceCount21_MemberFunctionTwoArguments()
        {
            string code =
                @"   class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     def foo(b:B,c:C)     {        return = null;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};a = A.A();r = a.foo(bs, c1);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("c1", 0);
            thisTest.VerifyReferenceCount("c2", 0);
            thisTest.VerifyReferenceCount("c3", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("cs", 0);
            thisTest.VerifyReferenceCount("a", 0);
        }

        [Test]
        public void TestReferenceCount21_MemberFunctionTwoArguments_Dispose()
        {
            string code =
                @"   class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     def foo(b:B,c:C)     {        return = null;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};a = A.A();r = a.foo(bs, c1);}aDispose = A.count;bDispose = B.count;cDispose = C.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }

        [Test]
        public void TestReferenceCount22_MemberFunctionTwoArguments()
        {
            string code =
                @"   class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     def foo(b:B,c:C[])     {        return = null;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};a = A.A();r = a.foo(bs, cs);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("c1", 0);
            thisTest.VerifyReferenceCount("c2", 0);
            thisTest.VerifyReferenceCount("c3", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("cs", 0);
            thisTest.VerifyReferenceCount("a", 0);
        }

        [Test]
        public void TestReferenceCount22_MemberFunctionTwoArguments_Dispose()
        {
            string code =
                @"   class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     def foo(b:B,c:C[])     {        return = null;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};a = A.A();r = a.foo(bs, cs);}aDispose = A.count;bDispose = B.count;cDispose = C.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }

        [Test]
        public void TestReferenceCount23_MemberFunctionTwoArguments()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     def foo(b:B,c:C)     {        return = null;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};r = as.foo(bs, cs);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("c1", 0);
            thisTest.VerifyReferenceCount("c2", 0);
            thisTest.VerifyReferenceCount("c3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("cs", 0);
        }

        [Test]
        public void TestReferenceCount23_MemberFunctionTwoArguments_Dispose()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     def foo(b:B,c:C)     {        return = null;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};r = as.foo(bs, cs);}aDispose = A.count;bDispose = B.count;cDispose = C.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }

        [Test]
        public void TestReferenceCount24_MemberFunctionTwoArguments()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }      def foo(b:B,c:C)     {        return = null;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};r = as.foo(bs, c1);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("c1", 0);
            thisTest.VerifyReferenceCount("c2", 0);
            thisTest.VerifyReferenceCount("c3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("cs", 0);
        }

        [Test]
        public void TestReferenceCount24_MemberFunctionTwoArguments_Dispose()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }      def foo(b:B,c:C)     {        return = null;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};r = as.foo(bs, c1);}aDispose = A.count;bDispose = B.count;cDispose = C.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }

        [Test]
        public void TestReferenceCount25_MemberFunctionTwoArguments()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     def foo(b:B,c:C[])     {        return = null;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};r = as.foo(bs, cs);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("c1", 0);
            thisTest.VerifyReferenceCount("c2", 0);
            thisTest.VerifyReferenceCount("c3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("cs", 0);
        }

        [Test]
        public void TestReferenceCount25_MemberFunctionTwoArguments_Dispose()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     def foo(b:B,c:C[])     {        return = null;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};r = as.foo(bs, cs);}aDispose = A.count;bDispose = B.count;cDispose = C.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }

        [Test]
        public void TestReferenceCount26_GlobalFunctionReturnArray()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    } }class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}def foo : B[] (b : B[]){    return = b;}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = foo(bs);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("c1", 0);
            thisTest.VerifyReferenceCount("c2", 0);
            thisTest.VerifyReferenceCount("c3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("cs", 0);
            thisTest.VerifyReferenceCount("x", 0);
        }

        [Test]
        public void TestReferenceCount26_GlobalFunctionReturnArray_Dispose()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    } }class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}def foo : B[] (b : B[]){    return = b;}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = foo(bs);}aDispose = A.count;bDispose = B.count;cDispose = C.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }

        [Test]
        public void TestReferenceCount27_GlobalFunctionReturnArray()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    } }class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}def foo : B[] (b : B){    return = b;}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = foo(bs);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("c1", 0);
            thisTest.VerifyReferenceCount("c2", 0);
            thisTest.VerifyReferenceCount("c3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("cs", 0);
            thisTest.VerifyReferenceCount("x", 0);
        }

        [Test]
        public void TestReferenceCount27_GlobalFunctionReturnArray_Dispose()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    } }class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}def foo : B[] (b : B){    return = b;}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = foo(bs);}aDispose = A.count;bDispose = B.count;cDispose = C.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }

        [Test]
        public void TestReferenceCount28_GlobalFunctionReturnArray()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    } }class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}def foo : B (b : B){    return = b;}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = foo(bs);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("c1", 0);
            thisTest.VerifyReferenceCount("c2", 0);
            thisTest.VerifyReferenceCount("c3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("cs", 0);
            thisTest.VerifyReferenceCount("x", 0);
        }

        [Test]
        public void TestReferenceCount28_GlobalFunctionReturnArray_Dispose()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    } }class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}def foo : B (b : B){    return = b;}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = foo(bs);}aDispose = A.count;bDispose = B.count;cDispose = C.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }

        [Test]
        public void TestReferenceCount29_MemberFunctionReturnArray()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     def foo : B[] (b : B[])    {        return = b;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = a1.foo(bs);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("c1", 0);
            thisTest.VerifyReferenceCount("c2", 0);
            thisTest.VerifyReferenceCount("c3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("cs", 0);
            thisTest.VerifyReferenceCount("x", 0);
        }

        [Test]
        public void TestReferenceCount29_MemberFunctionReturnArray_Dispose()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     def foo : B[] (b : B[])    {        return = b;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = a1.foo(bs);}aDispose = A.count;bDispose = B.count;cDispose = C.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }

        [Test]
        public void TestReferenceCount30_MemberFunctionReturnArray()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     def foo : B[] (b : B)    {        return = b;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = a1.foo(bs);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("c1", 0);
            thisTest.VerifyReferenceCount("c2", 0);
            thisTest.VerifyReferenceCount("c3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("cs", 0);
            thisTest.VerifyReferenceCount("x", 0);
        }

        [Test]
        public void TestReferenceCount30_MemberFunctionReturnArray_Dispose()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     def foo : B[] (b : B)    {        return = b;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = a1.foo(bs);}aDispose = A.count;bDispose = B.count;cDispose = C.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }

        [Test]
        public void TestReferenceCount31_MemberFunctionReturnArray()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     def foo : B (b : B)    {        return = b;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = a1.foo(bs);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("c1", 0);
            thisTest.VerifyReferenceCount("c2", 0);
            thisTest.VerifyReferenceCount("c3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("cs", 0);
            thisTest.VerifyReferenceCount("x", 0);
        }

        [Test]
        public void TestReferenceCount31_MemberFunctionReturnArray_Dispose()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     def foo : B (b : B)    {        return = b;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = a1.foo(bs);}aDispose = A.count;bDispose = B.count;cDispose = C.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }

        [Test]
        public void TestReferenceCount32_StaticFunctionReturnArray()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     static def foo : B[] (b : B[])    {        return = b;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = A.foo(bs);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("c1", 0);
            thisTest.VerifyReferenceCount("c2", 0);
            thisTest.VerifyReferenceCount("c3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("cs", 0);
            thisTest.VerifyReferenceCount("x", 0);
        }

        [Test]
        public void TestReferenceCount32_StaticFunctionReturnArray_Dispose()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     static def foo : B[] (b : B[])    {        return = b;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = A.foo(bs);}aDispose = A.count;bDispose = B.count;cDispose = C.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }

        [Test]
        public void TestReferenceCount33_StaticFunctionReturnArray()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     static def foo : B[] (b : B)    {        return = b;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = A.foo(bs);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("c1", 0);
            thisTest.VerifyReferenceCount("c2", 0);
            thisTest.VerifyReferenceCount("c3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("cs", 0);
            thisTest.VerifyReferenceCount("x", 0);
        }

        [Test]
        public void TestReferenceCount33_StaticFunctionReturnArray_Dispose()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     static def foo : B[] (b : B)    {        return = b;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = A.foo(bs);}aDispose = A.count;bDispose = B.count;cDispose = C.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }

        [Test]
        public void TestReferenceCount34_StaticFunctionReturnArray()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     static def foo : B (b : B)    {        return = b;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = A.foo(bs);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("c1", 0);
            thisTest.VerifyReferenceCount("c2", 0);
            thisTest.VerifyReferenceCount("c3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("cs", 0);
            thisTest.VerifyReferenceCount("x", 0);
        }

        [Test]
        public void TestReferenceCount34_StaticFunctionReturnArray_Dispose()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     static def foo : B (b : B)    {        return = b;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = A.foo(bs);}aDispose = A.count;bDispose = B.count;cDispose = C.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }

        [Test]
        public void TestReferenceCount35_StaticFunctionReturnObject()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     static def foo : B (b : B[])    {        return = b[0];    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = A.foo(bs);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("c1", 0);
            thisTest.VerifyReferenceCount("c2", 0);
            thisTest.VerifyReferenceCount("c3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("cs", 0);
            thisTest.VerifyReferenceCount("x", 0);
        }

        [Test]
        public void TestReferenceCount35_StaticFunctionReturnObject_Dispose()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     static def foo : B (b : B[])    {        return = b[0];    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = A.foo(bs);}aDispose = A.count;bDispose = B.count;cDispose = C.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }

        [Test]
        public void TestReferenceCount36_StaticFunctionReturnObject()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }      static def foo : B (b : B)    {        return = b;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = A.foo(b1);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("c1", 0);
            thisTest.VerifyReferenceCount("c2", 0);
            thisTest.VerifyReferenceCount("c3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("cs", 0);
            thisTest.VerifyReferenceCount("x", 0);
        }

        [Test]
        public void TestReferenceCount36_StaticFunctionReturnObject_Dispose()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }      static def foo : B (b : B)    {        return = b;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = A.foo(b1);}aDispose = A.count;bDispose = B.count;cDispose = C.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }

        [Test]
        public void TestReferenceCount37_MemberFunctionReturnObject()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }       def foo : B (b : B[])    {        return = b[0];    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = a1.foo(bs);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("c1", 0);
            thisTest.VerifyReferenceCount("c2", 0);
            thisTest.VerifyReferenceCount("c3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("cs", 0);
            thisTest.VerifyReferenceCount("x", 0);
        }

        [Test]
        public void TestReferenceCount37_MemberFunctionReturnObject_Dispose()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }       def foo : B (b : B[])    {        return = b[0];    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = a1.foo(bs);}aDispose = A.count;bDispose = B.count;cDispose = C.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }

        [Test]
        public void TestReferenceCount38_MemberFunctionReturnObject()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     def foo : B (b : B)    {        return = b;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = a1.foo(b1);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("c1", 0);
            thisTest.VerifyReferenceCount("c2", 0);
            thisTest.VerifyReferenceCount("c3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("cs", 0);
            thisTest.VerifyReferenceCount("x", 0);
        }

        [Test]
        public void TestReferenceCount38_MemberFunctionReturnObject_Dispose()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     def foo : B (b : B)    {        return = b;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = a1.foo(b1);}aDispose = A.count;bDispose = B.count;cDispose = C.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }

        [Test]
        public void TestReferenceCount39_GlobalFunctionReturnObject()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    } }class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}def foo : B (b : B[]){    return = b[0];}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = foo(bs);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("c1", 0);
            thisTest.VerifyReferenceCount("c2", 0);
            thisTest.VerifyReferenceCount("c3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("cs", 0);
            thisTest.VerifyReferenceCount("x", 0);
        }

        [Test]
        public void TestReferenceCount39_GlobalFunctionReturnObject_Dispose()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    } }class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}def foo : B (b : B[]){    return = b[0];}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = foo(bs);}aDispose = A.count;bDispose = B.count;cDispose = C.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }

        [Test]
        public void TestReferenceCount40_GlobalFunctionReturnObject()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    } }class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}def foo : B (b : B){    return = b;}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = foo(b1);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("c1", 0);
            thisTest.VerifyReferenceCount("c2", 0);
            thisTest.VerifyReferenceCount("c3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("cs", 0);
            thisTest.VerifyReferenceCount("x", 0);
        }

        [Test]
        public void TestReferenceCount40_GlobalFunctionReturnObject_Dispose()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    } }class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}def foo : B (b : B){    return = b;}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = foo(b1);}aDispose = A.count;bDispose = B.count;cDispose = C.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }

        [Test]
        public void TestReferenceCount41_MemberFunctionReturnArray()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     def foo : B[] (b : B[])    {        return = b;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = as.foo(bs);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("c1", 0);
            thisTest.VerifyReferenceCount("c2", 0);
            thisTest.VerifyReferenceCount("c3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("cs", 0);
            thisTest.VerifyReferenceCount("x", 0);
        }

        [Test]
        public void TestReferenceCount41_MemberFunctionReturnArray_Dispose()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     def foo : B[] (b : B[])    {        return = b;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = as.foo(bs);}aDispose = A.count;bDispose = B.count;cDispose = C.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }

        [Test]
        public void TestReferenceCount42_MemberFunctionReturnArray()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     def foo : B[] (b : B)    {        return = b;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = as.foo(bs);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("c1", 0);
            thisTest.VerifyReferenceCount("c2", 0);
            thisTest.VerifyReferenceCount("c3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("cs", 0);
            thisTest.VerifyReferenceCount("x", 0);
        }

        [Test]
        public void TestReferenceCount42_MemberFunctionReturnArray_Dispose()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     def foo : B[] (b : B)    {        return = b;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = as.foo(bs);}aDispose = A.count;bDispose = B.count;cDispose = C.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }

        [Test]
        public void TestReferenceCount43_MemberFunctionReturnArray()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     def foo : B (b : B)    {        return = b;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = as.foo(bs);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("c1", 0);
            thisTest.VerifyReferenceCount("c2", 0);
            thisTest.VerifyReferenceCount("c3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("cs", 0);
            thisTest.VerifyReferenceCount("x", 0);
        }

        [Test]
        public void TestReferenceCount43_MemberFunctionReturnArray_Dispose()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     def foo : B (b : B)    {        return = b;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = as.foo(bs);}aDispose = A.count;bDispose = B.count;cDispose = C.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }

        [Test]
        public void TestReferenceCount44_MemberFunctionReturnObject()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }       def foo : B (b : B[])    {        return = b[0];    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = as.foo(bs);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("c1", 0);
            thisTest.VerifyReferenceCount("c2", 0);
            thisTest.VerifyReferenceCount("c3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("cs", 0);
            thisTest.VerifyReferenceCount("x", 0);
        }

        [Test]
        public void TestReferenceCount44_MemberFunctionReturnObject_Dispose()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }       def foo : B (b : B[])    {        return = b[0];    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = as.foo(bs);}aDispose = A.count;bDispose = B.count;cDispose = C.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }

        [Test]
        public void TestReferenceCount45_MemberFunctionReturnObject()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     def foo : B (b : B)    {        return = b;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = as.foo(b1);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("c1", 0);
            thisTest.VerifyReferenceCount("c2", 0);
            thisTest.VerifyReferenceCount("c3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("cs", 0);
            thisTest.VerifyReferenceCount("x", 0);
        }

        [Test]
        public void TestReferenceCount45_MemberFunctionReturnObject_Dispose()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     def foo : B (b : B)    {        return = b;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = as.foo(b1);}aDispose = A.count;bDispose = B.count;cDispose = C.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }

        [Test]
        public void TestReferenceCount46_GlobalFunctionReturnNewArray()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    } }class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}def foo : A[] (b : B[]){    a = {A.A(),A.A(),A.A()};    return = a;}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = foo(bs);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("c1", 0);
            thisTest.VerifyReferenceCount("c2", 0);
            thisTest.VerifyReferenceCount("c3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("cs", 0);
            thisTest.VerifyReferenceCount("x", 0);
        }

        [Test]
        public void TestReferenceCount46_GlobalFunctionReturnNewArray_Dispose()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    } }class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}def foo : A[] (b : B[]){    a = {A.A(),A.A(),A.A()};    return = a;}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = foo(bs);}aDispose = A.count;bDispose = B.count;cDispose = C.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }

        [Test]
        public void TestReferenceCount47_GlobalFunctionReturnNewArray()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    } }class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}def foo : A[] (b : B){    a = A.A();    return = a;}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = foo(bs);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("c1", 0);
            thisTest.VerifyReferenceCount("c2", 0);
            thisTest.VerifyReferenceCount("c3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("cs", 0);
            thisTest.VerifyReferenceCount("x", 0);
        }

        [Test]
        public void TestReferenceCount47_GlobalFunctionReturnNewArray_Dispose()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    } }class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}def foo : A[] (b : B){    a = A.A();    return = a;}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = foo(bs);}aDispose = A.count;bDispose = B.count;cDispose = C.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }

        [Test]
        public void TestReferenceCount48_GlobalFunctionReturnNewArray()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    } }class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}def foo : A (b : B){    a = A.A();    return = a;}def foo : B (b : B){    return = b;}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = foo(bs);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("c1", 0);
            thisTest.VerifyReferenceCount("c2", 0);
            thisTest.VerifyReferenceCount("c3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("cs", 0);
            thisTest.VerifyReferenceCount("x", 0);
        }

        [Test]
        public void TestReferenceCount48_GlobalFunctionReturnNewArray_Dispose()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    } }class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}def foo : A (b : B){    a = A.A();    return = a;}def foo : B (b : B){    return = b;}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = foo(bs);}aDispose = A.count;bDispose = B.count;cDispose = C.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }

        [Test]
        public void TestReferenceCount49_MemberFunctionReturnNewArray()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     def foo : A[] (b : B[])    {        a = {A.A(),A.A(),A.A()};        return = a;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = a1.foo(bs);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("c1", 0);
            thisTest.VerifyReferenceCount("c2", 0);
            thisTest.VerifyReferenceCount("c3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("cs", 0);
            thisTest.VerifyReferenceCount("x", 0);
        }

        [Test]
        public void TestReferenceCount49_MemberFunctionReturnNewArray_Dispose()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     def foo : A[] (b : B[])    {        a = {A.A(),A.A(),A.A()};        return = a;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = a1.foo(bs);}aDispose = A.count;bDispose = B.count;cDispose = C.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }

        [Test]
        public void TestReferenceCount50_MemberFunctionReturnNewArray()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     def foo : A[] (b : B)    {        a = A.A();        return = a;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = a1.foo(bs);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("c1", 0);
            thisTest.VerifyReferenceCount("c2", 0);
            thisTest.VerifyReferenceCount("c3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("cs", 0);
            thisTest.VerifyReferenceCount("x", 0);
        }

        [Test]
        public void TestReferenceCount50_MemberFunctionReturnNewArray_Dispose()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     def foo : A[] (b : B)    {        a = A.A();        return = a;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = a1.foo(bs);}aDispose = A.count;bDispose = B.count;cDispose = C.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }

        [Test]
        public void TestReferenceCount51_MemberFunctionReturnNewArray()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     def foo : A (b : B)    {        a = A.A();        return = a;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = a1.foo(bs);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("c1", 0);
            thisTest.VerifyReferenceCount("c2", 0);
            thisTest.VerifyReferenceCount("c3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("cs", 0);
            thisTest.VerifyReferenceCount("x", 0);
        }

        [Test]
        public void TestReferenceCount51_MemberFunctionReturnNewArray_Dispose()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     def foo : A (b : B)    {        a = A.A();        return = a;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = a1.foo(bs);}aDispose = A.count;bDispose = B.count;cDispose = C.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }

        [Test]
        public void TestReferenceCount52_StaticFunctionReturnNewArray()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     static def foo : A[] (b : B[])    {        a = {A.A(),A.A(),A.A()};        return = a;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = A.foo(bs);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("c1", 0);
            thisTest.VerifyReferenceCount("c2", 0);
            thisTest.VerifyReferenceCount("c3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("cs", 0);
            thisTest.VerifyReferenceCount("x", 0);
        }

        [Test]
        public void TestReferenceCount52_StaticFunctionReturnNewArray_Dispose()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     static def foo : A[] (b : B[])    {        a = {A.A(),A.A(),A.A()};        return = a;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = A.foo(bs);}aDispose = A.count;bDispose = B.count;cDispose = C.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }

        [Test]
        public void TestReferenceCount53_StaticFunctionReturnNewArray()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     static def foo : A[] (b : B)    {        a = A.A();        return = a;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = A.foo(bs);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("c1", 0);
            thisTest.VerifyReferenceCount("c2", 0);
            thisTest.VerifyReferenceCount("c3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("cs", 0);
            thisTest.VerifyReferenceCount("x", 0);
        }

        [Test]
        public void TestReferenceCount53_StaticFunctionReturnNewArray_Dispose()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     static def foo : A[] (b : B)    {        a = A.A();        return = a;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = A.foo(bs);}aDispose = A.count;bDispose = B.count;cDispose = C.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }

        [Test]
        public void TestReferenceCount54_StaticFunctionReturnNewArray()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     static def foo : A (b : B)    {        a = A.A();        return = a;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = A.foo(bs);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("c1", 0);
            thisTest.VerifyReferenceCount("c2", 0);
            thisTest.VerifyReferenceCount("c3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("cs", 0);
            thisTest.VerifyReferenceCount("x", 0);
        }

        [Test]
        public void TestReferenceCount54_StaticFunctionReturnNewArray_Dispose()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     static def foo : A (b : B)    {        a = A.A();        return = a;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = A.foo(bs);}aDispose = A.count;bDispose = B.count;cDispose = C.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }

        [Test]
        public void TestReferenceCount55_StaticFunctionReturnNewObject()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     static def foo : A (b : B[])    {        a = {A.A(),A.A(),A.A()};        return = a[0];    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = A.foo(bs);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("c1", 0);
            thisTest.VerifyReferenceCount("c2", 0);
            thisTest.VerifyReferenceCount("c3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("cs", 0);
            thisTest.VerifyReferenceCount("x", 0);
        }

        [Test]
        public void TestReferenceCount55_StaticFunctionReturnNewObject_Dispose()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     static def foo : A (b : B[])    {        a = {A.A(),A.A(),A.A()};        return = a[0];    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = A.foo(bs);}aDispose = A.count;bDispose = B.count;cDispose = C.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }

        [Test]
        public void TestReferenceCount56_StaticFunctionReturnNewObject()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     static def foo : A (b : B)    {        a = A.A();        return = a;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = A.foo(b1);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("c1", 0);
            thisTest.VerifyReferenceCount("c2", 0);
            thisTest.VerifyReferenceCount("c3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("cs", 0);
            thisTest.VerifyReferenceCount("x", 0);
        }

        [Test]
        public void TestReferenceCount56_StaticFunctionReturnNewObject_Dispose()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     static def foo : A (b : B)    {        a = A.A();        return = a;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = A.foo(b1);}aDispose = A.count;bDispose = B.count;cDispose = C.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }

        [Test]
        public void TestReferenceCount57_MemberFunctionReturnNewObject()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     def foo : A (b : B[])    {        a = {A.A(),A.A(),A.A()};        return = a[0];    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = a1.foo(bs);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("c1", 0);
            thisTest.VerifyReferenceCount("c2", 0);
            thisTest.VerifyReferenceCount("c3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("cs", 0);
            thisTest.VerifyReferenceCount("x", 0);
        }

        [Test]
        public void TestReferenceCount57_MemberFunctionReturnNewObject_Dispose()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     def foo : A (b : B[])    {        a = {A.A(),A.A(),A.A()};        return = a[0];    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = a1.foo(bs);}aDispose = A.count;bDispose = B.count;cDispose = C.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }

        [Test]
        public void TestReferenceCount58_MemberFunctionReturnNewObject()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     def foo : A (b : B)    {        a = A.A();        return = a;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = a1.foo(b1);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("c1", 0);
            thisTest.VerifyReferenceCount("c2", 0);
            thisTest.VerifyReferenceCount("c3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("cs", 0);
            thisTest.VerifyReferenceCount("x", 0);
        }

        [Test]
        public void TestReferenceCount58_MemberFunctionReturnNewObject_Dispose()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     def foo : A (b : B)    {        a = A.A();        return = a;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = a1.foo(b1);}aDispose = A.count;bDispose = B.count;cDispose = C.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }

        [Test]
        public void TestReferenceCount59_GlobalFunctionReturnNewObject()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    } }class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}def foo : A (b : B[]){    a = {A.A(),A.A(),A.A()};    return = a[0];}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = foo(bs);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("c1", 0);
            thisTest.VerifyReferenceCount("c2", 0);
            thisTest.VerifyReferenceCount("c3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("cs", 0);
            thisTest.VerifyReferenceCount("x", 0);
        }

        [Test]
        public void TestReferenceCount59_GlobalFunctionReturnNewObject_Dispose()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    } }class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}def foo : A (b : B[]){    a = {A.A(),A.A(),A.A()};    return = a[0];}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = foo(bs);}aDispose = A.count;bDispose = B.count;cDispose = C.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }

        [Test]
        public void TestReferenceCount60_GlobalFunctionReturnNewObject()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    } }class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}def foo : A (b : B){    a = A.A();    return = a;}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = foo(b1);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("c1", 0);
            thisTest.VerifyReferenceCount("c2", 0);
            thisTest.VerifyReferenceCount("c3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("cs", 0);
            thisTest.VerifyReferenceCount("x", 0);
        }

        [Test]
        public void TestReferenceCount60_GlobalFunctionReturnNewObject_Dispose()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    } }class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}def foo : A (b : B){    a = A.A();    return = a;}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = foo(b1);}aDispose = A.count;bDispose = B.count;cDispose = C.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }

        [Test]
        public void TestReferenceCount61_MemberFunctionReturnNewArray()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     def foo : A[] (b : B[])    {        a = {A.A(),A.A(),A.A()};        return = a;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = as.foo(bs);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("c1", 0);
            thisTest.VerifyReferenceCount("c2", 0);
            thisTest.VerifyReferenceCount("c3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("cs", 0);
            thisTest.VerifyReferenceCount("x", 0);
        }

        [Test]
        public void TestReferenceCount61_MemberFunctionReturnNewArray_Dispose()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     def foo : A[] (b : B[])    {        a = {A.A(),A.A(),A.A()};        return = a;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = as.foo(bs);}aDispose = A.count;bDispose = B.count;cDispose = C.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }

        [Test]
        public void TestReferenceCount62_MemberFunctionReturnNewArray()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     def foo : A[] (b : B)    {        a = A.A();        return = a;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = as.foo(bs);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("c1", 0);
            thisTest.VerifyReferenceCount("c2", 0);
            thisTest.VerifyReferenceCount("c3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("cs", 0);
            thisTest.VerifyReferenceCount("x", 0);
        }

        [Test]
        public void TestReferenceCount62_MemberFunctionReturnNewArray_Dispose()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     def foo : A[] (b : B)    {        a = A.A();        return = a;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = as.foo(bs);}aDispose = A.count;bDispose = B.count;cDispose = C.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }

        [Test]
        public void TestReferenceCount63_MemberFunctionReturnNewArray()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     def foo : A (b : B)    {        a = A.A();        return = a;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = as.foo(bs);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("c1", 0);
            thisTest.VerifyReferenceCount("c2", 0);
            thisTest.VerifyReferenceCount("c3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("cs", 0);
            thisTest.VerifyReferenceCount("x", 0);
        }

        [Test]
        public void TestReferenceCount63_MemberFunctionReturnNewArray_Dispose()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     def foo : A (b : B)    {        a = A.A();        return = a;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = as.foo(bs);}aDispose = A.count;bDispose = B.count;cDispose = C.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }

        [Test]
        public void TestReferenceCount64_MemberFunctionReturnNewObject()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     def foo : A (b : B[])    {        a = {A.A(),A.A(),A.A()};        return = a[0];    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = as.foo(bs);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("c1", 0);
            thisTest.VerifyReferenceCount("c2", 0);
            thisTest.VerifyReferenceCount("c3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("cs", 0);
            thisTest.VerifyReferenceCount("x", 0);
        }

        [Test]
        public void TestReferenceCount64_MemberFunctionReturnNewObject_Dispose()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     def foo : A (b : B[])    {        a = {A.A(),A.A(),A.A()};        return = a[0];    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = as.foo(bs);}aDispose = A.count;bDispose = B.count;cDispose = C.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }

        [Test]
        public void TestReferenceCount65_MemberFunctionReturnNewObject()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     def foo : A (b : B)    {        a = A.A();        return = a;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = as.foo(b1);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyReferenceCount("a1", 0);
            thisTest.VerifyReferenceCount("a2", 0);
            thisTest.VerifyReferenceCount("a3", 0);
            thisTest.VerifyReferenceCount("b1", 0);
            thisTest.VerifyReferenceCount("b2", 0);
            thisTest.VerifyReferenceCount("b3", 0);
            thisTest.VerifyReferenceCount("c1", 0);
            thisTest.VerifyReferenceCount("c2", 0);
            thisTest.VerifyReferenceCount("c3", 0);
            thisTest.VerifyReferenceCount("as", 0);
            thisTest.VerifyReferenceCount("bs", 0);
            thisTest.VerifyReferenceCount("cs", 0);
            thisTest.VerifyReferenceCount("x", 0);
        }

        [Test]
        public void TestReferenceCount65_MemberFunctionReturnNewObject_Dispose()
        {
            string code =
                @"  class A{    static count : var = 0;    constructor A()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }     def foo : A (b : B)    {        a = A.A();        return = a;    }}class B{    static count : var = 0;    constructor B()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}class C{  static count : var = 0;    constructor C()    {        count = count + 1;    }    def _Dispose : int()    {        count = count - 1;        return = null;    }}[Associative]{a1 = A.A();a2 = A.A();a3 = A.A();as = {a1, a2, a3};b1 = B.B();b2 = B.B();b3 = B.B();bs = {b1, b2, b3};c1 = C.C();c2 = C.C();c3 = C.C();cs = {c1, c2, c3};x = as.foo(b1);}aDispose = A.count;bDispose = B.count;cDispose = C.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }

        [Test]
        public void TestReferenceCount66_DID1467277()
        {
            string code = @"class A{    x;    static s_dispose = 0;    constructor A(i)    {        x = i;    }    def _Dispose()    {        s_dispose = s_dispose + 1;        return = null;    }    def foo()    {        return = null;    }}class B{    def CreateA(i)    {        return = A.A(i);    }}b = B.B();r = b.CreateA(0..1).foo();t = A.s_dispose;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            // SSA'd transforms will not GC the temps until end of block
            // However, they must be GC's after every line when in debug step over
            thisTest.Verify("t", 0);
        }

        [Test]
        public void TestReferenceCount67_DID1467277_02()
        {
            string code = @"class A{    x;    static s_dispose = 0;    constructor A(i)    {        x = i;    }    def _Dispose()    {        s_dispose = s_dispose + 1;        return = null;    }    def foo()    {        return = null;    }}r = A.A(0..1).foo();t = A.s_dispose;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            // SSA'd transforms will not GC the temps until end of block
            // However, they must be GC's after every line when in debug setp over
            thisTest.Verify("t", 0);
        }

        [Test]
        public void TestReferenceCount68_TemporaryArrayIndexing01()
        {
            string code = @"class A{    static s_dispose = 0;    constructor A()    {    }        def _Dispose()    {        s_dispose = s_dispose + 1;    }}[Associative]{    a = {A.A(), A.A(), A.A()}[1];}d = A.s_dispose;";
            string errorString = "";
            thisTest.RunScriptSource(code, errorString);
            thisTest.Verify("d", 3);
        }

        [Test]
        public void TestReferenceCount69_TemporaryArrayIndexing02()
        {
            string code = @"class A{    static s_dispose = 0;    constructor A()    {    }        def _Dispose()    {        s_dispose = s_dispose + 1;    }}t = [Associative]{    a = {A.A(), A.A(), A.A()}[1];    return = a;}d = A.s_dispose;";
            string errorString = "";
            thisTest.RunScriptSource(code, errorString);
            thisTest.Verify("d", 2);
        }

        [Test]
        public void TestReferenceCount70_TemporaryArrayIndexing03()
        {
            string code = @"class A{    static s_dispose = 0;    constructor A()    {    }        def _Dispose()    {        s_dispose = s_dispose + 1;    }}t = [Associative]{    def foo()    {        return = {A.A(), A.A(), A.A()};    }    a = (foo())[1];    return = a;}d = A.s_dispose;";
            string errorString = "";
            thisTest.RunScriptSource(code, errorString);
            thisTest.Verify("d", 2);
        }

        [Test]
        public void TestReferenceCount71_TemporaryArrayIndexing04()
        {
            string code = @"class A{    static s_dispose = 0;    constructor A(i:int)    {    }        def _Dispose()    {        s_dispose = s_dispose + 1;    }}t = [Associative]{    a = (A.A(0..4))[1];    return = a;}d = A.s_dispose;";
            string errorString = "";
            thisTest.RunScriptSource(code, errorString);
            thisTest.Verify("d", 4);
        }

        [Test]
        public void TestReferenceCount72_TemporaryDefaultArgument()
        {
            string code = @"class A{    static s_dispose = 0;    constructor A(i:int)    {    }        def _Dispose()    {        s_dispose = s_dispose + 1;    }}def foo(a = A.A()){}t = foo();d = A.s_dispose;";
            string errorString = "";
            thisTest.RunScriptSource(code, errorString);
            thisTest.Verify("d", 1);
        }

        [Test]
        public void TestReferenceCount73_FunctionPointer()
        {
            string code = @"class A{    static s_dispose = 0;    constructor A(i:int)    {    }        def _Dispose()    {        s_dispose = s_dispose + 1;    }}def foo(a = A.A()){    return = null;}t = foo; def bar(f:function){    return = f();}r = bar(t);d = A.s_dispose;";
            string errorString = "";
            thisTest.RunScriptSource(code, errorString);
            thisTest.Verify("d", 1);
        }

        [Test]
        public void T074_DG1465049()
        {
            string code = @"class A{    static s_dispose = 0;    mi : int;    constructor A(i:int)    {        mi = i;    }        def _Dispose()    {        s_dispose = s_dispose + 1;    }    def Translate(i)    {        newi = mi + i;        return = A.A(newi);    }}as = {A.A(2), A.A(3), A.A(5)};as[1] = as[1].Translate(100);as = null;d = A.s_dispose;";
            thisTest.RunScriptSource(code);

            // IT gc's the line where it calls translate when variable as is nullified
            // It disposes 3 ssa temporaries and 1 element in the array 'as'
            thisTest.Verify("d", 4);
        }

        [Test]
        public void TestReferenceCountForMembers()
        {
            string code = @"a_dispose = 0;class A{    def _Dispose()    {        a_dispose = a_dispose + 1;    }}class B{    as : A[]..[];    constructor B()    {        [Imperative]        {            as = {A.A()};        }    }}[Associative]{    b = B.B();}";
            thisTest.RunScriptSource(code, "");
            thisTest.Verify("a_dispose", 1);
        }

        [Test]
        public void TestReferenceCountForStaticMembers()
        {
            string code = @"a_dispose = 0;class A{    def _Dispose()    {        a_dispose = a_dispose + 1;    }}class B{    static sas : A[]..[];    constructor B()    {        [Imperative]        {            sas[0] = {A.A()};        }    }}[Associative]{    b = B.B();    b.sas = null;}";
            thisTest.RunScriptSource(code, "");
            thisTest.Verify("a_dispose", 1);
        }

        [Test]
        public void TestReferenceCountForStaticMembers2()
        {
            string code = @"a_dispose = 0;class A{    static x;    def _Dispose()    {        a_dispose = a_dispose + 1;    }}class B{    sas : A[]..[];    constructor B()    {        [Imperative]        {            sas = {A.A()};            sas = {A.A()};        }    }}[Associative]{    b = B.B();    b.sas = null;}";
            thisTest.RunScriptSource(code, "");
            thisTest.Verify("a_dispose", 2);
        }
    }
}
