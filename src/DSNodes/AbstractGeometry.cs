using System;
using Autodesk.Revit.DB;
using RevitServices.Persistence;
using RevitServices.Transactions;

namespace DSRevitNodes
{
    /// <summary>
    /// Superclass of all geometry
    /// </summary>
    public abstract class AbstractGeometry : IDisposable
    {
        public static Document Document
        {
            get { return DocumentManager.GetInstance().CurrentDBDocument; }
        }

        protected ElementId InternalID;

        /// <summary>
        /// Default implementation of dispose that removes the element from the
        /// document
        /// </summary>
        public virtual void Dispose()
        {
            var transManager = new TransactionManager();
            var transaction = transManager.StartTransaction(Document);

            Document.Delete(InternalID);

            transaction.CommitTransaction();
        }
    }
}