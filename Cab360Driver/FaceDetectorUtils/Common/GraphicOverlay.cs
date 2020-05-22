using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Gms.Vision;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace Cab360Driver.FaceDetectorUtils.Common
{
    public class GraphicOverlay : View
    {
        private GraphicOverlay _overlay;
        private object _lock = new object();
        private int previewWidth;
        private float widthScaleFactor = 1.0f;
        private int previewHeight;
        private float heightScaleFactor = 1.0f;
        private CameraFacing facing = CameraFacing.Back;
        private List<Graphic> graphics = new List<Graphic>();


        public abstract class Graphic
        {
            private GraphicOverlay _overlay;
            public Graphic(GraphicOverlay overlay)
            {
                _overlay = overlay;
            }

            public abstract void Draw(Canvas canvas);

            public float GetScaleX(float horizontal)
            {
                return horizontal * _overlay.widthScaleFactor;
            }

            public float GetScaleY(float vertical)
            {
                return vertical * _overlay.heightScaleFactor;
            }

            public Context GetApplicationContext()
            {
                return _overlay.Context.ApplicationContext;
            }

            public float TranslateX(float x)
            {
                if (_overlay.facing == CameraFacing.Front)
                {
                    return _overlay.Width - GetScaleX(x);
                }
                else
                {
                    return GetScaleX(x);
                }
            }

            public float TranslateY(float y)
            {
                return GetScaleY(y);
            }

            public void PostInvalidate()
            {
                _overlay.PostInvalidate();
            }
        }



        public GraphicOverlay(Context context, IAttributeSet attrs) : base(context, attrs)
        {

        }

        public void Clear()
        {
            lock(_lock)
            {
                graphics.Clear();
            }
            PostInvalidate();
        }

        public void Add(Graphic graphic)
        {
            lock(_lock)
            {
                graphics.Add(graphic);
            }
        }

        public void Remove(Graphic graphic)
        {
            lock(_lock)
            {
                graphics.Remove(graphic);
            }
            PostInvalidate();
        }

        public void SetCameraInfo(int _previewWidth, int _previewHeight, CameraFacing _facing)
        {
            lock(_lock)
            {
                previewWidth = _previewWidth;
                previewHeight = _previewHeight;
                facing = _facing;
            }
            PostInvalidate();
        }

        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);
            lock(_lock)
            {
                if ((previewWidth != 0) && (previewHeight != 0))
                {
                    widthScaleFactor = canvas.Width / (float)previewWidth;
                    heightScaleFactor = canvas.Height / (float)previewHeight;
                }

                foreach (Graphic graphic in graphics)
                {
                    graphic.Draw(canvas);
                }
            }
        }
    }

    
}