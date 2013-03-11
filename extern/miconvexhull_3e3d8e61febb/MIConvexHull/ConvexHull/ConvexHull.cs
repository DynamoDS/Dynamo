namespace MIConvexHull
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Factory class for computing convex hulls.
    /// </summary>
    public static class ConvexHull
    {
        /// <summary>
        /// Creates a convex hull of the input data.
        /// </summary>
        /// <typeparam name="TVertex"></typeparam>
        /// <typeparam name="TFace"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ConvexHull<TVertex, TFace> Create<TVertex, TFace>(IEnumerable<TVertex> data)
            where TVertex : IVertex
            where TFace : ConvexFace<TVertex, TFace>, new()
        {
            return ConvexHull<TVertex, TFace>.Create(data);
        }

        /// <summary>
        /// Creates a convex hull of the input data.
        /// </summary>
        /// <typeparam name="TVertex"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ConvexHull<TVertex, DefaultConvexFace<TVertex>> Create<TVertex>(IEnumerable<TVertex> data)
            where TVertex : IVertex
        {
            return ConvexHull<TVertex, DefaultConvexFace<TVertex>>.Create(data);
        }

        /// <summary>
        /// Creates a convex hull of the input data.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ConvexHull<DefaultVertex, DefaultConvexFace<DefaultVertex>> Create(IEnumerable<double[]> data)
        {
            var points = data.Select(p => new DefaultVertex { Position = p.ToArray() });
            return ConvexHull<DefaultVertex, DefaultConvexFace<DefaultVertex>>.Create(points);
        }
    }

    /// <summary>
    /// Representation of a convex hull.
    /// </summary>
    /// <typeparam name="TVertex"></typeparam>
    /// <typeparam name="TFace"></typeparam>
    public class ConvexHull<TVertex, TFace>
        where TVertex : IVertex
        where TFace : ConvexFace<TVertex, TFace>, new()
    {
        /// <summary>
        /// Points of the convex hull.
        /// </summary>
        public IEnumerable<TVertex> Points { get; private set; }

        /// <summary>
        /// Faces of the convex hull.
        /// </summary>
        public IEnumerable<TFace> Faces { get; private set; }

        /// <summary>
        /// Creates the convex hull.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ConvexHull<TVertex, TFace> Create(IEnumerable<TVertex> data)
        {
            if (!(data is IList<TVertex>)) data = data.ToArray();
            var ch = ConvexHullInternal.GetConvexHullAndFaces<TVertex, TFace>(data.Cast<IVertex>());
            return new ConvexHull<TVertex, TFace> { Points = ch.Item1, Faces = ch.Item2 };
        }

        /// <summary>
        /// Can only be created using a factory method.
        /// </summary>
        private ConvexHull()
        {

        }
    }
}
