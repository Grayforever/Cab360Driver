using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Views;
using Google.Android.Material.BottomSheet;
using Google.Android.Material.Button;

namespace Cab360Driver.Fragments
{
    public class NoLocationBtmSht : BottomSheetDialogFragment
    {
        private MaterialButton enableBtn;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.no_net_btmsht, container, false);
            GetControls(view);
            return view;
        }

        private void GetControls(View view)
        {
            enableBtn = view.FindViewById<MaterialButton>(Resource.Id.no_loc_btn);
            enableBtn.Click += (s1, e1) =>
            {
                StartActivityForResult(new Intent(Settings.ActionLocationSourceSettings), 0);
            };
        }
    }
}