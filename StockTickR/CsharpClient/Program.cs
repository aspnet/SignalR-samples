using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Sockets;

namespace CsharpClient
{
    public class Program
    {
        static void Main(string[] args)
        {
            Task.Run(Run).Wait();
        }

        static async Task Run()
        {
            Console.WriteLine("Starting connection. Press Ctrl-C to close.");
            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, a) =>
            {
                a.Cancel = true;
                cts.Cancel();
            };

            await StartConnection(cts);

            // Keep client running until cancel requested.
            while (!cts.IsCancellationRequested)
            {
                await Task.Delay(250);
            }
        }

        private async static Task StartConnection(CancellationTokenSource cts)
        {
            var transports = new [] { TransportType.WebSockets, TransportType.ServerSentEvents, TransportType.LongPolling };
            HubConnection connection = null;
            foreach (var transport in transports)
            {
                try
                {
                    connection = await StartConnection(transport, cts);
                    Console.WriteLine($"Connected using {transport.ToString()} transport.");
                    break;
                }
                catch (Exception)
                {
                    Console.WriteLine($"Could not connect to server using {transport.ToString()} transport.");
                    if (transport == TransportType.LongPolling)
                    {
                        throw;
                    }
                }
            }

            // Do an initial check to see if we can start streaming the stocks
            var state = await connection.InvokeAsync<string>("GetMarketState");
            if (string.Equals(state, "Open"))
            {
                await StartStreaming(connection, cts);
            }
        }

        private async static Task<HubConnection> StartConnection(TransportType transportType, CancellationTokenSource cts)
        {
            var connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/stocks")
                // .WithConsoleLogger()
                .WithMessagePackProtocol()
                .WithTransport(transportType)
                .Build();

            connection.On("marketOpened", async () =>
            {
                await StartStreaming(connection, cts);
            });

            connection.Closed += async e =>
            {
                if (!await Reconnect(cts))
                {
                    Console.WriteLine($"Connection closed with error: {e} and reconnecting failed.");
                    cts.Cancel();
                }
                else
                {
                    Console.WriteLine("Client reconnected successfully.");
                }

                await connection.DisposeAsync();
            };

            await connection.StartAsync();
            return connection;
        }

        private async static Task StartStreaming(HubConnection connection, CancellationTokenSource cts)
        {
            try
            {
                var channel = await connection.StreamAsync<Stock>("StreamStocks", CancellationToken.None);
                while (await channel.WaitToReadAsync() && !cts.IsCancellationRequested)
                {
                    while (channel.TryRead(out var stock))
                    {
                        Console.WriteLine($"{stock.Symbol} {stock.Price}");
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error receiving results: {ex.Message}");
            }
        }

        private async static Task<bool> Reconnect(CancellationTokenSource cts)
        {
            for (var i = 0; i <= 5 && !cts.IsCancellationRequested; i++)
            {
                Console.WriteLine($"Reconnecting... Attempt {i}");
                try
                {
                    await StartConnection(cts);
                    return true;
                }
                catch
                {
                }
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
            return false;
        }
    }

    public class Stock
    {
        public string Symbol { get; set; }

        public decimal DayOpen { get; private set; }

        public decimal DayLow { get; private set; }

        public decimal DayHigh { get; private set; }

        public decimal LastChange { get; private set; }

        public decimal Change { get; set; }

        public double PercentChange { get; set; }

        public decimal Price { get; set; }
    }
}
