using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using RevitServices;
using DSNodeServices;
using RevitPersistenceManager;

namespace DSRevitNodes
{

    //TODO(Luke): Move me
    public abstract class AbstractGeometry : IDisposable
    {
        public static Document Document
        {
            get { return RevitPersistenceManager.DocumentManager.CurrentDoc; }
        }


        public abstract void Dispose();
    }


    /// <summary>
    /// A sample geometry class
    /// Node that it needs to be reistered for trace in order to be able to
    /// rebind itself
    /// </summary>
    [RegisterForTrace]
    public class Point : AbstractGeometry
    {
        protected ReferencePoint RefPoint;
        protected ElementId ID;

        private Point()
        {
            
        }



        /// <summary>
        /// A sample property
        /// </summary>
        public double X
        {
            get
            {
                return RefPoint.Position.X;
            }

            set
            {
                XYZ xyz = new XYZ(value, Y, Z);
                var transManager = new TransactionManager();
                var transaction = transManager.StartTransaction(Document);

                RefPoint.Position = xyz;

                transaction.CommitTransaction();
            }

        }
        public double Y { get; set; }
        public double Z { get; set; }

        /// <summary>
        /// A sample constructor
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static Point ByCoordinates(double x, double y, double z)
        {
            var transManager = new TransactionManager();
            var transaction = transManager.StartTransaction(Document);

            ReferencePoint refPoint = Document.FamilyCreate.NewReferencePoint(new XYZ(x, y, z));
            ElementId elementId = refPoint.Id;

            transaction.CommitTransaction();

            //@TODO(Luke): Add a test that does readback verification to make sure this is safe, it's almost certainly faster
            Point pt = new Point();
            pt.Y = y;
            pt.Z = z;
            pt.RefPoint = refPoint;
            pt.ID = elementId;

            return pt;
        }


        public override void Dispose()
        {
            //TODO(Luke): Move this into persistence
            var transManager = new TransactionManager();
            var transaction = transManager.StartTransaction(Document);

            Document.Delete(ID);

            transaction.CommitTransaction();

        }
    }
}
