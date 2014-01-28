using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using Revit.GeometryConversion;
using Revit.Elements;

namespace Revit.GeometryObjects
{
    /// <summary>
    /// Temporary class for bounding boxes.  There should be a 
    /// ProtoGeometry type for this.
    /// </summary>
    public class BoundingBox
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

        internal BoundingBox(BoundingBoxXYZ box)
        {
            InternalBoundingBoxXyz = box;
        }

        internal static BoundingBox FromElement( AbstractElement e )
        {
            return new BoundingBox(e.InternalElement.get_BoundingBox(null));
        }

        internal static BoundingBox FromElementInView( AbstractElement e, AbstractView3D view3D )
        {
            return new BoundingBox(e.InternalElement.get_BoundingBox( (Autodesk.Revit.DB.View) view3D.InternalElement) );
        }

        internal static BoundingBox FromExisting( BoundingBoxXYZ box )
        {
            return new BoundingBox( box );
        }
        
    }
}
