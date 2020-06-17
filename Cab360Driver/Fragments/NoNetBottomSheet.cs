using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using Google.Android.Material.BottomSheet;
using Google.Android.Material.Button;
using Java.IO;
using Java.Lang;
using System.Threading.Tasks;
using Process = Java.Lang.Process;

namespace Cab360Driver.Fragments
{
    public class NoNetBottomSheet : BottomSheetDialogFragment
    {
        private TextView noNetHeader;
        private TextView noNetSub;
        private ProgressBar progress;
        private MaterialButton btnRetry;
        private MaterialButton btnOpenSettings;
        private Runtime runtime;
        private Process mIpAddrProcess;

        private int retryText1 = Resource.String.no_net_retry1; 
        private int headerText1 = Resource.String.no_net_header1;
        private int subTitle1 = Resource.String.no_net_sub1;

        private int retryText2 = Resource.String.no_net_retry2;
        private int headerText2 = Resource.String.no_net_header2;
        private int subTitle2 = Resource.String.no_net_sub2;
        private FragmentActivity _context;

        public NoNetBottomSheet(FragmentActivity context)
        {
            _context = context;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetStyle(StyleNormal, Resource.Style.AppTheme_ModalDialog);
            runtime = Runtime.GetRuntime();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.no_net_btmsht, container, false);
            return view; 
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            noNetHeader = view.FindViewById<TextView>(Resource.Id.no_net_hdr);
            noNetSub = view.FindViewById<TextView>(Resource.Id.no_net_sbttl);
            progress = view.FindViewById<ProgressBar>(Resource.Id.no_net_prgs);
            btnRetry = view.FindViewById<MaterialButton>(Resource.Id.no_net_btn);
            btnOpenSettings = view.FindViewById<MaterialButton>(Resource.Id.no_net_btn_opn_stn);
            btnRetry.Click += (s1, e1) =>
            {
                SetLoading(true);
            };

            btnOpenSettings.Click += (s2, e2) =>
            {
                SetLoading(false);
            };
        }

        private async void SetLoading(bool val)
        {
            if (val)
            {
                await PingGoogleServer();
            }
            else
            {
                Intent intentOpenSettings = new Intent(Settings.ActionDataRoamingSettings);
                _context.StartActivityForResult(intentOpenSettings, 0);

                progress.Visibility = ViewStates.Invisible;
                btnRetry.Enabled = true;
                btnRetry.SetText(retryText1);
                noNetHeader.SetText(headerText1);
                noNetSub.SetText(subTitle1);
            }
        }

        private async Task<bool> PingGoogleServer()
        {
            progress.Visibility = ViewStates.Visible;
            btnRetry.Enabled = false;
            btnRetry.SetText(retryText2);
            noNetHeader.SetText(headerText2);
            noNetSub.SetText(subTitle2);
            try
            {
                mIpAddrProcess = runtime.Exec("/system/bin/ping -c 1 8.8.8.8");
                int mExitValue = await mIpAddrProcess.WaitForAsync();
                if (mExitValue == 0)
                {
                    Dismiss();
                    return true;
                }
                else
                {
                    progress.Visibility = ViewStates.Invisible;
                    btnRetry.Enabled = true;
                    btnRetry.SetText(retryText1);
                    noNetHeader.SetText(headerText1);
                    noNetSub.SetText(subTitle1);
                    return false;
                }
            }
            catch (InterruptedException)
            {
            }
            catch (IOException)
            {
               
            }
            return false;
        }
    }
}