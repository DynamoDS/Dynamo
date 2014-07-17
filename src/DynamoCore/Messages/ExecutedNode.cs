using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Autodesk.DesignScript.Interfaces;

namespace Dynamo.Messages
{
    /// <summary>
    /// The class that represents calculated result for a node
    /// </summary>
    [DataContract]
    public class ExecutedNode
    {
        /// <summary>
        /// The class that represents data for drawing a graphic primitive 
        /// </summary>
        public class Primitive
        {
            /// <summary>
            /// Name of the graphic primitive
            /// </summary>
            [DataMember]
            public string PrimitiveType { get; private set; }

            /// <summary>
            /// Data that is needed for drawing this primitive. For example
            /// coordinates of a point
            /// </summary>
            [DataMember]
            public string PrimitiveData { get; private set; }

            public Primitive(string type, string data)
            {
                PrimitiveType = type;
                PrimitiveData = data;
            }
        }

        /// <summary>
        /// Guid of the specified node
        /// </summary>
        [DataMember]
        public string NodeID { get; private set; }

        /// <summary>
        /// State of the node after executing
        /// </summary>
        [DataMember]
        public string State { get; private set; }

        /// <summary>
        /// State description. It is empty when state has Active or Dead value
        /// </summary>
        [DataMember]
        public string StateMessage { get; private set; }

        /// <summary>
        /// String representing of the result object
        /// </summary>
        [DataMember]
        public string Data { get; private set; }

        /// <summary>
        /// Indicates whether the result object should be drawn on the canvas
        /// </summary>
        [DataMember]
        public bool IsGraphic { get; private set; }

        /// <summary>
        /// List of the graphic primitives that result object consist of.
        /// It is empty for nongraphic objects
        /// </summary>
        [DataMember]
        public List<Primitive> GraphicPrimitives { get; private set; }

        public ExecutedNode(string id, string state, string stateMessage,
            string data, List<IRenderPackage> packages)
        {
            this.NodeID = id;
            this.State = state;
            this.StateMessage = stateMessage;
            this.Data = data;
            GeneratePrimitives(packages);
        }

        private void GeneratePrimitives(List<IRenderPackage> packages)
        {
            IsGraphic = packages != null && packages.Any();

            if (IsGraphic)
            {
                GraphicPrimitives = new List<Primitive>();
                foreach (var package in packages)
                {
                    // Add points
                    GraphicPrimitives.AddRange(GeneratePoints(package.PointVertices));

                    var points = GeneratePoints(package.LineStripVertices);
                    // Add lines
                    GraphicPrimitives.AddRange(ConcatPoints(points, 2, "Line"));

                    points = GeneratePoints(package.TriangleVertices);
                    // Add triangles
                    GraphicPrimitives.AddRange(ConcatPoints(points, 3, "Triangle"));
                }
            }
        }

        private List<Primitive> GeneratePoints(List<double> coordinates)
        {
            if (coordinates == null)
                return null;
            string name = "Point";
            string data;
            var points = new List<Primitive>();
            for (int i = 2; i < coordinates.Count; i += 3)
            {
                data = "(" + coordinates[i - 2] + ";" + coordinates[i - 1] + ";" + coordinates[i] + ")";
                points.Add(new Primitive(name, data));
            }
            return points;
        }

        private List<Primitive> ConcatPoints(List<Primitive> points, int count, string newName)
        {
            string data;
            var concatPoints = new List<Primitive>();
            while (points.Count >= count)
            {
                data = string.Empty;
                for (int j = 0; j < count; j++)
                {
                    data += points[j].PrimitiveData;
                }
                points.RemoveRange(0, count);
                concatPoints.Add(new Primitive(newName, data));
            }
            return concatPoints;
        }
    }
}
