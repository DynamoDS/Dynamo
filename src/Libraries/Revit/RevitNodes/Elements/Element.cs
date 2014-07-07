using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using DSCore;
using DSNodeServices;
using DynamoUnits;
using Revit.GeometryConversion;
using Revit.GeometryReferences;
using RevitServices.Persistence;
using RevitServices.Threading;
using RevitServices.Transactions;
using Color = DSCore.Color;
using Revit.GeometryObjects;
using Area = DynamoUnits.Area;
using ArgumentException = Autodesk.Revit.Exceptions.ArgumentException;
using Curve = Autodesk.DesignScript.Geometry.Curve;

namespace Revit.Elements
{
    /// <summary>
    /// Superclass of all Revit element wrappers
    /// </summary>
    //[SupressImportIntoVM]
    public abstract class Element : IDisposable, IGraphicItem, IFormattable
    {
        /// <summary>
        /// A reference to the current Document.
        /// </summary>
        [IsVisibleInDynamoLibrary(false)]
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
        [IsVisibleInDynamoLibrary(false)]
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
                TransactionManager.Instance.EnsureInTransaction(Document);
                DocumentManager.Regenerate();
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
        [IsVisibleInDynamoLibrary(false)]
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
        [IsVisibleInDynamoLibrary(false)]
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
        [IsVisibleInDynamoLibrary(false)]
        public virtual void Dispose()
        {

            // Do not cleanup Revit elements if we are shutting down Dynamo.
            if (DisposeLogic.IsShuttingDown)
                return;

            bool didRevitDelete = ElementIDLifecycleManager<int>.GetInstance().IsRevitDeleted(this.Id);



            var elementManager = ElementIDLifecycleManager<int>.GetInstance();
            int remainingBindings = elementManager.UnRegisterAssociation(this.Id, this);

            // Do not delete Revit owned elements
            if (!IsRevitOwned && remainingBindings == 0 && !didRevitDelete)
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
        [IsVisibleInDynamoLibrary(false)]
        public override string ToString()
        {
            return this.GetType().Name;
        }

        public virtual string ToString(string format, IFormatProvider formatProvider)
        {
            // As a default, return the standard string representation.
            // Override ToString with format information in children.
            return ToString();
        }

        [IsVisibleInDynamoLibrary(false)]
        public void Tessellate(IRenderPackage package, double tol, int gridLines)
        {
            // Do nothing. We implement this method only to prevent the GraphicDataProvider from
            // attempting to interrogate the public properties, some of which may require regeneration
            // or transactions and which must necessarily be threaded in a specific way.
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
                return string.Empty;

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
                            result = Length.FromFeet(param.AsDouble());
                            break;
                        case ParameterType.Area:
                            result = Area.FromSquareFeet(param.AsDouble());
                            break;
                        case ParameterType.Volume:
                            result = Volume.FromCubicFeet(param.AsDouble());
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

        /// <summary>
        /// Override the element's color in the active view.
        /// </summary>
        /// <param name="color">The color to apply to a solid fill on the element.</param>
        public Element OverrideColorInView(Color color)
        {
            TransactionManager.Instance.EnsureInTransaction(DocumentManager.Instance.CurrentDBDocument);

            var view = DocumentManager.Instance.CurrentUIDocument.ActiveView;
            var ogs = new OverrideGraphicSettings();

            var patternCollector = new FilteredElementCollector(DocumentManager.Instance.CurrentDBDocument);
            patternCollector.OfClass(typeof(FillPatternElement));
            FillPatternElement solidFill = patternCollector.ToElements().Cast<FillPatternElement>().First(x => x.GetFillPattern().Name == "Solid fill");

            ogs.SetProjectionFillColor(new Autodesk.Revit.DB.Color(color.Red, color.Green, color.Blue));
            ogs.SetProjectionFillPatternId(solidFill.Id);
            view.SetElementOverrides(this.InternalElementId, ogs);

            TransactionManager.Instance.TransactionTaskDone();
            return this;
        }

        /// <summary>
        /// Set one of the element's parameters.
        /// </summary>
        /// <param name="parameterName">The name of the parameter to set.</param>
        /// <param name="value">The value.</param>
        public Element SetParameterByName(string parameterName, object value)
        {
            var param = this.InternalElement.Parameters.Cast<Autodesk.Revit.DB.Parameter>().FirstOrDefault(x => x.Definition.Name == parameterName);

            if (param == null)
                throw new Exception("No parameter found by that name.");

            TransactionManager.Instance.EnsureInTransaction(DocumentManager.Instance.CurrentDBDocument);

            var dynval = value as dynamic;
            SetParameterValue(param, dynval);

            TransactionManager.Instance.TransactionTaskDone();

            return this;
        }

        #region dynamic parameter setting methods

        private void SetParameterValue(Autodesk.Revit.DB.Parameter param, double value)
        {
            if (param.StorageType != StorageType.Integer && param.StorageType != StorageType.Double)
                throw new Exception("The parameter's storage type is not a number.");

            param.Set(value);
        }

        private void SetParameterValue(Autodesk.Revit.DB.Parameter param, Revit.Elements.Element value)
        {
            if (param.StorageType != StorageType.ElementId)
                throw new Exception("The parameter's storage type is not an Element.");

            param.Set(value.InternalElementId);
        }

        private void SetParameterValue(Autodesk.Revit.DB.Parameter param, int value)
        {
            if (param.StorageType != StorageType.Integer && param.StorageType != StorageType.Double)
                throw new Exception("The parameter's storage type is not a number.");

            param.Set(value);
        }

        private void SetParameterValue(Autodesk.Revit.DB.Parameter param, string value)
        {
            if (param.StorageType != StorageType.String)
                throw new Exception("The parameter's storage type is not a string.");

            param.Set(value);
        }

        private void SetParameterValue(Autodesk.Revit.DB.Parameter param, bool value)
        {
            if (param.StorageType != StorageType.Integer)
                throw new Exception("The parameter's storage type is not an integer.");

            param.Set(value == false ? 0 : 1);
        }

        #endregion

        /// <summary>
        /// Get all of the Geometry associated with this object
        /// </summary>
        public object[] Geometry()
        {
            var converted = new List<object>();

            foreach (var geometryObject in InternalGeometry())
            {
                try
                {
                    var convert = geometryObject.Convert();
                    if (convert != null)
                    {
                        converted.Add(convert);
                    }
                }
                catch (Exception)
                {
                    // we catch all geometry conversion exceptions
                }
            }

            return converted.ToArray();
        }

        #region Geometry extraction

        [SupressImportIntoVM]
        public IEnumerable<Autodesk.Revit.DB.GeometryObject> InternalGeometry()
        {
            var thisElement = InternalElement;

            var goptions0 = new Autodesk.Revit.DB.Options();
            goptions0.ComputeReferences = true;

            var geomElement = thisElement.get_Geometry(goptions0);

            // GenericForm is a special case
            if ((thisElement is GenericForm) && (!geomElement.Any()))
            {
                var gF = (GenericForm)thisElement;
                if (!gF.Combinations.IsEmpty)
                {
                    var goptions1 = new Autodesk.Revit.DB.Options();
                    goptions1.IncludeNonVisibleObjects = true;
                    goptions1.ComputeReferences = true;
                    geomElement = thisElement.get_Geometry(goptions1);
                }
            }

            return CollectConcreteGeometry(geomElement);
        }

        private static IEnumerable<GeometryObject> CollectConcreteGeometry(GeometryElement geometryElement)
        {
            var instanceGeometryObjects = new List<Autodesk.Revit.DB.GeometryObject>();

            if (geometryElement == null) return instanceGeometryObjects;

            foreach (Autodesk.Revit.DB.GeometryObject geob in geometryElement)
            {
                var geomInstance = geob as GeometryInstance;
                var geomElement = geob as GeometryElement;

                if (geomInstance != null)
                {
                    var instanceGeom = geomInstance.GetInstanceGeometry();
                    instanceGeometryObjects.AddRange(CollectConcreteGeometry(instanceGeom));
                }
                else if (geomElement != null)
                {
                    instanceGeometryObjects.AddRange( CollectConcreteGeometry(geometryElement) );
                }
                else
                {
                    instanceGeometryObjects.Add(geob);
                }
            }

            // Certain kinds of Elements will return Solids with zero faces - make sure to filter them out
            return
                instanceGeometryObjects.Where(
                    x =>
                        !(x is Autodesk.Revit.DB.Solid) || (x as Autodesk.Revit.DB.Solid).Faces.Size > 0);
        }

        public Autodesk.DesignScript.Geometry.Solid[] Solids
        {
            get { return Geometry().OfType<Autodesk.DesignScript.Geometry.Solid>().ToArray(); }
        }

        public Curve[] Curves
        {
            get
            {
                var curves = GetCurves(new Options()
                {
                    ComputeReferences = true
                });

                return curves.Select(x => x.ToProtoType()).ToArray();
            }
        }

        public Surface[] Faces
        {
            get
            {
                var curves = GetFaces(new Options()
                {
                    ComputeReferences = true
                });

                return curves.Select(x => x.ToProtoType()).ToArray();
            }
        }

        public ElementCurveReference[] ElementCurveReferences
        {
            get
            {
                var curves = GetCurves(new Options()
                {
                    ComputeReferences = true
                });

                return curves.Select(ElementCurveReference.FromExisting).ToArray();
            }
        }

        public ElementFaceReference[] ElementFaceReferences
        {
            get
            {
                var faces = GetFaces(new Options()
                {
                    ComputeReferences = true
                });

                return faces.Select(ElementFaceReference.FromExisting).ToArray();
            }
        }


        /// <summary>
        /// Is this element still alive in Revit, and good to be drawn, queried etc.
        /// </summary>
        protected bool IsAlive
        {
            get
            {
                if (this.InternalElementId == null)
                {
                    return false;
                }

                //Ensure that the object is still alive
                
                //Check whether the internal element Id is null
                if (null == InternalElementId)
                    return false;

                return !ElementIDLifecycleManager<int>.GetInstance().IsRevitDeleted(this.InternalElementId.IntegerValue);
            }
        }

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
        private void GetCurves(IEnumerable<Autodesk.Revit.DB.GeometryObject> geomElem, ref CurveArray curves)
        {
            foreach (Autodesk.Revit.DB.GeometryObject geomObj in geomElem)
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
        private void GetFaces(IEnumerable<Autodesk.Revit.DB.GeometryObject> geomElement, ref FaceArray faces)
        {

                foreach (Autodesk.Revit.DB.GeometryObject geob in geomElement)
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