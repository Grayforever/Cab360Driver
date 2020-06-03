using Android.OS;
using Android.Views;
using AndroidX.CardView.Widget;
using AndroidX.Core.View;
using Ramotion.CardSliderLib;
using System;

namespace Cab360Driver.SliderCard
{
    public class CardsUpdater : DefaultViewUpdater
    {
        //[Obsolete]
        public override void UpdateView(View view, float position)
        {
            base.UpdateView(view, position);
            CardView card = ((CardView)view);
            View alphaView = card.GetChildAt(1);
            View imageView = card.GetChildAt(0);

            if (position < 0)
            {
                float alpha = ViewCompat.GetAlpha(view);
                ViewCompat.SetAlpha(view, 1f);
                ViewCompat.SetAlpha(alphaView, 0.9f - alpha);
                ViewCompat.SetAlpha(imageView, 0.3f + alpha);
            }
            else
            {
                ViewCompat.SetAlpha(alphaView, 0f);
                ViewCompat.SetAlpha(imageView, 1f);
            }

            if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
            {
                 CardSliderLayoutManager lm = LayoutManager;
                 float ratio = (float)lm.GetDecoratedLeft(view) / lm.ActiveCardLeft;

                 float z;

                if (position < 0)
                {
                    z = ZCenter1 * ratio;
                }
                else if (position < 0.5f)
                {
                    z = ZCenter1;
                }
                else if (position < 1f)
                {
                    z = ZCenter2;
                }
                else
                {
                    z = ZRight;
                }

                card.CardElevation = Math.Max(0, z);
            }
        }
    }
}