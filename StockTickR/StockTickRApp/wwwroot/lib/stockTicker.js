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
var rowTemplate = '<td>{symbol}</td><td>{price}</td><td>{dayOpen}</td><td>{dayHigh}</td><td>{dayLow}</td><td class="changeValue"><span class="dir {directionClass}">{direction}</span> {change}</td><td>{percentChange}</td>';
var tickerTemplate = '<span class="symbol">{symbol}</span> <span class="price">{price}</span> <span class="changeValue"><span class="dir {directionClass}">{direction}</span> {change} ({percentChange})</span>';
var stockTicker = document.getElementById('stockTicker');
var stockTickerBody = stockTicker.getElementsByTagName('ul')[0];
var up = '▲';
var down = '▼';

let connection = new signalR.HubConnectionBuilder()
    .withUrl("/stocks")
    .build();

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
    var stockNode = document.querySelector(type + "[data-symbol=" + stock.symbol + "]");
    if (stockNode) {
        var change = stockNode.querySelector(".changeValue");
        var prevChange = parseFloat(change.childNodes[1].data);
        if (prevChange > stock.change) {
            child.className = "decrease";
        }
        else if (prevChange < stock.change) {
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
    stock.price = stock.price.toFixed(2);
    stock.percentChange = (stock.percentChange * 100).toFixed(2) + '%';
    stock.direction = stock.change === 0 ? '' : stock.change >= 0 ? up : down;
    stock.directionClass = stock.change === 0 ? 'even' : stock.change >= 0 ? 'up' : 'down';
    return stock;
}

function createStockNode(stock, type, template) {
    var child = document.createElement(type);
    child.setAttribute('data-symbol', stock.symbol);
    child.setAttribute('class', stock.symbol);
    child.innerHTML = template.supplant(stock);
    return child;
}