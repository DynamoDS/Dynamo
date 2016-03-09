﻿using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.ZeroTouch;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace Dynamo.Manipulation
{
    public sealed class MousePointManipulatorFactory : INodeManipulatorFactory
    {
        public INodeManipulator Create(NodeModel node, DynamoManipulationExtension manipulatorContext)
        {
            return new MousePointManipulator(node as DSFunction, manipulatorContext);
        }
    }

    public class MousePointManipulator : NodeManipulator
    {
        private Point origin;
        internal override Point Origin { get { return origin; } }

        private TranslationGizmo gizmo;

        private Dictionary<int, Tuple<Vector, NodeModel>> indexedAxisNodePairs;

        internal MousePointManipulator(DSFunction node, DynamoManipulationExtension manipulatorContext)
            : base(node, manipulatorContext)
        {
        }
       
        #region overridden methods

        protected override void AssignInputNodes()
        {
            //Default axes
            var axes = new Vector[] { Vector.XAxis(), Vector.YAxis(), Vector.ZAxis() };
            //Holds manipulator axis and input node pair for each input port.
            indexedAxisNodePairs = new Dictionary<int, Tuple<Vector, NodeModel>>(3);

            for (int i = 0; i < 3; i++)
            {
                //First find out if the input can be manipulated.
                NodeModel node = null;
                if (!CanManipulateInputNode(i, out node))
                {
                    continue;
                }

                //Input can be manipulated but there is not input node yet.
                if (node == null)
                {
                    indexedAxisNodePairs.Add(i, Tuple.Create(axes[i], node));
                    continue;
                }

                //Now check if the input node is already connected to other
                //input of this node, then update the manipulation direction.
                Vector axis = null;
                int idx = -1;
                foreach (var item in indexedAxisNodePairs)
                {
                    //If same node is connected to more than one input port.
                    if (item.Value.Item2 == node)
                    {
                        //Combine old axis with this axis
                        axis = item.Value.Item1;
                        axis = axis.Add(axes[i]);
                        idx = item.Key;
                        break;
                    }
                }
                if (axis == null) //Didn't find matching node.
                {
                    indexedAxisNodePairs.Add(i, Tuple.Create(axes[i], node));
                }
                else
                {
                    //update the new axis value in dictionary
                    indexedAxisNodePairs[idx] = Tuple.Create(axis, node);
                }
            }
            // Normalize all axes in indexedAxisNodePairs
            for (int i = 0; i < 3; i++)
            {
                Tuple<Vector, NodeModel> pair;
                if (indexedAxisNodePairs.TryGetValue(i, out pair))
                {
                    indexedAxisNodePairs[i] = Tuple.Create(pair.Item1.Normalized(), pair.Item2);
                }
            }
        }

        /// <summary>
        /// Returns all the gizmos supported by this manipulator
        /// </summary>
        /// <param name="createOrUpdate">
        /// If true: Create a new gizmo or update a gizmo if already present.
        /// If false: Query for existing gizmos</param>
        /// <returns>List of Gizmo</returns>
        protected override IEnumerable<IGizmo> GetGizmos(bool createOrUpdate)
        {
            //Don't create a new gizmo if not requested
            if (gizmo == null && !createOrUpdate)
                yield break;

            //No axis data, so no gizmo.
            if (!indexedAxisNodePairs.Any())
                yield break;

            if (createOrUpdate)
            {
                UpdateGizmo();
            }

            yield return gizmo;
        }

        /// <summary>
        /// Called when Gizmo is clicked. Creates new input nodes if the
        /// specific input is selected for manipulation by the Gizmo.
        /// </summary>
        /// <param name="gizmoInAction">Gizmo that is clicked</param>
        /// <param name="hitObject">The axis or plane of the gizmo hit</param>
        protected override IEnumerable<NodeModel> OnGizmoClick(IGizmo gizmoInAction, object hitObject)
        {
            //If an axis is hit, only one node will be updated.
            var axis1 = hitObject as Vector;
            Vector axis2 = null;
            if(axis1 == null)
            {
                //Hit object is a plane, two axes will be updated simultaneously.
                var plane = hitObject as Plane;
                if(plane != null)
                {
                    axis1 = plane.XAxis;
                    axis2 = plane.YAxis;
                }
            }

            var nodes = new Dictionary<int, NodeModel>(2); //placeholder for new nodes.
            foreach (var item in indexedAxisNodePairs)
            {
                var v = item.Value.Item1;
                var node = item.Value.Item2;
                if (v.Equals(axis1) || v.Equals(axis2))
                {
                    if (node == null)
                    {
                        node = CreateAndConnectInputNode(0, item.Key);
                    }
                    
                    nodes.Add(item.Key, node);
                }
            }

            // Update the axisNodePairs with affected nodes.
            foreach (var n in nodes)
            {
                var axisIndex = n.Key;
                var axisNodePair = indexedAxisNodePairs[axisIndex];
                var axisVector = axisNodePair.Item1;
                var upstreamNode = n.Value;
                indexedAxisNodePairs[axisIndex] = Tuple.Create(axisVector, upstreamNode);
            }

            return nodes.Values;
        }

        /// <summary>
        /// Callback method when gizmo is moved by user action.
        /// </summary>
        /// <param name="gizmoInAction">Gizmo that moved.</param>
        /// <param name="offset">Offset by which the gizmo has moved.</param>
        protected override void OnGizmoMoved(IGizmo gizmoInAction, Vector offset)
        {
            var offsetPos = origin.Add(offset);
            origin.Dispose();
            origin = offsetPos;

            foreach (var item in indexedAxisNodePairs)
            {
                // When more than one input is connected to the same slider, this
                // method will decompose the axis corresponding to each input.
                using (var v = GetFirstAxisComponent(item.Value.Item1))
                {
                    var amount = offset.Dot(v);

                    if (Math.Abs(amount) > 0.001)
                    {
                        ModifyInputNode(item.Value.Item2, amount);
                    }
                }
            }
        }

        /// <summary>
        /// Synchronize the manipulator position with the node's value.
        /// </summary>
        protected override bool UpdatePosition()
        {
            if (Node == null || !indexedAxisNodePairs.Any()) return false;

            if (origin == null)
            {
                origin = Point.Origin(); //First time initialization
            }

            //Node output could be a collection, consider the first item as origin.
            Point pt = GetFirstValueFromNode(Node) as Point;
            if (null == pt) return false; //The node output is not Point, could be a function object.

            //Don't cache pt directly here, we need to create a copy, because 
            //pt may be GC'ed by VM.
            origin = Point.ByCoordinates(pt.X, pt.Y, pt.Z);
            
            return true;
        }

        #endregion

        #region helpers

        /// <summary>
        /// Creates a new Gizmo or Updates existing Gizmo with new axes and origin.
        /// This method is called every time Gizmo's are requested.
        /// </summary>
        private void UpdateGizmo()
        {
            var axes = new Vector[] { null, null, null };
            //Extract axis information from the axis node pairs.
            int index = 0;
            foreach (var item in indexedAxisNodePairs)
            {
                axes[index++] = item.Value.Item1;
            }

            if (null == gizmo)
            {
                gizmo = new TranslationGizmo(this, axes[0], axes[1], axes[2], gizmoScale);
            }
            else
            {
                gizmo.UpdateGeometry(axes[0], axes[1], axes[2], gizmoScale);
            }
        }


        /// <summary>
        /// Decomposes given vector in natural axes and returns first axis.
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        private Vector GetFirstAxisComponent(Vector vector)
        {
            var tol = 0.0001;
            var v1 = Vector.ByCoordinates(vector.X, 0, 0);
            if (v1.Length > tol)
                return v1.Normalized();

            var v2 = Vector.ByCoordinates(0, vector.Y, 0);
            if (v2.Length > tol)
                return v2.Normalized();

            return vector.Normalized();
        }

        /// <summary>
        /// Updates input node by specified amount.
        /// </summary>
        /// <param name="inputNode">Input node</param>
        /// <param name="amount">Amount by which it needs to be modified.</param>
        private static void ModifyInputNode(NodeModel inputNode, double amount)
        {
            if (inputNode == null) return;

            if (Math.Abs(amount) < 0.001) return;

            dynamic uiNode = inputNode;

            uiNode.Value = Math.Round(uiNode.Value + amount, 3);
        }

        #endregion
    }
}
