/*
Static class of functions used in multiple classes
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

namespace Chatr
{
    public static class Helpers
    {
        /// <summary>
        /// Helper function to check if the port is a real port
        /// </summary>
        /// <param name="portString"></param>
        /// <param name="portNum"></param>
        /// <returns></returns>
        public static bool IsValidPort(string portString, out int portNum)
        {
            if (!Int32.TryParse(portString, out portNum))
            {
                return false;
            }

            return IsValidPort(portNum);
        }

        /// <summary>
        /// Helper function to check if the port is a real port
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public static bool IsValidPort(int port)
        {
            if (port < 0 || port > 65535)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Helper function to check if the IP can be parsed
        /// </summary>
        /// <param name="ipAdress"></param>
        /// <returns></returns>
        public static bool IsValidIP(string ipAdress)
        {
            if (!IPAddress.TryParse(ipAdress, out _))
            { return false; }
            return true;
        }
    }
}