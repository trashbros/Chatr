/*
Encode/Decode messages to/from UTF8.

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

using System.Text;

namespace Chatr
{
    /// <summary>
    /// Encode/Decode messages to/from UTF8.
    /// </summary>
    /// <seealso cref="Chatr.IMessageTransform"/>
    internal class PlainTextMessageTransform : IMessageTransform
    {
        #region Public Methods

        /// <summary>
        /// Decodes the specified UTF8 encoded message.
        /// </summary>
        /// <param name="encodedMessage">The UTF8 encoded message.</param>
        /// <returns>Decoded message.</returns>
        public string Decode(byte[] encodedMessage)
        {
            return Encoding.UTF8.GetString(encodedMessage);
        }

        /// <summary>
        /// Encodes the specified message to UTF8.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>UTF8 encoded message.</returns>
        public byte[] Encode(string message)
        {
            return Encoding.UTF8.GetBytes(message);
        }

        #endregion Public Methods
    }
}
