namespace FFITarget
{
    public class ReplicationTestA
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public int A
        {
            get
            {
                return X + Y + Z;
            }
        }

        public ReplicationTestA()
        {

        }

        public ReplicationTestA(int x, int y, int z)
        {
            X = x; Y = y; Z = z;
        }

        public static int foo(int x, int y, int z)
        {
            return x + y + z;
        }

        public int bar(int x, int y, int z)
        {
            return x + y + z;
        }

        public int gety(int x, int y, int z)
        {
            return y;
        }
    }

    public class ReplicationX
    {
        public int sum(int x, int y)
        {
            return x + y;
        }

        public int foo(int x)
        {
            return x;
        }
    }

    public class ReplicationY : ReplicationX
    {
        public ReplicationY(int x, int y)
        {

        }
    }

}
