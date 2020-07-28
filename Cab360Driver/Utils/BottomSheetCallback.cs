using Android.Views;
using Google.Android.Material.BottomSheet;

namespace Cab360Driver.Utils
{
    public class BottomSheetCallback : BottomSheetBehavior.BottomSheetCallback
    {
        public override void OnSlide(View bottomSheet, float newState)
        {

        }

        public override void OnStateChanged(View p0, int p1)
        {
            switch (p1)
            {
                case BottomSheetBehavior.StateHidden:
                    break;

                case BottomSheetBehavior.StateExpanded:
                    break;

                case BottomSheetBehavior.StateCollapsed:
                    break;

                case BottomSheetBehavior.StateSettling:
                    break;
            }
        }
    }
}