using System;

using Xunit;

namespace OpenCollar.Azure.ReliableQueue.TESTS.Model
{
    public sealed class QueueKeyTests
    {
        [Fact]
        public void TestCompareTo()
        {
            var a = new OpenCollar.Azure.ReliableQueue.Model.QueueKey("AAA");
            var a1 = new OpenCollar.Azure.ReliableQueue.Model.QueueKey("AAA");
            var b = new OpenCollar.Azure.ReliableQueue.Model.QueueKey("BBB");
            var c = new OpenCollar.Azure.ReliableQueue.Model.QueueKey("CCC");

            Assert.Equal(1, a.CompareTo((OpenCollar.Azure.ReliableQueue.Model.QueueKey)null));
            Assert.Equal(0, a.CompareTo(a));
            Assert.Equal(0, a.CompareTo(a1));
            Assert.Equal(-1, a.CompareTo(b));
            Assert.Equal(1, b.CompareTo(a));
        }

        [Fact]
        public void TestCompareToObject()
        {
            var a = new OpenCollar.Azure.ReliableQueue.Model.QueueKey("AAA");
            var a1 = new OpenCollar.Azure.ReliableQueue.Model.QueueKey("AAA");
            var b = new OpenCollar.Azure.ReliableQueue.Model.QueueKey("BBB");
            var c = new OpenCollar.Azure.ReliableQueue.Model.QueueKey("CCC");

            Assert.Equal(1, a.CompareTo((object)null));
            Assert.Equal(0, a.CompareTo((object)a));
            Assert.Equal(0, a.CompareTo((object)a1));
            Assert.Equal(-1, a.CompareTo((object)b));
            Assert.Equal(1, b.CompareTo((object)a));
        }

        [Fact]
        public void TestConstructor()
        {
            Assert.Throws<ArgumentNullException>(() => { new OpenCollar.Azure.ReliableQueue.Model.QueueKey(null); });
            Assert.Throws<ArgumentException>(() => { new OpenCollar.Azure.ReliableQueue.Model.QueueKey(string.Empty); });
            Assert.Throws<ArgumentException>(() => { new OpenCollar.Azure.ReliableQueue.Model.QueueKey(" \t\r\n"); });

            var x = new OpenCollar.Azure.ReliableQueue.Model.QueueKey("TEST+NAME+1");
            Assert.NotNull(x);
            Assert.Equal("test-name-1", x.Identifier);
            Assert.Equal("TestxNamex1", x.TableIdentifier);
            Assert.Equal("TEST+NAME+1", x.ToString());
        }

        [Fact]
        public void TestEquals()
        {
            var a = new OpenCollar.Azure.ReliableQueue.Model.QueueKey("AAA");
            var a1 = new OpenCollar.Azure.ReliableQueue.Model.QueueKey("AAA");
            var b = new OpenCollar.Azure.ReliableQueue.Model.QueueKey("BBB");
            var c = new OpenCollar.Azure.ReliableQueue.Model.QueueKey("CCC");

            Assert.False(a.Equals((OpenCollar.Azure.ReliableQueue.Model.QueueKey)null));
            Assert.True(a.Equals(a));
            Assert.True(a.Equals(a1));
            Assert.False(a.Equals(22));
            Assert.False(a.Equals(b));
        }
    }
}