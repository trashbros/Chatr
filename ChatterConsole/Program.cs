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

            // Create a new Chatter client
            var chatterClient = new Chatter.Client(IPAddress.Parse(ipAddress), IPAddress.Parse("239.255.10.11"), 1314);

            // Attach a message handler
            chatterClient.MessageReceivedEventHandler += (sender, m) =>
            {
                HandleMessagingCalls(m, displayName);
            };

            // Start task to receive messages
            Task.Run(() =>
            {
                chatterClient.StartReceiving();
            });

            // Get messages and send them out
            Console.Write("> ");
            string message = Console.ReadLine().Trim();
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.Write(new string(' ', Console.WindowWidth - 1));
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write("> ");

            while (!IsQuitMessage(message))
            {
                if (!string.IsNullOrEmpty(message))
                {
                    chatterClient.Send(displayName + ">" + message);
                }

                message = Console.ReadLine().Trim();
                Console.SetCursorPosition(0, Console.CursorTop - 1);
                Console.Write(new string(' ', Console.WindowWidth - 1));
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write("> ");
            };
        }

        static void HandleMessagingCalls(Chatter.MessageReceivedEventArgs m, string displayName)
        {
            // Parse out the sender name
            string senderName = m.Message.Split('>')[0];
            string text = m.Message.Substring(senderName.Length + 1);

            // Check to see if this is a private message
            if (text.StartsWith("/pm "))
            {
                // Check to see if this private message is to me
                text = text.Substring(4).Trim();
                if (text.StartsWith(displayName + " "))
                {
                    // Display the message
                    text = text.Substring(displayName.Length);
                    Console.Write($"\n< [{ senderName }: { text.Trim() }]\n> ");
                }
            }
            else
            {
                // Display the message
                Console.Write($"\n< { senderName }: { text }\n> ");
            }
        }

        static bool IsQuitMessage(string message)
        {
            string text = message.ToLower();
            return (text == "/quit" || text == "/q");
        }
    }
}
