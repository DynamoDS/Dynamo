using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DSRevitNodes.Elements;
using RevitServices.Persistence;

namespace DSRevitNodes.Application
{
    /// <summary>
    /// A Revit Document
    /// </summary>
    public class DSDocument
    {
        /// <summary>
        /// Internal reference to the Document
        /// </summary>
        internal Autodesk.Revit.DB.Document Document
        {
            get
            {
                return DocumentManager.GetInstance().CurrentDBDocument;
            }
        }

        /// <summary>
        /// Get the active view for the document
        /// </summary>
        public AbstractView ActiveView 
        {
            get
            {
                return (AbstractView) ElementWrappingExtensions.ToDSType(Document.ActiveView, true);
            }
        }

        /// <summary>
        /// Is the Document a Family?
        /// </summary>
        public bool IsFamilyDocument
        {
            get
            {
                return Document.IsFamilyDocument;
            }
        }

        /// <summary>
        /// Get the current document
        /// </summary>
        /// <returns></returns>
        public static DSDocument Current
        {
            get
            {
                return new DSDocument();
            }
        }

    }
}
