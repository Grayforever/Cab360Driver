using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.Annotations;

namespace Cab360Driver.Utils
{
    public class TextSwitcherUtil : Java.Lang.Object, ViewSwitcher.IViewFactory
    {
        int styleId;
        bool center;
        Context context;

        public TextSwitcherUtil([StyleRes] int styleId, bool center, Context context)
        {
            this.styleId = styleId;
            this.center = center;
            this.context = context;
        }

        public View MakeView()
        {
            TextView textView = new TextView(context);

            if (center)
            {
                textView.Gravity = GravityFlags.Center;
            }

            textView.SetTextAppearance(styleId);

            return textView;
        }
    }
}