
using Dynamo.Core.Threading;
using NUnit.Framework;

namespace Dynamo.Tests
{
#if false

    class TimeStampOrderingTests
    {
        [Test]
        public void TestOrderingConstraints()
        {
            TimeStamp first = TimeStamp.First;
            
            TimeStamp next = first.Next();
            TimeStamp nextnext = next.Next();

            Assert.IsTrue(first < next);
            Assert.IsTrue(first < nextnext);

            Assert.IsTrue(next < nextnext);

            
            Assert.IsTrue(next > first);
            Assert.IsTrue(nextnext > first);
            
            Assert.IsTrue(nextnext > next);

            // Alisasing the variable is necessary to prevent the compiler's naive
            // comparison check preventing this
            TimeStamp a = first;
            Assert.IsFalse(a > first);
            Assert.IsFalse(a < first);

            TimeStamp b = next;
            Assert.IsFalse(b > next);
            Assert.IsFalse(b < next);
        }
    }

#else

    class TimeStampOrderingTests
    {
        [Test]
        public void TestOrderingConstraints()
        {
            var generator = new TimeStampGenerator();

            TimeStamp first = generator.Next;
            TimeStamp next = generator.Next;
            TimeStamp nextnext = generator.Next;

            Assert.IsTrue(first < next);
            Assert.IsTrue(first < nextnext);
            Assert.IsTrue(next < nextnext);

            Assert.IsTrue(next > first);
            Assert.IsTrue(nextnext > first);
            Assert.IsTrue(nextnext > next);

            // Alisasing the variable is necessary to prevent 
            // the compiler's naive comparison check preventing this
            TimeStamp a = first;
            Assert.IsFalse(a > first);
            Assert.IsFalse(a < first);

            TimeStamp b = next;
            Assert.IsFalse(b > next);
            Assert.IsFalse(b < next);
        }
    }

#endif
}
