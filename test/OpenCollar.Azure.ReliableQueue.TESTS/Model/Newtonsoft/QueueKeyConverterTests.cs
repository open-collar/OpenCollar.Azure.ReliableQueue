using System.IO;

using OpenCollar.Azure.ReliableQueue.Model;

using Xunit;

#pragma warning disable CS1718 // Comparison made to same variable

namespace OpenCollar.Azure.ReliableQueue.TESTS.Model.Newtonsoft
{
    public sealed class QueueKeyConverterTests
    {
        [Fact]
        public void TestRoundTrip()
        {
            var a = new OpenCollar.Azure.ReliableQueue.Model.QueueKey("AAA");

            var x = new QueueKeyTestObject();
            x.QueueKey = a;

            var serializer = new global::Newtonsoft.Json.JsonSerializer();

            using var stream = new MemoryStream();

            var writer = new StreamWriter(stream);

            var jsonWriter = new global::Newtonsoft.Json.JsonTextWriter(writer);

            serializer.Serialize(jsonWriter, x);

            jsonWriter.Flush();

            stream.Seek(0, SeekOrigin.Begin);

            var reader = new StreamReader(stream);

            var jsonReader = new global::Newtonsoft.Json.JsonTextReader(reader);

            var y = serializer.Deserialize<QueueKeyTestObject>(jsonReader);

            Assert.NotNull(y);
            Assert.NotNull(y.QueueKey);
            Assert.Equal(x.QueueKey, y.QueueKey);
        }

        [Fact]
        public void TestRoundTripNull()
        {
            var x = new QueueKeyTestObject();
            x.QueueKey = null;

            var serializer = new global::Newtonsoft.Json.JsonSerializer();

            using var stream = new MemoryStream();

            var writer = new StreamWriter(stream);

            var jsonWriter = new global::Newtonsoft.Json.JsonTextWriter(writer);

            serializer.Serialize(jsonWriter, x);

            jsonWriter.Flush();

            stream.Seek(0, SeekOrigin.Begin);

            var reader = new StreamReader(stream);

            var jsonReader = new global::Newtonsoft.Json.JsonTextReader(reader);

            var y = serializer.Deserialize<QueueKeyTestObject>(jsonReader);

            Assert.NotNull(y);
            Assert.Null(y.QueueKey);
        }

        internal class QueueKeyTestObject
        {
            [global::Newtonsoft.Json.JsonConverter(typeof(OpenCollar.Azure.ReliableQueue.Model.Newtonsoft.QueueKeyConverter))]
            public QueueKey QueueKey { get; set; }
        }
    }
}