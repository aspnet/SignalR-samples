
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;

namespace Company.Function
{
    public static class PullRequests
    {
        [FunctionName("PullRequests")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequest req, 
            [SignalR(HubName="repomonitor")] IAsyncCollector<SignalRMessage> messages,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a pull request-related call.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string action = data.action;

            var args = new {
                Url = data.pull_request.url,
                PullRequestId = data.number,
                Title = data.pull_request.title,
                Avatar = data.pull_request.user.avatar_url,
                Login = data.pull_request.user.login
            };

            await messages.AddAsync(new SignalRMessage {
                Target = "pullRequestOpened",
                Arguments = new [] { args }
            });

            return (ActionResult)new OkObjectResult(args);
        }
    }
}
