namespace FFITarget
{
    public class ClassWithRefParams
    {
        public ClassWithRefParams() {}

        public static int MethodWithRefParameter(int a, ref ValueReference vc)
        {
            vc.SomeValue = a;
            return vc.Square().SomeValue;
        }

        public static int MethodWithOutParameter(int a, out ValueReference vc)
        {
            vc = new ValueReference(a);
            return vc.SomeValue;
        }

        public static int MethodWithRefOutParameters(int a, ref ValueReference vc1, out ValueReference vc2)
        {
            vc1.SomeValue = a;
            vc2 = new ValueReference(a);
            return vc1.SomeValue + vc2.SomeValue;
        }
    
    }

    public class ValueReference
    {
        public ValueReference(int value)
        {
            this.SomeValue = value;
        }

        public ValueReference Square()
        {
            return  new ValueReference(SomeValue * SomeValue);
        }

        public int SomeValue { get; set; }
    }

}
