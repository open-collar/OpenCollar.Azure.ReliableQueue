using System.IO;

using OpenCollar.Azure.ReliableQueue.Model;

using Xunit;

#pragma warning disable CS1718 // Comparison made to same variable

namespace OpenCollar.Azure.ReliableQueue.TESTS.Model.Text.Json
{
    public sealed class QueueKeyConverterTests
    {
        [Fact]
        public void TestRoundTrip()
        {
            var a = new OpenCollar.Azure.ReliableQueue.Model.QueueKey("AAA");

            var x = new QueueKeyTestObject();
            x.QueueKey = a;

            using var stream = new MemoryStream();

            var jsonWriter = new System.Text.Json.Utf8JsonWriter(stream);

            System.Text.Json.JsonSerializer.Serialize(jsonWriter, x);

            jsonWriter.Flush();

            stream.Seek(0, SeekOrigin.Begin);

            var jsonReader = new System.Text.Json.Utf8JsonReader(stream.ToArray());

            var y = System.Text.Json.JsonSerializer.Deserialize<QueueKeyTestObject>(ref jsonReader);

            Assert.NotNull(y);
            Assert.NotNull(y.QueueKey);
            Assert.Equal(x.QueueKey, y.QueueKey);
        }

        [Fact]
        public void TestRoundTripNull()
        {
            var x = new QueueKeyTestObject();
            x.QueueKey = null;

            using var stream = new MemoryStream();

            var jsonWriter = new System.Text.Json.Utf8JsonWriter(stream);

            System.Text.Json.JsonSerializer.Serialize(jsonWriter, x);

            jsonWriter.Flush();

            stream.Seek(0, SeekOrigin.Begin);

            var jsonReader = new System.Text.Json.Utf8JsonReader(stream.ToArray());

            var y = System.Text.Json.JsonSerializer.Deserialize<QueueKeyTestObject>(ref jsonReader);

            Assert.NotNull(y);
            Assert.Null(y.QueueKey);
        }

        internal class QueueKeyTestObject
        {
            [System.Text.Json.Serialization.JsonConverter(typeof(OpenCollar.Azure.ReliableQueue.Model.Text.Json.QueueKeyConverter))]
            public QueueKey QueueKey { get; set; }
        }
    }
}