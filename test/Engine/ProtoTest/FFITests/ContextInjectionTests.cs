using FFITarget;
using ProtoTest.TD;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using ProtoTestFx.TD;
namespace ProtoFFITests
{
    class InternalContext
    {
        public InternalContext(int value)
        {
            Value = value;
        }
        public int Value { get; set; }
    }
    public class ContextInjectionTests : FFITestSetup
    {
        public bool ValidateInternalContext(Object obj, int value)
        {
            InternalContext cntx = obj as InternalContext;
            if (null != cntx && cntx.Value == value)
                return true;
            return false;
        }
        public object GetInternalContext(int value)
        {
            return new InternalContext(value);
        }

        [Test]
        public void TestObjectMarshaling()
        {
            String code =
            @"               test = ContextInjectionTests.ContextInjectionTests();               value = 5234;               data = test.GetInternalContext(value);               success = test.ValidateInternalContext(data, value);            ";
            ValidationData[] data = { new ValidationData { ValueName = "success", ExpectedValue = true, BlockIndex = 0 } };
            Type dummy = Type.GetType("ProtoFFITests.ContextInjectionTests");
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestContextInjectDummy()
        {
            //Define the context
            Dictionary<string, object> context = new Dictionary<string, object>();
            context.Add("dummy", new Dummy());
            String code =
            @"                             success = dummy.CallMethod();            ";
            ValidationData[] data = { new ValidationData { ValueName = "success", ExpectedValue = true, BlockIndex = 0 } };
            //Inject the context and execute code.
            ExecuteAndVerify(code, data, context);
        }

        [Test]
        public void TestContextInjectInternalClass()
        {
            //Define the context
            Dictionary<string, object> context = new Dictionary<string, object>();
            int value = 10;
            context.Add("test", new ContextInjectionTests());
            context.Add("data", new InternalContext(value));
            context.Add("value", value);
            String code =
            @"                             success = test.ValidateInternalContext(data, value);            ";
            ValidationData[] data = { new ValidationData { ValueName = "success", ExpectedValue = true, BlockIndex = 0 } };
            //Inject the context and execute code.
            ExecuteAndVerify(code, data, context);
        }

        [Test]
        public void TestContextInjectLiteralValues()
        {
            //Define the context
            Dictionary<string, object> context = new Dictionary<string, object>();
            context.Add("x", 1);
            context.Add("y", 1.5);
            context.Add("z", 0.5);
            object[] arr = new object[3];
            context.Values.CopyTo(arr, 0);
            String code =
            @"                          value = x + y + z;               arr = {x,y,z};               success = (value==3);            ";
            ValidationData[] data = { new ValidationData { ValueName = "success", ExpectedValue = true, BlockIndex = 0 },                                      new ValidationData { ValueName = "arr", ExpectedValue = arr, BlockIndex = 0 }};
            //Inject the context and execute code.
            ExecuteAndVerify(code, data, context);
        }

        [Test]
        public void TestContextInjectLiteralArray()
        {
            //Define the context
            Dictionary<string, object> context = new Dictionary<string, object>();
            object[] arr = new object[] { 1, 2, 3 };
            context.Add("arr", arr);
            String code =
            @"                       ";
            ValidationData[] data = { new ValidationData { ValueName = "arr", ExpectedValue = arr, BlockIndex = 0 } };
            //Inject the context and execute code.
            ExecuteAndVerify(code, data, context);
        }

        [Test]
        public void TestImportDataMethod()
        {
            //Define the context
            Dictionary<string, object> context = new Dictionary<string, object>();
            context.Add("x", 1);
            context.Add("y", 1.5);
            context.Add("z", 0.5);
            object[] arr = new object[3];
            context.Values.CopyTo(arr, 0);
            String code =
            @"                          arr = ImportData(""ProtoCoreDataProvider"", {""name1"", ""x"", ""name2"", ""y"", ""name3"", ""z""});            ";
            ValidationData[] data = { new ValidationData { ValueName = "arr", ExpectedValue = arr, BlockIndex = 0 } };
            //Inject the context and execute code.
            ExecuteAndVerify(code, data, context);
        }
    }
}
