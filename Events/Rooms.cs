using libMatrix.Responses;
using System;

namespace libMatrix
{
    public partial class Events
    {
        public class RoomJoinEventArgs : EventArgs
        {
            public string Room { get; set; }
            public MatrixEventRoomJoined Event { get; set; }
        }

        public class RoomInviteEventArgs : EventArgs
        {
            public string Room { get; set; }
            public MatrixEventRoomInvited Event { get; set; }
        }

        public class RoomLeaveEventArgs : EventArgs
        {
            public string Room { get; set; }
            public MatrixEventRoomLeft Event { get; set; }
        }

        public class RoomCreateEventArgs : EventArgs
        {
            public string RoomID { get; set; }
        }

        public class RoomAliasEventArgs : EventArgs
        {
            public string RoomID { get; set; }
            public string[] Servers { get; set; }
        }

        public event EventHandler<RoomJoinEventArgs> RoomJoinEvent;
        public event EventHandler<RoomInviteEventArgs> RoomInviteEvent;
        public event EventHandler<RoomLeaveEventArgs> RoomLeaveEvent;
        public event EventHandler<RoomCreateEventArgs> RoomCreateEvent;
        public event EventHandler<RoomAliasEventArgs> RoomAliasEvent;

        internal void FireRoomJoinEvent(string room, MatrixEventRoomJoined evt) => RoomJoinEvent?.Invoke(this, new RoomJoinEventArgs { Room = room, Event = evt });
        internal void FireRoomInviteEvent(string room, MatrixEventRoomInvited evt) => RoomInviteEvent?.Invoke(this, new RoomInviteEventArgs { Room = room, Event = evt });
        internal void FireRoomLeaveEvent(string room, MatrixEventRoomLeft evt) => RoomLeaveEvent?.Invoke(this, new RoomLeaveEventArgs { Room = room, Event = evt });
        internal void FireRoomCreateEvent(string room) => RoomCreateEvent?.Invoke(this, new RoomCreateEventArgs { RoomID = room });
        internal void FireRoomAliasEvent(string room, string[] servers) => RoomAliasEvent?.Invoke(this, new RoomAliasEventArgs { RoomID = room, Servers = servers });
    }
}
