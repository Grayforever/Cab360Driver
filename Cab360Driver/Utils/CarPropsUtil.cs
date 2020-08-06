using Android.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cab360Driver.Utils
{
    public static class CarPropsUtil
    {
        private static List<string> ColorList = new List<string>();
        private static List<int> Years = new List<int>();
        public static List<string> GetColorList()
        {
            ColorList.AddRange(typeof(Color).GetProperties().Where(property => property.PropertyType == typeof(Color)).Select(property => property.Name));
            return ColorList;
        }

        public static string[] GetModelYears()
        {
            for (int i = DateTime.UtcNow.Year; i >= 1992; i--)
            {
                Years.Add(i);
            }

            return Years.Select(i => i.ToString()).ToArray();
        }
    }
}