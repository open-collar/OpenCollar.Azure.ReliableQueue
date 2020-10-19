using System.IO;

using OpenCollar.Azure.ReliableQueue.Model;

using Xunit;

#pragma warning disable CS1718 // Comparison made to same variable

namespace OpenCollar.Azure.ReliableQueue.TESTS.Model.Text.Json
{
    public sealed class TopicConverterTests
    {
        [Fact]
        public void TestRoundTrip()
        {
            var a = new OpenCollar.Azure.ReliableQueue.Model.Topic("AAA");

            var x = new TopicTestObject();
            x.Topic = a;

            using var stream = new MemoryStream();

            var jsonWriter = new System.Text.Json.Utf8JsonWriter(stream);

            System.Text.Json.JsonSerializer.Serialize(jsonWriter, x);

            jsonWriter.Flush();

            stream.Seek(0, SeekOrigin.Begin);

            var jsonReader = new System.Text.Json.Utf8JsonReader(stream.ToArray());

            var y = System.Text.Json.JsonSerializer.Deserialize<TopicTestObject>(ref jsonReader);

            Assert.NotNull(y);
            Assert.NotNull(y.Topic);
            Assert.Equal(x.Topic, y.Topic);
        }

        [Fact]
        public void TestRoundTripNull()
        {
            var x = new TopicTestObject();
            x.Topic = null;

            using var stream = new MemoryStream();

            var jsonWriter = new System.Text.Json.Utf8JsonWriter(stream);

            System.Text.Json.JsonSerializer.Serialize(jsonWriter, x);

            jsonWriter.Flush();

            stream.Seek(0, SeekOrigin.Begin);

            var jsonReader = new System.Text.Json.Utf8JsonReader(stream.ToArray());

            var y = System.Text.Json.JsonSerializer.Deserialize<TopicTestObject>(ref jsonReader);

            Assert.NotNull(y);
            Assert.Null(y.Topic);
        }

        internal class TopicTestObject
        {
            [System.Text.Json.Serialization.JsonConverter(typeof(OpenCollar.Azure.ReliableQueue.Model.Text.Json.TopicConverter))]
            public Topic Topic { get; set; }
        }
    }
}