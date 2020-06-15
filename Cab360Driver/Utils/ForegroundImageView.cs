using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Util;
using Android.Views;
using AndroidX.AppCompat.Widget;
using R = Cab360Driver.Resource;

namespace Cab360Driver.Utils
{
    public class ForegroundImageView : AppCompatImageView
    {
        protected Drawable foreground;

        public override bool HasOverlappingRendering 
        {
            get
            {
                return false;
            }
        }

        public ForegroundImageView(Context context, IAttributeSet attributeSet) : base(context, attributeSet)
        {
            
            TypedArray obtainStyledAttributes = context.ObtainStyledAttributes(attributeSet, R.Styleable.ForegroundView);
            Drawable drawable = obtainStyledAttributes.GetDrawable(R.Styleable.ForegroundView_drawable);
            if (drawable != null)
            {
                SetForeground(drawable);
            }
            obtainStyledAttributes.Recycle();
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                OutlineProvider = ViewOutlineProvider.Bounds;
            }
        }

        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            base.OnSizeChanged(w, h, oldw, oldh);
            if (foreground != null)
            {
                foreground.SetBounds(0, 0, w, h);
            }
        }

        protected override bool VerifyDrawable(Drawable who)
        {
            return base.VerifyDrawable(who) || who == foreground;

        }

        public override void JumpDrawablesToCurrentState()
        {
            base.JumpDrawablesToCurrentState();
            if (this.foreground != null)
            {
                this.foreground.JumpToCurrentState();
            }
        }

        protected override void DrawableStateChanged()
        {
            base.DrawableStateChanged();
            if (this.foreground != null && this.foreground.IsStateful)
            {
                this.foreground.SetState(GetDrawableState());
            }
        }

        public Drawable GetForeground()
        {
            return foreground;
        }

        public void SetForeground(Drawable drawable)
        {
            if (this.foreground != drawable)
            {
                if (this.foreground != null)
                {
                    this.foreground.SetCallback(null);
                    UnscheduleDrawable(this.foreground);
                }
                this.foreground = drawable;
                if (this.foreground != null)
                {
                    this.foreground.SetBounds(0, 0, Width, Height);
                    SetWillNotDraw(false);
                    this.foreground.SetCallback(this);
                    if (this.foreground.IsStateful)
                    {
                        this.foreground.SetState(GetDrawableState());
                    }
                }
                else
                {
                    SetWillNotDraw(true);
                }
                Invalidate();
            }
        }

        public override void Draw(Canvas canvas)
        {
            base.Draw(canvas);
            if (this.foreground != null)
            {
                this.foreground.Draw(canvas);
            }
        }

        public override void DrawableHotspotChanged(float x, float y)
        {
            base.DrawableHotspotChanged(x, y);
            if (this.foreground != null && Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                this.foreground.SetHotspot(x, y);
            }
        }
    }
}