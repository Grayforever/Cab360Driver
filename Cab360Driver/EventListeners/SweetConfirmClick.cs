using CN.Pedant.SweetAlert;
using System;
using static CN.Pedant.SweetAlert.SweetAlertDialog;

namespace Cab360Driver.EventListeners
{
    public sealed class SweetConfirmClick : Java.Lang.Object, IOnSweetClickListener
    {
        private readonly Action<SweetAlertDialog> _onClick;
        public SweetConfirmClick(Action<SweetAlertDialog> onClick)
        {
            _onClick = onClick;
        }
        public void OnClick(SweetAlertDialog p0)
        {
            _onClick?.Invoke(p0);
        }
    }
}