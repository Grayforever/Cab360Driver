using Android.OS;
using Android.Views;
using Android.Widget;
using Cab360Driver.Helpers;
using static Android.Widget.RadioGroup;

namespace Cab360Driver.Fragments
{
    public class AccountFragment : Android.Support.V4.App.Fragment, IOnCheckedChangeListener
    {
        private LinearLayout tab1;
        private LinearLayout tab2;
        private LinearLayout tab3;
        private RadioGroup tabIndexer;
       
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            var view = inflater.Inflate(Resource.Layout.account, container, false);
            return view;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            var DriverNameTv = view.FindViewById<TextView>(Resource.Id.username_tv);
            var DriverPhoneTv = view.FindViewById<TextView>(Resource.Id.userphone_tv);
            var DriverCityTv = view.FindViewById<TextView>(Resource.Id.usercity_tv);
            var DriverEmailTv = view.FindViewById<TextView>(Resource.Id.useremail_tv);

            SetValues(DriverNameTv, DriverPhoneTv, DriverCityTv, DriverEmailTv);

            tabIndexer = view.FindViewById<RadioGroup>(Resource.Id.radioGroup_account);
            tabIndexer.SetOnCheckedChangeListener(this);

            tab1 = view.FindViewById<LinearLayout>(Resource.Id.tab1);
            tab2 = view.FindViewById<LinearLayout>(Resource.Id.tab2);
            tab3 = view.FindViewById<LinearLayout>(Resource.Id.tab3);

            tab1.Visibility = ViewStates.Visible;
            tab2.Visibility = ViewStates.Gone;
            tab3.Visibility = ViewStates.Gone;
        }

        private void SetValues(TextView driverNameTv, TextView driverPhoneTv, TextView driverCityTv, TextView driverEmailTv)
        {
            driverNameTv.Text = $"{AppDataHelper.GetFirstName()} {AppDataHelper.GetLastName()}";
            driverPhoneTv.Text = AppDataHelper.GetPhone();
            driverEmailTv.Text = AppDataHelper.GetEmail();
            driverCityTv.Text = AppDataHelper.GetCity();
        }

        public void OnCheckedChanged(RadioGroup group, int checkedId)
        {
            switch (checkedId)
            {
                case Resource.Id.radioUser:
                    tab1.Visibility = ViewStates.Visible;
                    tab2.Visibility = ViewStates.Gone;
                    tab3.Visibility = ViewStates.Gone;
                    break;

                case Resource.Id.radioExp:
                    tab2.Visibility = ViewStates.Visible;
                    tab1.Visibility = ViewStates.Gone;
                    tab1.Visibility = ViewStates.Gone;
                    break;

                case Resource.Id.radioReview:
                    tab3.Visibility = ViewStates.Visible;
                    tab2.Visibility = ViewStates.Gone;
                    tab1.Visibility = ViewStates.Gone;
                    break;
            }
        }

        
    }
}