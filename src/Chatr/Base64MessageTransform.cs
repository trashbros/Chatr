/*
Encode/Decode messages to/from Base64.

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
using System.Text;

namespace Chatr
{
    /// <summary>
    /// Encode/Decode messages to/from Base64.
    /// </summary>
    /// <seealso cref="Chatr.IMessageTransform"/>
    internal class Base64MessageTransform : IMessageTransform
    {
        #region Public Methods

        /// <summary>
        /// Decodes the specified Base64 encoded message.
        /// </summary>
        /// <param name="encodedMessage">The Base64 encoded message.</param>
        /// <returns>Decoded message.</returns>
        public byte[] Decode(byte[] encodedMessage)
        {
            return Convert.FromBase64String(Encoding.UTF8.GetString(encodedMessage));
        }

        /// <summary>
        /// Encodes the specified message to Base64.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>Base64 encoded message.</returns>
        public byte[] Encode(byte[] message)
        {
            return Encoding.UTF8.GetBytes(Convert.ToBase64String(message));
        }

        #endregion Public Methods
    }
}
