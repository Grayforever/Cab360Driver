using Android.OS;
using Android.Views;
using Android.Widget;
using Google.Android.Material.BottomSheet;
using Google.Android.Material.Button;

namespace Cab360Driver.Fragments
{
    public class NoNetBottomSheet : BottomSheetDialogFragment
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetStyle(StyleNormal, Resource.Style.AppTheme_ModalDialog);
            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.no_net_btmsht, container, false);
            return view; 
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            var noNetIcon = view.FindViewById<ImageView>(Resource.Id.no_net_img);
            var noNetHeader = view.FindViewById<TextView>(Resource.Id.no_net_hdr);
            var noNetSub = view.FindViewById<TextView>(Resource.Id.no_net_sbttl);
            var progress = view.FindViewById<ProgressBar>(Resource.Id.no_net_prgs);
            var btnRetry = view.FindViewById<MaterialButton>(Resource.Id.no_net_btn);
            var btnOpenSettings = view.FindViewById<MaterialButton>(Resource.Id.no_net_btn_opn_stn);
            btnRetry.Click += (s1, e1) =>
            {
                if(progress.Visibility == ViewStates.Invisible)
                {
                    //noNetIcon.Visibility = ViewStates.Invisible
                    Activity.RunOnUiThread(() =>
                    {
                        SetLoading(noNetHeader, noNetSub, progress, btnRetry);
                    });
                    
                }
            };

            btnOpenSettings.Click += (s2, e2) =>
            {
                if (progress.Visibility == ViewStates.Visible)
                {
                    //noNetIcon.Visibility = ViewStates.Invisible
                    Activity.RunOnUiThread(() =>
                    {
                        ResetLoading(noNetHeader, noNetSub, progress, btnRetry);
                    });

                }
            };
        }

        private static void ResetLoading(TextView noNetHeader, TextView noNetSub, ProgressBar progress, MaterialButton btnRetry)
        {
            progress.Visibility = ViewStates.Invisible;
            btnRetry.SetText(Resource.String.no_net_retry1);
            btnRetry.Enabled = true;
            noNetHeader.SetText(Resource.String.no_net_header1);
            noNetSub.SetText(Resource.String.no_net_sub1);
        }

        private static void SetLoading(TextView noNetHeader, TextView noNetSub, ProgressBar progress, MaterialButton btnRetry)
        {
            progress.Visibility = ViewStates.Visible;
            btnRetry.SetText(Resource.String.no_net_retry2);
            btnRetry.Enabled = false;
            noNetHeader.SetText(Resource.String.no_net_header2);
            noNetSub.SetText(Resource.String.no_net_sub2);
        }
    }
}