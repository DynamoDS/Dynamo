using System;
using System.Collections.Generic;
using FFITarget;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoFFI;
using ProtoTest.TD;
using ProtoTestFx.TD;
namespace ProtoFFITests
{
    public abstract class FFITestSetup
    {
        public ProtoCore.Core Setup()
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
    public class CSFFITest : FFITestSetup
    {
        /*
[Test]        public void TestMethodResolution()        {            String code =            @"               import(""ProtoGeometry.dll"");               line1 = Line.ByStartPointEndPoint(Point.ByCoordinates(0,0,0), Point.ByCoordinates(2,2,0));                line2 = Line.ByStartPointEndPoint(Point.ByCoordinates(2,0,0), Point.ByCoordinates(0,2,0));               geom = line1.Intersect(line2);            ";            ValidationData[] data = { new ValidationData() { ValueName = "geom", ExpectedValue = (Int64)1, BlockIndex = 0 } };            ExecuteAndVerify(code, data);        }*/
        TestFrameWork thisTest = new TestFrameWork();

        [Test]
        public void TestImportDummyClass()
        {
            String code =
            @"                             dummy = Dummy.Dummy();               success = dummy.CallMethod();            ";
            Type dummy = typeof (FFITarget.Dummy);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "success", ExpectedValue = true, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestImportAllClasses()
        {
            String code =
            @"import(""FFITarget.dll"");success;x;             [Associative]              {               vec = DummyVector.ByCoordinates(1,0,0);               point = DummyPoint.ByCoordinates(0,0,0);               x = point.X;             }            ";
            ValidationData[] data = { new ValidationData { ValueName="x",         ExpectedValue = 0.0, BlockIndex = 0}                                    };
            ExecuteAndVerify(code, data);
        }


        [Test]
        public void TestImportPointClassWithoutImportingVectorClass()
        {
            String code =
            @"               import(DummyPoint from ""FFITarget.dll"");               p1 = DummyPoint.ByCoordinates(1,2,3);               p2 = DummyPoint.ByCoordinates(3,4,5);               v = p1.DirectionTo(p2);               p3 = p1.Translate(v);               px = p3.X;               py = p3.Y;               pz = p3.Z;            ";
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
            @"               import(""FFITarget.dll"");               p1 = DummyPoint.ByCoordinates(1,2,3);               p2 = DummyPoint.ByCoordinates(3,4,5);               v = p1.DirectionTo(p2);               vx = v.X;               vy = v.Y;               vz = v.Z;            ";
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
            @"            import(""FFITarget.dll"");            cf1 = ClassFunctionality.ClassFunctionality(1);            vc2 = ValueContainer.ValueContainer(2);            o = cf1.AddWithValueContainer(vc2);            o2 = vc2.Square().SomeValue;            ";
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
           @"              import(""FFITarget.dll"");              cf1 = ClassFunctionality.ClassFunctionality(1);              cf2 = ClassFunctionality.ClassFunctionality(2);              v = cf1.IntVal;              t = cf1.IsEqualTo(cf2);           ";
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
           @"              import(""FFITarget.dll"");              cf1 = ClassFunctionality.ClassFunctionality(1);              cf2 = ClassFunctionality.ClassFunctionality(2);              ClassFunctionality.StaticProp = 42;              v = cf1.StaticProp;              t = cf2.StaticProp;              s = ClassFunctionality.StaticProp;           ";

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
            @"               dummy = Dummy.Dummy();               arr = dummy.GetMixedObjects();               value = arr[0].random123() - arr[1].GetNumber() + arr[2].Value + arr[3].Value;            ";
            Type dummy = typeof (FFITarget.DerivedDummy);
            Type derived1 = typeof (FFITarget.Derived1);
            Type testdispose = typeof (FFITarget.TestDispose);
            Type dummydispose = typeof (FFITarget.DummyDispose);
            code = string.Format("import(\"{0}\");\r\nimport(\"{1}\");\r\nimport(\"{2}\");\r\nimport(\"{3}\");\r\n{4}",
                dummy.AssemblyQualifiedName, derived1.AssemblyQualifiedName, testdispose.AssemblyQualifiedName, dummydispose.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "value", ExpectedValue = 128.0, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }


        [Test]
        [Category("Method Resolution")]
        public void TestArrayElementReturnedFromFunction()
        {
            String code =
            @"               def GetAt(index : int)               {                    dummy = Dummy.Dummy();                    arr = dummy.GetMixedObjects(); //GC of this array should not dispose the returned element.                    return = arr[index];               }               a = GetAt(0);               b = GetAt(1);               c = GetAt(2);               d = GetAt(3);               x = {a.random123(), b.GetNumber(), c.Value, d.Value};               value = a.random123() - b.GetNumber() + c.Value + d.Value;            ";
            Type dummy = typeof (FFITarget.DerivedDummy);
            Type derived1 = typeof (FFITarget.Derived1);
            Type testdispose = typeof (FFITarget.TestDispose);
            Type dummydispose = typeof (FFITarget.DummyDispose);
            code = string.Format("import(\"{0}\");\r\nimport(\"{1}\");\r\nimport(\"{2}\");\r\nimport(\"{3}\");\r\n{4}",
                dummy.AssemblyQualifiedName, derived1.AssemblyQualifiedName, testdispose.AssemblyQualifiedName, dummydispose.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "value", ExpectedValue = 128.0, BlockIndex = 0 } };
            Assert.IsTrue(ExecuteAndVerify(code, data) == 0); //runs without any error
        }

        [Test]
        public void TestArrayMarshalling_DStoCS()
        {
            String code =
            @"sum;             [Associative]              {                dummy = Dummy.Dummy();                arr = 1..10.0;                sum = dummy.SumAll(arr);             }            ";
            Type dummy = typeof (FFITarget.Dummy);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "sum", ExpectedValue = 55.0, BlockIndex = 0 } };
            Assert.IsTrue(ExecuteAndVerify(code, data) == 0); //runs without any error
        }

        [Test]
        public void TestArrayMarshalling_DStoCS_CStoDS()
        {
            String code =
            @"sum;             [Associative]              {                dummy = Dummy.Dummy();                arr = 1..10.0;                arr_2 = dummy.Twice(arr);                sum = dummy.SumAll(arr_2);             }            ";
            Type dummy = typeof (FFITarget.Dummy);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "sum", ExpectedValue = 110.0, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestListMarshalling_DStoCS()
        {
            String code =
            @"sum;             [Associative]              {                dummy = Dummy.Dummy();                arr = 1..10.0;                sum = dummy.AddAll(arr);             }            ";
            Type dummy = typeof (FFITarget.Dummy);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "sum", ExpectedValue = 55.0, BlockIndex = 0 } };
            Assert.IsTrue(ExecuteAndVerify(code, data) == 0); //runs without any error
        }

        [Test]
        public void TestStackMarshalling_DStoCS_CStoDS()
        {
            String code =
            @"size;             [Associative]              {                dummy = Dummy.Dummy();                stack = dummy.DummyStack();                size = dummy.StackSize(stack);             }            ";
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
        [Category("Failing")]
        public void TestDictionaryMarshalling_DStoCS_CStoDS()
        {
            // Tracked by: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4035
            String code =
            @"             [Associative]              {                dummy = Dummy.Dummy();                dictionary =                 {                     dummy.CreateDictionary() => dict;                    dummy.AddData(dict, ""ABCD"", 22);                    dummy.AddData(dict, ""xyz"", 11);                    dummy.AddData(dict, ""teas"", 12);                }                sum = dummy.SumAges(dictionary);             }            ";
            Type dummy = typeof (FFITarget.Dummy);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "sum", ExpectedValue = 45, BlockIndex = 1 } };
            Assert.IsTrue(ExecuteAndVerify(code, data) == 0); //runs without any error
        }

        [Test]
        public void TestListMarshalling_DStoCS_CStoDS()
        {
            String code =
            @"sum;             [Associative]              {                dummy = Dummy.Dummy();                arr = dummy.Range(1,10,1);                sum = dummy.AddAll(arr);             }            ";
            Type dummy = typeof (FFITarget.Dummy);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "sum", ExpectedValue = 55.0, BlockIndex = 0 } };
            Assert.IsTrue(ExecuteAndVerify(code, data) == 0); //runs without any error
        }

        [Test]
        public void TestInheritanceImport()
        {
            String code =
            @"               dummy = DerivedDummy.DerivedDummy();               num123 = dummy.random123();            ";
            Type dummy = typeof (FFITarget.DerivedDummy);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "num123", ExpectedValue = (Int64)123, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestMethodCallOnNullFFIObject()
        {
            String code =
            @"               dummy = Dummy.ReturnNullDummy();               value = dummy.CallMethod();            ";
            Type dummy = typeof (FFITarget.Dummy);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "value", ExpectedValue = null, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestStaticMethod()
        {
            String code =
            @"               value = Dummy.Return100();            ";
            Type dummy = typeof (FFITarget.Dummy);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "value", ExpectedValue = (Int64)100, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestCtor()
        {
            String code =
            @"               value = Dummy.Return100();            ";
            Type dummy = typeof (FFITarget.Dummy);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "value", ExpectedValue = (Int64)100, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestInheritanceBaseClassMethodCall()
        {
            String code =
            @"               dummy = DerivedDummy.DerivedDummy();               arr = 1..10.0;               sum = dummy.SumAll(arr);            ";
            Type dummy = typeof (FFITarget.DerivedDummy);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "sum", ExpectedValue = 55.0, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestInheritanceVirtualMethodCallDerived()
        {
            String code =
            @"               tempDerived = DerivedDummy.DerivedDummy();               dummy = tempDerived.CreateDummy(true); //for now static methods are not supported.               isBase = dummy.CallMethod();            ";
            Type dummy = typeof (FFITarget.DerivedDummy);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "isBase", ExpectedValue = false, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestInheritanceVirtualMethodCallBase()
        {
            String code =
            @"               tempDerived = DerivedDummy.DerivedDummy();               dummy = tempDerived.CreateDummy(false); //for now static methods are not supported.               isBase = dummy.CallMethod();            ";
            Type dummy = typeof (FFITarget.DerivedDummy);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "isBase", ExpectedValue = true, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestInheritanceCtorsVirtualMethods()
        {
            string code = @"                            b = Base.Create();                            num = b.GetNumber();                            ";
            Type dummy = typeof (FFITarget.Derived1);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            Console.WriteLine(code);
            ValidationData[] data = { new ValidationData { ValueName = "num", ExpectedValue = 10.0, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestInheritanceCtorsVirtualMethods2()
        {
            string code = @"                            derived = Derived1.Create();                            num = derived.GetNumber();                            ";
            Type dummy = typeof (FFITarget.Derived1);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "num", ExpectedValue = 20.0, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestInheritanceCtorsVirtualMethods3()
        {
            string code = @"                            b = Base.CreateDerived();                            num = b.GetNumber();                            ";
            Type dummy = typeof (FFITarget.Derived1);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "num", ExpectedValue = 20.0, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        /// <summary>
        /// This is to test Dispose method on IDisposable object. Dispose method 
        /// on IDisposable is renamed to _Dispose as DS destructor. Calling 
        /// Dispose doesn't affect the state.
        /// </summary>

        [Test]
        public void TestDisposeNotAvailable()
        {
            string code = @"                            a = TestDispose.Create();                            neglect = a.Dispose();                            val = a.Value;                            ";
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
            string code = @"                            a = TestDispose.Create();                            neglect = a._Dispose();                            val = a.Value;                            ";
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
            string code = @"                            a = DummyBase.Create();                            neglect = a._Dispose();                            val = a.Value;                            ";
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
            string code = @"                            a = DummyDispose.DummyDispose();                            neglect = a.Dispose();                            val = a.Value;                            ";
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
            string code = @"                            a = DummyDispose.DummyDispose(); //instance1                            a = DummyDispose.DummyDispose(); //instance2, instance1 will be freed                            a = DummyDispose.DummyDispose(); //instance3, instance1 will be re-used.                            val = a.Value;                            ";

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
            string code = @"                            import(""FFITarget.dll"");                            a = DummyDispose.DummyDispose(); //instance1                            a = DummyVector.ByCoordinates(1,1,1); //instance2, instance1 will be freed                            a = DummyDispose.DummyDispose(); //instance3, instance1 will be re-used.                            val = a.Value;                            ";
            Type dummy = typeof (FFITarget.DummyDispose);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
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
            string code = @"                            a = TestDisposeDerived.CreateDerived();                            neglect = AClass._Dispose();                            val = AClass.Value;                            ";
            Type dummy = typeof (FFITarget.TestDisposeDerived);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "val", ExpectedValue = null, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }
        /// <summary>
        /// This is to test import of multiple dlls.
        /// </summary>

        [Test]
        public void TestMultipleImport()
        {
            String code =
            @"               import(""DSCoreNodes.dll"");               dummy = DerivedDummy.DerivedDummy();               arr = 1..Math.Factorial(5);               sum = dummy.SumAll(arr);            ";
            Type dummy = typeof (FFITarget.DerivedDummy);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "sum", ExpectedValue = 7260.0, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        /// <summary>
        /// This is to test import of multiple dlls.
        /// </summary>

        [Test]
        public void TestImportSameModuleMoreThanOnce()
        {
            String code =
            @"               import(""DSCoreNodes.dll"");               import(""DSCoreNodes.dll"");               dummy = DerivedDummy.DerivedDummy();               arr = 1..Math.Factorial(5);               sum = dummy.SumAll(arr);            ";
            Type dummy = typeof (FFITarget.DerivedDummy);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            Type dummy2 =  typeof (FFITarget.DerivedDummy);
            Type derived1 =  typeof (FFITarget.Derived1);
            Type testdispose =  typeof (FFITarget.TestDispose);
            Type dummydispose = typeof (FFITarget.DummyDispose);
            code = string.Format("import(\"{0}\");\r\nimport(\"{1}\");\r\nimport(\"{2}\");\r\nimport(\"{3}\");\r\n{4}",
                dummy2.AssemblyQualifiedName, derived1.AssemblyQualifiedName, testdispose.AssemblyQualifiedName, dummydispose.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "sum", ExpectedValue = 7260.0, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void geometryFunction()
        {
            String code =
            @"                import(""FFITarget.dll"");                              pt1=DummyPoint.ByCoordinates(1,1,1);a11;                [Associative]                {                    def foo : DummyPoint( a:DummyPoint)                    {                        return = a;                    }                    a = foo( pt1);                    a11={a.X,a.Y,a.Z};}            ";
            object[] c = new object[] { 1.0, 1.0, 1.0 };
            ValidationData[] data = { new ValidationData { ValueName = "a11", ExpectedValue = c, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void geometryIfElse()
        {
            String code =
            @"               import(""FFITarget.dll"");         a1;ptcoords;                [Imperative]                {                pt1=DummyPoint.ByCoordinates(1,1,1);                  a1 = 10;                  if( a1>=10 )                 {                pt1=DummyPoint.ByCoordinates(2,2,2);                ptcoords={pt1.X,pt1.Y,pt1.Z};                a1=1;                 }                  elseif( a1<2 )                 {                 pt1=DummyPoint.ByCoordinates(3,3,3);                 }                 else                  {                pt1=DummyPoint.ByCoordinates(4,4,4);                 }                }                    ";
            object[] d = new object[] { 2.0, 2.0, 2.0 };
            ValidationData[] data = { new ValidationData { ValueName = "a1", ExpectedValue = 1, BlockIndex = 0 } ,                                      new ValidationData { ValueName = "ptcoords", ExpectedValue = d, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestExplicitGetterAndSetters()
        {
            string code = @"                            i = 10;                            a = DummyBase.DummyBase(i);                            neglect = a.set_MyValue(15);                            val = a.get_MyValue();                            i = 5;                            ";

            Type dummy = typeof(FFITarget.DummyBase);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "val", ExpectedValue = (Int64)15, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestNullableTypes()
        {
            string code = @"                            a = DummyBase.DummyBase(10);                            v111 = a.TestNullable(null, null);                            v123 = a.TestNullable(5, null);                            v321 = a.TestNullable(null, 2);                            ";
            Type dummy = typeof(FFITarget.DummyBase);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "v111", ExpectedValue = (Int64)111, BlockIndex = 0 },                                      new ValidationData { ValueName = "v123", ExpectedValue = (Int64)123, BlockIndex = 0 },                                      new ValidationData { ValueName = "v321", ExpectedValue = (Int64)321, BlockIndex = 0 }                                    };
            Assert.IsTrue(ExecuteAndVerify(code, data) == 0); //runs without any error
        }
      

        [Test]
        [Category("Update")]
        public void SimplePropertyUpdate()
        {
            string code = @"                            i = 10;                            a = DummyBase.DummyBase(i);                            a.Value = 15;                            val = a.Value;                            i = 5;                            ";

            Type dummy = typeof (FFITarget.DummyBase);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "val", ExpectedValue = (Int64)15, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void SimplePropertyUpdateUsingSetMethod()
        {
            string code = @"                            i = 10;                            a = DummyBase.DummyBase(i);                            neglect = a.SetValue(15);                            val = a.Value;                            i = 5;                            ";
            Type dummy = typeof(FFITarget.DummyBase);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "val", ExpectedValue = (Int64)15, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        [Category("Update")]
        public void PropertyReadback()
        {
            string code =
                @"import(""FFITarget.dll"");                cls = ClassFunctionality.ClassFunctionality();                cls.IntVal = 3;                readback = cls.IntVal;";
            ValidationData[] data = { new ValidationData { ValueName = "readback", ExpectedValue = (Int64)3, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        [Category("Update")]
        public void PropertyUpdate()
        {
            string code =
                @"import(""FFITarget.dll"");                cls = ClassFunctionality.ClassFunctionality();                cls.IntVal = 3;                readback = cls.IntVal;
                cls.IntVal = 4;";
            ValidationData[] data = { new ValidationData { ValueName = "readback", ExpectedValue = (Int64)4, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void DisposeOnFFITest001()
        {
            string code = @"                            def foo : int()                            {                                a = AClass.CreateObject(2);                                return = 3;                            }                            dv = DisposeVerify.CreateObject();                            m = dv.SetValue(1);                            a = foo();                            b = dv.GetValue();                            ";
            code = string.Format("{0}\r\n{1}\r\n{2}", "import(DisposeVerify from \"FFITarget.dll\");",
                "import(AClass from \"FFITarget.dll\");", code);
            ValidationData[] data = { new ValidationData { ValueName = "m", ExpectedValue = 1, BlockIndex = 0 },                                         new ValidationData { ValueName = "a", ExpectedValue = 3, BlockIndex = 0 },                                        new ValidationData { ValueName = "b", ExpectedValue = 2, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void DisposeOnFFITest004()
        {
            string code = @"                            def foo : BClass(b : BClass)                            {                                a1 = AClass.CreateObject(9);                                a2 = { BClass.CreateObject(1), BClass.CreateObject(2), BClass.CreateObject(3), BClass.CreateObject(4) };                                    a4 = b;                                a3 = BClass.CreateObject(5);                                                                return = a3;                            }                            dv = DisposeVerify.CreateObject();                            m = dv.SetValue(1);                            a = BClass.CreateObject(-1);                            b = foo(a);                            c = dv.GetValue();                            ";
            code = string.Format("{0}\r\n{1}\r\n{2}\r\n{3}", "import(DisposeVerify from \"FFITarget.dll\");",
                "import(AClass from \"FFITarget.dll\");", "import(BClass from \"FFITarget.dll\");", code);
            ValidationData[] data = { new ValidationData { ValueName = "m", ExpectedValue = 1, BlockIndex = 0 },                                          new ValidationData { ValueName = "c", ExpectedValue = 19, BlockIndex = 0 }};
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void DisposeOnFFITest005()
        {
            string code = @"                            class Foo                            {                                a : A;                                b : B;                                                                constructor Foo(_a : A, _b : B)                                {                                    a = _a; b = _b;                                }                               }                            def foo : int()                            {                                fb = BClass.CreateObject(9);                                fa = AClass.CreateObject(8);                                ff = Foo.Foo(fa, fb);                                return = 3;                            }                                                       dv = DisposeVerify.CreateObject();                            m = dv.SetValue(1);                            a = foo();                            b = dv.GetValue();                            ";
            code = string.Format("{0}\r\n{1}\r\n{2}\r\n{3}", "import(DisposeVerify from \"FFITarget.dll\");",
                "import(AClass from \"FFITarget.dll\");", "import(BClass from \"FFITarget.dll\");", code);
            ValidationData[] data = { new ValidationData { ValueName = "a", ExpectedValue = 3, BlockIndex = 0 },                                          new ValidationData { ValueName = "b", ExpectedValue = 17, BlockIndex = 0 }};
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void DisposeOnFFITest002()
        {
            string code = @"                            def foo : AClass()                            {                                a = AClass.CreateObject(10);                                return = a;                            }                            dv = DisposeVerify.CreateObject();                            m = dv.SetValue(1);                            a = foo();                            b = dv.GetValue();                            ";
            code = string.Format("{0}\r\n{1}\r\n{2}\r\n{3}", "import(DisposeVerify from \"FFITarget.dll\");",
                "import(AClass from \"FFITarget.dll\");", "import(BClass from \"FFITarget.dll\");", code);
            ValidationData[] data = { new ValidationData { ValueName = "b", ExpectedValue = 1, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void DisposeOnFFITest003()
        {
            string code = @"                            def foo : int(a : AClass)                            {                                b = a;                                return = 1;                            }                            dv = DisposeVerify.CreateObject();                            m = dv.SetValue(2);                            a = AClass.CreateObject(3);                            b = foo(a);                            c = dv.GetValue();                            ";
            code = string.Format("{0}\r\n{1}\r\n{2}\r\n{3}", "import(DisposeVerify from \"FFITarget.dll\");",
                "import(AClass from \"FFITarget.dll\");", "import(BClass from \"FFITarget.dll\");", code);
            ValidationData[] data = { new ValidationData { ValueName = "c", ExpectedValue = 2, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void DisposeOnFFITest006()
        {
            string code = @"                            dv = DisposeVerify.CreateObject();                            m = dv.SetValue(2);                            a1 = AClass.CreateObject(3);                            a2 = AClass.CreateObject(4);                            a2 = a1;                            b = { BClass.CreateObject(1), BClass.CreateObject(2), BClass.CreateObject(3) };                            b = a1;                                v = dv.GetValue();                            ";
            code = string.Format("{0}\r\n{1}\r\n{2}\r\n{3}", "import(DisposeVerify from \"FFITarget.dll\");",
                "import(AClass from \"FFITarget.dll\");", "import(BClass from \"FFITarget.dll\");", code);
            ValidationData[] data = { new ValidationData { ValueName = "v", ExpectedValue = 10, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void DisposeOnFFITest007()
        {
            string code = @"                            class Foo                            {                                b : BClass;                                constructor Foo(_b : BClass)                                {                                    b = _b;                                }                                def foo : int(_b : BClass)                                {                                    b = _b;                                    return = 0;                                }                            }                            v1;                            v2;                            [Imperative]                            {                                dv = DisposeVerify.CreateObject();                                m = dv.SetValue(3);                                b1 = BClass.CreateObject(10);                                                            f = Foo.Foo(b1);                                m = f.foo(b1);                                b2 = BClass.CreateObject(20);                                b3 = BClass.CreateObject(30);                                f2 = Foo.Foo(b2);                                b2 = null;                                v1 = dv.GetValue();                                m = f2.foo(b3);                                v2 = dv.GetValue();                            }                            ";
            code = string.Format("{0}\r\n{1}\r\n{2}\r\n{3}", "import(DisposeVerify from \"FFITarget.dll\");",
                "import(AClass from \"FFITarget.dll\");", "import(BClass from \"FFITarget.dll\");", code);
            ValidationData[] data = { new ValidationData { ValueName = "v1", ExpectedValue = 3, BlockIndex = 0 },                                      new ValidationData { ValueName = "v2", ExpectedValue = 23, BlockIndex = 0 }};
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void DisposeOnFFITest008()
        {
            string code = @"                            class Foo                            {                                a : AClass;                                b : BClass;                                constructor Foo(_a : AClass, _b : BClass)                                {                                    a = _a; b = _b;                                }                                def foo : int(_a : AClass, _b : BClass)                                {                                    a = _a; b = _b;                                    return = 0;                                }                            }                            v1;                            v2;                            v3;                            [Imperative]                            {                                dv = DisposeVerify.CreateObject();                                m = dv.SetValue(3);                                a1 = AClass.CreateObject(3);                                b1 = BClass.CreateObject(13);                                b2 = BClass.CreateObject(14);                                b3 = BClass.CreateObject(15);                                a1 = a1;                                v1 = dv.GetValue();                                a2 = AClass.CreateObject(4);                                f = Foo.Foo(a1, b1);                                f2 = Foo.Foo(a1, b2);                                //f.a = f2.a;                                b1 = b3;                                b2 = b3;                                v2 = dv.GetValue();                                f = f2;                                v3 = dv.GetValue();                            }                            ";
            code = string.Format("{0}\r\n{1}\r\n{2}\r\n{3}", "import(DisposeVerify from \"FFITarget.dll\");",
                "import(AClass from \"FFITarget.dll\");", "import(BClass from \"FFITarget.dll\");", code);
            ValidationData[] data = { new ValidationData { ValueName = "v1", ExpectedValue = 3, BlockIndex = 0 },                                     new ValidationData { ValueName = "v2", ExpectedValue = 3, BlockIndex = 0 },                                     new ValidationData { ValueName = "v3", ExpectedValue = 16, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestReplicationAndDispose()
        {
            //Defect: DG-1464910 Sprint 22: Rev:2385-324564: Planes get disposed after querying properties on returned planes.
            string code = @"                            def foo : int(a : AClass)                            {                                return = a.Value;                            }                            dv = DisposeVerify.CreateObject();                            m = dv.SetValue(2);                            a = {AClass.CreateObject(1), AClass.CreateObject(2), AClass.CreateObject(3)};                            b = foo(a);                            c = dv.GetValue();                            ";
            code = string.Format("{0}\r\n{1}\r\n{2}", "import(DisposeVerify from \"FFITarget.dll\");",
                "import(AClass from \"FFITarget.dll\");", code);
            object[] b = new object[] { 1, 2, 3 };
            ValidationData[] data = { new ValidationData { ValueName = "c", ExpectedValue = 2, BlockIndex = 0 },                                      new ValidationData { ValueName = "b", ExpectedValue = b, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestNestedClassImport()
        {
            string code =
               @"import(NestedClass from ""FFITarget.dll"");                t = NestedClass.GetType(123);                t5 = NestedClass_Type.Type(123);                success = t5.Equals(t);                ";
            ValidationData[] data = { new ValidationData { ValueName = "success", ExpectedValue = true, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

 
        [Test]
        public void TestNamespaceImport()
        {
            string code =
                @"import(MicroFeatureTests from ""ProtoTest.dll"");";
            TestFrameWork theTest = new TestFrameWork();
            var mirror = theTest.RunScriptSource(code);
            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kMultipleSymbolFound);
            string[] classes = theTest.GetAllMatchingClasses("MicroFeatureTests");
            Assert.True(classes.Length > 1, "More than one implementation of MicroFeatureTests class expected");
        }

        [Test]
        public void TestNamespaceFullResolution01()
        {
            var mirror = thisTest.RunScriptSource(
            @"                import(""FFITarget.dll"");
                p = A.NamespaceResolutionTargetTest.NamespaceResolutionTargetTest();                x = p.Prop;            "
            );

            Assert.IsTrue((Int64)mirror.GetFirstValue("x").Payload == 0);
        }

        [Test]
        public void TestNamespaceFullResolution02()
        {
            var mirror = thisTest.RunScriptSource(
            @"                import(""FFITarget.dll"");
                p = A.NamespaceResolutionTargetTest.NamespaceResolutionTargetTest(1);                x = p.Prop;            "
            );

            Assert.IsTrue((Int64)mirror.GetFirstValue("x").Payload == 1);
        }

        [Test]
        public void TestNamespaceFullResolution03()
        {
            var mirror = thisTest.RunScriptSource(
            @"                import(""FFITarget.dll"");
                p = A.NamespaceResolutionTargetTest.Foo(1);                x = p.Prop;            "
            );

            Assert.IsTrue((Int64)mirror.GetFirstValue("x").Payload == 1);
        }

        [Test]
        public void TestNamespacePartialResolution01()
        {
            var mirror = thisTest.RunScriptSource(
            @"                import(""FFITarget.dll"");
                p = A.NamespaceResolutionTargetTest.Foo(1);                x = p.Prop;            "
            );

            Assert.IsTrue((Int64)mirror.GetFirstValue("x").Payload == 1);
        }

        [Test]
        public void TestNamespacePartialResolution02()
        {
            var mirror = thisTest.RunScriptSource(
            @"                import(""FFITarget.dll"");
                p = A.NamespaceResolutionTargetTest(1);                x = p.Prop;            "
            );

            Assert.IsTrue((Int64)mirror.GetFirstValue("x").Payload == 1);
        }

        [Test]
        public void TestNamespacePartialResolution03()
        {
            var mirror = thisTest.RunScriptSource(
            @"                import(""FFITarget.dll"");
                p = B.NamespaceResolutionTargetTest();                x = p.Prop;            "
            );

            Assert.IsTrue((Int64)mirror.GetFirstValue("x").Payload == 1);
        }

        [Test]
        [Category("Failing")]
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

            var mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("check", true);
            thisTest.Verify("Xo", 1);

            thisTest.Verify("aReadback", 1);
            thisTest.Verify("bReadback", 2);

            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kMultipleSymbolFound);
            string[] classes = thisTest.GetAllMatchingClasses("DupTargetTest");
            Assert.True(classes.Length > 1, "More than one implementation of DupTargetTest class expected");
        }

        [Test]
        [Category("Failing")]
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
            var mirror = thisTest.RunScriptSource(code, err);
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
            @"              import(""FFITarget.dll"");o = InheritenceDriver.Gen();oy = o.Y;            ";
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