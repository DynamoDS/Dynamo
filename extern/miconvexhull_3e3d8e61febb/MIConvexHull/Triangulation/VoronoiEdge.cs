namespace MIConvexHull
{
    /// <summary>
    /// A class representing an (undirected) edge of the Voronoi graph.
    /// </summary>
    /// <typeparam name="TVertex"></typeparam>
    /// <typeparam name="TCell"></typeparam>
    public class VoronoiEdge<TVertex, TCell>
        where TVertex : IVertex
        where TCell : TriangulationCell<TVertex, TCell>
    {
        /// <summary>
        /// Source of the edge.
        /// </summary>
        public TCell Source
        {
            get;
            internal set;
        }

        /// <summary>
        /// Target of the edge.
        /// </summary>
        public TCell Target
        {
            get;
            internal set;
        }

        /// <summary>
        /// ...
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var other = obj as VoronoiEdge<TVertex, TCell>;
            if (other == null) return false;
            if (object.ReferenceEquals(this, other)) return true;
            return (Source == other.Source && Target == other.Target)
                || (Source == other.Target && Target == other.Source);
        }

        /// <summary>
        /// ...
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            int hash = 23;
            hash = hash * 31 + Source.GetHashCode();
            return hash * 31 + Target.GetHashCode();
        }

        /// <summary>
        /// Create an instance of the edge.
        /// </summary>
        public VoronoiEdge()
        {

        }

        /// <summary>
        /// Create an instance of the edge.
        /// </summary>
        public VoronoiEdge(TCell source, TCell target)
        {
            this.Source = source;
            this.Target = target;
        }
    }
}
