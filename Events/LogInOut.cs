using System;
using libMatrix.Responses.Session;

namespace libMatrix
{
    public partial class Events
    {
        public class LoginEventArgs : EventArgs
        {
            public string AccessToken;
            public string DeviceID;
            public string UserID;
        }

        public event EventHandler<ErrorEventArgs> LoginFailEvent;
        public event EventHandler<LoginEventArgs> LoginEvent;
        public event EventHandler<LoginEventArgs> LogoutEvent;

        internal void FireLoginFailEvent(string message) => LoginFailEvent?.Invoke(this, new ErrorEventArgs { Message = message });
        internal void FireLoginEvent(LoginResponse resp) => LoginEvent?.Invoke(this, new LoginEventArgs {
            AccessToken = resp.AccessToken,
            DeviceID = resp.DeviceID,
            UserID = resp.UserID
        });
        internal void FireLogoutEvent() => LogoutEvent?.Invoke(this, new LoginEventArgs { });
    }
}
