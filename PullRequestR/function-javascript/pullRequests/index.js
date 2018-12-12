module.exports = function (context, req) {
    context.bindings.signalRMessages = [{
      "target": "pullRequestOpened",
      "arguments": [{
        Url : req.body.pull_request.url,
        PullRequestId : req.body.number,
        Title : req.body.pull_request.title,
        Avatar : req.body.pull_request.user.avatar_url,
        Login : req.body.pull_request.user.login
      }]
    }];
    context.done();
  };