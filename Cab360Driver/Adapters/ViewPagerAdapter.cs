using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;

namespace Cab360Driver.Adapters
{
    public class ViewPagerAdapter : FragmentPagerAdapter
    {
       public List<Android.Support.V4.App.Fragment> Fragments { get; set; }
       public List<string> FragmentNames { get; set; }
        
        public ViewPagerAdapter(Android.Support.V4.App.FragmentManager fragmentManager) : base(fragmentManager)
        {
            Fragments = new List<Android.Support.V4.App.Fragment>();
            FragmentNames = new List<string>();
        }

        public void AddFragment(Android.Support.V4.App.Fragment fragment, string name)
        {
            Fragments.Add(fragment);
            FragmentNames.Add(name);
        }
        public override int Count
        {
            get
            {
                return Fragments.Count;
            }
        }

        public override Android.Support.V4.App.Fragment GetItem(int position)
        {
            return Fragments[position];
        }
    }
}