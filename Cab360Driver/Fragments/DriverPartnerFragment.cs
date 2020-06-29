using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Cab360Driver.Activities;
using Cab360Driver.EnumsConstants;
using Cab360Driver.Helpers;
using Google.Android.Material.Button;
using System;
using static Android.Widget.ViewSwitcher;
using static Google.Android.Material.Button.MaterialButtonToggleGroup;

namespace Cab360Driver.Fragments
{
    public class DriverPartnerFragment : AndroidX.Fragment.App.Fragment, IViewFactory
    {
        private bool partner = true;
        private int[] anim = { Resource.Animation.slide_in_top, Resource.Animation.slide_out_bottom };

        public event EventHandler<PartnerEventArgs> PartnerTypeComplete;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.driver_partner_layout, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            var AccountTypeTGroup = view.FindViewById<MaterialButtonToggleGroup>(Resource.Id.drv_part_tggl_grp);
            var InfoImgSwitcher = view.FindViewById<ImageSwitcher>(Resource.Id.drv_part_img_swtchr1);
            var InfoTSwitcher = view.FindViewById<TextSwitcher>(Resource.Id.drv_part_txt_swtchr1);
            var ContinueBtn = view.FindViewById<MaterialButton>(Resource.Id.drv_part_cnt_btn);

            InitSwitchers(InfoImgSwitcher, InfoTSwitcher);

            AccountTypeTGroup.AddOnButtonCheckedListener(new OnButtonChecked(id =>
            {
                switch (id)
                {
                    case Resource.Id.drv_tggl_partner_btn:
                        partner = true;
                        InfoTSwitcher.SetText(GetString(Resource.String.partner_desc_txt));
                        InfoImgSwitcher.SetImageResource(Resource.Drawable.driver_pablo);
                        break;

                    case Resource.Id.drv_tggl_driver_btn:
                        partner = false;
                        InfoTSwitcher.SetText(GetString(Resource.String.driving_desc_txt));
                        InfoImgSwitcher.SetImageResource(Resource.Drawable.driver_pablo);
                        break;
                }
            }));

            
            ContinueBtn.Click += (s1, e1) =>
            {
                if (AppDataHelper.GetCurrentUser() != null)
                {
                    OnboardingActivity.ShowProgressDialog();
                    var driverRef = AppDataHelper.GetDatabase().GetReference($"Drivers/{AppDataHelper.GetCurrentUser().Uid}");
                    driverRef.Child("isPartner").SetValue(partner.ToString())
                        .AddOnSuccessListener(new OnSuccessListener(r1 =>
                        {
                            driverRef.Child(StringConstants.StageofRegistration).SetValue(RegistrationStage.Capturing.ToString())
                               .AddOnSuccessListener(new OnSuccessListener(r2 =>
                               {
                                   PartnerTypeComplete.Invoke(this, new PartnerEventArgs { IsPartnerComplete = true });
                                   OnboardingActivity.CloseProgressDialog();
                               }))
                               .AddOnFailureListener(new OnFailureListener(e1 => { OnboardingActivity.CloseProgressDialog(); }));
                        }))
                        .AddOnFailureListener(new OnFailureListener(e1 => { OnboardingActivity.CloseProgressDialog(); }));
                }
                else
                {
                    Toast.MakeText(Activity, "Something aint right", ToastLength.Short).Show();
                }
            };
        }

        private void InitSwitchers(ImageSwitcher imageSwitcher, TextSwitcher textSwitcher)
        {
            imageSwitcher.SetFactory(new ViewFactory());
            imageSwitcher.SetInAnimation(Activity, anim[0]);
            imageSwitcher.SetOutAnimation(Activity, anim[1]);
            imageSwitcher.SetImageResource(Resource.Drawable.driver_pablo);

            textSwitcher.SetFactory(this);
            textSwitcher.SetInAnimation(Activity, anim[0]);
            textSwitcher.SetOutAnimation(Activity, anim[1]);
            textSwitcher.SetCurrentText(GetString(Resource.String.partner_desc_txt));
        }

        public View MakeView()
        {
            var textInfos = new TextView(Activity);
            textInfos.Gravity = GravityFlags.Center | GravityFlags.CenterVertical;
            textInfos.SetTextAppearance(Resource.Style.TextAppearance_MaterialComponents_Body1);
            return textInfos;
        }

        public class PartnerEventArgs : EventArgs
        {
            public bool IsPartnerComplete { get; set; }
        }

        internal sealed class ViewFactory : Java.Lang.Object, IViewFactory
        {

            public View MakeView()
            {
                var image = new ImageView(Application.Context);
                image.SetScaleType(ImageView.ScaleType.CenterCrop);
                return image;
            }
        }

        internal sealed class OnButtonChecked : Java.Lang.Object, IOnButtonCheckedListener
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
}