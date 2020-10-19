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
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

using OpenCollar.Azure.ReliableQueue;

#pragma warning disable CS8604 // Possible null reference argument.

namespace ReliableQueueReceiverFunctionTestRig
{
    /// <summary>
    ///     This function runs just once when the host starts-up and can be used to complete initialization where necessary.
    /// </summary>
    public class InitializationFunction
    {
        /// <summary>
        ///     A count of the number of times this function has run.
        /// </summary>
        private int _runCount;

        private readonly IReliableQueue _reliableQueue;

        public InitializationFunction(IReliableQueueService ReliableQueueService)
        {
            _reliableQueue = ReliableQueueService["TEST+1"];
            _reliableQueue.Subscribe(OnMessageReceived);
        }

        private void OnMessageReceived(object? sender, ReceivedMessageEventArgs e)
        {
            using var json = e.GetBodyAsStreamAsync().Result;

            json.Seek(0, SeekOrigin.Begin);

            using var reader = new StreamReader(json);

            Console.WriteLine(
                $"Received: {e.Topic}; Thread ID: [{Thread.CurrentThread.ManagedThreadId}]; Message: {reader.ReadToEnd()}");

            e.Handled = true;
        }

        /// <summary>
        ///     This function runs just once when the host starts-up and can be used to complete initialization where necessary.
        /// </summary>
        /// <param name="timer">
        ///     My timer that fired this function.
        /// </param>
        /// <param name="log">
        ///     The log in which to record information.
        /// </param>
#pragma warning disable CA1801 // Parameter is never used. Remove the parameter or use it in the method body

        [FunctionName("InitializationFunction")]
        public void Run([TimerTrigger(@"99:23:59" /* Fire every 100 days. */, RunOnStartup = true)]
            TimerInfo timer, ILogger log)
#pragma warning restore CA1801 // Parameter is never used. Remove the parameter or use it in the method body
        {
            if(Interlocked.Increment(ref _runCount) > 1)
            {
                // On the off-chance this app has been in memory for more than 100 days then we'll drop straight out to
                // avoid re-initializing.
                return;
            }

            try
            {
                log.LogInformation(@"Initializing function app.");
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch(Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                log.LogError(ex, $@"Initialization function failed with error: ""{ex.Message}"".");
            }
        }
    }
}