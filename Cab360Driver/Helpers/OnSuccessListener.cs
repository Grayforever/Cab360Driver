using Android.Gms.Tasks;
using System;
using Object = Java.Lang.Object;

namespace Cab360Driver.Helpers
{
    public sealed class OnSuccessListener : Object, IOnSuccessListener
    {
        private readonly Action<Object> _onSuccess;
        public OnSuccessListener(Action<Object> onSuccess)
        {
            _onSuccess = onSuccess;
        }
        public void OnSuccess(Object result)
        {
            _onSuccess?.Invoke(result);
        }
    }
}