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
using static Android.Animation.Animator;
using Object = Java.Lang.Object;

namespace Cab360Driver.EventListeners
{
    public sealed class AnimatorListener : Object, IAnimatorListener
    {
        private readonly Action<Animator> _onAnimationEnd;
        private readonly Action<Animator> _onAnimationCancel;
        private readonly Action<Animator> _onAnimationRepeat;
        private readonly Action<Animator> _onAnimationStart;

        public AnimatorListener(Action<Animator> onAnimationEnd, Action<Animator> onAnimationCancel, Action<Animator> onAnimationRepeat, Action<Animator> onAnimationStart)
        {
            _onAnimationEnd = onAnimationEnd;
            _onAnimationCancel = onAnimationCancel;
            _onAnimationRepeat = onAnimationRepeat;
            _onAnimationStart = onAnimationStart;
        }

        public void OnAnimationCancel(Animator animation)
        {
            _onAnimationCancel?.Invoke(animation);
        }

        public void OnAnimationEnd(Animator animation)
        {
            _onAnimationEnd?.Invoke(animation);
        }

        public void OnAnimationRepeat(Animator animation)
        {
            _onAnimationRepeat?.Invoke(animation);
        }

        public void OnAnimationStart(Animator animation)
        {
            _onAnimationStart?.Invoke(animation);
        }
    }
}