using Android.Graphics;
using Android.OS;
using Android.Views;
using Google.Android.Material.Button;
using Refractored.Controls;
using System;

namespace Cab360Driver.Fragments
{
    public class PicDisplayFragment : AndroidX.Fragment.App.DialogFragment
    {
        private Bitmap _bitmap;
        private int _rotation;
        public event EventHandler RetakePic;
        public event EventHandler SavePic;

        public PicDisplayFragment(Bitmap bitmap, int rotation)
        {
            _bitmap = bitmap;
            _rotation = rotation;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetStyle(StyleNormal, Resource.Style.AppTheme_DialogWhenLarge);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.pic_preview_layout, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            var profileImage = view.FindViewById<CircleImageView>(Resource.Id.preview_iv);

            var retakeBtn = view.FindViewById<MaterialButton>(Resource.Id.prev_retake_btn);
            var saveBtn = view.FindViewById<MaterialButton>(Resource.Id.prev_save_btn);
            retakeBtn.Click += RetakeBtn_Click;
            saveBtn.Click += SaveBtn_Click;

            if (_bitmap != null)
            {
                profileImage.SetImageBitmap(_bitmap);
                profileImage.Rotation = -_rotation;
            }
            else
            {
                return;
            }   

        }

        private void RetakeBtn_Click(object sender, EventArgs e)
        {
            RetakePic.Invoke(this, new EventArgs());
            _bitmap = null;
            Dismiss();
        }

        private void SaveBtn_Click(object sender, EventArgs e)
        {
            SavePic.Invoke(this, new EventArgs());
            Dismiss();
        }
    }
}