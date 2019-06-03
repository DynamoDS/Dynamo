namespace FFITarget
{
    public class TestUpdateCount
    {

        public static void Reset()
        {
            UpdateCount = 0;
        }

        public static int UpdateCount { get; private set; }
        public int Val { get; private set; }

        public static TestUpdateCount Ctor(int x, int y)
        {
            TestUpdateCount item = new TestUpdateCount();
            item.Val = x + y;
            UpdateCount++;

            return item;
        }

    }
}
