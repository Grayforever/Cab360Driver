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
using AndroidX.ViewPager2.Widget;

namespace Cab360Driver.Utils
{
    public class CrossfadeTransformer : Java.Lang.Object, ViewPager2.IPageTransformer
    {
        public void TransformPage(View p0, float p1)
        {
            if(p1 <= -1.0f || p1 >= 1.0f)
            {
                p0.TranslationX = p0.Width * p1;
                p0.Alpha = 0.0f;
            }
            else if(p1 == 0.0f)
            {
                p0.TranslationX = p0.Width * p1;
                p0.Alpha = 1.0f;
            }
            else
            {
                p0.TranslationX = p0.Width * -p1;
                p0.Alpha = 1.0f * Math.Abs(p1);
            }
        }
    }
}