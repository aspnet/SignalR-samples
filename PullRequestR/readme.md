## Sample Setup

### Requirements

This setup process assumes you have the following tools installed:

1. [Visual Studio Code](https://code.visualstudio.com)
1. The [Azure Extension for Visual Studio Code](https://marketplace.visualstudio.com/items?itemName=ms-vscode.vscode-node-azure-pack)
1. [JetBrains IntelliJ Idea](https://www.jetbrains.com/idea/) with the supporting prerequisites for developing Android applications in Java. 
1. A [Microsoft Azure](https://azure.microsoft.com/en-us/free/) subscription. You can sign up for a free account [here](https://azure.microsoft.com/en-us/free/). 

Note - you could also use Visual Studio 2017+ with the Azure workload installed to do these items, use [Android Studio](https://developer.android.com/studio/) for your client development, or any other setup you prefer. 

### Setup Process

1. Create an Azure Resource group for the resources you'll create for the app. 
1. Create a new Azure Function in the new resource group. 
1. Create a new Azure SignalR Service instance in the same resource group (free tier is fine). 
1. Set the `AzureSignalRConnectionString` environment variable in your Azure Function to be the connection string of the Azure SignalR Service you created. 
1. Open the project workspace in this directory by typing `code .\project.code-workspace` at the command line.
1. Deploy **either** the `function-dotnet` code if you want a .NET back-end Function, or the `function-javascript` if you'd like to have a Node.js back-end function. Both sets of code operate the same way and will achieve the same result. 
1. Change line 33 of  `android\app\src\main\java\com.example.pullrequestr\MainActivity.java` from this:

    ```java
    String url = "https://YOUR-FUNCTION-URI.azurewebsites.net/api";
    ```

    to be the URL of your function, like this:

    ```java
    String url = "https://MyGitHubReceiver.azurewebsites.net/api";
    ```
1. In the GitHub repository you wish to monitor, create a new WebHook and provide the URL of your function as the target URL, with the `pullrequests` suffix. Given the example URL above, the full target URL for your GitHub WebHook would be `https://MyGitHubReceiver.azurewebsites.net/pullrequests`. 
1. Change the **Content type** of the request that will be sent to your Azure Function to `application/json`. 
1. In the Function App settings blade of the Azure portal, copy the `default` host key and paste it into the **Secret** property for the GitHub WebHook. 
1. Make sure to enable all pull request events so new pull request, closes, comments, and other activity will all be sent to your function. 
1. Save the WebHook.
1. Run the Android app in the debugger.  
1. Branch your repository (or fork it), make a change, and send pull request to your repository. 