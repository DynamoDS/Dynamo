using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Dynamo.Wpf.CurveMapper
{
    public class CurveRenderer
    {
        /// <summary>
        /// Renders a curve as a Path object based on given X and Y values, adjusting for the inverted Y-axis in WPF.
        /// </summary>
        public static List<Path> RenderCurve(List<double> xValues, List<double> yValues, double canvasSize, bool isControlLine = false)
        {
            if (xValues == null || yValues == null || xValues.Count != yValues.Count || xValues.Count < 2)
                return null;

            List<Path> paths = new List<Path>();
            PathGeometry currentGeometry = new PathGeometry();
            PathFigure currentFigure = null;

            int hitCount = 0;

            for (int i = 0; i < xValues.Count; i++)
            {
                Point currentPoint = new Point(xValues[i], canvasSize - yValues[i]);

                if (yValues[i] == canvasSize) // Odd occurrences: Close the path and reset
                {
                    hitCount++;

                    if (hitCount % 2 == 1)
                    {
                        // Add the last point to the current path before switching
                        if (currentFigure != null)
                        {
                            currentFigure.Segments.Add(new LineSegment(currentPoint, true));
                            currentGeometry.Figures.Add(currentFigure);
                            paths.Add(CreatePathFromGeometry(currentGeometry, isControlLine));
                        }

                        // Reset for a new path
                        currentGeometry = new PathGeometry();
                        currentFigure = null;
                    }
                    else // Even occurrences: Start the new path with this point
                    {
                        currentFigure = new PathFigure { StartPoint = currentPoint };
                        currentGeometry.Figures.Add(currentFigure);
                    }
                }
                else // Regular points: Add to the current path normally
                {
                    if (currentFigure == null)
                    {
                        currentFigure = new PathFigure { StartPoint = currentPoint };
                        currentGeometry.Figures.Add(currentFigure);
                    }
                    else
                    {
                        currentFigure.Segments.Add(new LineSegment(currentPoint, true));
                    }
                }
            }

            if (currentFigure != null)
            {
                paths.Add(CreatePathFromGeometry(currentGeometry, isControlLine));
            }

            return paths;
        }

        /// <summary>
        /// Creates a styled Path object from a given PathGeometry.
        /// </summary>
        private static Path CreatePathFromGeometry(PathGeometry geometry, bool isControlLine)
        {
            return new Path
            {
                Data = geometry,
                Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B385F2")),
                StrokeThickness = isControlLine ? 1 : 3,
                StrokeDashArray = isControlLine ? new DoubleCollection { 4, 4 } : null
            };
        }
    }

}
