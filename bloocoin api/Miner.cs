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
using System.Security.Cryptography;
using System.Threading;

namespace Bloocoin_API
{
    class Miner
    {
        /// <summary>
        /// The difficulty it needs to mine with
        /// </summary>
        private string difficulty = null;

        /// <summary>
        /// Initializes the miner with the 
        /// given difficulty
        /// </summary>
        /// <param name="difficulty"></param>
        public Miner(int difficulty)
        {
            this.difficulty = null;
            for (int i = 0; i < difficulty; i++)
            {
                this.difficulty += "0";
            }
        }

        /// <summary>
        /// Checks if random strings
        /// start with the difficulty
        /// when they are hashed with sha512
        /// </summary>
        public void mine()
        {
            try
            {
                int i = 0;
                string hash;
                string currentstring;
                string startstring = randomstring();
                while (true)
                {
                    currentstring = startstring + i;
                    hash = sha512Hex(currentstring);
                    if (hash.StartsWith(this.difficulty))
                    {
                        new Thread(Complete).Start(new KeyValuePair<string, string>(currentstring, hash));
                    }
                    if (i == 10000000)
                    {
                        i = 0;
                        startstring = randomstring();
                    }
                    i++;
                    Main.updateCounter();
                    
                }
            }
            catch (ThreadAbortException) { return;  }
        }

        /// <summary>
        /// Starts the thread that submits the hashes
        /// and writes the solution to a file 
        /// if it found a valid hash
        /// </summary>
        private void Complete(object mhash)
        {
            KeyValuePair<string, string> _hash = (KeyValuePair<string, string>) mhash;
            string currentstring = _hash.Key;
            string hash = _hash.Value;
            Thread sub = new Thread(new Submit(hash,
                              currentstring).run);
            sub.Start();
            Main.updateSolved(currentstring);
            Console.WriteLine("Success: " + currentstring);
            try
            {
                if (!System.IO.File.Exists("BLC_Solved.txt"))
                {
                    System.IO.FileStream fs = System.IO.File.Create("BLC_Solved.txt");
                    fs.Dispose();
                    fs.Close();
                }
                string s = System.IO.File.ReadAllText("BLC_Solved.txt");
                s += currentstring + Environment.NewLine;
                System.IO.File.WriteAllText("BLC_Solved.txt", s);
            }
            catch
            {

            }
        }

        /// <summary>
        /// Generates an SHA512 hash out of the given string
        /// </summary>
        private static string sha512Hex(string data)
        {
            Org.BouncyCastle.Crypto.Digests.Sha512Digest shaB = new Org.BouncyCastle.Crypto.Digests.Sha512Digest();
            byte[] input = Encoding.UTF8.GetBytes(data);
            byte[] output = new byte[64];
            shaB.BlockUpdate(input, 0, input.Length);
            shaB.DoFinal(output, 0);
            return encodeHex(output, "abcdefghijklmnopqrstuvwxyz0123456789".ToCharArray()).ToString();
        }

        /// <summary>
        /// Generates a random 5 char string
        /// </summary>
        /// <returns></returns>
        public string randomstring()
        {
            string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random r = new Random();
            int limit = 5;
            StringBuilder buf = new StringBuilder();

            buf.Append(chars[r.Next(26)]);
            for (int i = 0; i < limit; i++)
            {
                buf.Append(chars[r.Next(chars.Length)]);
            }
            return buf.ToString();
        }

        /// <summary>
        /// Converts a byte array to a hex string
        /// </summary>
        protected static char[] encodeHex(byte[] data, char[] toDigits)
        {
            int l = data.Length;
            char[] _out = new char[l << 1];

            int i = 0; for (int j = 0; i < l; i++)
            {
                _out[(j++)] = toDigits[((0xF0 & data[i]) >> 4)];
                _out[(j++)] = toDigits[(0xF & data[i])];
            }
            return _out;
        }

    }
}
