using Android.Graphics;
using Android.Views;
using Android.Widget;
using AndroidX.Annotations;
using AndroidX.RecyclerView.Widget;
using Cab360Driver.EventListeners;
using Cab360Driver.Utils;

namespace Cab360Driver.SliderCard
{
    public class SliderCard : RecyclerView.ViewHolder, DecodeBitmapTask.IListener
    {
        private static int viewWidth = 0;
        private static int viewHeight = 0;

        private ImageView imageView;

        private DecodeBitmapTask task;
        public SliderCard(View itemView): base(itemView)
        {
            imageView = (ImageView)itemView.FindViewById(Resource.Id.image);
        }

        public void SetContent([DrawableRes]int resId)
        {
            if (viewWidth == 0)
            {
                var l = new MyVtoOnGlobalLayoutListener();
                l.GlobalLayoutEvent += (s, e) =>
                {
                    ItemView.ViewTreeObserver.RemoveOnGlobalLayoutListener(l);
                    viewWidth = ItemView.Width;
                    viewHeight = ItemView.Height;
                    LoadBitmap(resId);
                };
                ItemView.ViewTreeObserver.AddOnGlobalLayoutListener(l);
            }
            else 
            {
                LoadBitmap(resId);
            }
        }

        public void ClearContent()
            => task?.Cancel(true);

        private void LoadBitmap([DrawableRes] int resId)
        {
            task = new DecodeBitmapTask(ItemView.Resources, resId, viewWidth, viewHeight, this);
            task.Execute();
        }

        public void OnPostExecuted(Bitmap bitmap) 
            => imageView?.SetImageBitmap(bitmap);
    }
}
    
