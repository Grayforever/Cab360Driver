using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Cab360Driver.EnumsConstants;
using Cab360Driver.Helpers;
using Firebase.Database;
using Google.Android.Material.Button;
using System;
using static Android.Widget.ViewSwitcher;
using static Google.Android.Material.Button.MaterialButtonToggleGroup;

namespace Cab360Driver.Fragments
{
    public class DriverPartnerFragment : AndroidX.Fragment.App.Fragment, IViewFactory
    {
        private MaterialButton ContinueBtn;
        private TextSwitcher InfoTSwitcher;
        private MaterialButtonToggleGroup AccountTypeTGroup;
        private ImageSwitcher InfoImgSwitcher;
        private MakeViewClass makeView;
        private DatabaseReference driverRef;
        private bool partner = true;

        public class PartnerEventArgs : EventArgs
        {
            public bool IsPartnerComplete { get; set; }
        }
        public event EventHandler<PartnerEventArgs> PartnerTypeComplete;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view =  inflater.Inflate(Resource.Layout.driver_partner_layout, container, false);
            GetControls(view);
            return view;
        }

        private void GetControls(View view)
        {
            InfoTSwitcher = view.FindViewById<TextSwitcher>(Resource.Id.drv_part_txt_swtchr1);
            InitSwitcher();
            InfoImgSwitcher = view.FindViewById<ImageSwitcher>(Resource.Id.drv_part_img_swtchr1);
            InitImgSwitcher();
            AccountTypeTGroup = view.FindViewById<MaterialButtonToggleGroup>(Resource.Id.drv_part_tggl_grp);
            AccountTypeTGroup.AddOnButtonCheckedListener(new OnButtonChecked(id=> 
            {
                switch (id)
                {
                    case Resource.Id.drv_tggl_partner_btn:
                        partner = true;
                        InfoTSwitcher.SetText(GetString(Resource.String.partner_desc_txt));
                        InfoImgSwitcher.SetImageResource(Resource.Drawable.ic_driver1);
                        break;

                    case Resource.Id.drv_tggl_driver_btn:
                        partner = false;
                        InfoTSwitcher.SetText(GetString(Resource.String.driving_desc_txt));
                        InfoImgSwitcher.SetImageResource(Resource.Drawable.ic_driver2);
                        break;
                }
            }));

            ContinueBtn = view.FindViewById<MaterialButton>(Resource.Id.drv_part_cnt_btn);
            ContinueBtn.Click += (s1, e1) =>
            {
                var currUser = AppDataHelper.GetCurrentUser();
                if(currUser != null)
                {
                    driverRef = AppDataHelper.GetParentReference().Child(currUser.Uid);
                    driverRef.Child("isPartner").SetValue(partner.ToString())
                        .AddOnSuccessListener(new OnSuccessListener(r1 =>
                        {
                            driverRef.Child("stage_of_registration").SetValue(RegistrationStage.Capturing.ToString())
                               .AddOnSuccessListener(new OnSuccessListener(r2 =>
                               {
                                   PartnerTypeComplete.Invoke(this, new PartnerEventArgs { IsPartnerComplete = true });
                               }))
                               .AddOnFailureListener(new OnFailureListener(e1 => { }));
                        }))
                        .AddOnFailureListener(new OnFailureListener(e1 => { }));
                }
                else
                {
                    return;
                }
                
            };
        }

        private void InitImgSwitcher()
        {
            makeView = new MakeViewClass(Activity);
            InfoImgSwitcher.SetFactory(makeView);
            InfoImgSwitcher.SetInAnimation(Activity, Resource.Animation.slide_in_right);
            InfoImgSwitcher.SetOutAnimation(Activity, Resource.Animation.slide_out_left);
            InfoImgSwitcher.SetImageResource(Resource.Drawable.ic_driver1);
        }

        private void InitSwitcher()
        {
            InfoTSwitcher.SetFactory(this);
            InfoTSwitcher.SetInAnimation(Activity, Resource.Animation.slide_in_right);
            InfoTSwitcher.SetOutAnimation(Activity, Resource.Animation.slide_out_left);
            InfoTSwitcher.SetCurrentText(GetString(Resource.String.partner_desc_txt));
        }

        public View MakeView()
        {
            var textInfos = new TextView(Activity);
            textInfos.Gravity = GravityFlags.Center | GravityFlags.CenterVertical;
            textInfos.SetTextAppearance(Resource.Style.TextAppearance_AppCompat_Body1);
            return textInfos;
        }
    }

    public sealed class MakeViewClass : Java.Lang.Object, IViewFactory
    {
        Context context;

        public MakeViewClass(Context _context)
        {
            context = _context;
        }

        public View MakeView()
        {
            var image = new ImageView(context);
            image.SetScaleType(ImageView.ScaleType.CenterCrop);
            return image;
        }
    }

    public sealed class OnButtonChecked : Java.Lang.Object, IOnButtonCheckedListener
    {
        private readonly Action<int> _onButtonChecked;

        public OnButtonChecked(Action<int> onButtonChecked)
        {
            _onButtonChecked = onButtonChecked;
        }

        void IOnButtonCheckedListener.OnButtonChecked(MaterialButtonToggleGroup p0, int p1, bool p2)
        {
            _onButtonChecked?.Invoke(p1);
        }
    }
}