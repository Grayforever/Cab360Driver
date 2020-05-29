using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Java.Lang;
using System;

namespace Cab360Driver.Utils
{
    public class BackgroundBitmapCache : LruCache
    {
        public BackgroundBitmapCache(int size): base(size) { }

        private static BackgroundBitmapCache Instance;

        public static BackgroundBitmapCache GetInstance()
        {
            if (Instance == null)
            {
                var maxMemory = (int)(Runtime.GetRuntime().MaxMemory() / 1024);
                int cacheSize = maxMemory / 5;
                Instance = new BackgroundBitmapCache(cacheSize);
            }
            return Instance;
        }

        protected override int SizeOf(Java.Lang.Object key, Java.Lang.Object value)
		{
			// android.graphics.Bitmap.getByteCount() method isn't currently implemented in Xamarin. Invoke Java method.
			IntPtr classRef = JNIEnv.FindClass("android/graphics/Bitmap");
			var getBytesMethodHandle = JNIEnv.GetMethodID(classRef, "getByteCount", "()I");
			var byteCount = JNIEnv.CallIntMethod(value.Handle, getBytesMethodHandle);
           
			return byteCount / 1024;
		}

        public void AddBitmapToBgMemoryCache(int key, Bitmap bitmap)
        {
            if (GetBitmapFromBgMemCache(key) == null)
            {
                Instance.Put(key, bitmap);
            }
        }

        public Bitmap GetBitmapFromBgMemCache(int key)
        {
            var bit = Instance.Get(key);
            var bitmap = bit.JavaCast<Bitmap>();
            return bitmap;
        }
    }
}