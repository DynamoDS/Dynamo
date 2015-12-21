using System;
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

    public class ProductExpiredException : Exception
    {
        public ProductExpiredException(string message, DateTime expirydate)
            : base(message)
        {
            ExpiryDate = expirydate;
        }

        public DateTime ExpiryDate { get; private set; }
    }
}
