using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX;
using AndroidX.AppCompat.Widget;
using Cab360Driver.DataModels;
using Google.Android.Material.Button;
using Google.Android.Material.TextField;
using Java.Lang;
using static Android.Views.View;

namespace Cab360Driver.Fragments
{
    public class CarRegFragment : AndroidX.Fragment.App.Fragment, ITextWatcher, IOnKeyListener
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

        public event EventHandler<CarModelArgs> OnCardetailsSaved;
        public class CarModelArgs : EventArgs
        {
            public CarModel CarDetails { get; set; }
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.partner_vehicle_fragment, container, false);
            InitControls(view);
            return view;
        }

        private void InitControls(View view)
        {
            CarBrandEt = view.FindViewById<AppCompatAutoCompleteTextView>(Resource.Id.brand_ac_tv);
            CarBrandEt.AddTextChangedListener(this);
            CarBrandEt.SetOnKeyListener(this);

            CarModelEt = view.FindViewById<AppCompatAutoCompleteTextView>(Resource.Id.model_ac_tv);
            CarModelEt.AddTextChangedListener(this);
            CarModelEt.SetOnKeyListener(this);

            CarYearEt = view.FindViewById<AppCompatAutoCompleteTextView>(Resource.Id.year_ac_tv);
            CarYearEt.AddTextChangedListener(this);
            CarYearEt.SetOnKeyListener(this);

            CarColorEt = view.FindViewById<AppCompatAutoCompleteTextView>(Resource.Id.color_ac_tv);
            CarColorEt.AddTextChangedListener(this);
            CarColorEt.SetOnKeyListener(this);

            ConditionEt = view.FindViewById<AppCompatAutoCompleteTextView>(Resource.Id.state_ac_tv);
            ConditionEt.AddTextChangedListener(this);
            ConditionEt.SetOnKeyListener(this);

            RegNoEt = view.FindViewById<TextInputLayout>(Resource.Id.reg_no_til);
            RegNoEt.EditText.AddTextChangedListener(this);
            RegNoEt.EditText.SetOnKeyListener(this);

            CurrOwnEt = view.FindViewById<TextInputLayout>(Resource.Id.curr_user_til);
            CurrOwnEt.EditText.AddTextChangedListener(this);
            CurrOwnEt.EditText.SetOnKeyListener(this);

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

            ContinueBtn.Enabled = brand.Length >= 3 && model.Length >= 2 && color.Length >= 2 && year.Length == 4 && condition.Length >= 2 && currUser.Length >= 3 && regNo.Length >= 5;

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
    }
}