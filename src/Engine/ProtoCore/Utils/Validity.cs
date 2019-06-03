namespace ProtoCore.Utils
{
    public class Validity
    {
        public static void Assert(bool cond)
        {
            if (!cond)
                throw new Exceptions.CompilerInternalException("");
        }

        public static void Assert(bool cond, string message)
        {
            if (!cond)
                throw new Exceptions.CompilerInternalException(message);
        }
    }
}
