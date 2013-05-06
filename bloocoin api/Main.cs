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
    class Main
    {
        #region private vars
        //Are we mining?
        private static bool mining = false;
        //How much hashes did we try
        private static long counter = 0L;
        //Our addres
        private static string addr = "";
        //our key to authenticate to the server
        private static string key = "";
        //the server url
        private const string url = "bloocoin.zapto.org";
        //the server port
        private const int port = 3122;
        //all mining threads
        private static Thread[] threads;
        //the difficulty
        private static int Difficulty;
        #endregion
        #region events
        ///<summary>
        ///Calls when the kh/s needs to be updated
        ///</summary>
        public static event KHSUpdatedEventHandler OnKHSUpdated;
        public delegate void KHSUpdatedEventHandler(object sender, KHSChangedEventArgs e);
        ///<summary>
        ///calls when the status needs to be updated
        ///</summary>
        public static event StatusUpdatedEventHandler OnStatusUpdated;
        public delegate void StatusUpdatedEventHandler(object sender, StatusChangedEventArgs e);
        ///<summary>
        ///Calls when the coins needs to be updated
        ///</summary>
        public static event CoinsUpdatedEventHandler OnCoinsUpdated;
        public delegate void CoinsUpdatedEventHandler(object sender, CoinsChangedEventArgs e);
        ///<summary>
        ///Calls when a new solution is added
        ///</summary>
        public static event SolutionAddedEventHandler OnSolutionAdded;
        public delegate void SolutionAddedEventHandler(object sender, SolutionAddedEventArgs e);
        ///<summary>
        ///Calls when the transactions need to be cleared
        ///</summary>
        public static event TransactionsClearedEventHandler OnTransactionsCleared;
        public delegate void TransactionsClearedEventHandler(object sender, EventArgs e);
        ///<summary>
        ///Calls when a transaction needs to be added
        ///</summary>
        public static event TransactionAddedEventHandler OnTransactionAdded;
        public delegate void TransactionAddedEventHandler(object sender, TransactionAddedEventArgs e);
        #endregion
        #region public vars
        /// <summary>
        /// Returns the count of tried hashes
        /// </summary>
        public static long getCounter()
        {
            return counter;
        }
        /// <summary>
        /// Returns wether it is mining or not
        /// </summary>
        public static bool getStatus()
        {
            return mining;
        }
        /// <summary>
        /// Returns the BLC address
        /// </summary>
        public static string getAddr()
        {
            return addr;
        }
        /// <summary>
        /// Returns the BLC key
        /// </summary>
        public static string getKey()
        {
            return key;
        }
        /// <summary>
        /// Returns the server's url
        /// </summary>
        public static string getURL()
        {
            return "bloocoin.zapto.org";
        }
        /// <summary>
        /// Returns the server's port
        /// </summary>
        /// <returns></returns>
        public static int getPort()
        {
            return 3122;
        }
        /// <summary>
        /// Returns the difficulty
        /// </summary>
        public static int GetDifficulty()
        {
            return Difficulty;
        }
        /// <summary>
        /// Starts a thread wich will update the BLC amount
        /// </summary>
        public static void getCoins()
        {
            Thread gc = new Thread(new Coins().run);
            gc.Start();
        }
        /// <summary>
        /// Returns how much threads are running
        /// </summary>
         public static int getThreads()
        {
            return threads.Length;
        }
        /// <summary>
        /// Starts a thread wich will update the transactions
        /// </summary>
        public static void getTransactions()
        {
            clearTransactions();
            Thread gt = new Thread(new Transactions().run);
            gt.Start();

        }
        /// <summary>
        /// Clears all transactions
        /// </summary>
        public static void clearTransactions()
        {
            OnTransactionsCleared(null, new EventArgs());
        }
        /// <summary>
        /// Adds an transaction
        /// </summary>
        public static void addTransaction(string trans)
        {
            string[] transactionData = trans.Split(',');
            transactionData[1] = transactionData[1].Replace(" amount: ", "");
            transactionData[2] = transactionData[2].Replace(" from: ", "");
            transactionData[2] = transactionData[2].Replace("}", "");
            transactionData[2] = transactionData[2].Replace("]", "");
            Transaction transaction = new Transaction();
            transaction.amount = double.Parse(transactionData[1]);
            transaction.from = transactionData[2];
            transaction.to = transactionData[0];
            OnTransactionAdded(null, new TransactionAddedEventArgs(transaction));
        }
        #endregion
        #region Update variables
        /// <summary>
        /// Updates the count of tried hashes
        /// </summary>
        public static void updateCounter()
        {
            counter += 1L;
        }
        /// <summary>
        /// Updates the KHS
        /// </summary>
        public static void updateKHS(double KHS)
        {
            OnKHSUpdated(null, new KHSChangedEventArgs(KHS, counter));
        }
        /// <summary>
        /// Updates the solution
        /// </summary>
        public static void updateSolved(string solution)
        {
            OnSolutionAdded(null, new SolutionAddedEventArgs(solution));
        }
        /// <summary>
        /// Updates the status
        /// </summary>
        public static void updateStatusText(string status, Color color)
        {
            OnStatusUpdated(null, new StatusChangedEventArgs(status, color));
        }
        /// <summary>
        /// Updates the bloocoins amount
        /// </summary>
        public static void updateBLC(int blc)
        {
            OnCoinsUpdated(null, new CoinsChangedEventArgs(blc));
        }
        #endregion
        #region mining
        ///<summary>
        ///starts mining with a specified number of threads 
        ///</summary>
        public static void StartMining(int Threads)
        {
            if (!mining)
            {
                mining = true;
                threads = new Thread[Threads];
                new Thread(new Khs().run);
                try
                {
                    Difficulty = UpdateDifficulty();
                }
                catch
                {
                    Difficulty = 7;
                }
                for (int i = 0; i < Threads; i++)
                {
                    new Thread(new Khs().run).Start();
                    threads[i] = new Thread(new Miner(Difficulty).mine);
                    threads[i].Start();
                }
            }
            else
            {
                throw new Exception("Mining already started");
            }
        }
        ///<summary>
        ///stops the mining
        ///</summary>
        public static void StopMining()
        {
            mining = false;
            foreach (Thread t in threads)
            {
                t.Abort();
            }
        }
        /// <summary>
        /// Checks with the server for the difficulty
        /// </summary>
        private static int UpdateDifficulty()
        {
            string result = null;
            TcpClient sock = new TcpClient();
            sock.Connect(url, port);
            string command = "{\"cmd\":\"get_coin\"}";
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
            string diff = new System.Text.RegularExpressions.Regex("(?<=difficulty\\\": )(\\d*?)(?=})").Match(result).ToString();
            return int.Parse(diff);
        }
        #endregion
        #region initialization
        /// <summary>
        /// initializes the Main class (addr and key)
        /// </summary>
        public static void loadData()
        {
            try
            {
                string filename = "blootstamp";
                if (!File.Exists("blootstamp"))
                {
                    throw new Exception("blootstamp not found");
                }
                string data = File.ReadAllText(filename);
                addr = data.Split(':')[0];
                key = data.Split(':')[1];
            }
            catch
            {

            }

            finally
            {
                updateStatusText("Bloostamp data loaded successfully", Color.Black);
                Console.WriteLine("Bloostamp data loaded.");
            }
        }
        #endregion
    }
    class KHSChangedEventArgs : EventArgs
    {
        private double khs;
        public double Khs
        {
            get
            {
                return khs;
            }
        }
        private long count;
        public long Count
        {
            get
            {
                return count;
            }
        }
        public KHSChangedEventArgs(double Khs, long count)
        {
            this.khs = Khs;
            this.count = count;
        }
    }
    class TransactionAddedEventArgs : EventArgs
    {
        private Transaction transaction;
        public Transaction Transaction
        {
            get
            {
                return transaction;
            }
        }
        public TransactionAddedEventArgs(Transaction transaction)
        {
            this.transaction = transaction;
        }
    }
    class StatusChangedEventArgs : EventArgs
    {
        private Color color;
        public Color ForeColor
        {
            get
            {
                return color;
            }
        }
        private string message;
        public string Message
        {
            get
            {
                return message;
            }
        }
        public StatusChangedEventArgs(string message, Color color)
        {
            this.message = message;
            this.color = color;
        }
    }
    class CoinsChangedEventArgs : EventArgs
    {
        private int coins;
        public int Coins
        {
            get
            {
                return Coins;
            }
        }

        public CoinsChangedEventArgs(int coins)
        {
            this.coins = coins;
        }

    }
    class SolutionAddedEventArgs : EventArgs
    {
        private string solution;
        public string Solution
        {
            get
            {
                return solution;
            }
        }
        public SolutionAddedEventArgs(string solution)
        {
            this.solution = solution;
        }
    }
    public struct Transaction
    {
        public double amount;
        public string from;
        public string to;
    }
}
