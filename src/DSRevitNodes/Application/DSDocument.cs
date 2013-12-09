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

        internal Autodesk.Revit.DB.Document Document
        {
            get
            {
                return DocumentManager.GetInstance().CurrentDBDocument;
            }
        }

        public AbstractView ActiveView 
        {
            get
            {
                return (AbstractView) ElementSelector.WrapElement(Document.ActiveView, true);
            }
        }

        public bool IsFamilyDocument
        {
            get
            {
                return Document.IsFamilyDocument;
            }
        }

        public static DSDocument Current()
        {
            return new DSDocument();
        }

    }
}
