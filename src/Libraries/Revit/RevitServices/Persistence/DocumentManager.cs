using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitServices.Elements;
using RevitServices.Threading;
using RevitServices.Transactions;

namespace RevitServices.Persistence
{

    /// <summary>
    /// Singleton class to manage Revit document resources
    /// </summary>
    public class DocumentManager
    {
        public static event Action<string> OnLogError;

        internal static void LogError(string obj)
        {
            var handler = OnLogError;
            if (handler != null)
                handler(obj);
        }

        internal static void LogError(Exception exception)
        {
            var handler = OnLogError;
            if (handler != null)
                handler(exception.Message);
        }

        private static DocumentManager instance;
        private static readonly Object mutex = new Object();

        public static DocumentManager Instance
        {
            get
            {
                lock (mutex)
                {
                    return instance ?? (instance = new DocumentManager());
                }
            }
        }

        private DocumentManager() { }

        /// <summary>
        /// Determine if Element exists in the current document
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Obsolete]
        public bool ElementExistsInDocument(ElementId id)
        {
            Element e;
            return CurrentDBDocument.TryGetElement(id, out e);
        }


        /// <summary>
        /// Determine if Element exists in the current document
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool ElementExistsInDocument(ElementUUID id)
        {
            Element e;
            return CurrentDBDocument.TryGetElement(id.UUID, out e);
        }


        /// <summary>
        /// Delete an element from the current document given the ElementId
        /// </summary>
        /// <param name="element">The id of the element to delete</param>
        [Obsolete]
        public void DeleteElement(ElementId element)
        {
            TransactionManager.Instance.EnsureInTransaction(CurrentDBDocument);

            try
            {
                CurrentDBDocument.Delete(element);
            }
            catch (Exception e)
            {
                LogError(e);
            }

            TransactionManager.Instance.TransactionTaskDone();
        }

        /// <summary>
        /// Delete an element from the current document given the ElementUUID
        /// </summary>
        /// <param name="element">The UUID of the element to delete</param>
        public void DeleteElement(ElementUUID element)
        {
            TransactionManager.Instance.EnsureInTransaction(CurrentDBDocument);

            CurrentDBDocument.Delete(ElementBinder.GetIdForUUID(CurrentDBDocument, element));

            TransactionManager.Instance.TransactionTaskDone();
        }


        /// <summary>
        /// Obtain all elements of a given type from the current document
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IEnumerable<T> ElementsOfType<T>() where T : Element
        {
            var fec = new FilteredElementCollector(CurrentDBDocument);
            return fec.OfClass(typeof(T)).Cast<T>();
        }

        public IEnumerable<Element> ElementsOfCategory(BuiltInCategory category) 
        {
            var fec = new FilteredElementCollector(CurrentDBDocument);
            return fec.OfCategory(category);
        }

        /// <summary>
        /// Provide source of the currently active document
        /// Dynamo is reponsible for updating this before use
        /// </summary>
        public Document CurrentDBDocument {
            get
            {
                var c = CurrentUIDocument;
                return c == null ? null : c.Document;
            }
        }

        /// <summary>
        /// Provides the currently active UI document.
        /// </summary>
        public UIDocument CurrentUIDocument {
            get
            {
                var c = CurrentUIApplication;
                return c == null ? null : c.ActiveUIDocument;
            }
        }

        /// <summary>
        /// Provides the current UIApplication
        /// </summary>
        public UIApplication CurrentUIApplication { get; set; }

        /// <summary>
        /// Trigger a document regeneration in the idle context or without
        /// depending on the state of the transaction manager.
        /// </summary>
        public static void Regenerate()
        {

            if (TransactionManager.Instance.DoAssertInIdleThread)
            {

                IdlePromise.ExecuteOnIdleSync(() =>
                 {
                     TransactionManager.Instance.EnsureInTransaction(
                                  DocumentManager.Instance.CurrentDBDocument);
                     Instance.CurrentDBDocument.Regenerate();
                 }
                    );
            }
            else
            {
                Instance.CurrentDBDocument.Regenerate();
            }
        }
    }
}
