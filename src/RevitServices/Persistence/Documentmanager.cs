using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using RevitServices.Transactions;

namespace RevitServices.Persistence
{

    /// <summary>
    /// Singleton class to manage Revit document resources
    /// </summary>
    public class DocumentManager
    {

        private static DocumentManager instance = null;
        private static Object mutex = new Object();

        public static DocumentManager GetInstance()
        {
            lock (mutex)
            {
                if (instance == null)
                    instance = new DocumentManager();

                return instance;
            }
        }

        private DocumentManager()
        {
                
        }

        /// <summary>
        /// Delete an element from the current document given the ElementId
        /// </summary>
        /// <param name="element">The id of the element to delete</param>
        public void DeleteElement(ElementId element)
        {
            TransactionManager.GetInstance().EnsureInTransaction(this.CurrentDBDocument);

            this.CurrentDBDocument.Delete(element);

            TransactionManager.GetInstance().TransactionTaskDone();
        }

        /// <summary>
        /// Provide source of the currently active document
        /// Dynamo is reponsible for updating this before use
        /// </summary>
        public Document CurrentDBDocument { get; set; }

    }
}
