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
    using System.Diagnostics;
    using System.Globalization;

    using JetBrains.Annotations;

    /// <summary>
    /// Defines the <see cref="Identity" />.
    /// </summary>
    internal static class Identity
    {
        /// <summary>
        /// Defines the _identity.
        /// </summary>
        [NotNull]
        private static readonly Lazy<string> _identity = new Lazy<string>(() => GetCurrentIdentifier());

        /// <summary>
        /// Gets the Current.
        /// </summary>
        [NotNull]
        public static string Current => _identity.Value;

        /// <summary>
        /// The GetCurrentIdentifier.
        /// </summary>
        /// <returns>A string containing the unique identity for the current process.</returns>
        [NotNull]
        private static string GetCurrentIdentifier()
        {
            var hostName = Environment.MachineName.ToUpperInvariant();
            var hostProcessId = Process.GetCurrentProcess().Id;
            return string.Concat(Identifiers.MakeSafe(hostName), "-", hostProcessId.ToString(CultureInfo.InvariantCulture));
        }
    }
}
