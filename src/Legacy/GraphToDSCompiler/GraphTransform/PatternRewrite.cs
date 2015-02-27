using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using ProtoCore.Utils;


namespace GraphToDSCompiler
{
    /// <summary>
    /// An inteface class used by GraphTransform to apply pattern rewrites
    /// New pattern classes must derive from this interface
    /// </summary>
    public interface IPatternRewrite
    {
        List<SnapshotNode> Rewrite(List<SnapshotNode> graph);
        List<SnapshotNode> RewriteFromPattern(List<SnapshotNode> graph);
    }

    /// <summary>
    /// The pattern rewriter that transforms a graph into the final form
    /// </summary>
    class IntermediateFormRewrite : IPatternRewrite
    {
        public List<SnapshotNode> Rewrite(List<SnapshotNode> graph)
        {
            Validity.Assert(graph != null);
            return graph = RewriteFromPattern(graph);
        }

        /// <summary>
        /// Given a graph, rewrite is given the heuristics of this pattern class
        /// </summary>
        /// <param name="graph"></param>
        /// <returns></returns>
        public List<SnapshotNode> RewriteFromPattern(List<SnapshotNode> graph)
        {
            return graph;
        }
    }
    class DegreeToRadianRewrite : IPatternRewrite
    {
        public List<SnapshotNode> Rewrite(List<SnapshotNode> graph)
        {
            Validity.Assert(graph != null);
            return graph = RewriteFromPattern(graph);
        }

        /// <summary>
        /// Given a graph, rewrite is given the heuristics of this pattern class
        /// </summary>
        /// <param name="graph"></param>
        /// <returns></returns>
        public List<SnapshotNode> RewriteFromPattern(List<SnapshotNode> graph)
        {
            List<SnapshotNode> n=new List<SnapshotNode>();
            foreach (SnapshotNode c in graph)
                                n.Add(c);
            foreach (SnapshotNode node in n)
            {
                if (node.Type == SnapshotNodeType.Function)
                {
                    string[] functionQualifers = node.Content.Split(';');
                    switch (functionQualifers[1])
                    {
                        case "Math.Sin":
                        case "Math.Cos":
                        case "Math.Tan":
                            SnapshotNode ssn = new SnapshotNode((uint)(node.Id * 100), node.Type, "Math.dll;Math.RadiansToDegrees; ");
                            List<Connection> inp = new List<Connection>();
                            foreach (Connection c in node.InputList)
                                inp.Add(c);
                            ssn.InputList = inp;
                            ssn.OutputList = new List<Connection>();
                            graph.Add(ssn);
                            Connection connection = new Connection();
                            connection.OtherNode = ssn.Id;
                            connection.LocalIndex = 0;
                            connection.OtherIndex = 0;
                            int num = graph.IndexOf(node);
                            graph[num].InputList.Clear();
                            graph[num].InputList.Add(connection);
                            break;
                        default: break;
                    }
                }
            }
            return graph;
        }
    }
    class ThreePointCircleRewrite : IPatternRewrite
    {
        public List<SnapshotNode> Rewrite(List<SnapshotNode> graph)
        {
            Validity.Assert(graph != null);
            return graph = RewriteFromPattern(graph);
        }

        /// <summary>
        /// Given a graph, rewrite is given the heuristics of this pattern class
        /// </summary>
        /// <param name="graph"></param>
        /// <returns></returns>
        public List<SnapshotNode> RewriteFromPattern(List<SnapshotNode> graph)
        {
            List<SnapshotNode> n = new List<SnapshotNode>();
            foreach (SnapshotNode c in graph)
                n.Add(c);
            foreach (SnapshotNode node in n)
            {
                if (node.Type == SnapshotNodeType.Function)
                {
                    string[] functionQualifers = node.Content.Split(';');
                    switch (functionQualifers[1])
                    {
                        case "CircleBy3Points":
                            SnapshotNode funcWrap = new SnapshotNode((uint)node.Id * 101, SnapshotNodeType.CodeBlock, "def CircleBy3Points(p1, p2, p3) \n{\n    temp1 = p3.X - p1.X;\n    temp2 = p3.Y - p1.Y;\n    temp3 = p3.Z - p1.Z;\n    temp4 = p2.X - p1.X;\n    temp5 = p2.Y - p1.Y;\n    temp6 = p2.Z - p1.Z;\n    temp7 = Vector.ByCoordinates(temp1, temp2, temp3);\n    temp8 = Vector.ByCoordinates(temp4, temp5, temp6);\n    centerX = (p1.X + p2.X + p3.X) / 3;\n    centerY = (p1.Y + p2.Y + p3.Y) / 3;\n    centerZ = (p1.Z + p2.Z + p3.Z) / 3;\n    centre = Point.ByCoordinates(centerX, centerY, centerZ);\n    radius = centre.DistanceTo(p1);return = Circle.ByCenterPointRadius(centre, radius,temp8.Cross(temp7));\n}");
                            graph.Add(funcWrap);
                            break;
                        default: break;
                    }
                }
            }
            return graph;
        }
    }
    class ThreePointToPlaneRewrite : IPatternRewrite
    {
        public List<SnapshotNode> Rewrite(List<SnapshotNode> graph)
        {
            Validity.Assert(graph != null);
            return graph = RewriteFromPattern(graph);
        }

        /// <summary>
        /// Given a graph, rewrite is given the heuristics of this pattern class
        /// </summary>
        /// <param name="graph"></param>
        /// <returns></returns>
        public List<SnapshotNode> RewriteFromPattern(List<SnapshotNode> graph)
        {
            List<SnapshotNode> n = new List<SnapshotNode>();
            foreach (SnapshotNode c in graph)
                n.Add(c);
            foreach (SnapshotNode node in n)
            {
                if (node.Type == SnapshotNodeType.Function)
                {
                    string[] functionQualifers = node.Content.Split(';');
                    switch (functionQualifers[1])
                    {
                        case "PlaneBy3Points":
                            SnapshotNode funcWrap = new SnapshotNode((uint)node.Id * 101, SnapshotNodeType.CodeBlock, "def PlaneBy3Points(p1, p2, p3) \n{\n    temp1 = p3.X - p1.X;\n    temp2 = p3.Y - p1.Y;\n    temp3 = p3.Z - p1.Z;\n    temp4 = p2.X - p1.X;\n    temp5 = p2.Y - p1.Y;\n    temp6 = p2.Z - p1.Z;\n    temp7 = Vector.ByCoordinates(temp1, temp2, temp3);\n    temp8 = Vector.ByCoordinates(temp4, temp5, temp6);\n    return = Plane.ByOriginNormal(p1,temp8.Cross(temp7));\n}");
                            graph.Add(funcWrap);
                            /*List<Connection> inp = new List<Connection>();
                            foreach (Connection c in node.InputList)
                                inp.Add(c);
                            SnapshotNode ssn = new SnapshotNode((uint)(node.Id), node.Type, ";PlaneBy3Points; ");
                            ssn.InputList = inp;
                            ssn.OutputList = new List<Connection>();
                            int num = graph.IndexOf(node);
                            graph[num] = ssn;*/
                            //node.Content=";PlaneBy3Points;";
                            /*
                            List<uint> pointsl=new List<uint>();
                            foreach(Connection c in node.InputList)
                                pointsl.Add(c.OtherNode);
                            SnapshotNode p1 = n.Find(x => x.Id == pointsl[0]);
                            List<uint> p1cords = new List<uint>();
                            foreach (Connection c in p1.InputList)
                                p1cords.Add(c.OtherNode);
                            SnapshotNode p1x = n.Find(x => x.Id == p1cords[0]);
                            SnapshotNode p1y = n.Find(x => x.Id == p1cords[1]);
                            SnapshotNode p1z = n.Find(x => x.Id == p1cords[2]);
                            SnapshotNode p2 = n.Find(x => x.Id == pointsl[1]);
                            List<uint> p2cords = new List<uint>();
                            foreach (Connection c in p2.InputList)
                                p2cords.Add(c.OtherNode);
                            SnapshotNode p2x = n.Find(x => x.Id == p1cords[0]);
                            SnapshotNode p2y = n.Find(x => x.Id == p1cords[1]);
                            SnapshotNode p2z = n.Find(x => x.Id == p1cords[2]);
                            SnapshotNode p3 = n.Find(x => x.Id == pointsl[2]);
                            List<uint> p3cords = new List<uint>();
                            foreach (Connection c in p3.InputList)
                                p3cords.Add(c.OtherNode);
                            SnapshotNode p3x = n.Find(x => x.Id == p1cords[0]);
                            SnapshotNode p3y = n.Find(x => x.Id == p1cords[1]);
                            SnapshotNode p3z = n.Find(x => x.Id == p1cords[2]);
                            SnapshotNode vector1x = new SnapshotNode((uint)((Math.Pow(p3x.Id,p1x.Id)) * 10), node.Type, ";-; ");
                            vector1x.OutputList = new List<Connection>();
                            vector1x.InputList = new List<Connection>();
                            Connection vector1x1 = new Connection();
                            vector1x1.LocalIndex = 0;
                            vector1x1.OtherIndex = 0;
                            vector1x1.OtherNode = p3x.Id;
                            Connection vector1x2 = new Connection();
                            vector1x2.LocalIndex = 1;
                            vector1x2.OtherIndex = 0;
                            vector1x2.OtherNode = p1x.Id;
                            vector1x.InputList.Add(vector1x1);
                            vector1x.InputList.Add(vector1x2);
                            graph.Add(vector1x);
                            SnapshotNode vector1y = new SnapshotNode((uint)((Math.Pow(p3y.Id, p1y.Id)) * 10), node.Type, ";-; ");
                            vector1y.OutputList = new List<Connection>();
                            vector1y.InputList = new List<Connection>();
                            Connection vector1y1 = new Connection();
                            vector1y1.LocalIndex = 0;
                            vector1y1.OtherIndex = 0;
                            vector1y1.OtherNode = p3y.Id;
                            Connection vector1y2 = new Connection();
                            vector1y2.LocalIndex = 1;
                            vector1y2.OtherIndex = 0;
                            vector1y2.OtherNode = p1y.Id;
                            vector1y.InputList.Add(vector1y1);
                            vector1y.InputList.Add(vector1y2);
                            graph.Add(vector1y);
                            SnapshotNode vector1z = new SnapshotNode((uint)((Math.Pow(p3z.Id, p1z.Id)) * 10), node.Type, ";-; ");
                            vector1z.OutputList = new List<Connection>();
                            vector1z.InputList = new List<Connection>();
                            Connection vector1z1 = new Connection();
                            vector1z1.LocalIndex = 0;
                            vector1z1.OtherIndex = 0;
                            vector1z1.OtherNode = p3z.Id;
                            Connection vector1z2 = new Connection();
                            vector1z2.LocalIndex = 1;
                            vector1z2.OtherIndex = 0;
                            vector1z2.OtherNode = p1z.Id;
                            vector1z.InputList.Add(vector1z1);
                            vector1z.InputList.Add(vector1z2);
                            graph.Add(vector1z);
                            SnapshotNode vector2x = new SnapshotNode((uint)((Math.Pow(p2x.Id,p1x.Id)) * 10), node.Type, ";-; ");
                            vector2x.OutputList = new List<Connection>();
                            vector2x.InputList = new List<Connection>();
                            Connection vector2x1 = new Connection();
                            vector2x1.LocalIndex = 0;
                            vector2x1.OtherIndex = 0;
                            vector2x1.OtherNode = p2x.Id;
                            Connection vector2x2 = new Connection();
                            vector2x2.LocalIndex = 1;
                            vector2x2.OtherIndex = 0;
                            vector2x2.OtherNode = p1x.Id;
                            vector2x.InputList.Add(vector2x1);
                            vector2x.InputList.Add(vector2x2);
                            graph.Add(vector2x);
                            SnapshotNode vector2y = new SnapshotNode((uint)((Math.Pow(p2y.Id, p1y.Id)) * 10), node.Type, ";-; ");
                            vector2y.OutputList = new List<Connection>();
                            vector2y.InputList = new List<Connection>();
                            Connection vector2y1 = new Connection();
                            vector2y1.LocalIndex = 0;
                            vector2y1.OtherIndex = 0;
                            vector2y1.OtherNode = p2y.Id;
                            Connection vector2y2 = new Connection();
                            vector2y2.LocalIndex = 1;
                            vector2y2.OtherIndex = 0;
                            vector2y2.OtherNode = p1y.Id;
                            vector2y.InputList.Add(vector2y1);
                            vector2y.InputList.Add(vector2y2);
                            graph.Add(vector2y);
                            SnapshotNode vector2z = new SnapshotNode((uint)((Math.Pow(p2z.Id, p1z.Id)) * 10), node.Type, ";-; ");
                            vector2z.OutputList = new List<Connection>();
                            vector2z.InputList = new List<Connection>();
                            Connection vector2z1 = new Connection();
                            vector2z1.LocalIndex = 0;
                            vector2z1.OtherIndex = 0;
                            vector2z1.OtherNode = p2z.Id;
                            Connection vector2z2 = new Connection();
                            vector2z2.LocalIndex = 1;
                            vector2z2.OtherIndex = 0;
                            vector2z2.OtherNode = p1z.Id;
                            vector2z.InputList.Add(vector2z1);
                            vector2z.InputList.Add(vector2z2);
                            graph.Add(vector2z);*/
                            ;
                            break;
                        default: break;
                    }
                }
            }
            return graph;
        }
    }
}
