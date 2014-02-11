namespace CPTC.Octree
{
    public class OctreeDistance
    {
        public double dist = 0;

        public OctreeDistance(double distance)
        {
            dist = distance;
        }

        public double Distance
        {
            get
            {
                return dist;
            }
            set 
            {
                value = dist;
            }
        }
}
}