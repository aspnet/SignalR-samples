using Android.App;
using Android.Widget;
using Android.OS;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace AndroidApp
{
    [Activity(Label = "Xamarin Android Chat Client", MainLauncher = true)]
    public class MainActivity : Activity
    {
        private ViewState _state = ViewState.Disconnected;
        private HubConnection _connection;
        private Button _connectButton;
        private ListView _listView;
        private Button _sendButton;
        private EditText _messageText;
        private EditText _serverUrlText;
        private ArrayAdapter _messages;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            //Setting up references to views
            _connectButton = FindViewById<Button>(Resource.Id.bConnect);
            _listView = FindViewById<ListView>(Resource.Id.lvMessages);
            _sendButton = FindViewById<Button>(Resource.Id.bSend);
            _messageText = FindViewById<EditText>(Resource.Id.etMessageText);
            _serverUrlText = FindViewById<EditText>(Resource.Id.etServerUrl);

            _sendButton.Click += SendButton_Click;
            _connectButton.Click += ConnectButton_Click;

            _messages = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, new List<string>());
            _listView.Adapter = _messages;
        }

        private async void ConnectButton_Click(object sender, EventArgs e)
        {
            if (_state == ViewState.Connected)
            {
                UpdateState(ViewState.Disconneting);

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
                        .WithUrl(_serverUrlText.Text)
                        .ConfigureLogging(logging => { logging.AddDebug(); })
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
                }

                UpdateState(ViewState.Connected);
            }
        }

        private async void SendButton_Click(object sender, EventArgs e)
        {
            if (_state != ViewState.Connected)
            {
                Toast.MakeText(this, "Must be Connected to Send!", ToastLength.Short).Show();
                return;
            }

            try
            {
                await _connection.SendAsync("Send", _messageText.Text);
                _messageText.Text = "";
            }
            catch (Exception ex)
            {
                AppendMessage($"An error occurred while sending: {ex}");
            }
        }

        private void UpdateState(ViewState state)
        {
            if (_state == state)
            {
                return;
            }

            switch (state)
            {
                case ViewState.Connected:
                    _connectButton.Text = "Disconnect";
                    _sendButton.Enabled = true;
                    _messageText.Enabled = true;
                    _serverUrlText.Enabled = false;
                    break;
                case ViewState.Disconnected:
                    _connectButton.Text = "Connect";
                    _sendButton.Enabled = false;
                    _messageText.Enabled = false;
                    _serverUrlText.Enabled = true;
                    break;
                case ViewState.Connecting:
                    _connectButton.Enabled = false;
                    _connectButton.Text = "Connecting...";
                    _sendButton.Enabled = false;
                    _messageText.Enabled = false;
                    _serverUrlText.Enabled = false;
                    break;
                case ViewState.Disconneting:
                    _connectButton.Enabled = false;
                    _connectButton.Text = "Disconnecting...";
                    _sendButton.Enabled = false;
                    _messageText.Enabled = false;
                    _serverUrlText.Enabled = false;
                    break;
            }

            _state = state;
        }

        private void AppendMessage(string message)
        {
            RunOnUiThread(() =>
            {
                _messages.Add(message);
                _messages.NotifyDataSetChanged();

                _listView.SmoothScrollToPosition(_messages.Count - 1);
            });
        }

        private enum ViewState
        {
            Disconnected,
            Connecting,
            Connected,
            Disconneting,
        }
    }
}

