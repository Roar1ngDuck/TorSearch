using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorSearch.Scraper
{
    internal class TorInstance
    {
        public int SocksPort { get; set; }
        private int ControlPort { get; set; }

        public bool IsAvailable { get; set; }

        private Process torProcess { get; set; }
        private Timer RestartTimer { get; set; }

        private TimeSpan RestartTimeSpan { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="socksPort">Socks port to use for TOR instance</param>
        /// <param name="controlPort">Control port to use for TOR instance</param>
        /// <param name="restartTimeSpan">Amount of time to wait before restarting TOR instance</param>
        public TorInstance(int socksPort, int controlPort, TimeSpan restartTimeSpan)
        {
            SocksPort = socksPort;
            ControlPort = controlPort;
            RestartTimeSpan = restartTimeSpan;
        }

        /// <summary>
        /// Start the TOR instance
        /// </summary>
        public void Start()
        {
            Console.WriteLine($"Starting TOR with socks port {SocksPort}");

            if (RestartTimeSpan.TotalSeconds != 0)
            {
                Console.WriteLine("Set timer");
                RestartTimer = new Timer((e) =>
                {
                    RestartInstance();
                }, null, RestartTimeSpan, RestartTimeSpan);
            }

            TryStartUntilSuccess();

            new Thread(() => InstanceWatcher()).Start();

            IsAvailable = true;
        }

        /// <summary>
        /// Try to start the TOR instance and retry if a failure occurs
        /// </summary>
        private void TryStartUntilSuccess()
        {
            // Set execution timeout to 60 seconds
            while (!Helpers.ExecuteWithTimeLimit(TimeSpan.FromSeconds(60), () =>
            {
                var currentDirectory = Environment.CurrentDirectory;

                string dataDirectory = Path.Combine(currentDirectory, "tor\\Datadirs\\" + SocksPort);

                // Try to delete TOR data directory if it exists
                if (Directory.Exists(dataDirectory))
                {
                    try
                    {
                        Directory.Delete(dataDirectory, true);
                    }
                    catch { }
                }

                Directory.CreateDirectory(dataDirectory);

                int currentProcessId = Process.GetCurrentProcess().Id;

                torProcess = new Process();
                torProcess.StartInfo = new ProcessStartInfo()
                {
                    FileName = Path.Combine(currentDirectory, "tor\\Tor\\tor.exe"),
                    Arguments = $"DataDirectory \"{dataDirectory}\" " +
                    $"+__SocksPort \"127.0.0.1:{SocksPort} IPv6Traffic PreferIPv6\" " +
                    $"+__ControlPort {ControlPort} " +
                    $"__OwningControllerProcess {currentProcessId}", // Tor process will exit when the owning process exits
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };

                torProcess.Start();

                while (true)
                {
                    var output = torProcess.StandardOutput.ReadLine();

                    if (output == null)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }

                    if (output.Contains("Bootstrapped"))
                    {
                        string percentage = output.Substring(output.IndexOf("Bootstrapped") + 13);
                        percentage = percentage.Substring(0, percentage.IndexOf("%") + 1);

                        Console.WriteLine(percentage);
                    }

                    if (output.Contains("Bootstrapped 100% (done): Done"))
                    {
                        break;
                    }
                }
            }))
            {
                Console.WriteLine("Timeout while starting TOR process. Retrying...");

                try
                {
                    torProcess.Kill();
                }
                catch { }

                Thread.Sleep(10000);
            }
        }

        /// <summary>
        /// Kill the currently running instance and start a new one
        /// </summary>
        private void RestartInstance()
        {
            Console.WriteLine("Restarting instance");

            IsAvailable = false;

            try
            {
                torProcess.Kill();
            }
            catch { }

            Thread.Sleep(5000);

            TryStartUntilSuccess();

            IsAvailable = true;
        }

        private void InstanceWatcher()
        {
            while (true)
            {
                while (torProcess == null)
                {
                    Thread.Sleep(1000);
                }

                torProcess.WaitForExit();

                if (IsAvailable)
                {
                    RestartInstance();
                }

                Thread.Sleep(10000);
            }
        }
    }
}
