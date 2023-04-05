using System;

namespace FFITarget
{
    /// <summary>
    /// Tests for basic functional testing of FFI implementations
    /// </summary>
    public class ClassFunctionality : IDisposable
    {
#if NETFRAMEWORK
        public static Microsoft.Office.Interop.Excel.XlPivotLineType GetExcelInteropType()
        {
            return Microsoft.Office.Interop.Excel.XlPivotLineType.xlPivotLineBlank;
        }

        public static bool TestExcelInteropType(Microsoft.Office.Interop.Excel.XlPivotLineType arg1)
        {
            if (arg1 == Microsoft.Office.Interop.Excel.XlPivotLineType.xlPivotLineBlank)
                return true;

            return false;
        }
#endif
        private int intVal;
        public int IntVal {
            get { return intVal; }
            set {
                this.intVal = value;
                this.classProperty = new ValueContainer(intVal);
            }
        }

        public ClassFunctionality()
        {

        }

        public ClassFunctionality(int intVal)
        {
            this.IntVal = intVal;
        }

        public ClassFunctionality(int i1, int i2, int i3)
        {
            IntVal = i1 + i2 + i3;
        }

        public void Set(int intVal)
        {
            this.IntVal = intVal;
        }

        public int SetAndReturn(int intVal)
        {
            return this.IntVal = intVal;
        }

        public bool IsEqualTo(ClassFunctionality cf)
        {
            return this.IntVal == cf.IntVal;
        }

        public static int StaticProp { get; set; }

        public static ClassFunctionality Instance
        {
            get { return new ClassFunctionality(2349); }
        }

        public static int get_StaticProperty
        {
            get { return 99; }
        }

        public int get_Property { get { return IntVal; } }

        public static int get_StaticMethod()
        {
            return get_StaticProperty;
        }

        public int get_Method()
        {
            return get_Property;
        }

        public int AddWithValueContainer(ValueContainer valueContainer)
        {
            return IntVal + valueContainer.SomeValue;
        }

        private ValueContainer classProperty;

        public ValueContainer ClassProperty
        {
            get { return classProperty; }
        }


        public int OverloadedAdd(ClassFunctionality cf)
        {
            return this.IntVal + cf.IntVal;
        }

        public int OverloadedAdd(int i)
        {
            return this.IntVal + i;
        }

        public static int StaticFunction()
        {
            return StaticProp;
        }

        public void Dispose()
        {
            StaticProp++;
        }


    }

    public class ValueContainer
    {
        public ValueContainer(int value)
        {
            this.SomeValue = value;
        }

        public ValueContainer Square()
        {
            return new ValueContainer(SomeValue * SomeValue);
        }

        public int SomeValue { get; set; }

        public static int SomeStaticProperty { get { return 123; } }
    }


    /// <summary>
    /// A class that contains the same IntVal member as ClassFunctionality
    /// </summary>
    public class ClassFunctionalityMirror : IDisposable
    {
        private int intVal;
        public int IntVal
        {
            get { return intVal; }
            set
            {
                this.intVal = value;
            }
        }

        public ClassFunctionalityMirror()
        {

        }

        public ClassFunctionalityMirror(int intVal)
        {
            this.IntVal = intVal;
        }

        public void Dispose()
        {
        }
    }

    public class ClassWithExceptionToString{

        public static ClassWithExceptionToString Construct()
        {
            return new ClassWithExceptionToString();
        }
        public override string ToString()
        {
            throw new NotImplementedException();
        }
    }
}
