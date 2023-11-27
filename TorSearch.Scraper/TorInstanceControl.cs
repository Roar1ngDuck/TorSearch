using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorSearch.Scraper
{
    internal class TorInstanceControl
    {
        private static List<TorInstance> instances = new List<TorInstance>();

        private static int currentIndex = 0;

        private static object instancesLock = new object();

        /// <summary>
        /// Start the specified number of TOR instances
        /// </summary>
        /// <param name="instanceCount">Number of instances to start</param>
        /// <param name="restartTimeSpan">Amount of time to wait before restarting instances</param>
        public static void StartInstances(int instanceCount, TimeSpan restartTimeSpan)
        {
            for (int i = 0; i < instanceCount; i++)
            {
                var instance = new TorInstance(9150 + i, 8150 + i, restartTimeSpan);
                instances.Add(instance);
                instance.Start();
            }
        }

        /// <summary>
        /// Get the next available socks port
        /// </summary>
        /// <returns></returns>
        public static int GetNextInstancePort()
        {
            lock (instancesLock)
            {
                while (!instances[currentIndex].IsAvailable)
                {
                    Console.WriteLine($"Instance {currentIndex} unavailable. Trying the next one.");

                    currentIndex += 1;

                    if (currentIndex >= instances.Count)
                    {
                        currentIndex = 0;
                    }

                    Thread.Sleep(100);
                }

                var port = instances[currentIndex].SocksPort;

                currentIndex += 1;

                if (currentIndex >= instances.Count)
                {
                    currentIndex = 0;
                }

                return port;
            }
        }
    }
}
