using Android.Annotation;
using Android.Content;
using Android.Graphics;
using Android.Renderscripts;
using Android.Util;
using Android.Views;
using System;
using static Android.Graphics.Bitmap;

namespace Cab360Driver.Utils
{
    public class Blur
    {
        public static Bitmap GausianBlur(Context context, Bitmap bitmap, int i)
        {
            Bitmap blurImage;
            if (i < 1)
                i = 1;
            if (i > 25)
                i = 25;

            try
            {
                blurImage = BlurImage(context, bitmap);
            }
            catch (Exception e)
            {
                Log.Debug("blur", e.Message);
                return bitmap;
            }

            return blurImage;
        }

        public static Bitmap JavaBlur(Context context, Bitmap bitmap, int i)
        {
            int i2 = i;
            Bitmap copy = bitmap.Copy(bitmap.GetConfig(), true);
            if (i2 < 1)
                return null;
            Bitmap bitmap2;
            int i3;
            int i4;
            int i5;
            int i6;
            int i7;
            int i8;
            int i9;
            int i10;
            int i11;
            int[] iArr;
            int i12;
            int width = copy.Width;
            int height = copy.Height;
            int i13 = width * height;
            int[] iArr2 = new int[i13];
            copy.GetPixels(iArr2, 0, width, 0, 0, width, height);
            int i14 = width - 1;
            int i15 = height - 1;
            int i16 = (i2 + i2) + 1;
            int[] iArr3 = new int[i13];
            int[] iArr4 = new int[i13];
            int[] iArr5 = new int[i13];
            int[] iArr6 = new int[Math.Max(width, height)];
            int i17 = (i16 + 1) >> 1;
            i17 *= i17;
            i13 = i17 * 256;
            int[] iArr7 = new int[i13];
            for(int i18 = 0; i18 < i13; i18++)
            {
                iArr7[i18] = i18 / i17;
            }

            int[][] iArr8 = (int[][])Array.CreateInstance(typeof(int), new int[] { i16, 3 });
            i17 = i2 + 1;
            i13 = 0;
            int i19 = 0;
            int i20 = 0;
            while(i13 < height)
            {
                bitmap2 = copy;
                i3 = -i2;
                i4 = 0;
                i5 = 0;
                i6 = 0;
                i7 = 0;
                i8 = 0;
                i9 = 0;
                int i21 = 0;
                int i22 = 0;
                int i23 = 0;
                while (i3 <= i2)
                {
                    i10 = i15;
                    i11 = height;
                    i15 = iArr2[i19 + Math.Min(i14, Math.Max(i3, 0))];
                    int[] iArr9 = iArr8[i3 + i2];
                    iArr9[0] = (i15 & 16711680) >> 16;
                    iArr9[1] = (i15 & (int)MotionEventActions.PointerIndexMask) >> 8;
                    iArr9[2] = i15 & 255;
                    i15 = i17 - Math.Abs(i3);
                    i4 += iArr9[0] * i15;
                    i5 += iArr9[1] * i15;
                    i6 += iArr9[2] * i15;
                    if (i3 > 0)
                    {
                        i7 += iArr9[0];
                        i8 += iArr9[1];
                        i9 += iArr9[2];
                    }
                    else
                    {
                        i21 += iArr9[0];
                        i22 += iArr9[1];
                        i23 += iArr9[2];
                    }
                    i3++;
                    height = i11;
                    i15 = i10;
                }

                i10 = i15;
                i11 = height;
                i15 = i2;
                i3 = 0;
                while (i3 < width)
                {
                    iArr3[i19] = iArr7[i4];
                    iArr4[i19] = iArr7[i5];
                    iArr5[i19] = iArr7[i6];
                    i4 -= i21;
                    i5 -= i22;
                    i6 -= i23;
                    int[] iArr10 = iArr8[((i15 - i2) + i16) % i16];
                    i21 -= iArr10[0];
                    i22 -= iArr10[1];
                    i23 -= iArr10[2];
                    if (i13 == 0)
                    {
                        iArr = iArr7;
                        iArr6[i3] = Math.Min((i3 + i2) + 1, i14);
                    }
                    else
                    {
                        iArr = iArr7;
                    }
                    i12 = iArr2[i20 + iArr6[i3]];
                    iArr10[0] = (i12 & 16711680) >> 16;
                    iArr10[1] = (i12 & (int)MotionEventActions.PointerIndexMask) >> 8;
                    iArr10[2] = i12 & 255;
                    i7 += iArr10[0];
                    i8 += iArr10[1];
                    i9 += iArr10[2];
                    i4 += i7;
                    i5 += i8;
                    i6 += i9;
                    i15 = (i15 + 1) % i16;
                    iArr10 = iArr8[i15 % i16];
                    i21 += iArr10[0];
                    i22 += iArr10[1];
                    i23 += iArr10[2];
                    i7 -= iArr10[0];
                    i8 -= iArr10[1];
                    i9 -= iArr10[2];
                    i19++;
                    i3++;
                    iArr7 = iArr;
                }
                iArr = iArr7;
                i20 += width;
                i13++;
                copy = bitmap2;
                height = i11;
                i15 = i10;
            }
            bitmap2 = copy;
            i10 = i15;
            i11 = height;
            iArr = iArr7;
            i3 = 0;
            while (i3 < width)
            {
                int[] iArr11;
                int abs;
                i14 = -i2;
                i15 = i14 * width;
                height = 0;
                i13 = 0;
                i12 = 0;
                i19 = 0;
                i20 = 0;
                i4 = 0;
                i5 = 0;
                i6 = 0;
                i7 = 0;
                while (i14 <= i2)
                {
                    iArr11 = iArr6;
                    i8 = Math.Max(0, i15) + i3;
                    int[] iArr12 = iArr8[i14 + i2];
                    iArr12[0] = iArr3[i8];
                    iArr12[1] = iArr4[i8];
                    iArr12[2] = iArr5[i8];
                    abs = i17 - Math.Abs(i14);
                    height += iArr3[i8] * abs;
                    i13 += iArr4[i8] * abs;
                    i12 += iArr5[i8] * abs;
                    if (i14 > 0)
                    {
                        i19 += iArr12[0];
                        i20 += iArr12[1];
                        i4 += iArr12[2];
                    }
                    else
                    {
                        i5 += iArr12[0];
                        i6 += iArr12[1];
                        i7 += iArr12[2];
                    }
                    abs = i10;
                    if (i14 < abs)
                    {
                        i15 += width;
                    }
                    i14++;
                    i10 = abs;
                    iArr6 = iArr11;
                }
                iArr11 = iArr6;
                abs = i10;
                i15 = i3;
                i8 = i20;
                i9 = i4;
                i14 = 0;
                i20 = i2;
                i4 = i19;
                i19 = i12;
                i12 = i13;
                i13 = height;
                height = i11;
                while (i14 < height)
                {
                    iArr2[i15] = (((iArr2[i15] & View.MeasuredStateMask) | (iArr[i13] << 16)) | (iArr[i12] << 8)) | iArr[i19];
                    i13 -= i5;
                    i12 -= i6;
                    i19 -= i7;
                    int[] iArr13 = iArr8[((i20 - i2) + i16) % i16];
                    i5 -= iArr13[0];
                    i6 -= iArr13[1];
                    i7 -= iArr13[2];
                    if (i3 == 0)
                    {
                        iArr11[i14] = Math.Min(i14 + i17, abs) * width;
                    }
                    i2 = iArr11[i14] + i3;
                    iArr13[0] = iArr3[i2];
                    iArr13[1] = iArr4[i2];
                    iArr13[2] = iArr5[i2];
                    i4 += iArr13[0];
                    i8 += iArr13[1];
                    i9 += iArr13[2];
                    i13 += i4;
                    i12 += i8;
                    i19 += i9;
                    i20 = (i20 + 1) % i16;
                    int[] iArr14 = iArr8[i20];
                    i5 += iArr14[0];
                    i6 += iArr14[1];
                    i7 += iArr14[2];
                    i4 -= iArr14[0];
                    i8 -= iArr14[1];
                    i9 -= iArr14[2];
                    i15 += width;
                    i14++;
                    i2 = i;
                }
                i3++;
                i10 = abs;
                i11 = height;
                iArr6 = iArr11;
                i2 = i;
            }
            bitmap2.SetPixels(iArr2, 0, width, 0, 0, width, i11);
            return bitmap2;
        }

        [TargetApi(Value =17)]
        private static Bitmap BlurImage(Context context, Bitmap bitmap)
        {
            RenderScript create = RenderScript.Create(context);
            Allocation createFromBitmap = Allocation.CreateFromBitmap(create, bitmap);
            ScriptIntrinsicBlur create2 = ScriptIntrinsicBlur.Create(create, createFromBitmap.Element);
            create2.SetRadius(12.0f);
            create2.SetInput(createFromBitmap);
            bitmap = Bitmap.CreateBitmap(bitmap.Width, bitmap.Height, bitmap.GetConfig());
            createFromBitmap = Allocation.CreateFromBitmap(create, bitmap);
            create2.ForEach(createFromBitmap);
            createFromBitmap.CopyTo(bitmap);
            create.Destroy();
            return bitmap;
        }

        [TargetApi(Value = 17)]
        private static Bitmap RenderScriptBlur(Context context, Bitmap bitmap, int i)
        {
            Bitmap createBitmap = CreateBitmap(bitmap.Width, bitmap.Height, Bitmap.Config.Argb8888);
            RenderScript create = RenderScript.Create(context);
            ScriptIntrinsicBlur create2 = ScriptIntrinsicBlur.Create(create, Element.U8_4(create));
            Allocation createFromBitmap = Allocation.CreateFromBitmap(create, bitmap);
            Allocation createFromBitmap2 = Allocation.CreateFromBitmap(create, createBitmap);
            create2.SetRadius(i);
            try
            {
                create2.SetInput(createFromBitmap);
                create2.ForEach(createFromBitmap2);
                createFromBitmap2.CopyTo(createBitmap);
                create.Destroy();
                return createBitmap;
            }
            catch (Exception unused)
            {
                Log.Debug("blur_exception", unused.Message);
                return null;
            }
        }
    }
}