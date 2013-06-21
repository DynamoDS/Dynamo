using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Dynamo.Nodes
{
    public interface IQuadTree<T>
    {
        IEnumerable<T> CollectCircle(Point p, double radius);
    }

    abstract class AQuadTree<T> : IQuadTree<T>
    {
        protected double MinX, MinY, MaxX, MaxY;

        public IEnumerable<T> CollectCircle(Point p, double radius)
        {
            throw new NotImplementedException();
        }
    }

    class QTNode<T> : AQuadTree<T>
    {

    }

    class QTBranch<T> : AQuadTree<T>
    {

    }
}
