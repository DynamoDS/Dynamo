namespace FFITarget
{
    public class TestOverloadA 
    {
        public int execute(TestOverloadA a)
        {
            return 1;
        }

        public int unique(TestOverloadA a)
        {
            return 4;
        }

        public int foo(double x)
        {
            return 100;
        }
    }

    public class TestOverloadB : TestOverloadA
    {
        public int execute(TestOverloadB[] bs)
        {
            return 3;
        }

        public int foo(object x)
        {
            return 200;
        }
    }

    public class TestOverloadC: TestOverloadB { }
}
