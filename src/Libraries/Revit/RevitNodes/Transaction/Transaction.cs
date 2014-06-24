using RevitServices.Persistence;
using RevitServices.Transactions;

namespace Revit.Transaction
{
    /// <summary>
    ///     A Revit Transaction.
    /// </summary>
    public class Transaction
    {
        private Transaction() { }

        /// <summary>
        /// Start a transaction if neccesssary, returning
        /// whatever was passed in.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static object Start(object input)
        {
            TransactionManager.Instance.EnsureInTransaction(DocumentManager.Instance.CurrentDBDocument);
            return input;
        }

        /// <summary>
        ///     Ends the current Dynamo transaction, returning whatever was
        ///     passed in.
        /// </summary>
        /// <param name="input">An object.</param>
        public static object End(object input)
        {
            TransactionManager.Instance.ForceCloseTransaction();
            return input;
        }
    }
}
