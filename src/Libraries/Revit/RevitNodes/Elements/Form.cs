using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;

using Revit.GeometryObjects;
using Revit.GeometryReferences;
using RevitServices.Persistence;
using RevitServices.Transactions;

namespace Revit.Elements
{
    [DSNodeServices.RegisterForTrace]
    public class Form : Element
    {

        #region Internal Properties

        internal Autodesk.Revit.DB.Form InternalForm
        {
            get; private set;
        }

        /// <summary>
        /// Reference to the Element
        /// </summary>
        public override Autodesk.Revit.DB.Element InternalElement
        {
            get { return InternalForm; }
        }

        #endregion

        #region Private constructor

        /// <summary>
        /// Construct a Revit Form from an existing form.  
        /// </summary>
        /// <param name="form"></param>
        private Form(Autodesk.Revit.DB.Form form)
        {
            InternalSetForm(form);
        }

        /// <summary>
        /// Create a Form by lofting
        /// </summary>
        /// <param name="isSolid"></param>
        /// <param name="curves"></param>
        private Form(bool isSolid, ReferenceArrayArray curves)
        {
            // clean it up
            TransactionManager.Instance.EnsureInTransaction(Document);

            var f = Document.FamilyCreate.NewLoftForm(isSolid, curves);
            InternalSetForm(f);

            TransactionManager.Instance.TransactionTaskDone();

            ElementBinder.CleanupAndSetElementForTrace(Document, this.InternalElement);
        }

        #endregion

        #region Private mutator

        private void InternalSetForm(Autodesk.Revit.DB.Form form)
        {
            this.InternalForm = form;
            this.InternalElementId = form.Id;
            this.InternalUniqueId = form.UniqueId;
        }

        #endregion

        #region Hidden public static constructors 

        [IsVisibleInDynamoLibrary(false)]
        public static Form ByLoftCrossSections(ElementCurveReference[] curves, bool isSolid = true)
        {
            if (curves == null) throw new ArgumentNullException("curves");
            return ByLoftCrossSectionsInternal(curves, isSolid);
        }

        [IsVisibleInDynamoLibrary(false)]
        public static Form ByLoftCrossSections(ElementCurveReference[][] curves, bool isSolid = true)
        {
            if (curves == null) throw new ArgumentNullException("curves");
            return ByLoftMultiPartCrossSectionsInternal(curves, isSolid);
        }

        [IsVisibleInDynamoLibrary(false)]
        public static Form ByLoftCrossSections(Revit.Elements.Element[] curves, bool isSolid = true)
        {
            if (curves == null) throw new ArgumentNullException("curves");
            return ByLoftCrossSectionsInternal(curves, isSolid);
        }

        [IsVisibleInDynamoLibrary(false)]
        public static Form ByLoftCrossSections(Revit.Elements.Element[][] curves, bool isSolid = true)
        {
            if (curves == null) throw new ArgumentNullException("curves");
            return ByLoftMultiPartCrossSectionsInternal(curves, isSolid);
        }

        #endregion

        #region Public static constructors

        public static Form ByLoftCrossSections(Autodesk.DesignScript.Geometry.Curve[] curves, bool isSolid = true)
        {
            if (curves == null) throw new ArgumentNullException("curves");
            return ByLoftCrossSectionsInternal(curves, isSolid);
        }

        public static Form ByLoftCrossSections(Autodesk.DesignScript.Geometry.Curve[][] curves, bool isSolid = true)
        {
            if (curves == null) throw new ArgumentNullException("curves");
            return ByLoftMultiPartCrossSectionsInternal(curves, isSolid);
        }

        #endregion

        #region Private static constructors

        private static Form ByLoftCrossSectionsInternal(object[] curves, bool isSolid = true)
        {
            if (curves == null) throw new ArgumentNullException("curves");

            // if the arguments are polycurves, explode them
            if (curves.Any(x => x is PolyCurve))
            {
                var ca = curves.Select(x => x is PolyCurve ? ((PolyCurve)x).Curves() : new[] { x }).ToArray();
                return ByLoftMultiPartCrossSectionsInternal(ca, isSolid);
            }

            var refArrArr = new ReferenceArrayArray();

            foreach (var l in curves)
            {
                if (l == null) throw new ArgumentNullException("curves");
                var refArr = new ReferenceArray();

                refArr.Append(ElementCurveReference.TryGetCurveReference(l, "Form").InternalReference);
                refArrArr.Append(refArr);
            }

            return new Form(isSolid, refArrArr);
        }

        private static Form ByLoftMultiPartCrossSectionsInternal(object[][] curves, bool isSolid = true)
        {
            if (curves == null || curves.SelectMany(x => x).Any(x => x == null))
            {
                throw new ArgumentNullException("Some of the input curves are null.");
            }

            var refArrArr = new ReferenceArrayArray();

            foreach (var curveArr in curves)
            {
                var refArr = new ReferenceArray();
                curveArr.ForEach(x => refArr.Append(ElementCurveReference.TryGetCurveReference(x, "Form").InternalReference));
                refArrArr.Append(refArr);
            }

            return new Form(isSolid, refArrArr);
        }

        #endregion

        #region Internal static constructors

        /// <summary>
        /// Construct the Revit element by selection.  
        /// </summary>
        /// <param name="formElement"></param>
        /// <param name="isRevitOwned"></param>
        /// <returns></returns>
        internal static Form FromExisting(Autodesk.Revit.DB.Form formElement, bool isRevitOwned)
        {
            return new Form(formElement)
            {
                IsRevitOwned = isRevitOwned
            };
        }

        #endregion

    }
}

