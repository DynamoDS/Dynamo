using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.Revit.DB;
using DSNodeServices;
using Revit.GeometryConversion;
using Revit.GeometryObjects;
using Revit.References;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Plane = Autodesk.DesignScript.Geometry.Plane;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace Revit.Elements
{
    /// <summary>
    /// A Revit ModelText Element Point
    /// </summary>
    [RegisterForTrace]
    public class ModelText : AbstractElement
    {

        #region Internal properties

        /// <summary>
        /// Internal variable containing the wrapped Revit object
        /// </summary>
        internal Autodesk.Revit.DB.ModelText InternalModelText
        {
            get;
            private set;
        }

        /// <summary>
        /// Reference to the Element
        /// </summary>
        public override Autodesk.Revit.DB.Element InternalElement
        {
            get { return InternalModelText; }
        }

        #endregion

        #region Private constructors

        /// <summary>
        /// Internal constructor for wrapping a ModelText. 
        /// </summary>
        /// <param name="element"></param>
        private ModelText(Autodesk.Revit.DB.ModelText element)
        {
            InternalSetModelText(element);
        }

        /// <summary>
        /// Internal constructor for the ModelText
        /// </summary>
        /// <param name="text"></param>
        /// <param name="sketchPlane"></param>
        /// <param name="xCoordinateInPlane"></param>
        /// <param name="yCoordinateInPlane"></param>
        /// <param name="textDepth"></param>
        /// <param name="modelTextType"></param>
        private ModelText(string text, Autodesk.Revit.DB.SketchPlane sketchPlane, double xCoordinateInPlane, double yCoordinateInPlane, double textDepth, Autodesk.Revit.DB.ModelTextType modelTextType)
        {
            //Phase 1 - Check to see if the object exists and should be rebound
            var oldEle =
                ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.ModelText>(Document);


            // Note: not sure if there's a way to mutate the sketchPlane for a ModelText, so we need
            // to insure the Element hasn't changed position, otherwise, we destroy and rebuild the Element
            if (oldEle != null && PositionUnchanged(oldEle, sketchPlane, xCoordinateInPlane, yCoordinateInPlane))
            {            
                // There was an element and it's position hasn't changed
                InternalSetModelText(oldEle);
                InternalSetText(text);
                InternalSetDepth(textDepth);
                InternalSetModelTextType(modelTextType);
                return;
            }

            TransactionManager.Instance.EnsureInTransaction(Document);

            // We have to clean up the old ModelText b/c we can't mutate it's position
            if (oldEle != null)
            {
                DocumentManager.Instance.DeleteElement( oldEle.Id );
            }

            var mt = CreateModelText(text, sketchPlane, xCoordinateInPlane, yCoordinateInPlane, textDepth, modelTextType);
            InternalSetModelText(mt);

            TransactionManager.Instance.TransactionTaskDone();

            ElementBinder.SetElementForTrace(this.InternalElement);
        }

        #endregion

        #region Private mutators

        /// <summary>
        /// Set the Depth of the ModelText
        /// </summary>
        /// <param name="depth"></param>
        private void InternalSetDepth(double depth)
        {
            TransactionManager.Instance.EnsureInTransaction(Document);

            InternalModelText.Depth = depth;

            TransactionManager.Instance.TransactionTaskDone();
        }

        /// <summary>
        /// Set the Text of the ModelText
        /// </summary>
        /// <param name="text"></param>
        private void InternalSetText(string text)
        {
            TransactionManager.Instance.EnsureInTransaction(Document);

            InternalModelText.Text = text;

            TransactionManager.Instance.TransactionTaskDone();
        }

        /// <summary>
        /// Set the ModelTextType of the text
        /// </summary>
        /// <param name="modelTextType"></param>
        private void InternalSetModelTextType(Autodesk.Revit.DB.ModelTextType modelTextType)
        {
            TransactionManager.Instance.EnsureInTransaction(Document);

            InternalModelText.ModelTextType = modelTextType;

            TransactionManager.Instance.TransactionTaskDone();
        }

        /// <summary>
        /// Set the Element, ElementId, and UniqueId
        /// </summary>
        /// <param name="p"></param>
        private void InternalSetModelText(Autodesk.Revit.DB.ModelText p)
        {

            InternalModelText = p;
            this.InternalElementId = InternalModelText.Id;
            this.InternalUniqueId = InternalModelText.UniqueId;
        }

        #endregion

        #region Private static helper methods

        /// <summary>
        /// Check if the position of a ModelText has changed, given the original ModelText Element
        /// and the new position in the SketchPlane
        /// </summary>
        /// <param name="oldModelText"></param>
        /// <param name="newSketchPlane"></param>
        /// <param name="xCoordinateInPlane"></param>
        /// <param name="yCoordinateInPlane"></param>
        /// <returns></returns>
        private static bool PositionUnchanged(Autodesk.Revit.DB.ModelText oldModelText, Autodesk.Revit.DB.SketchPlane newSketchPlane, double xCoordinateInPlane, double yCoordinateInPlane)
        {

            var oldPosition = ((LocationPoint)oldModelText.Location).Point;

            var plane = newSketchPlane.GetPlane();
            var newPosition = plane.Origin + plane.XVec * xCoordinateInPlane + plane.YVec * yCoordinateInPlane;

            return (oldPosition.IsAlmostEqualTo(newPosition));

        }
        
        /// <summary>
        /// Create a ModelText element in the current Family Document
        /// </summary>
        /// <param name="text"></param>
        /// <param name="sketchPlane"></param>
        /// <param name="xCoordinateInPlane"></param>
        /// <param name="yCoordinateInPlane"></param>
        /// <param name="textDepth"></param>
        /// <param name="modelTextType"></param>
        /// <returns></returns>
        private static Autodesk.Revit.DB.ModelText CreateModelText(string text, Autodesk.Revit.DB.SketchPlane sketchPlane, double xCoordinateInPlane, double yCoordinateInPlane, double textDepth, Autodesk.Revit.DB.ModelTextType modelTextType)
        {
            TransactionManager.Instance.EnsureInTransaction(Document);

            // obtain the position of the ModelText element in the plane of the sketchPlane
            var plane = sketchPlane.GetPlane();
            var pos = plane.Origin + plane.XVec * xCoordinateInPlane + plane.YVec * yCoordinateInPlane;
                
            // create the modeltext
            var mt = Document.FamilyCreate.NewModelText(text, modelTextType, sketchPlane, pos, HorizontalAlign.Left, textDepth);         

            TransactionManager.Instance.TransactionTaskDone();

            return mt;
        }

        #endregion

        #region Public properties

        /// <summary>
        /// The Text of the ModelText Element
        /// </summary>
        public string Text
        {
            get
            {
                return InternalModelText.Text;
            }
        }

        /// <summary>
        /// The Depth of the ModelText Element
        /// </summary>
        public double Depth
        {
            get
            {
                return InternalModelText.Depth;
            }
        }

        /// <summary>
        /// The Position of the ModelText Element
        /// </summary>
        public XYZ Position
        {
            get
            {
                return ((LocationPoint) InternalElement.Location).Point;
            }
        }

        #endregion

        #region Static constructors

        /// <summary>
        /// Create a ModelText Element in the Family Document by providing the text, SketchPlane Element host, coordinates (within the plane of the SketchPlane),
        /// the depth of the text, and the text type name
        /// </summary>
        /// <param name="text"></param>
        /// <param name="sketchPlane"></param>
        /// <param name="xCoordinateInPlane"></param>
        /// <param name="yCoordinateInPlane"></param>
        /// <param name="textDepth"></param>
        /// <param name="modelTextType"></param>
        /// <returns></returns>
        public static ModelText ByTextSketchPlaneAndPosition(string text, SketchPlane sketchPlane, double xCoordinateInPlane, double yCoordinateInPlane, double textDepth, ModelTextType modelTextType )
        {
            if (!Document.IsFamilyDocument)
            {
                throw new Exception("ModelText Elements can only be created in a Family Document");
            }

            if (text == null)
            {
                throw new ArgumentNullException("text");
            }

            if (sketchPlane == null)
            {
                throw new ArgumentNullException("sketchPlane");
            }

            if (modelTextType == null)
            {
                throw new ArgumentNullException("modelTextType");
            }

            return new ModelText(text, sketchPlane.InternalSketchPlane, xCoordinateInPlane, yCoordinateInPlane,
                textDepth, modelTextType.InternalModelTextType);
        }

        #endregion

        #region Internal static constructors

        /// <summary>
        /// Create a ModelText Element from a user selected Element.
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="isRevitOwned"></param>
        /// <returns></returns>
        internal static ModelText FromExisting(Autodesk.Revit.DB.ModelText pt, bool isRevitOwned)
        {
            return new ModelText(pt)
            {
                IsRevitOwned = isRevitOwned
            };
        }

        #endregion

    }
}
