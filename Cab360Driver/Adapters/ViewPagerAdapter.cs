using AndroidX.Fragment.App;
using AndroidX.Lifecycle;
using AndroidX.ViewPager2.Adapter;
using System.Collections.Generic;

namespace Cab360Driver.Adapters
{
    public class ViewPagerAdapter : FragmentStateAdapter
    {
        public List<Fragment> Fragments { get; set; }
        public List<string> FragmentNames { get; set; }


        public ViewPagerAdapter(FragmentManager fragmentManager, Lifecycle lifecycle) : base(fragmentManager, lifecycle)
        {
            Fragments = new List<Fragment>();
            FragmentNames = new List<string>();
        }

        public void AddFragment(Fragment fragment, string name)
        {
            Fragments.Add(fragment);
            FragmentNames.Add(name);
        }

        public override int ItemCount
        {
            get
            {
                return Fragments.Count;
            }
        }

        public override Fragment CreateFragment(int p0)
        {
            return Fragments[p0];
        }

    }
}