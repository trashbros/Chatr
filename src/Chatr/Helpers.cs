/*
Static helper functions used in multiple classes.

Copyright (C) 2020  Trash Bros (BlinkTheThings, Reakain)

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Net;

namespace Chatr
{
    /// <summary>
    /// Static helper funcitons used in multiple classes.
    /// </summary>
    public static class Helpers
    {
        #region Public Methods

        /// <summary>
        /// Determines whether a string is a valid IP address.
        /// </summary>
        /// <param name="ipString">The string to validate.</param>
        /// <returns><c>true</c> if ipString is a valid IP address; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="ipString"/> is null.</exception>
        public static bool IsValidIP(string ipString)
        {
            return IPAddress.TryParse(ipString, out _);
        }

        /// <summary>
        /// Determines whether a string is a valid multicast IP address.
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns><c>true</c> if ipString is a valid Multicast IP address; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="ipString"/> is null.</exception>
        public static bool IsValidMulticastIP(string ipAddress)
        {
            if (IPAddress.TryParse(ipAddress, out IPAddress address))
            {
                byte firstOctet = address.GetAddressBytes()[0];

                return firstOctet >= 224 && firstOctet <= 239;
            }

            return false;
        }

        /// <summary>
        /// Determines whether the specifed port number is valid.
        /// </summary>
        /// <param name="port">The port number.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="port"/> is in the range 1024 - 65535; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsValidPort(int port)
        {
            return port > 1023 && port < 65536;
        }

        /// <summary>
        /// Converts the string representation of a port number to its integer equivalent. A return
        /// value indicates whether the operation succeeded.
        /// </summary>
        /// <param name="portString">A string containing a port number to convert.</param>
        /// <param name="port">
        /// When this method returns, contains the integer equivalent of the port number containted
        /// in <paramref name="portString"/>, if the conversion succeeded, or zero if the conversion
        /// failed. The conversion fails if the <paramref name="portString"/> is null or <see
        /// cref="string.Empty"/>, is not the correct format, or represents a number in the range
        /// 1024 - 65535. This parameter is passed uninitialized; any value originally supplied in
        /// port will be overwritten.
        /// </param>
        /// <returns>
        /// <c>true</c> if <paramref name="portString"/> was converted successfully; otherwise, <c>false</c>.
        /// </returns>
        public static bool TryParsePort(string portString, out int port)
        {
            if (Int32.TryParse(portString, out port))
            {
                if (IsValidPort(port))
                {
                    return true;
                }
                else
                {
                    port = 0;
                    return false;
                }
            }

            return false;
        }

        #endregion Public Methods
    }
}
