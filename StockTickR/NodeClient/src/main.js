"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const signalR = require("@aspnet/signalr-client");
const EventSource = require("eventsource");
global.XMLHttpRequest = require("xmlhttprequest").XMLHttpRequest;
global.WebSocket = require("websocket").w3cwebsocket;
global.EventSource = EventSource;
global.btoa = require("btoa");
global.atob = require("atob");
const hubConnection = new signalR.HubConnection("http://localhost:5000/signalr");
hubConnection.start().then(() => {
    hubConnection.invoke("GetMarketState").then(function (state) {
        if (state === "Open") {
            streamStocks();
        }
        else {
            hubConnection.invoke("OpenMarket");
        }
    });
}).catch(err => {
    console.log(err);
});
hubConnection.on("marketOpened", function () {
    streamStocks();
});
function streamStocks() {
    hubConnection.stream("StreamStocks").subscribe({
        next: (stock) => {
            console.log(stock);
            // console.log(stock.Symbol + " " + stock.Price);
        },
        error: () => { },
        complete: () => { }
    });
}
