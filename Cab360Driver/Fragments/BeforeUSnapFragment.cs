using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace Cab360Driver.Fragments
{
    public class BeforeUSnapFragment : Android.Support.V4.App.DialogFragment
    {
        public event EventHandler StartCameraAsync;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetStyle(StyleNormal, Resource.Style.Theme_AppCompat_Light_DialogWhenLarge);
            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            

            return inflater.Inflate(Resource.Layout.pic_intro_layout, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            var btn_take = view.FindViewById<Button>(Resource.Id.intro_takephoto_btn);
            btn_take.Click += Btn_take_Click;
        }

        private void Btn_take_Click(object sender, EventArgs e)
        {
            StartCameraAsync.Invoke(this, new EventArgs());
            Dismiss();
        }
    }
}