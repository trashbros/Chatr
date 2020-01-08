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
            try
            {
                using (var udpClient = new UdpClient())
                {
                    udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    udpClient.Client.Bind(new IPEndPoint(LocalIP, Port));

                    udpClient.JoinMulticastGroup(MulticastIP);

                    IPEndPoint remoteIPEndPoint = new IPEndPoint(MulticastIP, 0);

                    while (true)
                    {
                        byte[] datagram = udpClient.Receive(ref remoteIPEndPoint);

                        string message = Encoding.UTF8.GetString(datagram);

                        MessageReceivedEventHandler?.Invoke(this, new MessageReceivedEventArgs(message, remoteIPEndPoint.Address));
                    }
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(e);
            }
        }

        public void Send(string message)
        {
            using (var udpClient = new UdpClient(new IPEndPoint(LocalIP, 0)))
            {
                byte[] datagram = Encoding.UTF8.GetBytes(message);
                udpClient.Send(datagram, datagram.Length, new IPEndPoint(MulticastIP, Port));
            }
        }
    }
}
