using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unfold;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;


namespace Unfold.Interfaces
{
    
        // this class is a comparator that we feed to the constructor of any dictionaries 
        // we want to use these equals and hashcode methods
        // this is done so we do not need to override the equals and hashcode methods on the orignal objects
        public class SpatiallyEquatableComparer<S> : IEqualityComparer<S> where S:ISpatialEquatable
        {

            public bool Equals(S x, S y)
            {

                return x.SpatialEquals(y);

            }

            public int GetHashCode(S spatial)
            {
                return spatial.GetSpatialHashCode();
            }
        }

        // this interface forces objects to implement the spatial equals + hashcode methods that the comparator asks for
        public interface ISpatialEquatable
        {
            int GetSpatialHashCode();

            bool SpatialEquals(ISpatialEquatable y);

        }


       public interface IUnfoldPlanarFace<K> where K:IUnfoldEdge
        {
             Object OriginalEntity { get; set; }
             Surface SurfaceEntity { get; set; }
             List<K> EdgeLikeEntities { get; set; }
           
           

        }

      public  interface IUnfoldEdge : ISpatialEquatable
        {
            Point Start { get; set; }
            Point End { get; set; }
            Curve Curve { get; set; }

        }

    }

