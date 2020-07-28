using Android.Graphics;
using AndroidX.RecyclerView.Widget;

namespace Cab360Driver.Utils
{
    public class ItemDecorator : RecyclerView.ItemDecoration
    {
        private SwipeControllerUtils _sc;
        public ItemDecorator(SwipeControllerUtils sc)
        {
            _sc = sc;
        }
        public override void OnDraw(Canvas c, RecyclerView parent, RecyclerView.State state)
        {
            _sc.OnDraw(c);
        }
    }
}