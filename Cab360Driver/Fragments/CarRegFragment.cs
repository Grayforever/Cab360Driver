using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using Cab360Driver.Adapters;
using Cab360Driver.EnumsConstants;
using Cab360Driver.Helpers;
using Cab360Driver.IServices;
using Firebase.Database;
using Google.Android.Material.Button;
using Google.Android.Material.TextField;
using Java.Lang;
using Java.Util;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using static Android.Widget.AutoCompleteTextView;

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
        private TextInputLayout CurrOwnEt;
        private MaterialButton ContinueBtn;
        private ICarsApi CarsApi;
        public event EventHandler CarRegComplete;
        private FirebaseDatabase FireDatabase;
        private string[] conditions = new string[] { "Slightly Used", "Used", "New" };
        private List<string> LColors;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            FireDatabase = AppDataHelper.GetDatabase();
            LColors = new List<string>();
            foreach (PropertyInfo property in typeof(Color).GetProperties())
            {
                if (property.PropertyType == typeof(Color))
                    LColors.Add(property.Name);
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.partner_vehicle_fragment, container, false);
            InitControls(view);
            return view;
        }

        public async override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            List<int> Years = new List<int>();

            for (int i = DateTime.UtcNow.Year; i >= 1992; i--)
                Years.Add(i);

            var _years = Years.Select(i => i.ToString()).ToArray();
            var yr_arrayAdapter = ArrayAdapterClass.CreateArrayAdapter(Activity, _years);
            CarYearEt.Adapter = yr_arrayAdapter;
            CarYearEt.TextChanged += TextChanged;
            CarYearEt.Validator = new Validator(CarYearEt);

            CarsApi = RestService.For<ICarsApi>(StringConstants.GetGateway());
            try
            {
                var MyResult = await CarsApi.GetWelcome();
                var make = new List<string>();
                foreach (var car in MyResult.Results)
                {
                    make.Add(car.Make);

                    var brandAdapter = ArrayAdapterClass.CreateArrayAdapter(Activity, make);
                    CarBrandEt.Adapter = brandAdapter;
                }

                var model = new List<string>();
                foreach(var car in MyResult.Results)
                {
                    model.Add(car.Model);
                    var modelAdapter = ArrayAdapterClass.CreateArrayAdapter(Activity, model);
                    CarModelEt.Adapter = modelAdapter;
                }
                   
            }
            catch (System.Exception e)
            {

                Log.Error("car_reg_fragment", e.Message);
            }

            CarBrandEt.TextChanged += TextChanged;
            CarBrandEt.Validator = new Validator(CarBrandEt);

            //will be set by volly httprequest 
            CarModelEt.TextChanged += TextChanged;
            CarModelEt.Validator = new Validator(CarModelEt);

            var coloradapter = ArrayAdapterClass.CreateArrayAdapter(Activity, LColors);
            CarColorEt.Adapter = coloradapter;
            CarColorEt.TextChanged += TextChanged;
            CarColorEt.Validator = new Validator(CarColorEt);

            //car condition
            
            var conditionsAdapter = ArrayAdapterClass.CreateArrayAdapter(Activity, conditions);
            ConditionEt.Adapter = conditionsAdapter;
            ConditionEt.TextChanged += TextChanged;
            ConditionEt.Validator = new Validator(ConditionEt);

            RegNoEt.EditText.TextChanged += TextChanged;

            CurrOwnEt.EditText.TextChanged += TextChanged;
            
        }

        private void TextChanged(object sender, TextChangedEventArgs e)
        {


            CheckIfEmpty();
        }

        private void InitControls(View view)
        {
            CarBrandEt = view.FindViewById<AppCompatAutoCompleteTextView>(Resource.Id.brand_ac_tv);
            CarModelEt = view.FindViewById<AppCompatAutoCompleteTextView>(Resource.Id.model_ac_tv);
            CarYearEt = view.FindViewById<AppCompatAutoCompleteTextView>(Resource.Id.year_ac_tv);
            CarColorEt = view.FindViewById<AppCompatAutoCompleteTextView>(Resource.Id.color_ac_tv);
            ConditionEt = view.FindViewById<AppCompatAutoCompleteTextView>(Resource.Id.state_ac_tv);
            RegNoEt = view.FindViewById<TextInputLayout>(Resource.Id.reg_no_til);
            CurrOwnEt = view.FindViewById<TextInputLayout>(Resource.Id.curr_user_til);
            ContinueBtn = view.FindViewById<MaterialButton>(Resource.Id.part_cont_btn);
            ContinueBtn.Click += ContinueBtn_Click;
        }

        

        private void ContinueBtn_Click(object sender, EventArgs e)
        {
            HashMap carMap = new HashMap();
            carMap.Put("car_model", CarModelEt.Text);
            carMap.Put("car_brand", CarBrandEt.Text);
            carMap.Put("car_year", CarYearEt.Text);
            carMap.Put("car_color", CarColorEt.Text);
            carMap.Put("car_condition", ConditionEt.Text);
            carMap.Put("curr_user", CurrOwnEt.EditText.Text);
            carMap.Put("reg_no", RegNoEt.EditText.Text);

            SaveCarDetailsToDb(carMap);
        }

        private void SaveCarDetailsToDb(HashMap carMap)
        {
            var currUser = AppDataHelper.GetCurrentUser();
            if(currUser != null)
            {
                var vehicleRef = FireDatabase.GetReference("Drivers" + currUser.Uid).Child("MyCars");
                vehicleRef.SetValue(carMap)
                    .AddOnSuccessListener(new OnSuccessListener(result =>
                    {
                        var driverRef = AppDataHelper.GetParentReference().Child(currUser.Uid).Child("stage_of_registration");
                        driverRef.SetValue(RegistrationStage.CarCapturing.ToString())
                            .AddOnSuccessListener(new OnSuccessListener(result2 =>
                            {
                                CarRegComplete.Invoke(this, new EventArgs());
                            })).AddOnFailureListener(new OnFailureListener(e2 =>
                            {
                                Toast.MakeText(Activity, e2.Message, ToastLength.Short).Show();
                            }));

                    }))
                    .AddOnFailureListener(new OnFailureListener(e1 =>
                    {
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
            ContinueBtn.Enabled = CarBrandEt.Text.Length >= 3 && CarModelEt.Text.Length >= 2 && CarColorEt.Text.Length >= 3 &&
                CarYearEt.Text.Length == 4 && ConditionEt.Text.Length >= 3 && CurrOwnEt.EditText.Text.Length >= 3 && RegNoEt.EditText.Text.Length >= 5;
        } 
    }

    public sealed class Validator : Java.Lang.Object, IValidator
    {
        private AppCompatAutoCompleteTextView _edittext;

        public Validator(AppCompatAutoCompleteTextView edittext)
        {
            _edittext = edittext;
        }

        public ICharSequence FixTextFormatted(ICharSequence invalidText)
        {
            return null;
        }

        public bool IsValid(ICharSequence text)
        {
            var listAdapter = _edittext.Adapter;
            for (int i = 0; i < listAdapter.Count; i++)
            {
                string temp = listAdapter.GetItem(i).ToString();
                if (text.ToString().CompareTo(temp) != 0)
                    continue;
                return true;
            }
            return false;
        }
    }
}