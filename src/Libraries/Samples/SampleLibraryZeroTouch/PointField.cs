using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using Autodesk.DesignScript.Geometry;

using DynamoServices;

namespace SampleLibraryZeroTouch
{
    // This sample demonstrates the use of the CanUpdatePeriodically
    // attribute. Placing an instance of the PeriodicUpdateExample.PointField 
    // node in your workspace will enable the Periodic RunType in the run
    // settings control. This formula used to create the point field
    // requires a parameter, 't', that is incremented during each run. 
    // This example also demonstrates how to use trace to store data that
    // you want to carry over between evaluations.
    public class  PeriodicUpdateExample : IGraphicItem
    {
        private Point[,] vertexCoords;
        private const int width = 20;
        private const int length = 20;

        private PeriodicUpdateExample(double t, int id)
        {
            vertexCoords = new Point[width, length];

            for (var x = 0; x < width; x += 1)
            {
                for (var y = 0; y < length; y += 1)
                {
                    var z = Math.Sin(Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)) - t);
                    vertexCoords[x, y] = Point.ByCoordinates(x, y, z);
                }
            }

            t += 0.1;
            if (t > Math.PI*2) t = 0.0;

            // Remember to store the updated object in the trace object manager,
            // so it's available to use the next time around.
            TraceableObjectManager.RegisterTraceableObjectForId(id, t);
        }

        // The CanUpdatePeriodicallyAttribute can be applied to methods in 
        // your library which you want to enable the Periodic RunType in Dynamo
        // when exposed as nodes.
        /// <summary>
        /// Create a field of waving points that periodically updates.
        /// </summary>
        /// <returns>A PeriodicUpdateExample object.</returns>
        [CanUpdatePeriodically(true)]
        public static PeriodicUpdateExample PointField()
        {
            // See if the data for this object is in trace.
            var traceId = TraceableObjectManager.GetObjectIdFromTrace();

            var t = 0.0;
            int id;
            if (traceId == null)
            {
                // If there's no id stored in trace for this object,
                // then grab the next unused trace id.
                id = TraceableObjectManager.GetNextUnusedID();
            }
            else
            {
                // If there's and id stored in trace, then retrieve the object stored
                // with that id from the trace object manager.
                id = traceId.IntID;
                t = (double)TraceableObjectManager.GetTracedObjectById(traceId.IntID);
            }

            return new PeriodicUpdateExample(t, id);
        }

        public void Tessellate(IRenderPackage package, double tol = -1, int maxGridLines = 512)
        {
            for (var i = 0; i < width - 1; i++)
            {
                for (var j = 0; j < length - 1; j++)
                {
                    var a = vertexCoords[i , j];
                    var b = vertexCoords[i , j + 1];
                    var c = vertexCoords[i + 1, j];
                    var d = vertexCoords[i + 1, j + 1];

                    var v1 = Vector.ByTwoPoints(b, a).Cross(Vector.ByTwoPoints(c, b)).Normalized().Reverse();
                    var v2 = Vector.ByTwoPoints(c, d).Cross(Vector.ByTwoPoints(b, d)).Normalized().Reverse();

                    PushTriangleVertex(package, a, v1);
                    PushTriangleVertex(package, b, v1);
                    PushTriangleVertex(package, c, v1);

                    PushTriangleVertex(package, d, v2);
                    PushTriangleVertex(package, c, v2);
                    PushTriangleVertex(package, b, v2);
                }
            }
        }

        private void PushTriangleVertex(IRenderPackage package, Point p, Vector n)
        {
            package.AddTriangleVertex(p.X, p.Y, p.Z);
            package.AddTriangleVertexColor(255, 255, 255, 255);
            package.AddTriangleVertexNormal(n.X,n.Y,n.Z);
            package.AddTriangleVertexUV(0,0);
        }
    }
}
