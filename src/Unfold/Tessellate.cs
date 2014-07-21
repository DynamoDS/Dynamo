using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.DSEngine;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;



namespace Unfold
{
    public class Tessellate
    {

        class MeshHelpers
        {
            public static List<List<T>> Split<T>(List<T> source, int subListLength)
            {
                return source
                    .Select((x, i) => new { Index = i, Value = x })
                    .GroupBy(x => x.Index / subListLength)
                    .Select(x => x.Select(v => v.Value).ToList())
                    .ToList();
            }
        }
        public static List<List<Point>> Tesselate(List<Face> faces)
        {
            var surfaces = faces.Select(x => x.SurfaceGeometry()).ToList();

            var result = Tesselate(surfaces);
            return result;

        }

        public static List<List<Point>> Tesselate(List<Surface> surfaces)
        {
            var rp = new RenderPackage();

            foreach (var surface in surfaces)
            {
                surface.Tessellate(rp);
                rp.ItemsCount++;

            }
            //grab double components from rp and subset them into points and further into triangles
            List<List<double>> pointdata = MeshHelpers.Split(rp.TriangleVertices, 3);

            List<Point> points = pointdata.Select(x => Point.ByCoordinates(x[0], x[1], x[2])).ToList();

            List<List<Point>> tris = MeshHelpers.Split(points, 3);

            return tris;



        }

    }
}
