import * as signalR from "@aspnet/signalr-client";
import * as EventSource from "eventsource";

(<any>global).XMLHttpRequest = require("xmlhttprequest").XMLHttpRequest;
(<any>global).WebSocket = require("websocket").w3cwebsocket;
(<any>global).EventSource = EventSource;
(<any>global).btoa = require("btoa");
(<any>global).atob = require("atob");

const hubConnection : signalR.HubConnection = new signalR.HubConnection("http://localhost:5000/signalr");

hubConnection.start().then(() => {
    hubConnection.invoke("GetMarketState").then(function (state : string): void {
        if (state === "Open") {
            streamStocks();
        } else {
            hubConnection.invoke("OpenMarket");
        }
    });
}).catch(err => {
    console.log(err);
});

hubConnection.on("marketOpened", function(): void {
    streamStocks();
});

function streamStocks (): void {
    hubConnection.stream("StreamStocks").subscribe({
        next: (stock : any) => {
            console.log(stock);
            // console.log(stock.Symbol + " " + stock.Price);
        },
        error: () => {},
        complete: () => {}
    });
}