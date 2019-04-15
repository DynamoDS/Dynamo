
using System;
using System.Collections.Generic;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoFFI;
using ProtoTestFx.TD;
namespace ProtoFFITests
{
    public abstract class FFITestSetup
    {
        public ProtoCore.Core Setup()
        {
            ProtoCore.Core core = new ProtoCore.Core(new ProtoCore.Options());
            core.Options.ExecutionMode = ProtoCore.ExecutionMode.Serial;
            core.Compilers.Add(ProtoCore.Language.Associative, new ProtoAssociative.Compiler(core));
            core.Compilers.Add(ProtoCore.Language.Imperative, new ProtoImperative.Compiler(core));
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
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            ProtoCore.CompileTime.Context compileContext = new ProtoCore.CompileTime.Context(code, context);
            ProtoCore.RuntimeCore runtimeCore = null;
            ExecutionMirror mirror = fsr.Execute(compileContext, core, out runtimeCore);
            int nWarnings = runtimeCore.RuntimeStatus.WarningCount;
            nErrors = core.BuildStatus.ErrorCount;
            if (data == null)
            {
                runtimeCore.Cleanup();
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
            runtimeCore.Cleanup();
            return nWarnings + nErrors;
        }
    }
    public class CSFFITest : FFITestSetup
    {
        /*
[Test]
        public void TestMethodResolution()
        {
            String code =
            @"
               import(""ProtoGeometry.dll"");
               line1 = Line.ByStartPointEndPoint(Point.ByCoordinates(0,0,0), Point.ByCoordinates(2,2,0));
                line2 = Line.ByStartPointEndPoint(Point.ByCoordinates(2,0,0), Point.ByCoordinates(0,2,0));
               geom = line1.Intersect(line2);
            ";
            ValidationData[] data = { new ValidationData() { ValueName = "geom", ExpectedValue = (Int64)1, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }*/
        TestFrameWork thisTest = new TestFrameWork();

        [Test]
        public void TestImportDummyClass()
        {
            String code =
            @"              
               dummy = Dummy.Dummy();
               success = dummy.CallMethod();
            ";
            Type dummy = typeof (FFITarget.Dummy);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "success", ExpectedValue = true, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
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
            ValidationData[] data = { new ValidationData { ValueName="x",         ExpectedValue = 0.0, BlockIndex = 0}
                                    };
            ExecuteAndVerify(code, data);
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
            ValidationData[] data =
                {
                    new ValidationData { ValueName = "px", ExpectedValue = 3.0, BlockIndex = 0 },
                    new ValidationData { ValueName = "py", ExpectedValue = 4.0, BlockIndex = 0 },
                    new ValidationData { ValueName = "pz", ExpectedValue = 5.0, BlockIndex = 0 }
                
                };
            ExecuteAndVerify(code, data);
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
            ValidationData[] data =
                {
                    new ValidationData { ValueName = "vx", ExpectedValue = 2.0, BlockIndex = 0 },
                    new ValidationData { ValueName = "vy", ExpectedValue = 2.0, BlockIndex = 0 },
                    new ValidationData { ValueName = "vz", ExpectedValue = 2.0, BlockIndex = 0 }
                
                };
            ExecuteAndVerify(code, data);
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
            ValidationData[] data =
                {
                    new ValidationData { ValueName = "o", ExpectedValue = 3, BlockIndex = 0 },
                    new ValidationData { ValueName = "o2", ExpectedValue = 4, BlockIndex = 0 }

                };

            ExecuteAndVerify(code, data);
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
           ValidationData[] data =
               {
                   new ValidationData { ValueName = "v", ExpectedValue = 1, BlockIndex = 0 },
                   new ValidationData { ValueName = "t", ExpectedValue = false, BlockIndex = 0 }

               };
           ExecuteAndVerify(code, data);

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

            ValidationData[] data =
               {
                   new ValidationData { ValueName = "v", ExpectedValue = 42, BlockIndex = 0 },
                   new ValidationData { ValueName = "s", ExpectedValue = 42, BlockIndex = 0 },
                   new ValidationData { ValueName = "t", ExpectedValue = 42, BlockIndex = 0 }

               };
            
            ExecuteAndVerify(code, data);
        }


        [Test]
        public void TestArrayMarshling_MixedTypes()
        {
            String code =
            @"
               dummy = Dummy.Dummy();
               arr = dummy.GetMixedObjects();
               value = arr[0].random123() - arr[1].GetNumber() + arr[2].Value + arr[3].Value;
            ";
            Type dummy = typeof (FFITarget.DerivedDummy);
            Type derived1 = typeof (FFITarget.Derived1);
            Type testdispose = typeof (FFITarget.TestDispose);
            Type dummydispose = typeof (FFITarget.DummyDispose);
            code = string.Format("import(\"DesignScriptBuiltin.dll\");\r\nimport(\"{0}\");\r\nimport(\"{1}\");\r\nimport(\"{2}\");\r\nimport(\"{3}\");\r\n{4}",
                dummy.AssemblyQualifiedName, derived1.AssemblyQualifiedName, testdispose.AssemblyQualifiedName, dummydispose.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "value", ExpectedValue = 128.0, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }


        [Test]
        [Category("Method Resolution")]
        public void TestArrayElementReturnedFromFunction()
        {
            String code =
            @"
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
               x = [a.random123(), b.GetNumber(), c.Value, d.Value];
               value = a.random123() - b.GetNumber() + c.Value + d.Value;
            ";
            Type dummy = typeof (FFITarget.DerivedDummy);
            Type derived1 = typeof (FFITarget.Derived1);
            Type testdispose = typeof (FFITarget.TestDispose);
            Type dummydispose = typeof (FFITarget.DummyDispose);
            code = string.Format("import(\"DesignScriptBuiltin.dll\");\r\nimport(\"{0}\");\r\nimport(\"{1}\");\r\nimport(\"{2}\");\r\nimport(\"{3}\");\r\n{4}",
                dummy.AssemblyQualifiedName, derived1.AssemblyQualifiedName, testdispose.AssemblyQualifiedName, dummydispose.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "value", ExpectedValue = 128.0, BlockIndex = 0 } };
            Assert.IsTrue(ExecuteAndVerify(code, data) == 0); //runs without any error
        }

        [Test]
        public void TestArrayMarshalling_DStoCS()
        {
            String code =
            @"
sum;
             [Associative] 
             {
                dummy = Dummy.Dummy();
                arr = 1..10.0;
                sum = dummy.SumAll1D(arr);
             }
            ";
            Type dummy = typeof (FFITarget.Dummy);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "sum", ExpectedValue = 55.0, BlockIndex = 0 } };
            Assert.IsTrue(ExecuteAndVerify(code, data) == 0); //runs without any error
        }

        [Test]
        public void TestArrayMarshalling_DStoCS_CStoDS()
        {
            String code =
            @"
sum;
             [Associative] 
             {
                dummy = Dummy.Dummy();
                arr = 1..10.0;
                arr_2 = dummy.Twice(arr);
                sum = dummy.SumAll1D(arr_2);
             }
            ";
            Type dummy = typeof (FFITarget.Dummy);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "sum", ExpectedValue = 110.0, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestListMarshalling_DStoCS()
        {
            String code =
            @"
sum;
             [Associative] 
             {
                dummy = Dummy.Dummy();
                arr = 1..10.0;
                sum = dummy.AddAll(arr);
             }
            ";
            Type dummy = typeof (FFITarget.Dummy);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "sum", ExpectedValue = 55.0, BlockIndex = 0 } };
            Assert.IsTrue(ExecuteAndVerify(code, data) == 0); //runs without any error
        }

        [Test]
        public void TestStackMarshalling_DStoCS_CStoDS()
        {
            String code =
            @"
size;
             [Associative] 
             {
                dummy = Dummy.Dummy();
                stack = dummy.DummyStack();
                size = dummy.StackSize(stack);
             }
            ";
            Type dummy = typeof (FFITarget.DerivedDummy);
            Type derived1 = typeof(FFITarget.Derived1);
            Type testdispose = typeof(FFITarget.TestDispose);
            Type dummydispose = typeof(FFITarget.DummyDispose);
            code = string.Format("import(\"{0}\");\r\nimport(\"{1}\");\r\nimport(\"{2}\");\r\nimport(\"{3}\");\r\n{4}",
                dummy.AssemblyQualifiedName, derived1.AssemblyQualifiedName, testdispose.AssemblyQualifiedName, dummydispose.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "size", ExpectedValue = 3, BlockIndex = 0 } };
            Assert.IsTrue(ExecuteAndVerify(code, data) == 0); //runs without any error
        }

        [Test]
        public void TestListMarshalling_DStoCS_CStoDS()
        {
            String code =
            @"
sum;
             [Associative] 
             {
                dummy = Dummy.Dummy();
                arr = dummy.Range(1,10,1);
                sum = dummy.AddAll(arr);
             }
            ";
            Type dummy = typeof (FFITarget.Dummy);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "sum", ExpectedValue = 55.0, BlockIndex = 0 } };
            Assert.IsTrue(ExecuteAndVerify(code, data) == 0); //runs without any error
        }

        [Test]
        public void TestInheritanceImport()
        {
            String code =
            @"
               dummy = DerivedDummy.DerivedDummy();
               num123 = dummy.random123();
            ";
            Type dummy = typeof (FFITarget.DerivedDummy);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "num123", ExpectedValue = (Int64)123, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestMethodCallOnNullFFIObject()
        {
            String code =
            @"
               dummy = Dummy.ReturnNullDummy();
               value = dummy.CallMethod();
            ";
            Type dummy = typeof (FFITarget.Dummy);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "value", ExpectedValue = null, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestStaticMethod()
        {
            String code =
            @"
               value = Dummy.Return100();
            ";
            Type dummy = typeof (FFITarget.Dummy);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "value", ExpectedValue = (Int64)100, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestCtor()
        {
            String code =
            @"
               value = Dummy.Return100();
            ";
            Type dummy = typeof (FFITarget.Dummy);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "value", ExpectedValue = (Int64)100, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestInheritanceBaseClassMethodCall()
        {
            String code =
            @"
               dummy = DerivedDummy.DerivedDummy();
               arr = 1..10.0;
               sum = dummy.SumAll1D(arr);
            ";
            Type dummy = typeof (FFITarget.DerivedDummy);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "sum", ExpectedValue = 55.0, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestInheritanceVirtualMethodCallDerived()
        {
            String code =
            @"
               tempDerived = DerivedDummy.DerivedDummy();
               dummy = tempDerived.CreateDummy(true); //for now static methods are not supported.
               isBase = dummy.CallMethod();
            ";
            Type dummy = typeof (FFITarget.DerivedDummy);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "isBase", ExpectedValue = false, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestInheritanceVirtualMethodCallBase()
        {
            String code =
            @"
               tempDerived = DerivedDummy.DerivedDummy();
               dummy = tempDerived.CreateDummy(false); //for now static methods are not supported.
               isBase = dummy.CallMethod();
            ";
            Type dummy = typeof (FFITarget.DerivedDummy);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "isBase", ExpectedValue = true, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestInheritanceCtorsVirtualMethods()
        {
            string code = @"
                            b = Base.Create();
                            num = b.GetNumber();
                            ";
            Type dummy = typeof (FFITarget.Derived1);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            Console.WriteLine(code);
            ValidationData[] data = { new ValidationData { ValueName = "num", ExpectedValue = 10.0, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestInheritanceCtorsVirtualMethods2()
        {
            string code = @"
                            derived = Derived1.Create();
                            num = derived.GetNumber();
                            ";
            Type dummy = typeof (FFITarget.Derived1);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "num", ExpectedValue = 20.0, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestInheritanceCtorsVirtualMethods3()
        {
            string code = @"
                            b = Base.CreateDerived();
                            num = b.GetNumber();
                            ";
            Type dummy = typeof (FFITarget.Derived1);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "num", ExpectedValue = 20.0, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestDisposeNotAvailable()
        {
            string code = @"
                            a = TestDispose.Create();
                            neglect = a.Dispose();
                            val = a.Value;
                            ";
            Type dummy = typeof (FFITarget.TestDispose);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "val", ExpectedValue = (Int64)15, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
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
                            a = TestDispose.Create();
                            neglect = a._Dispose();
                            val = a.Value;
                            ";
            Type dummy = typeof (FFITarget.TestDispose);
            
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "val", ExpectedValue = null, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }
        /// <summary>
        /// This is to test _Dispose method is added to all the classes.
        /// If object is IDisposable Dispose will be renamed to _Dispose
        /// </summary>

        [Test]
        public void TestDummyBase_Dispose()
        {
            string code = @"
                            a = DummyBase.Create();
                            neglect = a._Dispose();
                            val = a.Value;
                            ";
            Type dummy = typeof (FFITarget.DummyBase);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "val", ExpectedValue = null, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
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
                            a = DummyDispose.DummyDispose();
                            neglect = a.Dispose();
                            val = a.Value;
                            ";
            Type dummy = typeof (FFITarget.DummyDispose);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "val", ExpectedValue = (Int64)(-2), BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
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
                            a = DummyDispose.DummyDispose(); //instance1
                            a = DummyDispose.DummyDispose(); //instance2, instance1 will be freed
                            a = DummyDispose.DummyDispose(); //instance3, instance1 will be re-used.
                            val = a.Value;
                            ";

            Type dummy = typeof (FFITarget.DummyDispose);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "val", ExpectedValue = (Int64)(20), BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
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
                            import(""FFITarget.dll"");
                            a = DummyDispose.DummyDispose(); //instance1
                            a = DummyVector.ByCoordinates(1,1,1); //instance2, instance1 will be freed
                            a = DummyDispose.DummyDispose(); //instance3, instance1 will be re-used.
                            val = a.Value;
                            ";
            Type dummy = typeof (FFITarget.DummyDispose);
            ValidationData[] data = { new ValidationData { ValueName = "val", ExpectedValue = (Int64)(20), BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
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
                            a = TestDisposeDerived.CreateDerived();
                            neglect = AClass._Dispose();
                            val = AClass.Value;
                            ";
            Type dummy = typeof (FFITarget.TestDisposeDerived);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "val", ExpectedValue = null, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestReturnFromFunctionSingle()
        {
            String code =
            @"
           import(""FFITarget.dll"");
            pt = DummyPoint.ByCoordinates(1,2,3);
            a = [ pt.X, pt.X];
            def test (pt : DummyPoint)
            {
            return = [  pt.X];
            }
            b = test(pt);
            ";
            var b = new object[] { 1.0 };
            ValidationData[] data = { new ValidationData { ValueName = "b", ExpectedValue = b, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void Defect_1462300()
        {
            String code =
            @"
            import(""FFITarget.dll"");
            pt = DummyPoint.ByCoordinates(1,2,3);
            def test (pt : DummyPoint)
            {
            return = [  pt.X,pt.Y];
            }
            b = test(pt);
            ";
            object[] b = new object[] { 1.0, 2.0 };
            ValidationData[] data = { new ValidationData { ValueName = "b", ExpectedValue = b, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void geometryArrayAssignment()
        {
            String code =
            @"
                import(""DesignScriptBuiltin.dll"");
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
               arr = [Imperative]
               {
                    a    = [ [pt1,pt2], [pt3,pt4] ];
                    a11  = [a[0][0].X,a[0][0].Y,a[0][0].Z];
                    a[1] = [pt5,pt6];
                    a12  = [a[1][1].X,a[1][1].Y,a[1][1].Z];
                    d    = a[0];
                    b    = [ pt1, pt2 ];
                    b11  = [b[0].X,b[0].Y,b[0].Z];
                    b[0] = [pt3,pt4,pt5];
                    b12  = [b[0][0].X,b[0][0].Y,b[0][0].Z];
                    e    = b[0];
                    e12  = [e[0].X,e[0].Y,e[0].Z];
                    
                    return [a11, a12, b11, b12];
               }
            ";
            var c = new[] {1.0, 1.0, 1.0};
            var e = new[] {3.0, 3.0, 3.0};
            var f = new[] {6.0, 6.0, 6.0};
            var res = new[] {c, f, c, e};
            ValidationData[] data = { new ValidationData { ValueName = "arr", ExpectedValue = res, BlockIndex = 0 }};
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void geometryForLoop()
        {
            String code =
            @"
                import(""DesignScriptBuiltin.dll"");
                import(""FFITarget.dll"");
                
                pt1=DummyPoint.ByCoordinates(1,1,1);
                pt2=DummyPoint.ByCoordinates(2,2,2);
                pt3=DummyPoint.ByCoordinates(3,3,3);
a11;
a12;
                arr = [Imperative]
                {
                    a = [ pt1, pt2, pt3 ];
                    a11=[a[0].X,a[0].Y,a[0].Z];
                    x = 0;
 
                    for (y in a )
                    {
                        a[x]=pt3;
                        a12=[a[0].X,a[0].Y,a[0].Z];
                        x=x+1;
                    }
                    return [a11, a12];
                 } 
            ";
            var c = new[] { 1.0, 1.0, 1.0 };
            var e = new[] { 3.0, 3.0, 3.0 };
            var res = new[] {c, e};
            ValidationData[] data = { new ValidationData { ValueName = "arr", ExpectedValue = res, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void geometryFunction()
        {
            String code =
            @"
                import(""FFITarget.dll"");
                pt1=DummyPoint.ByCoordinates(1,1,1);
                def foo : DummyPoint( a:DummyPoint)
                {
                    return = a;
                }
                a = foo( pt1);
                a11=[a.X,a.Y,a.Z];
            ";
            object[] c = new object[] { 1.0, 1.0, 1.0 };
            ValidationData[] data = { new ValidationData { ValueName = "a11", ExpectedValue = c, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void geometryIfElse()
        {
            String code =
            @"
               import(""FFITarget.dll"");
         
                ptcoords;
                i = [Imperative]
                {
                    pt1=DummyPoint.ByCoordinates(1,1,1);
                    a1 = 10;
 
                    if( a1>=10 )
                    {
                       pt1=DummyPoint.ByCoordinates(2,2,2);
                       ptcoords=[pt1.X,pt1.Y,pt1.Z];
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
                    return [a1, ptcoords];
                }
        
            ";
            var d = new object[] {1, new[] {2.0, 2.0, 2.0}};
            ValidationData[] data = { new ValidationData { ValueName = "i", ExpectedValue = d, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void geometryInlineConditional()
        {
            String code =
            @"
               import(""FFITarget.dll"");
                pt1=DummyPoint.ByCoordinates(1,1,1);
                pt2=DummyPoint.ByCoordinates(2,2,2);

                i = [Imperative]
                {
                    a	=	10;				
                    b	=	20;
                    smallest   =   a	<   b   ?   pt1	:	pt2;
                    largest	=   a	>   b   ?   pt1	:	pt2;
                    s11=[smallest.X,smallest.Y,smallest.Z];
                    l11=[largest.X,largest.Y,largest.Z];
                    return [s11, l11];
                 }
        
            ";
            var c = new [] { 1.0, 1.0, 1.0 };
            var d = new [] { 2.0, 2.0, 2.0 };
            var e = new[] {c, d};

            ValidationData[] data = { new ValidationData { ValueName = "i", ExpectedValue = e, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void geometryRangeExpression()
        {
            String code =
            @"
               import(""DesignScriptBuiltin.dll"");
               import(""FFITarget.dll"");
                 
                a=1;
                a12;
                a = [Imperative]
                {
                    return 1/2..1/4..-1/4;
                }
                [Associative]
                {
                    pt=DummyPoint.ByCoordinates(a[0],0,0);
                    a12=[pt.X,pt.Y,pt.Z];
                }
            ";
            var c = new[] { 0.5, 0.0, 0.0 };
            ValidationData[] data = { new ValidationData { ValueName = "a12", ExpectedValue = c, BlockIndex = 0 } 
                                      };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestExplicitGetterAndSetters()
        {
            string code = @"
                            i = 10;
                            a = DummyBase.DummyBase(i);
                            neglect = a.set_MyValue(15);
                            val = a.get_MyValue();
                            i = 5;
                            ";

            Type dummy = typeof(FFITarget.DummyBase);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "val", ExpectedValue = (Int64)15, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestNullableTypes()
        {
            string code = @"
                            a = DummyBase.DummyBase(10);
                            v111 = a.TestNullable(null, null);
                            v123 = a.TestNullable(5, null);
                            v321 = a.TestNullable(null, 2);
                            ";
            Type dummy = typeof(FFITarget.DummyBase);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "v111", ExpectedValue = (Int64)111, BlockIndex = 0 },
                                      new ValidationData { ValueName = "v123", ExpectedValue = (Int64)123, BlockIndex = 0 },
                                      new ValidationData { ValueName = "v321", ExpectedValue = (Int64)321, BlockIndex = 0 }
                                    };
            Assert.IsTrue(ExecuteAndVerify(code, data) == 0); //runs without any error
        }

        [Test]
        public void geometryWhileLoop()
        {
            String code =
            @"
               import(""FFITarget.dll"");
               pt=[0,0,0,0,0,0];

               x = [Imperative]
               {
                    p11;
                    i=0;
                    temp=0;
                    while( i <= 5 )
                    { 
                        i = i + 1;
                        pt[i]=DummyPoint.ByCoordinates(i,1,1);
                        p11=[pt[i].X,pt[i].Y,pt[i].Z];
                    }
                    return p11;
                }
            ";
            object[] a = new object[] { 6.0, 1.0, 1.0 };
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", a);
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
                            __GC();
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
                                a1 = BClass.CreateObject(9);
                                a2 = [ BClass.CreateObject(1), BClass.CreateObject(2), BClass.CreateObject(3), BClass.CreateObject(4) ];    
                                a4 = b;
                                a3 = BClass.CreateObject(5);
                                
                                return = a3;
                            }
                            dv = DisposeVerify.CreateObject();
                            m = dv.SetValue(1);
                            a = BClass.CreateObject(-1);
                            b = foo(a);
                            __GC();
                            c = dv.GetValue();
                            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("m", 1);
            thisTest.Verify("c", 20);
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
                            b = [ BClass.CreateObject(1), BClass.CreateObject(2), BClass.CreateObject(3) ];
                            b = a1;    
                            v = dv.GetValue();
                            ";
            thisTest.RunScriptSource(code);
            // For mark and sweep, the order of object dispose is not defined,
            // so we are not sure if AClass objects or BClass objects will be
            // disposed firstly, hence we can't verify the expected value.
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
                            a = [AClass.CreateObject(1), AClass.CreateObject(2), AClass.CreateObject(3)];
                            b = foo(a);
                            c = dv.GetValue();
                            ";
            thisTest.RunScriptSource(code);
            object[] b = new object[] { 1, 2, 3 };
            thisTest.Verify("c", 2);
            thisTest.Verify("b", b);
        }

        [Test]
        public void TestDefaultConstructorNotAvailableOnAbstractClass()
        {
            string code = @"
                import(""FFITarget.dll"");
                ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verify that AbstractDisposeTracert.AbstractDisposeTracert constructor deson't exists
            thisTest.VerifyMethodExists("AbstractDisposeTracert", "AbstractDisposeTracert", false);
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
            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.MultipleSymbolFound);
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
        public void TestNamespaceClassResolution()
        {
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1947
            string code =
                @"import(""FFITarget.dll"");
                    import(""BuiltIn.ds"");

                    x = 1..2;
                    Xo = x[0];

                    aDup = A.DupTargetTest.DupTargetTest(x);
                    aReadback = aDup.Prop[0];

                    bDup = B.DupTargetTest.DupTargetTest(x);
                    bReadback = bDup.Prop[1];";

            thisTest.RunScriptSource(code);
            thisTest.Verify("Xo", 1);

            thisTest.Verify("aReadback", 1);
            thisTest.Verify("bReadback", null);

            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.MultipleSymbolFound);
            string[] classes = thisTest.GetAllMatchingClasses("DupTargetTest");
            Assert.True(classes.Length > 1, "More than one implementation of DupTargetTest class expected");
        }

        [Test]
        public void TestSubNamespaceClassResolution()
        {
            string code =
                @"import(""FFITarget.dll"");
                    import(""BuiltIn.ds"");
                    aDup = A.DupTargetTest.DupTargetTest(0);
                    aReadback = aDup.Prop;

                    bDup = B.DupTargetTest.DupTargetTest(1);
                    bReadback = bDup.Prop;

                    cDup = C.B.DupTargetTest.DupTargetTest(2);
                    cReadback = cDup.Prop;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("aReadback", 0);
            thisTest.Verify("bDup", null);
            thisTest.Verify("bReadback", null);
            thisTest.Verify("cReadback", 2);

            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.MultipleSymbolFound);
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
value = [u.X, u.Y, u.Z];
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
value = [u.X, u.Y, u.Z];
                 ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("value", new double[] { 1, 2, 3 });
            thisTest.Verify("newPoint", FFITarget.DummyPoint.ByCoordinates(2, 4, 6));
        }

        [Test]
        public void AllowRankReductionAttributeWorksForProperty()
        {
            string code =
                @"import(""FFITarget.dll"");
rankReduceTestObject = FFITarget.TestRankReduce(""test"");
property = rankReduceTestObject.Property; 
reducedProperty = rankReduceTestObject.RankReduceProperty; ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("property", new List<string> { "test" });
            thisTest.Verify("reducedProperty", "test");
        }

        [Test]
        public void AllowRankReductionAttributeWorksForMethod()
        {
            string code =
                @"import(""FFITarget.dll"");
rankReduceTestObject = FFITarget.TestRankReduce(""test"");
method = rankReduceTestObject.Method(); 
reducedMethod = rankReduceTestObject.RankReduceMethod(); ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("method", new List<string> { "test" });
            thisTest.Verify("reducedMethod", "test");
        }
    }
}




