using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using System.Net;

namespace DynamicDNSUpdater
{
    class Program
    {
        class App
        {
            private readonly SemaphoreSlim _disposeLock = new(1, 1);
            private ManualResetEvent _quitEvent = new(false);
            private StateReaderWriter? _stateReaderWriter;
            private Timer? _timer;
            private bool _disposed;

#if DOCKER
            private const string DATA_BASE_PATH = "/data/";
#else
            private const string DATA_BASE_PATH = "";
#endif

            public async Task RunAsync()
            {
                _stateReaderWriter = new(DATA_BASE_PATH + "DDNS_data.json");
                Console.Write("Reading state... ");
                State state = await _stateReaderWriter.ReadAsync();
                Console.WriteLine("Done");

                Console.Write("Reading config... ");
                ConfigReaderWriter configReaderWriter = new(DATA_BASE_PATH + "DDNS_config.json");
                Config? config = await configReaderWriter.ReadAsync();
                if (config == null)
                {
                    Console.WriteLine("Failed - File not found");
                    Console.Write("Creating new config file... ");
                    try
                    {
                        File.Create(DATA_BASE_PATH + "DDNS_config.json").Close();
                        Config newConfig = new()
                        {
                            Provider = Config.DDNSProvider.Namecheap,
                            Password = "your-ddns-password-here",
                            Domains = new []
                            {
                                new Config.DomainConfig() {
                                    Domain = "example.com",
                                    Hosts = new string[] { "@", "www" }
                                }
                            },
                            UpdateFrequencySeconds = 15*60
                        };
                        await configReaderWriter.WriteAsync(newConfig);
                        Console.WriteLine("Done");
                        Console.WriteLine($"*** Edit the {DATA_BASE_PATH}DDNS_config.json file to configure settings, then restart this application. ***");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed - {ex.Message}");
                    }

                    Environment.Exit(1);
                    return;
                }
                else if (!config.IsValid(out string? configError))
                {
                    Console.WriteLine($"Failed - {configError}");

                    Environment.Exit(2);
                    return;
                }

                Console.WriteLine("Done");

                DDNSUpdater updater = config.Provider switch
                {
                    Config.DDNSProvider.Namecheap => new NamecheapDDNSUpdater(config.Domains, config.Password),
                    _ => throw new Exception($"Unrecognised provider."),
                };
                IPFinder IPFinder = new();

                Console.WriteLine();
                Console.WriteLine($"Provider: {config.Provider}");
                Console.WriteLine($"Update Interval: {config.UpdateFrequencySeconds} seconds");
                int hostsCount = config.Domains.Aggregate(0, (acc, val) => acc + val.Hosts.Length);
                Console.WriteLine($"Hosts: {hostsCount}");

                _timer = new(async e =>
                {
                    await _disposeLock.WaitAsync();
                    try
                    {
                        Console.WriteLine();
                        Console.Write("Getting current public IP address... ");
                        IPFinder.IPFinderResponse ipFinderResponse = await IPFinder.FindAsync();
                        if (!ipFinderResponse.IPFound)
                        {
                            Console.WriteLine($"Failed - {ipFinderResponse.ErrorMessage}");
                            return;
                        }
                        Console.WriteLine($"Done [{ipFinderResponse.IPAddress}]");

                        if (ipFinderResponse.IPAddress.Equals(state.CurrentIPAddress))
                        {
                            Console.WriteLine("IP address has not changed.");
                            return;
                        }

                        if (state.CurrentIPAddress == null) Console.WriteLine("Performing first time sync.");
                        else Console.WriteLine("IP address has changed. Updating dynamic DNS...");

                        bool success = await updater.UpdateAsync(ipFinderResponse.IPAddress);
                        if (!success)
                        {
                            // TODO
                            return;
                        }

                        if (state.CurrentIPAddress != null)
                        {
                            IPAddress[] arr = new IPAddress[state.PreviousIPAddresses.Length + 1];
                            Array.Copy(state.PreviousIPAddresses, 0, arr, 1, state.PreviousIPAddresses.Length);
                            arr[0] = state.CurrentIPAddress;
                            state.PreviousIPAddresses = arr;
                        }
                        state.CurrentIPAddress = ipFinderResponse.IPAddress;
                        state.LastUpdatedTimestamp = DateTime.Now;
                        await _stateReaderWriter.WriteAsync(state);
                    }
                    finally
                    {
                        _disposeLock.Release();
                    }
                }, null, TimeSpan.Zero, TimeSpan.FromSeconds(config.UpdateFrequencySeconds));

                _quitEvent.WaitOne();
            }

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;

                _disposeLock.Wait(TimeSpan.FromSeconds(3));
                Console.WriteLine("Preparing to exit...");
                _timer?.Dispose();
                _stateReaderWriter?.Dispose();
                _quitEvent.Set();
            }
        }

        static async Task Main(string[] args)
        {
            App app = new();

            Console.CancelKeyPress += (object? sender, ConsoleCancelEventArgs e) =>
            {
                e.Cancel = true;
                app.Dispose();
            };

            AppDomain.CurrentDomain.ProcessExit += (object? sender, EventArgs e) => app.Dispose();

            await app.RunAsync();
        }
    }
}
