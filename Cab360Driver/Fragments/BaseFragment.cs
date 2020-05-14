using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Google.Android.Material.BottomNavigation;

namespace Cab360Driver.Fragments
{
    public abstract class BaseFragment : AndroidX.Fragment.App.Fragment
    {
        public BaseFragment NewInstance()
        {
            Bundle args = new Bundle();
            BaseFragment baseFragment = ProvideYourfragment();
            baseFragment.Arguments = args;
            return baseFragment;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = ProvideYourFragmentView(inflater, container, savedInstanceState);
            return view;
        }

        public abstract View ProvideYourFragmentView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState);

        public abstract BaseFragment ProvideYourfragment();

    }
}