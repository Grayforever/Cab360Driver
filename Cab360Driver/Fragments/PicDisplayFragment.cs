using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using Google.Android.Material.Button;
using Refractored.Controls;
using System;

namespace Cab360Driver.Fragments
{
    public class PicDisplayFragment : AndroidX.Fragment.App.DialogFragment
    {
        public static Bitmap _bitmap;
        public event EventHandler RetakePic;
        public event EventHandler<HasImageEventArgs> SavePic;
        private ImageButton backBtn;

        public class HasImageEventArgs : EventArgs
        {
            public bool viewHasImage { get; set; }
            public byte[] ImageArray { get; set; }
        }
        private bool hasImage = false;

        public PicDisplayFragment(Bitmap bitmap)
        {
            _bitmap = bitmap;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetStyle(StyleNormal, Resource.Style.Theme_AppCompat_Light_DialogWhenLarge);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.pic_preview_layout, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            var profileImage = view.FindViewById<CircleImageView>(Resource.Id.preview_iv);

            backBtn = view.FindViewById<ImageButton>(Resource.Id.back_btn);
            backBtn.Click += BackBtn_Click;

            var retakeBtn = view.FindViewById<MaterialButton>(Resource.Id.prev_retake_btn);
            var saveBtn = view.FindViewById<MaterialButton>(Resource.Id.prev_save_btn);
            retakeBtn.Click += RetakeBtn_Click;
            saveBtn.Click += SaveBtn_Click;

            if (_bitmap != null)
            {
                profileImage.SetImageBitmap(_bitmap);
                hasImage = true;
            }
            else
            {
                hasImage = false;
            }   

        }

        private void BackBtn_Click(object sender, EventArgs e)
        {
            RetakePic = null;
            SavePic = null;
            Dismiss();
        }

        private void RetakeBtn_Click(object sender, EventArgs e)
        {
            RetakePic.Invoke(this, new EventArgs());
            Dismiss();
        }

        private void SaveBtn_Click(object sender, EventArgs e)
        {
            SavePic.Invoke(this, new HasImageEventArgs { viewHasImage = hasImage});
            Dismiss();
        }
    }
}