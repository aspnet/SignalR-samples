using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.AspNetCore.SignalR.Client;
using Xamarin.Forms;

namespace FormsClient
{
    public partial class MainPage : ContentPage
	{
        private ViewState _state = ViewState.Disconnected;

	    private ObservableCollection<string> _messages = new ObservableCollection<string>();
        private HubConnection _connection;

		public MainPage()
		{
			InitializeComponent();
		    MessagesListView.ItemsSource = _messages;

		}

        private async void ConnectDisconnectButton_Clicked(object sender, EventArgs e)
        {
            if (_state == ViewState.Connected)
            {
                UpdateState(ViewState.Disconnecting);

                try
                {
                    await _connection.StopAsync();
                }
                catch (Exception ex)
                {
                    AppendMessage($"An error occurred while disconnecting: {ex}");
                    UpdateState(ViewState.Connected);
                    return;
                }

                UpdateState(ViewState.Disconnected);
            }
            else
            {
                UpdateState(ViewState.Connecting);

                try
                {
                    _connection = new HubConnectionBuilder()
                        .WithUrl(ServerUrlEntry.Text)
                        .Build();

                    _connection.On<string>("Send", (message) =>
                    {
                        AppendMessage(message);
                    });

                    await _connection.StartAsync();
                }
                catch (Exception ex)
                {
                    AppendMessage($"An error occurred while connecting: {ex}");
                    UpdateState(ViewState.Disconnected);
                    return;
                }

                UpdateState(ViewState.Connected);
            }
        }

        private async void SendButton_Clicked(object sender, EventArgs e)
        {
            if (_state != ViewState.Connected)
            {
                await DisplayAlert("Error", "Must be Connected to Send!", "OK");
                return;
            }

            try
            {
                await _connection.SendAsync("Send", MessageEntry.Text);
                MessageEntry.Text = "";
            }
            catch (Exception ex)
            {
                AppendMessage($"An error occurred while sending: {ex}");
            }
        }

        private void AppendMessage(string message)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                _messages.Add(message);
                MessagesListView.ScrollTo(_messages.Last(), ScrollToPosition.End, animated: true);
            });
        }

        private void UpdateState(ViewState state)
        {
            if (_state == state)
            {
                return;
            }

            switch (state)
            {
                case ViewState.Disconnected:
                    SendButton.IsEnabled = false;
                    MessageEntry.IsEnabled = false;
                    ServerUrlEntry.IsEnabled = true;
                    ConnectDisconnectButton.IsEnabled = true;
                    ConnectDisconnectButton.Text = "Connect";
                    break;
                case ViewState.Connecting:
                    SendButton.IsEnabled = false;
                    MessageEntry.IsEnabled = false;
                    ServerUrlEntry.IsEnabled = false;
                    ConnectDisconnectButton.IsEnabled = false;
                    ConnectDisconnectButton.Text = "Connecting...";
                    break;
                case ViewState.Disconnecting:
                    SendButton.IsEnabled = false;
                    MessageEntry.IsEnabled = false;
                    ServerUrlEntry.IsEnabled = false;
                    ConnectDisconnectButton.IsEnabled = false;
                    ConnectDisconnectButton.Text = "Disconnecting...";
                    break;
                case ViewState.Connected:
                    SendButton.IsEnabled = true;
                    MessageEntry.IsEnabled = true;
                    ServerUrlEntry.IsEnabled = false;
                    ConnectDisconnectButton.IsEnabled = true;
                    ConnectDisconnectButton.Text = "Disconnect";
                    break;
            }
            _state = state;
        }

        private enum ViewState
        {
            Disconnected,
            Connecting,
            Connected,
            Disconnecting
        }
    }
}
