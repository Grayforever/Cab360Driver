using Android.App;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Cab360Driver.EnumsConstants;
using System;
using static Android.Views.View;

namespace Cab360Driver.Fragments
{
    public class DriverPartnerFragment : Android.Support.V4.App.Fragment, IOnFocusChangeListener
    {
        private CardView CardPartner, CardDriver;
        private TextView PartnerHeaderText, PartnerDetTxt, DriverHeaderTxt, DriverDetTxt;

        private PartnershipEnum partnershipEnum;

        private Button ContinueBtn;

        public class StageTwoEventArgs : EventArgs
        {
            public int IsPartner { get; set; }
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public event EventHandler<StageTwoEventArgs> StageTwoPassEvent;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.driver_partner_layout, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            GetControls(view);

            CardPartner.RequestFocus();
            ChangeColorOnFocus(CardPartner, PartnerHeaderText, PartnerDetTxt);
            partnershipEnum = PartnershipEnum.IsPartner;
        }

        private void GetControls(View view)
        {
            CardPartner = view.FindViewById<CardView>(Resource.Id.drv_part_c1);
            CardPartner.OnFocusChangeListener = this;

            CardDriver = view.FindViewById<CardView>(Resource.Id.drv_part_c2);
            CardDriver.OnFocusChangeListener = this; 

            DriverDetTxt = view.FindViewById<TextView>(Resource.Id.drv_drvdetails_txt);
            DriverHeaderTxt = view.FindViewById<TextView>(Resource.Id.drv_drv_txt);

            PartnerHeaderText = view.FindViewById<TextView>(Resource.Id.drv_vehi_txt);
            PartnerDetTxt = view.FindViewById<TextView>(Resource.Id.drv_vehidetails_txt);

            ContinueBtn = view.FindViewById<Button>(Resource.Id.drv_part_cnt_btn);
            ContinueBtn.Click += ContinueBtn_Click;
        }

        public void OnFocusChange(View v, bool hasFocus)
        {
            var view_id = v.Id;

            switch (view_id)
            {
                case Resource.Id.drv_part_c1:
                    if (hasFocus == true)
                    {
                        ChangeColorOnFocus(CardPartner, PartnerHeaderText, PartnerDetTxt);
                        partnershipEnum = PartnershipEnum.IsPartner;
                    }
                    else
                    {
                        ChangeColorToDefault(CardPartner, PartnerHeaderText, PartnerDetTxt);
                    }

                    break;

                case Resource.Id.drv_part_c2:
                    if (hasFocus == true)
                    {
                        ChangeColorOnFocus(CardDriver, DriverHeaderTxt, DriverDetTxt);
                        partnershipEnum = PartnershipEnum.IsDriver;
                    }
                    else
                    {
                        ChangeColorToDefault(CardDriver, DriverHeaderTxt, DriverDetTxt);
                    }
                    break;

                default:
                    partnershipEnum = PartnershipEnum.Default;
                    break;
            }
        }

        private void ContinueBtn_Click(object sender, EventArgs e)
        {
            StageTwoPassEvent?.Invoke(this, new StageTwoEventArgs { IsPartner = (int)partnershipEnum });
        }

        private void ChangeColorOnFocus(CardView card, TextView t1, TextView t2)
        {
            card.CardBackgroundColor = ColorStateList.ValueOf(Color.Blue);
            t1.SetTextColor(Color.White);
            t2.SetTextColor(Color.White);
        }

        private void ChangeColorToDefault(CardView card, TextView t1, TextView t2)
        {
            card.CardBackgroundColor = ColorStateList.ValueOf(Color.White);
            t1.SetTextColor(Color.Black);
            t2.SetTextColor(Color.Black);
        }
    }
}