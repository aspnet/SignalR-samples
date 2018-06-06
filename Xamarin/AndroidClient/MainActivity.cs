extern alias slowspan;

using Android.App;
using Android.Widget;
using Android.OS;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.SignalR.Protocol;

namespace AndroidApp
{
    [Activity(Label = "Xamarin Android Chat Client", MainLauncher = true)]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            MakeProtocol();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void MakeProtocol()
        {
            // TypeLoadException at runtime
            var proto = new JsonHubProtocol();

            // Can't even compile this...
            //var proto = new MyHubProtocol();
        }

        //private class MyHubProtocol : IHubProtocol
        //{
        //    public string Name => throw new NotImplementedException();

        //    public int Version => throw new NotImplementedException();

        //    public TransferFormat TransferFormat => throw new NotImplementedException();

        //    public slowspan::System.ReadOnlyMemory<byte> GetMessageBytes(HubMessage message)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    public bool IsVersionSupported(int version)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    public bool TryParseMessage(ref slowspan::System.Buffers.ReadOnlySequence<byte> input, IInvocationBinder binder, out HubMessage message)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    public void WriteMessage(HubMessage message, slowspan::System.Buffers.IBufferWriter<byte> output)
        //    {
        //        throw new NotImplementedException();
        //    }
        //}
    }
}

