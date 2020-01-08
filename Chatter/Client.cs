﻿using System;
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

        public Client(IPAddress localIP, IPAddress multicastIP, int port)
        {
            LocalIP = localIP;
            MulticastIP = multicastIP;
            Port = port;
        }

        public void StartReceiving()
        {
            using (var sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                sock.Bind(new IPEndPoint(IPAddress.Any, Port));
                sock.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(MulticastIP, LocalIP));

                IPEndPoint remoteIPEndpoint = new IPEndPoint(MulticastIP, 0);
                EndPoint remoteEndPoint = remoteIPEndpoint;

                try
                {
                    while (true)
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