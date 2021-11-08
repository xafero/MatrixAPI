using libMatrix.Responses.Pushers;
using System;

namespace libMatrix
{
    public partial class Events
    {
        public class NotificationEventArgs : EventArgs
        {
            public Notification Notification { get; set; }
        }

        public event EventHandler<NotificationEventArgs> NotificationEvent;

        internal void FireNotificationEvent(Notification notif) => NotificationEvent?.Invoke(this, new NotificationEventArgs() { Notification = notif });
    }
}
