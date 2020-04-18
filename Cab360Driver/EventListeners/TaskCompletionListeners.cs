using Android.Gms.Tasks;
using System;

namespace Cab360Driver.EventListeners
{
    public class TaskCompletionListeners : Java.Lang.Object, IOnSuccessListener, IOnFailureListener
    {
        public event EventHandler<ResultArgs> Successful;
        public event EventHandler Failure;
        
        public class ResultArgs : EventArgs
        {
            public Java.Lang.Object Result { get; set; }
        }

        public void OnFailure(Java.Lang.Exception e)
        {
            Failure?.Invoke(this, new EventArgs());
        }

        public void OnSuccess(Java.Lang.Object result)
        {
            Successful?.Invoke(this, new ResultArgs { Result = result});
        }
    }
}