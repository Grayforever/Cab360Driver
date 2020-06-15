using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Util;
using Android.Views;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.Core.View;
using AndroidX.Interpolator.View.Animation;
using Cab360Driver.EventListeners;
using Google.Android.Material.AppBar;
using Java.Lang.Reflect;
using System;

namespace Cab360Driver.Utils
{
    public class ProfileBtnBehavior : CoordinatorLayout.Behavior
    {
        int _appBarLayoutMinHeight = -1;
        private Context _context;
        private bool _mIsHided;
        Rect _rect = new Rect();

        public ProfileBtnBehavior(Context context, IAttributeSet attrs): base(context, attrs)
        {
            _context = context;
        }

        public override bool LayoutDependsOn(CoordinatorLayout parent, Java.Lang.Object child, View dependency)
        {
            var view = child as View;
            ViewCompat.SetElevation(view, 5.0f);
            return dependency is AppBarLayout;
        }

        public override bool OnDependentViewChanged(CoordinatorLayout parent, Java.Lang.Object child, View dependency)
        {
            return dependency is AppBarLayout ? UpdateVisibility(parent, (AppBarLayout)dependency, child) : false;
            //return false;
        }

        private bool UpdateVisibility(CoordinatorLayout parent, AppBarLayout dependency, Java.Lang.Object child)
        {
            var view = child as View;
            if (((CoordinatorLayout.LayoutParams)view.LayoutParameters).AnchorId != dependency.Id)
            {
                return false;
            }
            view.GetGlobalVisibleRect(_rect);
            int centerY = _rect.CenterY();
            if(Build.Model.Contains("MI 8") || Build.Model.Contains("Nokia X6"))
            {
                centerY += 24;
            }
            else if(GetStatesBarHeight(dependency.Context) > 100)
            {
                centerY += GetStatesBarHeight(dependency.Context) / 2;
            }

            if(_appBarLayoutMinHeight == -1)
            {
                Method method = ReflectUtils.GetMethod(dependency.Class, "getMinimumHeightForVisibleOverlappingContent");
                try
                {
                    _appBarLayoutMinHeight = (int)ReflectUtils.InvokeMethod(method, dependency.Class);
                }
                catch (Exception e)
                {
                    Log.Debug("error", e.Message);
                }
                int[] iArr = new int[2];
                parent.GetLocationOnScreen(iArr);
                _appBarLayoutMinHeight += iArr[1];
            }
            if (centerY <= _appBarLayoutMinHeight)
            {
                if (!_mIsHided)
                {
                    Hide(view);
                }
            }
            else if (_mIsHided)
            {
                Show(view);
            }
            return true;
        }

        private int GetStatesBarHeight(Context context)
        {
            int identifier = context.Resources.GetIdentifier("status_bar_height", "dimen", "android");
            return identifier > 0 ? context.Resources.GetDimensionPixelSize(identifier) : 0;
        }

        private void Show(View view)
        {
            _mIsHided = false;
            view.Visibility = ViewStates.Visible;
            view.Animate()
                .ScaleX(1.0f)
                .ScaleY(1.0f)
                .Alpha(1.0f)
                .SetDuration(200)
                .SetInterpolator(new FastOutLinearInInterpolator())
                .SetListener(new AnimatorListener(null, null, null, null))
                .Start();
        }

        public void Hide(View view)
        {
            _mIsHided = true;
            view.Animate().Cancel();
            view.Animate()
                .ScaleX(0.0f)
                .ScaleY(0.0f)
                .Alpha(0.0f)
                .SetDuration(200)
                .SetInterpolator(new FastOutLinearInInterpolator())
                .SetListener(new AnimatorListener(animEnd =>
                {
                if (_mIsHided)
                {
                        view.Visibility = ViewStates.Invisible;
                }
                }, null, null, null)).Start();
        }
    }
}