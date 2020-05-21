using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Views;
using Android.Widget;
using AndroidX.Annotations;
using AndroidX.AppCompat.Widget;
using Cab360Driver.Adapters;
using Cab360Driver.DataModels;
using Cab360Driver.EnumsConstants;
using Cab360Driver.IServices;
using Google.Android.Material.Button;
using Google.Android.Material.TextField;
using Java.Lang;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Android.Views.View;
using static Android.Widget.AutoCompleteTextView;

namespace Cab360Driver.Fragments
{
    public class CarRegFragment : BaseFragment, ITextWatcher, IOnKeyListener
    {
        private AppCompatAutoCompleteTextView CarBrandEt;
        private AppCompatAutoCompleteTextView CarModelEt;
        private AppCompatAutoCompleteTextView CarYearEt;
        private AppCompatAutoCompleteTextView CarColorEt;
        private AppCompatAutoCompleteTextView ConditionEt;
        private TextInputLayout RegNoEt;
        private TextInputLayout CurrOwnEt;
        private MaterialButton ContinueBtn;

        private string brand;
        private string model;
        private string color;
        private string year;
        private string condition;
        private string currUser;
        private string regNo;
        private ICarsApi CarsApi;

        public event EventHandler<CarModelArgs> OnCardetailsSaved;
        public class CarModelArgs : EventArgs
        {
            public CarModel CarDetails { get; set; }
        }

        public override View ProvideYourFragmentView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.partner_vehicle_fragment, container, false);
            InitControls(view);
            return view;
        }

        public override BaseFragment ProvideYourfragment()
        {
            return new CarRegFragment();
        }

        public async override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            //getting years from 1992 to current
            List<int> Years = new List<int>();

            for (int i = DateTime.UtcNow.Year; i >= 1992; i--)
                Years.Add(i);

            var _years = Years.Select(i => i.ToString()).ToArray();
            var yr_arrayAdapter = ArrayAdapterClass.CreateArrayAdapter(Activity, _years);
            CarYearEt.Adapter = yr_arrayAdapter;
            CarYearEt.AddTextChangedListener(this);
            CarYearEt.SetOnKeyListener(this);
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
                    var model2 = model.Distinct().ToList();
                    var modelAdapter = ArrayAdapterClass.CreateArrayAdapter(Activity, model2);
                    CarModelEt.Adapter = modelAdapter;
                }
                   
            }
            catch (System.Exception)
            {

                throw;
            }

            CarBrandEt.AddTextChangedListener(this);
            CarBrandEt.SetOnKeyListener(this);
            CarBrandEt.Validator = new Validator(CarBrandEt);

            //will be set by volly httprequest 
            CarModelEt.AddTextChangedListener(this);
            CarModelEt.SetOnKeyListener(this);
            CarModelEt.Validator = new Validator(CarModelEt);

            //all known colors
            var LColors = new List<string>();
            foreach(PropertyInfo property in typeof(Color).GetProperties())
            {
                if (property.PropertyType == typeof(Color))
                    LColors.Add(property.Name);
            }
            var coloradapter = ArrayAdapterClass.CreateArrayAdapter(Activity, LColors);
            CarColorEt.Adapter = coloradapter;
            CarColorEt.AddTextChangedListener(this);
            CarColorEt.SetOnKeyListener(this);
            CarColorEt.Validator = new Validator(CarColorEt);

            //car condition
            var conditions = new string[] { "Slightly Used", "Used", "New" };
            var conditionsAdapter = ArrayAdapterClass.CreateArrayAdapter(Activity, conditions);
            ConditionEt.Adapter = conditionsAdapter;
            ConditionEt.AddTextChangedListener(this);
            ConditionEt.SetOnKeyListener(this);
            ConditionEt.Validator = new Validator(ConditionEt);

            //write custom regex for car number
            RegNoEt.EditText.AddTextChangedListener(this);
            RegNoEt.EditText.SetOnKeyListener(this);

            //nameText
            CurrOwnEt.EditText.AddTextChangedListener(this);
            CurrOwnEt.EditText.SetOnKeyListener(this);
            
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
            var carModel = new CarModel
            {
                Brand = brand,
                Model = model,
                Year = year,
                Color = color,
                Condition = condition,
                CurrUser = currUser,
                RegNo = regNo

            };

            SendToActivity(carModel);
        }

        private void SendToActivity(CarModel carModel)
        {
            OnCardetailsSaved.Invoke(this, new CarModelArgs { CarDetails = carModel });
        }

        public void AfterTextChanged(IEditable s)
        {
            CheckIfEmpty();
        }

        private void CheckIfEmpty()
        {
            brand = CarBrandEt.Text;
            model = CarModelEt.Text;
            color = CarColorEt.Text;
            year = CarYearEt.Text;
            condition = ConditionEt.Text;
            currUser = CurrOwnEt.EditText.Text;
            regNo = RegNoEt.EditText.Text;

            ContinueBtn.Enabled = brand.Length >= 3 && model.Length >= 2 && color.Length >= 3 && 
                year.Length == 4 && condition.Length >= 3 && currUser.Length >= 3 && regNo.Length >= 5;

        }

        public void BeforeTextChanged(ICharSequence s, int start, int count, int after)
        {
            
        }

        public void OnTextChanged(ICharSequence s, int start, int before, int count)
        {
            
        }

        public bool OnKey(View v, [GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
            var action = e.Action;
            if (action == KeyEventActions.Up)
            {
                CheckIfEmpty();
            }
            return false;
        }

        public class Validator : Java.Lang.Object, IValidator
        {
            private AppCompatAutoCompleteTextView _edittext;

            public Validator(AppCompatAutoCompleteTextView edittext)
            {
                _edittext = edittext;
            }

            [Nullable]
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
}