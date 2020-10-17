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
using System.Diagnostics;

using JetBrains.Annotations;

using OpenCollar.Azure.ReliableQueue.Services;
using OpenCollar.Extensions;

namespace OpenCollar.Azure.ReliableQueue.Model
{
    /// <summary>A token used to represent an individual subscription to callbacks for messages arriving on a reliable queue.</summary>
    /// <seealso cref="OpenCollar.Extensions.Disposable"/>
    [DebuggerDisplay("SubscriptionToken: {" + nameof(ReliableQueueKey) + ",nq}")]
    public sealed class SubscriptionToken : Disposable
    {
        /// <summary>The event handler that will be called if a message arrives.</summary>
        [NotNull]
        private readonly EventHandler<ReceivedMessageEventArgs> _eventHandler;

        /// <summary>The key identifying the reliable queue to which the subscription belongs.</summary>
        [NotNull]
        private readonly ReliableQueueKey _reliableQueueKey;

        /// <summary>The reliable queue service to which the subscription belongs.</summary>
        [NotNull]
        private readonly ReliableQueueService _owner;

        /// <summary>Initializes a new instance of the <see cref="SubscriptionToken"/> class.</summary>
        /// <param name="reliableQueueKey">The key identifying the reliable queue to which the subscription belongs.</param>
        /// <param name="eventHandler">The event handler that will be called if a message arrives.</param>
        /// <param name="owner">The reliable queue service to which the subscription belongs.</param>
        internal SubscriptionToken([NotNull] ReliableQueueKey reliableQueueKey, [NotNull] EventHandler<ReceivedMessageEventArgs> eventHandler,
            [NotNull] ReliableQueueService owner)
        {
            _eventHandler = eventHandler;
            _reliableQueueKey = reliableQueueKey;
            _owner = owner;
        }

        /// <summary>Gets an ID that uniquely identifies this subscription.</summary>
        /// <value>The ID that uniquely identifies this subscription.</value>
        public Guid Id { get; } = Guid.NewGuid();

        /// <summary>Gets or sets the event handler that will be called if a message arrives.</summary>
        /// <value>The event handler that will be called if a message arrives.</value>
        [NotNull]
        internal EventHandler<ReceivedMessageEventArgs> EventHandler => _eventHandler;

        /// <summary>Gets or sets the key identifying the reliable queue to which the subscription belongs.</summary>
        /// <value>The key identifying the reliable queue to which the subscription belongs.</value>
        [NotNull]
        internal ReliableQueueKey ReliableQueueKey => _reliableQueueKey;

        /// <summary>Gets or sets the reliable queue service to which the subscription belongs.</summary>
        /// <value>The reliable queue service to which the subscription belongs.</value>
        [NotNull]
        internal ReliableQueueService Owner => _owner;

        /// <summary>Releases unmanaged and - optionally - managed resources.</summary>
        /// <param name="disposing">
        ///     <see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged
        ///     resources.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                _owner.Unsubscribe(this);
            }

            base.Dispose(disposing);
        }
    }
}