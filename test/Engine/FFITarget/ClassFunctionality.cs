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


        public int IntVal { get; set; }

        public ClassFunctionality()
        {
                
        }

        public ClassFunctionality(int intVal)
        {
            this.IntVal = intVal;
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
}
