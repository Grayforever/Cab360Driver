using AndroidX.Lifecycle;
using System.Collections.Generic;

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

        public override int ItemCount => fragments.Count;
        

        public override AndroidX.Fragment.App.Fragment CreateFragment(int p0)
        {
            return fragments[p0]; 
        }
    }
}