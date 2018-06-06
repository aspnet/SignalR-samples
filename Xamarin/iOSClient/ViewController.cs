using System;
using System.Buffers;
using System.Collections.Generic;
using Foundation;
using Microsoft.AspNetCore.SignalR.Client;
using UIKit;

namespace iOSClient
{
    public partial class ViewController : UIViewController
    {
        private ViewState _state = ViewState.Disconnected;
        private MessagesDataSource _source = new MessagesDataSource();
        private HubConnection _connection;

        public ViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.

            MessagesTableView.Delegate = new MessagesTableViewDelegate();
            MessagesTableView.DataSource = _source;

            ServerUrlTextField.ShouldReturn = (sender) =>
            {
                sender.ResignFirstResponder();
                return true;
            };

            MessageTextField.ShouldReturn = (sender) =>
            {
                sender.ResignFirstResponder();
                return true;
            };
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        async partial void ConnectDisconnectButton_TouchUpInside(UIButton sender)
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
                        .WithUrl(ServerUrlTextField.Text)
                        .Build();

                    _connection.On<string>("Send", (message) =>
                    {
                        InvokeOnMainThread(() =>
                        {
                            AppendMessage(message);
                        });
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

        async partial void SendButton_TouchUpInside(UIButton sender)
        {
            if (_state != ViewState.Connected)
            {
                var alert = new UIAlertView()
                {
                    Title = "Error",
                    Message = "Must be Connected to Send!",
                };
                alert.AddButton("OK");
                alert.Show();
                return;
            }

            try
            {
                await _connection.SendAsync("Send", MessageTextField.Text);
            }
            catch (Exception ex)
            {
                AppendMessage($"An error ocurred while sending: {ex}");
                return;
            }
        }

        private void AppendMessage(string message)
        {
            _source.Messages.Add(message);
            MessagesTableView.ReloadData();

            // Scroll to the bottom
            var indexPath = NSIndexPath.FromRowSection(MessagesTableView.NumberOfRowsInSection(0) - 1, 0);
            MessagesTableView.ScrollToRow(indexPath, UITableViewScrollPosition.Bottom, animated: true);
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
                    SendButton.Enabled = false;
                    MessageTextField.Enabled = false;
                    ServerUrlTextField.Enabled = true;
                    ConnectDisconnectButton.Enabled = true;
                    ConnectDisconnectButton.SetTitle("Connect", UIControlState.Normal);
                    break;
                case ViewState.Connecting:
                    SendButton.Enabled = false;
                    MessageTextField.Enabled = false;
                    ServerUrlTextField.Enabled = false;
                    ConnectDisconnectButton.Enabled = false;
                    ConnectDisconnectButton.SetTitle("Connecting...", UIControlState.Normal);
                    break;
                case ViewState.Disconnecting:
                    SendButton.Enabled = false;
                    MessageTextField.Enabled = false;
                    ServerUrlTextField.Enabled = false;
                    ConnectDisconnectButton.Enabled = false;
                    ConnectDisconnectButton.SetTitle("Disconnecting...", UIControlState.Normal);
                    break;
                case ViewState.Connected:
                    SendButton.Enabled = true;
                    MessageTextField.Enabled = true;
                    ServerUrlTextField.Enabled = false;
                    ConnectDisconnectButton.Enabled = true;
                    ConnectDisconnectButton.SetTitle("Disconnect", UIControlState.Normal);
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

        private class MessagesTableViewDelegate : UITableViewDelegate
        {
            [Export("tableView:canPerformAction:forRowAtIndexPath:withSender:")]
            public override bool CanPerformAction(UITableView tableView, ObjCRuntime.Selector action, NSIndexPath indexPath, NSObject sender)
            {
                return action.Name.Equals("copy:");
            }

            public override bool ShouldShowMenu(UITableView tableView, NSIndexPath rowAtindexPath)
            {
                return true;
            }

            [Export("tableView:performAction:forRowAtIndexPath:withSender:")]
            public override void PerformAction(UITableView tableView, ObjCRuntime.Selector action, NSIndexPath indexPath, NSObject sender)
            {
                if (action.Name.Equals("copy:"))
                {
                    var text = tableView.CellAt(indexPath).TextLabel.Text;
                    UIPasteboard.General.SetData(NSData.FromString(text), "public.plain-text");
                }
            }
        }

        private class MessagesDataSource : UITableViewDataSource
        {
            private static readonly string CellIdentifier = "TableCell";
            public List<string> Messages = new List<string>();

            public override nint RowsInSection(UITableView tableView, nint section)
            {
                return Messages.Count;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                var item = Messages[indexPath.Row];

                // Try to dequeue a now-unused cell
                var cell = tableView.DequeueReusableCell(CellIdentifier);
                if (cell == null)
                {
                    // No reusable cells available, create a new one
                    cell = new UITableViewCell(UITableViewCellStyle.Default, CellIdentifier);
                }

                cell.TextLabel.Lines = 0;
                cell.TextLabel.LineBreakMode = UILineBreakMode.WordWrap;
                cell.TextLabel.Text = item;

                return cell;
            }
        }
    }
}
