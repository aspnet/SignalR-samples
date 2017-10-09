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
            var connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/stocks")
                .WithConsoleLogger()
                .WithMessagePackProtocol()
                .WithTransport(TransportType.WebSockets)
                .Build();

            await connection.StartAsync();

            Console.WriteLine("Starting connection. Press Ctrl-C to close.");
            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, a) =>
            {
                a.Cancel = true;
                cts.Cancel();
            };

            connection.Closed += e =>
            {
                Console.WriteLine("Connection closed with error: {0}", e);

                cts.Cancel();
                return Task.CompletedTask;
            };

            connection.On("marketOpened", async () =>
            {
                await StartStreaming();
            });

            // Do an initial check to see if we can start streaming the stocks
            var state = await connection.InvokeAsync<string>("GetMarketState");
            if (string.Equals(state, "Open"))
            {
                await StartStreaming();
            }

            // Keep client running until cancel requested.
            while (!cts.IsCancellationRequested)
            {
                await Task.Delay(250);
            }

            async Task StartStreaming()
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
