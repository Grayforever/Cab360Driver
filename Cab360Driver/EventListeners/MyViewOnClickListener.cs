using Android.Views;
using System;
using Object = Java.Lang.Object;

namespace Cab360Driver.EventListeners
{
    internal sealed class MyViewOnClickListener : Object, View.IOnClickListener
    {
        private readonly Action<View> _onClick;

        public MyViewOnClickListener(Action<View> onClick)
        {
            _onClick = onClick;
        }

        public void OnClick(View v) => _onClick?.Invoke(v);
    }
}