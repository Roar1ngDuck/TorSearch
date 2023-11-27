using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorSearch.Scraper
{
    internal class Threading
    {
        private static int runningThreads = 0;

        private static object threadsLock = new object();

        /// <summary>
        /// Add the specified amount to the number of running threads. Negative numbers are used to decrease the number of running threads.
        /// </summary>
        /// <param name="amount"></param>
        public static void IncrementThreads(int amount)
        {
            lock (threadsLock)
            {
                runningThreads += amount;
            }
        }

        /// <summary>
        /// Get the number of threads currently running
        /// </summary>
        /// <returns></returns>
        public static int GetRunningThreads() 
        { 
            lock (threadsLock)
            {
                return runningThreads;
            }
        }
    }
}
