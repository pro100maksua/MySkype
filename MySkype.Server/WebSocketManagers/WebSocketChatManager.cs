namespace MySkype.Server.WebSocketManagers
{
    public class WebSocketChatManager //: WebSocketManager
    {
        //    private readonly IUsersRepository _usersRepository;


        //    private readonly ConcurrentDictionary<Guid, HashSet<Guid>> _chatRoomSessions;

        //    public WebSocketChatManager(IUsersRepository usersRepository, WebSocketConnectionManager connectionManager)
        //        : base(connectionManager)
        //    {
        //        _usersRepository = usersRepository;
        //    }

        //    public override async Task ReceiveAsync(WebSocket socket, WebSocketReceiveResult result, byte[] buffer)
        //    {
        //        var json = Encoding.UTF8.GetString(buffer, 0, result.Count);

        //        var message = JsonConvert.DeserializeObject<WebSocketRequest>(json);

        //        await SendFriendRequestAsync(socket, message);
        //    }

        //    private async Task SendToAllAsync(string message)
        //    {
        //        foreach (var pair in _connectionManager.GetAll())
        //        {
        //            if (pair.Value.State == WebSocketState.Open)
        //            {
        //                await SendFriendRequestAsync(pair.Value, message);
        //            }
        //        }
        //    }

        //    public async Task SendFriendRequestAsync(Guid senderId, Guid targetId)
        //    {
        //        var targetSocket = _connectionManager.Get(targetId);

        //        if (targetSocket != null)
        //        {
        //            var message = JsonConvert.SerializeObject(new { SenderId = senderId });

        //            await targetSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(message), 0, message.Length),
        //                WebSocketMessageType.Text, true, CancellationToken.None);
        //        }
        //    }

    }
}
