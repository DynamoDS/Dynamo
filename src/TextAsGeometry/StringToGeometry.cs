using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using System.Windows;
using System.Globalization;


namespace TextAsGeometry
{
    public class StringToGeometry
    {


        private static Autodesk.DesignScript.Geometry.Point WindowsPoint2DSPoint (System.Windows.Point point){

            var result = Autodesk.DesignScript.Geometry.Point.ByCoordinates(point.X, point.Y, 0);
            return result;
        }

        private static Autodesk.DesignScript.Geometry.PolyCurve PolyLineSegmentsToProtoLine(PolyLineSegment polyline)
        {

            var points = polyline.Points;

            var convertedPoints = points.Select(x => WindowsPoint2DSPoint(x)).ToList();

            return Autodesk.DesignScript.Geometry.PolyCurve.ByPoints(convertedPoints);
          

        }


        private static Autodesk.DesignScript.Geometry.Line LineSegmentsToProtoLine(LineSegment lineseg1, LineSegment lineseg2)
        {

            var startwp = lineseg1.Point;
            var endwp = lineseg2.Point;

            var start = Autodesk.DesignScript.Geometry.Point.ByCoordinates(startwp.X, startwp.Y, 0);

            var end = Autodesk.DesignScript.Geometry.Point.ByCoordinates(endwp.X, endwp.Y, 0);

            return Autodesk.DesignScript.Geometry.Line.ByStartPointEndPoint(start, end);

        }

        public static System.Windows.Media.Geometry TextGeoFromString(string inputstring)
        {

            System.Windows.FontStyle fontStyle = FontStyles.Normal;
            FontWeight fontWeight = FontWeights.Medium;

            // Create the formatted text based on the properties set.
            FormattedText formattedText = new FormattedText(
                inputstring,
                CultureInfo.GetCultureInfo("en-us"),
                FlowDirection.LeftToRight,
                new Typeface("Verdana"),
                11,
                System.Windows.Media.Brushes.Black);

            // Build the geometry object that represents the text.

            return formattedText.BuildGeometry(new System.Windows.Point(0, 0));
        }



        public static List<Autodesk.DesignScript.Geometry.PolyCurve> TextGeoToProtoGeo(System.Windows.Media.Geometry textGeometry)
        {
            PathFigureCollection pfc = null;

            // Update UI component here


            // flatten the geometry
            var copy = textGeometry.Clone();
            var flatten = copy.GetFlattenedPathGeometry();
            // get the figure collection from the flattened geometry
            pfc = flatten.Figures;
            //iterate all path figures

            // convert all the linesegments into protolines nested foreach figure


            var lineCollector = new List<List<PolyLineSegment>>();

            foreach (PathFigure pf in pfc)
            {
                var figure = new List<PolyLineSegment>();
                foreach (PathSegment ps in pf.Segments)
                {
                    
                    if (ps is PolyLineSegment)
                    {
                        figure.Add(ps as PolyLineSegment);
                    }
                }
                lineCollector.Add(figure);
            }

            //var zipped = lineCollector.Select(x => x.Zip(x.Skip(1), LineSegmentsToProtoLine).ToList()).ToList();

            var zipped = lineCollector.Select(x => PolyLineSegmentsToProtoLine(x.First())).ToList();

            return zipped;



        }
    }
}