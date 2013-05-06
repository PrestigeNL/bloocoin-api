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
using System.Threading;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace Bloocoin_API
{
    class Coins
    {
        /// <summary>
        /// server url
        /// </summary>
        string url = Main.getURL();
        /// <summary>
        /// server port
        /// </summary>
        int port = Main.getPort();

        /// <summary>
        /// Runs the Coin updater
        /// </summary>
        public void run()
        {
            getCoins();
        }

        /// <summary>
        /// Receives the amount of coins from the server
        /// and sends it to the main thread
        /// </summary>
        public void getCoins()
        {
            try
            {
                string result = null;
                TcpClient sock = new TcpClient();
                sock.Connect(this.url, this.port);
                string command = "{\"cmd\":\"my_coins\",\"addr\":\"" + Main.getAddr() + "\",\"pwd\":\"" +
                    Main.getKey() + "\"}";
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
                string coins = System.Text.RegularExpressions.Regex.Split(result, "t\": ")[1];
                coins = coins.Split('}')[0];
                Main.updateBLC(int.Parse(coins));
                Console.WriteLine(int.Parse(coins));
            }
            catch (Exception ex)
            {
                Main.updateStatusText("An error occured in getting the amount of coins you have: " + ex.Message, Color.Red);
            }
        }
    }
}
