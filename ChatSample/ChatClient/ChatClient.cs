using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Sockets;
using Microsoft.Extensions.Logging;

namespace ChatClient
{
    public class ChatClient
    {
        private readonly string _name;
        private readonly CancellationTokenSource _lifetimeCts = new CancellationTokenSource();
        private volatile HubConnection _hubConnection;
        private volatile Task _reconnectTask = Task.CompletedTask;

        public ChatClient(string name)
        {
            _name = name;
        }

        public async Task Run()
        {
            Console.WriteLine("Starting connection...");
            _hubConnection = await StartWithFallback();
            Console.WriteLine("Connection started.");
            await Read();

            await _reconnectTask;
            await _hubConnection.DisposeAsync();
        }

        private async Task Read()
        {
            Console.WriteLine("Starting connection. Press Ctrl-C to close.");
            Console.CancelKeyPress += (sender, a) =>
            {
                a.Cancel = false;
                _lifetimeCts.Cancel();
            };

            var exitTask = Task.Delay(-1, _lifetimeCts.Token);

            while (!_lifetimeCts.IsCancellationRequested)
            {
                Console.Write("> ");
                var result = await Task.WhenAny(Task.Run(() => Console.ReadLine()), exitTask);
                if (result == exitTask || _lifetimeCts.IsCancellationRequested)
                {
                    break;
                }

                var line = ((Task<string>)result).Result;
                // line can be null when Ctrl-C is pressed
                if (line != null)
                {
                    await SendMessage(line);
                }
            }
        }

        private async Task SendMessage(string message)
        {
            try
            {
                await _hubConnection.SendAsync("Send", _name, message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Message not sent due to {ex.Message}");
            }
        }

        private async Task<HubConnection> StartWithFallback()
        {
            for (var transport = TransportType.WebSockets;
                transport <= TransportType.LongPolling && !_lifetimeCts.IsCancellationRequested;
                transport = (TransportType)((int)transport << 1))
            {
                var hubConnection = new HubConnectionBuilder()
                    .WithUrl("http://localhost:5000/chat")
                    .WithTransport(transport)
                    .WithConsoleLogger(LogLevel.Trace)
                    .Build();

                hubConnection.On<string, string>("broadcastMessage", (name, message) => Console.Write($"{name}: {message}\n> "));

                hubConnection.Closed += ex =>
                {
                    Interlocked.Exchange(ref _reconnectTask, Reconnect());
                    return Task.CompletedTask;
                };

                try
                {
                    await hubConnection.StartAsync();
                    return hubConnection;
                }
                catch
                {
                    Console.WriteLine($"Could not start connection using {transport.ToString()} transport.");
                }
            }

            throw new InvalidOperationException("Connection could not be started.");
        }

        private async Task Reconnect()
        {
            for (var i = 1; i <= 5 && !_lifetimeCts.IsCancellationRequested; i++)
            {
                Console.WriteLine($"Reconnect attempt {i}");
                try
                {
                    var connection = await StartWithFallback();
                    Interlocked.Exchange(ref _hubConnection, connection);
                    Console.WriteLine($"Reconnected");
                    return;
                }
                catch { }

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(3), _lifetimeCts.Token);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }

            Console.WriteLine($"Could not reconnect - stopping connection.");
            _lifetimeCts.Cancel();
        }
    }
}
