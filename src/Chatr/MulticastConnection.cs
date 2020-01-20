/*
Connection for sending and receiving messages over multicast UDP sockets.

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
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Chatr
{
    /// <summary>
    /// Connection for sending and receiving messages over multicast UDP sockets.
    /// </summary>
    /// <seealso cref="System.IDisposable"/>
    public class MulticastConnection : IDisposable
    {
        #region Private Fields

        /// <summary>
        /// The local IP address used for sending and receiving data.
        /// </summary>
        private readonly IPAddress _localIP;

        /// <summary>
        /// The message transform used when sending and receiving data.
        /// </summary>
        private readonly IMessageTransform _messageTransform;

        /// <summary>
        /// The multicast end point used for sending and receiving data.
        /// </summary>
        private readonly IPEndPoint _multicastEndPoint;

        /// <summary>
        /// UdpClient for sending data.
        /// </summary>
        private readonly UdpClient _sendClient;

        /// <summary>
        /// Has Disposed already been called?
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// The socket used for receiving data.
        /// </summary>
        private Socket _receiveSocket;

        #endregion Private Fields

        #region Protected Methods

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">
        /// <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release
        /// only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    StopReceiving();

                    _sendClient.Close();
                }

                _disposed = true;
            }
        }

        #endregion Protected Methods

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MulticastConnection"/> class.
        /// </summary>
        /// <param name="localIP">The local IP address to use for sending and receiving data.</param>
        /// <param name="multicastEndPoint">
        /// The multicast end point to use for sending and receiving data.
        /// </param>
        /// <param name="messageTransform">
        /// The message transform to use when sending and receiving data.
        /// </param>
        public MulticastConnection(IPAddress localIP, IPEndPoint multicastEndPoint, IMessageTransform messageTransform)
        {
            _localIP = localIP;
            _multicastEndPoint = multicastEndPoint;
            _messageTransform = messageTransform;
            _sendClient = new UdpClient(new IPEndPoint(_localIP, 0));
        }

        #endregion Public Constructors

        #region Public Events

        /// <summary>
        /// Occurs when a properly decoded message is received.
        /// </summary>
        public event EventHandler<MessageReceivedEventArgs> MessageReceivedEventHandler;

        #endregion Public Events

        #region Public Properties

        /// <summary>
        /// Gets the local IP address being used by this connection.
        /// </summary>
        /// <value>The local IP Address.</value>
        public IPAddress LocalIP
        {
            get => new IPAddress(_localIP.GetAddressBytes());
        }

        /// <summary>
        /// Gets the multicast end point being used by this connection.
        /// </summary>
        /// <value>The multicast end point.</value>
        public IPEndPoint MulticastEndPoint
        {
            get => new IPEndPoint(new IPAddress(_multicastEndPoint.Address.GetAddressBytes()), _multicastEndPoint.Port);
        }

        /// <summary>
        /// Gets a value indicating whether receive has started on this connection.
        /// </summary>
        /// <value><c>true</c> if receive has started; otherwise, <c>false</c>.</value>
        public bool ReceiveStarted
        {
            get; private set;
        }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Sends the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Send(string message)
        {
            byte[] datagram = _messageTransform.Encode(message);

            try
            {
                _sendClient.Send(datagram, datagram.Length, _multicastEndPoint);
            }
            catch (SocketException e)
            {
                Console.WriteLine(e);
            }
        }

        /// <summary>
        /// Start receiving messages.
        /// </summary>
        /// <exception cref="AlreadyReceivingException">Recveving has already been started.</exception>
        public async Task StartReceiving()
        {
            if (ReceiveStarted)
            {
                throw new AlreadyReceivingException();
            }

            try
            {
                using (_receiveSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
                {
                    _receiveSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    _receiveSocket.Bind(new IPEndPoint(IPAddress.Any, _multicastEndPoint.Port));
                    _receiveSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(_multicastEndPoint.Address, _localIP));

                    IPEndPoint remoteIPEndPoint = new IPEndPoint(_multicastEndPoint.Address, 0);
                    EndPoint remoteEndPoint = remoteIPEndPoint;

                    await Task.Run(() =>
                    {
                        try
                        {
                            ReceiveStarted = true;

                            while (true)
                            {
                                byte[] datagram = new byte[65536];
                                int length = _receiveSocket.ReceiveFrom(datagram, 0, datagram.Length, SocketFlags.None, ref remoteEndPoint);
                                Array.Resize(ref datagram, length);

                                string message = null;
                                try
                                {
                                    message = _messageTransform.Decode(datagram);
                                }
                                catch (Exception)
                                {
                                    // Exception trying to decode the message. Silently ignore it
                                }

                                if (message != null)
                                {
                                    MessageReceivedEventHandler?.Invoke(this, new MessageReceivedEventArgs(message, remoteIPEndPoint.Address));
                                }
                            }
                        }
                        catch (SocketException e)
                        {
                            if (e.SocketErrorCode != SocketError.Shutdown)
                            {
                                throw e;
                            }
                        }
                    });
                }
            }
            catch (SocketException e)
            {
                StopReceiving();
                Console.WriteLine(e);
            }
        }

        /// <summary>
        /// Stop receiving.
        /// </summary>
        public void StopReceiving()
        {
            try
            {
                _receiveSocket?.Shutdown(SocketShutdown.Receive);
                _receiveSocket?.Close();
                ReceiveStarted = false;
            }
            catch (SocketException e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion Public Methods
    }
}
