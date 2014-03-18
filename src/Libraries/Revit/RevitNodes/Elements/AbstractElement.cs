using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using DSNodeServices;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using RevitServices.Transactions;

namespace Revit.Elements
{
    /// <summary>
    /// Superclass of all Revit element wrappers
    /// </summary>
    //[SupressImportIntoVM]
    public abstract class AbstractElement : IDisposable
    {
        /// <summary>
        /// A reference to the current Document.
        /// </summary>
        public static Document Document
        {
            get { return DocumentManager.Instance.CurrentDBDocument; }
        }

        /// <summary>
        /// Indicates whether the element is owned by Revit or not.  If the element
        /// is Revit owned, it should not be deleted by Dispose().
        /// </summary>
        internal bool IsRevitOwned = false;

        /// <summary>
        /// Obtain all of the Parameters from an Element
        /// </summary>
        public Parameter[] Parameters
        {
            get
            {
                var parms = this.InternalElement.Parameters;
                return parms.Cast<Autodesk.Revit.DB.Parameter>().Select(x => new Parameter(x)).ToArray();
            }
        }

        /// <summary>
        /// Get the Name of the Element
        /// </summary>
        public string Name
        {
            get
            {
                return InternalElement.Name;
            }
        }

        /// <summary>
        /// Get an Axis-aligned BoundingBox of the Element
        /// </summary>
        public Autodesk.DesignScript.Geometry.BoundingBox BoundingBox
        {
            get
            {
                TransactionManager.Instance.EnsureInTransaction( Document );

                DocumentManager.Instance.CurrentDBDocument.Regenerate();
                var bb = this.InternalElement.get_BoundingBox(null);

                TransactionManager.Instance.TransactionTaskDone();

                return bb.ToProtoType();
            }
        }

        /// <summary>
        /// Get the Element Id for this element
        /// </summary>
        public int Id
        {
            get
            {
                return this.InternalElementId.IntegerValue;
            }
        }

        /// <summary>
        /// Get the Element Unique Id for this element
        /// </summary>
        public string UniqueId
        {
            get
            {
                return this.InternalUniqueId;
            }
        }

        /// <summary>
        /// A reference to the element
        /// </summary>
        //[SupressImportIntoVM]
        public abstract Autodesk.Revit.DB.Element InternalElement
        {
            get;
        }

        private ElementId internalId;
        
        /// <summary>
        /// The element id for this element
        /// </summary>
        protected ElementId InternalElementId
        {
            get { return internalId; }
            set { 
                internalId = value;

                var elementManager = ElementIDLifecycleManager<int>.GetInstance();
                elementManager.RegisterAsssociation(this.Id, this);

            }
        }

        /// <summary>
        /// The unique id for this element
        /// </summary>
        protected string InternalUniqueId;

        /// <summary>
        /// Default implementation of dispose that removes the element from the
        /// document
        /// </summary>
        public virtual void Dispose()
        {

            // Do not cleanup Revit elements if we are shutting down Dynamo.
            if (DisposeLogic.IsShuttingDown)
                return;

            var elementManager = ElementIDLifecycleManager<int>.GetInstance();
            int remainingBindings = elementManager.UnRegisterAssociation(this.Id, this);

            // Do not delete Revit owned elements
            if (!IsRevitOwned && remainingBindings == 0)
            {
                DocumentManager.Instance.DeleteElement(this.InternalElementId);
            }
            else
            {
                //This element has gone
                //but there was something else holding onto the Revit object so don't purge it

                internalId = null;
            }

        }

        /// <summary>
        /// A basic implementation of ToString for Elements
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return InternalElement.ToString();
        }

        /// <summary>
        /// Set one of the element's parameters.
        /// </summary>
        /// <param name="parameterName">The name of the parameter to set.</param>
        /// <param name="value">The value.</param>
        public void SetParameterByName(string parameterName, object value)
        {
            var param = this.InternalElement.Parameters.Cast<Autodesk.Revit.DB.Parameter>().FirstOrDefault(x => x.Definition.Name == parameterName);

            TransactionManager.Instance.EnsureInTransaction(DocumentManager.Instance.CurrentDBDocument);
            var dynval = value as dynamic;
            SetParameterValue(param, dynval);
            TransactionManager.Instance.TransactionTaskDone();
        }

        /// <summary>
        /// Get the value of one of the element's parameters.
        /// </summary>
        /// <param name="parameterName">The name of the parameter whose value you want to obtain.</param>
        /// <returns></returns>
        public object GetParameterValueByName(string parameterName)
        {
            object result = null;

            var param = this.InternalElement.Parameters.Cast<Autodesk.Revit.DB.Parameter>().FirstOrDefault(x => x.Definition.Name == parameterName);

            if (param == null || !param.HasValue)
                return null;

            switch (param.StorageType)
            {
                case StorageType.ElementId:
                    result = param.AsElementId();
                    break;
                case StorageType.String:
                    result = param.AsString();
                    break;
                case StorageType.Integer:
                    result = param.AsInteger();
                    break;
                case StorageType.Double:
                    switch (param.Definition.ParameterType)
                    {
                        case ParameterType.Length:
                            result = Dynamo.Units.Length.FromFeet(param.AsDouble());
                            break;
                        case ParameterType.Area:
                            result = Dynamo.Units.Area.FromSquareFeet(param.AsDouble());
                            break;
                        case ParameterType.Volume:
                            result = Dynamo.Units.Volume.FromCubicFeet(param.AsDouble());
                            break;
                        default:
                            result = param.AsDouble();
                            break;
                    }
                    break;
                default:
                    throw new Exception(string.Format("Parameter {0} has no storage type.", param));
            }

            return result;
        }

        #region dynamic parameter setting methods

        private void SetParameterValue(Autodesk.Revit.DB.Parameter param, double value)
        {
            param.Set(value);
        }

        private void SetParameterValue(Autodesk.Revit.DB.Parameter param, int value)
        {
            param.Set(value);
        }

        private void SetParameterValue(Autodesk.Revit.DB.Parameter param, string value)
        {
            param.Set(value);
        }

        #endregion

        #region Internal Geometry Helpers

        protected IEnumerable<Autodesk.Revit.DB.Curve> GetCurves(Autodesk.Revit.DB.Options options)
        {
            var geomElem = this.InternalElement.get_Geometry(options);
            var curves = new CurveArray();
            GetCurves(geomElem, ref curves);

            return curves.Cast<Autodesk.Revit.DB.Curve>();

        }

        protected IEnumerable<Autodesk.Revit.DB.Face> GetFaces(Autodesk.Revit.DB.Options options)
        {
            var geomElem = this.InternalElement.get_Geometry(options);
            var faces = new FaceArray();
            GetFaces(geomElem, ref faces);

            return faces.Cast<Autodesk.Revit.DB.Face>();

        }

        /// <summary>
        /// Recursively traverse the GeometryElement obtained from this Element, collecting the Curves
        /// </summary>
        /// <param name="geomElem"></param>
        /// <param name="curves"></param>
        private void GetCurves(IEnumerable<GeometryObject> geomElem, ref CurveArray curves)
        {
            foreach (GeometryObject geomObj in geomElem)
            {
                var curve = geomObj as Autodesk.Revit.DB.Curve;
                if (null != curve)
                {
                    curves.Append(curve);
                    continue;
                }

                //If this GeometryObject is Instance, call AddCurve
                var geomInst = geomObj as GeometryInstance;
                if (null != geomInst)
                {
                    var transformedGeomElem // curves transformed into project coords
                        = geomInst.GetInstanceGeometry(geomInst.Transform.Inverse);
                    GetCurves(transformedGeomElem, ref curves);
                }
            }
        }

        /// <summary>
        /// Recursively traverse the GeometryElement obtained from this Element, collecting the Curves
        /// </summary>
        /// <param name="geomElem"></param>
        /// <param name="faces"></param>
        private void GetFaces(IEnumerable<GeometryObject> geomElement, ref FaceArray faces)
        {

                foreach (GeometryObject geob in geomElement)
                {
                    if (geob is GeometryInstance)
                    {
                        GetFaces((geob as GeometryInstance).GetInstanceGeometry(), ref faces);
                    }
                    else if (geob is Autodesk.Revit.DB.Solid)
                    {
                        var mySolid = geob as Autodesk.Revit.DB.Solid;
                        if (mySolid != null)
                        {
                            foreach (var f in mySolid.Faces.Cast<Autodesk.Revit.DB.Face>().ToList())
                            {
                                faces.Append(f);
                            }
                        }

                    } else if (geob is Autodesk.Revit.DB.GeometryElement)
                    {
                        GetFaces(geob as GeometryElement, ref faces);
                    }
                }

        }

        #endregion

    }
}