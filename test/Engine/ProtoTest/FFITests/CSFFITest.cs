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
            core.Compilers.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Compiler(core));
            core.Compilers.Add(ProtoCore.Language.kImperative, new ProtoImperative.Compiler(core));
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
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void TestImportPointClass()
        {
            String code =
            @"import(Point from ""ProtoGeometry.dll"");x;y;z;             [Associative]              {               point = Point.ByCoordinates(1,2,3);               x = point.X;               y = point.Y;               z = point.Z;             }            ";
            ValidationData[] data = { new ValidationData { ValueName="x", ExpectedValue = 1.0, BlockIndex = 0},                                      new ValidationData { ValueName="y", ExpectedValue = 2.0, BlockIndex = 0},                                      new ValidationData { ValueName="z", ExpectedValue = 3.0, BlockIndex = 0}                                    };
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
        public void TestDictionaryMarshalling_DStoCS_CStoDS()
        {
            // Tracked by: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4035
            String code =
            @"                dummy = Dummy.Dummy();                dictionary =                 {                     dummy.CreateDictionary() => dict;                    dummy.AddData(dict, ""ABCD"", 22) => dict;                    dummy.AddData(dict, ""xyz"", 11) => dict;                    dummy.AddData(dict, ""teas"", 12) => dict;                }                sum = dummy.SumAges(dictionary);            ";
            Type dummy = typeof (FFITarget.Dummy);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "sum", ExpectedValue = 45, BlockIndex = 0 } };
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

        [Test]
        [Category("DSDefinedClass")]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void TestInheritanceAcrossLangauges_CS_DS()
        {
            string code = @"                import (Vector from ""ProtoGeometry.dll"");                class Vector2 extends Vector                {                    public constructor Vector2(x : double, y : double, z : double) : base ByCoordinates(x, y, z)                    {}                }                                vec2 = Vector2.Vector2(1,1,1);                x = vec2.GetLength();                ";
            ValidationData[] data = { new ValidationData { ValueName = "x", ExpectedValue = Math.Sqrt(3.0), BlockIndex = 0 } };
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () => ExecuteAndVerify(code, data));
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
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void TestPropertyAccessor()
        {
            String code =
            @"               import(""ProtoGeometry.dll"");               pt = Point.ByCoordinates(1,2,3);               a = pt.X;            ";
            double aa = 1;
            ValidationData[] data = { new ValidationData { ValueName = "a", ExpectedValue = aa, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void TestAssignmentSingleton()
        {
            String code =
            @"               import(""ProtoGeometry.dll"");               pt = Point.ByCoordinates(1,2,3);               a = { pt.X};            ";
            object[] aa = new object[] { 1.0 };
            ValidationData[] data = { new ValidationData { ValueName = "a", ExpectedValue = aa, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void TestAssignmentAsArray()
        {
            String code =
            @"               import(""ProtoGeometry.dll"");               pt = Point.ByCoordinates(1,2,3);               a = { pt.X, pt.X};               def test (pt : Point)               {                  return = { pt.X, pt.X};               }               c = test(pt);            ";
            object[] aa = new object[] { 1.0, 1.0 };
            ValidationData[] data = { new ValidationData { ValueName = "a", ExpectedValue = aa, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void TestReturnFromFunctionSingle()
        {
            String code =
            @"               import(""ProtoGeometry.dll"");            pt = Point.ByCoordinates(1,2,3);            a = { pt.X, pt.X};            def test (pt : Point)            {            return = {  pt.X};            }            b = test(pt);            ";
            var b = new object[] { 1.0 };
            ValidationData[] data = { new ValidationData { ValueName = "b", ExpectedValue = b, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void Defect_1462300()
        {
            String code =
            @"               import(""ProtoGeometry.dll"");            pt = Point.ByCoordinates(1,2,3);            def test (pt : Point)            {            return = {  pt.X,pt.Y};            }            b = test(pt);            ";
            object[] b = new object[] { 1.0, 2.0 };
            ValidationData[] data = { new ValidationData { ValueName = "b", ExpectedValue = b, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        [Category("DSDefinedClass")]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void geometryinClass()
        {
            String code =
            @"               import(""ProtoGeometry.dll"");               pt1=Point.ByCoordinates(1.0,1.0,1.0);               class baseClass               {                     val1 : Point  ;                    a:int;                    constructor baseClass()                    {                        a=1;                        val1=Point.ByCoordinates(1,1,1);                    }                           }                instance1= baseClass.baseClass();                a2=instance1.a;                b2=instance1.val1;                c2={b2.X,b2.Y,b2.Z};            ";
            object[] c = new object[] { 1.0, 1.0, 1.0 };
            ValidationData[] data = { new ValidationData { ValueName = "a2", ExpectedValue = 1, BlockIndex = 0 }, new ValidationData { ValueName = "c2", ExpectedValue = c, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void geometryArrayAssignment()
        {
            String code =
            @"               import(""ProtoGeometry.dll"");                             pt1=Point.ByCoordinates(1,1,1);               pt2=Point.ByCoordinates(2,2,2);               pt3=Point.ByCoordinates(3,3,3);               pt4=Point.ByCoordinates(4,4,4);               pt5=Point.ByCoordinates(5,5,5);               pt6=Point.ByCoordinates(6,6,6);a11;a12;b11;b12;               [Imperative]               {                    a    = { {pt1,pt2}, {pt3,pt4} };                    a11  = {a[0][0].X,a[0][0].Y,a[0][0].Z};                    a[1] = {pt5,pt6};                    a12  = {a[1][1].X,a[1][1].Y,a[1][1].Z};                    d    = a[0];                    b    = { pt1, pt2 };                    b11  = {b[0].X,b[0].Y,b[0].Z};                    b[0] = {pt3,pt4,pt5};                    b12  = {b[0][0].X,b[0][0].Y,b[0][0].Z};                    e    = b[0];                    e12  = {e[0].X,e[0].Y,e[0].Z};               }            ";
            object[] c = new object[] { 1.0, 1.0, 1.0 };
            object[] d = new object[] { 4.0, 4.0, 4.0 };
            object[] e = new object[] { 3.0, 3.0, 3.0 };
            object[] f = new object[] { 6.0, 6.0, 6.0 };
            ValidationData[] data = { new ValidationData { ValueName = "a11", ExpectedValue = c, BlockIndex = 0 },                                      new ValidationData { ValueName = "a12", ExpectedValue = f, BlockIndex = 0 },                                      new ValidationData { ValueName = "b11", ExpectedValue = c, BlockIndex = 0 },                                      new ValidationData { ValueName = "b12", ExpectedValue = e, BlockIndex = 0 },                                      new ValidationData { ValueName = "b12", ExpectedValue = e, BlockIndex = 0 }};
            ExecuteAndVerify(code, data);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void geometryForLoop()
        {
            String code =
            @"                import(""ProtoGeometry.dll"");                                pt1=Point.ByCoordinates(1,1,1);                pt2=Point.ByCoordinates(2,2,2);                pt3=Point.ByCoordinates(3,3,3);a11;a12;                [Imperative]                {                    a = { pt1, pt2, pt3 };                    a11={a[0].X,a[0].Y,a[0].Z};                    x = 0;                     for (y in a )                    {                                        a[x]=pt3;                    a12={a[0].X,a[0].Y,a[0].Z};                    x=x+1;                    }                                     }             ";
            object[] c = new object[] { 1.0, 1.0, 1.0 };
            object[] e = new object[] { 3.0, 3.0, 3.0 };
            ValidationData[] data = { new ValidationData { ValueName = "a11", ExpectedValue = c, BlockIndex = 0 },                                      new ValidationData { ValueName = "a12", ExpectedValue = e, BlockIndex = 0 }                                    };
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
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void geometryInlineConditional()
        {
            String code =
            @"               import(""ProtoGeometry.dll"");               WCS = CoordinateSystem.Identity();                pt1=Point.ByCoordinates(1,1,1);                pt2=Point.ByCoordinates(2,2,2);s11;l11;                [Imperative]                {                    def fo1 : int(a1 : int)                    {                        return = a1 * a1;                    }                    a	=	10;				                    b	=	20;                                    smallest   =   a	<   b   ?   pt1	:	pt2;                    largest	=   a	>   b   ?   pt1	:	pt2;                    s11={smallest.X,smallest.Y,smallest.Z};                    l11={largest.X,largest.Y,largest.Z};                 }                    ";
            object[] c = new object[] { 1.0, 1.0, 1.0 };
            object[] d = new object[] { 2.0, 2.0, 2.0 };
            ValidationData[] data = { new ValidationData { ValueName = "s11", ExpectedValue = c, BlockIndex = 0 } ,                                      new ValidationData { ValueName = "l11", ExpectedValue = d, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void geometryRangeExpression()
        {
            String code =
            @"               import(""ProtoGeometry.dll"");                                     a=1;a12;                    [Imperative]                    {                        a = 1/2..1/4..-1/4;                    }                    [Associative]                    {                    pt=Point.ByCoordinates(a[0],0,0);                    a12={pt.X,pt.Y,pt.Z};                    }                    ";
            object[] c = new object[] { 0.5, 0.0, 0.0 };
            ValidationData[] data = { new ValidationData { ValueName = "a12", ExpectedValue = c, BlockIndex = 0 }                                       };
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
        /*  
          [Test]
          public void geometryUpdateAcrossMultipleLanguageBlocks()
          {
              String code =
              @"
                 import(""ProtoGeometry.dll"");
                 
                    pt=Point.ByCoordinates(0,0,0);
                    pt11={pt.X,pt.Y,pt.Z};
                 
                    [Associative]
                    {
                          pt=Point.ByCoordinates(1,1,1);
                          pt12={pt.X,pt.Y,pt.Z};
                       
                          [Imperative]
                          {
                              pt=Point.ByCoordinates(2,2,2);
                              pt13={pt.X,pt.Y,pt.Z};
                          }
                          pt=Point.ByCoordinates(3,3,3);
                          pt14={pt.X,pt.Y,pt.Z};
                     }
        
              ";
              object[] a = new object[] { 0.0, 0.0, 0.0 };
              object[] b = new object[] { 1.0, 1.0, 1.0 };
              object[] c = new object[] { 2.0, 2.0, 2.0 };
              object[] d = new object[] { 3.0, 3.0, 3.0 };
              ValidationData[] data = {   new ValidationData() { ValueName   = "p11", ExpectedValue = a, BlockIndex = 0 },
                                          new ValidationData() { ValueName = "p12", ExpectedValue = b, BlockIndex = 0 },
                                          new ValidationData() { ValueName = "p13", ExpectedValue = c, BlockIndex = 0 },
                                          new ValidationData() { ValueName = "p14", ExpectedValue = d, BlockIndex = 0 }
                                        };
              ExecuteAndVerify(code, data);
          }*/

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
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
                                a2 = { BClass.CreateObject(1), BClass.CreateObject(2), BClass.CreateObject(3), BClass.CreateObject(4) };    
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
        [Category("DSDefinedClass")]
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
                                fa = BClass.CreateObject(8);
                                ff = Foo.Foo(fa, fb);
                                return = 3;
                            }
                           
                            dv = DisposeVerify.CreateObject();
                            m = dv.SetValue(1);
                            a = foo();
                            __GC();
                            b = dv.GetValue();
                            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 3);
            thisTest.Verify("b", 18);
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
            // For mark and sweep, the order of object dispose is not defined,
            // so we are not sure if AClass objects or BClass objects will be
            // disposed firstly, hence we can't verify the expected value.
        }

        [Test]
        [Category("DSDefinedClass")]
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
                            __GC();
                            v3 = dv.GetValue();
                            ";
            thisTest.RunScriptSource(code);
                // For mark and sweep, it is straight, we only need to focus 
                // on created objects. So the value = 3 + 10 + 20 + 30 = 63.
                thisTest.Verify("v3", 63);
        }

        [Test]
        [Category("DSDefinedClass")]
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
                            __GC();
                            v4 = dv.GetValue();
                            ";

            thisTest.RunScriptSource(code);
            {
                // For mark and sweep, the last object is a2, se the expected
                // value = 4. But it is very fragile because the order of
                // disposing is not defined. 
                thisTest.Verify("v4", 4);
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