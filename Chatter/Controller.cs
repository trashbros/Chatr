using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Chatter
{
    public class Controller
    {
        Client chatterClient;

        string LocalIP;

        string DisplayName;

        public event EventHandler<string> MessageDisplayEventHandler;

        public Controller(string localIP, string displayName)
        {
            LocalIP = localIP;
            DisplayName = displayName;
        }

        public void Init()
        {
            // Create a new Chatter client
            chatterClient = new Chatter.Client(IPAddress.Parse(LocalIP), IPAddress.Parse("239.255.10.11"), 1314);

            // Attach a message handler
            chatterClient.MessageReceivedEventHandler += (sender, m) =>
            {
                HandleMessagingCalls(m);
            };

            // Start task to receive messages
            System.Threading.Tasks.Task.Run(() =>
            {
                chatterClient.StartReceiving();
            });
        }

        public void SendMessage(string message)
        {
            chatterClient.Send(DisplayName + ">" + message);
        }

        void HandleMessagingCalls(Chatter.MessageReceivedEventArgs m)
        {
            // Parse out the sender name
            string senderName = m.Message.Split('>')[0];
            string text = m.Message.Substring(senderName.Length + 1);

            // Check to see if this is a command message
            if (text.StartsWith("/"))
            {
                text = text.TrimStart('/');
                HandleCommandText(text, senderName);
            }
            else
            {
                // Display the message
                MessageDisplayEventHandler?.Invoke(this, $"\n< { senderName }: { text }\n> ");
                //Console.Write($"\n< { senderName }: { text }\n> ");
            }
        }

        void HandleCommandText(string message, string senderName)
        {
            string command = message.Split(' ')[0];
            // Private message
            if (command == "pm")
            {
                string text = message.Substring(3).Trim();
                if (text.StartsWith(DisplayName + " "))
                {
                    // Display the message
                    text = text.Substring(DisplayName.Length);
                    MessageDisplayEventHandler?.Invoke(this, $"\n< [[PM]{ senderName }: { text.Trim() }]\n> ");
                    //Console.Write($"\n< [[PM]{ senderName }: { text.Trim() }]\n> ");
                }
            }
            // Active user list request
            else if (command == "users")
            {
                chatterClient.Send(DisplayName + ">" + "/userinfo " + senderName);
            }
            // Active user return message
            else if (command == "userinfo")
            {
                string text = message.Substring(8).Trim();
                if (text.StartsWith(DisplayName))
                {
                    MessageDisplayEventHandler?.Invoke(this, $"\n[{ senderName } Logged In]\n> ");
                    //Console.Write($"\n[{ senderName } Logged In]\n> ");
                }
            }
        }
    }
}
