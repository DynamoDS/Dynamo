namespace FFITarget
{
    public class TestPersistentNodeValues
    {
        private int sum = 0;
        public int Sum
        {
            get
            {
                return sum;
            }
        }

        public TestPersistentNodeValues Add(int a, int b)
        {
            sum += a + b;
            return this;
        }
    }
}
