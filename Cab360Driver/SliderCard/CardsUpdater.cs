﻿using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.CardView.Widget;
using Ramotion.CardSliderLib;
using System;

namespace Cab360Driver.SliderCard
{
    [Register("id.Cab360Driver.SliderCard.CardsUpdater")]
    public sealed class CardsUpdater : DefaultViewUpdater
    {
        public override void UpdateView(View view, float position)
        {
            base.UpdateView(view, position);
            CardView card = (CardView)view;
            View alphaView = card.GetChildAt(1);
            View imageView = card.GetChildAt(0);

            if (position < 0)
            {
                var alpha = view.Alpha;
                view.Alpha = 1f;
                alphaView.Alpha = 0.9f - alpha;
                imageView.Alpha = 0.3f + alpha;
            }
            else
            {
                alphaView.Alpha = 0f;
                imageView.Alpha = 1f;
            }

            if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
            {
                var lm = LayoutManager;
                var ratio = (float)lm.GetDecoratedLeft(view) / lm.ActiveCardLeft;

                var z = position switch
                {
                    _ when position < 0 => ZCenter1 * ratio,
                    _ when position < 0.5f => ZCenter1,
                    _ when position < 1f => ZCenter2,
                    _ => ZRight
                };

                card.CardElevation = Math.Max(0, z);
            }
        }
    }
}