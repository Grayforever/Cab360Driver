using Android.App;
using Android.Content;
using Android.Gms.Tasks;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using Cab360Driver.Helpers;
using Firebase.Auth;
using Firebase.Database;
using Google.Android.Material.FloatingActionButton;
using Google.Android.Material.TextField;
using System;

namespace Cab360Driver.Fragments
{
    public class DriverSignInFragment : AndroidX.Fragment.App.Fragment, IOnSuccessListener, IOnFailureListener, IValueEventListener
    {
        private TextInputLayout EmailText, PassText;
        private FloatingActionButton ContinueFab;
        private FirebaseAuth FireAuth;
        private FirebaseDatabase FireDatabase;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            FireAuth = AppDataHelper.GetFirebaseAuth();
            FireDatabase = AppDataHelper.GetDatabase();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {

            var view = inflater.Inflate(Resource.Layout.driver_signin_layout, container, false);
            GetControls(view);
            return view;
        }

        private void GetControls(View view)
        {
            EmailText = view.FindViewById<TextInputLayout>(Resource.Id.drv_signin_email_et);

            PassText = view.FindViewById<TextInputLayout>(Resource.Id.drv_signin_pass_et);

            ContinueFab = view.FindViewById<FloatingActionButton>(Resource.Id.fab1);
            ContinueFab.Click += ContinueFab_Click;
        }

        private bool CheckIfEmpty()
        {
            var email = EmailText.EditText.Text;

            var pass = PassText.EditText.Text;

            bool enabled = Patterns.EmailAddress.Matcher(email).Matches() && pass.Length >= 8;
            return enabled;
        }

        private void ContinueFab_Click(object sender, EventArgs e)
        {
            CheckIfEmpty();
            if (CheckIfEmpty() == false)
                return;

            FireAuth.SignInWithEmailAndPassword(EmailText.EditText.Text, PassText.EditText.Text)
                .AddOnSuccessListener(this)
                .AddOnFailureListener(this);

        }

        public void OnSuccess(Java.Lang.Object result)
        {
            //var driverData = FireDatabase.GetReference("Cab360Drivers").Child(FireAuth.CurrentUser.Uid);
            //driverData.AddListenerForSingleValueEvent(this);

            var intent = new Intent(Application.Context, typeof(MainActivity));
            intent.SetFlags(ActivityFlags.ClearTask | ActivityFlags.ClearTop | ActivityFlags.NewTask);
            StartActivity(intent);
        }

        public void OnFailure(Java.Lang.Exception e)
        {
            Toast.MakeText(Application.Context, e.Message, ToastLength.Short).Show();
        }

        public void OnCancelled(DatabaseError error)
        {

        }

        public void OnDataChange(DataSnapshot snapshot)
        {
            //if (snapshot.Value != null)
            //{
            //    Driver driver = new Driver
            //    {
            //        Fname = snapshot?.Child("firstname").Value.ToString(),
            //        Email = snapshot?.Child("email").Value.ToString(),
            //        Lname = snapshot?.Child("lastname").Value.ToString(),
            //        City = snapshot?.Child("city").Value.ToString(),
            //        Phone = snapshot?.Child("phone").Value.ToString(),
            //        Code = snapshot?.Child("invite_code").Value.ToString(),
            //    };
            //}
        }
    }
}