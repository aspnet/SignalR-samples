const signalR = require("@microsoft/signalr");

var hubConnection = new signalR.HubConnectionBuilder()
    .withUrl("http://127.0.0.1:57853/stocks")
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