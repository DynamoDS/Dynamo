using System;
using System.Collections;
using Octree.Tools.Vector;

namespace Octree.OctreeSearch
{

    /// <summary> The OctreeNode is the part of the Octree that either holds
    /// branch nodes, or objects as leaves. Currently, the nodes that
    /// have branch do not hold items that span across branch
    /// boundaries, since this was designed to handle point data.
    /// </summary>

    [Serializable]
    public class OctreeNode : IOctree
    {
 
        public const float NO_MIN_SIZE = -1;
        public const float DEFAULT_MIN_SIZE = 5;

        protected internal ArrayList items;
        protected internal OctreeNode[] branch;
        protected internal int maxItems;
        protected internal float minSize;
        public OctreeBox bounds;
        /// <summary> Added to avoid problems when a node is completely filled with a
        /// single point value.
        /// </summary>
        protected internal bool allTheSamePoint;
        protected internal float firstX;
        protected internal float firstY;
        protected internal float firstZ;

        /// <summary> Constructor to use if you are going to store the objects in
        /// x/y space, and there is really no smallest node size.</summary>
        /// <param name="Top">northern border of node coverage.</param>
        /// <param name="Left">western border of node coverage.</param>
        /// <param name="Bottom">southern border of node coverage.</param>
        /// <param name="Right">eastern border of node coverage.</param>
        /// <param name="maximumItems">number of items to hold in a node before
        /// splitting itself into four branch and redispensing the
        /// items into them.</param>
        public OctreeNode(float xMax, float xMin, float yMax, float yMin, float zMax, float zMin, int maximumItems)
            : this(xMax, xMin, yMax, yMin, zMax, zMin, maximumItems, NO_MIN_SIZE)
        {
        }

        /// <summary> Constructor to use if you are going to store the objects in x/y
        /// space, and there is a smallest node size because you don't want
        /// the nodes to be smaller than a group of pixels.</summary>
        /// <param name="Top">northern border of node coverage.</param>
        /// <param name="Left">western border of node coverage.</param>
        /// <param name="Bottom">southern border of node coverage.</param>
        /// <param name="Right">eastern border of node coverage.</param>
        /// <param name="maximumItems">number of items to hold in a node before
        /// splitting itself into four branch and redispensing the items into them.</param>
        /// <param name="minimumSize">the minimum difference between the boundaries of the node.</param>
        public OctreeNode(float xMax, float xMin, float yMax, float yMin, float zMax, float zMin, int maximumItems, float minimumSize)
        {
            bounds = new OctreeBox(xMax, xMin, yMax, yMin, zMax, zMin);
            maxItems = maximumItems;
            minSize = minimumSize;
            items = ArrayList.Synchronized(new ArrayList(10));
        }

        /// <summary>Return true if the node has branch. </summary>
        public bool hasChildren()
        {
            if (branch != null)
                return true;
            else
                return false;
        }

        /// <summary> This method splits the node into four branch, and disperses
        /// the items into the branch. The split only happens if the
        /// boundary size of the node is larger than the minimum size (if
        /// we care). The items in this node are cleared after they are put
        /// into the branch.
        /// </summary>
        protected internal void split()
        {
            // Make sure we're bigger than the minimum, if we care,
            if (minSize != NO_MIN_SIZE)
                if (Math.Abs(bounds.Top - bounds.Bottom) < minSize &&
                    Math.Abs(bounds.Right - bounds.Left) < minSize &&
                    Math.Abs(bounds.Front - bounds.Back) < minSize)
                    return;

            float nsHalf = (float)(bounds.Top - (bounds.Top - bounds.Bottom) * 0.5);
            float ewHalf = (float)(bounds.Right - (bounds.Right - bounds.Left) * 0.5);
            float fbHalf = (float)(bounds.Front - (bounds.Front - bounds.Back) * 0.5);

            branch = new OctreeNode[8];

            branch[0] = new OctreeNode(ewHalf, bounds.Left, bounds.Front, fbHalf, bounds.Top, nsHalf, maxItems); //left-front-top
            branch[1] = new OctreeNode(bounds.Right, ewHalf, bounds.Front, fbHalf, bounds.Top, nsHalf, maxItems);
            branch[2] = new OctreeNode(ewHalf, bounds.Left, bounds.Front, fbHalf, nsHalf, bounds.Bottom, maxItems);
            branch[3] = new OctreeNode(bounds.Right, ewHalf, bounds.Front, fbHalf, nsHalf, bounds.Bottom, maxItems);

            branch[4] = new OctreeNode(ewHalf, bounds.Left, fbHalf, bounds.Back, bounds.Top, nsHalf, maxItems); //left-back-top
            branch[5] = new OctreeNode(bounds.Right, ewHalf, fbHalf, bounds.Back, bounds.Top, nsHalf, maxItems);
            branch[6] = new OctreeNode(ewHalf, bounds.Left, fbHalf, bounds.Back, nsHalf, bounds.Bottom, maxItems);
            branch[7] = new OctreeNode(bounds.Right, ewHalf, fbHalf, bounds.Back, nsHalf, bounds.Bottom, maxItems);

            ArrayList temp = (ArrayList)items.Clone();
            items.Clear();
            IEnumerator things = temp.GetEnumerator();
            while (things.MoveNext())
            {
                AddNode((OctreeLeaf)things.Current);
            }
        }

        /// <summary> Get the node that covers a certain x/y pair.</summary>
        /// <param name="x">up-down location in Octree Grid (x, y)</param>
        /// <param name="y">left-right location in Octree Grid (y, x)</param>
        /// <returns> node if child covers the point, null if the point is
        /// out of range.</returns>
        protected internal OctreeNode getChild(float x, float y, float z)
        {
            if (bounds.pointWithinBounds(x, y, z))
            {
                if (branch != null)
                {
                    for (int i = 0; i < branch.Length; i++)
                        if (branch[i].bounds.pointWithinBounds(x, y, z))
                            return branch[i].getChild(x, y, z);

                }
                else
                    return this;
            }
            return null;
        }

        #region Add Node

        /// <summary> Add a object into the tree at a location.</summary>
        /// <param name="x">up-down location in Octree Grid (x, y)</param>
        /// <param name="y">left-right location in Octree Grid (y, x)</param>
        /// <param name="obj">object to add to the tree.</param>
        /// <returns> true if the pution worked.</returns>
        public bool AddNode(float x, float y, float z, object obj)
        {
            return AddNode(new OctreeLeaf(x, y, z, obj));
        }
        public bool AddNode(float x, float y, float z, int obj)
        {
            return AddNode(new OctreeLeaf(x, y, z, obj));
        }
        public bool AddNode(float x, float y, float z, uint obj)
        {
            return AddNode(new OctreeLeaf(x, y, z, obj));
        }
        public bool AddNode(float x, float y, float z, short obj)
        {
            return AddNode(new OctreeLeaf(x, y, z, obj));
        }
        public bool AddNode(float x, float y, float z, long obj)
        {
            return AddNode(new OctreeLeaf(x, y, z, obj));
        }
        public bool AddNode(float x, float y, float z, float obj)
        {
            return AddNode(new OctreeLeaf(x, y, z, obj));
        }
        public bool AddNode(float x, float y, float z, double obj)
        {
            return AddNode(new OctreeLeaf(x, y, z, obj));
        }
        public bool AddNode(float x, float y, float z, bool obj)
        {
            return AddNode(new OctreeLeaf(x, y, z, obj));
        }

        public bool AddNode(Vector3f vector, object obj)
        {
            return AddNode(new OctreeLeaf(vector.x, vector.y, vector.z, obj));
        }
        public bool AddNode(Vector3f vector, int obj)
        {
            return AddNode(new OctreeLeaf(vector.x, vector.y, vector.z, obj));
        }
        public bool AddNode(Vector3f vector, uint obj)
        {
            return AddNode(new OctreeLeaf(vector.x, vector.y, vector.z, obj));
        }
        public bool AddNode(Vector3f vector, short obj)
        {
            return AddNode(new OctreeLeaf(vector.x, vector.y, vector.z, obj));
        }
        public bool AddNode(Vector3f vector, long obj)
        {
            return AddNode(new OctreeLeaf(vector.x, vector.y, vector.z, obj));
        }
        public bool AddNode(Vector3f vector, float obj)
        {
            return AddNode(new OctreeLeaf(vector.x, vector.y, vector.z, obj));
        }
        public bool AddNode(Vector3f vector, double obj)
        {
            return AddNode(new OctreeLeaf(vector.x, vector.y, vector.z, obj));
        }
        public bool AddNode(Vector3f vector, bool obj)
        {
            return AddNode(new OctreeLeaf(vector.x, vector.y, vector.z, obj));
        }

        public bool AddNode(double x, double y, double z, object obj)
        {
            return AddNode(new OctreeLeaf(x, y, z, obj));
        }
        public bool AddNode(double x, double y, double z, int obj)
        {
            return AddNode(new OctreeLeaf(x, y, z, obj));
        }
        public bool AddNode(double x, double y, double z, uint obj)
        {
            return AddNode(new OctreeLeaf(x, y, z, obj));
        }
        public bool AddNode(double x, double y, double z, short obj)
        {
            return AddNode(new OctreeLeaf(x, y, z, obj));
        }
        public bool AddNode(double x, double y, double z, long obj)
        {
            return AddNode(new OctreeLeaf(x, y, z, obj));
        }
        public bool AddNode(double x, double y, double z, float obj)
        {
            return AddNode(new OctreeLeaf(x, y, z, obj));
        }
        public bool AddNode(double x, double y, double z, double obj)
        {
            return AddNode(new OctreeLeaf(x, y, z, obj));
        }
        public bool AddNode(double x, double y, double z, bool obj)
        {
            return AddNode(new OctreeLeaf(x, y, z, obj));
        }

        public bool AddNode(Vector3d vector, object obj)
        {
            return AddNode(new OctreeLeaf(vector.x, vector.y, vector.z, obj));
        }
        public bool AddNode(Vector3d vector, int obj)
        {
            return AddNode(new OctreeLeaf(vector.x, vector.y, vector.z, obj));
        }
        public bool AddNode(Vector3d vector, uint obj)
        {
            return AddNode(new OctreeLeaf(vector.x, vector.y, vector.z, obj));
        }
        public bool AddNode(Vector3d vector, short obj)
        {
            return AddNode(new OctreeLeaf(vector.x, vector.y, vector.z, obj));
        }
        public bool AddNode(Vector3d vector, long obj)
        {
            return AddNode(new OctreeLeaf(vector.x, vector.y, vector.z, obj));
        }
        public bool AddNode(Vector3d vector, float obj)
        {
            return AddNode(new OctreeLeaf(vector.x, vector.y, vector.z, obj));
        }
        public bool AddNode(Vector3d vector, double obj)
        {
            return AddNode(new OctreeLeaf(vector.x, vector.y, vector.z, obj));
        }
        public bool AddNode(Vector3d vector, bool obj)
        {
            return AddNode(new OctreeLeaf(vector.x, vector.y, vector.z, obj));
        }

        /// <summary> Add a OctreeLeaf into the tree at a location.</summary>
        /// <param name="leaf">object-location composite</param>
        /// <returns> true if the pution worked.</returns>
        public bool AddNode(OctreeLeaf leaf)
        {
            if (branch == null)
            {
                this.items.Add(leaf);
                if (this.items.Count == 1)
                {
                    this.allTheSamePoint = true;
                    this.firstX = leaf.X;
                    this.firstY = leaf.Y;
                    this.firstZ = leaf.Z;
                }
                else
                {
                    if (this.firstX != leaf.X || this.firstY != leaf.Y || this.firstZ != leaf.Z)
                    {
                        this.allTheSamePoint = false;
                    }
                }

                if (this.items.Count > maxItems && !this.allTheSamePoint)
                    split();
                return true;
            }
            else
            {
                OctreeNode node = getChild(leaf.X, leaf.Y, leaf.Z);
                if (node != null)
                {
                    return node.AddNode(leaf);
                }
            }
            return false;
        }

        #endregion

        #region Remove Node

        /// <summary> Remove a object out of the tree at a location.
        /// 
        /// </summary>
        /// <param name="x">up-down location in Octree Grid (x, y)

        /// <param name="y">left-right location in Octree Grid (y, x)

        /// <returns> the object removed, null if the object not found.
        /// </returns>
        public object RemoveNode(float x, float y, float z, object obj)
        {
            return RemoveNode(new OctreeLeaf(x, y, z, obj));
        }
        public object RemoveNode(float x, float y, float z, int obj)
        {
            return RemoveNode(new OctreeLeaf(x, y, z, obj));
        }
        public object RemoveNode(float x, float y, float z, uint obj)
        {
            return RemoveNode(new OctreeLeaf(x, y, z, obj));
        }
        public object RemoveNode(float x, float y, float z, short obj)
        {
            return RemoveNode(new OctreeLeaf(x, y, z, obj));
        }
        public object RemoveNode(float x, float y, float z, long obj)
        {
            return RemoveNode(new OctreeLeaf(x, y, z, obj));
        }
        public object RemoveNode(float x, float y, float z, float obj)
        {
            return RemoveNode(new OctreeLeaf(x, y, z, obj));
        }
        public object RemoveNode(float x, float y, float z, double obj)
        {
            return RemoveNode(new OctreeLeaf(x, y, z, obj));
        }
        public object RemoveNode(float x, float y, float z, bool obj)
        {
            return RemoveNode(new OctreeLeaf(x, y, z, obj));
        }

        public object RemoveNode(Vector3f vector, object obj)
        {
            return RemoveNode(new OctreeLeaf(vector.x, vector.y, vector.z, obj));
        }
        public object RemoveNode(Vector3f vector, int obj)
        {
            return RemoveNode(new OctreeLeaf(vector.x, vector.y, vector.z, obj));
        }
        public object RemoveNode(Vector3f vector, uint obj)
        {
            return RemoveNode(new OctreeLeaf(vector.x, vector.y, vector.z, obj));
        }
        public object RemoveNode(Vector3f vector, short obj)
        {
            return RemoveNode(new OctreeLeaf(vector.x, vector.y, vector.z, obj));
        }
        public object RemoveNode(Vector3f vector, long obj)
        {
            return RemoveNode(new OctreeLeaf(vector.x, vector.y, vector.z, obj));
        }
        public object RemoveNode(Vector3f vector, float obj)
        {
            return RemoveNode(new OctreeLeaf(vector.x, vector.y, vector.z, obj));
        }
        public object RemoveNode(Vector3f vector, double obj)
        {
            return RemoveNode(new OctreeLeaf(vector.x, vector.y, vector.z, obj));
        }
        public object RemoveNode(Vector3f vector, bool obj)
        {
            return RemoveNode(new OctreeLeaf(vector.x, vector.y, vector.z, obj));
        }

        public object RemoveNode(double x, double y, double z, object obj)
        {
            return RemoveNode(new OctreeLeaf(x, y, z, obj));
        }
        public object RemoveNode(double x, double y, double z, int obj)
        {
            return RemoveNode(new OctreeLeaf(x, y, z, obj));
        }
        public object RemoveNode(double x, double y, double z, uint obj)
        {
            return RemoveNode(new OctreeLeaf(x, y, z, obj));
        }
        public object RemoveNode(double x, double y, double z, short obj)
        {
            return RemoveNode(new OctreeLeaf(x, y, z, obj));
        }
        public object RemoveNode(double x, double y, double z, long obj)
        {
            return RemoveNode(new OctreeLeaf(x, y, z, obj));
        }
        public object RemoveNode(double x, double y, double z, float obj)
        {
            return RemoveNode(new OctreeLeaf(x, y, z, obj));
        }
        public object RemoveNode(double x, double y, double z, double obj)
        {
            return RemoveNode(new OctreeLeaf(x, y, z, obj));
        }
        public object RemoveNode(double x, double y, double z, bool obj)
        {
            return RemoveNode(new OctreeLeaf(x, y, z, obj));
        }

        public object RemoveNode(Vector3d vector, object obj)
        {
            return RemoveNode(new OctreeLeaf(vector.x, vector.y, vector.z, obj));
        }
        public object RemoveNode(Vector3d vector, int obj)
        {
            return RemoveNode(new OctreeLeaf(vector.x, vector.y, vector.z, obj));
        }
        public object RemoveNode(Vector3d vector, uint obj)
        {
            return RemoveNode(new OctreeLeaf(vector.x, vector.y, vector.z, obj));
        }
        public object RemoveNode(Vector3d vector, short obj)
        {
            return RemoveNode(new OctreeLeaf(vector.x, vector.y, vector.z, obj));
        }
        public object RemoveNode(Vector3d vector, long obj)
        {
            return RemoveNode(new OctreeLeaf(vector.x, vector.y, vector.z, obj));
        }
        public object RemoveNode(Vector3d vector, float obj)
        {
            return RemoveNode(new OctreeLeaf(vector.x, vector.y, vector.z, obj));
        }
        public object RemoveNode(Vector3d vector, double obj)
        {
            return RemoveNode(new OctreeLeaf(vector.x, vector.y, vector.z, obj));
        }
        public object RemoveNode(Vector3d vector, bool obj)
        {
            return RemoveNode(new OctreeLeaf(vector.x, vector.y, vector.z, obj));
        }

        /// <summary> Remove a OctreeLeaf out of the tree at a location.
        /// 
        /// </summary>
        /// <param name="leaf">object-location composite

        /// <returns> the object removed, null if the object not found.
        /// </returns>
        public object RemoveNode(OctreeLeaf leaf)
        {
            if (branch == null)
            {
                // This must be the node that has it...
                for (int i = 0; i < items.Count; i++)
                {
                    OctreeLeaf qtl = (OctreeLeaf)items[i];
                    if (leaf.LeafObject == qtl.LeafObject)
                    {
                        items.RemoveAt(i);
                        return qtl.LeafObject;
                    }
                }
            }
            else
            {
                OctreeNode node = getChild(leaf.X, leaf.Y, leaf.Z);
                if (node != null)
                {
                    return node.RemoveNode(leaf);
                }
            }
            return null;
        }

        #endregion

        #region Get Node

        /// <summary> Get an object closest to a x/y.</summary>
        /// <param name="x">up-down location in Octree Grid (x, y)</param>
        /// <param name="y">left-right location in Octree Grid (y, x)</param>
        /// <returns> the object that matches the best distance, null if no objects were found. </returns>
        public object GetNode(float x, float y, float z)
        {
            return GetNode(x, y, z, Double.PositiveInfinity);
        }
        public object GetNode(Vector3f vector)
        {
            return GetNode(vector.x, vector.y, vector.z, Double.PositiveInfinity);
        }
        public object GetNode(double x, double y, double z)
        {
            return GetNode(x, y, z, Double.PositiveInfinity);
        }
        public object GetNode(Vector3d vector)
        {
            return GetNode(vector.x, vector.y, vector.z, Double.PositiveInfinity);
        }

        /// <summary> Get an object closest to a x/y/z. If there are branches at
        /// this node, then the branches are searched. The branches are
        /// checked first, to see if they are closer than the best distance
        /// already found. If a closer object is found, bestDistance will
        /// be updated with a new Double object that has the new distance.</summary>
        /// <param name="x">left-right location in Octree Grid</param>
        /// <param name="y">up-down location in Octree Grid</param>
        /// <param name="z">front-nack location in Octree Grid</param>
        /// <param name="bestDistance">the closest distance of the object found so far.</param>
        /// <returns> the object that matches the best distance, null if no closer objects were found.</returns>
        public object GetNode(float x, float y, float z, double ShortestDistance)
        {
            double distance;
            object closest = null;
            if (branch == null)
            {
                foreach (OctreeLeaf leaf in this.items)
                {
                    distance = Math.Sqrt(
                                Math.Pow(x - leaf.X, 2.0) +
                                Math.Pow(y - leaf.Y, 2.0) +
                                Math.Pow(z - leaf.Z, 2.0));

                    if (distance < ShortestDistance)
                    {
                        ShortestDistance = distance;
                        closest = leaf.LeafObject;
                    }
                }
                return closest;
            }
            else
            {
                // Check the distance of the bounds of the branch,
                // versus the bestDistance. If there is a boundary that
                // is closer, then it is possible that another node has an
                // object that is closer.
                for (int i = 0; i < branch.Length; i++)
                {
                    double childDistance = branch[i].bounds.borderDistance(x, y, z);
                    if (childDistance < ShortestDistance)
                    {
                        object test = branch[i].GetNode(x, y, z, ShortestDistance);
                        if (test != null)
                            closest = test;
                    }
                }
            }
            return closest;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="ShortestDistance"></param>
        /// <returns></returns>
        public object GetNode(Vector3f vector, double ShortestDistance)
        {
            return GetNode(vector.x, vector.y, vector.z, ShortestDistance);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="ShortestDistance"></param>
        /// <returns></returns>
        public object GetNode(double x, double y, double z, double ShortestDistance)
        {
            object closest = null;
            double distance;
            if (branch == null)
            {
                foreach (OctreeLeaf leaf in this.items)
                {
                    distance = Math.Sqrt(
                                Math.Pow(x - leaf.X, 2.0) +
                                Math.Pow(y - leaf.Y, 2.0) +
                                Math.Pow(z - leaf.Z, 2.0));

                    if (distance < ShortestDistance)
                    {
                        ShortestDistance = distance;
                        closest = leaf.LeafObject;
                    }
                }
                return closest;
            }
            else
            {
                // Check the distance of the bounds of the branch,
                // versus the bestDistance. If there is a boundary that
                // is closer, then it is possible that another node has an
                // object that is closer.
                for (int i = 0; i < branch.Length; i++)
                {
                    double childDistance = branch[i].bounds.borderDistance(x, y, z);
                    if (childDistance < ShortestDistance)
                    {
                        object test = branch[i].GetNode(x, y, z, ShortestDistance);
                        if (test != null)
                            closest = test;
                    }
                }
            }
            return closest;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="ShortestDistance"></param>
        /// <returns></returns>
        public object GetNode(Vector3d vector, double ShortestDistance)
        {
            return GetNode(vector.x, vector.y, vector.z, ShortestDistance);
        }

        /// <summary> Get all the objects within a bounding box.</summary>
        /// <param name="Top">top location in Octree Grid (x, y)</param>
        /// <param name="Left">left location in Octree Grid (y, x)</param>
        /// <param name="Bottom">lower location in Octree Grid (x, y)</param>
        /// <param name="Right">right location in Octree Grid (y, x)</param>
        /// <returns> Vector of objects. </returns>
        public ArrayList GetNode(float xMax, float xMin, float yMax, float yMin, float zMax, float zMin)
        {
            return GetNode(new OctreeBox(xMax, xMin, yMax, yMin, zMax, zMin), ArrayList.Synchronized(new ArrayList(10)));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="xMax"></param>
        /// <param name="xMin"></param>
        /// <param name="yMax"></param>
        /// <param name="yMin"></param>
        /// <param name="zMax"></param>
        /// <param name="zMin"></param>
        /// <returns></returns>
        public ArrayList GetNode(double xMax, double xMin, double yMax, double yMin, double zMax, double zMin)
        {
            return GetNode(new OctreeBox(xMax, xMin, yMax, yMin, zMax, zMin), ArrayList.Synchronized(new ArrayList(10)));
        }

        /// <summary> Get all the objects within a bounding box.</summary>
        /// <param name="Top">top location in Octree Grid (x, y)</param>
        /// <param name="Left">left location in Octree Grid (y, x)</param>
        /// <param name="Bottom">lower location in Octree Grid (x, y)</param>
        /// <param name="Right">right location in Octree Grid (y, x)</param>
        /// <param name="vector">current vector of objects.</param>
        /// <returns> Vector of objects. </returns>
        public ArrayList GetNode(float xMax, float xMin, float yMax, float yMin, float zMax, float zMin, ArrayList nodes)
        {
            return GetNode(new OctreeBox(xMax, xMin, yMax, yMin, zMax, zMin), nodes);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="xMax"></param>
        /// <param name="xMin"></param>
        /// <param name="yMax"></param>
        /// <param name="yMin"></param>
        /// <param name="zMax"></param>
        /// <param name="zMin"></param>
        /// <param name="nodes"></param>
        /// <returns></returns>
        public ArrayList GetNode(double xMax, double xMin, double yMax, double yMin, double zMax, double zMin, ArrayList nodes)
        {
            return GetNode(new OctreeBox(xMax, xMin, yMax, yMin, zMax, zMin), nodes);
        }
        /// <summary> Get all the objects within a bounding box.</summary>
        /// <param name="rect">boundary of area to fill.</param>
        /// <param name="vector">current vector of objects.</param>
        /// <returns> updated Vector of objects.</returns>
        public ArrayList GetNode(OctreeBox rect, ArrayList nodes)
        {
            if (branch == null)
            {
                IEnumerator things = this.items.GetEnumerator();
                while (things.MoveNext())
                {
                    OctreeLeaf qtl = (OctreeLeaf)things.Current;
                    if (rect.pointWithinBounds(qtl.X, qtl.Y, qtl.Z))
                        nodes.Add(qtl.LeafObject);
                }
            }
            else
            {
                for (int i = 0; i < branch.Length; i++)
                {
                    if (branch[i].bounds.within(rect))
                        branch[i].GetNode(rect, nodes);
                }
            }
            return nodes;
        }
        
        #endregion

        #region Get Nodes
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public ArrayList GetNodes(float x, float y, float z, double radius)
        {
            ArrayList Nodes = new ArrayList();
            double distance;
            if (branch == null)
            {
                foreach (OctreeLeaf leaf in this.items)
                {
                    distance = Math.Sqrt(
                                Math.Pow(x - leaf.X, 2.0) +
                                Math.Pow(y - leaf.Y, 2.0) +
                                Math.Pow(z - leaf.Z, 2.0));

                    if (distance < radius)
                        Nodes.Add(leaf.LeafObject);

                }
                return Nodes;
            }
            else
            {
                // Check the distance of the bounds of the branch,
                // versus the bestDistance. If there is a boundary that
                // is closer, then it is possible that another node has an
                // object that is closer.
                for (int i = 0; i < branch.Length; i++)
                {
                    double childDistance = branch[i].bounds.borderDistance(x, y, z);

                    if (childDistance < radius)
                    {
                        object test = branch[i].GetNode(x, y, z, radius);
                        if (test != null)
                            Nodes.Add(test);
                    }

                }
            }
            return Nodes;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public ArrayList GetNodes(Vector3f vector, double radius)
        {
            return GetNodes(vector.x, vector.y, vector.z, radius);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public ArrayList GetNodes(double x, double y, double z, double radius)
        {
            ArrayList Nodes = new ArrayList();
            double distance;
            if (branch == null)
            {
                foreach (OctreeLeaf leaf in this.items)
                {
                    distance = Math.Sqrt(
                                Math.Pow(x - leaf.X, 2.0) +
                                Math.Pow(y - leaf.Y, 2.0) +
                                Math.Pow(z - leaf.Z, 2.0));

                    if (distance < radius)
                        Nodes.Add(leaf.LeafObject);

                }
                return Nodes;
            }
            else
            {
                // Check the distance of the bounds of the branch,
                // versus the bestDistance. If there is a boundary that
                // is closer, then it is possible that another node has an
                // object that is closer.
                for (int i = 0; i < branch.Length; i++)
                {
                    double childDistance = branch[i].bounds.borderDistance(x, y, z);

                    if (childDistance < radius)
                    {
                        object test = branch[i].GetNode(x, y, z, radius);
                        if (test != null)
                            Nodes.Add(test);
                    }

                }
            }
            return Nodes;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public ArrayList GetNodes(Vector3d vector, double radius)
        {
            return GetNodes(vector.x, vector.y, vector.z, radius);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="MinRadius"></param>
        /// <param name="MaxRadius"></param>
        /// <returns></returns>
        public ArrayList GetNodes(float x, float y, float z, double MinRadius, double MaxRadius)
        {
            ArrayList Nodes = new ArrayList();
            if (branch == null)
            {
                foreach (OctreeLeaf leaf in this.items)
                {
                    double distance = Math.Sqrt(
                                Math.Pow(x - leaf.X, 2.0) +
                                Math.Pow(y - leaf.Y, 2.0) +
                                Math.Pow(z - leaf.Z, 2.0));

                    if (distance >= MinRadius && distance < MaxRadius)
                    {
                        //if (distance <= minDistance) //closest object first
                        //{
                       //     Nodes.Insert(0, leaf.LeafObject);
                       //     minDistance = Math.Min(minDistance, distance);
                       // }
                       // else
                            Nodes.Add(leaf.LeafObject);
                    }

                }
                return Nodes;
            }
            else
            {
                // Check the distance of the bounds of the branch,
                // versus the bestDistance. If there is a boundary that
                // is closer, then it is possible that another node has an
                // object that is closer.
                for (int i = 0; i < branch.Length; i++)
                {
                    double childDistance = branch[i].bounds.borderDistance(x, y, z);

                    if (childDistance > MinRadius && childDistance <= MaxRadius)
                    {
                        object test = branch[i].GetNode(x, y, z, MinRadius);
                        if (test != null)
                            Nodes.Add(test);
                    }

                }
            }
            return Nodes;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="MinRadius"></param>
        /// <param name="MaxRadius"></param>
        /// <returns></returns>
        public ArrayList GetNodes(Vector3f vector, double MinRadius, double MaxRadius)
        {
            return GetNodes(vector.x, vector.y, vector.z, MinRadius, MaxRadius);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="MinRadius"></param>
        /// <param name="MaxRadius"></param>
        /// <returns></returns>
        public ArrayList GetNodes(double x, double y, double z, double MinRadius, double MaxRadius)
        {
            ArrayList Nodes = new ArrayList();
            if (branch == null)
            {
                foreach (OctreeLeaf leaf in this.items)
                {
                    double distance = Math.Sqrt(
                                Math.Pow(x - leaf.X, 2.0) +
                                Math.Pow(y - leaf.Y, 2.0) +
                                Math.Pow(z - leaf.Z, 2.0));

                    if (distance >= MinRadius && distance < MaxRadius)
                    {
                       // if (distance <= minDistance) //closest object first
                       // {
                       //     Nodes.Insert(0, leaf.LeafObject);
                       //     minDistance = Math.Min(minDistance, distance);
                       // }
                       // else
                            Nodes.Add(leaf.LeafObject);
                    }
                    
                }
                return Nodes;
            }
            else
            {
                // Check the distance of the bounds of the branch,
                // versus the bestDistance. If there is a boundary that
                // is closer, then it is possible that another node has an
                // object that is closer.
                for (int i = 0; i < branch.Length; i++)
                {
                    double childDistance = branch[i].bounds.borderDistance(x, y, z);

                    if (childDistance >= MinRadius && childDistance < MaxRadius)
                    {
                        object test = branch[i].GetNode(x, y, z, MaxRadius);
                        if (test != null)
                        {
                            Nodes.Add(test);
                        }
                    }

                }
            }
            return Nodes;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="MinRadius"></param>
        /// <param name="MaxRadius"></param>
        /// <returns></returns>
        public ArrayList GetNodes(Vector3d vector, double MinRadius, double MaxRadius)
        {
            return GetNodes(vector.x, vector.y, vector.z, MinRadius, MaxRadius);
        }

        #endregion

        /// <summary>Clear the tree below this node. </summary>
        public void Clear()
        {
            this.items.Clear();
            if (branch != null)
            {
                for (int i = 0; i < branch.Length; i++)
                {
                    branch[i].Clear();
                }
                branch = null;
            }
        }

    }
}