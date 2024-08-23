using System.Net;
using System.Net.WebSockets;
using System.Text;

Console.WriteLine("Starting websocket server...");

var listener = new HttpListener();
listener.Prefixes.Add("http://localhost:8080/ws/");
listener.Start();

Console.WriteLine("WebSocket server started at http://localhost:8080/ws/");

while (true)
{
    var context = await listener.GetContextAsync();

    if (context.Request.IsWebSocketRequest)
    {
        Console.WriteLine("Client connected");

        var wsContext = await context.AcceptWebSocketAsync(null);
        var webSocket = wsContext.WebSocket;

        const string predefinedMessage = "Welcome to the WebSocket server!";
        var messageBuffer = Encoding.UTF8.GetBytes(predefinedMessage);
        await webSocket.SendAsync(new ArraySegment<byte>(messageBuffer, 0, messageBuffer.Length),
            WebSocketMessageType.Text, true, CancellationToken.None);

        await HandleWebSocketConnection(webSocket);
    }
    else
    {
        context.Response.StatusCode = 400;
        context.Response.Close();
    }
}

static async Task HandleWebSocketConnection(WebSocket webSocket)
{
    var buffer = new byte[1024 * 4];
    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

    while (webSocket.State == WebSocketState.Open)
    {
        var receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
        Console.WriteLine($"Received: {receivedMessage}");

        // echo message back to client
        var responseBuffer = Encoding.UTF8.GetBytes(receivedMessage);
        await webSocket.SendAsync(new ArraySegment<byte>(responseBuffer, 0, responseBuffer.Length),
            WebSocketMessageType.Text, true, CancellationToken.None);

        result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
    }

    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing connection", CancellationToken.None);
    webSocket.Dispose();
}