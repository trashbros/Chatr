/*
Interface that provides methods to encode and decode messages.

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

namespace Chatr
{
    /// <summary>
    /// Interface that provides methods to encode and decode messages.
    /// </summary>
    public interface IMessageTransform
    {
        #region Public Methods

        /// <summary>
        /// Decodes the specified encoded message.
        /// </summary>
        /// <param name="encodedMessage">The encoded message.</param>
        /// <returns>Decoded message.</returns>
        string Decode(byte[] encodedMessage);

        /// <summary>
        /// Encodes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>Encoded message.</returns>
        byte[] Encode(string message);

        #endregion Public Methods
    }
}
