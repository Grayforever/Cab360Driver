using Android.Animation;
using System;
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