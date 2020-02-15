/*
Settings.cs

Provides a struct that represents a setting as a name/value pair.

Copyright (C) 2020 Trash Bros (BlinkTheThings, Reakain)

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
using System.Collections.Generic;

namespace IniUtils
{
    /// <summary>
    ///     A name/value pair
    /// </summary>
    /// <seealso cref="System.IEquatable{Setting}" />
    public struct Setting : IEquatable<Setting>
    {
        #region Private Fields

        /// <summary>
        ///     The key value pair used to hold the setting.
        /// </summary>
        private readonly KeyValuePair<string, string> _keyValuePair;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="Setting" /> struct.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public Setting(string name, string value)
        {
            _keyValuePair = new KeyValuePair<string, string>(name, value);
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        ///     Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name => _keyValuePair.Key;

        /// <summary>
        ///     Gets the value.
        /// </summary>
        /// <value>The value.</value>
        public string Value => _keyValuePair.Value;

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        ///     Implements the operator !=.
        /// </summary>
        /// <param name="left">The left setting.</param>
        /// <param name="right">The right setting.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(Setting left, Setting right)
        {
            return !(left == right);
        }

        /// <summary>
        ///     Implements the operator ==.
        /// </summary>
        /// <param name="left">The left setting.</param>
        /// <param name="right">The right setting.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(Setting left, Setting right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///     <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance;
        ///     otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is Setting setting)
            {
                return Equals(setting);
            }

            return false;
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///     true if the current object is equal to the <paramref name="other" /> parameter;
        ///     otherwise, false.
        /// </returns>
        public bool Equals(Setting other)
        {
            return _keyValuePair.Equals(other._keyValuePair);
        }

        /// <summary>
        ///     Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        ///     A hash code for this instance, suitable for use in hashing algorithms and data
        ///     structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            return _keyValuePair.GetHashCode();
        }

        /// <summary>
        ///     Converts to string.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return $"{Name}={Value}";
        }

        #endregion Public Methods
    }
}