using Android.App;
using Android.Content;
using Android.Util;
using Cab360Driver.Helpers;

namespace Cab360Driver.EventListeners
{
    public sealed class EarningsEventLister
    {
        private readonly ISharedPreferences preferences = Application.Context.GetSharedPreferences("earningsInfo", FileCreationMode.MultiProcess);
        private ISharedPreferencesEditor editor;
        private string totEarnings { get; set; }
        public void Create()
        {
            editor = preferences.Edit();
            var currentUser = AppDataHelper.GetCurrentUser();
            if(currentUser != null)
            {
                var driverRef = AppDataHelper.GetDatabase().GetReference($"Drivers/{currentUser.Uid}/MyEarnings");
                driverRef.AddValueEventListener(new SingleValueListener(r=> 
                {
                    if (!r.Exists())
                        return;

                    totEarnings = r.Child("totEarnings") != null ? r.Child("totEarnings").Value.ToString() : "";
                    SaveToSharedPreference();
                }, e=> 
                {
                    Log.Debug("database_error", e.Message);
                }));
            }
            else
            {
                return;
            }
        }

        private void SaveToSharedPreference()
        {
            editor.PutString("totEarnings", totEarnings);
            editor.Apply();
        }
    }
}