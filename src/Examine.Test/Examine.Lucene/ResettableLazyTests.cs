using System;
using System.Threading;
using System.Threading.Tasks;
using Examine.Lucene;
using NUnit.Framework;

namespace Examine.Test.Examine.Lucene
{
    [TestFixture]
    public class ResettableLazyTests
    {
        [Test]
        public void Value_Is_Created_Once_And_Cached()
        {
            var calls = 0;
            var lazy = new ResettableLazy<object>(() =>
            {
                Interlocked.Increment(ref calls);
                return new object();
            });

            Assert.IsFalse(lazy.IsValueCreated);

            var first = lazy.Value;
            var second = lazy.Value;

            Assert.AreSame(first, second);
            Assert.AreEqual(1, calls);
            Assert.IsTrue(lazy.IsValueCreated);
        }

        [Test]
        public void Factory_Exception_Is_Not_Cached_And_Can_Recover()
        {
            var calls = 0;
            var lazy = new ResettableLazy<object>(() =>
            {
                calls++;

                // Fail the first two attempts, succeed afterwards (simulates a transient
                // condition such as a momentarily locked index file during a host recycle).
                if (calls < 3)
                {
                    throw new InvalidOperationException("transient failure");
                }

                return new object();
            });

            // First two accesses throw (and crucially the exception is NOT cached).
            Assert.Throws<InvalidOperationException>(() => _ = lazy.Value);
            Assert.IsFalse(lazy.IsValueCreated);

            Assert.Throws<InvalidOperationException>(() => _ = lazy.Value);
            Assert.IsFalse(lazy.IsValueCreated);

            // Third access succeeds and the value is then cached.
            var value = lazy.Value;
            Assert.IsNotNull(value);
            Assert.IsTrue(lazy.IsValueCreated);
            Assert.AreEqual(3, calls);

            // Subsequent access does not re-invoke the factory.
            Assert.AreSame(value, lazy.Value);
            Assert.AreEqual(3, calls);
        }

        [Test]
        public void Null_Value_Is_Created_Once_And_Not_Re_Invoked()
        {
            var calls = 0;
            var lazy = new ResettableLazy<object>(() =>
            {
                calls++;
                return null!;
            });

            Assert.IsFalse(lazy.IsValueCreated);

            // A factory that legitimately returns null must be treated as "created" so that it is
            // not re-invoked indefinitely on every subsequent access.
            Assert.IsNull(lazy.Value);
            Assert.IsTrue(lazy.IsValueCreated);
            Assert.IsNull(lazy.Value);
            Assert.AreEqual(1, calls);
        }

        [Test]
        public void Concurrent_Access_Creates_Value_Only_Once()
        {
            var calls = 0;
            var lazy = new ResettableLazy<object>(() =>
            {
                Interlocked.Increment(ref calls);
                Thread.Sleep(50);
                return new object();
            });

            var results = new object[16];
            Parallel.For(0, results.Length, i => results[i] = lazy.Value);

            Assert.AreEqual(1, calls);
            for (var i = 1; i < results.Length; i++)
            {
                Assert.AreSame(results[0], results[i]);
            }
        }
    }
}
