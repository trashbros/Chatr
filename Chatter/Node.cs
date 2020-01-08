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
        public string SenderName { get; }
        public MessageReceivedEventArgs(string message, IPAddress senderIP)
        {
            SenderName = message.Split('>')[0];
            Message = message.Substring(SenderName.Length + 1);
            SenderIP = senderIP;
        }
    }
    public class Node
    {
        public IPAddress LocalIP { get; }
        public IPAddress MulticastIP { get; }
        public int Port { get; }

        public event EventHandler<MessageReceivedEventArgs> MessageReceivedEventHandler;

        public Node(IPAddress localIP, IPAddress multicastIP, int port)
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

                        string message = Encoding.UTF8.GetString(datagram);
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
            using (var sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                byte[] datagram = Encoding.UTF8.GetBytes(message);
                sock.Bind(new IPEndPoint(LocalIP, 0));
                sock.SendTo(datagram, 0, datagram.Length, SocketFlags.None, new IPEndPoint(MulticastIP, Port));
            }
        }
    }
}
