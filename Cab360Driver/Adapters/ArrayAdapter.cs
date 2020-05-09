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