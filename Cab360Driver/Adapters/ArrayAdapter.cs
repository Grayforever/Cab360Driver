using Android.Content;
using Android.Widget;
using System.Collections.Generic;

namespace Cab360Driver.Adapters
{
    public static class ArrayAdapterClass
    {
        public static ArrayAdapter CreateArrayAdapter(Context context, string[] objects)
        {
            return new ArrayAdapter<string>(context, Resource.Layout.support_simple_spinner_dropdown_item, objects);
        }
        public static ArrayAdapter CreateArrayAdapter(Context context, IList<string> objects)
        {
            return new ArrayAdapter<string>(context, Resource.Layout.support_simple_spinner_dropdown_item, objects);
        }
    }
}