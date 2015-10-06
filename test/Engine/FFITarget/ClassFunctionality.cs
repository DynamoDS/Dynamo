using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FFITarget
{
    /// <summary>
    /// Tests for basic functional testing of FFI implementations
    /// </summary>
    public class ClassFunctionality : IDisposable
    {

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

        static public int StaticProp { get; set; }

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
            return  new ValueContainer(SomeValue * SomeValue);
        }

        public int SomeValue { get; set; }
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
}
