using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;

namespace Cab360Driver.Utils
{
    public static class StatusBarUtil
    {
        public static int IS_SET_PADDING_KEY = 54648632;
        private static int TRANSLUCENT_VIEW_ID = Resource.Id.translucent_view;

        public static void SetTranslucentForImageViewInFragment(AppCompatActivity activity, int alpha)
        {
            SetTranslucentStatusBar(activity, null, alpha);
        }
        
        public static void SetTranslucentStatusBar(AppCompatActivity activity, View topView, int alpha)
        {
            SetARGBStatusBar(activity, topView, 0, 0, 0, alpha);
        }

        public static void SetARGBStatusBar(AppCompatActivity activity, View topView, int r, int g, int b, int alpha)
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.Kitkat)
            {
                return;
            }
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                activity.Window.DecorView.SystemUiVisibility = (StatusBarVisibility)SystemUiFlags.LayoutFullscreen;
                activity.Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                activity.Window.ClearFlags(WindowManagerFlags.TranslucentStatus);
                activity.Window.SetStatusBarColor(Color.Argb(alpha, r, g, b));
            }
            else if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
            {
                activity.Window.AddFlags(WindowManagerFlags.TranslucentStatus);
                SetARGBStatusViewToAct(activity, r, g, b, alpha);
            }
            if (topView != null)
            {
                bool isSetPadding = topView.GetTag(IS_SET_PADDING_KEY) != null;
                if (!isSetPadding)
                {
                    topView.SetPadding(topView.PaddingLeft, topView.PaddingTop + GetStatusBarHeight(activity), topView.PaddingRight, topView.PaddingBottom);
                    topView.SetTag(IS_SET_PADDING_KEY, true);
                }
            }
        }

        private static int GetStatusBarHeight(AppCompatActivity activity)
        {
            int resourceId = activity.Resources.GetIdentifier("status_bar_height", "dimen", "android");
            return activity.Resources.GetDimensionPixelSize(resourceId);
        }

        private static void SetARGBStatusViewToAct(AppCompatActivity activity, int r, int g, int b, int statusBarAlpha)
        {

            ViewGroup contentView = (ViewGroup)activity.FindViewById(Android.Resource.Id.Content);
            View fakeStatusBarView = contentView.FindViewById(TRANSLUCENT_VIEW_ID);
            if (fakeStatusBarView != null)
            {
                if (fakeStatusBarView.Visibility == ViewStates.Gone)
                {
                    fakeStatusBarView.Visibility = ViewStates.Visible;
                }
                fakeStatusBarView.SetBackgroundColor(Color.Argb(statusBarAlpha, r, g, b));
            }
            else
            {
                contentView.AddView(CreateARGBStatusBarView(activity, r, g, b, statusBarAlpha));
            }
        }

        private static View CreateARGBStatusBarView(AppCompatActivity activity, int r, int g, int b, int alpha)
        {
            // 绘制一个和状态栏一样高的矩形
            View statusBarView = new View(activity);

            LinearLayout.LayoutParams @params = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, GetStatusBarHeight(activity));
            statusBarView.LayoutParameters = @params;
            statusBarView.SetBackgroundColor(Color.Argb(alpha, r, g, b));
            statusBarView.Id = TRANSLUCENT_VIEW_ID;
            return statusBarView;
        }
    }
}
