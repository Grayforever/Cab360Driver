using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Cab360Driver.EnumsConstants;
using Google.Android.Material.Button;
using System;
using static Android.Widget.ViewSwitcher;
using static Google.Android.Material.Button.MaterialButtonToggleGroup;

namespace Cab360Driver.Fragments
{
    public class DriverPartnerFragment : AndroidX.Fragment.App.Fragment, IOnButtonCheckedListener, IViewFactory
    {

        private PartnershipEnum partnershipEnum;

        private MaterialButton ContinueBtn;
        TextSwitcher InfoTSwitcher;
        MaterialButtonToggleGroup AccountTypeTGroup;
        ImageSwitcher InfoImgSwitcher;
        MakeViewClass makeView;

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
            var view =  inflater.Inflate(Resource.Layout.driver_partner_layout, container, false);
            partnershipEnum = PartnershipEnum.IsPartner;
            GetControls(view);
            return view;
        }

        private void GetControls(View view)
        {
            ContinueBtn = view.FindViewById<MaterialButton>(Resource.Id.drv_part_cnt_btn);
            ContinueBtn.Click += ContinueBtn_Click;
            InfoTSwitcher = view.FindViewById<TextSwitcher>(Resource.Id.drv_part_txt_swtchr1);
            InitSwitcher();
            InfoImgSwitcher = view.FindViewById<ImageSwitcher>(Resource.Id.drv_part_img_swtchr1);
            InitImgSwitcher();

            AccountTypeTGroup = view.FindViewById<MaterialButtonToggleGroup>(Resource.Id.drv_part_tggl_grp);
            AccountTypeTGroup.AddOnButtonCheckedListener(this);
        }

        private void ContinueBtn_Click(object sender, EventArgs e)
        {
            StageTwoPassEvent.Invoke(this, new StageTwoEventArgs { IsPartner = (int)partnershipEnum });
        }

        private void InitImgSwitcher()
        {
            makeView = new MakeViewClass(this.Activity);
            InfoImgSwitcher.SetFactory(makeView);
            InfoImgSwitcher.SetInAnimation(Activity, Resource.Animation.slide_in_right);
            InfoImgSwitcher.SetOutAnimation(Activity, Resource.Animation.slide_out_left);
            InfoImgSwitcher.SetImageResource(Resource.Drawable.ic_driver1);
        }

        public class MakeViewClass : Java.Lang.Object, IViewFactory
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

        public void OnButtonChecked(MaterialButtonToggleGroup p0, int p1, bool p2)
        {
            switch (p1)
            {
                case Resource.Id.drv_tggl_partner_btn:
                    partnershipEnum = PartnershipEnum.IsPartner;
                    InfoTSwitcher.SetText(GetString(Resource.String.partner_desc_txt));
                    InfoImgSwitcher.SetImageResource(Resource.Drawable.ic_driver1);
                    break;

                case Resource.Id.drv_tggl_driver_btn:
                    partnershipEnum = PartnershipEnum.IsDriver;
                    InfoTSwitcher.SetText(GetString(Resource.String.driving_desc_txt));
                    InfoImgSwitcher.SetImageResource(Resource.Drawable.ic_driver2);
                    break;

                default:
                    return;
                    
            }
        }
    }
}