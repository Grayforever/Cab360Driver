using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Views;
using AndroidX.Fragment.App;
using Google.Android.Material.BottomSheet;
using Google.Android.Material.Button;

namespace Cab360Driver.Fragments
{
    public class NoLocationBtmSht : BottomSheetDialogFragment
    {
        private MaterialButton enableBtn;
        private FragmentActivity _context;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }


        public NoLocationBtmSht(FragmentActivity context)
        {
            _context = context;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.no_loc_btmsht, container, false);
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