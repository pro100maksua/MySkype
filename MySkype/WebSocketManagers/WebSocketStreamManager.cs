namespace MySkype.Server.WebSocketManagers
{
    public class WebSocketStreamManager //: WebSocketManager
    {
        //public WebSocketStreamManager(WebSocketConnectionManager connectionManager) : base(connectionManager)
        //{
        //}

        //public override async Task ReceiveAsync(WebSocket socket, WebSocketReceiveResult result, byte[] buffer)
        //{
        //    await SendBytesAsync(socket, buffer);
        //}

        //public async Task SendBytesAsync(WebSocket socket, byte[] videoBytes)
        //{
        //    if (socket.State == WebSocketState.Open)
        //    {
        //        await socket.SendAsync(new ArraySegment<byte>(videoBytes), WebSocketMessageType.Binary, true,
        //            CancellationToken.None);
        //    }
        //}

        //public async Task SendBytesToAllAsync(byte[] videoBytes)
        //{
        //    foreach (var pair in _connectionManager.GetAll())
        //    {
        //        if (pair.Value.State == WebSocketState.Open)
        //            await SendBytesAsync(pair.Value, videoBytes);
        //    }
        //}
    }
}
