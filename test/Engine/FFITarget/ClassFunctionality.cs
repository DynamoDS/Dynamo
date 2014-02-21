using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FFITarget
{
    public class ClassFunctionality
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

    }
}
