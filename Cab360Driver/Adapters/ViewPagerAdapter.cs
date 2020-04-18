using AndroidX.Fragment.App;
using System.Collections.Generic;

namespace Cab360Driver.Adapters
{
    public class ViewPagerAdapter : FragmentPagerAdapter
    {
       public List<Fragment> Fragments { get; set; }
       public List<string> FragmentNames { get; set; }
        
        public ViewPagerAdapter(FragmentManager fragmentManager) : base(fragmentManager, BehaviorResumeOnlyCurrentFragment)
        {
            Fragments = new List<Fragment>();
            FragmentNames = new List<string>();
        }

        public void AddFragment(Fragment fragment, string name)
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

        public override Fragment GetItem(int position)
        {
            return Fragments[position];
        }
    }
}