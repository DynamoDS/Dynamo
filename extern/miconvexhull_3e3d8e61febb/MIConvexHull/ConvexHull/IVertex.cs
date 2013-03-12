
namespace MIConvexHull
{
    /// <summary>
    /// An interface for a structure with nD position.
    /// </summary>
    public interface IVertex
    {
        /// <summary>
        /// Position of the vertex.
        /// </summary>
        double[] Position { get; set; }
    }

    /// <summary>
    /// "Default" vertex.
    /// </summary>
    public class DefaultVertex : IVertex
    {
        /// <summary>
        /// Position of the vertex.
        /// </summary>
        public double[] Position { get; set; }
    }

}
