using System.Collections.Generic;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using ReferenceEqualityComparer = ProtoFFI.ReferenceEqualityComparer;

namespace ProtoFFITests
{
    [TestFixture]
    public class ReferenceEqualityComparerTests
    {
        /// <summary>
        /// Test class that simulates geometry objects with value-based hash codes
        /// (similar to Point objects with identical coordinates)
        /// </summary>
        private class TestPoint
        {
            public double X { get; set; }
            public double Y { get; set; }
            public double Z { get; set; }

            public TestPoint(double x, double y, double z)
            {
                X = x;
                Y = y;
                Z = z;
            }

            // Value-based hash code (like Point.ComputeHashCode in LibG)
            public override int GetHashCode()
            {
                int hash = 17;
                hash = hash * 23 + X.GetHashCode();
                hash = hash * 23 + Y.GetHashCode();
                hash = hash * 23 + Z.GetHashCode();
                return hash;
            }

            public override bool Equals(object obj)
            {
                if (obj == null || GetType() != obj.GetType())
                    return false;

                var other = (TestPoint)obj;
                return X == other.X && Y == other.Y && Z == other.Z;
            }
        }

        [Test]
        [Category("UnitTests")]
        public void ProducesDifferentHashCodesForDifferentInstances()
        {
            // Arrange: Create multiple objects with identical values but different instances
            var point1 = new TestPoint(0, 0, 0);
            var point2 = new TestPoint(0, 0, 0);
            var point3 = new TestPoint(0, 0, 0);

            var comparer = new ReferenceEqualityComparer();

            // Act: Get hash codes using ReferenceEqualityComparer
            int hash1 = comparer.GetHashCode(point1);
            int hash2 = comparer.GetHashCode(point2);
            int hash3 = comparer.GetHashCode(point3);

            // Assert: Different instances should produce different hash codes
            // (even though they have identical values)
            Assert.AreNotEqual(hash1, hash2, "Different instances should produce different hash codes");
            Assert.AreNotEqual(hash1, hash3, "Different instances should produce different hash codes");
            Assert.AreNotEqual(hash2, hash3, "Different instances should produce different hash codes");
        }

        [Test]
        [Category("UnitTests")]
        public void UsesIdentityHashCode()
        {
            // Arrange
            var point = new TestPoint(1, 2, 3);
            var comparer = new ReferenceEqualityComparer();

            // Act
            int comparerHash = comparer.GetHashCode(point);
            int identityHash = RuntimeHelpers.GetHashCode(point);

            // Assert: ReferenceEqualityComparer should use RuntimeHelpers.GetHashCode
            Assert.AreEqual(identityHash, comparerHash,
                "ReferenceEqualityComparer should use RuntimeHelpers.GetHashCode for identity-based hashing");
        }

        [Test]
        [Category("UnitTests")]
        public void DictionaryLookupPerformance_NoCollisions()
        {
            // Arrange: Create dictionary using ReferenceEqualityComparer
            var dictionary = new Dictionary<object, string>(new ReferenceEqualityComparer());

            // Create objects with identical values but different instances
            const int count = 20;
            var objects = new TestPoint[count];
            for (int i = 0; i < count; i++)
            {
                objects[i] = new TestPoint(0, 0, 0); // All have identical coordinates
                dictionary[objects[i]] = $"Value_{i}";
            }

            // Act & Assert: All lookups should succeed and be fast (O(1))
            for (int i = 0; i < count; i++)
            {
                Assert.IsTrue(dictionary.TryGetValue(objects[i], out string value),
                    $"Lookup should succeed for object at index {i}");
                Assert.AreEqual($"Value_{i}", value,
                    $"Retrieved value should match for object at index {i}");
            }

            // Verify that objects with same values but different instances are treated as different
            var newPoint = new TestPoint(0, 0, 0);
            Assert.IsFalse(dictionary.ContainsKey(newPoint),
                "New instance with same values should not be found (reference equality)");
        }

        [Test]
        [Category("UnitTests")]
        public void ReferenceEqualitySemantics()
        {
            // Arrange
            var point1 = new TestPoint(1, 2, 3);
            var point2 = new TestPoint(1, 2, 3); // Same values, different instance
            IEqualityComparer<object> comparer = new ReferenceEqualityComparer();

            // Act & Assert: Reference equality should be used, not value equality
            Assert.IsFalse(comparer.Equals(point1, point2),
                "Different instances should not be equal (reference equality)");
            Assert.IsTrue(comparer.Equals(point1, point1),
                "Same instance should be equal to itself");
        }
    }
}
