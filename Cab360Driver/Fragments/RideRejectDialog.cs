using Android.OS;
using Android.Views;
using Android.Widget;
using Cab360Driver.EventListeners;
using CN.Pedant.SweetAlert;
using Google.Android.Material.TextField;
using System;
using static Android.Widget.RadioGroup;
using static CN.Pedant.SweetAlert.SweetAlertDialog;


namespace Cab360Driver.Fragments
{
    public class RideRejectDialog : AndroidX.Fragment.App.DialogFragment
    {
        private int[] rejectReasons = { Resource.String.text1, Resource.String.text2, Resource.String.text3, Resource.String.text4 };
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetStyle(StyleNormal, Resource.Style.AppTheme_DialogWhenLarge);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.ride_reject, container, false);
            return view;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            var radioGroup = view.FindViewById<RadioGroup>(Resource.Id.reject_rgrp);
            radioGroup.SetOnCheckedChangeListener(new RadioCheckedListener(id => 
            {
                switch (id)
                {
                    case Resource.Id.reject_r1:
                        SetRejectRationale(rejectReasons[0]);
                        break;

                    case Resource.Id.reject_r2:
                        SetRejectRationale(rejectReasons[1]);
                        break;

                    case Resource.Id.reject_r3:
                        SetRejectRationale(rejectReasons[2]);
                        break;

                    case Resource.Id.reject_r4:
                        SetRejectRationale(rejectReasons[3]);
                        break;

                    default:
                        break;
                }
            }));
            var otherReasonEt = view.FindViewById<TextInputLayout>(Resource.Id.reject_othr_et);
            otherReasonEt.EditText.EditorAction += (s1, e1) =>
            {
                if (string.IsNullOrEmpty(otherReasonEt.EditText.Text))
                {
                    Toast.MakeText(Activity, "Please say something", ToastLength.Short).Show();
                }
                else
                {
                    ShowAlert();
                }
            };
        }

        private void ShowAlert()
        {
            var alert = new SweetAlertDialog(Activity, SweetAlertDialog.WarningType);
            alert.SetTitleText("Feedback confirmation");
            alert.SetContentText("Do you wish to send this feedback?");
            alert.SetConfirmText("Yes");
            alert.SetCancelText("No");
            alert.SetCancelable(false);
            alert.SetCanceledOnTouchOutside(false);
            alert.SetConfirmClickListener(new SweetConfirmClick(swAlert =>
            {
                swAlert.SetTitleText("Success");
                swAlert.SetContentText("Your feedback has been submitted successfully!");
                swAlert.SetConfirmText("OK");
                swAlert.ShowCancelButton(false);
                swAlert.SetConfirmClickListener(new SweetConfirmClick(swAlert2 =>
                {
                    swAlert2.Dismiss();
                    base.Dismiss();
                }));
                swAlert.ChangeAlertType(SweetAlertDialog.SuccessType);
                swAlert.Show();

            }));
            alert.Show();
        }

        private void SetRejectRationale(int rationale)
        {
            ShowAlert();
            //var dataRef = AppDataHelper.GetDatabase().GetReference("DriverRideRejections/" + AppDataHelper.GetCurrentUser().Uid);
            //dataRef.SetValueAsync(GetString(rationale));
        }
    }

    public sealed class RadioCheckedListener : Java.Lang.Object, IOnCheckedChangeListener
    {
        private readonly Action<int> _onCheckedChanged;

        public RadioCheckedListener(Action<int> onCheckedChanged)
        {
            _onCheckedChanged = onCheckedChanged;
        }
        public void OnCheckedChanged(RadioGroup group, int checkedId)
        {
            _onCheckedChanged?.Invoke(checkedId);
        }
    }
}