using System;

namespace MySkype.Server.WebSocketModels
{
    public class WebSocketRequest
    {
        public RequestType RequestType { get; set; }

        public Guid UserId { get; set; }

        public Guid TargetSocketId { get; set; }
    }
}
