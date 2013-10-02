using System.Collections.Generic;

namespace Dynamo
{
    public class Visualization
    {
        public bool RequiresUpdate { get; set; }
        public RenderDescription Description { get; internal set; }
        public List<object> Geometry { get; internal set; }

        public Visualization()
        {
            RequiresUpdate = false;
            Description = new RenderDescription();
            Geometry = new List<object>();
        }
    }
}
