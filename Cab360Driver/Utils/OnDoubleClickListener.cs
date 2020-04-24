using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Cab360Driver.Utils
{
    public class OnDoubleClickListener : Java.Lang.Object, View.IOnClickListener
    {
        private static readonly long DoubleClickTimeDelta = 300;
        private OnClickListener onClickListener;
        long lastClickTime = 0;

        public OnDoubleClickListener(OnClickListener onClickListener)
        {
            this.onClickListener = onClickListener;
        }

        public void OnClick(View v)
        {
            if (IsSingleClick())
            {
                onClickListener.OnClick(v);
            }
        }

        public class OnClickListener : Java.Lang.Object, View.IOnClickListener
        {
            public void OnClick(View v)
            {
                
            }
        }

        private bool IsSingleClick()
        {
            long clickTime = Java.Lang.JavaSystem.CurrentTimeMillis();
            long transcureTime = clickTime - lastClickTime;
            lastClickTime = clickTime;
            return transcureTime > DoubleClickTimeDelta;
        }

        private bool IsDoubleClick()
        {
            long clickTime = Java.Lang.JavaSystem.CurrentTimeMillis();
            long transcureTime = clickTime - lastClickTime;
            lastClickTime = clickTime;
            return transcureTime < DoubleClickTimeDelta;
        }

        
    }
}