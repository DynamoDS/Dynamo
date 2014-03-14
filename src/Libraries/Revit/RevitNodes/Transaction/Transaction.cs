using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RevitServices.Transactions;

namespace Revit.Transaction
{
    /// <summary>
    ///     A Revit Transaction.
    /// </summary>
    public class Transaction
    {
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
