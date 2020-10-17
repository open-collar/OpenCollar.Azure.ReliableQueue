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
    /// <summary>Defines the potential states in which a record may be.</summary>
    public enum MessageState
    {
        /// <summary>
        ///     The state of the message is unknown or undefined.  This is a sentinel used to detect unset values and will normally result in an exception being
        ///     thrown.
        /// </summary>
        Unknown = 0,

        ///<summary>The message has been passed from the sender to the service, but not yet processed.</summary>
        New,

        ///<summary>The message has been queued and is ready for receipt.</summary>
        Queued,

        ///<summary>The message has been dequeued and is being processed by the recipient.</summary>
        Processing,

        ///<summary>The message has failed processing too many times and is being moved to the dead letter queue.</summary>
        Failed
    }
}