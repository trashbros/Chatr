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
using System.Text;
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
        /// Has Disposed already been called?
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// The socket used for receiving data.
        /// </summary>
        private Socket _recvSocket;

        /// <summary>
        /// The task the receive loop is running on.
        /// </summary>
        private Task _recvTask;

        /// <summary>
        /// UdpClient for sending data.
        /// </summary>
        private UdpClient _sendClient;

        /// <summary>
        /// Flag to tell the recevie task that it needs to shutdown.
        /// </summary>
        private bool _shutdown;

        #endregion Private Fields

        #region Private Methods

        /// <summary>
        /// Sends the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="checkActive">
        /// if set to <c>true</c> check to see if connection is active before sending.
        /// </param>
        private void Send(byte[] message, bool checkActive)
        {
            if (!checkActive || Active)
            {
                byte[] datagram = _messageTransform.Encode(message);

                try
                {
                    if (_sendClient == null)
                    {
                        _sendClient = new UdpClient(new IPEndPoint(_localIP, 0));
                    }

                    _sendClient.Send(datagram, datagram.Length, _multicastEndPoint);
                }
                catch (SocketException)
                {
                    _sendClient?.Dispose();
                    _sendClient = null;
                }
            }
        }

        /// <summary>
        /// Start receiving messages in a separate task.
        /// </summary>
        private async Task StartReceiving()
        {
            try
            {
                using (_recvSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
                {
                    _recvSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    _recvSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 250);
                    _recvSocket.Bind(new IPEndPoint(IPAddress.Any, _multicastEndPoint.Port));
                    _recvSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(_multicastEndPoint.Address, _localIP));

                    IPEndPoint remoteIPEndPoint = new IPEndPoint(_multicastEndPoint.Address, 0);
                    EndPoint remoteEndPoint = remoteIPEndPoint;

                    await Task.Run(() =>
                    {
                        while (!_shutdown)
                        {
                            byte[] datagram = new byte[65536];
                            int length = 0;

                            Active = true;

                            try
                            {
                                length = _recvSocket.ReceiveFrom(datagram, 0, datagram.Length, SocketFlags.None, ref remoteEndPoint);
                            }
                            catch (SocketException e)
                            {
                                if (e.SocketErrorCode == SocketError.TimedOut)
                                {
                                    continue;
                                }
                                else
                                {
                                    throw e;
                                }
                            }

                            Array.Resize(ref datagram, length);

                            byte[] message = null;
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

                        _shutdown = false;
                    });
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                Active = false;
            }
        }

        #endregion Private Methods

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
                    Disconnect();

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
        /// Gets a value indicating whether this <see cref="MulticastConnection"/> is active.
        /// </summary>
        /// <value><c>true</c> if active; otherwise, <c>false</c>.</value>
        public bool Active { get; private set; }

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

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Connect to the multicast end point.
        /// </summary>
        public void Connect()
        {
            Disconnect();

            _recvTask = StartReceiving();
        }

        /// <summary>
        /// Disconnect from the multicast end point.
        /// </summary>
        public void Disconnect()
        {
            if (_recvTask != null)
            {
                _shutdown = true;
                _recvTask.Wait();
                _recvTask = null;
            }
        }

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
        public void Send(byte[] message)
        {
            Send(message, false);
        }

        #endregion Public Methods
    }
}
