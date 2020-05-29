using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using AndroidX.Annotations;
using System;

namespace Cab360Driver.Utils
{
    public class DecodeBitmapTask : AsyncTask<Java.Lang.Object, Java.Lang.Object, Bitmap>
    {
        private BackgroundBitmapCache cache;
        private Resources resources;
        private int bitmapResId;
        private int reqWidth;
        private int reqHeight;
        private WeakReference<IListener> refListener;

        public interface IListener
        {
            void OnPostExecuted(Bitmap bitmap);
        }

        public DecodeBitmapTask(Resources resources, [DrawableRes]int bitmapResId, int reqWidth, int reqHeight, [NonNull]IListener listener)
        {
            this.cache = BackgroundBitmapCache.GetInstance();
            this.resources = resources;
            this.bitmapResId = bitmapResId;
            this.reqWidth = reqWidth;
            this.reqHeight = reqHeight;
            refListener = new WeakReference<IListener>(listener);

        }

        
        protected override Bitmap RunInBackground(params Java.Lang.Object[] @params)
        {
            Bitmap cachedBitmap = cache.GetBitmapFromBgMemCache(bitmapResId);
            if (cachedBitmap != null)
            {
                return cachedBitmap;
            }
            BitmapFactory.Options options = new BitmapFactory.Options();
            options.InJustDecodeBounds = true;
            BitmapFactory.DecodeResource(resources, bitmapResId, options);
            int width = options.OutWidth;
            int height = options.OutHeight;

            int inSampleSize = 1;
            if (height > reqHeight || width > reqWidth)
            {
                int halfWidth = width / 2;
                int halfHeight = height / 2;

                while ((halfHeight / inSampleSize) >= reqHeight && (halfWidth / inSampleSize) >= reqWidth && !IsCancelled)
                {
                    inSampleSize *= 2;
                }
            }

            if (IsCancelled)
            {
                return null;
            }

            options.InSampleSize = inSampleSize;
            options.InJustDecodeBounds = false;
            options.InPreferredConfig = Bitmap.Config.Argb8888;

            Bitmap decodedBitmap = BitmapFactory.DecodeResource(resources, bitmapResId, options);

            Bitmap result;
            if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
            {
                result = GetRoundedCornerBitmap(decodedBitmap, resources.GetDimension(Resource.Dimension.card_corner_radius), reqWidth, reqHeight);
                decodedBitmap.Recycle();
            }
            else
            {
                result = decodedBitmap;
            }

            cache.AddBitmapToBgMemoryCache(bitmapResId, result);
            return result;
        }

        protected override void OnPostExecute(Bitmap result)
        {
            //base.OnPostExecute(result);
            if(refListener.TryGetTarget(out var listener))
            {
                listener.OnPostExecuted(result);
            }
        }

        public static Bitmap GetRoundedCornerBitmap(Bitmap bitmap, float pixels, int width, int height)
        {

            Bitmap output = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888);
            Canvas canvas = new Canvas(output);

            int sourceWidth = bitmap.Width;
            int sourceHeight = bitmap.Height;

            float xScale = (float)width / bitmap.Width;
            float yScale = (float)height / bitmap.Height;
            float scale = Math.Max(xScale, yScale);

            float scaledWidth = scale * sourceWidth;
            float scaledHeight = scale * sourceHeight;

            float left = (width - scaledWidth) / 2;
            float top = (height - scaledHeight) / 2;

            Paint paint = new Paint();
            Rect rect = new Rect(0, 0, width, height);
            RectF rectF = new RectF(rect);

            RectF targetRect = new RectF(left, top, left + scaledWidth, top + scaledHeight);

            paint.AntiAlias = true;
            canvas.DrawARGB(0, 0, 0, 0);
            paint.Color = Color.Blue;
            canvas.DrawRoundRect(rectF, pixels, pixels, paint);

            paint.SetXfermode(new PorterDuffXfermode(PorterDuff.Mode.SrcIn));
            canvas.DrawBitmap(bitmap, null, targetRect, paint);

            return output;
        }
    }
}