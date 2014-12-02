using System;
using System.Collections.Generic;
using FFITarget;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoFFI;
using ProtoTest.TD;
using ProtoTestFx.TD;
using ProtoTest;
namespace ProtoFFITests
{
    public abstract class FFITestSetup
    {
        private ProtoCore.Core Setup()
        {
            ProtoCore.Core core = new ProtoCore.Core(new ProtoCore.Options());
            core.Options.ExecutionMode = ProtoCore.ExecutionMode.Serial;
            core.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(core));
            core.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(core));
            DLLFFIHandler.Register(FFILanguage.CSharp, new CSModuleHelper());
            CLRModuleType.ClearTypes();
            return core;
        }
        protected struct ValidationData
        {
            public string ValueName;
            public dynamic ExpectedValue;
            public int BlockIndex;
        }
        protected int ExecuteAndVerify(String code, ValidationData[] data, Dictionary<string, Object> context)
        {
            int errors = 0;
            return ExecuteAndVerify(code, data, context, out errors);
        }
        protected int ExecuteAndVerify(String code, ValidationData[] data)
        {
            int errors = 0;
            return ExecuteAndVerify(code, data, out errors);
        }
        protected int ExecuteAndVerify(String code, ValidationData[] data, out int nErrors)
        {
            return ExecuteAndVerify(code, data, null, out nErrors);
        }
        protected int ExecuteAndVerify(String code, ValidationData[] data, Dictionary<string, Object> context, out int nErrors)
        {
            ProtoCore.Core core = Setup();
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(code, core, context);
            int nWarnings = core.RuntimeStatus.WarningCount;
            nErrors = core.BuildStatus.ErrorCount;
            if (data == null)
            {
                core.Cleanup();
                return nWarnings + nErrors;
            }
            TestFrameWork thisTest = new TestFrameWork();
            foreach (var item in data)
            {
                if (item.ExpectedValue == null)
                {
                    object nullOb = null;
                    TestFrameWork.Verify(mirror, item.ValueName, nullOb, item.BlockIndex);
                }
                else
                {
                    TestFrameWork.Verify(mirror, item.ValueName, item.ExpectedValue, item.BlockIndex);
                }
            }
            core.Cleanup();
            return nWarnings + nErrors;
        }
    }
    
    class CSFFITest : ProtoTestBase
    {
        [Test]
        public void TestImportDummyClass()
        {
            String code =
            @"              
               import (Dummy from ""FFITarget.dll"");
               dummy = Dummy.Dummy();
               success = dummy.CallMethod();
            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("success", true);
        }

        [Test]
        public void TestImportAllClasses()
        {
            String code =
            @"import(""FFITarget.dll"");
success;
x;
             [Associative] 
             {
               vec = DummyVector.ByCoordinates(1,0,0);
               point = DummyPoint.ByCoordinates(0,0,0);
               x = point.X;
             }
            ";

            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 0);
        }

        [Test]
        [Category("PortToCodeBlocks")]
        public void TestImportPointClass()
        {
            String code =
            @"import(DummyPoint from ""FFITarget.dll"");
x;
y;
z;
             [Associative] 
             {
               point = DummyPoint.ByCoordinates(1,2,3);
               x = point.X;
               y = point.Y;
               z = point.Z;
             }
            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 1.0);
            thisTest.Verify("y", 2.0);
            thisTest.Verify("z", 3.0);
        }

        [Test]
        public void TestImportPointClassWithoutImportingVectorClass()
        {
            String code =
            @"
               import(DummyPoint from ""FFITarget.dll"");
               p1 = DummyPoint.ByCoordinates(1,2,3);
               p2 = DummyPoint.ByCoordinates(3,4,5);
               v = p1.DirectionTo(p2);
               p3 = p1.Translate(v);
               px = p3.X;
               py = p3.Y;
               pz = p3.Z;
            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("px", 3.0);
            thisTest.Verify("py", 4.0);
            thisTest.Verify("pz", 5.0);
        }

        [Test]
        public void TestImportPointClassWithImportingVectorClass()
        {
            String code =
            @"
               import(""FFITarget.dll"");
               p1 = DummyPoint.ByCoordinates(1,2,3);
               p2 = DummyPoint.ByCoordinates(3,4,5);
               v = p1.DirectionTo(p2);
               vx = v.X;
               vy = v.Y;
               vz = v.Z;

            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("vx", 2.0);
            thisTest.Verify("vy", 2.0);
            thisTest.Verify("vz", 2.0);
        }

        [Test]
        [Category("Method Resolution")]
        public void TestInstanceMethodResolution()
        {
            String code =
            @"
            import(""FFITarget.dll"");
            cf1 = ClassFunctionality.ClassFunctionality(1);
            vc2 = ValueContainer.ValueContainer(2);
            o = cf1.AddWithValueContainer(vc2);
            o2 = vc2.Square().SomeValue;
            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("o", 3);
            thisTest.Verify("o2", 4);
        }

        [Test]
        public void TestProperty()
        {
            String code =
           @"
              import(""FFITarget.dll"");
              cf1 = ClassFunctionality.ClassFunctionality(1);
              cf2 = ClassFunctionality.ClassFunctionality(2);
              v = cf1.IntVal;
              t = cf1.IsEqualTo(cf2);
           ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("v", 1);
            thisTest.Verify("t", false);
        }

        [Test]
        public void TestStaticProperty()
        {
            String code =
           @"
              import(""FFITarget.dll"");
              cf1 = ClassFunctionality.ClassFunctionality(1);
              cf2 = ClassFunctionality.ClassFunctionality(2);
              ClassFunctionality.StaticProp = 42;
              v = cf1.StaticProp;
              t = cf2.StaticProp;
              s = ClassFunctionality.StaticProp;
           ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("v", 42);
            thisTest.Verify("s", 42);
        }


        [Test]
        public void TestArrayMarshling_MixedTypes()
        {
            String code =
            @"
               import (""FFITarget.dll"");
               dummy = Dummy.Dummy();
               arr = dummy.GetMixedObjects();
               value = arr[0].random123() - arr[1].GetNumber() + arr[2].Value + arr[3].Value;
            ";

            thisTest.RunScriptSource(code);
            thisTest.Verify("value", 128.0);
        }

        [Test]
        [Category("Method Resolution")]
        public void TestArrayElementReturnedFromFunction()
        {
            String code =
            @"
               import (""FFITarget.dll"");

               def GetAt(index : int)
               {
                    dummy = Dummy.Dummy();
                    arr = dummy.GetMixedObjects(); //GC of this array should not dispose the returned element.
                    return = arr[index];
               }
               a = GetAt(0);
               b = GetAt(1);
               c = GetAt(2);
               d = GetAt(3);
               x = {a.random123(), b.GetNumber(), c.Value, d.Value};
               value = a.random123() - b.GetNumber() + c.Value + d.Value;
            ";

            thisTest.RunScriptSource(code);
            thisTest.Verify("value", 128.0);
        }

        [Test]
        public void TestArrayMarshalling_DStoCS()
        {
            String code =
            @"
             import (""FFITarget.dll"");
             sum;
             [Associative] 
             {
                dummy = Dummy.Dummy();
                arr = 1..10.0;
                sum = dummy.SumAll(arr);
             }
            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("sum", 55);
        }

        [Test]
        public void TestArrayMarshalling_DStoCS_CStoDS()
        {
            String code =
            @"
             import (""FFITarget.dll"");
             sum;
             [Associative] 
             {
                dummy = Dummy.Dummy();
                arr = 1..10.0;
                arr_2 = dummy.Twice(arr);
                sum = dummy.SumAll(arr_2);
             }
            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("sum", 110.0);
        }

        [Test]
        public void TestListMarshalling_DStoCS()
        {
            String code =
            @"
             import (""FFITarget.dll"");
             sum;
             [Associative] 
             {
                dummy = Dummy.Dummy();
                arr = 1..10.0;
                sum = dummy.AddAll(arr);
             }
            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("sum", 55.0);
        }

        [Test]
        public void TestStackMarshalling_DStoCS_CStoDS()
        {
            String code =
            @"
             import (""FFITarget.dll"");
             size;
             [Associative] 
             {
                dummy = Dummy.Dummy();
                stack = dummy.DummyStack();
                size = dummy.StackSize(stack);
             }
            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("size", 3);
        }

        [Test]
        [Category("Failure")]
        public void TestDictionaryMarshalling_DStoCS_CStoDS()
        {
            // Tracked by: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4035
            String code =
            @"
             import (""FFITarget.dll"");
             [Associative] 
             {
                dummy = Dummy.Dummy();
                dictionary = 
                { 
                    dummy.CreateDictionary() => dict;
                    dummy.AddData(dict, ""ABCD"", 22);
                    dummy.AddData(dict, ""xyz"", 11);
                    dummy.AddData(dict, ""teas"", 12);
                }
                sum = dummy.SumAges(dictionary);
             }
            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("sum", 45);
        }

        [Test]
        public void TestListMarshalling_DStoCS_CStoDS()
        {
            String code =
            @"
             import (""FFITarget.dll"");
             sum;
             [Associative] 
             {
                dummy = Dummy.Dummy();
                arr = dummy.Range(1,10,1);
                sum = dummy.AddAll(arr);
             }
            ";

            thisTest.RunScriptSource(code);
            thisTest.Verify("sum", 55.0);
        }

        [Test]
        public void TestInheritanceImport()
        {
            String code =
            @"
               import (""FFITarget.dll"");
               dummy = DerivedDummy.DerivedDummy();
               num123 = dummy.random123();
            ";

            thisTest.RunScriptSource(code);
            thisTest.Verify("num123", 123);
        }

        [Test]
        public void TestMethodCallOnNullFFIObject()
        {
            String code =
            @"
               import (""FFITarget.dll"");
               dummy = Dummy.ReturnNullDummy();
               value = dummy.CallMethod();
            ";

            thisTest.RunScriptSource(code);
            thisTest.Verify("value", null);
        }

        [Test]
        public void TestStaticMethod()
        {
            String code =
            @"
               import (""FFITarget.dll"");
               value = Dummy.Return100();
            ";

            thisTest.RunScriptSource(code);
            thisTest.Verify("value", 100);
        }

        [Test]
        public void TestCtor()
        {
            String code =
            @"
               import (""FFITarget.dll"");
               value = Dummy.Return100();
            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("value", 100); 
        }

        [Test]
        public void TestInheritanceBaseClassMethodCall()
        {
            String code =
            @"
               import (""FFITarget.dll"");
               dummy = DerivedDummy.DerivedDummy();
               arr = 1..10.0;
               sum = dummy.SumAll(arr);
            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("sum", 55.0);
        }

        [Test]
        public void TestInheritanceVirtualMethodCallDerived()
        {
            String code =
            @"
               import (""FFITarget.dll"");
               tempDerived = DerivedDummy.DerivedDummy();
               dummy = tempDerived.CreateDummy(true); //for now static methods are not supported.
               isBase = dummy.CallMethod();
            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("isBase", false);
        }

        [Test]
        public void TestInheritanceVirtualMethodCallBase()
        {
            String code =
            @"
               import (""FFITarget.dll"");
               tempDerived = DerivedDummy.DerivedDummy();
               dummy = tempDerived.CreateDummy(false); //for now static methods are not supported.
               isBase = dummy.CallMethod();
            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("isBase", true);
        }

        [Test]
        public void TestInheritanceCtorsVirtualMethods()
        {
            string code = @"
                            import (""FFITarget.dll"");
                            b = Base.Create();
                            num = b.GetNumber();
                            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("num", 10.0);
        }

        [Test]
        public void TestInheritanceCtorsVirtualMethods2()
        {
            string code = @"
                            import (""FFITarget.dll"");
                            derived = Derived1.Create();
                            num = derived.GetNumber();
                            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("num", 20.0);
        }

        [Test]
        public void TestInheritanceCtorsVirtualMethods3()
        {
            string code = @"
                            import (""FFITarget.dll"");
                            b = Base.CreateDerived();
                            num = b.GetNumber();
                            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("num", 20.0);
        }

        [Test]
        [Category("PortToCodeBlocks")]
        public void TestInheritanceAcrossLangauges_CS_DS()
        {
            string code = @"
                import (DummyVector from ""FFITarget.dll"");
                class Vector2 extends DummyVector
                {
                    public constructor Vector2(x : double, y : double, z : double) : base ByCoordinates(x, y, z)
                    {}
                }
                
                vec2 = Vector2.Vector2(1,1,1);
                x = vec2.GetLength();
                ";
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () => thisTest.RunScriptSource(code));
        }
        /// <summary>
        /// This is to test Dispose method on IDisposable object. Dispose method 
        /// on IDisposable is renamed to _Dispose as DS destructor. Calling 
        /// Dispose doesn't affect the state.
        /// </summary>

        [Test]
        public void TestDisposeNotAvailable()
        {
            string code = @"
                            import (""FFITarget.dll"");
                            a = TestDispose.Create();
                            neglect = a.Dispose();
                            val = a.Value;
                            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("val", 15);
        }
        /// <summary>
        /// This is to test Dispose method on IDisposable object. Dispose method 
        /// on IDisposable is renamed to _Dispose as DS destructor. Calling 
        /// _Dispose will make the object a null.
        /// </summary>

        [Test]
        public void TestDisposable_Dispose()
        {
            string code = @"
                            import (""FFITarget.dll"");
                            a = TestDispose.Create();
                            neglect = a._Dispose();
                            val = a.Value;
                            ";

            thisTest.RunScriptSource(code);
            thisTest.Verify("val", null);
        }
        /// <summary>
        /// This is to test _Dispose method is added to all the classes.
        /// If object is IDisposable Dispose will be renamed to _Dispose
        /// </summary>

        [Test]
        public void TestDummyBase_Dispose()
        {
            string code = @"
                            import (""FFITarget.dll"");
                            a = DummyBase.Create();
                            neglect = a._Dispose();
                            val = a.Value;
                            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("val", null);
        }
        /// <summary>
        /// This is to test Dispose method is not renamed to _Dispose if the
        /// object is not IDisposable. Calling Dispose doesn't invalidate the
        /// object and value is set to -2, in Dispose implementation.
        /// </summary>

        [Test]
        public void TestDummyDisposeDispose()
        {
            string code = @"
                            import (""FFITarget.dll"");
                            a = DummyDispose.DummyDispose();
                            neglect = a.Dispose();
                            val = a.Value;
                            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("val", -2);
        }
        /// <summary>
        /// This test is to test for dispose due to update. When the same 
        /// variable is re-initialized, the previous instance will be disposed
        /// and this pointer will be re-used for next instance. Test if object
        /// reference is properly cleaned from CLRObjectMarshaler and this works
        /// without an issue.
        /// </summary>

        [Test]
        public void TestDisposeForUpdate()
        {
            string code = @"
                            import (""FFITarget.dll"");
                            a = DummyDispose.DummyDispose(); //instance1
                            a = DummyDispose.DummyDispose(); //instance2, instance1 will be freed
                            a = DummyDispose.DummyDispose(); //instance3, instance1 will be re-used.
                            val = a.Value;
                            ";

            thisTest.RunScriptSource(code);
            thisTest.Verify("val", 20);
        }
        /// <summary>
        /// This test is to test for dispose due to update. When the same 
        /// variable is re-initialized, the previous instance will be disposed
        /// and this pointer will be re-used for next instance. Test if object
        /// reference is properly cleaned from CLRObjectMarshaler and this works
        /// without an issue.
        /// </summary>

        [Test]
        public void TestDisposeForUpdate2()
        {
            string code = @"
                            import (""FFITarget.dll"");
                            a = DummyDispose.DummyDispose(); //instance1
                            a = DummyVector.ByCoordinates(1,1,1); //instance2, instance1 will be freed
                            a = DummyDispose.DummyDispose(); //instance3, instance1 will be re-used.
                            val = a.Value;
                            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("val", 20);
        }
        /// <summary>
        /// This is to test Dispose method renamed to _Dispose if the object
        /// is derived from IDisposable object. Calling _Dispose will make 
        /// the object null.
        /// </summary>
        [Test]
        public void TestDisposeDerived_Dispose()
        {
            string code = @"
                            import (""FFITarget.dll"");
                            a = AClass.CreateObject(42); 
                            a._Dispose();
                            val = DisposeVerify.GetValue();
                            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("val", 42);
        }

        /// <summary>
        /// This is to test import of multiple dlls.
        /// </summary>
        [Test]
        public void TestMultipleImport()
        {
            String code =
            @"
               import (""FFITarget.dll"");
               import(""DSCoreNodes.dll"");
               dummy = DerivedDummy.DerivedDummy();
               arr = 1..Math.Factorial(5);
               sum = dummy.SumAll(arr);
            ";

            thisTest.RunScriptSource(code);
            thisTest.Verify("sum", 7260.0);
        }

        /// <summary>
        /// This is to test import of multiple dlls.
        /// </summary>

        [Test]
        public void TestImportSameModuleMoreThanOnce()
        {
            String code =
            @"
               import(""FFITarget.dll"");
               import(""DSCoreNodes.dll"");
               import(""DSCoreNodes.dll"");
               dummy = DerivedDummy.DerivedDummy();
               arr = 1..Math.Factorial(5);
               sum = dummy.SumAll(arr);
            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("sum", 7260.0);
        }

        [Test]
        [Category("PortToCodeBlocks")]
        public void TestPropertyAccessor()
        {
            String code =
            @"
               import(""FFITarget.dll"");
               pt = DummyPoint.ByCoordinates(1,2,3);
               a = pt.X;
            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 1);
        }

        [Test]
        [Category("PortToCodeBlocks")]
        public void TestAssignmentSingleton()
        {
            String code =
            @"
               import(""FFITarget.dll"");
               pt = DummyPoint.ByCoordinates(1,2,3);
               a = { pt.X};
            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", new [] {1.0});
        }

        [Test]
        [Category("PortToCodeBlocks")]
        public void TestAssignmentAsArray()
        {
            String code =
            @"
               import(""FFITarget.dll"");
               pt = DummyPoint.ByCoordinates(1,2,3);
               a = { pt.X, pt.X};
               def test (pt : Point)
               {
                  return = { pt.X, pt.X};
               }
               c = test(pt);
            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", new [] {1.0, 1.0});
        }

        [Test]
        [Category("PortToCodeBlocks")]
        public void TestReturnFromFunctionSingle()
        {
            String code =
            @"
               import(""FFITarget.dll"");
            pt = DummyPoint.ByCoordinates(1,2,3);
            a = { pt.X, pt.X};
            def test (pt : DummyPoint)
            {
            return = {  pt.X};
            }
            b = test(pt);
            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("b", new [] {1.0});
        }

        [Test]
        [Category("PortToCodeBlocks")]
        public void Defect_1462300()
        {
            String code =
            @"
               import(""FFITarget.dll"");
            pt = DummyPoint.ByCoordinates(1,2,3);
            def test (pt : DummyPoint)
            {
            return = {  pt.X,pt.Y};
            }
            b = test(pt);
            ";

            thisTest.RunScriptSource(code);
            thisTest.Verify("b", new [] {1.0, 2.0});
        }

        [Test]
        [Category("PortToCodeBlocks")]
        public void geometryinClass()
        {
            String code =
            @"
               import(""FFITarget.dll"");
               pt1=DummyPoint.ByCoordinates(1.0,1.0,1.0);
               class baseClass
               { 
                    val1 : DummyPoint  ;
                    a:int;
                    constructor baseClass()
                    {
                        a=1;
                        val1=DummyPoint.ByCoordinates(1,1,1);
                    }           
                }
                instance1= baseClass.baseClass();
                a2=instance1.a;
                b2=instance1.val1;
                c2={b2.X,b2.Y,b2.Z};
            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a2", 1);
            thisTest.Verify("c2", new [] {1.0, 1.0, 1.0});
        }

        [Test]
        [Category("PortToCodeBlocks")]
        public void geometryArrayAssignment()
        {
            String code =
            @"
               import(""FFITarget.dll"");
              
               pt1=DummyPoint.ByCoordinates(1,1,1);
               pt2=DummyPoint.ByCoordinates(2,2,2);
               pt3=DummyPoint.ByCoordinates(3,3,3);
               pt4=DummyPoint.ByCoordinates(4,4,4);
               pt5=DummyPoint.ByCoordinates(5,5,5);
               pt6=DummyPoint.ByCoordinates(6,6,6);
a11;
a12;
b11;
b12;
               [Imperative]
               {
                    a    = { {pt1,pt2}, {pt3,pt4} };
                    a11  = {a[0][0].X,a[0][0].Y,a[0][0].Z};
                    a[1] = {pt5,pt6};
                    a12  = {a[1][1].X,a[1][1].Y,a[1][1].Z};
                    d    = a[0];
                    b    = { pt1, pt2 };
                    b11  = {b[0].X,b[0].Y,b[0].Z};
                    b[0] = {pt3,pt4,pt5};
                    b12  = {b[0][0].X,b[0][0].Y,b[0][0].Z};
                    e    = b[0];
                    e12  = {e[0].X,e[0].Y,e[0].Z};
               }
            ";

            thisTest.RunScriptSource(code);
            object[] c = new object[] { 1.0, 1.0, 1.0 };
            object[] d = new object[] { 4.0, 4.0, 4.0 };
            object[] e = new object[] { 3.0, 3.0, 3.0 };
            object[] f = new object[] { 6.0, 6.0, 6.0 };
            thisTest.Verify("a11", c);
            thisTest.Verify("a12", f);
            thisTest.Verify("b11", c);
            thisTest.Verify("b12", e);
        }

        [Test]
        [Category("PortToCodeBlocks")]
        public void geometryForLoop()
        {
            String code =
            @"
                import(""FFITarget.dll"");
                
                pt1=DummyPoint.ByCoordinates(1,1,1);
                pt2=DummyPoint.ByCoordinates(2,2,2);
                pt3=DummyPoint.ByCoordinates(3,3,3);
a11;
a12;
                [Imperative]
                {
                    a = { pt1, pt2, pt3 };
                    a11={a[0].X,a[0].Y,a[0].Z};
                    x = 0;
 
                    for (y in a )
                    {
                    
                    a[x]=pt3;
                    a12={a[0].X,a[0].Y,a[0].Z};
                    x=x+1;
                    }
                    
                 } 
            ";
            object[] c = new object[] { 1.0, 1.0, 1.0 };
            object[] e = new object[] { 3.0, 3.0, 3.0 };
            thisTest.RunScriptSource(code);
            thisTest.Verify("a11", c);
            thisTest.Verify("a12", e);
        }

        [Test]
        public void geometryFunction()
        {
            String code =
            @"
                import(""FFITarget.dll"");
              
                pt1=DummyPoint.ByCoordinates(1,1,1);
a11;
                [Associative]
                {
                    def foo : DummyPoint( a:DummyPoint)
                    {
                        return = a;
                    }
                    a = foo( pt1);
                    a11={a.X,a.Y,a.Z};
}
            ";
            object[] c = new object[] { 1.0, 1.0, 1.0 };
            thisTest.RunScriptSource(code);
            thisTest.Verify("a11", c);
        }

        [Test]
        public void geometryIfElse()
        {
            String code =
            @"
               import(""FFITarget.dll"");
         
a1;
ptcoords;
                [Imperative]
                {
                pt1=DummyPoint.ByCoordinates(1,1,1);
 
                 a1 = 10;
 
                 if( a1>=10 )
                 {
                pt1=DummyPoint.ByCoordinates(2,2,2);
                ptcoords={pt1.X,pt1.Y,pt1.Z};
                a1=1;
                 }
 
                 elseif( a1<2 )
                 {
                 pt1=DummyPoint.ByCoordinates(3,3,3);
                 }
                 else 
                 {
                pt1=DummyPoint.ByCoordinates(4,4,4);
                 }
                }
        
            ";
            object[] d = new object[] { 2.0, 2.0, 2.0 };
            thisTest.RunScriptSource(code);
            thisTest.Verify("a1", 1); 
            thisTest.Verify("ptcoords", d);
        }

        [Test]
        [Category("PortToCodeBlocks")]
        public void geometryInlineConditional()
        {
            String code =
            @"
               import(""FFITarget.dll"");
                pt1=DummyPoint.ByCoordinates(1,1,1);
                pt2=DummyPoint.ByCoordinates(2,2,2);
s11;
l11;
                [Imperative]
                {
                    def fo1 : int(a1 : int)
                    {
                        return = a1 * a1;
                    }
                    a	=	10;				
                    b	=	20;
                
                    smallest   =   a	<   b   ?   pt1	:	pt2;
                    largest	=   a	>   b   ?   pt1	:	pt2;
                    s11={smallest.X,smallest.Y,smallest.Z};
                    l11={largest.X,largest.Y,largest.Z};
                 }
        
            ";
            object[] c = new object[] { 1.0, 1.0, 1.0 };
            object[] d = new object[] { 2.0, 2.0, 2.0 };
            thisTest.RunScriptSource(code);
            thisTest.Verify("s11", c);
            thisTest.Verify("l11", d);
        }

        [Test]
        [Category("PortToCodeBlocks")]
        public void geometryRangeExpression()
        {
            String code =
            @"
               import(""FFITarget.dll"");
                 
                    a=1;
a12;
                    [Imperative]
                    {
                        a = 1/2..1/4..-1/4;
                    }
                    [Associative]
                    {
                    pt=DummyPoint.ByCoordinates(a[0],0,0);
                    a12={pt.X,pt.Y,pt.Z};
                    }
        
            ";
            object[] c = new object[] { 0.5, 0.0, 0.0 };
            thisTest.RunScriptSource(code);
            thisTest.Verify("a12", c);
        }

        [Test]
        public void TestExplicitGetterAndSetters()
        {
            string code = @"
                            import (""FFITarget.dll"");
                            i = 10;
                            a = DummyBase.DummyBase(i);
                            neglect = a.set_MyValue(15);
                            val = a.get_MyValue();
                            i = 5;
                            ";

            thisTest.RunScriptSource(code);
            thisTest.Verify("val", 15);
        }

        [Test]
        public void TestNullableTypes()
        {
            string code = @"
                            import (""FFITarget.dll"");
                            a = DummyBase.DummyBase(10);
                            v111 = a.TestNullable(null, null);
                            v123 = a.TestNullable(5, null);
                            v321 = a.TestNullable(null, 2);
                            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("v111", 111);
            thisTest.Verify("v123", 123);
            thisTest.Verify("v321", 321);
        }

        [Test]
        [Category("PortToCodeBlocks")]
        public void geometryWhileLoop()
        {
            String code =
            @"
               import(""FFITarget.dll"");
               pt={0,0,0,0,0,0};
p11;
               [Imperative]
               {
                    i=0;
                    temp=0;
                    while( i <= 5 )
                    { 
                        i = i + 1;
                        pt[i]=DummyPoint.ByCoordinates(i,1,1);
                        p11={pt[i].X,pt[i].Y,pt[i].Z};
                    }
                    
                }
            ";
            object[] a = new object[] { 6.0, 1.0, 1.0 };
            thisTest.RunScriptSource(code);
            thisTest.Verify("p11", a);
        }

        [Test]
        [Category("PortToCodeBlocks")]
        public void properties()
        {
            String code =
            @"
              import(""FFITarget.dll"");
              pt1 = DummyPoint.ByCoordinates(10, 10, 10);
              a=pt1.X;
            ";
            double a = 10.000000;
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", a);
        }

        [Test]
        [Ignore]
        [Category("Replication")]
        [Category("PortToCodeBlocks")]
        public void coercion_notimplmented()
        {
            String code =
            @"
               import (""FFITarget.dll"");
           vec =  DummyVector.ByCoordinates(1,0,0);
           newVec=vec.Scale({1,null});//array
           prop = VecGuarantedProperties(vec);
           def VecGuarantedProperties(vec :DummyVector)
           {
              return = {vec.GetLengthSquare() };
            }
        
            ";
            object[] c = new object[] { new object[] { 1.0 }, null };
            thisTest.RunScriptSource(code);
            thisTest.Verify("prop", c);
        }

        [Test]
        [Category("Update")]
        public void SimplePropertyUpdate()
        {
            string code = @"
                            import (""FFITarget.dll"");
                            i = 10;
                            a = DummyBase.DummyBase(i);
                            a.Value = 15;
                            val = a.Value;
                            i = 5;
                            ";

            thisTest.RunScriptSource(code);
            thisTest.Verify("val", 15);
        }

        [Test]
        public void SimplePropertyUpdateUsingSetMethod()
        {
            string code = @"
                            import (""FFITarget.dll"");
                            i = 10;
                            a = DummyBase.DummyBase(i);
                            neglect = a.SetValue(15);
                            val = a.Value;
                            i = 5;
                            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("val", 15);
        }

        [Test]
        [Category("Update")]
        public void PropertyReadback()
        {
            string code =
                @"import(""FFITarget.dll"");
                cls = ClassFunctionality.ClassFunctionality();
                cls.IntVal = 3;
                readback = cls.IntVal;";

            thisTest.RunScriptSource(code);
            thisTest.Verify("readback", (Int64)3);
        }

        [Test]
        [Category("Update")]
        public void PropertyUpdate()
        {
            string code =
                @"import(""FFITarget.dll"");
                cls = ClassFunctionality.ClassFunctionality();
                cls.IntVal = 3;
                readback = cls.IntVal;
                cls.IntVal = 4;";
            thisTest.RunScriptSource(code);
            thisTest.Verify("readback", (Int64)4);
        }

        [Test]
        public void DisposeOnFFITest001()
        {
            string code = @"
                            import (""FFITarget.dll"");
                            def foo : int()
                            {
                                a = AClass.CreateObject(2);
                                return = 3;
                            }
                            dv = DisposeVerify.CreateObject();
                            m = dv.SetValue(1);
                            a = foo();
                            b = dv.GetValue();
                            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("m", 1); 
            thisTest.Verify("a", 3); 
            thisTest.Verify("b", 2); 
        }

        [Test]
        public void DisposeOnFFITest004()
        {
            string code = @"
                            import (""FFITarget.dll"");
                            def foo : BClass(b : BClass)
                            {
                                a1 = AClass.CreateObject(9);
                                a2 = { BClass.CreateObject(1), BClass.CreateObject(2), BClass.CreateObject(3), BClass.CreateObject(4) };    
                                a4 = b;
                                a3 = BClass.CreateObject(5);
                                
                                return = a3;
                            }
                            dv = DisposeVerify.CreateObject();
                            m = dv.SetValue(1);
                            a = BClass.CreateObject(-1);
                            b = foo(a);
                            c = dv.GetValue();
                            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("m", 1);
            thisTest.Verify("c", 19);
        }

        [Test]
        public void DisposeOnFFITest005()
        {
            string code = @"
                            import (""FFITarget.dll"");
                            class Foo
                            {
                                a : A;
                                b : B;
                                
                                constructor Foo(_a : A, _b : B)
                                {
                                    a = _a; b = _b;
                                }   
                            }
                            def foo : int()
                            {
                                fb = BClass.CreateObject(9);
                                fa = AClass.CreateObject(8);
                                ff = Foo.Foo(fa, fb);
                                return = 3;
                            }
                           
                            dv = DisposeVerify.CreateObject();
                            m = dv.SetValue(1);
                            a = foo();
                            b = dv.GetValue();
                            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 3);
            thisTest.Verify("b", 17);
        }

        [Test]
        public void DisposeOnFFITest002()
        {
            string code = @"
                            import (""FFITarget.dll"");
                            def foo : AClass()
                            {
                                a = AClass.CreateObject(10);
                                return = a;
                            }
                            dv = DisposeVerify.CreateObject();
                            m = dv.SetValue(1);
                            a = foo();
                            b = dv.GetValue();
                            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("b", 1);
        }

        [Test]
        public void DisposeOnFFITest003()
        {
            string code = @"
                            import (""FFITarget.dll"");
                            def foo : int(a : AClass)
                            {
                                b = a;
                                return = 1;
                            }
                            dv = DisposeVerify.CreateObject();
                            m = dv.SetValue(2);
                            a = AClass.CreateObject(3);
                            b = foo(a);
                            c = dv.GetValue();
                            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("c", 2);
        }

        [Test]
        public void DisposeOnFFITest006()
        {
            string code = @"
                            import (""FFITarget.dll"");
                            dv = DisposeVerify.CreateObject();
                            m = dv.SetValue(2);
                            a1 = AClass.CreateObject(3);
                            a2 = AClass.CreateObject(4);
                            a2 = a1;
                            b = { BClass.CreateObject(1), BClass.CreateObject(2), BClass.CreateObject(3) };
                            b = a1;    
                            v = dv.GetValue();
                            ";
            thisTest.RunScriptSource(code);
            var gcStrategy =  thisTest.SetupTestCore().Heap.GCStrategy;
            if (gcStrategy == ProtoCore.DSASM.Heap.GCStrategies.kReferenceCounting)
            {
                thisTest.Verify("v", 10);
            }
            // For mark and sweep, the order of object dispose is not defined,
            // so we are not sure if AClass objects or BClass objects will be
            // disposed firstly, hence we can't verify the expected value.
        }

        [Test]
        public void DisposeOnFFITest007()
        {
            string code = @"
                            import (""FFITarget.dll"");
                            class Foo
                            {
                                b : BClass;
                                constructor Foo(_b : BClass)
                                {
                                    b = _b;
                                }
                                def foo : int(_b : BClass)
                                {
                                    b = _b;
                                    return = 0;
                                }
                            }
                            v1;
                            v2;
                            dv = DisposeVerify.CreateObject();
                            [Imperative]
                            {
                                m = dv.SetValue(3);
                                b1 = BClass.CreateObject(10);                            
                                f = Foo.Foo(b1);
                                m = f.foo(b1);
                                b2 = BClass.CreateObject(20);
                                b3 = BClass.CreateObject(30);
                                f2 = Foo.Foo(b2);
                                b2 = null;
                                v1 = dv.GetValue();
                                m = f2.foo(b3);
                                v2 = dv.GetValue();
                            }
                            v3 = dv.GetValue();
                            ";
            thisTest.RunScriptSource(code);
            var gcStrategy =  thisTest.GetTestCore().Heap.GCStrategy;
            if (gcStrategy == ProtoCore.DSASM.Heap.GCStrategies.kMarkAndSweep)
            {
                // For mark and sweep, it is straight, we only need to focus 
                // on created objects. So the value = 3 + 10 + 20 + 30 = 63.
                thisTest.Verify("v3", 63);
            }
            else
            {
                thisTest.Verify("v1", 3); 
                thisTest.Verify("v2", 23);
            }
        }

        [Test]
        public void DisposeOnFFITest008()
        {
            string code = @"
                            import (""FFITarget.dll"");
                            class Foo
                            {
                                a : AClass;
                                b : BClass;
                                constructor Foo(_a : AClass, _b : BClass)
                                {
                                    a = _a; b = _b;
                                }
                                def foo : int(_a : AClass, _b : BClass)
                                {
                                    a = _a; b = _b;
                                    return = 0;
                                }
                            }
                            v1;
                            v2;
                            v3;
                            dv = DisposeVerify.CreateObject();
                            [Imperative]
                            {
                                m = dv.SetValue(3);
                                a1 = AClass.CreateObject(3);
                                b1 = BClass.CreateObject(13);
                                b2 = BClass.CreateObject(14);
                                b3 = BClass.CreateObject(15);
                                a1 = a1;
                                v1 = dv.GetValue();
                                a2 = AClass.CreateObject(4);
                                f = Foo.Foo(a1, b1);
                                f2 = Foo.Foo(a1, b2);
                                //f.a = f2.a;
                                b1 = b3;
                                b2 = b3;
                                v2 = dv.GetValue();
                                f = f2;
                                v3 = dv.GetValue();
                            }
                            v4 = dv.GetValue();
                            ";

            thisTest.RunScriptSource(code);
            var gcStrategy = thisTest.GetTestCore().Heap.GCStrategy;
            if (gcStrategy == ProtoCore.DSASM.Heap.GCStrategies.kMarkAndSweep)
            {
                // For mark and sweep, the last object is a2, se the expected
                // value = 4. But it is very fragile because the order of
                // disposing is not defined. 
                thisTest.Verify("v4", 4);
            }
            else
            {
                thisTest.Verify("v1", 3); 
                thisTest.Verify("v2", 3); 
                thisTest.Verify("v3", 16);
            }
        }

        [Test]
        public void TestReplicationAndDispose()
        {
            //Defect: DG-1464910 Sprint 22: Rev:2385-324564: Planes get disposed after querying properties on returned planes.
            string code = @"
                            import (""FFITarget.dll"");
                            def foo : int(a : AClass)
                            {
                                return = a.Value;
                            }
                            dv = DisposeVerify.CreateObject();
                            m = dv.SetValue(2);
                            a = {AClass.CreateObject(1), AClass.CreateObject(2), AClass.CreateObject(3)};
                            b = foo(a);
                            c = dv.GetValue();
                            ";
            thisTest.RunScriptSource(code);
            object[] b = new object[] { 1, 2, 3 };
            thisTest.Verify("c", 2);
            thisTest.Verify("b", b);
        }

        [Test]
        [Category("ProtoGeometry")]
        public void TestNonBrowsableClass()
        {
            string code = @"
                import(""ProtoGeometry.dll"");
                ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue(thisTest.GetClassIndex("Geometry") != ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(thisTest.GetClassIndex("Point") != ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(thisTest.GetClassIndex("DesignScriptEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(thisTest.GetClassIndex("GeometryFactory") == ProtoCore.DSASM.Constants.kInvalidIndex);
        }

        [Test]
        [Category("ProtoGeometry")] 
        public void TestImportNonBrowsableClass()
        {
            string code = @"
                import(DesignScriptEntity from ""ProtoGeometry.dll"");
                ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue(thisTest.GetClassIndex("Geometry") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(thisTest.GetClassIndex("Point") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(thisTest.GetClassIndex("DesignScriptEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(thisTest.GetClassIndex("GeometryFactory") == ProtoCore.DSASM.Constants.kInvalidIndex);
        }

        [Test]
        [Category("ProtoGeometry")] 
        public void TestImportBrowsableClass()
        {
            string code = @"
                import(NurbsCurve from ""ProtoGeometry.dll"");
                ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //This import must import NurbsCurve and related classes.
            Assert.IsTrue(thisTest.GetClassIndex("Geometry") != ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(thisTest.GetClassIndex("Point") != ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(thisTest.GetClassIndex("Vector") != ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(thisTest.GetClassIndex("Solid") != ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(thisTest.GetClassIndex("Surface") != ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(thisTest.GetClassIndex("Plane") != ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(thisTest.GetClassIndex("CoordinateSystem") != ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(thisTest.GetClassIndex("CoordinateSystem") != ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(thisTest.GetClassIndex("Curve") != ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(thisTest.GetClassIndex("NurbsCurve") != ProtoCore.DSASM.Constants.kInvalidIndex);
            //Non-browsable as well as unrelated class should not be imported.
            Assert.IsTrue(thisTest.GetClassIndex("DesignScriptEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(thisTest.GetClassIndex("Circle") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(thisTest.GetClassIndex("SubDivisionMesh") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(thisTest.GetClassIndex("GeometryFactory") == ProtoCore.DSASM.Constants.kInvalidIndex);
        }

        [Test]
        [Category("ProtoGeometry")] 
        public void TestNonBrowsableInterfaces()
        {
            string code = @"
                import(""ProtoGeometry.dll"");
                ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue(thisTest.GetClassIndex("Geometry") != ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(thisTest.GetClassIndex("IColor") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(thisTest.GetClassIndex("IDesignScriptEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(thisTest.GetClassIndex("IDisplayable") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(thisTest.GetClassIndex("IPersistentObject") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(thisTest.GetClassIndex("IPersistencyManager") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(thisTest.GetClassIndex("ICoordinateSystemEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(thisTest.GetClassIndex("IGeometryEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(thisTest.GetClassIndex("IPointEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(thisTest.GetClassIndex("ICurveEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(thisTest.GetClassIndex("ILineEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(thisTest.GetClassIndex("ICircleEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(thisTest.GetClassIndex("IArcEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(thisTest.GetClassIndex("IBSplineCurveEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(thisTest.GetClassIndex("IBRepEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(thisTest.GetClassIndex("ISurfaceEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(thisTest.GetClassIndex("IBSplineSurfaceEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(thisTest.GetClassIndex("IPlaneEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(thisTest.GetClassIndex("ISolidEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(thisTest.GetClassIndex("IPrimitiveSolidEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(thisTest.GetClassIndex("IConeEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(thisTest.GetClassIndex("ICuboidEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(thisTest.GetClassIndex("ISphereEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(thisTest.GetClassIndex("IPolygonEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(thisTest.GetClassIndex("ISubDMeshEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(thisTest.GetClassIndex("IBlockEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(thisTest.GetClassIndex("IBlockHelper") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(thisTest.GetClassIndex("ITopologyEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(thisTest.GetClassIndex("IShellEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(thisTest.GetClassIndex("ICellEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(thisTest.GetClassIndex("IFaceEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(thisTest.GetClassIndex("ICellFaceEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(thisTest.GetClassIndex("IVertexEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(thisTest.GetClassIndex("IEdgeEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(thisTest.GetClassIndex("ITextEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(thisTest.GetClassIndex("IGeometryFactory") == ProtoCore.DSASM.Constants.kInvalidIndex);
        }

        [Test]
        [Category("ProtoGeometry")] 
        public void TestDefaultConstructorNotAvailableOnAbstractClass()
        {
            string code = @"
                import(""ProtoGeometry.dll"");
                ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verify that Geometry.Geometry constructor deson't exists
            thisTest.VerifyMethodExists("Geometry", "Geometry", false);
        }

        [Test]
        [Category("PortToCodeBlocks")]
        public void TestNestedClass()
        {
            string code =
               @"import(NestedClass from ""FFITarget.dll"");
                 
                t = NestedClass.GetType(5);
                success = NestedClass.CheckType(t, 5);
                ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("success", true);
        }

        [Test]
        public void TestNestedClassImport()
        {
            string code =
               @"import(NestedClass from ""FFITarget.dll"");
                t = NestedClass.GetType(123);
                t5 = NestedClass_Type.Type(123);
                success = t5.Equals(t);
                ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("success", true);
        }

 
        [Test]
        public void TestNamespaceImport()
        {
            string code =
                @"import(Point from ""FFITarget.dll"");";
            thisTest.RunScriptSource(code);
            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kMultipleSymbolFound);
            string[] classes = thisTest.GetAllMatchingClasses("Point");
            Assert.True(classes.Length > 1, "More than one implementation of Point class expected");
        }

        [Test]
        public void TestNamespaceFullResolution01()
        {
            thisTest.RunScriptSource(
            @"
                import(""FFITarget.dll"");
                p = A.NamespaceResolutionTargetTest.NamespaceResolutionTargetTest();
                x = p.Prop;
            "
            );

            thisTest.Verify("x", 0);
        }

        [Test]
        public void TestNamespaceFullResolution02()
        {
            thisTest.RunScriptSource(
            @"
                import(""FFITarget.dll"");
                p = A.NamespaceResolutionTargetTest.NamespaceResolutionTargetTest(1);
                x = p.Prop;
            "
            );

            thisTest.Verify("x", 1);
        }

        [Test]
        public void TestNamespaceFullResolution03()
        {
            thisTest.RunScriptSource(
            @"
                import(""FFITarget.dll"");
                p = A.NamespaceResolutionTargetTest.Foo(1);
                x = p.Prop;
            "
            );

            thisTest.Verify("x", 1);
        }

        [Test]
        public void TestNamespacePartialResolution01()
        {
            thisTest.RunScriptSource(
            @"
                import(""FFITarget.dll"");
                p = A.NamespaceResolutionTargetTest.Foo(1);
                x = p.Prop;
            "
            );

            thisTest.Verify("x", 1);
        }

        [Test]
        public void TestNamespacePartialResolution02()
        {
            thisTest.RunScriptSource(
            @"
                import(""FFITarget.dll"");
                p = A.NamespaceResolutionTargetTest(1);
                x = p.Prop;
            "
            );

            thisTest.Verify("x", 1);
        }

        [Test]
        public void TestNamespacePartialResolution03()
        {
            thisTest.RunScriptSource(
            @"
                import(""FFITarget.dll"");
                p = B.NamespaceResolutionTargetTest();
                x = p.Prop;
            "
            );

            thisTest.Verify("x", 1);
        }

        [Test]
        [Category("Failure")]
        public void TestNamespaceClassResolution()
        {
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1947
            string code =
                @"import(""FFITarget.dll"");
                    x = 1..2;

                    Xo = x[0];

                    aDup = A.DupTargetTest(x);
                    aReadback = aDup.Prop[0];

                    bDup = B.DupTargetTest(x);
                    bReadback = bDup.Prop[1];

                    check = Equals(aDup.Prop,bDup.Prop);";

            thisTest.RunScriptSource(code);
            thisTest.Verify("check", true);
            thisTest.Verify("Xo", 1);

            thisTest.Verify("aReadback", 1);
            thisTest.Verify("bReadback", 2);

            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kMultipleSymbolFound);
            string[] classes = thisTest.GetAllMatchingClasses("DupTargetTest");
            Assert.True(classes.Length > 1, "More than one implementation of DupTargetTest class expected");
        }

        [Test]
        [Category("Failure")]
        public void TestSubNamespaceClassResolution()
        {
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1947
            string code =
                @"import(""FFITarget.dll"");
                    aDup = A.DupTargetTest(0);
                    aReadback = aDup.Prop;

                    bDup = B.DupTargetTest(1); //This should match exactly BClass.DupTargetTest
                    bReadback = bDup.Prop;
                    
                    cDup = C.B.DupTargetTest(2);
                    cReadback = cDup.Prop;

                    check = Equals(aDup.Prop,bDup.Prop);
                    check = Equals(bDup.Prop,cDup.Prop);
";
            string err = "MAGN-1947 IntegrationTests.NamespaceConflictTest.DupImportTest";
            thisTest.RunScriptSource(code, err);
            thisTest.Verify("check", true);
            thisTest.Verify("aReadback", 0);
            thisTest.Verify("bReadback", 1);
            thisTest.Verify("cReadback", 2);

            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kMultipleSymbolFound);
            string[] classes = thisTest.GetAllMatchingClasses("DupTargetTest");
            Assert.True(classes.Length > 1, "More than one implementation of DupTargetTest class expected");
        }

        [Test]
        public void FFI_Target_Inheritence()
        {
            String code =
            @"              
import(""FFITarget.dll"");
o = InheritenceDriver.Gen();
oy = o.Y;
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("oy", 2);
        }

        [Test]
        public void InheritenceMethodInvokeOnEmptyDerivedClassInstance()
        {
            string code =
                @"
import(UnknownPoint from ""FFITarget.dll"");
p = DummyPoint.ByCoordinates(1, 2, 3);
u = p.UnknownPoint();
newPoint = u.Translate(1,2,3);
value = {u.X, u.Y, u.Z};
                 ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("value", new double[] {1,2,3});
            thisTest.Verify("newPoint", FFITarget.DummyPoint.ByCoordinates(2, 4, 6));
        }

        [Test]
        public void InheritenceMethodInvokeWithoutImportDerivedClass()
        {
            string code =
                @"
import(DummyPoint from ""FFITarget.dll"");
p = DummyPoint.ByCoordinates(1, 2, 3);
u = p.UnknownPoint();
newPoint = u.Translate(1,2,3);
value = {u.X, u.Y, u.Z};
                 ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("value", new double[] { 1, 2, 3 });
            thisTest.Verify("newPoint", FFITarget.DummyPoint.ByCoordinates(2, 4, 6));
        }
    }
}
