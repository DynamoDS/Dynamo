using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Geometry;
using NUnit.Framework;
using Autodesk.DesignScript.Interfaces;
namespace UnfoldTests
{
    [TestFixture]
    public class TopologyTests
    {

        public class InitialGraphTests
        {
            [Test]
            public void GraphHasVertsForEachFace()
            {
                //
            }

            [Test]
            public void EveryFaceIsReachable()
            {
                //
            }
            [Test]
            public void NoRepeatVerts()
            {
                //

            }


        }

        public class BFSTreeTests
        {
            [Test]
            public void TreeIsAcyclic()
            {
                //
            }

           [Test]
            public void TreeContainsAllFaces()
            {
               //
            }

            [Test]
           public void TreeContainsNoRepeatEdges()
           {
                //
           }

            [Test]
            public void AllFinishingTimesSet()
            {
                //
            }

        }


    }
}
