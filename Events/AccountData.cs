using libMatrix.Responses;
using System;

namespace libMatrix
{
    public partial class Events
    {
        public class AccountDataEventArgs : EventArgs
        {
            public MatrixEvents Event;
        }

        public event EventHandler<AccountDataEventArgs> AccountDataEvent;

        internal void FireAccountDataEvent(MatrixEvents evt) => AccountDataEvent?.Invoke(this, new AccountDataEventArgs { Event = evt });
    }
}
