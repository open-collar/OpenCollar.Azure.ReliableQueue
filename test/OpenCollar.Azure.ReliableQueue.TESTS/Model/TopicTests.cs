using System;

using Xunit;

namespace OpenCollar.Azure.ReliableQueue.TOPICS.Model
{
    public sealed class TopicTests
    {
        [Fact]
        public void TestCastOperands()
        {
            var a = new OpenCollar.Azure.ReliableQueue.Model.Topic("AAA");

            Assert.Equal(a, "AAA");
            Assert.Equal("AAA", a);

            Assert.NotEqual(a, null);
            Assert.NotEqual(null, a);
        }

        [Fact]
        public void TestCompareOperands()
        {
            var a = new OpenCollar.Azure.ReliableQueue.Model.Topic("AAA");
            var a1 = new OpenCollar.Azure.ReliableQueue.Model.Topic("AAA");
            var b = new OpenCollar.Azure.ReliableQueue.Model.Topic("BBB");
            var c = new OpenCollar.Azure.ReliableQueue.Model.Topic("CCC");

            Assert.True(a == a);
            Assert.True(a == a1);
            Assert.False(a > a);
            Assert.True(a >= a);

            Assert.False(a != a);
            Assert.False(a < a);
            Assert.True(a <= a);

            Assert.True(a != b);
            Assert.False(a == b);
            Assert.True(a < b);
            Assert.True(a <= b);
        }

        [Fact]
        public void TestCompareTo()
        {
            var a = new OpenCollar.Azure.ReliableQueue.Model.Topic("AAA");
            var a1 = new OpenCollar.Azure.ReliableQueue.Model.Topic("AAA");
            var b = new OpenCollar.Azure.ReliableQueue.Model.Topic("BBB");
            var c = new OpenCollar.Azure.ReliableQueue.Model.Topic("CCC");

            Assert.Equal(1, a.CompareTo((OpenCollar.Azure.ReliableQueue.Model.Topic)null));
            Assert.Equal(0, a.CompareTo(a));
            Assert.Equal(0, a.CompareTo(a1));
            Assert.Equal(-1, a.CompareTo(b));
            Assert.Equal(1, b.CompareTo(a));
        }

        [Fact]
        public void TestCompareToObject()
        {
            var a = new OpenCollar.Azure.ReliableQueue.Model.Topic("AAA");
            var a1 = new OpenCollar.Azure.ReliableQueue.Model.Topic("AAA");
            var b = new OpenCollar.Azure.ReliableQueue.Model.Topic("BBB");
            var c = new OpenCollar.Azure.ReliableQueue.Model.Topic("CCC");

            Assert.Equal(1, a.CompareTo((object)null));
            Assert.Equal(0, a.CompareTo((object)a));
            Assert.Equal(0, a.CompareTo((object)a1));
            Assert.Equal(-1, a.CompareTo((object)b));
            Assert.Equal(1, b.CompareTo((object)a));
            Assert.Throws<ArgumentException>(() => a.CompareTo(222));
        }

        [Fact]
        public void TestConstructor()
        {
            var x = new OpenCollar.Azure.ReliableQueue.Model.Topic("TOPIC+NAME+1");
            Assert.NotNull(x);
            Assert.Equal("topic-name-1", x.Identifier);
            Assert.Equal("TOPIC+NAME+1", x.ToString());
            Assert.False(x.IsEmpty);

            x = new OpenCollar.Azure.ReliableQueue.Model.Topic(null);
            Assert.NotNull(x);
            Assert.Equal(OpenCollar.Azure.ReliableQueue.Model.Topic.Default, x);
            Assert.Equal(OpenCollar.Azure.ReliableQueue.Model.Topic.DefaultIdentifier, x.Identifier);
            Assert.True(x.IsEmpty);

            x = new OpenCollar.Azure.ReliableQueue.Model.Topic(string.Empty);
            Assert.NotNull(x);
            Assert.Equal(OpenCollar.Azure.ReliableQueue.Model.Topic.Default, x);
            Assert.Equal(OpenCollar.Azure.ReliableQueue.Model.Topic.DefaultIdentifier, x.Identifier);
            Assert.True(x.IsEmpty);

            x = new OpenCollar.Azure.ReliableQueue.Model.Topic(" \r\n\t");
            Assert.NotNull(x);
            Assert.Equal(OpenCollar.Azure.ReliableQueue.Model.Topic.Default, x);
            Assert.Equal(OpenCollar.Azure.ReliableQueue.Model.Topic.DefaultIdentifier, x.Identifier);
            Assert.True(x.IsEmpty);
        }

        [Fact]
        public void TestEquals()
        {
            var a = new OpenCollar.Azure.ReliableQueue.Model.Topic("AAA");
            var a1 = new OpenCollar.Azure.ReliableQueue.Model.Topic("AAA");
            var b = new OpenCollar.Azure.ReliableQueue.Model.Topic("BBB");
            var c = new OpenCollar.Azure.ReliableQueue.Model.Topic("CCC");

            Assert.False(a.Equals((OpenCollar.Azure.ReliableQueue.Model.Topic)null));
            Assert.True(a.Equals(a));
            Assert.True(a.Equals(a1));
            Assert.False(a.Equals(22));
            Assert.False(a.Equals(b));
        }

        [Fact]
        public void TestEqualsObjects()
        {
            var a = new OpenCollar.Azure.ReliableQueue.Model.Topic("AAA");
            var a1 = new OpenCollar.Azure.ReliableQueue.Model.Topic("AAA");
            var b = new OpenCollar.Azure.ReliableQueue.Model.Topic("BBB");
            var c = new OpenCollar.Azure.ReliableQueue.Model.Topic("CCC");

            Assert.False(a.Equals((object)null));
            Assert.True(a.Equals((object)a));
            Assert.True(a.Equals((object)a1));
            Assert.False(a.Equals((object)22));
            Assert.False(a.Equals((object)b));
        }

        [Fact]
        public void TestToString()
        {
            var a = new OpenCollar.Azure.ReliableQueue.Model.Topic("AAA");
            var a1 = new OpenCollar.Azure.ReliableQueue.Model.Topic("AAA");
            var b = new OpenCollar.Azure.ReliableQueue.Model.Topic("BBB");

            Assert.Equal(a.GetHashCode(), a.GetHashCode());
            Assert.Equal(a.GetHashCode(), a1.GetHashCode());
            Assert.NotEqual(a.GetHashCode(), b.GetHashCode());
        }
    }
}