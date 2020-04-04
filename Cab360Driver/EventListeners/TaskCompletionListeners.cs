using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Gms.Tasks;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

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