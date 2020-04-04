using System;

namespace Cab360Driver.EventListeners
{
    public class GetSessionListener
    {
        public event EventHandler<GetSessionEventArgs> GetSession;

        public class GetSessionEventArgs : EventArgs
        {
            public string _stage { get; set; }
        }

        public void Invoker(string stage)
        {
            this.GetSession?.Invoke(this, new GetSessionEventArgs { _stage = stage });
        }
    }
}