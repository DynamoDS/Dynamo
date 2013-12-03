using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using DSRevitNodes.Elements;
using DSRevitNodes.GeometryConversion;

namespace DSRevitNodes.GeometryObjects
{
    /// <summary>
    /// Temporary class for bounding boxes.  There should be a 
    /// ProtoGeometry type for this.
    /// </summary>
    public class DSBoundingBox
    {
        internal BoundingBoxXYZ InternalBoundingBoxXyz
        {
            get; private set;
        }

        public Autodesk.DesignScript.Geometry.Point Max
        {
            get
            {
                return InternalBoundingBoxXyz.Max.ToPoint();
            }
        }

        public Autodesk.DesignScript.Geometry.Point Min
        {
            get
            {
                return InternalBoundingBoxXyz.Min.ToPoint();
            }
        } 

        private DSBoundingBox(BoundingBoxXYZ box)
        {
            InternalBoundingBoxXyz = box;
        }

        internal static DSBoundingBox FromElement( AbstractElement e )
        {
            return new DSBoundingBox(e.InternalElement.get_BoundingBox(null));
        }

        internal static DSBoundingBox FromElementInView( AbstractElement e, AbstractView3D view3D )
        {
            return new DSBoundingBox(e.InternalElement.get_BoundingBox( (Autodesk.Revit.DB.View) view3D.InternalElement) );
        }

        internal static DSBoundingBox FromExisting( BoundingBoxXYZ box )
        {
            return new DSBoundingBox( box );
        }
        
    }
}
