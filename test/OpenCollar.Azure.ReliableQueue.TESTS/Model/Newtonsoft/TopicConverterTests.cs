using System.IO;

using OpenCollar.Azure.ReliableQueue.Model;

using Xunit;

#pragma warning disable CS1718 // Comparison made to same variable

namespace OpenCollar.Azure.ReliableQueue.TESTS.Model.Newtonsoft
{
    public sealed class TopicConverterTests
    {
        [Fact]
        public void TestRoundTrip()
        {
            var a = new OpenCollar.Azure.ReliableQueue.Model.Topic("AAA");

            var x = new TopicTestObject();
            x.Topic = a;

            var serializer = new global::Newtonsoft.Json.JsonSerializer();

            using var stream = new MemoryStream();

            var writer = new StreamWriter(stream);

            var jsonWriter = new global::Newtonsoft.Json.JsonTextWriter(writer);

            serializer.Serialize(jsonWriter, x);

            jsonWriter.Flush();

            stream.Seek(0, SeekOrigin.Begin);

            var reader = new StreamReader(stream);

            var jsonReader = new global::Newtonsoft.Json.JsonTextReader(reader);

            var y = serializer.Deserialize<TopicTestObject>(jsonReader);

            Assert.NotNull(y);
            Assert.NotNull(y.Topic);
            Assert.Equal(x.Topic, y.Topic);
        }

        [Fact]
        public void TestRoundTripNull()
        {
            var x = new TopicTestObject();
            x.Topic = null;

            var serializer = new global::Newtonsoft.Json.JsonSerializer();

            using var stream = new MemoryStream();

            var writer = new StreamWriter(stream);

            var jsonWriter = new global::Newtonsoft.Json.JsonTextWriter(writer);

            serializer.Serialize(jsonWriter, x);

            jsonWriter.Flush();

            stream.Seek(0, SeekOrigin.Begin);

            var reader = new StreamReader(stream);

            var jsonReader = new global::Newtonsoft.Json.JsonTextReader(reader);

            var y = serializer.Deserialize<TopicTestObject>(jsonReader);

            Assert.NotNull(y);
            Assert.Null(y.Topic);
        }

        internal class TopicTestObject
        {
            [global::Newtonsoft.Json.JsonConverter(typeof(OpenCollar.Azure.ReliableQueue.Model.Newtonsoft.TopicConverter))]
            public Topic Topic { get; set; }
        }
    }
}