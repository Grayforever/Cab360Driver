using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Cab360Driver.DataModels;
using System.Collections.Generic;
using System.Linq;

namespace Cab360Driver.Adapters
{
    public class NotifsRecyclerAdapter : RecyclerView.Adapter
    {
        public List<NotifsDataModel> _notificationsList;

        public override int ItemCount => _notificationsList.Count();

        public NotifsRecyclerAdapter(List<NotifsDataModel> notificationsList)
        {
            _notificationsList = notificationsList;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var newHolder = holder as MyViewHolder;
            newHolder.Title.Text = _notificationsList[position].Title;
            newHolder.Image.SetImageResource(_notificationsList[position].Image);
            newHolder.DateTime.Text = _notificationsList[position].DateTime.ToString();
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.notifs_itemlist, parent, false);

            var vh = new MyViewHolder(itemView);
            return vh;
        }

        public class MyViewHolder : RecyclerView.ViewHolder
        {
            public TextView Title { get; set; }
            public ImageView Image { get; set; }
            public TextView DateTime { get; set; }

            public MyViewHolder(View itemView) : base(itemView)
            {
                Title = itemView.FindViewById<TextView>(Resource.Id.notif_item_header);
                DateTime = itemView.FindViewById<TextView>(Resource.Id.notif_item_datetime);
                Image = itemView.FindViewById<ImageView>(Resource.Id.notif_item_img);
            }
        }
    }
}