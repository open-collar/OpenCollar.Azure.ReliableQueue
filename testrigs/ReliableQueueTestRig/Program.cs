/*
 * This file is part of OpenCollar.Azure.ReliableQueue.
 *
 * OpenCollar.Azure.ReliableQueue is free software: you can redistribute it
 * and/or modify it under the terms of the GNU General Public License as published
 * by the Free Software Foundation, either version 3 of the License, or (at your
 * option) any later version.
 *
 * OpenCollar.Azure.ReliableQueue is distributed in the hope that it will be
 * useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public
 * License for more details.
 *
 * You should have received a copy of the GNU General Public License along with
 * OpenCollar.Azure.ReliableQueue.  If not, see <https://www.gnu.org/licenses/>.
 *
 * Copyright © 2020 Jonathan Evans (jevans@open-collar.org.uk).
 */

using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using OpenCollar.Azure.ReliableQueue;
using OpenCollar.Azure.ReliableQueue.Model;
using OpenCollar.Azure.ReliableQueue.Services;

#pragma warning disable CS8604 // Possible null reference argument.

namespace ReliableQueueTestRig
{
    internal class Program
    {
        private static readonly Random _random = new Random();

        private static async Task Main(string[] args)
        {
            Console.WriteLine("Starting Message Queue Test Rig");

            var startup = new Startup();

            var servicesCollection = new ServiceCollection();

            startup.Configure(servicesCollection);

            var services = servicesCollection.BuildServiceProvider();

            // await TestStorageAsync(services);

            // await TestStateAsync(services);

            // await TestQueueSenderAsync(services);

            await TestQueueAsync(services);
        }

        private static async Task TestQueueAsync(ServiceProvider services)
        {
            const string queueName = "TEST+1";

            const int max = 100;

            var queue = services.GetService<IReliableQueueService>()[queueName];
            Console.WriteLine("Starting tests");

            var accumulatedSend = TimeSpan.Zero;

            for(var n = 0; n < max; ++n)
            {
                var testBody = $@"Test BLOB Content ({n})";

                await using var stream = new MemoryStream();
                await using var writer = new StreamWriter(stream, Encoding.UTF8);
                await writer.WriteAsync(testBody);
                await writer.FlushAsync();
                stream.Seek(0, SeekOrigin.Begin);

                var start = DateTime.UtcNow;
                await queue.SendMessageAsync(stream, "topic_" + _random.Next(1, 5));
                accumulatedSend = accumulatedSend + (DateTime.UtcNow - start);
            }

            // Leave enough time for the queue processor to finish.
            Thread.Sleep(10_000);

            Console.WriteLine(
                $"Total Time Elapsed (Send): {accumulatedSend.TotalMilliseconds.ToString("F1", CultureInfo.InvariantCulture)}ms.; Average Time Per Call: {(accumulatedSend.TotalMilliseconds / max).ToString("F1", CultureInfo.InvariantCulture)}ms");
        }

        private static async Task TestQueueSenderAsync(ServiceProvider services)
        {
            var queue = services.GetService<IReliableQueueSenderService>();

            const string queueName = "TEST+1";

            const int max = 100;

            Console.WriteLine("Starting tests");

            var accumulatedCreate = TimeSpan.Zero;

            var configuration = services.GetService<IReliableQueueConfigurationService>()[queueName];

            for(var n = 0; n < max; ++n)
            {
                var message = Message.CreateNew(queueName, configuration, "topic-1");

                var start = DateTime.UtcNow;
                await queue.SendMessageAsync(queueName, message);
                accumulatedCreate = accumulatedCreate + (DateTime.UtcNow - start);
            }

            Console.WriteLine(
                $"Total Time Elapsed (Queue): {accumulatedCreate.TotalMilliseconds.ToString("F1", CultureInfo.InvariantCulture)}ms.; Average Time Per Call: {(accumulatedCreate.TotalMilliseconds / max).ToString("F1", CultureInfo.InvariantCulture)}ms");
        }

        private static async Task TestStateAsync(ServiceProvider services)
        {
            var state = services.GetService<IMessageStateService>();

            const string queue = "TEST+1";

            const int max = 100;

            Console.WriteLine("Starting tests");

            var accumulatedCreate = TimeSpan.Zero;

            var configuration = services.GetService<IReliableQueueConfigurationService>()[queue];

            for(var n = 0; n < max; ++n)
            {
                var message = Message.CreateNew(queue, configuration, "topic-1");

                var start = DateTime.UtcNow;
                await state.AddNewMessageAsync(queue, message);
                accumulatedCreate = accumulatedCreate + (DateTime.UtcNow - start);
            }

            Console.WriteLine(
                $"Total Time Elapsed (Create): {accumulatedCreate.TotalMilliseconds.ToString("F1", CultureInfo.InvariantCulture)}ms.; Average Time Per Call: {(accumulatedCreate.TotalMilliseconds / max).ToString("F1", CultureInfo.InvariantCulture)}ms");
        }

        private static async Task TestStorageAsync(ServiceProvider services)
        {
            var storage = services.GetService<IMessageStorageService>();

            const string queue = "TEST+1";

            const int max = 100;

            Console.WriteLine("Starting tests");

            var accumulatedReadTime = TimeSpan.Zero;
            var accumulatedWriteTime = TimeSpan.Zero;
            var accumulatedDeleteTime = TimeSpan.Zero;
            DateTime start;

            var configuration = services.GetService<IReliableQueueConfigurationService>()[queue];

            for(var n = 0; n < max; ++n)
            {
                var message = Message.CreateNew(queue, configuration, "topic-1");
                var testBody = $@"Test BLOB Content ({n}): ""{message.Id.ToString("D", CultureInfo.InvariantCulture)}""";

                using(var stream = new MemoryStream())
                {
                    using(var writer = new StreamWriter(stream, Encoding.UTF8))
                    {
                        await writer.WriteAsync(testBody);
                        await writer.FlushAsync();
                        stream.Seek(0, SeekOrigin.Begin);

                        start = DateTime.UtcNow;
                        await storage.WriteMessageAsync(queue, message, stream);
                        accumulatedWriteTime = accumulatedWriteTime + (DateTime.UtcNow - start);
                    }
                }

                using(var stream = new MemoryStream())
                {
                    using(var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        start = DateTime.UtcNow;
                        await storage.ReadMessageAsync(queue, message, stream);

                        stream.Seek(0, SeekOrigin.Begin);

                        var body = await reader.ReadToEndAsync();
                        accumulatedReadTime = accumulatedReadTime + (DateTime.UtcNow - start);

                        if(body != testBody)
                        {
                            Console.WriteLine($"FAIL: \"{body}\" != \"{testBody}\"");
                        }
                    }
                }

                start = DateTime.UtcNow;
                await storage.DeleteMessageAsync(queue, message);
                accumulatedDeleteTime = accumulatedDeleteTime + (DateTime.UtcNow - start);
            }

            Console.WriteLine(
                $"Total Time Elapsed (Write): {accumulatedWriteTime.TotalMilliseconds.ToString("F1", CultureInfo.InvariantCulture)}ms.; Average Time Per Call: {(accumulatedWriteTime.TotalMilliseconds / max).ToString("F1", CultureInfo.InvariantCulture)}ms");

            Console.WriteLine(
                $"Total Time Elapsed (Read): {accumulatedReadTime.TotalMilliseconds.ToString("F1", CultureInfo.InvariantCulture)}ms.; Average Time Per Call: {(accumulatedReadTime.TotalMilliseconds / max).ToString("F1", CultureInfo.InvariantCulture)}ms");

            Console.WriteLine(
                $"Total Time Elapsed (Delete): {accumulatedDeleteTime.TotalMilliseconds.ToString("F1", CultureInfo.InvariantCulture)}ms.; Average Time Per Call: {(accumulatedDeleteTime.TotalMilliseconds / max).ToString("F1", CultureInfo.InvariantCulture)}ms");
        }
    }
}