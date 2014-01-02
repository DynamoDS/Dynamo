using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitServices.Elements;
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
        /// Determine if Element exists in the current document
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool ElementExistsInDocument(ElementId id)
        {
            Element e = null;
            return this.CurrentDBDocument.TryGetElement<Element>(id, out e);
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
        /// Obtain all elements of a given type from the current document
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IEnumerable<T> ElementsOfType<T>() where T : Autodesk.Revit.DB.Element
        {
            var fec = new Autodesk.Revit.DB.FilteredElementCollector(CurrentDBDocument);
            return fec.OfClass(typeof(T)).Cast<T>();
        }

        /// <summary>
        /// Provide source of the currently active document
        /// Dynamo is reponsible for updating this before use
        /// </summary>
        public Document CurrentDBDocument { get; set; }

        /// <summary>
        /// Provides the currently active UI document.
        /// </summary>
        public UIDocument CurrentUIDocument { get; set; }

        /// <summary>
        /// Provides the current UIApplication
        /// </summary>
        public UIApplication CurrentUIApplication { get; set; }

        /// <summary>
        /// A method to clear some elements from the CurrentDBDocument.  This is intended
        /// only as a temporary fix until trace properly handles model cleanup
        /// </summary>
        public void ClearCurrentDocument()
        {
            TransactionManager.GetInstance().EnsureInTransaction(this.CurrentDBDocument);

            var collector = new FilteredElementCollector(CurrentDBDocument);

            var filter = new LogicalOrFilter(new List<ElementFilter>()
            {
                new ElementCategoryFilter(BuiltInCategory.OST_ReferencePoints),
                new ElementCategoryFilter(BuiltInCategory.OST_AdaptivePoints),
                new ElementCategoryFilter(BuiltInCategory.OST_DividedPath),
                new ElementCategoryFilter(BuiltInCategory.OST_DividedSurface),

                new ElementClassFilter(typeof (FamilyInstance)),
                new ElementClassFilter(typeof (CurveElement))
            });

            collector.WherePasses(filter).ToList().ForEach(x=> CurrentDBDocument.Delete(x.Id));

            TransactionManager.GetInstance().TransactionTaskDone();
        }
    }
}
