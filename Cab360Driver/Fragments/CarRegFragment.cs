using Android.OS;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using Cab360Driver.Activities;
using Cab360Driver.Adapters;
using Cab360Driver.EnumsConstants;
using Cab360Driver.Helpers;
using Cab360Driver.IServices;
using Cab360Driver.Utils;
using Google.Android.Material.Button;
using Google.Android.Material.TextField;
using Java.Util;
using Refit;
using System;
using System.Linq;

namespace Cab360Driver.Fragments
{
    public class CarRegFragment : AndroidX.Fragment.App.Fragment
    {
        private AppCompatAutoCompleteTextView CarBrandEt;
        private AppCompatAutoCompleteTextView CarModelEt;
        private AppCompatAutoCompleteTextView CarYearEt;
        private AppCompatAutoCompleteTextView CarColorEt;
        private AppCompatAutoCompleteTextView ConditionEt;
        private TextInputLayout RegNoEt;
        private TextInputLayout DriverLicenceEt;
        private MaterialButton ContinueBtn;
        private ICarsApi CarsApi;
        public event EventHandler CarRegComplete;
        private string[] conditions = new string[] { "Slightly Used", "Used", "New" };
        private ArrayAdapter brandAdapter;
        private ArrayAdapter modelAdapter;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.partner_vehicle_fragment, container, false);
            CarBrandEt = view.FindViewById<AppCompatAutoCompleteTextView>(Resource.Id.brand_ac_tv);
            CarModelEt = view.FindViewById<AppCompatAutoCompleteTextView>(Resource.Id.model_ac_tv);
            CarYearEt = view.FindViewById<AppCompatAutoCompleteTextView>(Resource.Id.year_ac_tv);
            CarColorEt = view.FindViewById<AppCompatAutoCompleteTextView>(Resource.Id.color_ac_tv);
            ConditionEt = view.FindViewById<AppCompatAutoCompleteTextView>(Resource.Id.state_ac_tv);
            RegNoEt = view.FindViewById<TextInputLayout>(Resource.Id.reg_no_til);
            DriverLicenceEt = view.FindViewById<TextInputLayout>(Resource.Id.curr_user_til);
            ContinueBtn = view.FindViewById<MaterialButton>(Resource.Id.part_cont_btn);
            return view;
        }

        public async override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ContinueBtn.Click += ContinueBtn_Click;

            CarsApi = RestService.For<ICarsApi>(StringConstants.GetGateway());
            try
            {
                var MyResult = await CarsApi.GetWelcome();

                var make = MyResult.Results.Select(car => car.Make).ToList();
                brandAdapter = ArrayAdapterClass.CreateArrayAdapter(Activity, make);

                var model = MyResult.Results.Select(car => car.Model).ToList();
                modelAdapter = ArrayAdapterClass.CreateArrayAdapter(Activity, model);
            }
            catch (Exception e)
            {
                Log.Error("car_reg_fragment", e.Message);
            }

            CarYearEt.Adapter = ArrayAdapterClass.CreateArrayAdapter(Activity, CarPropsUtil.GetModelYears());
            CarYearEt.TextChanged += TextChanged;
            CarYearEt.Validator = new Validator(CarYearEt);

            CarBrandEt.Adapter = brandAdapter;
            CarBrandEt.TextChanged += TextChanged;

            CarModelEt.Adapter = modelAdapter;
            CarModelEt.TextChanged += TextChanged;

            CarColorEt.Adapter = ArrayAdapterClass.CreateArrayAdapter(Activity, CarPropsUtil.GetColorList());
            CarColorEt.TextChanged += TextChanged;
            CarColorEt.Validator = new Validator(CarColorEt);

            ConditionEt.Adapter = ArrayAdapterClass.CreateArrayAdapter(Activity, conditions);
            ConditionEt.TextChanged += TextChanged;
            ConditionEt.Validator = new Validator(ConditionEt);

            RegNoEt.EditText.TextChanged += TextChanged;
            DriverLicenceEt.EditText.TextChanged += TextChanged;  
        }

        private void TextChanged(object sender, TextChangedEventArgs e)=> CheckIfEmpty();

        private void ContinueBtn_Click(object sender, EventArgs e)
        {
            OnboardingActivity.ShowProgressDialog();
            HashMap carMap = new HashMap();
            carMap.Put("car_model", CarModelEt.Text);
            carMap.Put("car_brand", CarBrandEt.Text);
            carMap.Put("car_year", CarYearEt.Text);
            carMap.Put("car_color", CarColorEt.Text);
            carMap.Put("car_condition", ConditionEt.Text);
            carMap.Put("licence_no", DriverLicenceEt.EditText.Text);
            carMap.Put("reg_no", RegNoEt.EditText.Text);

            SaveCarDetailsToDb(carMap);
        }

        private void SaveCarDetailsToDb(HashMap carMap)
        {
            if(AppDataHelper.GetCurrentUser() != null)
            {
                var vehicleRef = AppDataHelper.GetDatabase().GetReference($"Drivers/{AppDataHelper.GetCurrentUser().Uid}/MyCars");
                vehicleRef.SetValue(carMap)
                    .AddOnSuccessListener(new OnSuccessListener(result =>
                    {
                        var driverRef = AppDataHelper.GetDatabase().GetReference($"Drivers/{AppDataHelper.GetCurrentUser().Uid}/{StringConstants.StageofRegistration}");
                        driverRef.SetValue($"{RegistrationStage.Agreement}")
                            .AddOnSuccessListener(new OnSuccessListener(result2 =>
                            {
                                CarRegComplete.Invoke(this, new EventArgs());
                                OnboardingActivity.CloseProgressDialog();
                            })).AddOnFailureListener(new OnFailureListener(e2 =>
                            {
                                OnboardingActivity.CloseProgressDialog();
                                Toast.MakeText(Activity, e2.Message, ToastLength.Short).Show();
                            }));

                    }))
                    .AddOnFailureListener(new OnFailureListener(e1 =>
                    {
                        OnboardingActivity.CloseProgressDialog();
                        Toast.MakeText(Activity, e1.Message, ToastLength.Short).Show();
                    }));
            }
            else
            {
                return;
            }
        }

        public void AfterTextChanged(IEditable s)
        {
            CheckIfEmpty();
        }

        private void CheckIfEmpty()
        {
            ContinueBtn.Enabled = CarBrandEt.Text.Length >= 3 
                && CarModelEt.Text.Length >= 2 
                && CarColorEt.Text.Length >= 3 
                && CarYearEt.Text.Length == 4 
                && ConditionEt.Text.Length >= 3 
                && DriverLicenceEt.EditText.Text.Length >= 3 
                && StringConstants.IsCarNumMatch(RegNoEt.EditText.Text);
        } 
    }
}