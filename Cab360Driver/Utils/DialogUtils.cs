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
using Cab360Driver.EnumsConstants;
using CN.Pedant.SweetAlert;

namespace Cab360Driver.Utils
{
    public abstract class DialogUtils
    {
        public abstract void ShowDialog(string message);
    }
}