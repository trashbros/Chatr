/*
Primary udp messaging clien class
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

namespace Chatter
{
    public class MessageReceivedEventArgs
    {
        public string Message { get; }
        public IPAddress SenderIP { get; }
        public MessageReceivedEventArgs(string message, IPAddress senderIP)
        {
            Message = message;
            SenderIP = senderIP;
        }
    }
    public class Client
    {
        public IPAddress LocalIP { get; }
        public IPAddress MulticastIP { get; }
        public int Port { get; }

        public event EventHandler<MessageReceivedEventArgs> MessageReceivedEventHandler;

        private bool m_isReceiving = false;

        public Client(IPAddress localIP, IPAddress multicastIP, int port)
        {
            LocalIP = localIP;
            MulticastIP = multicastIP;
            Port = port;
        }

        public void StartReceiving()
        {
            m_isReceiving = true;
            using (var sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                sock.Bind(new IPEndPoint(IPAddress.Any, Port));
                sock.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(MulticastIP, LocalIP));

                IPEndPoint remoteIPEndpoint = new IPEndPoint(MulticastIP, 0);
                EndPoint remoteEndPoint = remoteIPEndpoint;

                try
                {
                    while (m_isReceiving)
                    {
                        byte[] datagram = new byte[65536];
                        int length = sock.ReceiveFrom(datagram, 0, datagram.Length, SocketFlags.None, ref remoteEndPoint);
                        Array.Resize(ref datagram, length);

                        string message = Encoding.UTF8.GetString(Convert.FromBase64String(Encoding.UTF8.GetString(datagram)));
                        MessageReceivedEventHandler?.Invoke(this, new MessageReceivedEventArgs(message, remoteIPEndpoint.Address));
                    }
                }
                catch (SocketException e)
                {
                    Console.WriteLine(e);
                }
            }
            m_isReceiving = false;
        }

        public void Dispose()
        {
            m_isReceiving = false;
        }

        public void Send(string message)
        {
            using (var udpClient = new UdpClient(new IPEndPoint(LocalIP, 0)))
            {
                byte[] datagram = Encoding.UTF8.GetBytes(Convert.ToBase64String(Encoding.UTF8.GetBytes(message)));
                udpClient.Send(datagram, datagram.Length, new IPEndPoint(MulticastIP, Port));
            }
        }
    }
}
