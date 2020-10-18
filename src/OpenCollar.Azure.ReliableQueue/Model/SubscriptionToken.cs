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

namespace OpenCollar.Azure.ReliableQueue.Model
{
    using System;
    using System.Diagnostics;

    using JetBrains.Annotations;

    using OpenCollar.Azure.ReliableQueue.Services;
    using OpenCollar.Extensions;

    /// <summary>
    /// Defines the <see cref="SubscriptionToken" />.
    /// </summary>
    [DebuggerDisplay("SubscriptionToken: {" + nameof(QueueKey) + ",nq}")]
    public sealed class SubscriptionToken : Disposable
    {
        /// <summary>
        /// Defines the _eventHandler.
        /// </summary>
        [NotNull]
        private readonly EventHandler<ReceivedMessageEventArgs> _eventHandler;

        /// <summary>
        /// Defines the _owner.
        /// </summary>
        [NotNull]
        private readonly ReliableQueueService _owner;

        /// <summary>
        /// Defines the _queueKey.
        /// </summary>
        [NotNull]
        private readonly QueueKey _queueKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionToken"/> class.
        /// </summary>
        /// <param name="queueKey">The key identifying the reliable queue to which the subscription belongs.</param>
        /// <param name="eventHandler">The event handler that will be called if a message arrives.</param>
        /// <param name="owner">The reliable queue service to which the subscription belongs.</param>
        internal SubscriptionToken([NotNull] QueueKey queueKey, [NotNull] EventHandler<ReceivedMessageEventArgs> eventHandler,
            [NotNull] ReliableQueueService owner)
        {
            _eventHandler = eventHandler;
            _queueKey = queueKey;
            _owner = owner;
        }

        /// <summary>
        /// Gets the Id.
        /// </summary>
        public Guid Id { get; } = Guid.NewGuid();

        /// <summary>
        /// Gets the EventHandler.
        /// </summary>
        [NotNull]
        internal EventHandler<ReceivedMessageEventArgs> EventHandler => _eventHandler;

        /// <summary>
        /// Gets the Owner.
        /// </summary>
        [NotNull]
        internal ReliableQueueService Owner => _owner;

        /// <summary>
        /// Gets the QueueKey.
        /// </summary>
        [NotNull]
        internal QueueKey QueueKey => _queueKey;

        /// <summary>
        /// The Dispose.
        /// </summary>
        /// <param name="disposing">The disposing<see cref="bool"/>.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _owner.Unsubscribe(this);
            }

            base.Dispose(disposing);
        }
    }
}
