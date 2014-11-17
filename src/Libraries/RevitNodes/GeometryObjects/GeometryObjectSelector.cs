using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using Revit.GeometryConversion;
using Revit.GeometryReferences;
using RevitServices.Persistence;
using Face = Autodesk.DesignScript.Geometry.Face;

namespace Revit.GeometryObjects
{
    [IsVisibleInDynamoLibrary(false)]
    public static class GeometryObjectSelector
    {
        /// <summary>
        /// Return an AbstractGeometryObject given a string representation of the geometry's reference.
        /// </summary>
        /// <param name="referenceString"></param>
        /// <returns></returns>
        public static object ByReferenceStableRepresentation(string referenceString)
        {
            try
            {
                var doc = DocumentManager.Instance.CurrentDBDocument;
                var elRef =
                    Reference.ParseFromStableRepresentation(doc, referenceString);

                var ele =
                    DocumentManager.Instance
                        .CurrentDBDocument.GetElement(elRef);

                var geob = ele.GetGeometryObjectFromReference(elRef);
                if (geob == null) return null;
                
                var familyInstance = ele as FamilyInstance;
                if (familyInstance != null && RequiresTransform(familyInstance))
                {
                    var transf = familyInstance.GetTransform().ToCoordinateSystem();
                    return geob.Convert(elRef, transf);
                }

                return geob.Convert(elRef);
            }
            catch(Exception ex)
            {
                throw new Exception("Could not get a geometry object from the current document using the provided reference.");
            }
        }

        /// <summary>
        /// 
        ///     Determine if the Geometry extracted from a FamilyInstance requires transformation.
        /// 
        ///     Bizarrely, some FamilyInstance's geom is transformed and some not when obtained
        ///     from GetGeometryObjectFromReference.  This is because some need to be transformed
        ///     to interact with adjacent geometry in the document.  This stop-gap, suggested by
        ///     SC in the Revit API team, checks if there are any non-empty GeometryInstances in 
        ///     FamilyInstance's geometry.  Apparently this is a good heuristic for checking if
        ///     the geometry requires a transform or not.
        /// 
        /// </summary>
        /// <param name="familyInstance"></param>
        /// <returns></returns>
        private static bool RequiresTransform(Autodesk.Revit.DB.FamilyInstance familyInstance)
        {
            var geom = familyInstance.get_Geometry(new Options());
            return geom.OfType<Autodesk.Revit.DB.GeometryInstance>()
                .Any(x => x.GetInstanceGeometry().Any());
        }

    }
}
