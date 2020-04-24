using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace Cab360Driver.DataModels
{
    public class CarModel
    {
        public string Brand { get; set; }
        public string Model { get; set; }
        public string Year { get; set; }
        public string RegNo { get; set; }
        public string Color { get; set; }
        public string CurrUser { get; set; }
        public string Condition { get; set; }
    }
}