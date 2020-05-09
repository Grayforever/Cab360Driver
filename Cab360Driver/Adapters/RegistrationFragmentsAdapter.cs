using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.Lifecycle;

namespace Cab360Driver.Adapters
{
    public class RegistrationFragmentsAdapter : AndroidX.ViewPager2.Adapter.FragmentStateAdapter
    {
        public List<AndroidX.Fragment.App.Fragment> fragments { get; set; }
        public RegistrationFragmentsAdapter(AndroidX.Fragment.App.FragmentManager fm, Lifecycle lifecycle) : base(fm, lifecycle)
        {
            fragments = new List<AndroidX.Fragment.App.Fragment>();
        }

        public void AddFragments(AndroidX.Fragment.App.Fragment fragment)
        {
            fragments.Add(fragment);
        }

        public override int ItemCount
        {
            get
            {
                return fragments.Count;
            }
        }

        public override AndroidX.Fragment.App.Fragment CreateFragment(int p0)
        {
            return fragments[p0];
        }
    }
}