module.exports = function (context, req, connectionInfo) {
    context.res = { body: connectionInfo };
    context.done();
  };