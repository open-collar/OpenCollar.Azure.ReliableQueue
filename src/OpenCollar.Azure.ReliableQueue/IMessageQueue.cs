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

namespace OpenCollar.Azure.ReliableQueue
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using JetBrains.Annotations;

    using OpenCollar.Azure.ReliableQueue.Model;

    /// <summary>
    ///     Defines the <see cref="IReliableQueue" />.
    /// </summary>
    public interface IReliableQueue
    {
        /// <summary>
        ///     Gets a value indicating whether CanReceive.
        /// </summary>
        public bool CanReceive { get; }

        /// <summary>
        ///     Gets a value indicating whether CanSend.
        /// </summary>
        public bool CanSend { get; }

        /// <summary>
        ///     Gets the QueueKey.
        /// </summary>
        public QueueKey QueueKey { get; }

        /// <summary>
        ///     The IsSubscribed.
        /// </summary>
        /// <returns>
        ///     <see langword="true" /> if the specified reliable queue key is subscribed; otherwise, <see langword="false" />.
        /// </returns>
        public bool IsSubscribed();

        /// <summary>
        ///     Called when a trigger (e.g. EventGrid or StorageQueue) receives a notification of a message to be
        ///     processed. The notification can be intended for any reliable queue.
        /// </summary>
        /// <param name="base64">
        ///     The Base-64 encoded JSON representation of the serialized message.
        /// </param>
        /// <param name="timeout">
        ///     The timeout <see cref="TimeSpan" />.
        /// </param>
        /// <param name="cancellationToken">
        ///     The cancellationToken <see cref="CancellationToken" />.
        /// </param>
        /// <returns>
        ///     A task that processes the message supplied.
        /// </returns>
        public Task OnReceivedAsync([NotNull] string base64, TimeSpan? timeout = null, [CanBeNull] CancellationToken? cancellationToken = null);

        /// <summary>
        ///     The SendMessageAsync.
        /// </summary>
        /// <param name="body">
        ///     The body of the message to send. This can be <see langword="null" /> or contain any content.
        /// </param>
        /// <param name="topic">
        ///     The topic <see cref="Topic" />.
        /// </param>
        /// <param name="timeout">
        ///     The timeout <see cref="TimeSpan" />.
        /// </param>
        /// <param name="cancellationToken">
        ///     The cancellationToken <see cref="CancellationToken" />.
        /// </param>
        /// <returns>
        ///     A task that performs the action specified.
        /// </returns>
        public Task SendMessageAsync([CanBeNull] byte[]? body, [CanBeNull] Topic? topic = null, TimeSpan? timeout = null,
            [CanBeNull] CancellationToken? cancellationToken = null);

        /// <summary>
        ///     The SendMessageAsync.
        /// </summary>
        /// <param name="body">
        ///     The body of the message to send. This can be <see langword="null" /> or contain any content.
        /// </param>
        /// <param name="topic">
        ///     The topic <see cref="Topic" />.
        /// </param>
        /// <param name="timeout">
        ///     The timeout <see cref="TimeSpan" />.
        /// </param>
        /// <param name="cancellationToken">
        ///     The cancellationToken <see cref="CancellationToken" />.
        /// </param>
        /// <returns>
        ///     A task that performs the action specified.
        /// </returns>
        public Task SendMessageAsync([CanBeNull] Stream? body, [CanBeNull] Topic? topic = null, TimeSpan? timeout = null,
            [CanBeNull] CancellationToken? cancellationToken = null);

        /// <summary>
        ///     The SendMessageAsync.
        /// </summary>
        /// <param name="body">
        ///     The body of the message to send. This can be <see langword="null" /> or contain any content.
        /// </param>
        /// <param name="topic">
        ///     The topic <see cref="Topic" />.
        /// </param>
        /// <param name="timeout">
        ///     The timeout <see cref="TimeSpan" />.
        /// </param>
        /// <param name="cancellationToken">
        ///     The cancellationToken <see cref="CancellationToken" />.
        /// </param>
        /// <returns>
        ///     A task that performs the action specified.
        /// </returns>
        public Task SendMessageAsync([CanBeNull] string? body, [CanBeNull] Topic? topic = null, TimeSpan? timeout = null,
            [CanBeNull] CancellationToken? cancellationToken = null);

        /// <summary>
        ///     The Subscribe.
        /// </summary>
        /// <param name="callbackHandler">
        ///     The event handler to call when a message arrives.
        /// </param>
        /// <returns>
        ///     A token that can be used to unsubscribe, either by calling <see cref="Unsubscribe" /> or by disposing.
        /// </returns>
        public SubscriptionToken Subscribe([NotNull] EventHandler<ReceivedMessageEventArgs> callbackHandler);

        /// <summary>
        ///     The Unsubscribe.
        /// </summary>
        /// <param name="token">
        ///     The token returned by <see cref="Subscribe" /> when the subscription was created.
        /// </param>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        public bool Unsubscribe([NotNull] SubscriptionToken token);
    }
}