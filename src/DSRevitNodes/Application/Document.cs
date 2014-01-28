using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Revit.Elements;
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
        internal Autodesk.Revit.DB.Document InternalDocument
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
                return (AbstractView) ElementWrappingExtensions.ToDSType(InternalDocument.ActiveView, true);
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
                return new Document();
            }
        }

    }
}
