using Android.Views;
using AndroidX.RecyclerView.Widget;
using static Android.Views.View;

namespace Cab360Driver.Adapters
{
    public class SliderAdapter : RecyclerView.Adapter, IOnClickListener
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
        {
            get { return count; }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            SliderCard.SliderCard sliderViewHolder = holder as SliderCard.SliderCard;
            sliderViewHolder.SetContent(content[position % content.Length]);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.layout_slider_card, parent, false);

            if (listener != null)
            {
                view.SetOnClickListener(this); 
            }
            SliderCard.SliderCard sliderCard = new SliderCard.SliderCard(view);
            return sliderCard;
        }

        public override void OnViewRecycled(Java.Lang.Object holder)
        {
            //base.OnViewRecycled(holder);
            var vHolder = holder as SliderCard.SliderCard;
            vHolder.ClearContent();
        }

        public void OnClick(View v)
        {
            listener.OnClick(v);
        }  
    }
    
}