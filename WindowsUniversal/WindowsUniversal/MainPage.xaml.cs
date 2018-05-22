using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.AspNetCore.SignalR.Client;
using System.Threading;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace WindowsUniversal
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private HubConnection _connection;

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _connection = new HubConnectionBuilder()
                    .WithUrl(UrlTextBox.Text)
                    .Build();

                var syncContext = SynchronizationContext.Current;
                _connection.On<string>("Send", s =>
                {
                    syncContext.Post((state) => AppendMessage((string)state), s);
                });

                await _connection.StartAsync();

                ClearMessages();

                AppendMessage("Connected");

                ConnectGrid.Visibility = Visibility.Collapsed;
                DisconnectGrid.Visibility = Visibility.Visible;
                MessageTextBox.IsEnabled = true;
                SendButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                AppendMessage($"Error connecting: {ex}");
            }
        }

        private void ClearMessages()
        {
            MessagesListView.Items.Clear();
        }

        private void AppendMessage(string message)
        {
            MessagesListView.Items.Add(message);
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _connection.InvokeAsync("Send", MessageTextBox.Text);
            }
            catch (Exception ex)
            {
                AppendMessage($"Error sending: {ex}");
            }
        }

        private async void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _connection.StopAsync();
                await _connection.DisposeAsync();

                AppendMessage("Disconnected");

                ConnectGrid.Visibility = Visibility.Visible;
                DisconnectGrid.Visibility = Visibility.Collapsed;
                MessageTextBox.IsEnabled = false;
                SendButton.IsEnabled = false;
            }
            catch (Exception ex)
            {
                AppendMessage($"Error disconnecting: {ex}");
            }
        }
    }
}
