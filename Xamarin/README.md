## Xamarin Clients

**NOTE:** This sample currently does not work because of a blocking issue in the System.Memory package. See https://github.com/aspnet/SignalR/issues/1886 for more information. This issue is being resolved in an upcoming patch release.

The server application is located in the `ChatSample` folder.

Be sure to enter your own server url in the hub connection builder.

```csharp
            var hubConnection = new HubConnectionBuilder()
                    .WithUrl("<YOUR SERVER URL HERE>")
                    .Build();
```


The application starts by promtping you to enter your user name. 
After that you click connect and wait for confirmation to begin communicating with the server.

