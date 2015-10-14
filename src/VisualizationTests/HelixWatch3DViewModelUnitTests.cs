using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace WpfVisualizationTests
{
    [TestFixture]
    public class HelixWatch3DViewModelUnitTests
    {
        #region clip plane tests

        [Test]
        public void WithinDefaultBounds_ClipPlane_IsCorrect()
        {
            
        }

        [Test]
        public void OutsideDefaultBounds_ClosestObjectInsideDefaultBounds_ClipPlane_IsCorrect()
        {

        }

        [Test]
        public void SameLocationAsClosestObject_ClipPlane_IsCorrect()
        {

        }

        [Test]
        public void ClosestObjectIsFarAway_WithinDefaultBounds_ClipPlane_IsCorrect()
        {

        }

        #endregion

        #region zoom to fit tests

        [Test]
        public void NothingSelected_ZoomToFit_DefaultZoom()
        {
            
        }

        [Test]
        public void PointNodeSelected_ZoomToFit_BoundingBoxIsCorrect()
        {
            
        }

        [Test]
        public void MultipleGeometryNodesSelected_ZoomToFit_BoundingBoxIsCorrect()
        {
            
        }

        #endregion
    }
}
