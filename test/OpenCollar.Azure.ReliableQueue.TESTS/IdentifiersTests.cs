using Xunit;

namespace OpenCollar.Azure.ReliableQueue.TESTS.Model
{
    public sealed class IdentifiersTests
    {
        [Fact]
        public void TestGetMessageContainerName()
        {
            Assert.Equal("reliable-queue-body-test-1", OpenCollar.Azure.ReliableQueue.Identifiers.GetMessageContainerName(new OpenCollar.Azure.ReliableQueue.Model.QueueKey("TEST+1")));
        }

        [Fact]
        public void TestGetReliableQueueName()
        {
            Assert.Equal("reliable-queue-test-1", OpenCollar.Azure.ReliableQueue.Identifiers.GetReliableQueueName(new OpenCollar.Azure.ReliableQueue.Model.QueueKey("TEST+1")));
        }

        [Fact]
        public void TestGetStateTableName()
        {
            Assert.Equal("ReliableQueueStateTestx1", OpenCollar.Azure.ReliableQueue.Identifiers.GetStateTableName(new OpenCollar.Azure.ReliableQueue.Model.QueueKey("TEST+1")));
        }

        [Fact]
        public void TestGetTopicTableName()
        {
            Assert.Equal("ReliableQueueTopicTestx1", OpenCollar.Azure.ReliableQueue.Identifiers.GetTopicTableName(new OpenCollar.Azure.ReliableQueue.Model.QueueKey("TEST+1")));
        }

        [Fact]
        public void TestMakeSafe()
        {
            Assert.Null(OpenCollar.Azure.ReliableQueue.Identifiers.MakeSafe(null));
            Assert.Empty(OpenCollar.Azure.ReliableQueue.Identifiers.MakeSafe(string.Empty));
            Assert.Equal("test", OpenCollar.Azure.ReliableQueue.Identifiers.MakeSafe("TEST"));
            Assert.Equal($"test{OpenCollar.Azure.ReliableQueue.Identifiers.SafeDelimiter}1", OpenCollar.Azure.ReliableQueue.Identifiers.MakeSafe("TEST+1"));
            Assert.Equal($"{OpenCollar.Azure.ReliableQueue.Identifiers.SafeDelimiter}test{OpenCollar.Azure.ReliableQueue.Identifiers.SafeDelimiter}1", OpenCollar.Azure.ReliableQueue.Identifiers.MakeSafe(" TEST 1"));
            Assert.Equal($"{OpenCollar.Azure.ReliableQueue.Identifiers.SafeDelimiter}test{OpenCollar.Azure.ReliableQueue.Identifiers.SafeDelimiter}1{OpenCollar.Azure.ReliableQueue.Identifiers.SafeDelimiter}", OpenCollar.Azure.ReliableQueue.Identifiers.MakeSafe(" TEST 1 "));
            Assert.Equal($"test{OpenCollar.Azure.ReliableQueue.Identifiers.SafeDelimiter}1", OpenCollar.Azure.ReliableQueue.Identifiers.MakeSafe($"TEST{OpenCollar.Azure.ReliableQueue.Identifiers.SafeDelimiter}1"));
        }

        [Fact]
        public void TestMakeTableSafe()
        {
            Assert.Null(OpenCollar.Azure.ReliableQueue.Identifiers.MakeTableSafe(null));
            Assert.Empty(OpenCollar.Azure.ReliableQueue.Identifiers.MakeTableSafe(string.Empty));
            Assert.Equal("Test", OpenCollar.Azure.ReliableQueue.Identifiers.MakeTableSafe("TEST"));
            Assert.Equal($"Test{OpenCollar.Azure.ReliableQueue.Identifiers.TableSafeDelimiter}1", OpenCollar.Azure.ReliableQueue.Identifiers.MakeTableSafe("TEST+1"));
            Assert.Equal($"{OpenCollar.Azure.ReliableQueue.Identifiers.TableSafeDelimiter}Test{OpenCollar.Azure.ReliableQueue.Identifiers.TableSafeDelimiter}1", OpenCollar.Azure.ReliableQueue.Identifiers.MakeTableSafe(" TEST 1"));
            Assert.Equal($"{OpenCollar.Azure.ReliableQueue.Identifiers.TableSafeDelimiter}Test{OpenCollar.Azure.ReliableQueue.Identifiers.TableSafeDelimiter}1{OpenCollar.Azure.ReliableQueue.Identifiers.TableSafeDelimiter}", OpenCollar.Azure.ReliableQueue.Identifiers.MakeTableSafe(" TEST 1 "));
            Assert.Equal($"Test{OpenCollar.Azure.ReliableQueue.Identifiers.TableSafeDelimiter}1", OpenCollar.Azure.ReliableQueue.Identifiers.MakeTableSafe($"TEST{OpenCollar.Azure.ReliableQueue.Identifiers.TableSafeDelimiter}1"));
        }
    }
}