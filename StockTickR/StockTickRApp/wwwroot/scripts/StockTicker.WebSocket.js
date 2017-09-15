(function() {
    var ws = new WebSocket('ws://' + document.location.host + "/signalr");
    var invocationId = 0;
    var invocationTracker = {};

    function invoke(methodName, callback) {
        var id = "" + invocationId;
        ws.send(JSON.stringify({
            type: 1,
            invocationId: "" + id,
            target: methodName,
            arguments: [],
            nonBlocking: false
        })+ "\x1e");

        invocationTracker[id] = callback;
        invocationId++;
    }

    ws.onmessage = function(e) {
        var txt = e.data.substring(0, e.data.length - 1);
        var message = JSON.parse(txt);
        if (message.type !== 3) {
            console.log('Unexpected message: ' + message);
            return;
        }

        var callback = invocationTracker[message.invocationId];
        if (!callback) {
            console.log('Invocation not tracked. Message: ' + message);
            return;
        }

        callback(message.error
            ? new Error(message.error)
            : message.result);
    }

    ws.onopen = function(e) {
        // negotiate protocol
        ws.send('{ "protocol": "json"}\x1e');
        initialize();
        invoke('GetAllStocks', function(stocks) {
            displayStocks(stocks);
        });
        invoke('GetMarketState', function(state) {
            changeMarketState(state);
            if (state === "Open") {
                // TODO: startStreaming();
             }
        });
    }
})();



/*
let connection = new signalR.HubConnection("/signalr", { logging: signalR.LogLevel.Trace });
connection.start().then(function () {
    initialize();
    connection.invoke("GetMarketState").then(function (state) {
         changeMarketState(state);
    });

    document.getElementById('open').onclick = function () {
        connection.invoke("OpenMarket");
    }

    document.getElementById('close').onclick = function () {
        connection.invoke("CloseMarket");
    }

    document.getElementById('reset').onclick = function () {
        connection.invoke("Reset").then(function () {
            connection.invoke("GetAllStocks")
                .then(function(stocks) {
                    displayStocks(stocks)
                });
        });
    }
});

connection.on("marketOpened", function () {
    openMarket();
});

connection.on("marketClosed", function () {
    closeMarket();
});

*/