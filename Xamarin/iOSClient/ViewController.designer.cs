// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace iOSClient
{
    [Register ("ViewController")]
    partial class ViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton ConnectDisconnectButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITableView MessagesTableView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField MessageTextField { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton SendButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField ServerUrlTextField { get; set; }

        [Action ("ConnectDisconnectButton_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void ConnectDisconnectButton_TouchUpInside (UIKit.UIButton sender);

        [Action ("SendButton_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void SendButton_TouchUpInside (UIKit.UIButton sender);

        void ReleaseDesignerOutlets ()
        {
            if (ConnectDisconnectButton != null) {
                ConnectDisconnectButton.Dispose ();
                ConnectDisconnectButton = null;
            }

            if (MessagesTableView != null) {
                MessagesTableView.Dispose ();
                MessagesTableView = null;
            }

            if (MessageTextField != null) {
                MessageTextField.Dispose ();
                MessageTextField = null;
            }

            if (SendButton != null) {
                SendButton.Dispose ();
                SendButton = null;
            }

            if (ServerUrlTextField != null) {
                ServerUrlTextField.Dispose ();
                ServerUrlTextField = null;
            }
        }
    }
}