using Android.OS;
using Android.Views;
using Google.Android.Material.BottomSheet;
using Google.Android.Material.Button;
using System;

namespace Cab360Driver.Fragments
{
    public class EmailSentFragment : BottomSheetDialogFragment
    {
        public event EventHandler onResendClick;
        public event EventHandler onOkClick;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.email_sent_btmsht, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            var resendBtn = view.FindViewById<MaterialButton>(Resource.Id.email_sent_btn_resend);
            var okBtn = view.FindViewById<MaterialButton>(Resource.Id.email_sent_btn_ok);

            resendBtn.Click += ResendBtn_Click; okBtn.Click += OkBtn_Click;
        }

        private void OkBtn_Click(object sender, EventArgs e)
        {
            onOkClick?.Invoke(this, new EventArgs());
            DismissAllowingStateLoss();
        }

        private void ResendBtn_Click(object sender, EventArgs e)
        {
            onResendClick?.Invoke(this, new EventArgs());
            DismissAllowingStateLoss();
        }
    }
}