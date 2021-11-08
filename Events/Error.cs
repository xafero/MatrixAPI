using System;

namespace libMatrix
{
    public partial class Events
    {
        public class ErrorEventArgs : EventArgs
        {
            public string Message { get; set; }
        }

        public event EventHandler<ErrorEventArgs> ErrorEvent;

        internal void FireErrorEvent(string err) => ErrorEvent?.Invoke(this, new ErrorEventArgs() { Message = err });
    }
}
