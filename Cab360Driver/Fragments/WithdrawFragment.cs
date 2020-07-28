using Android.OS;
using Android.Views;
using Android.Widget;
using Google.Android.Material.Button;

namespace Cab360Driver.Fragments
{
    public class WithdrawFragment : AndroidX.Fragment.App.DialogFragment
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetStyle(StyleNormal, Resource.Style.AppTheme_DialogWhenLarge);
            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.withdraw_dialog, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            var withdrawAmt = view.FindViewById<TextView>(Resource.Id.with_bal_txt);
            var balanceText = view.FindViewById<TextView>(Resource.Id.with_hdr3);
            var withdrawBtn = view.FindViewById<MaterialButton>(Resource.Id.with_yes_btn);

            withdrawBtn.Click += (s1, e1) =>
            {

            };
        }

    }
}