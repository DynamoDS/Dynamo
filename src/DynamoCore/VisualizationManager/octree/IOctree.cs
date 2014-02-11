using System.Collections;
using Octree.Tools.Vector;

namespace Octree.OctreeSearch
{
    /// <summary>
    /// This interface organizes the access to Octree class.
    /// </summary>
    public interface IOctree
    {

        /// <summary> Add a object into the organizer at a location.</summary>
        /// <param name="x">up-down location x/y/z</param>
        /// <param name="y">left-right location x/y/z</param>
        /// <param name="z">front-back location x/y/z</param>
        /// <returns> true if the insertion worked. </returns>
        bool AddNode(float x, float y, float z, object obj);
        bool AddNode(float x, float y, float z, int obj);
        bool AddNode(float x, float y, float z, uint obj);
        bool AddNode(float x, float y, float z, short obj);
        bool AddNode(float x, float y, float z, long obj);
        bool AddNode(float x, float y, float z, float obj);
        bool AddNode(float x, float y, float z, double obj);
        bool AddNode(float x, float y, float z, bool obj);

        bool AddNode(Vector3f vector, object obj);
        bool AddNode(Vector3f vector, int obj);
        bool AddNode(Vector3f vector, uint obj);
        bool AddNode(Vector3f vector, short obj);
        bool AddNode(Vector3f vector, long obj);
        bool AddNode(Vector3f vector, float obj);
        bool AddNode(Vector3f vector, double obj);
        bool AddNode(Vector3f vector, bool obj);

        bool AddNode(double x, double y, double z, object obj);
        bool AddNode(double x, double y, double z, int obj);
        bool AddNode(double x, double y, double z, uint obj);
        bool AddNode(double x, double y, double z, short obj);
        bool AddNode(double x, double y, double z, long obj);
        bool AddNode(double x, double y, double z, float obj);
        bool AddNode(double x, double y, double z, double obj);
        bool AddNode(double x, double y, double z, bool obj);

        bool AddNode(Vector3d vector, object obj);
        bool AddNode(Vector3d vector, int obj);
        bool AddNode(Vector3d vector, uint obj);
        bool AddNode(Vector3d vector, short obj);
        bool AddNode(Vector3d vector, long obj);
        bool AddNode(Vector3d vector, float obj);
        bool AddNode(Vector3d vector, double obj);
        bool AddNode(Vector3d vector, bool obj);

        /// <summary> Remove a object out of the organizer at a location.</summary>
        /// <param name="x">up-down location (x, y)</param>
        /// <param name="y">left-right location (y, x)</param>
        /// <returns> the object removed, null if the object not found.</returns>
        object RemoveNode(float x, float y, float z, object obj);
        object RemoveNode(float x, float y, float z, int obj);
        object RemoveNode(float x, float y, float z, uint obj);
        object RemoveNode(float x, float y, float z, short obj);
        object RemoveNode(float x, float y, float z, long obj);
        object RemoveNode(float x, float y, float z, float obj);
        object RemoveNode(float x, float y, float z, double obj);
        object RemoveNode(float x, float y, float z, bool obj);

        object RemoveNode(Vector3f vector, object obj);
        object RemoveNode(Vector3f vector, int obj);
        object RemoveNode(Vector3f vector, uint obj);
        object RemoveNode(Vector3f vector, short obj);
        object RemoveNode(Vector3f vector, long obj);
        object RemoveNode(Vector3f vector, float obj);
        object RemoveNode(Vector3f vector, double obj);
        object RemoveNode(Vector3f vector, bool obj);

        object RemoveNode(double x, double y, double z, object obj);
        object RemoveNode(double x, double y, double z, int obj);
        object RemoveNode(double x, double y, double z, uint obj);
        object RemoveNode(double x, double y, double z, short obj);
        object RemoveNode(double x, double y, double z, long obj);
        object RemoveNode(double x, double y, double z, float obj);
        object RemoveNode(double x, double y, double z, double obj);
        object RemoveNode(double x, double y, double z, bool obj);

        object RemoveNode(Vector3d vector, object obj);
        object RemoveNode(Vector3d vector, int obj);
        object RemoveNode(Vector3d vector, uint obj);
        object RemoveNode(Vector3d vector, short obj);
        object RemoveNode(Vector3d vector, long obj);
        object RemoveNode(Vector3d vector, float obj);
        object RemoveNode(Vector3d vector, double obj);
        object RemoveNode(Vector3d vector, bool obj);

        /// <summary>Clear the octree.</summary>
        void Clear();

        /// <summary> Find an object closest to point x/y/z.</summary>
        /// <param name="x">up-down location in Octree grid</param>
        /// <param name="y">left-right location in Octree grid</param>
        /// <param name="z">front-back location in Octree grid</param>
        /// <returns> the object that is closest to point x/y/z.</returns>
        object GetNode(float x, float y, float z);
        object GetNode(Vector3f vector);
        object GetNode(double x, double y, double z);
        object GetNode(Vector3d vector);

        /// <summary> Find an object closest to point x/y/z within a distance.</summary>
        /// <param name="x">up-down location in Octree grid</param>
        /// <param name="y">left-right location in Octree grid</param>
        /// <param name="z">front-back location in Octree grid</param>
        /// <param name="withinDistance">maximum distance to have a hit.</param>
        /// <returns> the object that is closest to the x/y/z, within the given distance.</returns>
        object GetNode(float x, float y, float z, double withinDistance);
        object GetNode(Vector3f vector, double withinDistance);
        object GetNode(double x, double y, double z, double withinDistance);
        object GetNode(Vector3d vector, double withinDistance);

        /// <summary> Find an set of objects closest to point x/y/z within a given radius.</summary>
        /// <param name="x">up-down location in Octree grid</param>
        /// <param name="y">left-right location in Octree grid</param>
        /// <param name="z">front-back location in Octree grid</param>
        /// <param name="radius">search radius</param>
        /// <returns> the object that is closest to point x/y/z, within the given distance.</returns>
        ArrayList GetNodes(float x, float y, float z, double radius);
        ArrayList GetNodes(Vector3f vector, double radius);
        ArrayList GetNodes(double x, double y, double z, double radius);
        ArrayList GetNodes(Vector3d vector, double radius);

        ArrayList GetNodes(float x, float y, float z, double MinRadius, double MaxRadius);
        ArrayList GetNodes(Vector3f vector, double MinRadius, double MaxRadius);
        ArrayList GetNodes(double x, double y, double z, double MinRadius, double MaxRadius);
        ArrayList GetNodes(Vector3d vector, double MinRadius, double MaxRadius);

        // <summary> Find an object closest to point x/y/z, within a cube.</summary>
        ArrayList GetNode(float xMax, float xMin, float yMax, float yMin, float zMax, float zMin);
    }
}