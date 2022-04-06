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
            {
                throw new Exceptions.CompilerInternalException(message);
            }
        }

        // Will throw a compiler exception if the boolean "cond" is true
        // The exception will containt a formatted string (i.e string.Format(format, items))
        internal static void Assert(bool cond, string format, params object[] items)
        {
            if (!cond)
            {
                throw new Exceptions.CompilerInternalException(string.Format(format, items));
            }
        }
    }
}
