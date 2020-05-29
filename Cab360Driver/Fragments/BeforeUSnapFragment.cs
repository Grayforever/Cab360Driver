using Android.OS;
using Android.Views;
using Google.Android.Material.Button;
using System;

namespace Cab360Driver.Fragments
{
    public class BeforeUSnapFragment : AndroidX.Fragment.App.DialogFragment
    {
        public event EventHandler<CamArgs> StartCameraAsync;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetStyle(StyleNormal, Resource.Style.AppTheme_DialogWhenLarge);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.pic_intro_layout, container, false);
            var btn_take = view.FindViewById<MaterialButton>(Resource.Id.intro_takephoto_btn);
            btn_take.Click += Btn_take_Click;
            return view;
        }

        private void Btn_take_Click(object sender, EventArgs e)
        {
            StartCameraAsync.Invoke(this, new CamArgs { Tag = Tag});
            Dismiss();
        }

        public class CamArgs : EventArgs
        {
            public string Tag { get; set; }
        }
    }
}