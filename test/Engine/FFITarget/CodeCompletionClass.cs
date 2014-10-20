using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FFITarget
{
    /// <summary>
    /// Tests for code completion in codeblock node
    /// </summary>
    public class CodeCompletionClass : IDisposable
    {

        private int intVal;
        public int IntVal 
        {
            get { return intVal; }
            set 
            { 
                this.intVal = value;
                this.classProperty = new ValueContainer(intVal);
            }
        }

        public static int StaticProp { get; set; }

        public CodeCompletionClass()
        {
                
        }

        public CodeCompletionClass(int intVal)
        {
            this.IntVal = intVal;
        }

        public CodeCompletionClass(int i1, int i2, int i3)
        {
            IntVal = i1 + i2 + i3;
        }

        public bool IsEqualTo(ClassFunctionality cf)
        {
            return this.IntVal == cf.IntVal;
        }

        public IEnumerable<ValueContainer> AddWithValueContainer(ValueContainer valueContainer)
        {
            return null;
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

}
