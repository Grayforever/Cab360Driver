using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Util;
using AndroidX.Annotations;

namespace Cab360Driver.Utils
{
    [Register("id.Cab360Driver.Utils.ProfileHeaderBitmap")]
    public class ProfileHeaderBitmap : ForegroundImageView
    {
        private int lastAlpha;
        private Drawable mask;


        public ProfileHeaderBitmap(Context context, IAttributeSet attrs): base(context, attrs)
        {

        }

        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            base.OnSizeChanged(w, h, oldw, oldh);
            if (this.mask != null)
            {
                this.mask.SetBounds(0, 0, w, h);
            }
        }

        public override void JumpDrawablesToCurrentState()
        {
            base.JumpDrawablesToCurrentState();
            if (this.mask != null)
            {
                this.mask.JumpToCurrentState();
            }
        }

        protected override void DrawableStateChanged()
        {
            base.DrawableStateChanged();
            if (this.mask != null && this.mask.IsStateful)
            {
                this.mask.SetState(GetDrawableState());
            }
        }

        public void SetMask(Drawable drawable)
        {
            if (this.mask != drawable)
            {
                if (this.mask != null)
                {
                    this.mask.SetCallback(null);
                    UnscheduleDrawable(this.mask);
                }
                this.mask = drawable;
                if (this.mask != null)
                {
                    this.mask.SetBounds(0, 0, Width, Height);
                    SetWillNotDraw(false);
                    this.mask.SetCallback(this);
                    if (this.mask.IsStateful)
                    {
                        this.mask.SetState(GetDrawableState());
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
            if (this.mask != null)
            {
                this.mask.Draw(canvas);
            }
        }

        public void SetForegroundAlpha([FloatRange(From = 0.0d, To = 1.0d)] float f)
        {
            if (this.foreground != null)
            {
                int i = (int)(20.0f * f);
                if (i != this.lastAlpha || this.lastAlpha == 0)
                {
                    this.lastAlpha = i;
                    this.foreground.SetAlpha((int)(f * 255.0f));
                }
            }
        }
    }
}