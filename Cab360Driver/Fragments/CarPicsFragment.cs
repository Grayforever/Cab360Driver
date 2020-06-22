using Android.OS;
using Android.Views;
using Android.Widget;
using Cab360Driver.EnumsConstants;
using Cab360Driver.Helpers;
using Firebase.Auth;
using Firebase.Database;
using Google.Android.Material.Button;
using System;

namespace Cab360Driver.Fragments
{
    public class CarPicsFragment : AndroidX.Fragment.App.Fragment
    {
        public event EventHandler CarCaptureComplete;
        private FirebaseDatabase fireDb;
        private FirebaseUser fireUser;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            fireDb = AppDataHelper.GetDatabase();
            fireUser = AppDataHelper.GetCurrentUser();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.partner_vehi_pics_fragment, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            var btnMain = view.FindViewById< MaterialButton>(Resource.Id.vehi_pics_cnt_btn);
            var frontCard = view.FindViewById(Resource.Id.vehi_capt_c1);
            var backCard = view.FindViewById(Resource.Id.vehi_capt_c2);
            frontCard.Click += (s1, e1) =>
            {
                CheckAndStartCamera(CaptureType.FrontSide);
            };
            backCard.Click += (s2, e2) =>
            {
                CheckAndStartCamera(CaptureType.BackSide);
            };
            btnMain.Click += BtnMain_Click;
        }

        private void CheckAndStartCamera(CaptureType captureType)
        {

        }

        private void BtnMain_Click(object sender, EventArgs e)
        {
            if(fireUser != null)
            {
                var dbRef = fireDb.GetReference("Drivers/" + fireUser.Uid).Child("stage_of_registration");
                dbRef.SetValue(RegistrationStage.Agreement.ToString())
                    .AddOnSuccessListener(new OnSuccessListener(r => { CarCaptureComplete?.Invoke(this, new EventArgs()); }))
                    .AddOnFailureListener(new OnFailureListener(e => { Toast.MakeText(Activity, e.Message, ToastLength.Short).Show(); }));
            }
            
        }
    }
}