using System;

namespace MySkype.WpfClient.Services
{
    public class NotificationService
    {
        public event EventHandler<MyEventArgs> FriendRequestReceived;
        public event EventHandler<MyEventArgs> CallRequestReceived;
        public event EventHandler<MyEventArgs> CallAccepted;
        public event EventHandler<MyEventArgs> CallRejected;
        public event EventHandler<MyEventArgs> CallEnded;

        public void NotifyCallRejected(Guid senderId)
        {
            CallRejected?.Invoke(this, new MyEventArgs { SenderId = senderId });
        }

        public void NotifyCallEnded(Guid senderId)
        {
            CallEnded?.Invoke(this, new MyEventArgs { SenderId = senderId });
        }

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