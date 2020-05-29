using Android.Graphics;
using Android.Views;
using Android.Widget;
using AndroidX.Annotations;
using AndroidX.RecyclerView.Widget;
using Cab360Driver.Utils;
using static Android.Views.ViewTreeObserver;

namespace Cab360Driver.SliderCard
{
    public class SliderCard : RecyclerView.ViewHolder, DecodeBitmapTask.IListener, IOnGlobalLayoutListener
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
                ItemView.ViewTreeObserver.AddOnGlobalLayoutListener(this);
                LoadBitmap(resId);
            }
            else 
            {
                LoadBitmap(resId);
            }
        }

        public void OnGlobalLayout()
        {
            ItemView.ViewTreeObserver.RemoveOnGlobalLayoutListener(this);

            viewWidth = ItemView.Width;
            viewHeight = ItemView.Height;
            
        }

        public void ClearContent()
        {
            if (task != null)
            {
                task.Cancel(true);
            }
        }

        private void LoadBitmap([DrawableRes] int resId)
        {
            task = new DecodeBitmapTask(ItemView.Resources, resId, viewWidth, viewHeight, this);
            task.Execute();
        }

        public void OnPostExecuted(Bitmap bitmap)
        {
            imageView.SetImageBitmap(bitmap);
        }
    }
}
    
