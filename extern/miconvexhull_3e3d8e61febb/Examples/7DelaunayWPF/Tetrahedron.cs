using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MIConvexHull;
using System.Windows.Media.Media3D;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows;

namespace DelaunayWPF
{
    /// <summary>
    /// Represents a tetrahedron - what a surprise.
    /// </summary>
    class Tetrahedron : TriangulationCell<Vertex, Tetrahedron>
    {
        // easing functions
        static readonly IEasingFunction pulseEasing = new ElasticEase() { Springiness = 1, EasingMode = EasingMode.EaseOut, Oscillations = 8 };
        static readonly IEasingFunction collapseEasing = new CircleEase() { EasingMode = EasingMode.EaseIn };
        static readonly IEasingFunction expandEasing = new CircleEase() { EasingMode = EasingMode.EaseInOut };
        
        // animations
        AnimationTimeline expandX, expandY, expandZ;
        AnimationTimeline movePositive, moveNegative;
        AnimationTimeline collapse;
        AnimationTimeline pulseX, pulseY, pulseZ;

        TranslateTransform3D translation;

        /// <summary>
        /// Helper function to get the position of the i-th vertex.
        /// </summary>
        /// <param name="i"></param>
        /// <returns>Position of the i-th vertex</returns>
        Point3D GetPosition(int i)
        {
            return Vertices[i].ToPoint3D();
        }


        /// <summary>
        /// Creates and "additive" expand animation.
        /// </summary>
        /// <param name="to"></param>
        /// <returns></returns>
        AnimationTimeline CreateExpandAnimation(double to)
        {            
            return new DoubleAnimation { From = 0, To = to, Duration = TimeSpan.FromSeconds(1), EasingFunction = expandEasing, IsAdditive = true };
        }

        /// <summary>
        /// Creates a pulsing animation.
        /// </summary>
        /// <param name="to"></param>
        /// <returns></returns>
        AnimationTimeline CreatePulseAnimation(double to)
        {
            DoubleAnimationUsingKeyFrames pulseAndCollapse = new DoubleAnimationUsingKeyFrames
            {
                Duration = new Duration(TimeSpan.FromSeconds(3.5)),
                KeyFrames = new DoubleKeyFrameCollection 
                {
                    new EasingDoubleKeyFrame(to, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(3)), pulseEasing),
                    new EasingDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(3.5)), collapseEasing)
                }
            };
            return pulseAndCollapse;
        }

        /// <summary>
        /// Applies animations to individual translation components.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        void Animate(AnimationTimeline x, AnimationTimeline y, AnimationTimeline z)
        {
            translation.BeginAnimation(TranslateTransform3D.OffsetXProperty, x, HandoffBehavior.SnapshotAndReplace);
            translation.BeginAnimation(TranslateTransform3D.OffsetYProperty, y, HandoffBehavior.SnapshotAndReplace);
            translation.BeginAnimation(TranslateTransform3D.OffsetZProperty, z, HandoffBehavior.SnapshotAndReplace);
        }

        /// <summary>
        /// Begins the expand animation.
        /// </summary>
        public void Expand()
        {
            Animate(expandX, expandY, expandZ);
        }

        static Random rnd = new Random();

        /// <summary>
        /// Let's introduce some chaos.
        /// </summary>
        public void ExpandRandom()
        {
            switch (rnd.Next(6))
            {
                case 0: translation.BeginAnimation(TranslateTransform3D.OffsetXProperty, movePositive, HandoffBehavior.SnapshotAndReplace); break;
                case 1: translation.BeginAnimation(TranslateTransform3D.OffsetXProperty, moveNegative, HandoffBehavior.SnapshotAndReplace); break;
                case 2: translation.BeginAnimation(TranslateTransform3D.OffsetYProperty, movePositive, HandoffBehavior.SnapshotAndReplace); break;
                case 3: translation.BeginAnimation(TranslateTransform3D.OffsetYProperty, moveNegative, HandoffBehavior.SnapshotAndReplace); break;
                case 4: translation.BeginAnimation(TranslateTransform3D.OffsetZProperty, movePositive, HandoffBehavior.SnapshotAndReplace); break;
                case 5: translation.BeginAnimation(TranslateTransform3D.OffsetZProperty, moveNegative, HandoffBehavior.SnapshotAndReplace); break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Begins the collapse animation.
        /// </summary>
        public void Collapse()
        {
            Animate(collapse, collapse, collapse);
        }

        /// <summary>
        /// Begins the pulse animation.
        /// </summary>
        public void Pulse()
        {
            Animate(pulseX, pulseY, pulseZ);
        }

        /// <summary>
        /// This function adds indices for a triangle representing the face.
        /// The order is in the CCW (counter clock wise) order so that the automatically calculated normals point in the right direction.
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <param name="k"></param>
        /// <param name="center"></param>
        /// <param name="indices"></param>
        void MakeFace(int i, int j, int k, Vector3D center, Int32Collection indices)
        {
            var u = GetPosition(j) - GetPosition(i);
            var v = GetPosition(k) - GetPosition(j);
                       
            // compute the normal and the plane corresponding to the side [i,j,k]
            var n = Vector3D.CrossProduct(u, v);
            var d = -Vector3D.DotProduct(n, center);
                        
            // check if the normal faces towards the center
            var t = Vector3D.DotProduct(n, (Vector3D)GetPosition(i)) + d;            
            if (t >= 0)
            {
                // swapping indices j and k also changes the sign of the normal, because cross product is anti-commutative
                indices.Add(k); indices.Add(j); indices.Add(i);
            }
            else
            {
                // indices are in the correct order
                indices.Add(i); indices.Add(j); indices.Add(k);
            }
        }

        /// <summary>
        /// Creates a model of the tetrahedron. Transparency is applied to the color.
        /// </summary>
        /// <param name="color"></param>
        /// <param name="radius"></param>
        /// <returns>A model representing the tetrahedron</returns>
        public Model3D CreateModel(Color color, double radius)
        {
            this.translation = new TranslateTransform3D();

            var points = new Point3DCollection(Enumerable.Range(0, 4).Select(i => GetPosition(i)));
            
            // center = Sum(p[i]) / 4
            var center = points.Aggregate(new Vector3D(), (a, c) => a + (Vector3D)c) / (double)points.Count;

            var normals = new Vector3DCollection();
            var indices = new Int32Collection();
            MakeFace(0, 1, 2, center, indices);
            MakeFace(0, 1, 3, center, indices);
            MakeFace(0, 2, 3, center, indices);
            MakeFace(1, 2, 3, center, indices);
                        
            var geometry = new MeshGeometry3D { Positions = points, TriangleIndices = indices };            
            var material = new MaterialGroup 
            { 
                Children = new MaterialCollection
                {
                    new DiffuseMaterial(new SolidColorBrush(color) { Opacity = 1.00 }),
                    // give it some shine
                    new SpecularMaterial(Brushes.LightYellow, 2.0) 
                } 
            };
            
            pulseX = CreatePulseAnimation(2 * center.X);
            pulseY = CreatePulseAnimation(2 * center.Y);
            pulseZ = CreatePulseAnimation(2 * center.Z);

            expandX = CreateExpandAnimation(2 * center.X);
            expandY = CreateExpandAnimation(2 * center.Y);
            expandZ = CreateExpandAnimation(2 * center.Z);
            
            movePositive = CreateExpandAnimation(radius / 2);
            moveNegative = CreateExpandAnimation(-radius / 2);

            collapse = new DoubleAnimation { To = 0, Duration = TimeSpan.FromSeconds(1), EasingFunction = collapseEasing };

            return new GeometryModel3D { Geometry = geometry, Material = material, BackMaterial = material, Transform = translation };
        }
    }
}
