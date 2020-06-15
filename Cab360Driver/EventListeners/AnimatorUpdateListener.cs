using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Animation;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using static Android.Animation.ValueAnimator;
using Object = Java.Lang.Object;

namespace Cab360Driver.EventListeners
{
    public sealed class AnimatorUpdateListener : Object, IAnimatorUpdateListener
    {
        private readonly Action<ValueAnimator> _onAnimationUpdate;

        public AnimatorUpdateListener(Action<ValueAnimator> onAnimationupdate)
        {
            _onAnimationUpdate = onAnimationupdate;
        }

        public void OnAnimationUpdate(ValueAnimator animation)
        {
            _onAnimationUpdate.Invoke(animation);
        }
    }
}