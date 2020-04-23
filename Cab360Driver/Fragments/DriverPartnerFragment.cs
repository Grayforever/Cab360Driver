using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.CardView.Widget;
using Cab360Driver.EnumsConstants;
using Google.Android.Material.Button;
using System;
using static Android.Views.View;

namespace Cab360Driver.Fragments
{
    public class DriverPartnerFragment : AndroidX.Fragment.App.Fragment
    {

        private PartnershipEnum partnershipEnum;

        private MaterialButton ContinueBtn;

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

        }

        private void GetControls(View view)
        {
            
        }
    }
}