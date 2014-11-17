using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Interfaces;
using Autodesk.Revit.DB;
using Microsoft.CSharp.RuntimeBinder;
using RevitServices.Persistence;

namespace Revit.GeometryReferences
{
    /// <summary>
    /// A stable reference to a Revit curve derived from a Revit Element
    /// </summary>
    /// See: http://revitapisearch.com.s3-website-us-east-1.amazonaws.com/html/d5e10517-24fa-4627-43be-8981746d30c8.htm
    public class ElementCurveReference : ElementGeometryReference
    {
        internal ElementCurveReference(Autodesk.Revit.DB.Curve curve)
        {
            if ( curve.Reference == null )
            {
                throw new Exception("A Curve Reference can only be obtained "
                                    + "from an Element.");
            }
            this.InternalReference = curve.Reference;
        }

        internal ElementCurveReference(Autodesk.Revit.DB.Reference reference)
        {
            this.InternalReference = reference;
        }

        public const string DefaultTag = "RevitCurveReference";

        internal static ElementCurveReference FromExisting(Autodesk.Revit.DB.Curve curve)
        {
            return new ElementCurveReference(curve);
        }

        internal static ElementCurveReference FromExisting(Autodesk.Revit.DB.Reference reference)
        {
            return new ElementCurveReference(reference);
        }

        internal static Autodesk.DesignScript.Geometry.Curve AddTag(Autodesk.DesignScript.Geometry.Curve curve, Autodesk.Revit.DB.Reference reference)
        {
            curve.Tags.AddTag(DefaultTag, reference);
            return curve;
        }

        /// <summary>
        /// Attempt to extract a CurveReference from an object which may be a Revit.References.CurveReference, Revit.Elements.CurveElement, or Revit.Elements.Element
        /// </summary>
        /// <param name="curveObject"></param>
        /// <param name="nodeTypeString"></param>
        /// <returns></returns>
        internal static ElementCurveReference TryGetCurveReference(object curveObject, string nodeTypeString = "This node")
        {
            dynamic curve = curveObject;

            try
            {
                return TryGetCurveReference(curve);
            }
            catch (RuntimeBinderException)
            {
                throw new ArgumentException(nodeTypeString +
                                            " requires a ElementCurveReference extracted from a Revit Element! ");
            }
            catch (Exception)
            {
                throw new ArgumentException("Could not obtain a CurveReference from the provided data! " + nodeTypeString +
                                            " requires a ElementCurveReference extracted from a Revit Element! ");
            }
        }

        private static ElementCurveReference TryGetCurveReference(ElementCurveReference elementCurveObject)
        {
            return elementCurveObject;
        }

        private static ElementCurveReference TryGetCurveReference(Revit.Elements.CurveElement curveObject)
        {
            return curveObject.ElementCurveReference;
        }

        private static ElementCurveReference TryGetCurveReference(Revit.Elements.Element curveObject, string nodeTypeString = "This node")
        {
            var cs = curveObject.InternalGeometry().OfType<Autodesk.Revit.DB.Curve>();
            if (cs.Any()) return new ElementCurveReference(cs.First());

            throw new ArgumentException(nodeTypeString + " requires a ElementCurveReference extracted from a Revit Element! " +
                             "You supplied an " + curveObject.ToString() + ", but we could not extract a CurveReference from it!");
        }

        private static ElementCurveReference TryGetCurveReference(Autodesk.DesignScript.Geometry.Curve curveObject, string nodeTypeString = "This node")
        {
            // If a Reference has been added to this object, we can use that
            // to build the element.
            object tagObj = curveObject.Tags.LookupTag(DefaultTag);
            if (tagObj != null)
            {
                var tagRef = (Reference) tagObj;
                return new ElementCurveReference(tagRef);
            }

            throw new ArgumentException(nodeTypeString + " requires a Curve extracted from a Revit Element! " +
                                         "You can use the ModelCurve.ByCurve or ImportInstance.ByGeometry to " +
                                            "turn this Curve into a Revit Element.");
        }

    }
}
