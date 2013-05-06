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

namespace Bloocoin_API
{
    class Khs
    {
        /// <summary>
        /// Runs the Khs class
        /// 
        /// Measures the Khs and 
        /// sends it to main
        /// </summary>
        public void run()
        {
            bool running = true;
            while (running)
            {
                bool sawException = false;
                try
                {
                    long oldAmount = Main.getCounter();
                    Thread.Sleep(1000);
                    long newAmount = Main.getCounter();
                    Main.updateKHS((newAmount - oldAmount) / 1000.0D);
                    if (!Main.getStatus())
                        running = false;
                }
                catch
                {
                    sawException = true;
                }
                if (sawException)
                    Thread.CurrentThread.Interrupt();
            }
            Main.updateKHS(0.0D);
        }
    }
}
