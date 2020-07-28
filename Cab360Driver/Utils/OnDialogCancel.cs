using Android.Content;
using System;
using Object = Java.Lang.Object;

namespace Cab360Driver.Utils
{
    public sealed class OnDialogCancel : Object, IDialogInterface
    {
        private Action _cancel, _dismiss;
        public OnDialogCancel(Action cancel, Action dismiss)
        {
            _cancel = cancel;
            _dismiss = dismiss;
        }

        public void Cancel()
        {
            _cancel?.Invoke();
        }

        public void Dismiss()
        {
            _dismiss?.Invoke();
        }
    }
}