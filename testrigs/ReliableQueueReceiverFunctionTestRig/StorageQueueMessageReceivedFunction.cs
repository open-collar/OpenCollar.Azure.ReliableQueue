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

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

using OpenCollar.Azure.ReliableQueue;

namespace ReliableQueueReceiverFunctionTestRig
{
    public sealed class StorageQueueMessageReceivedFunction
    {
        private readonly IReliableQueue _reliableQueue;

        public StorageQueueMessageReceivedFunction(IReliableQueueService ReliableQueueService)
        {
            _reliableQueue = ReliableQueueService["TEST+1"];
        }

        [FunctionName("StorageQueueMessageReceivedFunction")]
        public void Run([QueueTrigger("message-queue-test-1", Connection = "ReliableQueues:Queues:TEST+1:StorageConnectionString")] string myQueueItem, ILogger log)
        {
            _reliableQueue.OnReceivedAsync(myQueueItem);
        }
    }
}