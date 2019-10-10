using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
namespace ProtoTest.Associative
{
    class ReferenceCount : ProtoTestBase
    {
        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount01_NoFunctionCall_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();as = [a1, a2, a3];bs = [b1, b2, b3];a = DisposeTestClassA.DisposeTestClassA();b = DisposeTestClassB.DisposeTestClassB();}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
        }


        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount02_FunctionNonArray_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();as = [a1, a2, a3];bs = [b1, b2, b3];a = DisposeTestClassA.DisposeTestClassA();b = DisposeTestClassB.DisposeTestClassB();}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
        }


        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount03_FunctionReplication_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();as = [a1, a2, a3];bs = [b1, b2, b3];a = DisposeTestClassA.DisposeTestClassA();b = DisposeTestClassB.DisposeTestClassB();}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
        }


        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount04_FunctionArray_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();as = [a1, a2, a3];bs = [b1, b2, b3];a = DisposeTestClassA.DisposeTestClassA();b = DisposeTestClassB.DisposeTestClassB();r = bar(bs);}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
        }


        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount05_StaticFunctionNonArray_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();as = [a1, a2, a3];bs = [b1, b2, b3];a = DisposeTestClassA.DisposeTestClassA();b = DisposeTestClassB.DisposeTestClassB();}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
        }


        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount06_StaticFunctionReplication_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();as = [a1, a2, a3];bs = [b1, b2, b3];a = DisposeTestClassA.DisposeTestClassA();b = DisposeTestClassB.DisposeTestClassB();}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
        }


        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount07_StaticFunctionArray_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();as = [a1, a2, a3];bs = [b1, b2, b3];a = DisposeTestClassA.DisposeTestClassA();b = DisposeTestClassB.DisposeTestClassB();}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
        }



        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount08_MemFunctionNonArray_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();as = [a1, a2, a3];bs = [b1, b2, b3];a = DisposeTestClassA.DisposeTestClassA();b = DisposeTestClassB.DisposeTestClassB();}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
        }



        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount09_MemFunctionReplication_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();as = [a1, a2, a3];bs = [b1, b2, b3];a = DisposeTestClassA.DisposeTestClassA();b = DisposeTestClassB.DisposeTestClassB();}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
        }



        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount10_MemFunctionArray_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();as = [a1, a2, a3];bs = [b1, b2, b3];a = DisposeTestClassA.DisposeTestClassA();b = DisposeTestClassB.DisposeTestClassB();r = a.bar(bs);}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
        }


        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount11_ReplicationNonArray_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();as = [a1, a2, a3];bs = [b1, b2, b3];a = DisposeTestClassA.DisposeTestClassA();b = DisposeTestClassB.DisposeTestClassB();}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
        }


        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount12_ReplicationReplication_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();as = [a1, a2, a3];bs = [b1, b2, b3];a = DisposeTestClassA.DisposeTestClassA();b = DisposeTestClassB.DisposeTestClassB();}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
        }


        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount13_ReplicationArray_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();as = [a1, a2, a3];bs = [b1, b2, b3];a = DisposeTestClassA.DisposeTestClassA();b = DisposeTestClassB.DisposeTestClassB();r = as.bar(bs);}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
        }



        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount14_GlobalFunctionTwoArguments_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();as = [a1, a2, a3];b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();bs = [b1, b2, b3];}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
        }



        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount15_GlobalFunctionTwoArguments_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();as = [a1, a2, a3];b = DisposeTestClassB.DisposeTestClassB();}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
        }



        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount16_GlobalFunctionTwoArguments_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();as = [a1, a2, a3];b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();bs = [b1, b2, b3];}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
        }


        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount17_StaticFunctionTwoArguments_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();as = [a1, a2, a3];b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();bs = [b1, b2, b3];}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
        }



        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount18_StaticFunctionTwoArguments_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();as = [a1, a2, a3];b = DisposeTestClassB.DisposeTestClassB();}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
        }


        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount19_StaticFunctionTwoArguments_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();as = [a1, a2, a3];b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();bs = [b1, b2, b3];}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
        }


        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount20_MemberFunctionTwoArguments_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();DisposeTestClassC.Reset();[Associative]{b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();bs = [b1, b2, b3];c1 = DisposeTestClassC.DisposeTestClassC();c2 = DisposeTestClassC.DisposeTestClassC();c3 = DisposeTestClassC.DisposeTestClassC();cs = [c1, c2, c3];a = DisposeTestClassA.DisposeTestClassA();}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;cDispose = DisposeTestClassC.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }



        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount21_MemberFunctionTwoArguments_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();DisposeTestClassC.Reset();[Associative]{b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();bs = [b1, b2, b3];c1 = DisposeTestClassC.DisposeTestClassC();c2 = DisposeTestClassC.DisposeTestClassC();c3 = DisposeTestClassC.DisposeTestClassC();cs = [c1, c2, c3];a = DisposeTestClassA.DisposeTestClassA();}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;cDispose = DisposeTestClassC.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }


        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount22_MemberFunctionTwoArguments_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();DisposeTestClassC.Reset();[Associative]{b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();bs = [b1, b2, b3];c1 = DisposeTestClassC.DisposeTestClassC();c2 = DisposeTestClassC.DisposeTestClassC();c3 = DisposeTestClassC.DisposeTestClassC();cs = [c1, c2, c3];a = DisposeTestClassA.DisposeTestClassA();}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;cDispose = DisposeTestClassC.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }



        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount23_MemberFunctionTwoArguments_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();DisposeTestClassC.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();as = [a1, a2, a3];b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();bs = [b1, b2, b3];c1 = DisposeTestClassC.DisposeTestClassC();c2 = DisposeTestClassC.DisposeTestClassC();c3 = DisposeTestClassC.DisposeTestClassC();cs = [c1, c2, c3];}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;cDispose = DisposeTestClassC.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }



        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount24_MemberFunctionTwoArguments_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();DisposeTestClassC.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();as = [a1, a2, a3];b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();bs = [b1, b2, b3];c1 = DisposeTestClassC.DisposeTestClassC();c2 = DisposeTestClassC.DisposeTestClassC();c3 = DisposeTestClassC.DisposeTestClassC();cs = [c1, c2, c3];}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;cDispose = DisposeTestClassC.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }


        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount25_MemberFunctionTwoArguments_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();DisposeTestClassC.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();as = [a1, a2, a3];b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();bs = [b1, b2, b3];c1 = DisposeTestClassC.DisposeTestClassC();c2 = DisposeTestClassC.DisposeTestClassC();c3 = DisposeTestClassC.DisposeTestClassC();cs = [c1, c2, c3];}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;cDispose = DisposeTestClassC.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }



        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount26_GlobalFunctionReturnArray_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();DisposeTestClassC.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();as = [a1, a2, a3];b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();bs = [b1, b2, b3];c1 = DisposeTestClassC.DisposeTestClassC();c2 = DisposeTestClassC.DisposeTestClassC();c3 = DisposeTestClassC.DisposeTestClassC();cs = [c1, c2, c3];}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;cDispose = DisposeTestClassC.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }



        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount27_GlobalFunctionReturnArray_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();DisposeTestClassC.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();as = [a1, a2, a3];b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();bs = [b1, b2, b3];c1 = DisposeTestClassC.DisposeTestClassC();c2 = DisposeTestClassC.DisposeTestClassC();c3 = DisposeTestClassC.DisposeTestClassC();cs = [c1, c2, c3];}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;cDispose = DisposeTestClassC.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }



        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount28_GlobalFunctionReturnArray_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();DisposeTestClassC.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();as = [a1, a2, a3];b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();bs = [b1, b2, b3];c1 = DisposeTestClassC.DisposeTestClassC();c2 = DisposeTestClassC.DisposeTestClassC();c3 = DisposeTestClassC.DisposeTestClassC();cs = [c1, c2, c3];}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;cDispose = DisposeTestClassC.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }



        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount29_MemberFunctionReturnArray_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();DisposeTestClassC.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();as = [a1, a2, a3];b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();bs = [b1, b2, b3];c1 = DisposeTestClassC.DisposeTestClassC();c2 = DisposeTestClassC.DisposeTestClassC();c3 = DisposeTestClassC.DisposeTestClassC();cs = [c1, c2, c3];}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;cDispose = DisposeTestClassC.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }



        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount30_MemberFunctionReturnArray_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();DisposeTestClassC.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();as = [a1, a2, a3];b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();bs = [b1, b2, b3];c1 = DisposeTestClassC.DisposeTestClassC();c2 = DisposeTestClassC.DisposeTestClassC();c3 = DisposeTestClassC.DisposeTestClassC();cs = [c1, c2, c3];}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;cDispose = DisposeTestClassC.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }



        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount31_MemberFunctionReturnArray_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();DisposeTestClassC.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();as = [a1, a2, a3];b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();bs = [b1, b2, b3];c1 = DisposeTestClassC.DisposeTestClassC();c2 = DisposeTestClassC.DisposeTestClassC();c3 = DisposeTestClassC.DisposeTestClassC();cs = [c1, c2, c3];}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;cDispose = DisposeTestClassC.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }



        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount32_StaticFunctionReturnArray_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();DisposeTestClassC.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();as = [a1, a2, a3];b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();bs = [b1, b2, b3];c1 = DisposeTestClassC.DisposeTestClassC();c2 = DisposeTestClassC.DisposeTestClassC();c3 = DisposeTestClassC.DisposeTestClassC();cs = [c1, c2, c3];}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;cDispose = DisposeTestClassC.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }



        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount33_StaticFunctionReturnArray_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();DisposeTestClassC.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();as = [a1, a2, a3];b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();bs = [b1, b2, b3];c1 = DisposeTestClassC.DisposeTestClassC();c2 = DisposeTestClassC.DisposeTestClassC();c3 = DisposeTestClassC.DisposeTestClassC();cs = [c1, c2, c3];}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;cDispose = DisposeTestClassC.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }



        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount34_StaticFunctionReturnArray_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();DisposeTestClassC.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();as = [a1, a2, a3];b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();bs = [b1, b2, b3];c1 = DisposeTestClassC.DisposeTestClassC();c2 = DisposeTestClassC.DisposeTestClassC();c3 = DisposeTestClassC.DisposeTestClassC();cs = [c1, c2, c3];}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;cDispose = DisposeTestClassC.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }



        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount35_StaticFunctionReturnObject_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();DisposeTestClassC.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();as = [a1, a2, a3];b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();bs = [b1, b2, b3];c1 = DisposeTestClassC.DisposeTestClassC();c2 = DisposeTestClassC.DisposeTestClassC();c3 = DisposeTestClassC.DisposeTestClassC();cs = [c1, c2, c3];}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;cDispose = DisposeTestClassC.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }



        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount36_StaticFunctionReturnObject_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();DisposeTestClassC.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();as = [a1, a2, a3];b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();bs = [b1, b2, b3];c1 = DisposeTestClassC.DisposeTestClassC();c2 = DisposeTestClassC.DisposeTestClassC();c3 = DisposeTestClassC.DisposeTestClassC();cs = [c1, c2, c3];}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;cDispose = DisposeTestClassC.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }



        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount37_MemberFunctionReturnObject_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();DisposeTestClassC.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();as = [a1, a2, a3];b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();bs = [b1, b2, b3];c1 = DisposeTestClassC.DisposeTestClassC();c2 = DisposeTestClassC.DisposeTestClassC();c3 = DisposeTestClassC.DisposeTestClassC();cs = [c1, c2, c3];}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;cDispose = DisposeTestClassC.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }


        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount38_MemberFunctionReturnObject_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();DisposeTestClassC.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();as = [a1, a2, a3];b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();bs = [b1, b2, b3];c1 = DisposeTestClassC.DisposeTestClassC();c2 = DisposeTestClassC.DisposeTestClassC();c3 = DisposeTestClassC.DisposeTestClassC();cs = [c1, c2, c3];}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;cDispose = DisposeTestClassC.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }



        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount39_GlobalFunctionReturnObject_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();DisposeTestClassC.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();as = [a1, a2, a3];b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();bs = [b1, b2, b3];c1 = DisposeTestClassC.DisposeTestClassC();c2 = DisposeTestClassC.DisposeTestClassC();c3 = DisposeTestClassC.DisposeTestClassC();cs = [c1, c2, c3];}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;cDispose = DisposeTestClassC.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }



        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount40_GlobalFunctionReturnObject_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();DisposeTestClassC.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();as = [a1, a2, a3];b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();bs = [b1, b2, b3];c1 = DisposeTestClassC.DisposeTestClassC();c2 = DisposeTestClassC.DisposeTestClassC();c3 = DisposeTestClassC.DisposeTestClassC();cs = [c1, c2, c3];}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;cDispose = DisposeTestClassC.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }



        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount41_MemberFunctionReturnArray_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();DisposeTestClassC.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();as = [a1, a2, a3];b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();bs = [b1, b2, b3];c1 = DisposeTestClassC.DisposeTestClassC();c2 = DisposeTestClassC.DisposeTestClassC();c3 = DisposeTestClassC.DisposeTestClassC();cs = [c1, c2, c3];}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;cDispose = DisposeTestClassC.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }



        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount42_MemberFunctionReturnArray_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();DisposeTestClassC.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();as = [a1, a2, a3];b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();bs = [b1, b2, b3];c1 = DisposeTestClassC.DisposeTestClassC();c2 = DisposeTestClassC.DisposeTestClassC();c3 = DisposeTestClassC.DisposeTestClassC();cs = [c1, c2, c3];}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;cDispose = DisposeTestClassC.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }


        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount43_MemberFunctionReturnArray_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();DisposeTestClassC.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();as = [a1, a2, a3];b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();bs = [b1, b2, b3];c1 = DisposeTestClassC.DisposeTestClassC();c2 = DisposeTestClassC.DisposeTestClassC();c3 = DisposeTestClassC.DisposeTestClassC();cs = [c1, c2, c3];}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;cDispose = DisposeTestClassC.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }



        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount44_MemberFunctionReturnObject_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();DisposeTestClassC.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();as = [a1, a2, a3];b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();bs = [b1, b2, b3];c1 = DisposeTestClassC.DisposeTestClassC();c2 = DisposeTestClassC.DisposeTestClassC();c3 = DisposeTestClassC.DisposeTestClassC();cs = [c1, c2, c3];}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;cDispose = DisposeTestClassC.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }



        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount45_MemberFunctionReturnObject_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();DisposeTestClassC.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();as = [a1, a2, a3];b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();bs = [b1, b2, b3];c1 = DisposeTestClassC.DisposeTestClassC();c2 = DisposeTestClassC.DisposeTestClassC();c3 = DisposeTestClassC.DisposeTestClassC();cs = [c1, c2, c3];}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;cDispose = DisposeTestClassC.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }



        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount46_GlobalFunctionReturnNewArray_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();DisposeTestClassC.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();as = [a1, a2, a3];b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();bs = [b1, b2, b3];c1 = DisposeTestClassC.DisposeTestClassC();c2 = DisposeTestClassC.DisposeTestClassC();c3 = DisposeTestClassC.DisposeTestClassC();cs = [c1, c2, c3];}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;cDispose = DisposeTestClassC.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }


        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount47_GlobalFunctionReturnNewArray_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();DisposeTestClassC.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();as = [a1, a2, a3];b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();bs = [b1, b2, b3];c1 = DisposeTestClassC.DisposeTestClassC();c2 = DisposeTestClassC.DisposeTestClassC();c3 = DisposeTestClassC.DisposeTestClassC();cs = [c1, c2, c3];}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;cDispose = DisposeTestClassC.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }



        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount48_GlobalFunctionReturnNewArray_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();DisposeTestClassC.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();as = [a1, a2, a3];b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();bs = [b1, b2, b3];c1 = DisposeTestClassC.DisposeTestClassC();c2 = DisposeTestClassC.DisposeTestClassC();c3 = DisposeTestClassC.DisposeTestClassC();cs = [c1, c2, c3];}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;cDispose = DisposeTestClassC.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }


        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount49_MemberFunctionReturnNewArray_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();DisposeTestClassC.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();as = [a1, a2, a3];b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();bs = [b1, b2, b3];c1 = DisposeTestClassC.DisposeTestClassC();c2 = DisposeTestClassC.DisposeTestClassC();c3 = DisposeTestClassC.DisposeTestClassC();cs = [c1, c2, c3];}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;cDispose = DisposeTestClassC.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }



        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount50_MemberFunctionReturnNewArray_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();DisposeTestClassC.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();as = [a1, a2, a3];b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();bs = [b1, b2, b3];c1 = DisposeTestClassC.DisposeTestClassC();c2 = DisposeTestClassC.DisposeTestClassC();c3 = DisposeTestClassC.DisposeTestClassC();cs = [c1, c2, c3];}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;cDispose = DisposeTestClassC.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }


        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount51_MemberFunctionReturnNewArray_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();DisposeTestClassC.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();as = [a1, a2, a3];b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();bs = [b1, b2, b3];c1 = DisposeTestClassC.DisposeTestClassC();c2 = DisposeTestClassC.DisposeTestClassC();c3 = DisposeTestClassC.DisposeTestClassC();cs = [c1, c2, c3];}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;cDispose = DisposeTestClassC.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }



        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount52_StaticFunctionReturnNewArray_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();DisposeTestClassC.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();as = [a1, a2, a3];b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();bs = [b1, b2, b3];c1 = DisposeTestClassC.DisposeTestClassC();c2 = DisposeTestClassC.DisposeTestClassC();c3 = DisposeTestClassC.DisposeTestClassC();cs = [c1, c2, c3];}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;cDispose = DisposeTestClassC.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }



        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount53_StaticFunctionReturnNewArray_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();DisposeTestClassC.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();as = [a1, a2, a3];b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();bs = [b1, b2, b3];c1 = DisposeTestClassC.DisposeTestClassC();c2 = DisposeTestClassC.DisposeTestClassC();c3 = DisposeTestClassC.DisposeTestClassC();cs = [c1, c2, c3];}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;cDispose = DisposeTestClassC.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }



        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount54_StaticFunctionReturnNewArray_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();DisposeTestClassC.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();as = [a1, a2, a3];b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();bs = [b1, b2, b3];c1 = DisposeTestClassC.DisposeTestClassC();c2 = DisposeTestClassC.DisposeTestClassC();c3 = DisposeTestClassC.DisposeTestClassC();cs = [c1, c2, c3];}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;cDispose = DisposeTestClassC.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }



        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount55_StaticFunctionReturnNewObject_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();DisposeTestClassC.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();as = [a1, a2, a3];b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();bs = [b1, b2, b3];c1 = DisposeTestClassC.DisposeTestClassC();c2 = DisposeTestClassC.DisposeTestClassC();c3 = DisposeTestClassC.DisposeTestClassC();cs = [c1, c2, c3];}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;cDispose = DisposeTestClassC.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }



        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount56_StaticFunctionReturnNewObject_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();DisposeTestClassC.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();as = [a1, a2, a3];b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();bs = [b1, b2, b3];c1 = DisposeTestClassC.DisposeTestClassC();c2 = DisposeTestClassC.DisposeTestClassC();c3 = DisposeTestClassC.DisposeTestClassC();cs = [c1, c2, c3];}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;cDispose = DisposeTestClassC.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }



        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount57_MemberFunctionReturnNewObject_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();DisposeTestClassC.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();as = [a1, a2, a3];b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();bs = [b1, b2, b3];c1 = DisposeTestClassC.DisposeTestClassC();c2 = DisposeTestClassC.DisposeTestClassC();c3 = DisposeTestClassC.DisposeTestClassC();cs = [c1, c2, c3];}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;cDispose = DisposeTestClassC.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }



        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount58_MemberFunctionReturnNewObject_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();DisposeTestClassC.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();as = [a1, a2, a3];b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();bs = [b1, b2, b3];c1 = DisposeTestClassC.DisposeTestClassC();c2 = DisposeTestClassC.DisposeTestClassC();c3 = DisposeTestClassC.DisposeTestClassC();cs = [c1, c2, c3];}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;cDispose = DisposeTestClassC.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }


        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount59_GlobalFunctionReturnNewObject_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();DisposeTestClassC.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();as = [a1, a2, a3];b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();bs = [b1, b2, b3];c1 = DisposeTestClassC.DisposeTestClassC();c2 = DisposeTestClassC.DisposeTestClassC();c3 = DisposeTestClassC.DisposeTestClassC();cs = [c1, c2, c3];}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;cDispose = DisposeTestClassC.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }



        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount60_GlobalFunctionReturnNewObject_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();DisposeTestClassC.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();as = [a1, a2, a3];b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();bs = [b1, b2, b3];c1 = DisposeTestClassC.DisposeTestClassC();c2 = DisposeTestClassC.DisposeTestClassC();c3 = DisposeTestClassC.DisposeTestClassC();cs = [c1, c2, c3];}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;cDispose = DisposeTestClassC.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }



        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount61_MemberFunctionReturnNewArray_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();DisposeTestClassC.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();as = [a1, a2, a3];b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();bs = [b1, b2, b3];c1 = DisposeTestClassC.DisposeTestClassC();c2 = DisposeTestClassC.DisposeTestClassC();c3 = DisposeTestClassC.DisposeTestClassC();cs = [c1, c2, c3];}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;cDispose = DisposeTestClassC.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }



        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount62_MemberFunctionReturnNewArray_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();DisposeTestClassC.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();as = [a1, a2, a3];b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();bs = [b1, b2, b3];c1 = DisposeTestClassC.DisposeTestClassC();c2 = DisposeTestClassC.DisposeTestClassC();c3 = DisposeTestClassC.DisposeTestClassC();cs = [c1, c2, c3];}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;cDispose = DisposeTestClassC.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }



        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount63_MemberFunctionReturnNewArray_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();DisposeTestClassC.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();as = [a1, a2, a3];b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();bs = [b1, b2, b3];c1 = DisposeTestClassC.DisposeTestClassC();c2 = DisposeTestClassC.DisposeTestClassC();c3 = DisposeTestClassC.DisposeTestClassC();cs = [c1, c2, c3];}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;cDispose = DisposeTestClassC.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }


        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount64_MemberFunctionReturnNewObject_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();DisposeTestClassC.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();as = [a1, a2, a3];b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();bs = [b1, b2, b3];c1 = DisposeTestClassC.DisposeTestClassC();c2 = DisposeTestClassC.DisposeTestClassC();c3 = DisposeTestClassC.DisposeTestClassC();cs = [c1, c2, c3];}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;cDispose = DisposeTestClassC.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }


        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount65_MemberFunctionReturnNewObject_Dispose()
        {
            string code =
                @"import(""FFITarget.dll"");DisposeTestClassA.Reset();DisposeTestClassB.Reset();DisposeTestClassC.Reset();[Associative]{a1 = DisposeTestClassA.DisposeTestClassA();a2 = DisposeTestClassA.DisposeTestClassA();a3 = DisposeTestClassA.DisposeTestClassA();as = [a1, a2, a3];b1 = DisposeTestClassB.DisposeTestClassB();b2 = DisposeTestClassB.DisposeTestClassB();b3 = DisposeTestClassB.DisposeTestClassB();bs = [b1, b2, b3];c1 = DisposeTestClassC.DisposeTestClassC();c2 = DisposeTestClassC.DisposeTestClassC();c3 = DisposeTestClassC.DisposeTestClassC();cs = [c1, c2, c3];}__GC();aDispose = DisposeTestClassA.count;bDispose = DisposeTestClassB.count;cDispose = DisposeTestClassC.count;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aDispose", 0);
            thisTest.Verify("bDispose", 0);
            thisTest.Verify("cDispose", 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount68_TemporaryArrayIndexing01()
        {
            string code = @"import(""FFITarget.dll"");DisposeTestClassD.Reset();[Associative]{    a = [DisposeTestClassD.DisposeTestClassD(), DisposeTestClassD.DisposeTestClassD(), DisposeTestClassD.DisposeTestClassD()][1];}__GC();d = DisposeTestClassD.count;";
            string errorString = "";
            thisTest.RunScriptSource(code, errorString);
            thisTest.Verify("d", 3);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount69_TemporaryArrayIndexing02()
        {
            string code = @"import(""FFITarget.dll"");DisposeTestClassD.Reset();[Associative]{    a = [DisposeTestClassD.DisposeTestClassD(), DisposeTestClassD.DisposeTestClassD(), DisposeTestClassD.DisposeTestClassD()][1];}__GC();d = DisposeTestClassD.count;
";
            string errorString = "";
            thisTest.RunScriptSource(code, errorString);
            thisTest.Verify("d", 3);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount70_TemporaryArrayIndexing03()
        {
            string code = @"import(""FFITarget.dll"");DisposeTestClassD.Reset();    def foo()    {        return = [DisposeTestClassD.DisposeTestClassD(), DisposeTestClassD.DisposeTestClassD(), DisposeTestClassD.DisposeTestClassD()];    }t = [Associative]{    a = (foo())[1];    return = a;}__GC();
d = DisposeTestClassD.count;
";
            string errorString = "";
            thisTest.RunScriptSource(code, errorString);
            thisTest.Verify("d", 2);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount71_TemporaryArrayIndexing04()
        {
            string code = @"import(""FFITarget.dll"");DisposeTestClassD.Reset();t = [Associative]{    a = (DisposeTestClassD.DisposeTestClassD(0..4))[1];    return = a;}__GC();
d = DisposeTestClassD.count;
";
            string errorString = "";
            thisTest.RunScriptSource(code, errorString);
            thisTest.Verify("d", 4);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount72_TemporaryDefaultArgument()
        {
            string code = @"import(""FFITarget.dll"");DisposeTestClassD.Reset();def foo(a = DisposeTestClassD.DisposeTestClassD()){}t = foo();__GC();
d = DisposeTestClassD.count;
";
            string errorString = "";
            thisTest.RunScriptSource(code, errorString);
            thisTest.Verify("d", 1);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReferenceCount73_FunctionPointer()
        {
            string code = @"import(""FFITarget.dll"");DisposeTestClassD.Reset();def foo(a = DisposeTestClassD.DisposeTestClassD()){    return = null;}t = foo; def bar(f:function){    return = f();}[Associative]{    r = bar(t);}__GC();d = DisposeTestClassD.count;";
            string errorString = "";
            thisTest.RunScriptSource(code, errorString);
            thisTest.Verify("d", 1);
        }
    }
}
