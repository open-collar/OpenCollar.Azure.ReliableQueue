using System;
using System.Collections.Generic;
using System.Text;

using Xunit;

namespace OpenCollar.Azure.ReliableQueue.TESTS.Model
{
    public sealed class QueueKeyTests
    {
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
    }
}
