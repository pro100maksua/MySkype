﻿using System;

namespace MySkype.Client.Services
{
    public class NotificationService
    {
        public event EventHandler<MyEventArgs> FriendRequestReceived;
        public event EventHandler<MyEventArgs> CallRequestReceived;
        public event EventHandler<MyEventArgs> CallAccepted;
        
        public void NotifyFriendRequest(Guid senderId)
        {
            FriendRequestReceived?.Invoke(this, new MyEventArgs { SenderId = senderId });
        }

        public void NotifyCallRequest(Guid senderId)
        {
            CallRequestReceived?.Invoke(this, new MyEventArgs { SenderId = senderId });
        }

        public void NotifyCallAccepted(Guid senderId)
        {
            CallAccepted?.Invoke(this, new MyEventArgs { SenderId = senderId });
        }
    }
}