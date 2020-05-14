using Android.OS;
using Android.Views;

namespace Cab360Driver.Fragments
{
    public class AccountFragment : BaseFragment
    {
       
        public override BaseFragment ProvideYourfragment()
        {
            return new AccountFragment();
        }

        public override View ProvideYourFragmentView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.account, container, false);

            return view;
        }
    }
}