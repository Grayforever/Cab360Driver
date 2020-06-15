using Android.Views;
using AndroidX.RecyclerView.Widget;
using static Android.Views.View;

namespace Cab360Driver.Adapters
{
    public class SliderAdapter : RecyclerView.Adapter
    {
        private int count;
        private int[] content;
        private IOnClickListener listener;

        public SliderAdapter(int[] content, int count, View.IOnClickListener listener)
        {
            this.content = content;
            this.count = count;
            this.listener = listener;
        }
        public override int ItemCount 
            => count;
        
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            => (holder as SliderCard.SliderCard)?.SetContent(content[position % content.Length]);

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var view = LayoutInflater.From(parent.Context)
                .Inflate(Resource.Layout.layout_slider_card, parent, false);

            if (listener != null)
            {
                view.Click+=(s1, e1)=> { listener?.OnClick(view); }; 
            }
            return new SliderCard.SliderCard(view);
        }

        public override void OnViewRecycled(Java.Lang.Object holder)
            => (holder as SliderCard.SliderCard)?.ClearContent();
    }
    
}