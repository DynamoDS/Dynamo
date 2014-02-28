using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autodesk.DesignScript.Structure
{
    public class SectionDS
    {
        public string LabelName { get; protected set; }
        public SectionType BarSectionType { get; protected set; }
        public double Width { get; protected set; }
        public double Thickness { get; protected set; }
        public double Height { get; protected set; }

        public enum SectionType
        {
            SquareSection,
            RectantulareSection,
            CircularSection,
        };

        public static SectionDS BySection(SectionDS section)
        {
            SectionDS newSection = new SectionDS();

            newSection.LabelName = section.LabelName;
            newSection.BarSectionType = section.BarSectionType;
            newSection.Width = section.Width;
            newSection.Thickness = section.Thickness;
            newSection.Height = section.Height;

            return newSection;
        }
    }
    
    

}

