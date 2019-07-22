## Xamarin Clients

Be sure to enter your own server url in the hub connection builder.

```csharp
            var hubConnection = new HubConnectionBuilder()
                    .WithUrl("<YOUR SERVER URL HERE>")
                    .Build();
```

The application starts by promtping you to enter your user name. 
After that you click connect and wait for confirmation to begin communicating with the server.

