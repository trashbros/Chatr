/*
Chatr Monitor

Low level utility to send and receive messages
over a Chatr channel. Useful for debugging purposes.

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

using Chatr;
using CommandLine;
using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace ChatrMonitor
{
    internal class Program
    {
        #region Private Methods

        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        private static void Main(string[] args)
        {
            // Parse and use the arguments
            Parser.Default.ParseArguments<Options>(args).WithParsed(o =>
            {
                // Create a new transform to encode/decode messages
                var xform = new PasswordEncryptedMessageTransform(o.Password, "AES");

                // Create a new connection to send/recv messages
                var conn = new MulticastConnection(IPAddress.Parse(o.LocalIP), new IPEndPoint(IPAddress.Parse(o.MulticastIP), o.MulticastPort));

                // Handle messages as they are received from the connection
                conn.MessageReceivedEventHandler += (sender, eventArgs) =>
                {
                    try
                    {
                        // Decode the message
                        string message = Encoding.UTF8.GetString(xform.Decode(eventArgs.Message));

                        // Print the message to the console
                        Console.WriteLine(message);
                    }
                    catch (CryptographicException)
                    {
                        // Print a message indicating that we recevied a message we couldn't decode.
                        Console.WriteLine($"Unable to decode message from {eventArgs.SenderIP}");
                    }
                };

                // Open the connection
                conn.Open();

                // Read the first message to send
                string message = Console.ReadLine();

                // Keep processing message until the user says to quit
                while (!message.Equals("/quit", StringComparison.Ordinal))
                {
                    // Send the message
                    conn.Send(xform.Encode(Encoding.UTF8.GetBytes(message)));

                    // Read the next message
                    message = Console.ReadLine();
                }

                // Close the connection
                conn.Close();
            });
        }

        #endregion Private Methods

        #region Public Classes

        /// <summary>
        /// Command line options
        /// </summary>
        public class Options
        {
            #region Public Properties

            [Option('l', "local-ip", Required = true, HelpText = "Set local IP address.")]
            public string LocalIP { get; set; }

            [Option('m', "multicast-ip", Required = true, HelpText = "Set multicast IP address.")]
            public string MulticastIP { get; set; }

            [Option('n', "multicast-port", Required = true, HelpText = "Set multicast port number.")]
            public int MulticastPort { get; set; }

            [Option('p', "password", Required = true, HelpText = "Set password.")]
            public string Password { get; set; }

            #endregion Public Properties
        }

        #endregion Public Classes
    }
}
