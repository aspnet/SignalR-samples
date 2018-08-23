// Helper functions
function addMessage(name, message) {
    // Html encode display name and message.
    var encodedName = name;
    var encodedMsg = message;
    // Add the message to the page.
    var liElement = document.createElement('li');
    liElement.innerHTML = '<strong>' + encodedName + '</strong>:&nbsp;&nbsp;' + encodedMsg;
    document.getElementById('discussion').appendChild(liElement);
}

document.addEventListener('DOMContentLoaded', function () {
    var messageInput = document.getElementById('message');

    // Get the user name and store it to prepend to messages.
    var name = prompt('Enter your name:', '');
    // Set initial focus to message input box.
    messageInput.focus();

    // Do SignalR things!

    // Create a function that the hub can call to broadcast messages.
    connection.on('broadcastMessage', function (name, message) {
    });

    // On click handler for the "send" button
    document.getElementById('sendmessage').addEventListener('click', function (event) {
        // Send the message somehow?

        // Clear text box and reset focus for next comment.
        messageInput.value = '';
        messageInput.focus();
        event.preventDefault();
    });
});