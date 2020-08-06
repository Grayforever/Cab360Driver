using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Cab360Driver.Activities;
using Cab360Driver.EventListeners;
using Cab360Driver.Helpers;
using CN.Pedant.SweetAlert;
using Google.Android.Material.Button;

namespace Cab360Driver.Fragments
{
    public class SettingsFragment : AndroidX.Fragment.App.DialogFragment
    {
        ISharedPreferences preferences = Application.Context.GetSharedPreferences("userInfo", FileCreationMode.Private);
        ISharedPreferencesEditor editor;
        private SweetAlertDialog sweetAlert;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetStyle(StyleNormal, Resource.Style.AppTheme_DialogWhenLarge);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.settings_pref, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            var logoutBtn = view.FindViewById<MaterialButton>(Resource.Id.logout_btn);
            logoutBtn.Click += (s1, e1) =>
              {
                  if(AppDataHelper.GetCurrentUser() != null)
                  {
                      ShowDialog();
                  }
              };
        }


        public void ShowDialog()
        {
            sweetAlert = new SweetAlertDialog(Context, SweetAlertDialog.WarningType);
            sweetAlert.SetContentText("Your session will be dismissed and all unsaved data lost. Continue?");
            sweetAlert.SetTitleText("Log out?");
            sweetAlert.SetConfirmText("Yes");
            sweetAlert.SetCancelText("No");
            sweetAlert.SetConfirmClickListener(new SweetConfirmClick(alert => 
            {
                editor = preferences.Edit();
                editor.PutString("firstRun", "reg");
                editor.Commit();
                AppDataHelper.GetFirebaseAuth().SignOut();

                var intent = new Intent(Context, typeof(SplashActivity));
                StartActivity(intent);
                alert.DismissWithAnimation();
            }));
            sweetAlert.Show();
        }
    }
}