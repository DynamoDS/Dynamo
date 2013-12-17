using Autodesk.DesignScript.Geometry;

namespace DSCoreNodes
{
    /// <summary>
    /// A two dimensional UV domain specified by minimum and maximum UVs.
    /// </summary>
    public class Domain2D : ObjectWithPropertyDictionary
    {
        /// <summary>
        /// The minimum of the domain.
        /// </summary>
        public Vector Min { get; set; }

        /// <summary>
        /// The maximum of the domain.
        /// </summary>
        public Vector Max { get; set; }

        /// <summary>
        /// The u dimension span of the domain.
        /// </summary>
        public double USpan
        {
            get { return Max.X - Min.X; }
        }

        /// <summary>
        /// The v dimension span of the domain.
        /// </summary>
        public double VSpan
        {
            get { return Max.Y - Min.Y; }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="min">The minimum UV.</param>
        /// <param name="max">The maximum UV.</param>
        public Domain2D(Vector min, Vector max)
        {
            Min = min;
            Max = max;
        }

        /// <summary>
        /// Construct a domain by minimum and maximum.
        /// </summary>
        /// <param name="min">The minimum UV.</param>
        /// <param name="max">The maximum UV.</param>
        /// <returns></returns>
        public static Domain2D ByMinimumAndMaximum(Vector min, Vector max)
        {
            return new Domain2D(min, max);
        }

        public override string ToString()
        {
            return string.Format("Min:{0},Max:{1}", Min.ToString(), Max.ToString());
        }

        public override void Dispose()
        {
            //delete the elements

            //unregisters with the persistence manager
        }
    }

    /// <summary>
    /// A one dimensional domain specified by minimum and maximum.
    /// </summary>
    public class Domain : ObjectWithPropertyDictionary
    {
        /// <summary>
        /// The minimum of the domain.
        /// </summary>
        public double Min { get; set; }

        /// <summary>
        /// The maximum of the domain.
        /// </summary>
        public double Max { get; set; }

        /// <summary>
        /// The span of the domain.
        /// </summary>
        public double Span
        {
            get { return Max - Min; }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum.</param>
        public Domain(double min, double max)
        {
            Min = min;
            Max = max;
        }

        /// <summary>
        /// Construct a domain by minimum and maximum.
        /// </summary>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum.</param>
        /// <returns></returns>
        public static Domain ByMinimumAndMaximum(double min, double max)
        {
            return new Domain(min, max);
        }

        public override string ToString()
        {
            return string.Format("Min:{0},Max:{1}", Min.ToString(), Max.ToString());
        }

        public override void Dispose()
        {
            //delete the elements

            //unregisters with the persistence manager
        }
    }
}

/* Moved old Domain node to avoid circular assembly reference
namespace Dynamo.Nodes
{
    [NodeName("Domain")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_SURFACE_UV)]
    [NodeDescription("Create a domain specifying the Minimum and Maximum UVs.")]
    public class Domain : NodeWithOneOutput
    {
        public Domain()
        {
            InPortData.Add(new PortData("min", "The minimum of the domain.", typeof(FScheme.Value.Number)));
            InPortData.Add(new PortData("max", "The maximum of the domain.", typeof(FScheme.Value.Number)));
            OutPortData.Add(new PortData("domain", "A domain.", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var min = ((FScheme.Value.Number)args[0]).Item;
            var max = ((FScheme.Value.Number)args[1]).Item;

            return FScheme.Value.NewContainer(DSCoreNodes.Domain.ByMinimumAndMaximum(min, max));
        }
    }
}
*/
