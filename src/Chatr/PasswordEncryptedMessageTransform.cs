/*
Encode/Decode messages using a password-based symmetric encryption algorithm.

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
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Chatr
{
    /// <summary>
    /// Encode/Decode password encrypted messages.
    /// </summary>
    /// <seealso cref="Chatr.IMessageTransform"/>
    internal class PasswordEncryptedMessageTransform : IMessageTransform
    {
        #region Private Fields

        /// <summary>
        /// The name of the encryption algorithm to use.
        /// </summary>
        private readonly string _algName;

        /// <summary>
        /// Password based key derivation function.
        /// </summary>
        private readonly Rfc2898DeriveBytes _deriveBytes;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordEncryptedMessageTransform"/> class
        /// using the specified password and encryption algorithm.
        /// </summary>
        /// <param name="password">The password to use.</param>
        /// <param name="algName">Name of the encryption algorithm to use.</param>
        public PasswordEncryptedMessageTransform(string password, string algName)
        {
            _deriveBytes = new Rfc2898DeriveBytes(password, SymmetricAlgorithm.Create(algName).BlockSize / 8);
            _algName = algName;
        }

        #endregion Public Constructors

        #region Public Methods

        /// <summary>
        /// Decodes the specified encrypted message.
        /// </summary>
        /// <param name="encryptedMessage">The encrypted message.</param>
        /// <returns>Decoded message.</returns>
        public byte[] Decode(byte[] encryptedMessage)
        {
            byte[] message = null;

            using (var algorithm = SymmetricAlgorithm.Create(_algName))
            {
                byte[] iv = new byte[algorithm.BlockSize / 8];
                byte[] cipherText = new byte[encryptedMessage.Length - iv.Length];

                Array.Copy(encryptedMessage, 0, iv, 0, iv.Length);
                Array.Copy(encryptedMessage, iv.Length, cipherText, 0, cipherText.Length);

                _deriveBytes.Salt = iv;
                algorithm.IV = iv;
                algorithm.Key = _deriveBytes.GetBytes(algorithm.KeySize / 8);

                ICryptoTransform decryptor = algorithm.CreateDecryptor(algorithm.Key, algorithm.IV);
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (BinaryReader brDecrypt = new BinaryReader(csDecrypt))
                        {
                            message = brDecrypt.ReadBytes(cipherText.Length);
                        }
                    }
                }
            }

            return message;
        }

        /// <summary>
        /// Encrypts the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>Encrypted message.</returns>
        public byte[] Encode(byte[] message)
        {
            byte[] encryptedMessage;

            using (var algorithm = SymmetricAlgorithm.Create(_algName))
            {
                _deriveBytes.Salt = algorithm.IV;
                algorithm.Key = _deriveBytes.GetBytes(algorithm.KeySize / 8);

                ICryptoTransform encryptor = algorithm.CreateEncryptor(algorithm.Key, algorithm.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (BinaryWriter bwEncrypt = new BinaryWriter(csEncrypt))
                        {
                            bwEncrypt.Write(message);
                        }

                        encryptedMessage = algorithm.IV.Concat(msEncrypt.ToArray()).ToArray();
                    }
                }
            }

            return encryptedMessage;
        }

        #endregion Public Methods
    }
}
