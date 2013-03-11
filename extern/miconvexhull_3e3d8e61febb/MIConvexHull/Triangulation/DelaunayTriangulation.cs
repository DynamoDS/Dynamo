namespace MIConvexHull
{
    using System.Collections.Generic;
    using System.Linq;
    using System;

    /// <summary>
    /// Calculation and representation of Delaunay triangulation.
    /// </summary>
    /// <typeparam name="TVertex"></typeparam>
    /// <typeparam name="TCell"></typeparam>
    public class DelaunayTriangulation<TVertex, TCell> : ITriangulation<TVertex, TCell>
        where TCell : TriangulationCell<TVertex, TCell>, new()
        where TVertex : IVertex
    {
        /// <summary>
        /// Cells of the triangulation.
        /// </summary>
        public IEnumerable<TCell> Cells { get; private set; }

        /// <summary>
        /// Creates the Delaunay triangulation of the input data.
        /// Be careful with concurrency, because during the computation, the vertex position arrays get resized.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static DelaunayTriangulation<TVertex, TCell> Create(IEnumerable<TVertex> data)
        {
            if (data == null) throw new ArgumentException("data can't be null.");
            if (!(data is IList<TVertex>)) data = data.ToArray();
            if (data.Count() == 0) return new DelaunayTriangulation<TVertex, TCell> { Cells = Enumerable.Empty<TCell>() };

            int dimension = data.First().Position.Length;

            // Resize the arrays and lift the data.
            foreach (var p in data)
            {
                double lenSq = StarMath.norm2(p.Position, dimension, true);
                var v = p.Position;
                Array.Resize(ref v, dimension + 1);
                p.Position = v;
                p.Position[dimension] = lenSq;
            }

            // Find the convex hull
            var delaunayFaces = ConvexHullInternal.GetConvexFacesInternal<TVertex, TCell>(data);

            // Resize the data back
            foreach (var p in data)
            {
                var v = p.Position;
                Array.Resize(ref v, dimension);
                p.Position = v;
            }
            // Remove the "upper" faces
            for (var i = delaunayFaces.Count - 1; i >= 0; i--)
            {
                var candidate = delaunayFaces[i];
                if (candidate.Normal[dimension] >= 0)
                {
                    for (int fi = 0; fi < candidate.AdjacentFaces.Length; fi++)
                    {
                        var f = candidate.AdjacentFaces[fi];
                        if (f != null)
                        {
                            for (int j = 0; j < f.AdjacentFaces.Length; j++)
                            {
                                if (object.ReferenceEquals(f.AdjacentFaces[j], candidate))
                                {
                                    f.AdjacentFaces[j] = null;
                                }
                            }
                        }
                    }
                    var li = delaunayFaces.Count - 1;
                    delaunayFaces[i] = delaunayFaces[li];
                    delaunayFaces.RemoveAt(li);
                }
            }

            // Create the "TCell" representation.
            int cellCount = delaunayFaces.Count;
            var cells = new TCell[cellCount];

            for (int i = 0; i < cellCount; i++)
            {
                var face = delaunayFaces[i];
                var vertices = new TVertex[dimension + 1];
                for (int j = 0; j <= dimension; j++) vertices[j] = (TVertex)face.Vertices[j].Vertex;
                cells[i] = new TCell
                {
                    Vertices = vertices,
                    Adjacency = new TCell[dimension + 1]
                };
                face.Tag = i;
            }

            for (int i = 0; i < cellCount; i++)
            {
                var face = delaunayFaces[i];
                var cell = cells[i];
                for (int j = 0; j <= dimension; j++)
                {
                    if (face.AdjacentFaces[j] == null) continue;
                    cell.Adjacency[j] = cells[face.AdjacentFaces[j].Tag];
                }
            }

            return new DelaunayTriangulation<TVertex, TCell> { Cells = cells };
        }

        /// <summary>
        /// Can only be created using a factory method.
        /// </summary>
        private DelaunayTriangulation()
        {

        }
    }
}
