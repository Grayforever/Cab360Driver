using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Cab360Driver.Activities;
using Cab360Driver.EnumsConstants;
using Cab360Driver.Helpers;
using Google.Android.Material.Button;

namespace Cab360Driver.Fragments
{
    public class AckFragment : AndroidX.Fragment.App.Fragment
    {
        ISharedPreferences preferences = Application.Context.GetSharedPreferences("userInfo", FileCreationMode.Private);
        ISharedPreferencesEditor editor;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.driver_ack_layout, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            InitControls(view);
        }

        private void InitControls(View view)
        {
            var visit_btn = view.FindViewById<MaterialButton>(Resource.Id.visit_site_btn);
            var skip_btn = view.FindViewById<MaterialButton>(Resource.Id.skip_btn);
            skip_btn.Click += (s1, e1) =>
            {
                if (AppDataHelper.GetCurrentUser().Uid == null)
                {
                    return;
                }
                else
                {
                    OnboardingActivity.ShowProgressDialog();
                    var dataRef = AppDataHelper.GetDatabase().GetReference($"Drivers/{AppDataHelper.GetCurrentUser().Uid}/{StringConstants.StageofRegistration}");
                    dataRef.SetValue(RegistrationStage.RegistrationDone.ToString())
                    .AddOnSuccessListener(new OnSuccessListener(r =>
                    {
                        editor = preferences.Edit();
                        editor.PutString("firstRun", "regd");
                        editor.Commit();

                        OnboardingActivity.CloseProgressDialog();
                        var intent = new Intent(Context, typeof(MainActivity));
                        intent.SetFlags(ActivityFlags.ClearTask | ActivityFlags.ClearTop | ActivityFlags.NewTask);
                        StartActivity(intent);
                    })).AddOnFailureListener(new OnFailureListener(e => 
                    {
                        OnboardingActivity.CloseProgressDialog();
                        OnboardingActivity.ShowErrorDialog(e.Message);
                    }));
                }
                
            };

            visit_btn.Click += (s2, e2) =>
            {
                string url = "https://www.google.com";
                Intent i = new Intent(Intent.ActionView);
                i.SetData(Android.Net.Uri.Parse(url));
                StartActivity(i);
            };
        }
    }
}