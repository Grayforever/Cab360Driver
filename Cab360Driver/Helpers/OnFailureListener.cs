using Android.Gms.Tasks;
using System;
using Object = Java.Lang.Object;

namespace Cab360Driver.Helpers
{
    public sealed class OnFailureListener : Object, IOnFailureListener
    {
        private readonly Action<Java.Lang.Exception> _onFailure;
        public OnFailureListener(Action<Java.Lang.Exception> onFailure)
        {
            _onFailure = onFailure;
        }
        public void OnFailure(Java.Lang.Exception e)
        {
            _onFailure?.Invoke(e);
        }

        
    }
}