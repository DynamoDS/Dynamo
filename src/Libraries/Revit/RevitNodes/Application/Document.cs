using Autodesk.DesignScript.Runtime;
using Revit.Elements;
using Revit.Elements.Views;
using RevitServices.Persistence;

namespace Revit.Application
{
    /// <summary>
    /// A Revit Document
    /// </summary>
    public class Document
    {
        /// <summary>
        /// Internal reference to the Document
        /// </summary>
        [IsVisibleInDynamoLibrary(false)]
        public Autodesk.Revit.DB.Document InternalDocument { get; private set; }

        internal Document(Autodesk.Revit.DB.Document currentDBDocument)
        {
            InternalDocument = currentDBDocument;
        }

        /// <summary>
        /// Get the active view for the document
        /// </summary>
        public View ActiveView 
        {
            get
            {
                return (View)InternalDocument.ActiveView.ToDSType(true);
            }
        }

        /// <summary>
        /// Is the Document a Family?
        /// </summary>
        public bool IsFamilyDocument
        {
            get
            {
                return InternalDocument.IsFamilyDocument;
            }
        }

        /// <summary>
        /// Get the current document
        /// </summary>
        /// <returns></returns>
        public static Document Current
        {
            get
            {
                return new Document(DocumentManager.Instance.CurrentDBDocument);
            }
        }

    }
}
