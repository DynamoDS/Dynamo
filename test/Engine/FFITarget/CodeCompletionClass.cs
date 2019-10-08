using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Autodesk.DesignScript.Runtime;

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

    public class SampleClassA
    {
    }

    [IsVisibleInDynamoLibrary(false)]
    public class SampleClassB
    {
    }

    public class SampleClassC
    {
    }

    namespace FirstNamespace
    {
        public class ClassWithNameConflict
        {
            public string PropertyA { get; set; }
            public string PropertyB { get; set; }
            public string PropertyC { get; set; }
        }

        public class AnotherClassWithNameConflict
        {
            public static string PropertyA { get; set; }
            public static string PropertyB { get; set; }
            public static string PropertyC { get; set; }
        }
    }

    namespace SecondNamespace
    {
        public class ClassWithNameConflict
        {
            public string PropertyD { get; set; }
            public string PropertyE { get; set; }
            public string PropertyF { get; set; }
        }

        [IsVisibleInDynamoLibrary(false)]
        public class AnotherClassWithNameConflict
        {
            public static string PropertyD { get; set; }
            public static string PropertyE { get; set; }
            public static string PropertyF { get; set; }
        }
    }

    [IsVisibleInDynamoLibrary(false)]
    [Serializable]
    public class TraceableId : ISerializable
    {
        public int IntID { get; set; }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("intID", IntID, typeof(int));
        }
    }

    [IsVisibleInDynamoLibrary(false)]
    public enum Days
    {
        Sunday,
        Monday,
        Tuesday,
        Wednesday,
        Thursday,
        Friday,
        Saturday
    }
}
