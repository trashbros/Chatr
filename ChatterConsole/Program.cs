using System;
using System.Net;
using System.Threading.Tasks;

namespace ChatterConsole
{
    class Program
    {
        static void Main(string[] args)
        {
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

            var ChatterNode = new Chatter.Node(IPAddress.Parse(args[0]), IPAddress.Parse("239.255.10.11"), 1314);

            ChatterNode.MessageReceivedEventHandler += (sender, m) =>
            {
                HandleMessagingCalls(m, displayName);
            };

            Task.Run(() =>
            {
                ChatterNode.StartReceiving();
            });

            Console.Write("> ");
            string message = Console.ReadLine();

            while (message.ToLower() != "quit")
            {
                if (!string.IsNullOrEmpty(message))
                {
                    ChatterNode.Send(displayName + ">" + message);
                }

                Console.Write("\n> ");
                message = Console.ReadLine();
            };
        }

        static void HandleMessagingCalls(Chatter.MessageReceivedEventArgs m, string displayName)
        {
            string outputMessage = m.Message;
            if (outputMessage.StartsWith("/pm "))
            {
                outputMessage = outputMessage.Substring(4).Trim();
                if (outputMessage.StartsWith(displayName + " "))
                {
                    outputMessage = outputMessage.Substring(displayName.Length);
                    Console.WriteLine($"\n< { m.SenderName }: {outputMessage} \n> ");
                }
            }
            else
            {
                Console.WriteLine($"\n< { m.SenderName }: {m.Message} \n> ");
            }
        }
    }
}
