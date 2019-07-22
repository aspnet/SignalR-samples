const signalR = require("@aspnet/signalr");

XMLHttpRequest = require("xmlhttprequest").XMLHttpRequest;
WebSocket = require("websocket").w3cwebsocket;

var hubConnection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5000/stocks")
    .build();

hubConnection.start()
    .then(() => {
        hubConnection.stream("StreamStocks").subscribe({
            next: (stock) => {
                console.log(stock);
                // console.log(stock.Symbol + " " + stock.Price);
            },
            error: (err) => { },
            complete: () => { }
        });
    });