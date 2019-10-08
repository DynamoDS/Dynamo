namespace FFITarget
{
    public class TestThisOverload
    {
        private int _x;
        public TestThisOverload(int val)
        {
            _x = val;
        }

        // When this class is imported, an overloaded static function
        // static Add2:int(testThisOverload: TestThisOverload, x: int) will be
        // generated automatically.
        public int Add(int x)
        {
            return _x + x;
        }

        public int Mul(int x)
        {
            return _x * x;
        }

        public static int Mul(TestThisOverload thisPtr, int x)
        {
            return 2 * thisPtr.Mul(x);
        }
    }
}
