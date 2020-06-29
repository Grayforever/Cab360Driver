using Android.Gms.Tasks;
using System;

namespace Cab360Driver.Helpers
{
    public sealed class OnCompleteListener : Java.Lang.Object, IOnCompleteListener
    {
        private Action<Task> _t;
        public OnCompleteListener(Action<Task> t)
        {
            _t = t;
        }
        public void OnComplete(Task task)
        {
            _t?.Invoke(task);
        }
    }
}