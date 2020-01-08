using System;
using System.Net;
using System.Threading.Tasks;

namespace ChatterConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            // Get the local IP Address to use
            string ipAddress = "";
            if (args.Length > 0)
            {
                ipAddress = args[0];
            }
            if (string.IsNullOrEmpty(ipAddress))
            {
                Console.Write("Enter the local IP address to use: ");
                ipAddress = Console.ReadLine();
                while(string.IsNullOrEmpty(ipAddress))
                {
                    ipAddress = Console.ReadLine();
                }
            }

            // Get the display name to use
            string displayName = "";
            if (args.Length > 1)
            {
                displayName = args[1];
            }
            if(string.IsNullOrEmpty(displayName))
            {
                Console.Write("Enter your display name: ");
                displayName = Console.ReadLine();
                while (string.IsNullOrEmpty(displayName))
                {
                    displayName = Console.ReadLine();
                }
            }

            // Create a new Chatter node
            var chatterNode = new Chatter.Node(IPAddress.Parse(ipAddress), IPAddress.Parse("239.255.10.11"), 1314);

            // Attach a message handler
            chatterNode.MessageReceivedEventHandler += (sender, m) =>
            {
                HandleMessagingCalls(m, displayName);
            };

            // Start task to receive messages
            Task.Run(() =>
            {
                chatterNode.StartReceiving();
            });

            // Get messages and send them out
            Console.Write("> ");
            string message = Console.ReadLine();

            while (message.ToLower() != "/quit")
            {
                if (!string.IsNullOrEmpty(message))
                {
                    chatterNode.Send(displayName + ">" + message);
                }

                Console.Write("\n> ");
                message = Console.ReadLine();
            };
        }

        static void HandleMessagingCalls(Chatter.MessageReceivedEventArgs m, string displayName)
        {
            // Check to see if this is a private message
            string outputMessage = m.Message;
            if (outputMessage.StartsWith("/pm "))
            {
                // Check to see if this private message is to me
                outputMessage = outputMessage.Substring(4).Trim();
                if (outputMessage.StartsWith(displayName + " "))
                {
                    // Display the message
                    outputMessage = outputMessage.Substring(displayName.Length);
                    Console.WriteLine($"\n< { m.SenderName }: {outputMessage} \n> ");
                }
            }
            else
            {
                // Display the message
                Console.WriteLine($"\n< { m.SenderName }: {m.Message} \n> ");
            }
        }
    }
}
