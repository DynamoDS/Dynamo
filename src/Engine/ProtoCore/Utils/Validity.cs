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

        private static DateTime? mAppStartupTime = null;
        public static void AssertExpiry()
        {
            //Expires on 30th September 2013
            /*
            DateTime expires = new DateTime(2013, 9, 30);
            if(!mAppStartupTime.HasValue)
                mAppStartupTime = DateTime.Now;
            if (mAppStartupTime.Value >= expires)
                throw new ProductExpiredException("DesignScript Technology Preview has expired. Visit http://labs.autodesk.com for more information.", expires);
            */
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
