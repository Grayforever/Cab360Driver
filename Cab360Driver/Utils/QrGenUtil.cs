using Android.Graphics;
using Google.ZXing;
using Google.ZXing.Common;
using Java.Lang;

namespace Cab360Driver.Utils
{
    public static class QrGenUtil
    {
        private const int QRCodeWidth = 500;
        public static Bitmap TextToImageEncode(string value)
        {
            BitMatrix bitMatrix;
            try
            {
                bitMatrix = new MultiFormatWriter().Encode(value, BarcodeFormat.QrCode, QRCodeWidth, QRCodeWidth, null);
            }
            catch (IllegalArgumentException)
            {
                return null;
            }
            int bitMatrixWidth = bitMatrix.Width;
            int bitMatrixHeight = bitMatrix.Height;
            int[] pixels = new int[bitMatrixWidth * bitMatrixHeight];

            for (int y = 0; y < bitMatrixHeight; y++)
            {
                int offset = y * bitMatrixWidth;

                for (int x = 0; x < bitMatrixWidth; x++)
                {
                    pixels[offset + x] = bitMatrix.Get(x, y) ? Color.Black : Color.White;
                }
            }

            Bitmap bitmap = Bitmap.CreateBitmap(bitMatrixWidth, bitMatrixHeight, Bitmap.Config.Argb8888);
            bitmap.SetPixels(pixels, 0, 500, 0, 0, bitMatrixWidth, bitMatrixHeight);
            return bitmap;
        }
    }
}