let connection = new signalR.HubConnection("/signalr", { logging: signalR.LogLevel.Trace });
connection.start().then(function () {
    initialize();

    connection.invoke("GetAllStocks").then(function (stocks) {
        displayStocks(stocks);
    });

    connection.invoke("GetMarketState").then(function (state) {
         changeMarketState(state);
         if (state === "Open") {
            startStreaming();
         }
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

function startStreaming() {
    connection.stream("StreamStocks").subscribe({
        close: false,
        next: displayStock,
        error: function (err) {
            logger.log(err);
        }
    });
}
