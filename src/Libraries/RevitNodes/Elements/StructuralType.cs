using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB.Structure;

namespace Revit.Elements
{
    public enum StructuralType
    {
        Beam, Brace, Column, Footing, NonStructural
    }

    /// <summary>
    /// Convert to Revit enum
    /// </summary>
    internal static class DSStructuralTypeExtensions
    {
        internal static Autodesk.Revit.DB.Structure.StructuralType ToRevitType(this StructuralType value)
        {
            switch (value)
            {
                case StructuralType.Beam:
                    return Autodesk.Revit.DB.Structure.StructuralType.Beam;
                case StructuralType.Column:
                    return Autodesk.Revit.DB.Structure.StructuralType.Column;
                case StructuralType.Footing:
                    return Autodesk.Revit.DB.Structure.StructuralType.Footing;
                case StructuralType.NonStructural:
                    return Autodesk.Revit.DB.Structure.StructuralType.NonStructural;
                default:
                    return Autodesk.Revit.DB.Structure.StructuralType.UnknownFraming;
            }
        }
    }
   
}


