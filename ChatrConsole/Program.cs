/*
Startup console application. Handle format and console read/write.

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
using System.Collections.Generic;

namespace ChatrConsole
{
    class Program
    {
        #region Fields
        static string s_currentInput = string.Empty;
        static string s_nextInput = string.Empty;
        static readonly List<string> s_messageHistory = new List<string>();
        static int s_historyIndex = -1;
        static Chatr.MultiChannel s_chatrClient;
        #endregion Fields

        /// <summary>
        /// Entry point of the program.
        /// </summary>
        /// <param name="args">Command line arguments</param>
        static void Main(string[] args)
        {
            // Display the assembly version information
            DisplayVersion();

            // Try and read the settings path from the command line arguments
            string settingsPath = (args == null || args.Length == 0) ? null : args[0];

            // Startup the Chatr client
            StartupChatr(GetSettingsPath(settingsPath));

            // Read and process commands entered by the user until the quit message is entered
            ReadAndProcessMessagesUntilQuit();

            // Shutdown the Chatr client
            ShutdownChatr();
        }

        /// <summary>
        /// Display the assembly version information
        /// </summary>
        static void DisplayVersion()
        {
            // Get the version from currently executing assembly
            string assVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            // Display the version to the console
            Console.WriteLine($"You are running version {assVersion} of Chatr!");
        }

        /// <summary>
        ///  Start a new Chatr client using the settings from <paramref name="settingsPath"/>
        /// </summary>
        /// <param name="settingsPath">File path to chatr settings file</param>
        static void StartupChatr(string settingsPath)
        {
            // Create a new Chatr client
            s_chatrClient = new Chatr.MultiChannel(settingsPath);

            // Attach a message display handler
            s_chatrClient.MessageDisplayEventHandler += (sender, m) =>
            {
                DisplayMessage(m[0],m[1]);
            };
        }

        /// <summary>
        /// Read messages entered by the user and send them to the client util the user enters the quit message
        /// </summary>
        static void ReadAndProcessMessagesUntilQuit()
        {
            // Display intial prompt
            Console.Write("\n> ");

            // Read the first message
            string message = ReadMessage();

            // Loop while it isn't the quit message
            while (!IsQuitMessage(message))
            {
                // Check to see if user actually entered something
                if (!string.IsNullOrEmpty(message))
                {
                    // Send the message to the client
                    s_chatrClient.SendMessage(message);
                }

                // Read the next message
                message = ReadMessage();
            };
        }

        /// <summary>
        /// Shutdown the Chatr client
        /// </summary>
        static void ShutdownChatr()
        {
            s_chatrClient?.ShutDown();
            s_chatrClient = null;
        }

        /// <summary>
        /// Get the settings path based on a suggested settings path
        /// Use the default path if invalid
        /// </summary>
        /// <param name="settingsPath">Settings path to use</param>
        /// <returns></returns>
        static string GetSettingsPath(string settingsPath)
        {
            // Default path in case supplied path is invalid
            string path = System.IO.Path.GetFullPath(".chatrconfig");

            // Check to see if the path is valid
            if (IsPathValidRootedLocal(settingsPath))
            {
                // Use the supplied settings path
                path = System.IO.Path.GetFullPath(settingsPath);
            }

            return path;
        }

        /// <summary>
        /// Validate that path is a rooted, local path
        /// </summary>
        /// <param name="pathString">Path to validate</param>
        /// <returns>true if path is rooted, local path, false otherwise</returns>
        static bool IsPathValidRootedLocal(String pathString)
        {
            bool isValidUri = Uri.TryCreate(pathString, UriKind.Absolute, out Uri pathUri);
            return isValidUri && pathUri != null && pathUri.IsLoopback;
        }

        /// <summary>
        /// Read a message from the console
        /// </summary>
        /// <returns>Message that was read</returns>
        static string ReadMessage()
        {
            // Clear input strings
            s_currentInput = string.Empty;
            s_nextInput = string.Empty;

            // Read the first key from the console
            var key = Console.ReadKey(true);

            // Loop until the ENTER key is pressed
            while( key.Key != ConsoleKey.Enter)
            {
                // Intialize the next input to the current input
                s_nextInput = s_currentInput;

                // Reset the history index
                s_historyIndex = -1;

                // Check for a backspace
                if (key.Key == ConsoleKey.Backspace && s_currentInput.Length > 0)
                {
                    // Remote the last character from the input
                    s_nextInput = s_currentInput[0..^1];
                }
                // Check for the ESC key
                else if (key.Key == ConsoleKey.Escape)
                {
                    // Clear the input
                    s_nextInput = string.Empty;

                    // Reset the history index
                    s_historyIndex = -1;
                }
                // Check for UP arrow key
                else if (key.Key == ConsoleKey.UpArrow)
                {
                    // If we can go forward in history
                    if (s_messageHistory.Count > 0 && s_historyIndex < s_messageHistory.Count - 1)
                    {
                        // Move forward in the history
                        s_historyIndex++;
                        s_nextInput = s_messageHistory[s_historyIndex];
                    }
                }
                // Check for DOWN arrow key
                else if (key.Key == ConsoleKey.DownArrow)
                {
                    // If we can go back in history
                    if (s_messageHistory.Count > 0 && s_historyIndex > 0)
                    {
                        // Move back in history
                        s_historyIndex--;
                        s_nextInput = s_messageHistory[s_historyIndex];
                    }
                }
                // Check to see if this is not a control key
                else if (!char.IsControl(key.KeyChar))
                {
                    // Add the key character to the input
                    s_nextInput = s_currentInput + key.KeyChar;
                }

                // Display the input
                DisplayInput();

                // Current input now becomes the next input value
                s_currentInput = s_nextInput;

                // Read the next key
                key = Console.ReadKey(true);
            }

            // Check to see if we should add this input to the history
            if (!string.IsNullOrEmpty(s_currentInput) && !s_messageHistory.Contains(s_currentInput))
            {
                // Insert this input to the top of the history
                s_messageHistory.Insert(0, s_currentInput);
            }

            // Get the message to return
            string message = s_currentInput;

            // Clear the input strings
            s_currentInput = string.Empty;
            s_nextInput = string.Empty;

            // Return the message entered in the console
            return message;
        }

        /// <summary>
        /// Check to see if a message is the quit message
        /// </summary>
        /// <param name="message">Message to check</param>
        /// <returns></returns>
        static bool IsQuitMessage(string message)
        {
            string text = message.ToLower(System.Globalization.CultureInfo.CurrentCulture).TrimStart('/');
            return (text == Chatr.CommandList.QUIT || text == Chatr.CommandList.QUIT_S);
        }

        /// <summary>
        /// Display a recevied message
        /// </summary>
        /// <param name="message">Message to display</param>
        static void DisplayMessage(string message, string textColor)
        {
            ClearInputLines();
            ConsoleColor consoleColor;
            if (Enum.TryParse<ConsoleColor>(textColor, true, out consoleColor))
            {
                Console.ForegroundColor = consoleColor;
                //Console.Write($"Color is {textColor}\n>");
            }
            Console.Write(message); 
            Console.ResetColor();
            Console.Write("\n> " + s_nextInput);
        }

        /// <summary>
        /// Display the message input from the user
        /// </summary>
        static void DisplayInput()
        {
            // Clear the previous message input
            ClearInputLines();

            // Display the new message input
            Console.Write("> " + s_nextInput);
        }

        /// <summary>
        /// Clear any user message input from the display
        /// </summary>
        static void ClearInputLines()
        {
            Console.SetCursorPosition(0, Console.CursorTop - 1 * ((s_currentInput.Length + 1) / Console.WindowWidth));
            Console.Write(new string(' ', s_currentInput.Length + 2));
            Console.SetCursorPosition(0, Console.CursorTop - 1 * ((s_currentInput.Length + 1) / Console.WindowWidth));
        }
    }
}
