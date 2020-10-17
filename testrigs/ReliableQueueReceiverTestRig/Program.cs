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
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using OpenCollar.Azure.ReliableQueue;

namespace ReliableQueueReceiverTestRig
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            Console.WriteLine("Starting Message Queue Listener Test Rig");

            var startup = new Startup();

            var servicesCollection = new ServiceCollection();

            startup.Configure(servicesCollection);

            var services = servicesCollection.BuildServiceProvider();

            await TestQueueListenerAsync(services);
        }

        private static async Task TestQueueListenerAsync(ServiceProvider services)
        {
            const string queueName = "TEST+1";

            var queue = services.GetService<IReliableQueueService>()[queueName];

            using(queue.Subscribe(delegate (object? sender, ReceivedMessageEventArgs args)
            {
                Console.WriteLine(
                    $"Received: {args.Topic}; Thread ID: [{Thread.CurrentThread.ManagedThreadId}]; Message: {args.GetBodyAsStringAsync().Result}");
                args.Handled = true;
            }))
            {
                Console.WriteLine("Listening...");

                Console.WriteLine("Press <space> to end.");

                while(Console.ReadKey().KeyChar != ' ')
                {
                    // Keep waiting for the space key.
                    Thread.Sleep(5);
                }
            }
        }
    }
}