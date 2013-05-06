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
    class Submit
    {
        /// <summary>
        /// the hash
        /// </summary>
        string hash = "";
        /// <summary>
        /// the solution of the hash
        /// </summary>
        string solution = "";

        /// <summary>
        /// the server's url
        /// </summary>
        string url = Main.getURL();
        /// <summary>
        /// the server's port
        /// </summary>
        int port = Main.getPort();

        /// <summary>
        /// is our solution submitted?
        /// </summary>
        bool submitted = false;

        /// <summary>
        /// initializes the submit class
        /// </summary>
        public Submit(string hash, string solution)
        {
            this.hash = hash;
            this.solution = solution;
        }

        /// <summary>
        /// Runs the submit thread
        /// </summary>
        public void run()
        {
            while (!this.submitted)
            {
                bool sawException = false;
                try
                {
                    Main.updateStatusText("Submitting " + this.solution, Color.Blue);
                    submit();
                    Thread.Sleep(5000);
                }
                catch
                {
                    sawException = true;
                }
                if (sawException)
                    Thread.CurrentThread.Interrupt();
            }
        }

        /// <summary>
        /// Submits our solution to
        /// the server
        /// </summary>
        private void submit()
        {
            try
            {
                string result = null;
                TcpClient sock = new TcpClient();
                string command = "{\"cmd\":\"check\",\"winning_string\":\"" +
                this.solution + "\",\"winning_hash\":\"" +
                this.hash + "\",\"addr\":\"" + Main.getAddr() + "\"}";
                sock.Connect(this.url, this.port);
                StreamReader sr = new StreamReader(sock.GetStream());
                StreamWriter sw = new StreamWriter(sock.GetStream());
                sw.Write(command);

                string inputLine;
                while ((inputLine = sr.ReadLine()) != null)
                {
                    result = result + inputLine;
                }

                sr.Close();
                sw.Close();
                sock.Close();
                if (result.Contains("\"success\": true"))
                {
                    Console.WriteLine("Result: Submitted");
                    this.submitted = true;
                    Main.updateStatusText(this.solution + " submitted", Color.Blue);
                }
                else if (result.Contains("\"success\": false"))
                {
                    Console.WriteLine("Result: Failed");
                    this.submitted = true;
                    Main.updateStatusText("Submission of " + this.solution + " failed, already exists!", Color.Red);
                }
            }
            catch (Exception ex)
            {
                Main.updateStatusText("Submission of " + this.solution + " failed " + ex.ToString(), Color.Red);
            }
            Thread gc = new Thread(new Coins().run);
            gc.Start();
        }
    }
}
