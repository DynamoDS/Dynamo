namespace MIConvexHull
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Simple interface to unify different types of triangulations in the future.
    /// </summary>
    /// <typeparam name="TVertex"></typeparam>
    /// <typeparam name="TCell"></typeparam>
    public interface ITriangulation<TVertex, TCell>
        where TCell : TriangulationCell<TVertex, TCell>, new()
        where TVertex : IVertex
    {
        IEnumerable<TCell> Cells { get; }
    }

    /// <summary>
    /// Factory class for creating triangulations.
    /// </summary>
    public static class Triangulation
    {
        /// <summary>
        /// Creates the Delaunay triangulation of the input data.
        /// </summary>
        /// <typeparam name="TVertex"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ITriangulation<TVertex, DefaultTriangulationCell<TVertex>> CreateDelaunay<TVertex>(IEnumerable<TVertex> data)
            where TVertex : IVertex
        {
            return DelaunayTriangulation<TVertex, DefaultTriangulationCell<TVertex>>.Create(data);
        }

        /// <summary>
        /// Creates the Delaunay triangulation of the input data.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ITriangulation<DefaultVertex, DefaultTriangulationCell<DefaultVertex>> CreateDelaunay(IEnumerable<double[]> data)
        {
            var points = data.Select(p => new DefaultVertex { Position = p.ToArray() });
            return DelaunayTriangulation<DefaultVertex, DefaultTriangulationCell<DefaultVertex>>.Create(points);
        }

        /// <summary>
        /// Creates the Delaunay triangulation of the input data.
        /// </summary>
        /// <typeparam name="TVertex"></typeparam>
        /// <typeparam name="TFace"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ITriangulation<TVertex, TFace> CreateDelaunay<TVertex, TFace>(IEnumerable<TVertex> data)
            where TVertex : IVertex
            where TFace : TriangulationCell<TVertex, TFace>, new()
        {
            return DelaunayTriangulation<TVertex, TFace>.Create(data);
        }
    }
}
