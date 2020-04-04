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
using CN.Pedant.SweetAlert;

namespace Cab360Driver.EventListeners
{
    public class WarningEvent1 : Java.Lang.Object ,SweetAlertDialog.IOnSweetClickListener
    {
        public event EventHandler OnConfirmClick;
        public void OnClick(SweetAlertDialog p0)
        {
            OnConfirmClick.Invoke(this, new EventArgs());
            p0.DismissWithAnimation();
        }
    }

    public class WarningEvent2 : Java.Lang.Object, SweetAlertDialog.IOnSweetClickListener
    {
        public event EventHandler OnConfirmClick;

        public void OnClick(SweetAlertDialog p0)
        {
            OnConfirmClick.Invoke(this, new EventArgs());
            p0
                .SetTitleText("Success")
                .SetContentText("Trip Started successfully!")
                .SetConfirmText("OK")
                .SetConfirmClickListener(null)
                .ChangeAlertType(SweetAlertDialog.SuccessType);
        }
    }
}