// BLC miner api
// Copyright (C) 2013 Prestige - http://prestige-coding.net

// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Drawing;

namespace Bloocoin_API
{
    class SendCoins
    {
        /// <summary>
        /// the server's url
        /// </summary>
        string url = Main.getURL();
        /// <summary>
        /// the server's port
        /// </summary>
        int port = Main.getPort();

        /// <summary>
        /// the address we want to 
        /// send coins to
        /// </summary>
        string destAddr = "";
        /// <summary>
        /// the amount of coins we want to send
        /// </summary>
        int amount = 0;

        /// <summary>
        /// initializes the class and sets the 
        /// destination address and amount of 
        /// coins
        /// </summary>
        public SendCoins(string destAddr, int amount)
        {
            this.destAddr = destAddr;
            this.amount = amount;
        }

        /// <summary>
        /// Sends the coins
        /// </summary>
        public void Send()
        {
            try
            {
                string result = null;
                TcpClient sock = new TcpClient();
                sock.Connect(this.url, this.port);
                string command = "{\"cmd\":\"send_coin\",\"to\":\"" +
                                    this.destAddr + "\",\"addr\":\"" +
                                    Main.getAddr() + "\",\"pwd\":\"" + 
                                    Main.getKey() + "\",\"amount\":" + 
                                    this.amount + "}";
                StreamReader sr = new StreamReader(sock.GetStream());
                StreamWriter sw = new StreamWriter(sock.GetStream());
                sw.Write(command);
                sw.Flush();

                string inputLine;
                while ((inputLine = sr.ReadLine()) != null)
                {
                    result = result + inputLine;
                }

                sr.Close();
                sw.Close();
                sock.Close();
                if (result.Contains("true"))
                    Main.updateStatusText(this.amount + " BLC sent to " + this.destAddr, Color.Blue);
                else if (result.Contains("false"))
                    Main.updateStatusText("Transaction failed!", Color.Red);
            }
            catch (Exception ex)
            {
                Main.updateStatusText("An error occured in sending coins: " + ex.Message, Color.Red);
            }
        }
    }
}
