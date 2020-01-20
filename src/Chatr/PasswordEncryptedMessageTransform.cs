/*
Encode/Decode messages using a password based symmetric encryption algorithm
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
    internal class PasswordEncryptedMessageTransform : IMessageTransform
    {
        private readonly Rfc2898DeriveBytes _deriveBytes;
        private readonly string _algName;

        public PasswordEncryptedMessageTransform(string password, string algName)
        {
            _deriveBytes = new Rfc2898DeriveBytes(password, SymmetricAlgorithm.Create(algName).BlockSize / 8);
            _algName = algName;
        }

        public string Decode(byte[] encodedMessage)
        {
            string message = null;

            using (var algorithm = SymmetricAlgorithm.Create(_algName))
            {
                byte[] iv = new byte[algorithm.BlockSize / 8];
                byte[] cipherText = new byte[encodedMessage.Length - iv.Length];

                Array.Copy(encodedMessage, 0, iv, 0, iv.Length);
                Array.Copy(encodedMessage, iv.Length, cipherText, 0, cipherText.Length);

                _deriveBytes.Salt = iv;
                algorithm.IV = iv;
                algorithm.Key = _deriveBytes.GetBytes(algorithm.KeySize / 8);

                ICryptoTransform decryptor = algorithm.CreateDecryptor(algorithm.Key, algorithm.IV);
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            message = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return message;
        }

        public byte[] Encode(string message)
        {
            byte[] encrypted;

            using (var algorithm = SymmetricAlgorithm.Create(_algName))
            {
                _deriveBytes.Salt = algorithm.IV;
                algorithm.Key = _deriveBytes.GetBytes(algorithm.KeySize / 8);

                ICryptoTransform encryptor = algorithm.CreateEncryptor(algorithm.Key, algorithm.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(message);
                        }

                        encrypted = algorithm.IV.Concat(msEncrypt.ToArray()).ToArray();
                    }
                }
            }

            return encrypted;
        }
    }
}