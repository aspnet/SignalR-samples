// Crockford's supplant method (poor man's templating)
if (!String.prototype.supplant) {
    String.prototype.supplant = function (o) {
        return this.replace(/{([^{}]*)}/g,
            function (a, b) {
                var r = o[b];
                return typeof r === 'string' || typeof r === 'number' ? r : a;
            }
        );
    };
}

var stockTable = document.getElementById('stockTable');
var stockTableBody = stockTable.getElementsByTagName('tbody')[0];
var rowTemplate = '<td>{Symbol}</td><td>{Price}</td><td>{DayOpen}</td><td>{DayHigh}</td><td>{DayLow}</td><td class="changeValue"><span class="dir {DirectionClass}">{Direction}</span> {Change}</td><td>{PercentChange}</td>';
var tickerTemplate = '<span class="symbol">{Symbol}</span> <span class="price">{Price}</span> <span class="changeValue"><span class="dir {DirectionClass}">{Direction}</span> {Change} ({PercentChange})</span>';
var stockTicker = document.getElementById('stockTicker');
var stockTickerBody = stockTicker.getElementsByTagName('ul')[0];
var up = '▲';
var down = '▼';

let connection = new signalR.HubConnection("/stocks");
connection.start().then(function () {
    connection.invoke("GetAllStocks").then(function (stocks) {
        for (let i = 0; i < stocks.length; i++) {
            displayStock(stocks[i]);
        }
    });

    connection.invoke("GetMarketState").then(function (state) {
        if (state === 'Open') {
            marketOpened();
            startStreaming();
        } else {
            marketClosed();
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
            connection.invoke("GetAllStocks").then(function (stocks) {
                for (let i = 0; i < stocks.length; ++i) {
                    displayStock(stocks[i]);
                }
            });
        });
    }
});

connection.on("marketOpened", function () {
    marketOpened();
    startStreaming();
});

connection.on("marketClosed", function () {
    marketClosed();
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

var pos = 30;
var tickerInterval;
stockTickerBody.style.marginLeft = '30px';

function moveTicker() {
    pos--;
    if (pos < -600) {
        pos = 500;
    }

    stockTickerBody.style.marginLeft = pos + 'px';
}

function marketOpened() {
    tickerInterval = setInterval(moveTicker, 20);
    document.getElementById('open').setAttribute("disabled", "disabled");
    document.getElementById('close').removeAttribute("disabled");
    document.getElementById('reset').setAttribute("disabled", "disabled");
}

function marketClosed() {
    if (tickerInterval) {
        clearInterval(tickerInterval);
    }
    document.getElementById('open').removeAttribute("disabled");
    document.getElementById('close').setAttribute("disabled", "disabled");
    document.getElementById('reset').removeAttribute("disabled");
}

function displayStock(stock) {
    var displayStock = formatStock(stock);
    addOrReplaceStock(stockTableBody, displayStock, 'tr', rowTemplate);
    addOrReplaceStock(stockTickerBody, displayStock, 'li', tickerTemplate);
}

function addOrReplaceStock(table, stock, type, template) {
    var child = createStockNode(stock, type, template);

    // try to replace
    var stockNode = document.querySelector(type + "[data-symbol=" + stock.Symbol + "]");
    if (stockNode) {
        var change = stockNode.querySelector(".changeValue");
        var prevChange = parseFloat(change.childNodes[1].data);
        if (prevChange > stock.Change) {
            child.className = "decrease";
        }
        else if (prevChange < stock.Change) {
            child.className = "increase";
        }
        else {
            return;
        }
        table.replaceChild(child, stockNode);
    } else {
        // add new stock
        table.appendChild(child);
    }
}

function formatStock(stock) {
    stock.Price = stock.Price.toFixed(2);
    stock.PercentChange = (stock.PercentChange * 100).toFixed(2) + '%';
    stock.Direction = stock.Change === 0 ? '' : stock.Change >= 0 ? up : down;
    stock.DirectionClass = stock.Change === 0 ? 'even' : stock.Change >= 0 ? 'up' : 'down';
    return stock;
}

function createStockNode(stock, type, template) {
    var child = document.createElement(type);
    child.setAttribute('data-symbol', stock.Symbol);
    child.setAttribute('class', stock.Symbol);
    child.innerHTML = template.supplant(stock);
    return child;
}