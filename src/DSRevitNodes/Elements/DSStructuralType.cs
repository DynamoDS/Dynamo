using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB.Structure;

namespace DSRevitNodes.Elements
{
    public enum DSStructuralType
    {
        Beam, Brace, Column, Footing, NonStructural
    }

    /// <summary>
    /// Convert to Revit enum
    /// </summary>
    public static class DSStructuralTypeExtensions
    {
        public static Autodesk.Revit.DB.Structure.StructuralType ToRevitType(this DSStructuralType value)
        {
            switch (value)
            {
                case DSStructuralType.Beam:
                    return StructuralType.Beam;
                case DSStructuralType.Column:
                    return StructuralType.Column;
                case DSStructuralType.Footing:
                    return StructuralType.Footing;
                case DSStructuralType.NonStructural:
                    return StructuralType.NonStructural;
                default:
                    return StructuralType.UnknownFraming;
            }
        }
    }
   
}


