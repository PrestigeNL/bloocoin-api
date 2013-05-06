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
    class Transactions
    {
        /// <summary>
        /// runs the Transactions class
        /// </summary>
        public void run()
        {
            loadTransactions();
        }

        /// <summary>
        /// Gets all transactions from the server
        /// and sends it to the main class
        /// </summary>
        public void loadTransactions()
        {
            try
            {
                string result = null;
                TcpClient sock = new TcpClient();
                sock.Connect(Main.getURL(), Main.getPort());
                string command = "{\"cmd\":\"transactions\",\"addr\":\"" +
                  Main.getAddr() + "\",\"pwd\":\"" +
                  Main.getKey() + "\"}";
                StreamReader sr = new StreamReader(sock.GetStream());
                StreamWriter sw = new StreamWriter(sock.GetStream());
                sw.Write(command);
                sw.Flush();

                string inputLine;
                while ((inputLine = sr.ReadLine()) != null)
                {
                    result = result + "\n" + inputLine;
                }

                string[] transactions = System.Text.RegularExpressions.Regex.Split(result, "\\{\\\"to\\\"\\: ");
                Main.clearTransactions();
                for (int i = 1; i < transactions.Length; i++)
                {
                    transactions[i] = transactions[i].Replace("\"", "");
                    Console.WriteLine(transactions[i]);
                    Main.addTransaction(transactions[i]);
                }

                sr.Close();
                sw.Close();
                sock.Close();
                Main.updateStatusText("Transactions updated", Color.Blue);
            }
            catch (Exception ex)
            {
                Main.updateStatusText("An error occured in getting the transactions: " + ex.Message, Color.Red);
            }
        }
    }
}
